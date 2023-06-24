using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sylves
{
    /// <summary>
    /// Filters the cells in the the grid to a customizable subset.
    /// 
    /// Filtered cells will not be returned by GetCells, TryMove, etc. Passing them as inputs
    /// is undefined.
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

        public override bool IsCellInGrid(Cell cell) => Underlying.IsCellInGrid(cell) && containsFunc(cell);
        #endregion

        #region Topology

        public override bool TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection) => Underlying.TryMove(cell, dir, out dest, out inverseDir, out connection) && containsFunc(dest);

        public override bool TryMoveByOffset(Cell startCell, Vector3Int startOffset, Vector3Int destOffset, CellRotation startRotation, out Cell destCell, out CellRotation destRotation) => Underlying.TryMoveByOffset(startCell, startOffset, destOffset, startRotation, out destCell, out destRotation) && containsFunc(destCell);

        public override bool ParallelTransport(IGrid aGrid, Cell aSrcCell, Cell aDestCell, Cell srcCell, CellRotation startRotation, out Cell destCell, out CellRotation destRotation)
        {
            return Underlying.ParallelTransport(aGrid, aSrcCell, aDestCell, srcCell, startRotation, out destCell, out destRotation) && containsFunc(destCell);
        }

        #endregion

        #region Bounds
        public override IEnumerable<Cell> GetCellsInBounds(IBound bound) => Underlying.GetCellsInBounds(bound).Where(containsFunc);
        public override bool IsCellInBound(Cell cell, IBound bound) => Underlying.IsCellInBound(cell, bound) && containsFunc(cell);
        #endregion

        #region Query
        public override bool FindCell(Vector3 position, out Cell cell) => Underlying.FindCell(position, out cell) && containsFunc(cell);

        public override bool FindCell(
            Matrix4x4 matrix,
            out Cell cell,
            out CellRotation rotation) => Underlying.FindCell(matrix, out cell, out rotation) && containsFunc(cell);

        public override IEnumerable<Cell> GetCellsIntersectsApprox(Vector3 min, Vector3 max) => Underlying.GetCellsIntersectsApprox(min, max).Where(containsFunc);

        public override IEnumerable<RaycastInfo> Raycast(Vector3 origin, Vector3 direction, float maxDistance = float.PositiveInfinity)
        {
            return Underlying.Raycast(origin, direction, maxDistance).Where(info =>containsFunc(info.cell));
        }
        #endregion

        #region Symmetry

        public override bool TryApplySymmetry(GridSymmetry s, Cell src, out Cell dest, out CellRotation r) => Underlying.TryApplySymmetry(s, src, out dest, out r) && containsFunc(dest);
        #endregion
    }
}
