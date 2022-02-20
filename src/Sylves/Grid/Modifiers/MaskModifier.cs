using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylves
{
    /// <summary>
    /// Filters the cells in the the grid to a customizable subset.
    /// </summary>
    public class MaskModifier : BaseModifier
    {
        private readonly Func<Cell, bool> containsFunc;
        private readonly IEnumerable<Cell> allCells;

        public MaskModifier(IGrid underlying, ISet<Cell> allCells) : this(underlying, allCells.Contains, allCells)
        {

        }


        public MaskModifier(IGrid underlying, Func<Cell, bool> containsFunc, IEnumerable<Cell> allCells = null) : base(underlying)
        {
            this.containsFunc = containsFunc;
            this.allCells = allCells;
        }

        protected override IGrid Rebind(IGrid underlying)
        {
            return new MaskModifier(underlying, containsFunc, allCells);
        }

        #region Basics

        public override bool IsFinite => allCells != null || Underlying.IsFinite;

        #endregion

        #region Cell info

        public override IEnumerable<Cell> GetCells() => allCells ?? Underlying.GetCells().Where(containsFunc);

        #endregion

        #region Topology

        public override bool TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection) => Underlying.TryMove(cell, dir, out dest, out inverseDir, out connection) && containsFunc(dest);

        public override bool TryMoveByOffset(Cell startCell, Vector3Int startOffset, Vector3Int destOffset, CellRotation startRotation, out Cell destCell, out CellRotation destRotation) => Underlying.TryMoveByOffset(startCell, startOffset, destOffset, startRotation, out destCell, out destRotation) && containsFunc(destCell);

        #endregion

        #region Bounds
        public override IEnumerable<Cell> GetCellsInBounds(IBound bound) => Underlying.GetCellsInBounds(bound).Where(containsFunc);
        #endregion

        #region Query
        public override bool FindCell(Vector3 position, out Cell cell) => Underlying.FindCell(position, out cell) && containsFunc(cell);

        public override bool FindCell(
            Matrix4x4 matrix,
            out Cell cell,
            out CellRotation rotation) => Underlying.FindCell(matrix, out cell, out rotation) && containsFunc(cell);

        public override IEnumerable<Cell> GetCellsIntersectsApprox(Vector3 min, Vector3 max) => Underlying.GetCellsIntersectsApprox(min, max).Where(containsFunc);
        #endregion
    }
}
