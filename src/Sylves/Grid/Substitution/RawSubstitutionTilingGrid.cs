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

    // Implementation of SubstitutionTilingGrid with no caching, but as low allocation as possible
    public class RawSubstitutionTilingGrid : BaseSubstitutionTilingGrid
    {
        internal RawSubstitutionTilingGrid(BaseSubstitutionTilingGrid other, SubstitutionTilingBound bound) : base(other, bound)
        {
        }

        private RawSubstitutionTilingGrid(RawSubstitutionTilingGrid other, Func<int, InternalPrototile> hierarchy, Matrix4x4 baseTransform) : base(other, hierarchy, baseTransform)
        {
        }

        public RawSubstitutionTilingGrid(Prototile[] prototiles, string[] hierarchy, SubstitutionTilingBound bound = null) : base(prototiles, hierarchy, bound)
        {
        }

        public RawSubstitutionTilingGrid(Prototile[] prototiles, Func<int, string> hierarchy, SubstitutionTilingBound bound = null) : base(prototiles, hierarchy, bound)
        {
        }

        #region Utils

        // Some of the most common things we want to look up
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
            return (parent, GetChildTileAt(cell));
        }

        protected override InternalPrototile GetPrototile(Cell cell, int height)
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


        #region Relatives
        public override IGrid Unbounded => new RawSubstitutionTilingGrid(this, (SubstitutionTilingBound)null);


        public RawSubstitutionTilingGrid ParentGrid(int n = 1)
        {
            // Check subdividable
            if (tileBits != 0)
                throw new Exception("Parent/subdivision only works on substitution tiling grids with a single tile per prototile");

            var t = baseTransform;
            for (var i = 0; i < n; i++)
            {
                t = Up(t, hierarchy(i + 1));
            }

            return new RawSubstitutionTilingGrid(this, (h) => hierarchy(h + n), t);
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

        #region CellInfo

        public override ICellType GetCellType(Cell cell)
        {
            if (cellTypes.Count == 1)
                return cellTypes[0];
            var (prototile, childTile) = GetPrototileAndChildTile(cell);
            return NGonCellType.Get(prototile.ChildTiles[childTile].Length);
        }

        public override bool IsCellInGrid(Cell cell)
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

            if (bound != null)
            {
                return ClearChildTileAndPathBelow(cell, bound.Height) == bound.Path;
            }
            return true;
        }

        #endregion

        #region Topology

        public override bool TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection)
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
        private (Cell partialPath, int side, InternalPrototile prototile) TryMovePrototile(int height, Cell partialPath, int prototileSide, IList<InternalPrototile> parents)
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
        #endregion

        #region Bounds
        public override IGrid BoundBy(IBound bound) => new RawSubstitutionTilingGrid(this, (SubstitutionTilingBound)(IntersectBounds(bound, this.bound)));
        #endregion

        #region Position
        public override Vector3 GetCellCenter(Cell cell)
        {
            var(prototile, transform, childTile) = LocateCell(cell);
            return transform.MultiplyPoint3x4(prototile.Centers[childTile]);
        }

        public override TRS GetTRS(Cell cell)
        {
            var (prototile, transform, childTile) = LocateCell(cell);
            return new TRS(GetTRS(prototile, transform, childTile));
        }

        #endregion


        #region Shape
        public override void GetPolygon(Cell cell, out Vector3[] vertices, out Matrix4x4 transform)
        {
            var (prototile, prototileTransform, childTile) = LocateCell(cell);
            vertices = prototile.ChildTiles[childTile];
            transform = prototileTransform;
        }
        #endregion
    }
}

