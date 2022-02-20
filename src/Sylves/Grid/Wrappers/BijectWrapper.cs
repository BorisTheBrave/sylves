using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylves
{
    /// <summary>
    /// Remaps the cells of the grid by changing their co-ordinates,
    /// without touching the position, shape or topology.
    /// </summary>
    public class BijectWrapper : BaseWrapper
    {
        private readonly Func<Cell, Cell> toUnderlying;
        private readonly Func<Cell, Cell> fromUnderlying;

        public BijectWrapper(IGrid underlying, Func<Cell, Cell> toUnderlying, Func<Cell, Cell> fromUnderlying) : base(underlying)
        {
            this.toUnderlying = toUnderlying;
            this.fromUnderlying = fromUnderlying;
        }

        protected override IGrid Rebind(IGrid underlying)
        {
            return new BijectWrapper(underlying, toUnderlying, fromUnderlying);
        }


        #region Cell info

        public override ICellType GetCellType(Cell cell) => Underlying.GetCellType(toUnderlying(cell));
        #endregion

        #region Topology

        public override bool TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection)
        {
            if(Underlying.TryMove(toUnderlying(cell), dir, out var uDest, out inverseDir, out connection))
            {
                dest = fromUnderlying(uDest);
                return true;
            }
            else
            {
                dest = default;
                return false;
            }
        }

        public override bool TryMoveByOffset(Cell startCell, Vector3Int startOffset, Vector3Int destOffset, CellRotation startRotation, out Cell destCell, out CellRotation destRotation)
        {
            if (Underlying.TryMoveByOffset(startCell, startOffset, destOffset, startRotation, out var uDestCell, out destRotation))
            {
                destCell = fromUnderlying(uDestCell);
                return true;
            }
            else
            {
                destCell = default;
                return false;
            }
        }

        public override IEnumerable<CellDir> GetCellDirs(Cell cell) => Underlying.GetCellDirs(toUnderlying(cell));

        #endregion

        #region Index
        public override int GetIndex(Cell cell) => Underlying.GetIndex(toUnderlying(cell));

        public override Cell GetCellByIndex(int index) => fromUnderlying(Underlying.GetCellByIndex(index));
        #endregion


        #region Bounds
        public override IBound GetBound(IEnumerable<Cell> cells) => Underlying.GetBound(cells.Select(toUnderlying));
        #endregion

        #region Position
        public override Vector3 GetCellCenter(Cell cell) => Underlying.GetCellCenter(toUnderlying(cell));

        public override TRS GetTRS(Cell cell) => Underlying.GetTRS(toUnderlying(cell));

        public override Deformation GetDeformation(Cell cell) => Underlying.GetDeformation(toUnderlying(cell));
        #endregion

        #region Query
        public override bool FindCell(Vector3 position, out Cell cell)
        {
            if(Underlying.FindCell(position, out var uCell))
            {
                cell = fromUnderlying(uCell);
                return true;
            }
            else
            {
                cell = default;
                return false;
            }
        }

        public override bool FindCell(
            Matrix4x4 matrix,
            out Cell cell,
            out CellRotation rotation)
        {
            if(Underlying.FindCell(matrix, out var uCell, out rotation))
            {
                cell = fromUnderlying(uCell);
                return true;
            }
            else
            {
                cell = default;
                return false;
            }
        }

        public override IEnumerable<Cell> GetCellsIntersectsApprox(Vector3 min, Vector3 max) => Underlying.GetCellsIntersectsApprox(min, max).Select(fromUnderlying);
        #endregion
    }
}
