using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sylves
{
    /// <summary>
    /// Base class for <see cref="SubstitutionTilingGrid"/>
    /// This contains common utilities and calculations that do no rely on caching.
    /// </summary>
    public abstract class BaseSubstitutionTilingGrid : IGrid
	{
        // Raycast and IntersectsAabb have a hard time knowing when to stop searching.
        // They give up when they haven't found anything interesting in this many heights.
        protected const int DeadZone = 3;

        protected readonly InternalPrototile[] prototiles;
        protected int tileBits;
        protected int prototileBits;
        protected List<ICellType> cellTypes;
        protected Func<int, InternalPrototile> hierarchy;
        protected SubstitutionTilingBound bound;
        protected Matrix4x4 baseTransform;
        // By height
        private List<Dictionary<InternalPrototile, int>> indexCounts;

        // Copy constructor
        protected BaseSubstitutionTilingGrid(BaseSubstitutionTilingGrid other, SubstitutionTilingBound bound)
        {
            prototiles = other.prototiles;
            tileBits = other.tileBits;
            prototileBits = other.prototileBits;
            cellTypes = other.cellTypes;
            hierarchy = other.hierarchy;
            this.bound = bound;
            baseTransform = other.baseTransform;
            ValidateBound();
        }

        // Copy constructor
        protected BaseSubstitutionTilingGrid(BaseSubstitutionTilingGrid other, Func<int, InternalPrototile> hierarchy, Matrix4x4 baseTransform)
        {
            prototiles = other.prototiles;
            tileBits = other.tileBits;
            prototileBits = other.prototileBits;
            cellTypes = other.cellTypes;
            this.hierarchy = hierarchy;
            bound = other.bound;
            this.baseTransform = baseTransform;
        }


        public BaseSubstitutionTilingGrid(Prototile[] prototiles, string[] hierarchy, SubstitutionTilingBound bound = null):
            this(prototiles, i => hierarchy[i % hierarchy.Length], bound)
        {

        }

        public BaseSubstitutionTilingGrid(Prototile[] prototiles, Func<int, string> hierarchy, SubstitutionTilingBound bound = null)
		{
            // Prep bit arithmetic
            var maxTileCount = prototiles.Max(x => x.ChildTiles?.Length ?? 0);
            tileBits = (int)Math.Ceiling(Math.Log(maxTileCount) / Math.Log(2));
            var maxPrototileCount = prototiles.Max(x => x.ChildPrototiles?.Length ?? 0);
            prototileBits = (int)Math.Ceiling(Math.Log(maxPrototileCount) / Math.Log(2));

            var (internalPrototiles, prototilesByName) = BuildPrototiles(prototiles);
            this.prototiles = internalPrototiles;
            this.hierarchy = h => prototilesByName[hierarchy(h)];

            // Pre-compute cell types
            cellTypes = prototiles.SelectMany(x => x.ChildTiles.Select(y => y.Length)).Distinct().Select(NGonCellType.Get).ToList();

            this.bound = bound;
            ValidateBound();
            baseTransform = Matrix4x4.identity;

            BuildPrototileBounds();
        }

        #region Construction
        private void ValidateBound()
        {
            if (bound == null) return;
            if (GetChildTileAt(bound.Path) != 0)
            {
                throw new Exception($"Cell {bound.Path} is not valid for a path of height {bound.Height}. Its bitstring should start with {bound.Height * prototileBits + tileBits} zeros");

            }
            for (var i = 0; i < bound.Height; i++)
            {
                if (GetPathAt(bound.Path, i) != 0)
                    throw new Exception($"Cell {bound.Path} is not valid for a path of height {bound.Height}. Its bitstring should start with {bound.Height * prototileBits + tileBits} zeros");
            }
        }

        private (InternalPrototile[], Dictionary<string, InternalPrototile>) BuildPrototiles(Prototile[] prototiles)
        {
            // Build internal prototiles
            var internalPrototiles = prototiles.Select(x => new InternalPrototile
            {
                ChildTiles = x.ChildTiles,
                ExteriorPrototileAdjacencies = x.ExteriorPrototileAdjacencies,
                InteriorPrototileAdjacencies = x.InteriorPrototileAdjacencies,
                PassthroughPrototileAdjacencies = x.PassthroughPrototileAdjacencies,
                InteriorTileAdjacencies = x.InteriorTileAdjacencies,
                Name = x.Name,
                ExteriorTileAdjacencies = x.ExteriorTileAdjacencies,
            }).ToArray();
            var prototilesByName = internalPrototiles.ToDictionary(x => x.Name);
            for (var i = 0; i < prototiles.Length; i++)
            {
                internalPrototiles[i].ChildPrototiles = prototiles[i].ChildPrototiles
                    .Select(t => (t.transform, prototilesByName[t.childName]))
                    .ToArray();
            }

            // Precompute centers
            foreach(var prototile in internalPrototiles)
            {
                prototile.Centers = prototile.ChildTiles.Select(vs => vs.Aggregate((x, y) => x + y) / vs.Length).ToArray();
            }

            return (internalPrototiles, prototilesByName);
        }

        private void BuildPrototileBounds()
        {
            // Precompute bounds
            // Bounds can extend beyond the tile bound if a child prototile is transformed in certain ways.
            // And children's children could protude even further, ad-infinitum.
            // But it must be a geometric growth to a limit, so we measure the worst case growth,
            // then jump straight to the bound.
            // TODO: I'm not entirely certain this procedur is valid
            var tileBounds = this.prototiles.ToDictionary(x => x, x =>
            {
                return new Aabb(x.ChildTiles.SelectMany(t => t));
            });
            var deflation = this.prototiles.SelectMany(x => x.ChildPrototiles).Select(x => x.transform.lossyScale).Select(v => Mathf.Max(Mathf.Abs(v.x), Mathf.Max(Mathf.Abs(v.y), Mathf.Abs(v.z))));
            var prevBounds = tileBounds;
            // Do enough iterations to encounter every possible combination.
            for (var i = 0; i < this.prototiles.Count(); i++)
            {
                prevBounds = this.prototiles.ToDictionary(x => x, x =>
                {
                    return Aabb.Union(x.ChildPrototiles.Select(c => c.transform * prevBounds[c.child]));
                });
            }

            // Deflation is also affected by iterations
            var deflationPow = Math.Pow(default, prototiles.Count());
            var alpha = (float)(1 / (1 - deflationPow));
            foreach (var prototile in this.prototiles)
            {
                var tileBound = tileBounds[prototile];
                var prevBound = prevBounds[prototile];
                var max = alpha * (prevBound.max - tileBound.max) + tileBound.max;
                var min = alpha * (prevBound.min - tileBound.min) + tileBound.min;
                prototile.bound = new Aabb { min = min, max = max };
            };
        }
        #endregion

        #region Path Utils
        // Various functions for interpreting a Cell as a set of 12 bytes
        // that encodes a series of small integers, a "path".


        // Returns the largest value that GetPathAt(cell, i - 1) is non-zero
        internal int GetPathLength(Cell cell)
        {
            int leadingBits;
            if (cell.z != 0)
            {
                leadingBits = BitUtils.LeadingZeroCount((uint)cell.z);
            }
            else if (cell.y != 0)
            {
                leadingBits = 32 + BitUtils.LeadingZeroCount((uint)cell.y);
            }
            else
            {
                leadingBits = 64 + BitUtils.LeadingZeroCount((uint)cell.x);
            }
            var bits = 96 - leadingBits;
            bits -= tileBits;
            return Math.Max(0, (bits + prototileBits - 1) / prototileBits);
        }

        internal int GetPathDiffLength(Cell a, Cell b)
        {
            int leadingBits;
            if (a.z != b.z)
            {
                leadingBits = BitUtils.LeadingZeroCount((uint)(a.z ^ b.z));
            }
            else if (a.y != b.y)
            {
                leadingBits = 32 + BitUtils.LeadingZeroCount((uint)(a.y ^ b.y));
            }
            else
            {
                leadingBits = 64 + BitUtils.LeadingZeroCount((uint)(a.x ^ b.x));
            }
            var bits = 96 - leadingBits;
            bits -= tileBits;
            return Math.Max(0, (bits + prototileBits - 1) / prototileBits);
        }

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
            if(bits >= 96)
            {
                throw new Exception($"Cannot set bits at height {height}");
            }
            if (bits >= 64)
            {
                cell.z = cell.z & ~(int)(mask1 << (bits - 64)) | (int)(value << (bits - 64));

            }
            else if (bits > 32)
            {
                cell.y = cell.y & ~(int)(mask1 << (bits - 32)) | (int)(value << (bits - 32));
                cell.z = cell.z & ~(int)(mask1 << (bits - 64)) | (int)(value >>- (bits - 64));
            }
            else if(bits == 32)
            {
                cell.y = cell.y & ~(int)(mask1 << (bits - 32)) | (int)(value << (bits - 32));
            }
            else if(bits > 0)
            {
                cell.x = cell.x & ~(int)(mask1 << bits) | (int)(value << bits);
                cell.y = cell.y & ~(int)(mask1 << (bits - 32)) | (int)(value >>- (bits - 32));
            }
            else
            {
                cell.x = cell.x & ~(int)(mask1 << bits) | (int)(value << bits);
            }
            return cell;
        }

        internal int GetChildTileAt(Cell cell)
        {
            return cell.x & ((1 << tileBits) - 1);
        }

        internal Cell SetChildTileAt(Cell cell, int value)
        {
            cell.x = cell.x & ~((1 << tileBits) - 1) | value;
            return cell;
        }

        internal Cell ClearChildTileAndPathBelow(Cell cell, int height)
        {
            var bits = tileBits + prototileBits * height;
            if(bits >= 96)
            {
                return new Cell();
            }
            else if (bits >= 64)
            {
                var mask = (1 << (bits - 64)) - 1;
                return new Cell(0, 0, (int)((uint)cell.z & ~mask));
            }
            else if (bits >= 32)
            {
                var mask = (1 << (bits - 32)) - 1;
                return new Cell(0, (int)((uint)cell.y & ~mask), cell.z);
            }
            else
            {
                var mask = (1 << (bits - 0)) - 1;
                return new Cell((int)((uint)cell.x & ~mask), cell.y, cell.z);
            }
        }

        #endregion

        #region Other Utils


        // Utility for working with prototile transforms
        protected Matrix4x4 Up(Matrix4x4 transform, InternalPrototile parent)
        {
            return transform * parent.ChildPrototiles[0].transform.inverse;
        }

        protected (Matrix4x4, InternalPrototile) Down(Matrix4x4 transform, InternalPrototile prototile, int child)
        {
            var t = prototile.ChildPrototiles[child];
            return (transform * t.transform, t.child);
        }

        protected abstract InternalPrototile GetPrototile(Cell cell, int height);

        #endregion


        #region Basics
        public bool Is2d => true;

        public bool Is3d => false;

        public bool IsPlanar => true;

        public bool IsRepeating => false;

        public bool IsOrientable => true;

        public bool IsFinite => false;

        public bool IsSingleCellType => cellTypes.Count == 1;

        public int CoordinateDimension
        {
            get
            {
                if (bound == null)
                    return 3;
                if (bound.Path.y == 0 && bound.Path.z == 0)
                    return 1;
                if (bound.Path.z == 0)
                    return 2;
                return 3;
            }
        }

        public IEnumerable<ICellType> GetCellTypes() => cellTypes;
        #endregion

        #region Relatives
        public abstract IGrid Unbounded { get; }// => new BaseSubstitutionTilingGrid(this, (SubstitutionTilingBound) null);


        public IGrid Unwrapped => this;

        public virtual IDualMapping GetDual()
        {
            // Guess at reasonable size for chunking
            var maxPrototileSize = prototiles.Max(x => (x.bound.max.x - x.bound.min.x + x.bound.max.y - x.bound.min.y));
            var maxInflation = prototiles.SelectMany(x => x.ChildPrototiles).Select(x => x.transform.lossyScale).Max(x => 1/Mathf.Min(x.x, Mathf.Min(x.y, x.z)));
            var height = 2;
            var chunkSize = maxPrototileSize * (float)Math.Pow(maxInflation, height);

            return new DefaultDualMapping(this, chunkSize, CachePolicy.Always);
        }

        #endregion

        #region Cell info

        public IEnumerable<Cell> GetCells()
        {
            if (bound == null)
                throw new GridInfiniteException();

            return GetCellsInBounds(bound);
        }

        public abstract ICellType GetCellType(Cell cell);

        public abstract bool IsCellInGrid(Cell cell);

        #endregion

        #region Topology

        public abstract bool TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection);


        public bool TryMoveByOffset(Cell startCell, Vector3Int startOffset, Vector3Int destOffset, CellRotation startRotation, out Cell destCell, out CellRotation destRotation) => DefaultGridImpl.TryMoveByOffset(this, startCell, startOffset, destOffset, startRotation, out destCell, out destRotation);


        public bool ParallelTransport(IGrid aGrid, Cell aSrcCell, Cell aDestCell, Cell srcCell, CellRotation startRotation, out Cell destCell, out CellRotation destRotation) => DefaultGridImpl.ParallelTransport(aGrid, aSrcCell, aDestCell, this, srcCell, startRotation, out destCell, out destRotation);


        public IEnumerable<CellDir> GetCellDirs(Cell cell) => DefaultGridImpl.GetCellDirs(this, cell);

        public IEnumerable<CellCorner> GetCellCorners(Cell cell) => DefaultGridImpl.GetCellCorners(this, cell);

        public IEnumerable<(Cell, CellDir)> FindBasicPath(Cell startCell, Cell destCell) => throw new NotImplementedException();

        #endregion

        #region Index

        private void FillIndexCounts(int height)
        {
            if(indexCounts == null)
            {
                indexCounts = new List<Dictionary<InternalPrototile, int>>();
                indexCounts.Add(prototiles.ToDictionary(x => x, x => x.ChildTiles.Length));
            }

            for(var i=indexCounts.Count; i<=height; i++)
            {
                var prevIndexCounts = indexCounts[i - 1];
                indexCounts.Add(prototiles.ToDictionary(x => x, x => x.ChildPrototiles.Sum(y => prevIndexCounts[y.child])));
            }
        }

        public int IndexCount
        {
            get
            {
                if (bound == null)
                    throw new GridInfiniteException();
                FillIndexCounts(bound.Height);
                return indexCounts[bound.Height][GetPrototile(bound.Path, bound.Height)];
            }
        }

        public int GetIndex(Cell cell)
        {
            if (bound == null)
                throw new GridInfiniteException();
            var pathLength = GetPathDiffLength(cell, bound.Path);
            FillIndexCounts(pathLength - 1);
            var parent = GetPrototile(bound.Path, pathLength);
            var index = 0;
            for (var height = pathLength - 1; height >= 0; height--)
            {
                var p = GetPathAt(cell, height);
                for (var i = 0; i < p; i++)
                {
                    index += indexCounts[height][parent.ChildPrototiles[i].child];
                }
                parent = parent.ChildPrototiles[p].child;
            }
            return index + GetChildTileAt(cell);
        }

        public Cell GetCellByIndex(int index)
        {
            if (bound == null)
                throw new GridInfiniteException();
            var cell = bound.Path;
            /*
            var height = 0;
            InternalPrototile parent;
            while(true)
            {
                FillIndexCounts(height);
                if (indexCounts[height][parent = hierarchy(height)] > index)
                    break;
                height++;
            }
            */
            var height = bound.Height;
            var parent = GetPrototile(bound.Path, bound.Height);
            height--;
            for(;height >= 0; height--)
            {
                var p = 0;
                while(true)
                {
                    var i = indexCounts[height][parent.ChildPrototiles[p].child];
                    if(i <= index)
                    {
                        index -= i;
                    }
                    else
                    {
                        break;
                    }
                    p++;
                }

                cell = SetPathAt(cell, height, p);
                parent = parent.ChildPrototiles[p].child;
            }
            cell = SetChildTileAt(cell, index);
            return cell;
        }
        #endregion

        #region Bounds
        public IBound GetBound() => bound;

        public IBound GetBound(IEnumerable<Cell> cells)
        {
            int height = -1;
            Cell path = new Cell();
            foreach(var cell in cells)
            {
                if(height == -1)
                {
                    height = 0;
                    path = SetChildTileAt(cell, 0);
                }
                else
                {
                    var diffLength = GetPathDiffLength(path, cell);
                    if(diffLength > height)
                    {
                        height = diffLength;
                        path = ClearChildTileAndPathBelow(path, height);
                    }
                }
            }
            return new SubstitutionTilingBound
            {
                Height = height,
                Path = path,
            };
        }

        public abstract IGrid BoundBy(IBound bound);// => new SubstitutionTilingGrid(this, (SubstitutionTilingBound)(IntersectBounds(bound, this.bound)));

        public IBound IntersectBounds(IBound bound, IBound other)
        {
            if (bound == null) return other;
            if (other == null) return bound;
            // One bound must be a subset of the other, or the intersection is empty.
            var stBound = (SubstitutionTilingBound)bound;
            var stOther = (SubstitutionTilingBound)other;
            if (stBound.Height <= stOther.Height && ClearChildTileAndPathBelow(stBound.Path, stOther.Height) == stOther.Path)
                return stBound;
            if (stOther.Height < stBound.Height && ClearChildTileAndPathBelow(stOther.Path, stBound.Height) == stBound.Path)
                return stOther;

            return new SubstitutionTilingBound
            {
                Height = -1,
            };
        }

        public IBound UnionBounds(IBound bound, IBound other)
        {
            if (bound == null) return null;
            if (other == null) return null;
            var stBound = (SubstitutionTilingBound)bound;
            var stOther = (SubstitutionTilingBound)other;
            var newHeight = Math.Max(Math.Max(stBound.Height, stOther.Height), GetPathDiffLength(stBound.Path, stOther.Path));
            return new SubstitutionTilingBound
            {
                Height = newHeight,
                Path = ClearChildTileAndPathBelow(stBound.Path, newHeight),
            };
        }

        public IEnumerable<Cell> GetCellsInBounds(IBound bound)
        {
            var stBound = (SubstitutionTilingBound)bound;
            var stack = new Stack<(int height, InternalPrototile prototile, Cell partialPath)>();
            stack.Push((stBound.Height, GetPrototile(stBound.Path, stBound.Height), stBound.Path));
            while (stack.Count > 0)
            {
                var (height, prototile, partialPath) = stack.Pop();
                if (height == 0)
                {
                    for (var i = 0; i < prototile.ChildTiles.Length; i++)
                    {
                        yield return SetChildTileAt(partialPath, i);
                    }
                }
                else if(height < 0)
                {
                    continue;
                }
                else
                {
                    // Recurse
                    for (var i = 0; i < prototile.ChildPrototiles.Length; i++)
                    {
                        var child = prototile.ChildPrototiles[i].child;
                        stack.Push((height - 1, child, SetPathAt(partialPath, height - 1, i)));
                    }
                }
            }
        }

        public bool IsCellInBound(Cell cell, IBound bound)
        {
            var stBound = (SubstitutionTilingBound)bound;
            if (stBound != null)
            {
                return ClearChildTileAndPathBelow(cell, stBound.Height) == stBound.Path;
            }
            return true;
        }
        #endregion

        #region Position
        public abstract Vector3 GetCellCenter(Cell cell);

        public Vector3 GetCellCorner(Cell cell, CellCorner cellCorner)
        {
            GetPolygon(cell, out var vertices, out var transform);
            return transform.MultiplyPoint3x4(vertices[(int)cellCorner]);
        }

        protected Matrix4x4 GetTRS(InternalPrototile prototile, Matrix4x4 transform, int childTile)
        {
            // TODO: Translate this so the child is centered appropriately
            return transform;
        }

        public abstract TRS GetTRS(Cell cell);

        #endregion

        #region Shape
        public Deformation GetDeformation(Cell cell) => throw new NotImplementedException();

        public abstract void GetPolygon(Cell cell, out Vector3[] vertices, out Matrix4x4 transform);

        public IEnumerable<(Vector3, Vector3, Vector3, CellDir)> GetTriangleMesh(Cell cell)
        {
            throw new Grid2dException();
        }

        public void GetMeshData(Cell cell, out MeshData meshData, out Matrix4x4 transform)
        {
            DefaultGridImpl.GetMeshDataFromPolygon(this, cell, out meshData, out transform);
        }

        #endregion

        #region Query
        public abstract bool FindCell(Vector3 position, out Cell cell);
        public abstract bool FindCell(
            Matrix4x4 matrix,
            out Cell cell,
            out CellRotation rotation);
        public abstract IEnumerable<Cell> GetCellsIntersectsApprox(Vector3 min, Vector3 max);

        public abstract IEnumerable<RaycastInfo> Raycast(Vector3 origin, Vector3 direction, float maxDistance = float.PositiveInfinity);
        #endregion

        #region Symmetry
        public GridSymmetry FindGridSymmetry(ISet<Cell> src, ISet<Cell> dest, Cell srcCell, CellRotation cellRotation) => throw new NotImplementedException();

        public bool TryApplySymmetry(GridSymmetry s, IBound srcBound, out IBound destBound) => throw new NotImplementedException();

        public bool TryApplySymmetry(GridSymmetry s, Cell src, out Cell dest, out CellRotation r) => throw new NotImplementedException();
        #endregion



        protected struct Aabb
        {
            public Vector3 min;
            public Vector3 max;

            public Aabb(IEnumerable<Vector3> v)
            {
                min = v.Aggregate(Vector3.Min);
                max = v.Aggregate(Vector3.Max);
            }

            public static Aabb operator *(Matrix4x4 m, Aabb aabb)
            {
                var c = (aabb.min + aabb.max) / 2;
                var h = (aabb.max - aabb.min) / 2;

                c = m.MultiplyPoint3x4(c);
                var hx = m.MultiplyVector(new Vector3(h.x, 0, 0));
                var hy = m.MultiplyVector(new Vector3(0, h.y, 0));
                h = VectorUtils.Abs(hx) + VectorUtils.Abs(hy);
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
                while (i.MoveNext())
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

            public float? Raycast(Vector3 origin, Vector3 direction, float maxDistance)
            {
                if(MeshRaycast.RaycastAabbPlanar(origin, direction, min, max, out var distance) && distance <= maxDistance)
                {
                    return distance;
                }
                else
                {
                    return null;
                }
            }
        }

        protected class InternalPrototile
        {
            public string Name { get; set; }

            public (Matrix4x4 transform, InternalPrototile child)[] ChildPrototiles { get; set; }

            public (int fromChild, int fromChildSide, int toChild, int toChildSide)[] InteriorPrototileAdjacencies { get; set; }

            public (int parentSide, int parentSubSide, int parentSubSideCount, int child, int childSide)[] ExteriorPrototileAdjacencies { get; set; }
            
            public (int fromParentSide, int fromParentSubSide, int toParentSide, int toParentSubSide)[] PassthroughPrototileAdjacencies { get; set; }

            public Vector3[][] ChildTiles { get; set; }

            public Vector3[] Centers { get; set; }

            public (int fromChild, int fromChildSide, int toChild, int toChildSide)[] InteriorTileAdjacencies { get; set; }

            public (int parentSide, int parentSubSide, int parentSubSideCount, int child, int childSide)[] ExteriorTileAdjacencies { get; set; }

            public Aabb bound;

            public override string ToString() => Name;
        }
    }
}

