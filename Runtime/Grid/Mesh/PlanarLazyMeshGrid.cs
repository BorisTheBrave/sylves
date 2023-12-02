using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Sylves
{

    /// <summary>
    /// An infinite planar grid. It is evaluated lazily by splitting the plane into overlapping period rectangles
    /// which then each has a mesh associated.
    /// The meshes are converted to cells like a MeshGrid, then stitched together.
    /// </summary>
    // Implementation very heavily based on PeriodPlanarMeshGrid
    public class PlanarLazyMeshGrid : PlanarLazyGrid
    {
        private Func<Cell, MeshData> getMeshData;
        private MeshGridOptions meshGridOptions;

        // Stores the mesh data, and also the parsed edges and cells of that mesh
        private IDictionary<Cell, (MeshData, DataDrivenData, EdgeStore)> meshDatas;

        // Clone constructor. Clones share the same cache!
        protected PlanarLazyMeshGrid(PlanarLazyMeshGrid original, SquareBound bound):base(original, bound)
        {
            getMeshData = original.getMeshData;
            meshGridOptions = original.meshGridOptions;
            meshDatas = original.meshDatas;
        }

        /// <summary>
        /// Constructs a planar lazy grid that calls getMeshData to fill a chunked plane with a mesh per chunk.
        /// </summary>
        /// <param name="getMeshData">The function supplying the meshes per chunk.</param>
        /// <param name="strideX">The step from one chunk to the next.</param>
        /// <param name="strideY">The step from one chunk to the next.</param>
        /// <param name="aabbBottomLeft">The bottom left point of the central chunk. This should bound getMeshData(new Vector2(0, 0))</param>
        /// <param name="aabbSize">The size of each chunk. This should bound getMeshData(new Vector2(0, 0))</param>
        /// <param name="meshGridOptions">Options to use when converting meshes to the grid.</param>
        /// <param name="bound">Bounds which chunks are generated.</param>
        /// <param name="cellTypes">What should the response of GetCellType </param>
        /// <param name="cachePolicy">Configures how to store the cahced meshes.</param>
        public PlanarLazyMeshGrid(Func<Vector2Int, MeshData> getMeshData, Vector2 strideX, Vector2 strideY, Vector2 aabbBottomLeft, Vector2 aabbSize, bool translateMeshData=false, MeshGridOptions meshGridOptions = null, SquareBound bound = null, IEnumerable<ICellType> cellTypes = null, ICachePolicy cachePolicy = null)
        {
            Setup(getMeshData, strideX, strideY, aabbBottomLeft, aabbSize, translateMeshData, meshGridOptions, bound, cellTypes, cachePolicy);
        }

        /// <summary>
        /// Constructs a planar lazy grid that calls getMeshData to fill a hex grid with a mesh per cell.
        /// </summary>
        /// <param name="getMeshData">The function supplying the mesh per cell of chunkGrid</param>
        /// <param name="chunkGrid">Each cell of this grid becomes a chunk of the PlanarLazyMeshGrid.</param>
        /// <param name="margin">The output of getMeshData should be fit inside the shape of chunkGrid cell, expanded by margin.</param>
        /// <param name="meshGridOptions">Options to use when converting meshes to the grid.</param>
        /// <param name="bound">Bounds which chunks are generated.</param>
        /// <param name="cellTypes">What should the response of GetCellType </param>
        /// <param name="cachePolicy">Configures how to store the cahced meshes.</param>
        public PlanarLazyMeshGrid(Func<Cell, MeshData> getMeshData, HexGrid chunkGrid, float margin = 0.0f, bool translateMeshData = false, MeshGridOptions meshGridOptions = null, SquareBound bound = null, IEnumerable<ICellType> cellTypes = null, ICachePolicy cachePolicy = null)
        {
            Setup(getMeshData, chunkGrid, margin, translateMeshData, meshGridOptions, bound, cellTypes, cachePolicy);
        }

        /// <summary>
        /// Constructs a planar lazy grid that calls getMeshData to fill a square grid with a mesh per cell.
        /// </summary>
        /// <param name="getMeshData">The function supplying the mesh per cell of chunkGrid</param>
        /// <param name="chunkGrid">Each cell of this grid becomes a chunk of the PlanarLazyMeshGrid.</param>
        /// <param name="margin">The output of getMeshData should be fit inside the shape of chunkGrid cell, expanded by margin.</param>
        /// <param name="meshGridOptions">Options to use when converting meshes to the grid.</param>
        /// <param name="bound">Bounds which chunks are generated.</param>
        /// <param name="cellTypes">What should the response of GetCellType </param>
        /// <param name="cachePolicy">Configures how to store the cahced meshes.</param>
        public PlanarLazyMeshGrid(Func<Cell, MeshData> getMeshData, SquareGrid chunkGrid, float margin = 0.0f, bool translateMeshData = false, MeshGridOptions meshGridOptions = null, SquareBound bound = null, IEnumerable<ICellType> cellTypes = null, ICachePolicy cachePolicy = null)
        {
            Setup(getMeshData, chunkGrid, margin, translateMeshData, meshGridOptions, bound, cellTypes, cachePolicy);
        }

        // You must call setup if using this constructor.
        protected PlanarLazyMeshGrid()
        {

        }

        protected void Setup(Func<Cell, MeshData> getMeshData, HexGrid chunkGrid, float margin = 0.0f, bool translateMeshData = false, MeshGridOptions meshGridOptions = null, SquareBound bound = null, IEnumerable<ICellType> cellTypes = null, ICachePolicy cachePolicy = null)
        {
            // Work out the dimensions of the chunk grid
            var strideX = ToVector2(chunkGrid.GetCellCenter(new Cell(1, 0, -1)));
            var strideY = ToVector2(chunkGrid.GetCellCenter(new Cell(0, 1, -1)));

            var polygon = chunkGrid.GetPolygon(new Cell()).Select(ToVector2);
            var aabbBottomLeft = polygon.Aggregate(Vector2.Min);
            var aabbTopRight = polygon.Aggregate(Vector2.Max);
            var aabbSize = aabbTopRight - aabbBottomLeft;

            Setup(
                chunk => getMeshData(new Cell(chunk.x, chunk.y, -chunk.x - chunk.y)),
                strideX, strideY, aabbBottomLeft - margin * Vector2.one, aabbSize + 2 * margin * Vector2.one, translateMeshData, meshGridOptions, bound, cellTypes, cachePolicy);
        }

        protected void Setup(Func<Cell, MeshData> getMeshData, SquareGrid chunkGrid, float margin = 0.0f, bool translateMeshData = false, MeshGridOptions meshGridOptions = null, SquareBound bound = null, IEnumerable<ICellType> cellTypes = null, ICachePolicy cachePolicy = null)
        {
            // Work out the dimensions of the chunk grid
            var strideX = ToVector2(chunkGrid.GetCellCenter(new Cell(1, 0)));
            var strideY = ToVector2(chunkGrid.GetCellCenter(new Cell(0, 1)));

            var polygon = chunkGrid.GetPolygon(new Cell()).Select(ToVector2);
            var aabbBottomLeft = polygon.Aggregate(Vector2.Min);
            var aabbTopRight = polygon.Aggregate(Vector2.Max);
            var aabbSize = aabbTopRight - aabbBottomLeft;

            Setup(
                chunk => getMeshData(new Cell(chunk.x, chunk.y)),
                strideX, strideY, aabbBottomLeft - margin * Vector2.one, aabbSize + 2 * margin * Vector2.one, translateMeshData, meshGridOptions, bound, cellTypes, cachePolicy);
        }

        private static Vector2 ToVector2(Vector3 v) => new Vector2(v.x, v.y);

        protected void Setup(Func<Vector2Int, MeshData> getMeshData, Vector2 strideX, Vector2 strideY, Vector2 aabbBottomLeft, Vector2 aabbSize, bool translateMeshData = false, MeshGridOptions meshGridOptions = null, SquareBound bound = null, IEnumerable<ICellType> cellTypes = null, ICachePolicy cachePolicy = null)
        {
            this.getMeshData = chunkCell => getMeshData(new Vector2Int(chunkCell.x, chunkCell.y));
            this.meshGridOptions = meshGridOptions ?? new MeshGridOptions();
            meshDatas = (cachePolicy ?? Sylves.CachePolicy.Always).GetDictionary<(MeshData, DataDrivenData, EdgeStore)>(this);

            base.Setup(strideX, strideY, aabbBottomLeft, aabbSize, translateMeshData, bound, cellTypes, cachePolicy);
        }

        // Expose some methods for use with relax modifier
        internal (Cell childCell, Cell chunkCell) InternalSplit(Cell cell) => Split(cell);
        internal Cell InternalCombine(Cell childCell, Cell chunkCell) => Combine(childCell, chunkCell);


        /// <summary>
        /// Returns the mesh data for a chunk, plus also some processed details.
        /// Note that dataDrivenData/edgeStore is relative to the current chunk, so you need to add Promote(chunk) to
        /// the cells to the absolute values.
        /// </summary>
        internal (MeshData meshData, DataDrivenData dataDrivenData, EdgeStore edgeStore) GetMeshDataCached(Cell chunkCell)
        {
            if (meshDatas.TryGetValue(chunkCell, out var x))
            {
                return x;
            }
            var meshData = getMeshData(chunkCell);

            if(meshData.subMeshCount > 1)
            {
                throw new Exception("PlanarLazyMeshGrid doesn't support submeshes");
            }

            // TODO: Check bounds are within the chunk

            var dataDrivenData = MeshGridBuilder.Build(meshData, meshGridOptions, out var edgeStore);

            Cell Map(Cell childCell) => Combine(childCell, chunkCell);

            // Convert edge data and moves to use the global cell identifiers
            edgeStore.MapCells(Map);
            dataDrivenData.Moves = MapMoves(dataDrivenData.Moves, Map);

            return meshDatas[chunkCell] = (meshData, dataDrivenData, edgeStore);
        }

        private static IDictionary<(Cell, CellDir), (Cell, CellDir, Connection)> MapMoves(IDictionary<(Cell, CellDir), (Cell, CellDir, Connection)> moves, Func<Cell, Cell> f)
        {
            return moves.ToDictionary(kv => (f(kv.Key.Item1), kv.Key.Item2), kv => (f(kv.Value.Item1), kv.Value.Item2, kv.Value.Item3)); ;
        }


        // Returns a mesh grid for the given chunk.
        // This is simply the meshData provided, plus additional 
        // moves based on matching edges from the meshes of other chunks.
        protected override IGrid GetChildGrid(Cell chunkCell)
        {
            var (meshData, dataDrivenData, edgeStore) = GetMeshDataCached(chunkCell);

            foreach (var otherChunkCell in GetAdjacentChunks(chunkCell))
            {
                // Skip this chunk as it's already in edgeStore
                if (otherChunkCell == chunkCell)
                    continue;

                var otherEdges = GetMeshDataCached(otherChunkCell).edgeStore.UnmatchedEdges;

                foreach (var edgeTuple in otherEdges)
                {
                    var (v1, v2, c, dir) = edgeTuple;
                    // In this case, it's ok to subtract cells from each other, as we know what grid we're using (AabbGrid)
                    // which supports this.
                    var chunkDiff = new Cell(otherChunkCell.x - chunkCell.x, otherChunkCell.y - chunkCell.y, otherChunkCell.z - chunkCell.z);
                    if (TranslateMeshData)
                    {
                        var t = ChunkOffset(chunkDiff);
                        v1 += t;
                        v2 += t;
                    }
                    // This is nasty, it is *mutating* cached data.
                    edgeStore.MatchEdge(v1, v2, c, dir, dataDrivenData.Moves, clearEdge: false);
                }
            }

            var mg = new MeshGrid(meshData, meshGridOptions, dataDrivenData, true);
            mg.BuildMeshDetails();

            return mg;
        }

        #region Relatives

        public override IGrid Unbounded => GetBound() == null ? this : new PlanarLazyMeshGrid(this, null);
        #endregion

        #region Bounds
        public override IGrid BoundBy(IBound bound) => new PlanarLazyMeshGrid(this, (SquareBound)bound);
        #endregion



        #region Topology

        public override bool TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection)
        {
            var (_, chunkCell) = Split(cell);
            return GetChildGridCached(chunkCell).TryMove(cell, dir, out dest, out inverseDir, out connection);
        }
        #endregion
    }
}
