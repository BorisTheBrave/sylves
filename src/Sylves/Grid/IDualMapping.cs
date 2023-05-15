using System;
using System.Collections.Generic;
using System.Text;
#if UNITY
using UnityEngine;
#endif

namespace Sylves
{
    public interface IDualMapping
    {
        IGrid BaseGrid { get; }
        IGrid DualGrid { get; }

        (Cell cell, CellCorner inverseCorner)? ToDualPair(Cell cell, CellCorner corner);

        (Cell cell, CellCorner inverseCorner)? ToBasePair(Cell cell, CellCorner corner);
    }

    // TODO: An implementation that works for any 2d grid
    internal class Default2dDualMapping : IDualMapping, IGrid
    {
        private readonly IGrid baseGrid;

        public Default2dDualMapping(IGrid baseGrid)
        {
            if(!baseGrid.Is2d)
            {
                throw new ArgumentException("baseGrid should be 2d");
            }
            this.baseGrid = baseGrid;

        }

        #region DualMapping
        public IGrid BaseGrid => baseGrid;

        public IGrid DualGrid => throw new NotImplementedException();

        public (Cell cell, CellCorner inverseCorner)? ToBasePair(Cell cell, CellCorner corner)
        {
            throw new NotImplementedException();
        }

        public (Cell cell, CellCorner inverseCorner)? ToDualPair(Cell cell, CellCorner corner)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Basics
        public bool Is2d => throw new NotImplementedException();

        public bool Is3d => throw new NotImplementedException();

        public bool IsPlanar => throw new NotImplementedException();

        public bool IsRepeating => throw new NotImplementedException();

        public bool IsOrientable => throw new NotImplementedException();

        public bool IsFinite => throw new NotImplementedException();

        public bool IsSingleCellType => throw new NotImplementedException();

        public IEnumerable<ICellType> GetCellTypes() => throw new NotImplementedException();
        #endregion

        #region Relatives
        public IGrid Unbounded => throw new NotImplementedException();

        public IGrid Unwrapped => throw new NotImplementedException();

        public virtual IDualMapping GetDual() => this.Reversed();

        #endregion

        #region Cell info

        public IEnumerable<Cell> GetCells() => throw new NotImplementedException();

        public ICellType GetCellType(Cell cell) => throw new NotImplementedException();

        public bool IsCellInGrid(Cell cell) => throw new NotImplementedException();
        #endregion

        #region Topology

        public bool TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection) => throw new NotImplementedException();

        public bool TryMoveByOffset(Cell startCell, Vector3Int startOffset, Vector3Int destOffset, CellRotation startRotation, out Cell destCell, out CellRotation destRotation) => throw new NotImplementedException();


        public bool ParallelTransport(IGrid aGrid, Cell aSrcCell, Cell aDestCell, Cell srcCell, CellRotation startRotation, out Cell destCell, out CellRotation destRotation) => throw new NotImplementedException();


        public IEnumerable<CellDir> GetCellDirs(Cell cell) => throw new NotImplementedException();

        public IEnumerable<(Cell, CellDir)> FindBasicPath(Cell startCell, Cell destCell) => throw new NotImplementedException();

        #endregion

        #region Index
        public int IndexCount => throw new NotImplementedException();

        public int GetIndex(Cell cell) => throw new NotImplementedException();

        public Cell GetCellByIndex(int index) => throw new NotImplementedException();
        #endregion

        #region Bounds
        public IBound GetBound() => throw new NotImplementedException();

        public IBound GetBound(IEnumerable<Cell> cells) => throw new NotImplementedException();

        public IGrid BoundBy(IBound bound) => throw new NotImplementedException();

        public IBound IntersectBounds(IBound bound, IBound other) => throw new NotImplementedException();

        public IBound UnionBounds(IBound bound, IBound other) => throw new NotImplementedException();

        public IEnumerable<Cell> GetCellsInBounds(IBound bound) => throw new NotImplementedException();

        public bool IsCellInBound(Cell cell, IBound bound) => throw new NotImplementedException();
        #endregion

        #region Position
        public Vector3 GetCellCenter(Cell cell) => throw new NotImplementedException();

        public TRS GetTRS(Cell cell) => throw new NotImplementedException();

        #endregion

