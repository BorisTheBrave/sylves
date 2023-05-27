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
    /// An infinite planar grid. The plane is split into overlapping chunks,
    /// and each chunk defines a mesh which are converted to cells like a MeshGrid,
    /// then stitched together.
    /// </summary>
    // Implementation very heavily based on PeriodPlanarMeshGrid
    public class PlanarLazyMeshGrid : IGrid
    {
        private Func<Vector2Int, MeshData> getMeshData;
        private Vector2 strideX;
        private Vector2 strideY;
        private Vector2 aabbBottomLeft;
        private Vector2 aabbSize;
        private MeshGridOptions meshGridOptions;
        private SquareBound bound;
        private IEnumerable<ICellType> cellTypes;
        private ICachePolicy cachePolicy;
        // Stores the mesh data, and also the parsed edges and cells of that mesh
        private IDictionary<Cell, (MeshData, DataDrivenData, EdgeStore)> meshDatas;
        // Stores a grid per chunk. The cells are all (x, 0, 0), and moves are relative so, you must offset co-ordinates to work with it.
        // The positions of cells on the other hand, do not need offsetting
        private IDictionary<Cell, MeshGrid> meshGrids;
        private AabbChunks aabbChunks;

        // Clone constructor. Clones share the same cache!
        protected PlanarLazyMeshGrid(PlanarLazyMeshGrid original, SquareBound bound)
        {
            getMeshData = original.getMeshData;
            strideX = original.strideX;
            strideY = original.strideY;
            aabbBottomLeft = original.aabbBottomLeft;
            aabbSize = original.aabbSize;
            meshGridOptions = original.meshGridOptions;
            cellTypes = original.cellTypes;
            cachePolicy = original.cachePolicy;
            meshDatas = original.meshDatas;
            meshGrids = original.meshGrids;
            aabbChunks = original.aabbChunks;

            this.bound = bound;
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
        public PlanarLazyMeshGrid(Func<Vector2Int, MeshData> getMeshData, Vector2 strideX, Vector2 strideY, Vector2 aabbBottomLeft, Vector2 aabbSize, MeshGridOptions meshGridOptions = null, SquareBound bound = null, IEnumerable<ICellType> cellTypes = null, ICachePolicy cachePolicy = null)
        {
            Setup(getMeshData, strideX, strideY, aabbBottomLeft, aabbSize, meshGridOptions, bound, cellTypes, cachePolicy);
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
        public PlanarLazyMeshGrid(Func<Cell, MeshData> getMeshData, HexGrid chunkGrid, float margin = 0.0f, MeshGridOptions meshGridOptions = null, SquareBound bound = null, IEnumerable<ICellType> cellTypes = null, ICachePolicy cachePolicy = null)
        {
            Setup(getMeshData, chunkGrid, margin, meshGridOptions, bound, cellTypes, cachePolicy);
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
        public PlanarLazyMeshGrid(Func<Cell, MeshData> getMeshData, SquareGrid chunkGrid, float margin = 0.0f, MeshGridOptions meshGridOptions = null, SquareBound bound = null, IEnumerable<ICellType> cellTypes = null, ICachePolicy cachePolicy = null)
        {
            Setup(getMeshData, chunkGrid, margin, meshGridOptions, bound, cellTypes, cachePolicy);
        }

        // You must call setup if using this constructor.
        protected PlanarLazyMeshGrid()
        {

        }

        protected void Setup(Func<Cell, MeshData> getMeshData, HexGrid chunkGrid, float margin = 0.0f, MeshGridOptions meshGridOptions = null, SquareBound bound = null, IEnumerable<ICellType> cellTypes = null, ICachePolicy cachePolicy = null)
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
                strideX, strideY, aabbBottomLeft - margin * Vector2.one, aabbSize + 2 * margin * Vector2.one, meshGridOptions, bound, cellTypes, cachePolicy);
        }

        protected void Setup(Func<Cell, MeshData> getMeshData, SquareGrid chunkGrid, float margin = 0.0f, MeshGridOptions meshGridOptions = null, SquareBound bound = null, IEnumerable<ICellType> cellTypes = null, ICachePolicy cachePolicy = null)
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
                strideX, strideY, aabbBottomLeft - margin * Vector2.one, aabbSize + 2 * margin * Vector2.one, meshGridOptions, bound, cellTypes, cachePolicy);
        }

        private static Vector2 ToVector2(Vector3 v) => new Vector2(v.x, v.y);

        protected void Setup(Func<Vector2Int, MeshData> getMeshData, Vector2 strideX, Vector2 strideY, Vector2 aabbBottomLeft, Vector2 aabbSize, MeshGridOptions meshGridOptions = null, SquareBound bound = null, IEnumerable<ICellType> cellTypes = null, ICachePolicy cachePolicy = null)
        {
            this.getMeshData = getMeshData;
            this.strideX = strideX;
            this.strideY = strideY;
            this.aabbBottomLeft = aabbBottomLeft;
            this.aabbSize = aabbSize;
            this.meshGridOptions = meshGridOptions ?? new MeshGridOptions();
            this.bound = bound;
            this.cellTypes = cellTypes;
            this.cachePolicy = cachePolicy ?? CachePolicy.Always;
            meshDatas = this.cachePolicy.GetDictionary<(MeshData, DataDrivenData, EdgeStore)>(this);
            meshGrids = this.cachePolicy.GetDictionary<MeshGrid>(this);
            aabbChunks = new AabbChunks(strideX, strideY, aabbBottomLeft, aabbSize);
        }

        /// <summary>
        /// Returns the mesh data for a chunk, plus also some processed details.
        /// Note that dataDrivenData/edgeStore is relative to the current chunk, so you need to add Promote(chunk) to
        /// the cells to the absolute values.
        /// </summary>
        internal (MeshData meshData, DataDrivenData dataDrivenData, EdgeStore edgeStore) GetMeshDataCached(Vector2Int v)
        {
            var cell = new Cell(v.x, v.y);
            if (meshDatas.TryGetValue(cell, out var x))
            {
                return x;
            }
            var meshData = getMeshData(v);

            if(meshData.subMeshCount > 1)
            {
                throw new Exception("PlanarLazyMeshGrid doesn't support submeshes");
            }

            // TODO: Check bounds are within the chunk

            var dataDrivenData = MeshGridBuilder.Build(meshData, meshGridOptions, out var edgeStore);

            return meshDatas[cell] = (meshData, dataDrivenData, edgeStore);
        }

        // Returns the chungs that could have adjacencies to the current chunk
        protected virtual IEnumerable<Vector2Int> GetAdjacentChunks(Vector2Int chunk)
        {
            // By default, any two intersecting chunks could have adjacencies.
            return aabbChunks.GetChunkIntersects(chunk);
        }

        private MeshGrid GetMeshGrid(Vector2Int v)
        {
            var cell = new Cell(v.x, v.y);
            if (meshGrids.TryGetValue(cell, out MeshGrid meshGrid))
            {
                return meshGrid;
            }

            var (meshData, dataDrivenData, edgeStore) = GetMeshDataCached(v);

            foreach(var chunk in GetAdjacentChunks(v))
            {
                // Skip this chunk as it's already in edgeStore
                if (chunk == v)
                    continue;

                var otherEdges = GetMeshDataCached(chunk).edgeStore.UnmatchedEdges;

                foreach (var edgeTuple in otherEdges)
                {
                    var (v1, v2, c, dir) = edgeTuple;
                    c += Promote(chunk) - Promote(v);
                    // This is nasty, it is *mutating* cached data.
                    edgeStore.MatchEdge(v1, v2, c, dir, dataDrivenData.Moves, clearEdge: false);
                }
            }

            var mg = new MeshGrid(meshData, meshGridOptions, dataDrivenData, true);
            mg.BuildMeshDetails();

            return meshGrids[cell] = mg;
        }

        private Vector3 ChunkOffset(Vector2Int chunk)
        {
            var chunkOffset2 = strideX * chunk.x + strideY * chunk.y;
            var chunkOffset = new Vector3(chunkOffset2.x, chunkOffset2.y, 0);
            return chunkOffset;
        }

        private static (Cell meshCell, Vector2Int chunk) Split(Cell cell)
        {
            return (new Cell(cell.x, 0, 0), new Vector2Int(cell.y, cell.z));
        }

        private static Cell Combine(Cell meshCell, Vector2Int chunk)
        {
            return new Cell(meshCell.x, chunk.x, chunk.y);
        }
        private static Vector3Int Promote(Vector2Int chunk)
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

        public virtual IGrid Unbounded => GetBound() == null ? this : new PlanarLazyMeshGrid(this, null);

        public IGrid Unwrapped => this;


        public IDualMapping GetDual()
        {
            // Gets the dual for a single chunk,
            // and also the mapping of everything that maps *to* it.
            (MeshData meshData, List<((int face, Vector2Int chunk) primal, int primalVert, int dualFace, int dualVert)> mapping) GetDualData(Vector2Int currentChunk)
            {

                var (weldedMeshData, primalFaceMap, ddd) = ConcatChunks(GetAdjacentChunks(currentChunk).ToList());

                var dmb = new DualMeshBuilder(weldedMeshData, ddd);
                var dualMeshData = dmb.DualMeshData;

                // There's too many faces in dualGrid, work out which ones to keep,
                // and re-index them
                var keepFaces = new List<bool>();
                var keptFaceIndices = new List<int>();
                var keptFaceCount = 0;
                foreach (var face in MeshUtils.GetFaces(dualMeshData))
                {
                    var centroid = face.Select(i => dualMeshData.vertices[i]).Aggregate((a, b) => a + b) / face.Count;
                    var isCentral = aabbChunks.GetUniqueChunk(new Vector2(centroid.x, centroid.y)) == currentChunk;
                    if (isCentral)
                    {
                        keepFaces.Add(true);
                        keptFaceIndices.Add(keptFaceCount);
                        keptFaceCount++;
                    }
                    else
                    {
                        keepFaces.Add(false);
                        keptFaceIndices.Add(-1);
                    }
                }

                // Mesh filtered to just kept faces
                var dualMesh = dualMeshData.FaceFilter((f, i) => keepFaces[i]);

                // Convert primal faces back to which chunk they are from,
                // and compress dual faces to just yhose kept
                var mapping = dmb.Mapping
                    .Where(x => keepFaces[x.dualFace])
                    .Select(x => (primalFaceMap[x.primalFace], x.primalVert, keptFaceIndices[x.dualFace], x.dualVert))
                    .ToList();

                return (dualMesh, mapping);
            }

            var dualDataCache = cachePolicy.GetDictionary<(MeshData, List<((int face, Vector2Int chunk) primal, int primalVert, int dualFace, int dualVert)>)>(this);

            (MeshData meshData, List<((int face, Vector2Int chunk) primal, int primalVert, int dualFace, int dualVert)> mapping) GetDualDataCached(Vector2Int currentChunk)
            {
                var cell = new Cell(currentChunk.x, currentChunk.y);
                if (dualDataCache.TryGetValue(cell, out var dd))
                {
                    return dd;
                }
                return dualDataCache[cell] = GetDualData(currentChunk);
            }

            // TODO: These need to be increased in size.
            var dualAabbBottomLeft = aabbBottomLeft;
            var dualAabbSize = aabbSize;
            var dualGrid = new PlanarLazyMeshGrid(c => GetDualDataCached(c).meshData, strideX, strideY, dualAabbBottomLeft, dualAabbSize, meshGridOptions, cachePolicy: cachePolicy);
            // TODO: Bound

            return new DualMapping(this, dualGrid, c => GetDualDataCached(c).mapping, cachePolicy);
        }

        // Returns a single mesh that contains the concat of many chunks,
        // Plus a mapping back to the original cells
        // Plus also computes DataDrivenData, in terms of the new mesh
        // (i.e. ddd = MeshGridBuilder.Build(meshData(meshData))
        private (MeshData meshData, Dictionary<int, (int, Vector2Int)> faceToCell, DataDrivenData ddd) ConcatChunks(IList<Vector2Int> chunks)
        {
            // Like in the constructor, use offset copies of the mesh
            // This can probably be done more efficiently.
            var meshDatas = new List<MeshData>();
            var chunkDdds = new List<DataDrivenData>();
            foreach (var chunk in chunks)
            {
                // GetMeshGrid mutates cached data, so we need to call it even though we don't use the results, ugh
                GetMeshGrid(chunk);
                var mdc = GetMeshDataCached(chunk);
                meshDatas.Add(mdc.meshData);
                chunkDdds.Add(mdc.dataDrivenData);
            }

            // Once we've got all meshes, merge them together and compute the dual
            var mergedMeshData = MeshDataOperations.Concat(meshDatas, out var mergeIndexMap);


            // Build additional data about each face.
            // There's several different conventions here
            // * primal - the face id as it appears in mergedMeshData
            // * relativeOriginalCell - the cell as it appears in chunkDdds
            // * originalCell - the cell as it appears in this.

            var primalFaceMap = new Dictionary<int, (int, Vector2Int)>();
            var originalToNewMap = new Dictionary<Cell, Cell>();
            var cells = new Dictionary<Cell, DataDrivenCellData>();
            var moves = new Dictionary<(Cell, CellDir), (Cell, CellDir, Connection)>();

            // TODO: Can probably avoid this with some rewriting
            var primalFaces = MeshUtils.GetFaces(mergedMeshData).ToList();
            int primalFaceIndex = 0;
            for (var i = 0; i < chunks.Count; i++)
            {
                var chunk = chunks[i];
                var chunkDdd = chunkDdds[i];
                var count = chunkDdd.Cells.Count;

                for (var f = 0; f < count; f++)
                {
                    var relativeOriginalCell = new Cell(f, 0);
                    var originalCell = relativeOriginalCell + Promote(chunk);
                    var newCell = new Cell(primalFaceIndex, 0);

                    primalFaceMap[primalFaceIndex] = (f, chunk);
                    var cellData = chunkDdd.Cells[relativeOriginalCell].Clone() as MeshCellData;
                    cellData.Face = primalFaces[primalFaceIndex];
                    cells[newCell] = cellData;
                    originalToNewMap[originalCell] = newCell;
                    primalFaceIndex++;
                }
            }

            for (var i = 0; i < chunks.Count; i++)
            {
                var chunk = chunks[i];
                var chunkDdd = chunkDdds[i];
                foreach (var move in chunkDdd.Moves)
                {
                    var from = move.Key.Item1 + Promote(chunk);
                    var to = move.Value.Item1 + Promote(chunk);
                    if (!originalToNewMap.ContainsKey(to))
                        continue;
                    if (!originalToNewMap.ContainsKey(from))
                        continue;
                    moves[(originalToNewMap[from], move.Key.Item2)] = 
                        (originalToNewMap[to], move.Value.Item2, move.Value.Item3);
                }
            }


            return (mergedMeshData, primalFaceMap, new DataDrivenData { Moves = moves, Cells = cells});

        }


        private class DualMapping : BasicDualMapping
        {
            private readonly IDictionary<Cell, Dictionary<(Cell, CellCorner), (Cell, Vector2Int, CellCorner)>> toDualCache;
            private readonly IDictionary<Cell, Dictionary<(Cell, CellCorner), (Cell, Vector2Int, CellCorner)>> toBaseCache;
            private readonly PlanarLazyMeshGrid baseGrid;
            private readonly Func<Vector2Int, List<((int face, Vector2Int chunk) primal, int primalVert, int dualFace, int dualVert)>> mappingByDualChunkCached;

            public DualMapping(PlanarLazyMeshGrid baseGrid, PlanarLazyMeshGrid dualGrid, Func<Vector2Int, List<((int face, Vector2Int chunk) primal, int primalVert, int dualFace, int dualVert)>> mappingByDualChunkCached, ICachePolicy cachePolicy) : base(baseGrid, dualGrid)
            {

                toDualCache = cachePolicy.GetDictionary<Dictionary<(Cell, CellCorner), (Cell, Vector2Int, CellCorner)>>(baseGrid);
                toBaseCache = cachePolicy.GetDictionary<Dictionary<(Cell, CellCorner), (Cell, Vector2Int, CellCorner)>>(baseGrid);
                this.baseGrid = baseGrid;
                this.mappingByDualChunkCached = mappingByDualChunkCached;
            }

            public override (Cell dualCell, CellCorner inverseCorner)? ToDualPair(Cell cell, CellCorner corner)
            {
                var (centerCell, chunk) = Split(cell);
                if (!toDualCache.TryGetValue(new Cell(chunk.x, chunk.y), out var toDual))
                {
                    // Compute toDual
                    // Because we only have mappings per-*dual*-chunk, we need to aggregate several mappings

                    /*
                    toDualCache[new Cell(chunk.x, chunk.y)] = toDual = baseGrid.aabbChunks.GetChunkIntersects(min, max)
                        .SelectMany(dualChunk => mappingByDualChunkCached(dualChunk)
                            .Where(x => x.primal.chunk == chunk)
                            .Select(x => (dualChunk, x)))
                        .ToDictionary(x => (new Cell(x.x.primal.face, 0, 0), (CellCorner)x.x.primalVert),
                                      x => (new Cell(x.x.dualFace, 0, 0), x.dualChunk, (CellCorner)x.x.dualVert));
                    */

                    toDualCache[new Cell(chunk.x, chunk.y)] = toDual = new Dictionary<(Cell, CellCorner), (Cell, Vector2Int, CellCorner)>();
                    foreach(var dualChunk in baseGrid.aabbChunks.GetChunkIntersects(chunk))
                    {
                        var mapping = mappingByDualChunkCached(dualChunk);
                        foreach(var x in mapping)
                        {
                            if (x.primal.chunk != chunk)
                                continue;
                            toDual.Add((new Cell(x.primal.face, 0, 0), (CellCorner)x.primalVert),
                                       (new Cell(x.dualFace, 0, 0), dualChunk, (CellCorner)x.dualVert));
                        }
                    }

                }

                if (toDual.TryGetValue((centerCell, corner), out var r))
                {
                    var (dualCell, dualChunk, dualVert) = r;
                    return (Combine(dualCell, dualChunk), dualVert);
                }
                else
                {
                    return null;
                }
            }

            public override (Cell baseCell, CellCorner inverseCorner)? ToBasePair(Cell cell, CellCorner corner)
            {
                var (centerCell, chunk) = Split(cell);
                if (!toBaseCache.TryGetValue(new Cell(chunk.x, chunk.y), out var toBase))
                {
                    // Compute toBase
                    toBaseCache[new Cell(chunk.x, chunk.y)] = toBase = mappingByDualChunkCached(chunk)
                        .ToDictionary(
                            x => (new Cell(x.dualFace, 0, 0), (CellCorner)x.dualVert),
                            x => (new Cell(x.primal.face, 0, 0), x.primal.chunk, (CellCorner)x.primalVert)); ;
                }

                if (toBase.TryGetValue((centerCell, corner), out var r))
                {
                    var (primalCell, primalChunk, primalVert) = r;
                    return (Combine(primalCell, primalChunk), primalVert);
                }
                else
                {
                    return null;
                }
            }
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
            return GetMeshGrid(chunk).GetCellType(meshCell);
        }

        public bool IsCellInGrid(Cell cell) => IsCellInBound(cell, bound);
        #endregion

        #region Topology

        public bool TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection)
        {
            var (meshCell, chunk) = Split(cell);
            if (GetMeshGrid(chunk).TryMove(meshCell, dir, out dest, out inverseDir, out connection))
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
            return GetMeshGrid(chunk).GetCellDirs(meshCell);
        }
        public IEnumerable<CellCorner> GetCellCorners(Cell cell)
        {
            var (meshCell, chunk) = Split(cell);
            return GetMeshGrid(chunk).GetCellCorners(meshCell);
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

        public virtual IGrid BoundBy(IBound bound) => new PlanarLazyMeshGrid(this, (SquareBound)bound);

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
                foreach (var meshCell in GetMeshGrid(new Vector2Int(chunk.x, chunk.y)).GetCells())
                {
                    yield return Combine(meshCell, new Vector2Int(chunk.x, chunk.y));
                }
            }
        }

        public bool IsCellInBound(Cell cell, IBound bound)
        {
            var (meshCell, chunk) = Split(cell);
            return (bound == null || ((SquareBound)bound).Contains(new Cell(chunk.x, chunk.y)))
                && 0 <= meshCell.x && meshCell.x < GetMeshDataCached(chunk).Item2.Cells.Count;
        }
        #endregion


        #region Position
        public Vector3 GetCellCenter(Cell cell)
        {
            var (meshCell, chunk) = Split(cell);
            return GetMeshGrid(chunk).GetCellCenter(meshCell);
        }

        public Vector3 GetCellCorner(Cell cell, CellCorner cellCorner)
        {

            var (centerCell, chunk) = Split(cell);
            return GetMeshGrid(chunk).GetCellCorner(centerCell, cellCorner);
        }

        public TRS GetTRS(Cell cell)
        {
            var (meshCell, chunk) = Split(cell);
            return GetMeshGrid(chunk).GetTRS(meshCell);
        }

        #endregion

        #region Shape
        public Deformation GetDeformation(Cell cell)
        {
            var (meshCell, chunk) = Split(cell);
            return GetMeshGrid(chunk).GetDeformation(meshCell);
        }

        public void GetPolygon(Cell cell, out Vector3[] vertices, out Matrix4x4 transform)
        {
            var (meshCell, chunk) = Split(cell);
            GetMeshGrid(chunk).GetPolygon(meshCell, out vertices, out transform);
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
                var meshGrid = GetMeshGrid(chunk);
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
            var meshGrid = GetMeshGrid(chunk);
            return meshGrid.FindCell(matrix, out var _, out rotation);
        }

        public IEnumerable<Cell> GetCellsIntersectsApprox(Vector3 min, Vector3 max)
        {
            var min2 = new Vector2(min.x, min.y);
            var max2 = new Vector2(max.x, max.y);
            foreach (var chunk in aabbChunks.GetChunkIntersects(min2, max2))
            {
                foreach (var meshCell in GetMeshGrid(chunk).GetCellsIntersectsApprox(min, max))
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
                foreach (var raycastInfo in GetMeshGrid(chunk).Raycast(origin, direction, maxDistance))
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
