using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#if UNITY
using UnityEngine;
#endif

namespace Sylves
{
    public class Prototile
	{
		public string Name { get; set; }

		public (Matrix4x4 transform, string childName)[] ChildPrototiles { get; set; }

		public (int fromChild, int fromChildSide, int toChild, int toChildSide)[] InteriorPrototileAdjacencies { get; set; }

		public (int parentSide, int parentSubSide, int parentSubSideCount, int child, int childSide)[] ExteriorPrototileAdjacencies { get; set; }


		public Vector3[][] ChildTiles { get; set; }

        public (int fromChild, int fromChildSide, int toChild, int toChildSide)[] InteriorTileAdjacencies { get; set; }

        public (int parentSide, int parentSubSide, int parentSubSideCount, int child, int childSide)[] ExteriorTileAdjacencies { get; set; }

		public Prototile CopyPrototileToTiles()
		{
			var r = Clone();
			r.InteriorTileAdjacencies = r.InteriorPrototileAdjacencies;
			r.ExteriorTileAdjacencies = r.ExteriorPrototileAdjacencies;
			return r;
		}

		public Prototile RenameChildren(Dictionary<string, string> renames)
		{
			var r = Clone();
			r.ChildPrototiles = ChildPrototiles
				.Select(t => (t.transform, renames[t.childName]))
				.ToArray();
			return r;
		}

        public Prototile SwapChildren(int a, int b)
        {
            int Update(int c) => c == a ? b : c == b ? a : c;
            var r = Clone();
            r.ChildPrototiles = ((Matrix4x4, string)[])r.ChildPrototiles.Clone();
            r.ChildPrototiles[a] = ChildPrototiles[b];
            r.ChildPrototiles[b] = ChildPrototiles[a];
            r.InteriorPrototileAdjacencies = InteriorPrototileAdjacencies.Select(t =>
            (Update(t.fromChild), t.fromChildSide, Update(t.toChild), t.toChildSide)).ToArray();
            r.ExteriorPrototileAdjacencies = ExteriorPrototileAdjacencies.Select(t =>
            (t.parentSide, t.parentSubSide, t.parentSubSideCount, Update(t.child), t.childSide)).ToArray();
            return r;
        }

		public Prototile Rename(string name)
		{
			var r = Clone();
			r.Name = name;
			return r;
		}

		public Prototile Clone()
		{
			return (Prototile)MemberwiseClone();
		}

    }
    /*
    internal struct Path
    {
        public uint X;
        public uint Y;
        public uint Z;
        public int TileBits;
        public int ProtoTileBits;

        public Path(Cell cell, int tileBits, int protoTileBits)
        {
            X = (uint)cell.x;
            Y = (uint)cell.y;
            Z = (uint)cell.z;
            TileBits = tileBits;
            ProtoTileBits = protoTileBits;
        }

        public int Tile => (int)(X & ((1 << TileBits) - 1));

        public int MaxHeight
        {
            get
            {
                var pathBitsAvailable = (Z != 0 ? 3 : Y != 0 ? 2 : 1) * 32 - TileBits;
                return pathBitsAvailable / ProtoTileBits;
            }
        }

        public List<int> GetPath()
        {
            var l = GetPathInternal().ToList();
            while(l.Count > 0 && l[l.Count-1] == 0)
            {
                l.RemoveAt(l.Count - 1);
            }
        }

        private IEnumerable<int> GetPathInternal()
        {
            if (ProtoTileBits == 0)
                yield break;
            int pos = 1;
            ulong current = X;
            int bits = 32;
            current = current >> TileBits;
            bits -= TileBits;
            while(true)
            {
                if(bits < ProtoTileBits)
                {
                    switch(pos)
                    {
                        case 1:
                            current = current | (Y << bits);
                            bits += 32;
                            pos += 1;
                            break;
                        case 2:
                            current = current | (Z << bits);
                            bits += 32;
                            pos += 1;
                            break;
                        case 3:
                            yield break;
                    }
                }
                yield return (int)(current & (((ulong)1 << ProtoTileBits) - 1));
            }
        }
    }*/