        #region Shape
        public Deformation GetDeformation(Cell cell) => throw new NotImplementedException();

        public void GetPolygon(Cell cell, out Vector3[] vertices, out Matrix4x4 transform) => throw new NotImplementedException();

        public IEnumerable<(Vector3, Vector3, Vector3, CellDir)> GetTriangleMesh(Cell cell) => throw new NotImplementedException();

        public void GetMeshData(Cell cell, out MeshData meshData, out Matrix4x4 transform) => throw new NotImplementedException();

        #endregion

        #region Query
        public bool FindCell(Vector3 position, out Cell cell) => throw new NotImplementedException();

        public bool FindCell(
            Matrix4x4 matrix,
            out Cell cell,
            out CellRotation rotation) => throw new NotImplementedException();

        public IEnumerable<Cell> GetCellsIntersectsApprox(Vector3 min, Vector3 max) => throw new NotImplementedException();

        public IEnumerable<RaycastInfo> Raycast(Vector3 origin, Vector3 direction, float maxDistance = float.PositiveInfinity) => throw new NotImplementedException();
        #endregion

        #region Symmetry
        public GridSymmetry FindGridSymmetry(ISet<Cell> src, ISet<Cell> dest, Cell srcCell, CellRotation cellRotation) => throw new NotImplementedException();

        public bool TryApplySymmetry(GridSymmetry s, IBound srcBound, out IBound destBound) => throw new NotImplementedException();

        public bool TryApplySymmetry(GridSymmetry s, Cell src, out Cell dest, out CellRotation r) => throw new NotImplementedException();
        #endregion
    }

    internal abstract class BasicDualMapping : IDualMapping
    {
        public BasicDualMapping(IGrid baseGrid, IGrid dualGrid)
        {
            BaseGrid = baseGrid;
            DualGrid = dualGrid;
        }

        public IGrid BaseGrid { get; }
        public IGrid DualGrid { get; }

        public abstract (Cell cell, CellCorner inverseCorner)? ToBasePair(Cell cell, CellCorner corner);
        public abstract (Cell cell, CellCorner inverseCorner)? ToDualPair(Cell cell, CellCorner corner);

    }

    public static class DualMappingExtensions
    {
        public static Cell? ToDualCell(this IDualMapping dm, Cell cell, CellCorner corner) => dm.ToDualPair(cell, corner)?.cell;
        public static Cell? ToBaseCell(this IDualMapping dm, Cell cell, CellCorner corner) => dm.ToBasePair(cell, corner)?.cell;

        public static IEnumerable<(CellCorner corner, Cell cell, CellCorner inverseCorner)> DualNeighbours(this IDualMapping dm, Cell cell)
        {
            // TODO: Perhaps have this overridable as many grids will have swifter methods
            var cellType = dm.BaseGrid.GetCellType(cell);
            foreach(var corner in cellType.GetCellCorners())
            {
                var t = dm.ToDualPair(cell, corner);
                if(t != null)
                {
                    yield return (corner, t.Value.cell, t.Value.inverseCorner);
                }
            }
        }

        // TODO: Be less lazy
        public static IEnumerable<(CellCorner corner, Cell cell, CellCorner inverseCorner)> BaseNeighbours(this IDualMapping dm, Cell cell) => dm.Reversed().DualNeighbours(cell);


        public static IDualMapping Reversed(this IDualMapping dualMapping)
        {
            if (dualMapping is ReversedDualMapping rdm)
            {
                return rdm.Underlying;
            }
            else
            {
                return new ReversedDualMapping(dualMapping);
            }
        }

        private class ReversedDualMapping : IDualMapping
        {
            readonly IDualMapping underlying;

            public ReversedDualMapping(IDualMapping underlying)
            {
                this.underlying = underlying;
            }

            public IDualMapping Underlying => underlying;

            public IGrid BaseGrid => underlying.DualGrid;

            public IGrid DualGrid => underlying.BaseGrid;

            public (Cell cell, CellCorner inverseCorner)? ToBasePair(Cell cell, CellCorner corner) => underlying.ToDualPair(cell, corner);

            public (Cell cell, CellCorner inverseCorner)? ToDualPair(Cell cell, CellCorner corner) => underlying.ToBasePair(cell, corner);
        }

    }
}
