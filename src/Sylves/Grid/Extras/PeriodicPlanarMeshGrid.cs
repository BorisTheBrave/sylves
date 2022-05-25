using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY
using UnityEngine;
#endif


namespace Sylves
{
    /// <summary>
    /// A grid made of a repeating pattern of a given mesh, that must be planar in the XY plane.
    /// The repeats are given by translation in two independent axes, strideX and strideY .
    /// </summary>
    internal class PeriodicPlanarMeshGrid : IGrid
    {
        AabbChunks aabbChunks;
        DataDrivenGrid centerGrid;
        private readonly Vector2 strideX;
        private readonly Vector2 strideY;

        public PeriodicPlanarMeshGrid(MeshData meshData, Vector2 strideX, Vector2 strideY)
        {
            if (meshData.subMeshCount != 1)
                throw new Exception();

            // Analyse the original mesh
            var meshMin = meshData.vertices.Aggregate(Vector3.Min);
            var meshMax = meshData.vertices.Aggregate(Vector3.Max);
            var meshMin2 = new Vector2(meshMin.x, meshMin.y);
            var meshMax2 = new Vector2(meshMax.x, meshMax.y);
            var meshSize = meshMax2 - meshMin2;

            var dataDrivenData = MeshGridBuilder.Build(meshData, new MeshGridOptions(), out var edgeStore);

            // Use offset copies of the mesh to establish extra entries in the moves dictionary
            aabbChunks = new AabbChunks(strideX, strideY, meshMin2, meshSize);
            var originalEdges = edgeStore.UnmatchedEdges.ToList();
            foreach(var chunk in aabbChunks.GetChunkIntersects(meshMin2, meshMax2))
            {
                // Skip this chunk as it's already in edgeStore
                if (chunk == Vector2Int.zero)
                    continue;

                var chunkOffset2 = strideX * chunk.x + strideY * chunk.y;
                var chunkOffset = new Vector3(chunkOffset2.x, chunkOffset2.y, 0);

                foreach (var edgeTuple in originalEdges)
                {
                    var (v1, v2, cell, dir) = edgeTuple;
                    v1 += chunkOffset;
                    v2 += chunkOffset;
                    cell += new Vector3Int(0, chunk.x, chunk.y);
                    edgeStore.MatchEdge(v1, v2, cell, dir, dataDrivenData.Moves);
                }
            }

            centerGrid = new MeshGrid(dataDrivenData, true);
            this.strideX = strideX;
            this.strideY = strideY;
        }

        private Vector3 ChunkOffset(Vector2Int chunk)
        {
            var chunkOffset2 = strideX * chunk.x + strideY * chunk.y;
            var chunkOffset = new Vector3(chunkOffset2.x, chunkOffset2.y, 0);
            return chunkOffset;
        }

        private (Cell centerCell, Vector2Int chunk) Split(Cell cell)
        {
            return (new Cell(cell.x, 0, 0), new Vector2Int(cell.y, cell.z));
        }

        private Cell Combine(Cell centerCell, Vector2Int chunk)
        {
            return new Cell(centerCell.x, chunk.x, chunk.y);
        }

        private Vector3Int Promote(Vector2Int chunk)
        {
            return new Vector3Int(0, chunk.x, chunk.y);
        }



        #region Basics

        public bool Is2D => true;

        public bool Is3D => false;

        public bool IsPlanar => true;

        public bool IsRepeating => true;

        public bool IsOrientable => true;

        public bool IsFinite => false;

        public bool IsSingleCellType => centerGrid.IsSingleCellType;

        public IEnumerable<ICellType> GetCellTypes() => centerGrid.GetCellTypes();

        #endregion

        #region Relatives

        public IGrid Unbounded => this;

        public IGrid Unwrapped => this;

        #endregion

        #region Cell info

        public IEnumerable<Cell> GetCells() => throw new NotSupportedException();

        public ICellType GetCellType(Cell cell) => centerGrid.GetCellType(Split(cell).centerCell);

        public bool IsCellInGrid(Cell cell) => true;
        #endregion

        #region Topology

        public bool TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection)
        {
            var (centerCell, chunk) = Split(cell);
            if(centerGrid.TryMove(centerCell, dir, out dest, out inverseDir, out connection))
            {
                dest += Promote(chunk);
                return true;
            }
            return false;
        }

        public bool TryMoveByOffset(Cell startCell, Vector3Int startOffset, Vector3Int destOffset, CellRotation startRotation, out Cell destCell, out CellRotation destRotation)
        {
            throw new NotImplementedException();
        }