    public class SubstitutionTilingGrid : IGrid
	{
        private readonly Prototile[] prototiles;
        private readonly Dictionary<string, Prototile> prototilesByName;
        private int tileBits;
        private int prototileBits;
        private List<ICellType> cellTypes;
        Func<int, string> hierarchy;
        private Dictionary<string, Aabb> prototileBounds;
        private List<Crumb> crumbHierarchy = new List<Crumb>();

        public SubstitutionTilingGrid(Prototile[] prototiles, string[] hierarchy):
            this(prototiles, i => hierarchy[i % hierarchy.Length])
        {

        }

        private struct Aabb
        {
            public Vector3 min;
            public Vector3 max;

            public Aabb(IEnumerable<Vector3> v)
            {
                min = v.Aggregate(Vector3.Min);
                max = v.Aggregate(Vector3.Max);
            }

            public static Aabb operator*(Matrix4x4 m, Aabb aabb)
            {
                var c = (aabb.min + aabb.max) / 2;
                var h = (aabb.max - aabb.min) / 2;
                c = m.MultiplyPoint3x4(c);
                h = m.MultiplyVector(h);
                h.x = Mathf.Abs(h.x);
                h.y = Mathf.Abs(h.y);
                return new Aabb
                {
                    min = c - h,
                    max = c + h,
                };
            }

            public static Aabb Union(IEnumerable<Aabb> aabbs)
            {
                var i = aabbs.GetEnumerator();
                i.MoveNext();
                var first = i.Current;
                while(i.MoveNext())
                {
                    var current = i.Current;
                    first.min = Vector3.Min(first.min, current.min);
                    first.max = Vector3.Max(first.max, current.max);
                }
                return first;
            }

            public bool Intersects(Aabb other)
            {
                if (this.max.x < other.min.x ||
                    this.min.x > other.max.x ||
                    this.max.y < other.min.y ||
                    this.min.y > other.max.y ||
                    this.max.z < other.min.z ||
                    this.min.z > other.max.z)
                {
                    return false;
                }
                return true;
            }
        }

        public SubstitutionTilingGrid(Prototile[] prototiles, Func<int, string> hierarchy)
		{
            this.prototiles = prototiles;
            prototilesByName = prototiles.ToDictionary(x => x.Name);
            var maxTileCount = prototiles.Max(x => x.ChildTiles?.Length ?? 0);
            tileBits = (int)Math.Ceiling(Math.Log(maxTileCount) / Math.Log(2));
            var maxPrototileCount = prototiles.Max(x => x.ChildPrototiles?.Length ?? 0);
            prototileBits = (int)Math.Ceiling(Math.Log(maxPrototileCount) / Math.Log(2));
            this.hierarchy = hierarchy;

            // Pre-compute cell types
            cellTypes = prototiles.SelectMany(x => x.ChildTiles.Select(y => y.Length)).Distinct().Select(NGonCellType.Get).ToList();

            // Precompute bounds
            // Bounds can extend beyond the tile bound if a child prototile is transformed in certain ways.
            // And children's children could protude even further, ad-infinitum.
            // But it must be a geometric growth to a limit, so we measure the worst case growth,
            // then jump straight to the bound.
            // TODO: I'm not entirely certain this procedur is valid
            var tileBounds = prototiles.ToDictionary(x => x.Name, x =>
            {
                return new Aabb(x.ChildTiles.SelectMany(t => t));
            });
            var deflation = prototiles.SelectMany(x => x.ChildPrototiles).Select(x => x.transform.lossyScale).Select(v => Mathf.Max(Mathf.Abs(v.x), Mathf.Max(Mathf.Abs(v.y), Mathf.Abs(v.z))));
            var prevBounds = tileBounds;
            // Do enough iterations to encounter every possible combination.
            for(var i=0;i<prototiles.Count();i++)
            {
                prevBounds = prototiles.ToDictionary(x => x.Name, x =>
                {
                    return Aabb.Union(x.ChildPrototiles.Select(c => c.transform * prevBounds[c.childName]));
                });
            }
            // Deflation is also affected by iterations
            var deflationPow = Math.Pow(default, prototiles.Count());
            var alpha = (float)(1 / (1 - deflationPow));
            prototileBounds = prototiles.ToDictionary(x => x.Name, x =>
            {
                var tileBound = tileBounds[x.Name];
                var prevBound = prevBounds[x.Name];
                var max = alpha * (prevBound.max - tileBound.max) + tileBound.max;
                var min = alpha * (prevBound.min - tileBound.min) + tileBound.min;
                return new Aabb { min = min, max = max };
            });
        }

