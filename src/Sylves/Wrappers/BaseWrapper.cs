using System;
using System.Collections.Generic;
using System.Text;

namespace Sylves
{
    /// <summary>
    /// Abstract class for creating wrapper grids.
    /// Wrappers defer most methods to an underlying grid.
    /// </summary>
    public abstract class BaseWrapper : IGrid
    {
        private readonly IGrid underlying;

        public BaseWrapper(IGrid underlying)
        {
            this.underlying = underlying;
        }

        protected abstract IGrid Rebind(IGrid underlying);


        #region Basics

        public bool Is2D => underlying.Is2D;

        public bool Is3D => underlying.Is3D;

        public bool IsPlanar => underlying.IsPlanar;

        public bool IsRepeating => underlying.IsRepeating;

        public bool IsOrientable => underlying.IsOrientable;

        public bool IsFinite => underlying.IsFinite;

        public bool IsSingleCellType => underlying.IsSingleCellType;

        public IEnumerable<ICellType> GetCellTypes() => underlying.GetCellTypes();

        #endregion

        #region Relatives

        public IGrid Unbounded => Rebind(underlying.Unbounded);
        public IGrid Unwrapped => underlying.Unwrapped;
        public IGrid Underlying => underlying;
        #endregion

        #region Cell info

        public IEnumerable<Cell> GetCells() => underlying.GetCells();

        public ICellType GetCellType(Cell cell) => underlying.GetCellType(cell);
        #endregion

        #region Topology

        public bool TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection) => underlying.TryMove(cell, dir, out dest, out inverseDir, out connection);

        public bool TryMoveByOffset(Cell startCell, Vector3Int startOffset, Vector3Int destOffset, CellRotation startRotation, out Cell destCell, out CellRotation destRotation) => underlying.TryMoveByOffset(startCell, startOffset, destOffset, startRotation, out destCell, out destRotation);

        public IEnumerable<CellDir> GetCellDirs(Cell cell) => underlying.GetCellDirs(cell);

        #endregion

        #region Index
        public int IndexCount => underlying.IndexCount;

        public int GetIndex(Cell cell) => underlying.GetIndex(cell);

        public Cell GetCellByIndex(int index) => underlying.GetCellByIndex(index);
        #endregion

        #region Bounds
        public IBound GetBound(IEnumerable<Cell> cells) => underlying.GetBound(cells);

        public IGrid BoundBy(IBound bound) => Rebind(underlying.BoundBy(bound));

        public IBound IntersectBounds(IBound bound, IBound other) => underlying.IntersectBounds(bound, other);
        public IBound UnionBounds(IBound bound, IBound other) => underlying.UnionBounds(bound, other);
        public IEnumerable<Cell> GetCellsInBounds(IBound boud) => underlying.GetCellsInBounds(boud);
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