        public bool ParallelTransport(IGrid aGrid, Cell aSrcCell, Cell aDestCell, Cell srcCell, CellRotation startRotation, out Cell destCell, out CellRotation destRotation)
        {
            return DefaultGridImpl.ParallelTransport(aGrid, aSrcCell, aDestCell, this, srcCell, startRotation, out destCell, out destRotation);
        }

        public IEnumerable<CellDir> GetCellDirs(Cell cell) => centerGrid.GetCellDirs(Split(cell).centerCell);

        public IEnumerable<(Cell, CellDir)> FindBasicPath(Cell startCell, Cell destCell) => throw new NotImplementedException();

        #endregion

        #region Index
        public int IndexCount => throw new GridInfiniteException();

        public int GetIndex(Cell cell) => throw new GridInfiniteException();

        public Cell GetCellByIndex(int index) => throw new GridInfiniteException();
        #endregion

        #region Bounds
        public IBound GetBound() => throw new NotSupportedException();

        public IBound GetBound(IEnumerable<Cell> cells) => throw new NotSupportedException();

        public IGrid BoundBy(IBound bound) => throw new NotSupportedException();

        public IBound IntersectBounds(IBound bound, IBound other) => throw new NotSupportedException();
        public IBound UnionBounds(IBound bound, IBound other) => throw new NotSupportedException();
        public IEnumerable<Cell> GetCellsInBounds(IBound bound) => throw new NotSupportedException();

        public bool IsCellInBound(Cell cell, IBound bound) => throw new NotSupportedException();
        #endregion

        #region Position
        public Vector3 GetCellCenter(Cell cell)
        {
            var (centerCell, chunk) = Split(cell);
            return centerGrid.GetCellCenter(centerCell) + ChunkOffset(chunk);
        }

        public TRS GetTRS(Cell cell)
        {
            var (centerCell, chunk) = Split(cell);
            var trs = centerGrid.GetTRS(centerCell);
            return new TRS(trs.Position + ChunkOffset(chunk), trs.Rotation, trs.Scale);
        }

        #endregion

        #region Shape
        public Deformation GetDeformation(Cell cell)
        {
            var (centerCell, chunk) = Split(cell);
            var deformation = centerGrid.GetDeformation(centerCell);
            return Matrix4x4.Translate(ChunkOffset(chunk)) * deformation;

        }

        public void GetPolygon(Cell cell, out Vector3[] vertices, out Matrix4x4 transform) => throw new NotImplementedException();
        #endregion

        #region Query
        public bool FindCell(Vector3 position, out Cell cell)
        {
            var pos2 = new Vector2(position.x, position.y);
            foreach(var chunk in aabbChunks.GetChunkIntersects(pos2, pos2))
            {
                var p = position - ChunkOffset(chunk);
                if(centerGrid.FindCell(p, out cell))
                {
                    cell += Promote(chunk);
                    return true;
                }
            }
            cell = default;
            return false;
        }

        public bool FindCell(
            Matrix4x4 matrix,
            out Cell cell,
            out CellRotation rotation)
        {
            var position = matrix.MultiplyPoint(Vector3.zero);
            if (!FindCell(position, out cell))
            {
                rotation = default;
                return false;
            }
            var (_, chunk) = Split(cell);

            return centerGrid.FindCell(Matrix4x4.Translate(-ChunkOffset(chunk)) * matrix, out var _, out rotation);
        }

        public IEnumerable<Cell> GetCellsIntersectsApprox(Vector3 min, Vector3 max)
        {
            var min2 = new Vector2(min.x, min.y);
            var max2 = new Vector2(max.x, max.y);
            foreach(var chunk in aabbChunks.GetChunkIntersects(min2, max2))
            {
                var chunkOffset = ChunkOffset(chunk);
                foreach(var centerCell in centerGrid.GetCellsIntersectsApprox(min - chunkOffset, max - chunkOffset))
                {
                    yield return centerCell + Promote(chunk);
                }
            }
        }

        // TODO: FindCells, Raycast, GetPath
        #endregion

        #region Symmetry

        public GridSymmetry FindGridSymmetry(ISet<Cell> src, ISet<Cell> dest, Cell srcCell, CellRotation cellRotation)
        {
            throw new NotImplementedException();
        }

        public bool TryApplySymmetry(GridSymmetry s, IBound srcBound, out IBound destBound)
        {
            throw new NotImplementedException();
        }

        public bool TryApplySymmetry(GridSymmetry s, Cell src, out Cell dest, out CellRotation r)
        {
            throw new NotImplementedException();
        }
        #endregion


    }
}