        #region Utils

        internal int GetPathAt(Cell cell, int height)
        {
            var bits = tileBits + prototileBits * height;
            var mask1 = (1U << prototileBits) - 1;
            if (bits >= 32)
            {
                var l = (uint)cell.y |  (((ulong)(uint)cell.z) << 32);
                l = l >> (bits - 32);
                return (int)(l & mask1);
            }
            else
            {

                var l = (uint)cell.x | (((ulong)(uint)cell.y) << 32);
                l = l >> bits;
                return (int)(l & mask1);
            }
        }

        internal Cell SetPathAt(Cell cell, int height, int value)
        {
            var bits = tileBits + prototileBits * height;
            var mask1 = (1U << prototileBits) - 1;
            if (bits >= 32)
            {
                cell.y = cell.x & ~(int)(mask1 << (bits - 32)) | (int)(value << (bits - 32));
                cell.z = cell.z & ~(int)(mask1 << (bits - 64)) | (int)(value << (bits - 64));
            }
            else
            {
                cell.x = cell.x & ~(int)(mask1 << bits) | (int)(value << bits);
                cell.y = cell.y & ~(int)(mask1 << (bits - 32)) | (int)(value << (bits - 32));
            }
            return cell;
        }

        internal int GetChildTileAt(Cell cell)
        {
            return cell.x & ((1 << tileBits) - 1);
        }

        internal Cell SetChildTileAt(Cell cell, int value)
        {
            cell.x = cell.x & ~((1 << tileBits) - 1) | (value << tileBits);
            return cell;
        }

        // Convert a cell into a prototile path and a specific child
        internal (int childTile, List<int> path) Parse(Cell cell)
        {
            ulong current = (uint)cell.x;
            var childTile = (int)(current & ((1UL << tileBits) - 1));



            if (prototileBits == 0)
                return (childTile, new List<int>());

            var path = new List<int>();
            int pos = 1;
            int bits = 32;
            current = current >> tileBits;
            bits -= tileBits;
            while (true)
            {
                if (bits < prototileBits)
                {
                    if (pos == 3)
                        break;
                    switch (pos)
                    {
                        case 1:
                            current = current | ((uint)cell.y << bits);
                            bits += 32;
                            pos += 1;
                            break;
                        case 2:
                            current = current | ((uint)cell.z << bits);
                            bits += 32;
                            pos += 1;
                            break;
                    }
                }
                path.Add((int)(current & (((ulong)1 << prototileBits) - 1)));
                current = current >> prototileBits;
                bits -= prototileBits;
            }
            while (path.Count > 0 && path[path.Count - 1] == 0)
                path.RemoveAt(path.Count - 1);
            return (childTile, path);
        }

        // Inverse of Parse
        internal Cell Format(int childTile, List<int> path)
        {
            var cell = new Cell();
            ulong current = 0;
            var bits = 0;
            var pos = 1;

            current = (ulong)childTile;
            bits += tileBits;

            foreach(var p in path)
            {
                current = current | ((ulong)p << bits);
                bits += prototileBits;
                if(bits >= 32)
                {
                    switch(pos)
                    {
                        case 1:
                            cell.x = (int)(current & 0xFFFFFFFF);
                            current = current >> 32;
                            bits -= 32;
                            pos++;
                            break;
                        case 2:
                            cell.y = (int)(current & 0xFFFFFFFF);
                            current = current >> 32;
                            bits -= 32;
                            pos++;
                            break;
                        default:
                            throw new Exception();
                    }
                }
            }

            switch (pos)
            {
                case 1:
                    cell.x = (int)(current & 0xFFFFFFFF);
                    break;
                case 2:
                    cell.y = (int)(current & 0xFFFFFFFF);
                    break;
                default:
                    cell.z = (int)(current & 0xFFFFFFFF);
                    break;
            }

            return cell;

        }

