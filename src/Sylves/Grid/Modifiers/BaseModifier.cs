using System;
using System.Collections.Generic;
using System.Text;

namespace Sylves
{
    /// <summary>
    /// Abstract class for creating wrapper grids.
    /// Wrappers defer most methods to an underlying grid.
    /// </summary>
    public abstract class BaseModifier : IGrid
    {
        private readonly IGrid underlying;

        public BaseModifier(IGrid underlying)
        {
            this.underlying = underlying;
        }

        /// <summary>
        /// Creates a new grid applying the current wrapper to a new underlying.
        /// </summary>
        protected abstract IGrid Rebind(IGrid underlying);


        #region Basics

        public virtual bool Is2D => underlying.Is2D;

        public virtual bool Is3D => underlying.Is3D;

        public virtual bool IsPlanar => underlying.IsPlanar;

        public virtual bool IsRepeating => underlying.IsRepeating;

        public virtual bool IsOrientable => underlying.IsOrientable;

        public virtual bool IsFinite => underlying.IsFinite;

        public virtual bool IsSingleCellType => underlying.IsSingleCellType;

        public virtual IEnumerable<ICellType> GetCellTypes() => underlying.GetCellTypes();

        #endregion

        #region Relatives

        public virtual IGrid Unbounded => Rebind(underlying.Unbounded);
        public virtual IGrid Unwrapped => underlying.Unwrapped;
        public virtual IGrid Underlying => underlying;
        #endregion

        #region Cell info

        public virtual IEnumerable<Cell> GetCells() => underlying.GetCells();

        public virtual ICellType GetCellType(Cell cell) => underlying.GetCellType(cell);
        #endregion

        #region Topology

        public virtual bool TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection) => underlying.TryMove(cell, dir, out dest, out inverseDir, out connection);

        public virtual bool TryMoveByOffset(Cell startCell, Vector3Int startOffset, Vector3Int destOffset, CellRotation startRotation, out Cell destCell, out CellRotation destRotation) => underlying.TryMoveByOffset(startCell, startOffset, destOffset, startRotation, out destCell, out destRotation);

        public virtual IEnumerable<CellDir> GetCellDirs(Cell cell) => underlying.GetCellDirs(cell);
        public virtual IEnumerable<(Cell, CellDir)> FindBasicPath(Cell startCell, Cell destCell) => underlying.FindBasicPath(startCell, destCell);

        #endregion

        #region Index
        public virtual int IndexCount => underlying.IndexCount;

        public virtual int GetIndex(Cell cell) => underlying.GetIndex(cell);

        public virtual Cell GetCellByIndex(int index) => underlying.GetCellByIndex(index);
        #endregion

        #region Bounds
        public virtual IBound GetBound(IEnumerable<Cell> cells) => underlying.GetBound(cells);

        public virtual IGrid BoundBy(IBound bound) => Rebind(underlying.BoundBy(bound));

        public virtual IBound IntersectBounds(IBound bound, IBound other) => underlying.IntersectBounds(bound, other);
        public virtual IBound UnionBounds(IBound bound, IBound other) => underlying.UnionBounds(bound, other);
        public virtual IEnumerable<Cell> GetCellsInBounds(IBound bound) => underlying.GetCellsInBounds(bound);
        #endregion

        #region Position
        public virtual Vector3 GetCellCenter(Cell cell) => underlying.GetCellCenter(cell);

        public virtual TRS GetTRS(Cell cell) => underlying.GetTRS(cell);

        public virtual Deformation GetDeformation(Cell cell) => underlying.GetDeformation(cell);
        #endregion

        #region Query
        public virtual bool FindCell(Vector3 position, out Cell cell) => underlying.FindCell(position, out cell);

        public virtual bool FindCell(
            Matrix4x4 matrix,
            out Cell cell,
            out CellRotation rotation) => underlying.FindCell(matrix, out cell, out rotation);

        public virtual IEnumerable<Cell> GetCellsIntersectsApprox(Vector3 min, Vector3 max) => underlying.GetCellsIntersectsApprox(min, max);
        #endregion


    }
}
