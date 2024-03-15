using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Sylves
{
    /// <summary>
    /// Represents a planar grid of axis aligned 2d rectangles, which are
    /// each placed on at regular intervals, called strides.
    /// This grid is mostly for internal uses such as with NestedGrid, and is not very useful as
    /// many methods are not implemented or don't make sense.
    /// </summary>
    internal class AabbGrid : IGrid
    {
        private static readonly ICellType[] cellTypes = { SquareCellType.Instance };

        AabbChunks chunks;
        SquareBound bound;

        public AabbGrid(AabbChunks chunks, SquareBound bound)
        {
            this.chunks = chunks;
            this.bound = bound;
        }

        private void CheckBounded()
        {
            if (bound == null)
            {
                throw new GridInfiniteException();
            }
        }

        private static Vector2Int ToVector2Int(Cell cell)
        {
            if (cell.z != 0)
            {
                throw new Exception("SquareGrid only has cells with z = 0");
            }
            return new Vector2Int(cell.x, cell.y);
        }
        private static Cell FromVector2Int(Vector2Int v)
        {
            return new Cell(v.x, v.y);
        }

        #region Basics
        public bool Is2d => true;

        public bool Is3d => false;

        public bool IsPlanar => true;

        public bool IsRepeating => true;

        public bool IsOrientable => true;

        public bool IsFinite => bound != null;

        public bool IsSingleCellType => true;

        public int CoordinateDimension => 2;

        public IEnumerable<ICellType> GetCellTypes() => cellTypes;
        #endregion

        #region Relatives
        public IGrid Unbounded => new AabbGrid(chunks, null);

        public IGrid Unwrapped => this;

        public virtual IDualMapping GetDual()
        {
            throw new NotSupportedException();
        }

        #endregion

        #region Cell info

        public IEnumerable<Cell> GetCells()
        {
            CheckBounded();
            return GetCellsInBounds(bound);
        }

        public ICellType GetCellType(Cell cell) => SquareCellType.Instance;

        public bool IsCellInGrid(Cell cell)
        {
            if (cell.z != 0)
                return false;
            return IsCellInBound(cell, bound);
        }
        #endregion

        #region Topology

        public bool TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection) => throw new NotSupportedException();

        public bool TryMoveByOffset(Cell startCell, Vector3Int startOffset, Vector3Int destOffset, CellRotation startRotation, out Cell destCell, out CellRotation destRotation) => throw new NotSupportedException();

        public bool ParallelTransport(IGrid aGrid, Cell aSrcCell, Cell aDestCell, Cell srcCell, CellRotation startRotation, out Cell destCell, out CellRotation destRotation) => throw new NotSupportedException();

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
        public IBound GetBound() => bound;

        public IBound GetBound(IEnumerable<Cell> cells)
        {
            // As SquareGrid
            var enumerator = cells.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                throw new Exception($"Enumerator empty");
            }
            var min = ToVector2Int(enumerator.Current);
            var max = min;
            while (enumerator.MoveNext())
            {
                var current = ToVector2Int(enumerator.Current);
                min = Vector2Int.Min(min, current);
                max = Vector2Int.Max(max, current);
            }
            return new SquareBound(min, max + Vector2Int.one);
        }

        public IGrid BoundBy(IBound bound)
        {
            return new AabbGrid(chunks, (SquareBound)IntersectBounds(this.bound, bound));
        }

        public IBound IntersectBounds(IBound bound, IBound other)
        {
            if (bound == null) return other;
            if (other == null) return bound;
            return ((SquareBound)bound).Intersect((SquareBound)other);
        }

        public IBound UnionBounds(IBound bound, IBound other)
        {
            if (bound == null) return null;
            if (other == null) return null;
            return ((SquareBound)bound).Union((SquareBound)other);
        }

        public IEnumerable<Cell> GetCellsInBounds(IBound bound)
        {
            if (bound == null) throw new GridInfiniteException();
            return (SquareBound)bound;
        }

        public bool IsCellInBound(Cell cell, IBound bound) => bound is SquareBound sb ? sb.Contains(cell) : true;
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

        public IEnumerable<Cell> GetCellsIntersectsApprox(Vector3 min, Vector3 max)
        {
            return chunks.GetChunkIntersects(VectorUtils.ToVector2(min), VectorUtils.ToVector2(max), bound).Select(FromVector2Int);
        }

        public IEnumerable<RaycastInfo> Raycast(Vector3 origin, Vector3 direction, float maxDistance = float.PositiveInfinity)
        {
            return chunks.Raycast(VectorUtils.ToVector2(origin), VectorUtils.ToVector2(direction), maxDistance, bound);
        }
        #endregion

        #region Symmetry
        public GridSymmetry FindGridSymmetry(ISet<Cell> src, ISet<Cell> dest, Cell srcCell, CellRotation cellRotation) => throw new NotImplementedException();

        public bool TryApplySymmetry(GridSymmetry s, IBound srcBound, out IBound destBound) => throw new NotImplementedException();

        public bool TryApplySymmetry(GridSymmetry s, Cell src, out Cell dest, out CellRotation r) => throw new NotImplementedException();
        #endregion

    }
}
