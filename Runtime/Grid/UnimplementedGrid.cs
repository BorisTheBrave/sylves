using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sylves
{
    // This class is useless, it is just a handy copy paste template for new grids
    internal class UnimplementedGrid : IGrid
    {
        #region Basics
        public bool Is2d => throw new NotImplementedException();

        public bool Is3d => throw new NotImplementedException();

        public bool IsPlanar => throw new NotImplementedException();

        public bool IsRepeating => throw new NotImplementedException();

        public bool IsOrientable => throw new NotImplementedException();

        public bool IsFinite => throw new NotImplementedException();

        public bool IsSingleCellType => throw new NotImplementedException();

        public int CoordinateDimension => throw new NotImplementedException();

        public IEnumerable<ICellType> GetCellTypes() => throw new NotImplementedException();
        #endregion

        #region Relatives
        public IGrid Unbounded => throw new NotImplementedException();

        public IGrid Unwrapped => throw new NotImplementedException();

        public virtual IDualMapping GetDual() => throw new NotImplementedException();

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

        public IEnumerable<CellCorner> GetCellCorners(Cell cell) => throw new NotImplementedException();

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

        public Vector3 GetCellCorner(Cell cell, CellCorner cellCorner) => throw new NotImplementedException();

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
}