        // Utility for working with prototile transforms
        private Matrix4x4 Up(Matrix4x4 transform, string parentName)
        {
            return Up(transform, prototilesByName[parentName]);
        }
        private Matrix4x4 Up(Matrix4x4 transform, Prototile parentPrototile)
        {
            return transform * parentPrototile.ChildPrototiles[0].transform.inverse;
        }

        private (Matrix4x4, string) Down(Matrix4x4 transform, string prototileName, int child)
        {
            return Down(transform, prototilesByName[prototileName], child);
        }

        private (Matrix4x4, string) Down(Matrix4x4 transform, Prototile prototile, int child)
        {
            var t = prototile.ChildPrototiles[child];
            return (transform * t.transform, t.childName);
        }

        #endregion

        #region Crumb utils

        public class Crumb
        {
            // Null means not yet evaluated, use GetParent to force.
            public Crumb Parent;
            public int Height;
            // The child of this prototile in the parent
            public int ChildIndex;
            public Matrix4x4 Transform;
            public Prototile Prototile;
        }

        Crumb GetParent(Crumb c)
        {
            var parent = c.Parent;
            if (parent != null)
                return parent;

            var parentName = hierarchy(c.Height + 1);
            var parentPrototile = prototilesByName[parentName];

            parent = new Crumb
            {
                Parent = null,
                Height = c.Height + 1,
                ChildIndex = 0,
                Transform = Up(c.Transform, parentPrototile),
                Prototile = parentPrototile,
            };

            return c.Parent = parent;
        }

        Crumb CreateChild(Crumb c, int childIndex)
        {
            var (transform, childName) = Down(c.Transform, c.Prototile, childIndex);
            return new Crumb
            {
                Parent = c,
                ChildIndex = childIndex,
                Height = c.Height - 1,
                Transform = transform,
                Prototile = prototilesByName[childName],
            };
        }

        /*
        Crumb CrumbHierarchy(int height)
        {
            if (crumbHierarchy.Count == 0)
            {
                crumbHierarchy[0] = new Crumb
                {
                    Parent = null,
                    ChildIndex = 0,
                    Height = 0,
                    Transform = Matrix4x4.identity,
                    Prototile = prototilesByName[hierarchy(0)],
                };
            }
            for (var i = crumbHierarchy.Count; i < height; i++)
            {
                crumbHierarchy[i] = GetParent(crumbHierarchy)
            }
        }
        */

        /*
        (int childTile, Crumb) ToCrumb(Cell cell)
        {

            ulong current = (uint)cell.x;
            var childTile = (int)(current & ((1UL << tileBits) - 1));



            if (prototileBits == 0)
                return (childTile, ZeroCrumb());

            var crumb = ZeroCrumb();
            int pos = 1;
            int bits = 32;
            current = current >> tileBits;
            bits -= tileBits;
            while (true)
            {
                if (bits < prototileBits)
                {
                    if (pos == 3)
                        break;
                    switch (pos)
                    {
                        case 1:
                            current = current | ((uint)cell.y << bits);
                            bits += 32;
                            pos += 1;
                            break;
                        case 2:
                            current = current | ((uint)cell.z << bits);
                            bits += 32;
                            pos += 1;
                            break;
                    }
                }
                crumb
                path.Add((int)(current & (((ulong)1 << prototileBits) - 1)));
                current = current >> prototileBits;
                bits -= prototileBits;
            }
            while (path.Count > 0 && path[path.Count - 1] == 0)
                path.RemoveAt(path.Count - 1);
            return (childTile, path);
        }

        Cell FromCrumb(int childTile, Crumb crumb)
        {

        }
        */

        #endregion

        private (string prototile, Matrix4x4 prototileTransform, int childTile) LocateCell(Cell cell)
        {
            var (childTile, path) = Parse(cell);
            var transform = Matrix4x4.identity;
            string parentName = hierarchy(0);
            for (var i = 0; i < path.Count; i++)
            {
                parentName = hierarchy(i + 1);
                transform = Up(transform, parentName);
            }
            for (var i = path.Count - 1; i >= 0; i--)
            {
                (transform, parentName) = Down(transform, parentName, path[i]);
            }
            return (parentName, transform, childTile);
        }

