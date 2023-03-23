﻿using System;
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
    public class PeriodicPlanarMeshGrid : IGrid
    {
        private readonly AabbChunks aabbChunks;
        private readonly DataDrivenGrid centerGrid;
        private readonly Vector2 strideX;
        private readonly Vector2 strideY;
        private SquareBound bound;

        private PeriodicPlanarMeshGrid(AabbChunks aabbChunks, DataDrivenGrid centerGrid, Vector2 strideX, Vector2 strideY, SquareBound bound)
        {
            this.aabbChunks = aabbChunks;
            this.centerGrid = centerGrid;
            this.strideX = strideX;
            this.strideY = strideY;
            this.bound = bound;
        }

        public PeriodicPlanarMeshGrid(MeshData meshData, Vector2 strideX, Vector2 strideY)
        {
            if (meshData.subMeshCount != 1)
                throw new Exception($"Expected subMeshCount of 1");

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

            var mg = new MeshGrid(meshData, new MeshGridOptions { }, dataDrivenData, true);
            mg.BuildMeshDetails();
            centerGrid = mg;
            this.strideX = strideX;
            this.strideY = strideY;
        }

        private void CheckBounded()
        {
            if (bound == null)
            {
                throw new GridInfiniteException();
            }
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

        /// <inheritdoc />
        public bool Is2d => true;

        /// <inheritdoc />
        public bool Is3d => false;

        /// <inheritdoc />
        public bool IsPlanar => true;

        /// <inheritdoc />
        public bool IsRepeating => true;

        /// <inheritdoc />
        public bool IsOrientable => true;

        /// <inheritdoc />
        public bool IsFinite => false;

        /// <inheritdoc />
        public bool IsSingleCellType => centerGrid.IsSingleCellType;

        public IEnumerable<ICellType> GetCellTypes() => centerGrid.GetCellTypes();

        #endregion

        #region Relatives

        /// <inheritdoc />
        public IGrid Unbounded => new PeriodicPlanarMeshGrid(aabbChunks, centerGrid, strideX, strideY, null);

        /// <inheritdoc />
        public IGrid Unwrapped => this;

        #endregion

        #region Cell info

        /// <inheritdoc />
        public IEnumerable<Cell> GetCells()
        {
            CheckBounded();
            return GetCellsInBounds(bound);
        }

        /// <inheritdoc />
        public ICellType GetCellType(Cell cell) => centerGrid.GetCellType(Split(cell).centerCell);

        /// <inheritdoc />
        public bool IsCellInGrid(Cell cell) => IsCellInBound(cell, bound);
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
        /// <inheritdoc />
        public int IndexCount
        {
            get
            {
                CheckBounded();
                return centerGrid.IndexCount * this.bound.IndexCount;
            }
        }

        /// <inheritdoc />
        public int GetIndex(Cell cell)
        {
            CheckBounded();
            var (centerCell, chunk) = Split(cell);
            return centerGrid.GetIndex(centerCell) + centerGrid.IndexCount * this.bound.GetIndex(chunk);

        }

        /// <inheritdoc />
        public Cell GetCellByIndex(int index)
        {
            var centerCell = centerGrid.GetCellByIndex(index % centerGrid.IndexCount);
            var chunk = this.bound.GetCellByIndex(index / centerGrid.IndexCount);
            return Combine(centerCell, chunk);
        }
        #endregion

        #region Bounds
        /// <inheritdoc />
        public IBound GetBound() => bound;

        /// <inheritdoc />
        public IBound GetBound(IEnumerable<Cell> cells)
        {
            var min = cells.Select(x => Split(x).chunk).Aggregate(Vector2Int.Min);
            var max = cells.Select(x => Split(x).chunk).Aggregate(Vector2Int.Max);
            return new SquareBound(min, max + Vector2Int.one);
        }

        /// <inheritdoc />
        public IGrid BoundBy(IBound bound) => new PeriodicPlanarMeshGrid(aabbChunks, centerGrid, strideX, strideY, (SquareBound)IntersectBounds(this.bound, bound));

        /// <inheritdoc />
        public IBound IntersectBounds(IBound bound, IBound other)
        {
            if (bound == null) return other;
            if (other == null) return bound;
            return ((SquareBound)bound).Intersect((SquareBound)other);
        }
        /// <inheritdoc />
        public IBound UnionBounds(IBound bound, IBound other)
        {
            if (bound == null) return null;
            if (other == null) return null;
            return ((SquareBound)bound).Union((SquareBound)other);
        }
        /// <inheritdoc />
        public IEnumerable<Cell> GetCellsInBounds(IBound bound)
        {
            foreach (var chunk in (SquareBound)bound)
            {
                foreach (var centerCell in centerGrid.GetCells())
                {
                    yield return Combine(centerCell, new Vector2Int(chunk.x, chunk.y));
                }
            }
        }

        /// <inheritdoc />
        public bool IsCellInBound(Cell cell, IBound bound)
        {
            var (centerCell, chunk) = Split(cell);
            return (bound == null || ((SquareBound)bound).Contains(new Cell(chunk.x, chunk.y))) && 0 <= centerCell.x && centerCell.x < centerGrid.IndexCount;
        }
        #endregion

        #region Position
        /// <inheritdoc />
        public Vector3 GetCellCenter(Cell cell)
        {
            var (centerCell, chunk) = Split(cell);
            return centerGrid.GetCellCenter(centerCell) + ChunkOffset(chunk);
        }

        /// <inheritdoc />
        public TRS GetTRS(Cell cell)
        {
            var (centerCell, chunk) = Split(cell);
            var trs = centerGrid.GetTRS(centerCell);
            return new TRS(trs.Position + ChunkOffset(chunk), trs.Rotation, trs.Scale);
        }

        #endregion

        #region Shape
        /// <inheritdoc />
        public Deformation GetDeformation(Cell cell)
        {
            var (centerCell, chunk) = Split(cell);
            var deformation = centerGrid.GetDeformation(centerCell);
            return Matrix4x4.Translate(ChunkOffset(chunk)) * deformation;
        }

        /// <inheritdoc />
        public void GetPolygon(Cell cell, out Vector3[] vertices, out Matrix4x4 transform)
        {
            var (centerCell, chunk) = Split(cell);
            centerGrid.GetPolygon(centerCell, out vertices, out transform);
            transform = Matrix4x4.Translate(ChunkOffset(chunk)) * transform;
        }

        /// <inheritdoc />
        public IEnumerable<(Vector3, Vector3, Vector3, CellDir)> GetTriangleMesh(Cell cell)
        {
            throw new Grid2dException();
        }

        /// <inheritdoc />
        public void GetMeshData(Cell cell, out MeshData meshData, out Matrix4x4 transform)
        {
            throw new Grid2dException();
        }
        #endregion

        #region Query
        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public IEnumerable<RaycastInfo> Raycast(Vector3 origin, Vector3 direction, float maxDistance = float.PositiveInfinity)
        {
            var origin2 = new Vector2(origin.x, origin.y);
            var direction2 = new Vector2(direction.x, direction.y);
            var queuedRaycastInfos = new List<RaycastInfo>();
            foreach (var chunkRaycastInfo in aabbChunks.Raycast(origin2, direction2, maxDistance))
            {
                // Resort and drain queue
                queuedRaycastInfos.Sort((x, y) => -x.distance.CompareTo(y.distance));
                while (queuedRaycastInfos.Count > 0 && queuedRaycastInfos[queuedRaycastInfos.Count - 1].distance < chunkRaycastInfo.distance)
                {
                    var ri = queuedRaycastInfos[queuedRaycastInfos.Count - 1];
                    queuedRaycastInfos.RemoveAt(queuedRaycastInfos.Count - 1);
                    yield return ri;
                }

                var chunk = new Vector2Int(chunkRaycastInfo.cell.x, chunkRaycastInfo.cell.y);
                var chunkOffset = ChunkOffset(chunk);
                foreach(var raycastInfo in centerGrid.Raycast(origin - chunkOffset, direction, maxDistance))
                {
                    queuedRaycastInfos.Add(new RaycastInfo
                    {
                        cell = raycastInfo.cell + Promote(chunk),
                        cellDir = raycastInfo.cellDir,
                        distance = raycastInfo.distance,
                        point = raycastInfo.point + chunkOffset,
                    });
                }
            }

            // Final drain
            queuedRaycastInfos.Sort((x, y) => -x.distance.CompareTo(y.distance));
            for (var i = queuedRaycastInfos.Count - 1; i >= 0; i--)
            {
                var ri = queuedRaycastInfos[i];
                yield return ri;
            }
        }
        #endregion

        #region Symmetry

        /// <inheritdoc />
        public GridSymmetry FindGridSymmetry(ISet<Cell> src, ISet<Cell> dest, Cell srcCell, CellRotation cellRotation)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool TryApplySymmetry(GridSymmetry s, IBound srcBound, out IBound destBound)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool TryApplySymmetry(GridSymmetry s, Cell src, out Cell dest, out CellRotation r)
        {
            throw new NotImplementedException();
        }
        #endregion


    }
}
