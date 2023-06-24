using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sylves
{
    /// <summary>
    /// Turns any bounded grid into a grid which connects back on itself when you leave the grounds. 
    /// This is done via a canonicalize method that is responsible for replacing cells that are outside of the bounds.
    /// </summary>
    public class WrapModifier : BaseModifier
    {
        private readonly Func<Cell, Cell?> canonicalize;
        private readonly IGrid unboundedUnderlying;
        public WrapModifier(IGrid underlying, Func<Cell, Cell?> canonicalize) : base(underlying)
        {
            unboundedUnderlying = underlying.Unbounded;
            this.canonicalize = canonicalize;
        }
        protected override IGrid Rebind(IGrid underlying)
        {
            return new WrapModifier(underlying, canonicalize);
        }

        public Cell? Canonicalize(Cell cell) => canonicalize(cell);

        #region Relatives
        // It no longer makes sense to consider an unbounded variant of this.
        public override IGrid Unbounded => this;
        #endregion

        #region Topology
        public override bool TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection)
        {
            if(!unboundedUnderlying.TryMove(cell, dir, out var dest1, out inverseDir, out connection))
            {
                dest = default;
                return false;
            }
            var dest2 = canonicalize(dest1);
            dest = dest2 ?? default;
            return dest2 != null;
        }

        public override bool TryMoveByOffset(Cell startCell, Vector3Int startOffset, Vector3Int destOffset, CellRotation startRotation, out Cell destCell, out CellRotation destRotation)
        {
            if(!unboundedUnderlying.TryMoveByOffset(startCell, startOffset, destOffset, startRotation, out var dest1, out destRotation))
            {
                destCell = default;
                return false;
            }
            var dest2 = canonicalize(dest1);
            destCell = dest2 ?? default;
            return dest2 != null;
        }

        public override bool ParallelTransport(IGrid aGrid, Cell aSrcCell, Cell aDestCell, Cell srcCell, CellRotation startRotation, out Cell destCell, out CellRotation destRotation)
        {
            if(!unboundedUnderlying.ParallelTransport(aGrid, aSrcCell, aDestCell, srcCell, startRotation, out var dest1, out destRotation))
            {
                destCell = default;
                return false;
            }
            var dest2 = canonicalize(dest1);
            destCell = dest2 ?? default;
            return dest2 != null;
        }


        #endregion

        #region Shape
        public override IEnumerable<RaycastInfo> Raycast(Vector3 origin, Vector3 direction, float maxDistance = float.PositiveInfinity)
        {
            foreach (var info in unboundedUnderlying.Raycast(origin, direction, maxDistance))
            {
                var info2 = info;
                var cell = canonicalize(info.cell);
                if (cell != null)
                {
                    info2.cell = cell.Value;
                    yield return info2;
                }
            }
        }
        #endregion


        #region Symmetry

        public override GridSymmetry FindGridSymmetry(ISet<Cell> src, ISet<Cell> dest, Cell srcCell, CellRotation cellRotation)
        {
            // This is not a bijection, we should really make a set class that handles this property.
            var uncanonicalDest = new BijectSet(dest, c => c, c => canonicalize(c).Value);
            var s = Underlying.FindGridSymmetry(src, uncanonicalDest, srcCell, cellRotation);
            if(s == null)
                return null;
            var src2 = canonicalize(s.Src);
            var dest2 = canonicalize(s.Dest);
            if (src2 == null || dest2 == null)
                return null;
            return new GridSymmetry
            {
                Rotation = s.Rotation,
                Src = src2.Value,
                Dest = dest2.Value,
            };
        }

        public override bool TryApplySymmetry(GridSymmetry s, Cell src, out Cell dest, out CellRotation r)
        {
            if(!Underlying.TryApplySymmetry(s, src, out dest, out r))
            {
                return false;
            }
            var dest2 = canonicalize(dest);
            dest = dest2 ?? default;
            return dest2 != null;

        }
        #endregion
    }
}