        /// <summary>
        /// Returns n+1 parent elements for a path of length n
        /// </summary>
        private IList<Prototile> Parents(List<int> path)
        {
            var parents = new List<Prototile>();
            string parent = hierarchy(0);
            for (var i = 0; i < path.Count; i++)
            {
                parent = hierarchy(i + 1);
            }
            parents.Add(prototilesByName[parent]);
            for (var i = path.Count - 1; i >= 0; i--)
            {
                var t = prototilesByName[parent].ChildPrototiles[path[i]];
                parent = t.childName;
                parents.Insert(0, prototilesByName[parent]);
            }
            return parents;
        }


        #region Basics
        public bool Is2d => true;

        public bool Is3d => false;

        public bool IsPlanar => true;

        public bool IsRepeating => false;

        public bool IsOrientable => true;

        public bool IsFinite => false;

        public bool IsSingleCellType => cellTypes.Count == 1;

        public int CoordinateDimension => 3;

        public IEnumerable<ICellType> GetCellTypes() => cellTypes;
        #endregion

        #region Relatives
        public IGrid Unbounded => throw new NotImplementedException();

        public IGrid Unwrapped => throw new NotImplementedException();

        public virtual IDualMapping GetDual() => throw new NotImplementedException();

        #endregion

        #region Cell info

        public IEnumerable<Cell> GetCells() => throw new GridInfiniteException();

        public ICellType GetCellType(Cell cell)
        {
            if (cellTypes.Count == 1)
                return cellTypes[0];
            throw new NotImplementedException();
        }

        public bool IsCellInGrid(Cell cell)
        {
            // TODO: Don't use try-catch
            try
            {
                LocateCell(cell);
            }
            catch
            {
                return false;
            }
            return true;
        }

        #endregion

        #region Topology

        public bool TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection)
        {
            var (childTile, path) = Parse(cell);
            int GetPathItem(int height) => height < path.Count ? path[height] : 0;
            var parents = Parents(path);
            Prototile GetParent(int height) => height < parents.Count ? parents[height] : prototilesByName[hierarchy(height)];
            var prototile = GetParent(0);
            var tileInterior = prototile.InteriorTileAdjacencies
                .Where(x => x.fromChild == childTile && x.fromChildSide == (int)dir)
                .ToList();
            if (tileInterior.Count == 1)
            {
                dest = Format(tileInterior[0].toChild, path);
                inverseDir = (CellDir)tileInterior[0].toChildSide;
                connection = new Connection();
                return true;
            }
            else
            {
                var tileExterior = prototile.ExteriorTileAdjacencies
                    .Where(x => x.child == childTile && x.childSide == (int)dir)
                    .Single();


                (List<int> partialPath, int side, Prototile otherParent) TryMovePrototile(int height, int childPrototile, int prototileSide)
                {
                    var parent = GetParent(height + 1);
                    var interior = parent.InteriorPrototileAdjacencies
                        .Where(x => x.fromChild == childPrototile && x.fromChildSide == prototileSide)
                        .ToList();
                    if (interior.Count == 1)
                    {
                        var partialPath = path.Take(height + 1).ToList();
                        var toChild = interior[0].toChild;
                        partialPath.Insert(0, toChild);
                        return (partialPath, interior[0].toChildSide, prototilesByName[parent.ChildPrototiles[toChild].childName]);
                    }
                    else
                    {
                        var exterior = parent.ExteriorPrototileAdjacencies
                            .Where(x => x.child == childPrototile && x.childSide == prototileSide)
                            .Single();

                        var (partialPath, otherSide, otherParent) = TryMovePrototile(height + 1, GetPathItem(height + 1), exterior.parentSide);

                        var otherSubside = exterior.parentSubSideCount - 1 - exterior.parentSubSide;
                        var otherExterior = otherParent.ExteriorPrototileAdjacencies
                            .Where(x => x.parentSide == otherSide && x.parentSubSide == otherSubside)
                            .Single();
                        if (otherExterior.parentSubSideCount != exterior.parentSubSideCount)
                            throw new Exception();
                        partialPath.Insert(0, otherExterior.child);
                        return (partialPath, otherExterior.childSide, prototilesByName[otherParent.ChildPrototiles[otherExterior.child].childName]);
                    }
                }

                {
                    var (partialPath, otherSide, otherParent) = TryMovePrototile(0, GetPathItem(0), tileExterior.parentSide);


                    var otherSubside = tileExterior.parentSubSideCount - 1 - tileExterior.parentSubSide;
                    var otherExterior = otherParent.ExteriorTileAdjacencies
                        .Where(x => x.parentSide == otherSide && x.parentSubSide == otherSubside)
                        .Single();
                    if (otherExterior.parentSubSideCount != tileExterior.parentSubSideCount)
                        throw new Exception();
                    //partialPath.Insert(0, otherExterior.child);
                    //return (partialPath, otherExterior.childSide, prototilesByName[otherParent.ChildPrototiles[otherExterior.child].childName]);

                    dest = Format(otherExterior.child, partialPath);
                    inverseDir = (CellDir)otherExterior.childSide;
                    connection = default;
                    return true;
                }
            }
        }

