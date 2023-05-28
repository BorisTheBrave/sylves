using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if UNITY
using UnityEngine;
#endif

namespace Sylves
{

    /// <summary>
    /// An infinite planar grid. The plane is split into overlapping chunks, which each has a grid associated.
    /// This class requires you to compute intra-chunk neighbours yourself, which is often tricky.
    /// Use PlanarLazyMeshGrid to have that done automatically.
    /// </summary>
    // Implementation based on PeriodPlanarMeshGrid
    public abstract class PlanarLazyGrid : IGrid
    {
        private Vector2 strideX;
        private Vector2 strideY;
        private Vector2 aabbBottomLeft;
        private Vector2 aabbSize;
        private SquareBound bound;
        private IEnumerable<ICellType> cellTypes;
        private ICachePolicy cachePolicy;
        // Stores a grid per chunk. The cells are all (x, 0, 0), and moves are relative so, you must offset co-ordinates to work with it.
        // The positions of cells on the other hand, do not need offsetting
        private IDictionary<Cell, MeshGrid> meshGrids;
        private AabbChunks aabbChunks;

        // Clone constructor. Clones share the same cache!
        protected PlanarLazyGrid(PlanarLazyGrid original, SquareBound bound)
        {
            strideX = original.strideX;
            strideY = original.strideY;
            aabbBottomLeft = original.aabbBottomLeft;
            aabbSize = original.aabbSize;
            cellTypes = original.cellTypes;
            cachePolicy = original.cachePolicy;
            meshGrids = original.meshGrids;
            aabbChunks = original.aabbChunks;

            this.bound = bound;
        }

        /// <summary>
        /// Constructs a planar lazy grid that calls getMeshData to fill a chunked plane with a mesh per chunk.
        /// </summary>
        /// <param name="strideX">The step from one chunk to the next.</param>
        /// <param name="strideY">The step from one chunk to the next.</param>
        /// <param name="aabbBottomLeft">The bottom left point of the central chunk. This should bound getMeshData(new Vector2(0, 0))</param>
        /// <param name="aabbSize">The size of each chunk. This should bound getMeshData(new Vector2(0, 0))</param>
        /// <param name="bound">Bounds which chunks are generated.</param>
        /// <param name="cellTypes">What should the response of GetCellType </param>
        /// <param name="cachePolicy">Configures how to store the cahced meshes.</param>
        public PlanarLazyGrid(Vector2 strideX, Vector2 strideY, Vector2 aabbBottomLeft, Vector2 aabbSize, SquareBound bound = null, IEnumerable<ICellType> cellTypes = null, ICachePolicy cachePolicy = null)
        {
            Setup(strideX, strideY, aabbBottomLeft, aabbSize, bound, cellTypes, cachePolicy);
        }

        // You must call setup if using this constructor.
        protected PlanarLazyGrid()
        {

        }

        private static Vector2 ToVector2(Vector3 v) => new Vector2(v.x, v.y);

        protected void Setup(Vector2 strideX, Vector2 strideY, Vector2 aabbBottomLeft, Vector2 aabbSize, SquareBound bound = null, IEnumerable<ICellType> cellTypes = null, ICachePolicy cachePolicy = null)
        {
            this.strideX = strideX;
            this.strideY = strideY;
            this.aabbBottomLeft = aabbBottomLeft;
            this.aabbSize = aabbSize;
            this.bound = bound;
            this.cellTypes = cellTypes;
            this.cachePolicy = cachePolicy ?? CachePolicy.Always;
            meshGrids = this.cachePolicy.GetDictionary<MeshGrid>(this);
            aabbChunks = new AabbChunks(strideX, strideY, aabbBottomLeft, aabbSize);
        }

        // Returns the chunks that could have adjacencies to the current chunk
        protected virtual IEnumerable<Vector2Int> GetAdjacentChunks(Vector2Int chunk)
        {
            // By default, any two intersecting chunks could have adjacencies.
            return aabbChunks.GetChunkIntersects(chunk);
        }

        // Returns a mesh grid for the given chunk.
        // The mesh grid has additional moves for moving to other chunks.
        // Note that the mesh moves should be relative to the current chunk.
        protected abstract MeshGrid GetMeshGrid(Vector2Int v);

        private MeshGrid GetMeshGridCached(Vector2Int v)
        {
            var cell = new Cell(v.x, v.y);
            if (meshGrids.TryGetValue(cell, out MeshGrid meshGrid))
            {
                return meshGrid;
            }
            return meshGrids[cell] = GetMeshGrid(v);
        }

        protected Vector3 ChunkOffset(Vector2Int chunk)
        {
            var chunkOffset2 = strideX * chunk.x + strideY * chunk.y;
            var chunkOffset = new Vector3(chunkOffset2.x, chunkOffset2.y, 0);
            return chunkOffset;
        }

        protected static (Cell meshCell, Vector2Int chunk) Split(Cell cell)
        {
            return (new Cell(cell.x, 0, 0), new Vector2Int(cell.y, cell.z));
        }

        protected static Cell Combine(Cell meshCell, Vector2Int chunk)
        {
            return new Cell(meshCell.x, chunk.x, chunk.y);
        }
        protected static Vector3Int Promote(Vector2Int chunk)
        {
            return new Vector3Int(0, chunk.x, chunk.y);
        }
        private void CheckBounded()
        {
            if (bound == null)
            {
                throw new GridInfiniteException();
            }
        }

        public Vector2 StrideX => strideX;
        public Vector2 StrideY => strideY;
        public Vector2 AabbBottomLeft => aabbBottomLeft;
        public Vector2 AabbSize => aabbSize;

        #region Basics

        public virtual bool Is2d => true;

        public virtual bool Is3d => false;

        public virtual bool IsPlanar => true;

