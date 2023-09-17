using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
		public (int fromParentSide, int fromParentSubSide, int toParentSide, int toParentSubSide)[] PassthroughPrototileAdjacencies { get; set; }


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

        public override string ToString() => Name;
    }

    public class SubstitutionTilingBound : IBound
    {
        public int Height { get; set; }
        public Cell Path { get; set; }
    }

    // Creates a tiling of the 2d plane from a set of substitution rules.
    // It is quite flexible:
    // * imperfect substitution rules where the replacement outline doesn't follow the original outline
    // * tiles can freely transformed
    //   * tile equivalence under translation, euclidian motion, isometry, similarity all supported
    //   * "statistically round" substitutions like the pinwheel substitution supported

    public class SubstitutionTilingGrid : IGrid
	{
        // Raycast and IntersectsAabb have a hard time knowing when to stop searching.
        // They give up when they haven't found anything interesting in this many heights.
        private const int DeadZone = 2;

        private readonly InternalPrototile[] prototiles;
        private int tileBits;
        private int prototileBits;
        private List<ICellType> cellTypes;
        private Func<int, InternalPrototile> hierarchy;
        private SubstitutionTilingBound bound;
        private Matrix4x4 baseTransform;
        // By height
        private List<Dictionary<InternalPrototile, int>> indexCounts;

        // Copy constructor
        private SubstitutionTilingGrid(SubstitutionTilingGrid other, SubstitutionTilingBound bound)
        {
            prototiles = other.prototiles;
            tileBits = other.tileBits;
            prototileBits = other.prototileBits;
            cellTypes = other.cellTypes;
            hierarchy = other.hierarchy;
            this.bound = bound;
            baseTransform = other.baseTransform;
        }

        // Copy constructor
        private SubstitutionTilingGrid(SubstitutionTilingGrid other, Func<int, InternalPrototile> hierarchy, Matrix4x4 baseTransform)
        {
            prototiles = other.prototiles;
            tileBits = other.tileBits;
            prototileBits = other.prototileBits;
            cellTypes = other.cellTypes;
            this.hierarchy = hierarchy;
            bound = other.bound;
            this.baseTransform = baseTransform;
        }


        public SubstitutionTilingGrid(Prototile[] prototiles, string[] hierarchy, SubstitutionTilingBound bound = null):
            this(prototiles, i => hierarchy[i % hierarchy.Length], bound)
        {

        }

        public SubstitutionTilingGrid(Prototile[] prototiles, Func<int, string> hierarchy, SubstitutionTilingBound bound = null)
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
            baseTransform = Matrix4x4.identity;

            BuildPrototileBounds();
        }

        #region Construction
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

        #region Crumb Utils
        private class Crumb
        {
            public Crumb parent;
            public int height;
            public Cell partialPath;

            // For now, don't need this?
            //public Matrix4x4 transform;
            public InternalPrototile prototile;
        }

        private Crumb GetCrumb(Cell cell, int height = 0)
        {
            var pathLength = Math.Max(GetPathLength(cell), height);
            var crumb = new Crumb
            {
                parent = null,
                height = pathLength,
                partialPath = new Cell(),
                // transform = ...,
                prototile = hierarchy(pathLength),
            };
            while(crumb.height > height)
                crumb = CreateChild(crumb, GetPathAt(cell, crumb.height - 1));

            return crumb;
        }

        private Crumb GetParent(Crumb crumb)
        {
            if(crumb.parent != null)
                return crumb.parent;

            //var transform = Up(crumb.transform, crumb.prototile);
            return crumb.parent = new Crumb
            {
                parent = null,
                height = crumb.height + 1,
                partialPath = crumb.partialPath,
                //transform = transform,
                prototile = hierarchy(crumb.height + 1),
            };
        }

        private Crumb CreateChild(Crumb crumb, int child)
        {
            //var (transform, prototile) = Down(crumb.transform, crumb.prototile, child);
            return new Crumb
            {
                parent = crumb,
                height = crumb.height - 1,
                partialPath = SetPathAt(crumb.partialPath, crumb.height - 1, child),
                //transform = transform,
                //prototile = prototile,
                prototile = crumb.prototile.ChildPrototiles[child].child,
            };
        }
        #endregion

        #region Other Utils


        // Utility for working with prototile transforms
        private Matrix4x4 Up(Matrix4x4 transform, InternalPrototile parent)
        {
            return transform * parent.ChildPrototiles[0].transform.inverse;
        }

        private (Matrix4x4, InternalPrototile) Down(Matrix4x4 transform, InternalPrototile prototile, int child)
        {
            var t = prototile.ChildPrototiles[child];
            return (transform * t.transform, t.child);
        }

        private (InternalPrototile prototile, Matrix4x4 prototileTransform, int childTile) LocateCell(Cell cell)
        {
            var childTile = GetChildTileAt(cell);
            var pathLength = GetPathLength(cell);
            var transform = baseTransform;
            var parent = hierarchy(0);
            for (var i = 0; i < pathLength; i++)
            {
                parent = hierarchy(i + 1);
                transform = Up(transform, parent);
            }
            for (var i = pathLength - 1; i >= 0; i--)
            {
                (transform, parent) = Down(transform, parent, GetPathAt(cell, i));
            }
            return (parent, transform, childTile);
        }

        private (InternalPrototile prototile, int childTile) GetPrototileAndChildTile(Cell cell)
        {
            var pathLength = GetPathLength(cell);
            var parent = hierarchy(pathLength);
            for (var i = pathLength - 1; i >= 0; i--)
            {
                parent = parent.ChildPrototiles[GetPathAt(cell, i)].child;
            }
            return (GetPrototile(cell, 0), GetChildTileAt(cell));
        }

        private InternalPrototile GetPrototile(Cell cell, int height)
        {
            var pathLength = GetPathLength(cell);
            var parent = hierarchy(pathLength);
            for (var i = pathLength - 1; i >= height; i--)
            {
                parent = parent.ChildPrototiles[GetPathAt(cell, i)].child;
            }
            return parent;
        }

        /// <summary>
        /// Returns n+1 parent elements for a path of length n
        /// </summary>
        private IList<InternalPrototile> Parents(Cell cell, int minHeight = 0)
        {
            var pathLength = GetPathLength(cell);
            var parents = new InternalPrototile[pathLength + 1];
            var parent = parents[pathLength] = hierarchy(pathLength);
            for (var i = pathLength - 1; i >= minHeight; i--)
            {
                var t = parent.ChildPrototiles[GetPathAt(cell, i)];
                parent = t.child;
                parents[i] = parent;
            }
            return parents;
        }

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
        public IGrid Unbounded => new SubstitutionTilingGrid(this, (SubstitutionTilingBound) null);

        public IGrid Unwrapped => this;

        public virtual IDualMapping GetDual()
        {
            var maxPrototileSize = prototiles.Max(x => (x.bound.max.x - x.bound.min.x + x.bound.max.y - x.bound.min.y));
            var maxInflation = prototiles.SelectMany(x => x.ChildPrototiles).Select(x => x.transform.lossyScale).Max(x => 1/Mathf.Min(x.x, Mathf.Min(x.y, x.z)));
            var height = 2;
            var chunkSize = maxPrototileSize * (float)Math.Pow(maxInflation, height);

            return new DefaultDualMapping(this, chunkSize, CachePolicy.Always);
        }


        public SubstitutionTilingGrid ParentGrid(int n = 1)
        {
            // Check subdividable
            if (tileBits != 0)
                throw new Exception("Parent/subdivision only works on substitution tiling grids with a single tile per prototile");

            var t = baseTransform;
            for (var i = 0; i < n; i++)
            {
                t = Up(t, hierarchy(i + 1));
            }

            return new SubstitutionTilingGrid(this, (h) => hierarchy(h + n), t);
        }

        public Cell CellToParentGrid(Cell cell, int n = 1)
        {
            // TODO: This can be done with bit shifting
            var l = GetPathLength(cell);
            var r = new Cell();
            for (var i = n; i < l; i++)
            {
                r = SetPathAt(r, i - n, GetPathAt(cell, i));
            }
            return r;
        }




        #endregion

        #region Cell info

        public IEnumerable<Cell> GetCells()
        {
            if (bound == null)
                throw new GridInfiniteException();

            return GetCellsInBounds(bound);
        }

        public ICellType GetCellType(Cell cell)
        {
            if (cellTypes.Count == 1)
                return cellTypes[0];
            var (prototile, childTile) = GetPrototileAndChildTile(cell);
            return NGonCellType.Get(prototile.ChildTiles[childTile].Length);
        }

        public bool IsCellInGrid(Cell cell)
        {
            // TODO: Don't use try-catch to validate
            try
            {
                GetPrototileAndChildTile(cell);
            }
            catch
            {
                return false;
            }

            if(bound != null)
            {
                return ClearChildTileAndPathBelow(cell, bound.Height) == bound.Path;
            }
            return true;
        }

        #endregion

        #region Topology

        public bool TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection)
        {
            var childTile = GetChildTileAt(cell);
            var parents = Parents(cell);
            var prototile = 0 < parents.Count ? parents[0] : hierarchy(0);
            var tileInterior = prototile.InteriorTileAdjacencies
                .Where(x => x.fromChild == childTile && x.fromChildSide == (int)dir)
                .ToList();
            if (tileInterior.Count == 1)
            {
                dest = SetChildTileAt(cell, tileInterior[0].toChild);
                inverseDir = (CellDir)tileInterior[0].toChildSide;
                connection = new Connection();
                return true;
            }
            var tileExterior = prototile.ExteriorTileAdjacencies
                .Where(x => x.child == childTile && x.childSide == (int)dir)
                .Single();

            {
                var (otherPartialPath, otherSide, otherPrototile) = TryMovePrototile(0, cell, tileExterior.parentSide, parents);


                var otherSubside = tileExterior.parentSubSideCount - 1 - tileExterior.parentSubSide;
                var otherExterior = otherPrototile.ExteriorTileAdjacencies
                    .Where(x => x.parentSide == otherSide && x.parentSubSide == otherSubside)
                    .Single();
                if (otherExterior.parentSubSideCount != tileExterior.parentSubSideCount)
                    throw new Exception();
                //partialPath.Insert(0, otherExterior.child);
                //return (partialPath, otherExterior.childSide, prototilesByName[otherParent.ChildPrototiles[otherExterior.child].childName]);

                dest = SetChildTileAt(otherPartialPath, otherExterior.child);
                inverseDir = (CellDir)otherExterior.childSide;
                connection = default;
                return true;
            }
        }



        // Given a prototile (height, partialPath), and a side of that prototile,
        // Moves to the adjacent prototile at the same height, returning which side we entered from.
        // Parents must the list of parent prototiles for partial path. It's indexed from height 0, but it'll only be inspected at height+1 and above.
        private  (Cell partialPath, int side, InternalPrototile prototile) TryMovePrototile(int height, Cell partialPath, int prototileSide, IList<InternalPrototile> parents)
        {
            var childPrototile = GetPathAt(partialPath, height);
            var parent = height + 1 < parents.Count ? parents[height + 1] : hierarchy(height + 1);

            // Can does the parent directly tell us what is adjacent?
            var interior = parent.InteriorPrototileAdjacencies
                .Where(x => x.fromChild == childPrototile && x.fromChildSide == prototileSide)
                .ToList();
            if (interior.Count == 1)
            {
                var toChild = interior[0].toChild;
                return (SetPathAt(partialPath, height, toChild), interior[0].toChildSide, parent.ChildPrototiles[toChild].child);
            }

            // Nope. We'll have to do a move at height one up, then align subsides
            var exterior = parent.ExteriorPrototileAdjacencies
                .Where(x => x.child == childPrototile && x.childSide == prototileSide)
                .Single();
            {
                var (otherPartialPath, otherSide, otherParent) = TryMovePrototile(height + 1, partialPath, exterior.parentSide, parents);

                var otherSubside = exterior.parentSubSideCount - 1 - exterior.parentSubSide;

                while (true)
                {
                    if (otherParent.PassthroughPrototileAdjacencies != null)
                    {
                        var passThrough = otherParent.PassthroughPrototileAdjacencies
                            .Where(x => x.fromParentSide == otherSide && x.fromParentSubSide == otherSubside)
                            .ToList();

                        if (passThrough.Count == 1)
                        {
                            // TODO: We can probably optimize this better
                            parents = Parents(otherPartialPath, height + 1);
                            (otherPartialPath, otherSide, otherParent) = TryMovePrototile(height + 1, otherPartialPath, passThrough[0].toParentSide, parents);
                            otherSubside = passThrough[0].toParentSubSide;
                            continue;
                        }
                    }
                    break;
                }

                var otherExterior = otherParent.ExteriorPrototileAdjacencies
                    .Where(x => x.parentSide == otherSide && x.parentSubSide == otherSubside)
                    .Single();
                //if (otherExterior.parentSubSideCount != exterior.parentSubSideCount)
                //    throw new Exception($"At height {height+1}, moving from {otherSide}, {otherSubside}, expected {exterior.parentSubSideCount} subSide count, but got ");
                return (SetPathAt(otherPartialPath, height, otherExterior.child), otherExterior.childSide, otherParent.ChildPrototiles[otherExterior.child].child);
            }
        }

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
            var pathLength = GetPathLength(cell);
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
            var cell = new Cell();
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

        public IGrid BoundBy(IBound bound) => new SubstitutionTilingGrid(this, (SubstitutionTilingBound)(IntersectBounds(bound, this.bound)));

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
        public Vector3 GetCellCenter(Cell cell)
        {
            var (prototile, transform, childTile) = LocateCell(cell);
            return transform.MultiplyPoint3x4(prototile.Centers[childTile]);
        }

        public Vector3 GetCellCorner(Cell cell, CellCorner cellCorner)
        {
            GetPolygon(cell, out var vertices, out var transform);
            return transform.MultiplyPoint3x4(vertices[(int)cellCorner]);
        }

        private Matrix4x4 GetTRS(InternalPrototile prototile, Matrix4x4 transform, int childTile)
        {
            // TODO: Translate this so the child is centered appropriately
            return transform;
        }

        public TRS GetTRS(Cell cell)
        {
            var (prototile, transform, childTile) = LocateCell(cell);
            return new TRS(GetTRS(prototile, transform, childTile));
        }

        #endregion

        #region Shape
        public Deformation GetDeformation(Cell cell) => throw new NotImplementedException();

        public void GetPolygon(Cell cell, out Vector3[] vertices, out Matrix4x4 transform)
        {
            var (prototile, prototileTransform, childTile) = LocateCell(cell);
            vertices = prototile.ChildTiles[childTile];
            transform = prototileTransform;
        }

        public IEnumerable<(Vector3, Vector3, Vector3, CellDir)> GetTriangleMesh(Cell cell)
        {
            throw new Grid2dException();
        }

        public void GetMeshData(Cell cell, out MeshData meshData, out Matrix4x4 transform)
        {
            throw new Grid2dException();
        }

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
                        if (GeometryUtils.IsPointInTrianglePlanar(transformedPosition, v0, prev, v))
                        {
                            cell = SetChildTileAt(partialPath, childIndex);
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
            out CellRotation rotation)
        {
            var position = matrix.MultiplyPoint3x4(new Vector3());
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
                        if (GeometryUtils.IsPointInTrianglePlanar(transformedPosition, v0, prev, v))
                        {
                            // Get the cell
                            cell = SetChildTileAt(partialPath, childIndex);
                            // Get the rotation
                            var cellType = NGonCellType.Get(prototile.ChildTiles[childIndex].Length);
                            var cellTransform = GetTRS(prototile, transform, childIndex);
                            return MeshGrid.GetRotationFromMatrix(cellType, cellTransform, matrix, out rotation);
                        }
                        prev = v;
                    }
                    continue;
                }
            }
            cell = default;
            rotation = default;
            return false;
        }

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


        // Returns a set of non-overlapping prototiles at different heights that collectively contain all prototiles
        private IEnumerable<(int height, InternalPrototile prototile, Matrix4x4 transform, Cell partialPath)> Spigot()
        {
            var height = 0;
            var transform = baseTransform;
            var path = new Cell();
            while (true)
            {
                var parent = hierarchy(height + 1);
                transform = Up(transform, parent);

                // Skips 0, we just came from there!
                for (var i = height == 0 ? 0 : 1; i < parent.ChildPrototiles.Length; i++)
                {
                    var (childTransform, child) = Down(transform, parent, i);
                    yield return (height, child, childTransform, SetPathAt(path, height, i));
                }

                height = height + 1;
            }
        }


        // Returns the set of height 0 prototiles intersect inputAabb
        // This is done by iterating over spigot (an infinite stream of prototiles)
        // and recursively subdividing ones that look promising.
        // We stop when we haven't found anything in a while.
        private IEnumerable<(InternalPrototile prototile, Matrix4x4 transform, Cell partialPath)> GetPrototilesIntersectsApproxInternal(Aabb inputAabb)
        {
            var stack = new Stack<(int height, InternalPrototile prototile, Matrix4x4 transform, Cell partialPath)>();
            var highestFoundHeight = -1;
            foreach (var t in Spigot())
            {
                if (highestFoundHeight <= t.height - DeadZone && highestFoundHeight != -1)
                {
                    // Haven't found anything in a while, give up.
                    break;
                }

                // For each of these prototiles, walk the entire tree under it
                stack.Push(t);
                while (stack.Count > 0)
                {
                    var (height, prototile, transform, partialPath) = stack.Pop();
                    var bound = transform * prototile.bound;

                    if (bound.Intersects(inputAabb))
                    {
                        if (height == 0)
                        {
                            highestFoundHeight = Math.Max(highestFoundHeight, t.height);
                            yield return (prototile, transform, partialPath);
                        }
                        else
                        {
                            // Recurse
                            for (var i = 0; i < prototile.ChildPrototiles.Length; i++)
                            {
                                var (childTransform, child) = Down(transform, prototile, i);
                                stack.Push((height - 1, child, childTransform, SetPathAt(partialPath, height - 1, i)));
                            }
                        }
                    }
                }
            }
        }

        public IEnumerable<RaycastInfo> Raycast(Vector3 origin, Vector3 direction, float maxDistance = float.PositiveInfinity)
        {
            // Computes distance to interection of aabb, or null if it misses the truncated array.
            Func<Matrix4x4, InternalPrototile, float?> getDist = (transform, prototile) =>
            {
                var iTransform = transform.inverse;
                var localOrigin = iTransform.MultiplyPoint3x4(origin);
                var localDirection = iTransform.MultiplyVector(direction);
                return prototile.bound.Raycast(localOrigin, localDirection, maxDistance);
            };
            var queuedRaycastInfos = new PriorityQueue<RaycastInfo>(x => x.distance, (x, y) => -x.distance.CompareTo(y.distance));
            foreach (var (prototile, transform, partialPath, dist) in RaycastPrototiles(getDist))
            {
                // Raycast against the actual children of this prototile
                for (var i = 0; i < prototile.ChildTiles.Length; i++)
                {
                    if(MeshRaycast.RaycastPolygonPlanar(origin, direction, prototile.ChildTiles[i], transform, out var point, out var childDist, out var side) 
                       && childDist < maxDistance)
                    {
                        var ri = new RaycastInfo
                        {
                            cell = SetChildTileAt(partialPath, i),
                            cellDir = (CellDir?)side,
                            distance = childDist,
                            point = point,
                        };
                        queuedRaycastInfos.Add(ri);
                    }
                }

                // Drain the queue. As the prototiles are in order, and their child tiles are inside their bounds and thus always have a larger dist
                // we know we've seen everything up to dist
                foreach(var ri in queuedRaycastInfos.Drain(dist))
                {
                    yield return ri;
                }
            }
            foreach (var ri in queuedRaycastInfos.Drain())
            {
                yield return ri;
            }
        }


        /// <summary>
        /// Returns all the height 0 prototiles that intersect the ray, in the correct order.
        /// Much like GetPrototilesIntersectsApproxInternal, this walks over all the prototiles in Spigot,
        /// and recursively subdivides them if their bounds indiate they may contain a tile that intersects the ray.
        /// 
        /// As with that method, we stop if we haven't found anything worth outputting in a while.
        /// 
        /// But it's complicated by the fact we must output items in order. To do so, items are kept in a queue, and with similar logic to the stopping logic, we only output 
        /// something when we haven't found anything smaller in a while.
        /// </summary>
        private IEnumerable<(InternalPrototile prototile, Matrix4x4 transform, Cell partialPath, float minDist)> RaycastPrototiles(Func<Matrix4x4, InternalPrototile, float?> getDist)
        {
            var queue = new PriorityQueue<(int initialHeight, int height, InternalPrototile prototile, Matrix4x4 transform, Cell partialPath, float dist)>(x => x.dist, (x, y) => -x.dist.CompareTo(y.dist));
            
            var highestFoundHeight = -1;
            foreach (var t in Spigot())
            {
                if (queue.Count == 0 && highestFoundHeight <= t.height - DeadZone && highestFoundHeight != -1)
                {
                    // Haven't found anything in a while, give up.
                    break;
                }

                var dist = getDist(t.transform, t.prototile);
                if (dist != null)
                {
                    queue.Add((t.height, t.height, t.prototile, t.transform, t.partialPath, dist.Value));
                }

                while(queue.Count > 0)
                {
                    var t2 = queue.Peek();
                    if(t2.height > 0)
                    {
                        var (initialHeight, itemHeight, prototile, transform, partialPath, _) = queue.Pop();
                        // Recurse
                        for (var i = 0; i < prototile.ChildPrototiles.Length; i++)
                        {
                            var (childTransform, child) = Down(transform, prototile, i);
                            var childDist = getDist(childTransform, child);
                            if (childDist != null)
                            {
                                queue.Add((initialHeight, itemHeight - 1, child, childTransform, SetPathAt(partialPath, itemHeight - 1, i), childDist.Value));
                            }
                        }
                    }
                    else
                    {
                        // We've found a height 0 prototile that is ahead of everythign else in the queue.
                        // But is it also ahead of future items we'll find from Spigot?
                        if (t2.initialHeight <= t.height - DeadZone)
                        {
                            var (initialHeight, itemHeight, prototile, transform, partialPath, dist2) = queue.Pop();
                            highestFoundHeight = Math.Max(highestFoundHeight, initialHeight);
                            yield return (prototile, transform, partialPath, dist2);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            // Drain queue
            while (queue.Count > 0)
            {
                var (initialHeight, itemHeight, prototile, transform, partialPath, dist2) = queue.Pop();
                if (itemHeight > 0)
                {
                    // Recurse
                    for (var i = 0; i < prototile.ChildPrototiles.Length; i++)
                    {
                        var (childTransform, child) = Down(transform, prototile, i);
                        var childDist = getDist(childTransform, child);
                        if (childDist != null)
                        {
                            queue.Add((initialHeight, itemHeight - 1, child, childTransform, SetPathAt(partialPath, itemHeight - 1, i), childDist.Value));
                        }
                    }
                }
                else
                {
                    yield return (prototile, transform, partialPath, dist2);
                }
            }
        }
        #endregion

        #region Symmetry
        public GridSymmetry FindGridSymmetry(ISet<Cell> src, ISet<Cell> dest, Cell srcCell, CellRotation cellRotation) => throw new NotImplementedException();

        public bool TryApplySymmetry(GridSymmetry s, IBound srcBound, out IBound destBound) => throw new NotImplementedException();

        public bool TryApplySymmetry(GridSymmetry s, Cell src, out Cell dest, out CellRotation r) => throw new NotImplementedException();
        #endregion



        private struct Aabb
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

        private class InternalPrototile
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