        public bool TryMoveByOffset(Cell startCell, Vector3Int startOffset, Vector3Int destOffset, CellRotation startRotation, out Cell destCell, out CellRotation destRotation) => throw new NotImplementedException();


        public bool ParallelTransport(IGrid aGrid, Cell aSrcCell, Cell aDestCell, Cell srcCell, CellRotation startRotation, out Cell destCell, out CellRotation destRotation) => throw new NotImplementedException();


        public IEnumerable<CellDir> GetCellDirs(Cell cell) => throw new NotImplementedException();

        public IEnumerable<CellCorner> GetCellCorners(Cell cell) => throw new NotImplementedException();

        public IEnumerable<(Cell, CellDir)> FindBasicPath(Cell startCell, Cell destCell) => throw new NotImplementedException();

        #endregion

        #region Index
        public int IndexCount => throw new NotImplementedException();

        public int GetIndex(Cell cell) => throw new NotImplementedException();

        public Cell GetCellByIndex(int index) => throw new NotImplementedException();
        #endregion

        #region Bounds
        public IBound GetBound() => throw new NotImplementedException();

        public IBound GetBound(IEnumerable<Cell> cells) => throw new NotImplementedException();

        public IGrid BoundBy(IBound bound) => throw new NotImplementedException();

        public IBound IntersectBounds(IBound bound, IBound other) => throw new NotImplementedException();

        public IBound UnionBounds(IBound bound, IBound other) => throw new NotImplementedException();

        public IEnumerable<Cell> GetCellsInBounds(IBound bound) => throw new NotImplementedException();

        public bool IsCellInBound(Cell cell, IBound bound) => throw new NotImplementedException();
        #endregion

        #region Position
        public Vector3 GetCellCenter(Cell cell)
        {
            // TODO: More efficient
            GetPolygon(cell, out var vertices, out var transform);
            return vertices.Select(transform.MultiplyPoint3x4).Aggregate((x, y) => x + y) / vertices.Length;
        }

        public Vector3 GetCellCorner(Cell cell, CellCorner cellCorner) => throw new NotImplementedException();

        public TRS GetTRS(Cell cell) => throw new NotImplementedException();

        #endregion

        #region Shape
        public Deformation GetDeformation(Cell cell) => throw new NotImplementedException();

        public void GetPolygon(Cell cell, out Vector3[] vertices, out Matrix4x4 transform)
        {
            var (prototile, prototileTransform, childTile) = LocateCell(cell);
            vertices = prototilesByName[prototile].ChildTiles[childTile];
            transform = prototileTransform;
        }

        public IEnumerable<(Vector3, Vector3, Vector3, CellDir)> GetTriangleMesh(Cell cell) => throw new NotImplementedException();

        public void GetMeshData(Cell cell, out MeshData meshData, out Matrix4x4 transform) => throw new NotImplementedException();

        #endregion

        #region Query
        public bool FindCell(Vector3 position, out Cell cell)
        {
            var inputAabb = new Aabb { min = position, max = position };
            foreach (var (prototile, transform, partialPath) in GetPrototilesIntersectsApproxInternal(inputAabb))
            {
                for (var childIndex = 0; childIndex < prototile.ChildTiles.Length; childIndex++)
                {
                    // Currently does fan detection
                    // Doesn't work for convex faces
                    var transformedPosition = transform.inverse.MultiplyPoint3x4(position);
                    var vertices = prototile.ChildTiles[childIndex];
                    var v0 = vertices[0];
                    var prev = vertices[1];
                    for (var i = 2; i < vertices.Length; i++)
                    {
                        var v = vertices[i];
                        if (MeshGrid.IsPointInTriangle(transformedPosition, v0, prev, v))
                        {
                            cell =  SetChildTileAt(partialPath, childIndex);
                            return true;
                        }
                        prev = v;
                    }
                    continue;
                }
            }
            cell = default;
            return false;
        }