        public virtual bool IsRepeating => false;

        public virtual bool IsOrientable => true;

        public virtual bool IsFinite => bound != null;

        public virtual bool IsSingleCellType => false;

        public virtual int CoordinateDimension => 3;

        public virtual IEnumerable<ICellType> GetCellTypes() => cellTypes ?? throw new Exception("Unknown cell types");

        #endregion

        #region Relatives

        public abstract IGrid Unbounded { get; }

        public IGrid Unwrapped => this;


        public IDualMapping GetDual()
        {
            throw new Exception();
        }
        #endregion

        #region Cell info

        public IEnumerable<Cell> GetCells()
        {
            CheckBounded();
            return GetCellsInBounds(bound);
        }

        public ICellType GetCellType(Cell cell)
        {
            var (meshCell, chunk) = Split(cell);
            return GetMeshGridCached(chunk).GetCellType(meshCell);
        }

        public bool IsCellInGrid(Cell cell) => IsCellInBound(cell, bound);
        #endregion

        #region Topology

        public bool TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection)
        {
            var (meshCell, chunk) = Split(cell);
            if (GetMeshGridCached(chunk).TryMove(meshCell, dir, out dest, out inverseDir, out connection))
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

        public IEnumerable<CellDir> GetCellDirs(Cell cell)
        {
            var (meshCell, chunk) = Split(cell);
            return GetMeshGridCached(chunk).GetCellDirs(meshCell);
        }
        public IEnumerable<CellCorner> GetCellCorners(Cell cell)
        {
            var (meshCell, chunk) = Split(cell);
            return GetMeshGridCached(chunk).GetCellCorners(meshCell);
        }

        public IEnumerable<(Cell, CellDir)> FindBasicPath(Cell startCell, Cell destCell) => throw new NotImplementedException();

        #endregion

        #region Index
        public int IndexCount
        {
            get
            {
                throw new GridInfiniteException();
            }
        }

        public int GetIndex(Cell cell)
        {
            throw new GridInfiniteException();

        }

        public Cell GetCellByIndex(int index)
        {
            throw new GridInfiniteException();
        }
        #endregion


        #region Bounds
        public IBound GetBound() => bound;

        public IBound GetBound(IEnumerable<Cell> cells)
        {
            var min = cells.Select(x => Split(x).chunk).Aggregate(Vector2Int.Min);
            var max = cells.Select(x => Split(x).chunk).Aggregate(Vector2Int.Max);
            return new SquareBound(min, max + Vector2Int.one);
        }

        public abstract IGrid BoundBy(IBound bound);

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
            foreach (var chunk in (SquareBound)bound)
            {
                foreach (var meshCell in GetMeshGridCached(new Vector2Int(chunk.x, chunk.y)).GetCells())
                {
                    yield return Combine(meshCell, new Vector2Int(chunk.x, chunk.y));
                }
            }
        }

        public bool IsCellInBound(Cell cell, IBound bound)
        {
            var (meshCell, chunk) = Split(cell);
            return (bound == null || ((SquareBound)bound).Contains(new Cell(chunk.x, chunk.y)))
                && GetMeshGridCached(chunk).IsCellInGrid(meshCell);
        }
        #endregion


        #region Position
        public Vector3 GetCellCenter(Cell cell)
        {
            var (meshCell, chunk) = Split(cell);
            return GetMeshGridCached(chunk).GetCellCenter(meshCell);
        }

        public Vector3 GetCellCorner(Cell cell, CellCorner cellCorner)
        {

            var (centerCell, chunk) = Split(cell);
            return GetMeshGridCached(chunk).GetCellCorner(centerCell, cellCorner);
        }

        public TRS GetTRS(Cell cell)
        {
            var (meshCell, chunk) = Split(cell);
            return GetMeshGridCached(chunk).GetTRS(meshCell);
        }

        #endregion

        #region Shape
        public Deformation GetDeformation(Cell cell)
        {
            var (meshCell, chunk) = Split(cell);
            return GetMeshGridCached(chunk).GetDeformation(meshCell);
        }

        public void GetPolygon(Cell cell, out Vector3[] vertices, out Matrix4x4 transform)
        {
            var (meshCell, chunk) = Split(cell);
            GetMeshGridCached(chunk).GetPolygon(meshCell, out vertices, out transform);
        }

        public IEnumerable<(Vector3, Vector3, Vector3, CellDir)> GetTriangleMesh(Cell cell)
        {
            throw new Grid2dException();
        }

        public void GetMeshData(Cell cell, out MeshData meshData, out Matrix4x4 transform)
        {
            throw new Grid2dException();
        }
        #endregion

        #region Query
        public bool FindCell(Vector3 position, out Cell cell)
        {
            var pos2 = new Vector2(position.x, position.y);
            foreach (var chunk in aabbChunks.GetChunkIntersects(pos2, pos2))
            {
                var meshGrid = GetMeshGridCached(chunk);
                if (meshGrid.FindCell(position, out cell))
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
            var meshGrid = GetMeshGridCached(chunk);
            return meshGrid.FindCell(matrix, out var _, out rotation);
        }

        public IEnumerable<Cell> GetCellsIntersectsApprox(Vector3 min, Vector3 max)
        {
            var min2 = new Vector2(min.x, min.y);
            var max2 = new Vector2(max.x, max.y);
            foreach (var chunk in aabbChunks.GetChunkIntersects(min2, max2))
            {
                foreach (var meshCell in GetMeshGridCached(chunk).GetCellsIntersectsApprox(min, max))
                {
                    yield return Combine(meshCell, chunk);
                }
            }
        }

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
                foreach (var raycastInfo in GetMeshGridCached(chunk).Raycast(origin, direction, maxDistance))
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
