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
    // The cache plan:

    // * Add a method to find lowest ancestor in cache, and fill down crumbs (LoadCell).
    // * Amend functions as following
    //   * Anything with Parents()  (TryMove) - Just force get crumb
    //   * Anything with Up() - Force get crumb
    //   * Anything with Down() - Pass through crumb, if caching on
    //   * Anything with .childTile[index] 


    public class SubstitutionTilingGrid : BaseSubstitutionTilingGrid
    {
        private ICachePolicy cachePolicy;
        private Dictionary<(int Height, Cell Path), Crumb> crumbCache;


        private SubstitutionTilingGrid(SubstitutionTilingGrid other, SubstitutionTilingBound bound) : base(other, bound)
        {
            // Can share cache - bounds do not change anything fundamental
            cachePolicy = other.cachePolicy;
            crumbCache = other.crumbCache;
        }

        private SubstitutionTilingGrid(SubstitutionTilingGrid other, Func<int, InternalPrototile> hierarchy, Matrix4x4 baseTransform) : base(other, hierarchy, baseTransform)
        {
            cachePolicy = other.cachePolicy;
            crumbCache = new Dictionary<(int Height, Cell Path), Crumb>();
        }

        public SubstitutionTilingGrid(Prototile[] prototiles, string[] hierarchy, SubstitutionTilingBound bound = null, ICachePolicy cachePolicy = null) : this(prototiles, i => hierarchy[i % hierarchy.Length], bound, cachePolicy)
        {
        }

        public SubstitutionTilingGrid(Prototile[] prototiles, Func<int, string> hierarchy, SubstitutionTilingBound bound = null, ICachePolicy cachePolicy = null) : base(prototiles, hierarchy, bound)
        {
            this.cachePolicy = cachePolicy ?? CachePolicy.Always;
            crumbCache = new Dictionary<(int Height, Cell Path), Crumb>();
        }


        #region Crumb Utils

        /// <summary>
        /// Crumbs are a node in the partially evaluated tree of nodes.
        /// Crumbs always have their parent defined, unless their parent is
        /// a hierarchy path, then it's optional.
        /// Crumbs do not store children, the circulare reference would interfer with 
        /// garbage collecting unused parts of the tree. Children can be created from scratch
        /// or read out of the cache instead.
        /// </summary>
        private class Crumb
        {
            // Use GetParent, don't read this!
            public Crumb parent;

            public int height;
            public Cell partialPath;

            // For now, don't need this?
            //public Matrix4x4 transform;
            public InternalPrototile prototile;

            public override string ToString() => $"{height}/{partialPath}";
        }


        private void EnsureCached(Crumb crumb)
        {
            crumbCache[(crumb.height, crumb.partialPath)] = crumb;
        }

        // Force creates a crumb which is the ancestor of cell at the given height.
        // Uses the cache if possible
        private Crumb GetCrumb(Cell cell, int height = 0)
        {
            var pathLength = Math.Max(GetPathLength(cell), height);

            // Find the closed crumb we can walk down from
            Crumb crumb = null;
            // TODO: Rather than linear search, could do bisection?
            var partialPath = ClearChildTileAndPathBelow(cell, height);
            var h = height;
            while (h < pathLength)
            {
                if (crumbCache.TryGetValue((h, partialPath), out crumb))
                {
                    break;
                }
                partialPath = SetPathAt(partialPath, h, 0);
                h++;
            }

            // Nothing found, isntead create a crumb we can walk down from
            if (crumb == null)
            {
                crumb = new Crumb
                {
                    parent = null,
                    height = pathLength,
                    partialPath = new Cell(),
                    // transform = ...,
                    prototile = hierarchy(pathLength),
                };
                EnsureCached(crumb);
            }

            // Walk down from crumb to desired height
            while (crumb.height > height)
                crumb = GetChild(crumb, GetPathAt(cell, crumb.height - 1));

            return crumb;
        }

        // Gets the crumb that is the parent of the crumb,
        // This is being fast is the main reason to have crumbs.
        private Crumb GetParent(Crumb crumb)
        {
            if (crumb.parent != null)
                return crumb.parent;

            // If we're here, parent is on the hierarchy.

            //var transform = Up(crumb.transform, crumb.prototile);
            var parentCrumb = crumb.parent = new Crumb
            {
                parent = null,
                height = crumb.height + 1,
                partialPath = crumb.partialPath,
                //transform = transform,
                prototile = hierarchy(crumb.height + 1),
            };

            EnsureCached(parentCrumb);

            return parentCrumb;
        }

        private Crumb GetChild(Crumb crumb, int childIndex)
        {
            var height = crumb.height - 1;
            var partialPath = SetPathAt(crumb.partialPath, crumb.height - 1, childIndex);
            Crumb child;
            if (crumbCache != null)
            {
                if (crumbCache.TryGetValue((height, partialPath), out child))
                {
                    return child;
                }
            }

            //var (transform, prototile) = Down(crumb.transform, crumb.prototile, childIndex);
            child = new Crumb
            {
                parent = crumb,
                height = height,
                partialPath = partialPath,
                //transform = transform,
                //prototile = prototile,
                prototile = crumb.prototile.ChildPrototiles[childIndex].child,
            };

            EnsureCached(child);
            return child;
        }
        #endregion


        #region Relatives
        public override IGrid Unbounded => new SubstitutionTilingGrid(this, (SubstitutionTilingBound)null);

        public RawSubstitutionTilingGrid Raw => new RawSubstitutionTilingGrid(this, bound);


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

        #region Topology

        public override bool TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection)
        {
            var childTile = GetChildTileAt(cell);
            var crumb = GetCrumb(cell);
            var prototile = crumb.prototile;
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
                var (otherCrumb, otherSide) = TryMovePrototile(crumb, tileExterior.parentSide);
                var otherPrototile = otherCrumb.prototile;
                var otherPartialPath = otherCrumb.partialPath;

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
        private (Crumb crumb, int side) TryMovePrototile(Crumb crumb, int prototileSide)
        {
            var partialPath = crumb.partialPath;
            var height = crumb.height;
            var childPrototile = GetPathAt(partialPath, height);
            var parentCrumb = GetParent(crumb);
            var parent = parentCrumb.prototile;

            // Can does the parent directly tell us what is adjacent?
            var interior = parent.InteriorPrototileAdjacencies
                .Where(x => x.fromChild == childPrototile && x.fromChildSide == prototileSide)
                .ToList();
            if (interior.Count == 1)
            {
                return (GetChild(parentCrumb, interior[0].toChild), interior[0].toChildSide);
            }

            // Nope. We'll have to do a move at height one up, then align subsides
            var exterior = parent.ExteriorPrototileAdjacencies
                .Where(x => x.child == childPrototile && x.childSide == prototileSide)
                .Single();
            {
                var (otherParentCrumb, otherSide) = TryMovePrototile(parentCrumb, exterior.parentSide);
                var otherParent = otherParentCrumb.prototile;

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
                            (otherParentCrumb, otherSide) = TryMovePrototile(otherParentCrumb, passThrough[0].toParentSide);
                            otherParent = otherParentCrumb.prototile;
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
                return (GetChild(otherParentCrumb, otherExterior.child), otherExterior.childSide);
            }
        }
        #endregion

        #region Bounds
        public override IGrid BoundBy(IBound bound) => new SubstitutionTilingGrid(this, (SubstitutionTilingBound)(IntersectBounds(bound, this.bound)));
        #endregion
    }
}