        public bool FindCell(
            Matrix4x4 matrix,
            out Cell cell,
            out CellRotation rotation) => throw new NotImplementedException();

        public IEnumerable<Cell> GetCellsIntersectsApprox(Vector3 min, Vector3 max)
        {
            var inputAabb = new Aabb { min = min, max = max };
            foreach (var (prototile, transform, partialPath) in GetPrototilesIntersectsApproxInternal(inputAabb))
            {
                for (var i = 0; i < prototile.ChildTiles.Length; i++)
                {
                    yield return SetChildTileAt(partialPath, i);
                }
            }
        }

        // Returns the set of height 0 prototiles intersect inputAabb
        private IEnumerable<(Prototile prototile, Matrix4x4 transform, Cell partialPath)> GetPrototilesIntersectsApproxInternal(Aabb inputAabb)
        {
            var stack = new Stack<(int height, Prototile prototile, Matrix4x4 transform, Cell partialPath)>();
            foreach (var t in GetPrototilesIntersectsApproxInternalPartition(inputAabb))
            {
                // For each of these prototiles, walk the entire tree under it
                stack.Push(t);
                while(stack.Count > 0)
                {
                    var (height, prototile, transform, partialPath) = stack.Pop();
                    var bound = transform * prototileBounds[prototile.Name];
                    if(bound.Intersects(inputAabb))
                    {
                        if(height == 0)
                        {
                            yield return (prototile, transform, partialPath);
                        }
                        else
                        {
                            // Recurse
                            for(var i=0;i<prototile.ChildPrototiles.Length;i++)
                            {
                                var (childTransform, childName) = Down(transform, prototile, i);
                                var child = prototilesByName[childName];
                                stack.Push((height - 1, child, childTransform, SetPathAt(partialPath, height - 1, i)));
                            }
                        }
                    }
                }
            }
        }

        // Returns a set of non-overlapping prototiles at different hieghts that collectively contain all prototiles that intersect inputAabb
        private IEnumerable<(int height, Prototile prototile, Matrix4x4 transform, Cell partialPath)> GetPrototilesIntersectsApproxInternalPartition(Aabb inputAabb)
        {
            var height = 0;
            var transform = Matrix4x4.identity;
            var path = new Cell();
            var found = false;
            while (true)
            {
                var parentName = hierarchy(height + 1);
                transform = Up(transform, parentName);
                var parentPrototile = prototilesByName[parentName];
                var foundAtThisHeight = false;

                // Skips 0, we just came from there!
                for (var i = height == 0 ? 0 : 1; i < parentPrototile.ChildPrototiles.Length; i++)
                {
                    var (childTransform, childName) = Down(transform, parentPrototile, i);
                    var childBound = childTransform * prototileBounds[childName];
                    if(childBound.Intersects(inputAabb))
                    {
                        yield return (height, prototilesByName[childName], childTransform, SetPathAt(path, height, i));
                        foundAtThisHeight = true;
                        found = true;
                    }
                }

                height = height + 1;

                if (found && !foundAtThisHeight)
                    break;
            }
        }


        public IEnumerable<RaycastInfo> Raycast(Vector3 origin, Vector3 direction, float maxDistance = float.PositiveInfinity) => throw new NotImplementedException();
        #endregion

        #region Symmetry
        public GridSymmetry FindGridSymmetry(ISet<Cell> src, ISet<Cell> dest, Cell srcCell, CellRotation cellRotation) => throw new NotImplementedException();

        public bool TryApplySymmetry(GridSymmetry s, IBound srcBound, out IBound destBound) => throw new NotImplementedException();

        public bool TryApplySymmetry(GridSymmetry s, Cell src, out Cell dest, out CellRotation r) => throw new NotImplementedException();
        #endregion
    }
}

