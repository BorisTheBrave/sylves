using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sylves
{
    // Works by splitting up the plane into hexes
    // Each hex is joined with its 6 neighbours and relaxed to make overlapping patches
    // Each point is a blend of the three nearest patches
    /// <summary>
    /// Applies relaxation to an infinite 2d plane, similar to MeshDataOperations.Relax.
    /// </summary>
    public class RelaxModifier : PlanarLazyGrid
    {
        private readonly IGrid underlying;
        private readonly float chunkSize;
        private readonly float weldTolerance;
        private readonly int relaxIterations;

        // Fast path optimization
        // If the underlying mesh shares the same structure as the relax modifier,
        // we can save our selves some effort.
        // This is mostly for use with Townscaper.
        // Should this be factored into a separate class?
        private readonly bool passThroughMesh;
        private readonly bool translateUnrelaxed;

        // Description of the chunking used.
        // It matches the chunk grid, but uses 3 co-ordinates, not two.
        // This means we have to convert to/from hexes in a few places - ew
        private readonly HexGrid hexGrid;

        // Caches

        // Unrelaxed chunks are just the raw mesh data taken from underlying
        // We also store the mapping from mesh faces back to underlying cells
        private readonly IDictionary<Cell, (MeshData, BiMap<int, Cell>)> unrelaxedChunksByHex;

        // Relaxed patches are the result of concatting several unrelaxed chunks together,
        // then relaxing them
        // Also stored is for each neighbour chunk, where the vertices of the unrelated mesh data can be found in relaxed patch.
        private readonly IDictionary<Cell, (MeshData, Dictionary<Cell, int[]>)> relaxedPatchesByHex;

        /// <summary>
        /// Cache for the Split operation.
        /// This can be slow depending on the underlying grid.
        /// </summary>
        private readonly IDictionary<Cell, (Cell, Cell)> splitCache;

        public RelaxModifier(
            IGrid underlying,
            float chunkSize = 10,
            float weldTolerance = 1e-7f,
            int relaxIterations = 3,
            ICachePolicy cachePolicy = null)
            :base()
        {
            if (!underlying.IsPlanar)
            {
                throw new NotImplementedException("RelaxModifier only supports planar grids");
            }

            cachePolicy = cachePolicy ?? Sylves.CachePolicy.Always;

            hexGrid = new HexGrid(chunkSize);

            unrelaxedChunksByHex = cachePolicy.GetDictionary<(MeshData, BiMap<int, Cell>)>(hexGrid);
            relaxedPatchesByHex = cachePolicy.GetDictionary<(MeshData, Dictionary<Cell, int[]>)>(hexGrid);
            this.underlying = underlying;
            this.chunkSize = chunkSize;
            this.weldTolerance = weldTolerance;
            this.relaxIterations = relaxIterations;

            // Infer celltypes from underlying if possible
            IEnumerable<ICellType> cellTypes = null;
            try
            {
                cellTypes = underlying.GetCellTypes();
            } catch (Exception)
            {

            }

            // Infer bound from underlying if possible
            // Perhaps this should be done lazily?
            SquareBound bound = null;
            if(underlying.IsFinite)
            {
                var chunkCells = underlying.GetCells()
                    .Select(underlying.GetCellCenter)
                    .Select(hexGrid.FindCell)
                    .OfType<Cell>();
                var chunkBound = (HexBound)hexGrid.GetBound(chunkCells);
                bound = new SquareBound(new Vector2Int(chunkBound.min.x, chunkBound.min.y), new Vector2Int(chunkBound.max.x, chunkBound.max.y));

            }

            var margin = chunkSize / 2;

            Setup(hexGrid, margin, bound: bound, cellTypes: cellTypes);

            if (underlying is PlanarLazyMeshGrid pg)
            {
                // Compare dimensions.
                // This isn't strictly accurate (pg could have same aabb dimensions but not fit in a hex), but meh.
                var a = (StrideX, StrideY, AabbBottomLeft, AabbSize);
                var b = (pg.StrideX, pg.StrideY, pg.AabbBottomLeft - margin * Vector2.one, pg.AabbSize + 2 * margin * Vector2.one);
                passThroughMesh = a == b;
                translateUnrelaxed = passThroughMesh && pg.TranslateMeshData;
            }
            else
            {
                splitCache = cachePolicy.GetDictionary<(Cell, Cell)>(underlying);
            }
        }

        private void Setup(HexGrid chunkGrid, float margin = 0.0f, SquareBound bound = null, IEnumerable<ICellType> cellTypes = null, ICachePolicy cachePolicy = null)
        {
            // Work out the dimensions of the chunk grid
            var strideX = ToVector2(chunkGrid.GetCellCenter(new Cell(1, 0, -1)));
            var strideY = ToVector2(chunkGrid.GetCellCenter(new Cell(0, 1, -1)));

            var polygon = chunkGrid.GetPolygon(new Cell()).Select(ToVector2);
            var aabbBottomLeft = polygon.Aggregate(Vector2.Min);
            var aabbTopRight = polygon.Aggregate(Vector2.Max);
            var aabbSize = aabbTopRight - aabbBottomLeft;

            base.Setup(strideX, strideY, aabbBottomLeft - margin * Vector2.one, aabbSize + 2 * margin * Vector2.one, false, bound, cellTypes, cachePolicy);
        }

        // Clone constructor. Clones share the same cache!
        private RelaxModifier(RelaxModifier original, SquareBound bound)
            :base(original, bound)
        {
            underlying = original.underlying;
            chunkSize = original.chunkSize;
            weldTolerance = original.weldTolerance;
            relaxIterations = original.relaxIterations;
            passThroughMesh = original.passThroughMesh;
            translateUnrelaxed = original.translateUnrelaxed;
            hexGrid = original.hexGrid;
            unrelaxedChunksByHex = original.unrelaxedChunksByHex;
            relaxedPatchesByHex = original.relaxedPatchesByHex;
            splitCache = original.splitCache;
        }

        protected override (Cell childCell, Cell chunkCell) Split(Cell cell)
        {
            if (passThroughMesh)
            {
                var planarLazyMeshGrid = underlying as PlanarLazyMeshGrid;
                return planarLazyMeshGrid.InternalSplit(cell);
            }
            else
            {
                if(splitCache.TryGetValue(cell, out var split))
                {
                    return split;
                }

                // Find the chunk this cell is in
                var hex = hexGrid.FindCell(underlying.GetCellCenter(cell)).Value;
                // Find the face index in that chunk
                var child = GetUnrelaxedChunk(hex).cells[cell];
                return splitCache[cell] = (new Cell(child, 0), HexToChunk(hex));
            }
        }

        protected override Cell Combine(Cell childCell, Cell chunkCell)
        {
            if (passThroughMesh)
            {
                var planarLazyMeshGrid = underlying as PlanarLazyMeshGrid;
                return planarLazyMeshGrid.InternalCombine(childCell, chunkCell);
            }
            else
            {
                return GetUnrelaxedChunk(ChunkToHex(chunkCell)).cells[childCell.x];
            }
        }

        #region Calculations


        private static Vector2 ToVector2(Vector3 v) => new Vector2(v.x, v.y);

        private static Vector3 ToVector3(Vector2 v) => new Vector3(v.x, v.y, 0);

        private static Cell HexToChunk(Cell hex) => new Cell(hex.x, hex.y);
        private static Cell ChunkToHex(Cell chunkCell) => new Cell(chunkCell.x, chunkCell.y, -chunkCell.x - chunkCell.y);


        // We can give tighter bounds here as we know that we're hex based,
        // and also that the margin added to the bounds is irrelevant.
        protected override IEnumerable<Cell> GetAdjacentChunks(Cell chunkCell)
        {
            // Just hard code the hex adjacencies
            yield return new Cell(chunkCell.x - 1, chunkCell.y);
            yield return new Cell(chunkCell.x - 1, chunkCell.y + 1);
            yield return new Cell(chunkCell.x, chunkCell.y - 1);
            yield return new Cell(chunkCell.x, chunkCell.y); // Self
            yield return new Cell(chunkCell.x, chunkCell.y + 1);
            yield return new Cell(chunkCell.x + 1, chunkCell.y - 1);
            yield return new Cell(chunkCell.x + 1, chunkCell.y);
        }

        protected override IGrid GetChildGrid(Cell chunkCell)
        {
            // Unlike PlanarLazyMeshGrid, there's no need to do edge detection here,
            // as TryMove just forwards to underlying
            var meshData = GetRelaxedChunk(ChunkToHex(chunkCell));
            return new MeshGrid(meshData, new MeshGridOptions { Tolerance = weldTolerance });
        }

        // Unrelaxed chunks are just the raw mesh data taken from underlying
        (MeshData meshData, BiMap<int, Cell> cells) GetUnrelaxedChunk(Cell hex)
        {
            if (unrelaxedChunksByHex.ContainsKey(hex))
                return unrelaxedChunksByHex[hex];

            if (passThroughMesh)
            {
                // Underlying chunks match relax chunks, so we can safely just
                // pass the underlying chunk through here.
                var planarLazyMeshGrid = underlying as PlanarLazyMeshGrid;
                var chunk = HexToChunk(hex);
                var meshData = planarLazyMeshGrid.GetMeshDataCached(chunk).meshData;
                return unrelaxedChunksByHex[hex] = (meshData, null);
            }
            else
            {
                // Get cells near the chunk
                var min = AabbBottomLeft + hex.x * StrideX + hex.y * StrideY;
                var max = min + AabbSize;
                var unfilteredCells = underlying.GetCellsIntersectsApprox(ToVector3(min), ToVector3(max));

                // Filter to precisely cells in this chunk
                var cells = unfilteredCells
                    .Where(c => hexGrid.FindCell(underlying.GetCellCenter(c)) == hex)
                    .ToArray();

                // To mesh data
                var meshData = underlying.ToMeshData(cells);
                var map = new BiMap<int, Cell>(cells.Select((x, i) => (i, x)));
                return unrelaxedChunksByHex[hex] = (meshData, map);
            }
        }

        // Relaxed patches are the result of concatting several unrelaxed chunks together,
        // then relaxing them
        (MeshData meshData, Dictionary<Cell, int[]> indexMaps) GetRelaxedPatch(Cell hex)
        {
            if (relaxedPatchesByHex.ContainsKey(hex))
                return relaxedPatchesByHex[hex];

            var nearbyChunks = new[] { hex }.Concat(hexGrid.GetNeighbours(hex));

            var meshes = nearbyChunks.Select(c =>
            {
                var mesh = GetUnrelaxedChunk(c).meshData;
                var chunkDiff = HexToChunk(new Cell(c.x - hex.x, c.y - hex.y, c.z - hex.z));
                if (translateUnrelaxed)
                {
                    mesh = Matrix4x4.Translate(ChunkOffset(chunkDiff)) * mesh;
                }
                return mesh;
            }).ToList();

            var md = MeshDataOperations.Concat(meshes, out var concatIndexMaps);

            md = md.Weld(out var weldIndexMap, weldTolerance).Relax(relaxIterations);

            // Move from hex local to absolte space
            // Better to do this later, but more fiddly
            if (translateUnrelaxed)
            {
                md = Matrix4x4.Translate(ChunkOffset(HexToChunk(hex))) * md;
            }

            // For each nearby chunk, find the where each vertex corresponds to in the output md.
            var maps = nearbyChunks.Zip(concatIndexMaps, (a, b) => (a, b)).ToDictionary(x => x.a, x => x.b.Select(i => weldIndexMap[i]).ToArray());

            return relaxedPatchesByHex[hex] = (md, maps);
        }

        // The actual mesh data matches the unrelaxed chunk
        // but with position data interpolated from several relaxed patches.
        MeshData GetRelaxedChunk(Cell hex)
        {
            var unrelaxed = GetUnrelaxedChunk(hex).meshData;

            var result = unrelaxed.Clone();
            result.vertices = new Vector3[unrelaxed.vertices.Length];

            for (var i = 0; i < unrelaxed.vertices.Length; i++)
            {
                var v = unrelaxed.vertices[i];
                if(translateUnrelaxed)
                {
                    v += ChunkOffset(HexToChunk(hex));
                }
                var nearbyHexes = NearbyHexes.FindNearbyHexes(v / chunkSize);
                var patch1 = GetRelaxedPatch(nearbyHexes.Hex1);
                var patch2 = GetRelaxedPatch(nearbyHexes.Hex2);
                var patch3 = GetRelaxedPatch(nearbyHexes.Hex3);
                var v1 = patch1.meshData.vertices[patch1.indexMaps[hex][i]];
                var v2 = patch2.meshData.vertices[patch2.indexMaps[hex][i]];
                var v3 = patch3.meshData.vertices[patch3.indexMaps[hex][i]];
                result.vertices[i] = v1 * nearbyHexes.Weight1 +
                    v2 * nearbyHexes.Weight2 +
                    v3 * nearbyHexes.Weight3;
            }
            return result;
        }
        #endregion


        #region Basics

        public override bool Is2d => underlying.Is2d;

        public override bool Is3d => underlying.Is3d;

        public override bool IsPlanar => underlying.IsPlanar;

        public override bool IsRepeating => underlying.IsRepeating;

        public override bool IsOrientable => underlying.IsOrientable;

        public override bool IsFinite => base.IsFinite || underlying.IsFinite;

        public override bool IsSingleCellType => underlying.IsSingleCellType;

        public override int CoordinateDimension => underlying.CoordinateDimension;

        public override IEnumerable<ICellType> GetCellTypes() => underlying.GetCellTypes();

        #endregion

        #region Relatives
        public override IGrid Unbounded => new RelaxModifier(this, null);

        #endregion

        #region Topology
        public override bool TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection)
        {
            return underlying.TryMove(cell, dir, out dest, out inverseDir, out connection);
        }
        #endregion

        #region Bounds
        public override IGrid BoundBy(IBound bound) => new RelaxModifier(this, (SquareBound)bound);
        #endregion

    }


    /// <summary>
    /// Utility for performing linear interpolation between hexes
    /// </summary>
    internal struct NearbyHexes
    {
        public Cell Hex1 { get; set; }
        public float Weight1 { get; set; }

        public Cell Hex2 { get; set; }
        public float Weight2 { get; set; }

        public Cell Hex3 { get; set; }
        public float Weight3 { get; set; }



        // For a given point, finds the three hexes in a HexGrid(1, PointyTopped)
        // that are share the corner nearest the point.
        // Also give weights that smoothly interpolates between them.
        internal static NearbyHexes FindNearbyHexes(Vector3 position)
        {
            // The dual of a HexGrid(1, PointyTopped)
            // is a TriangleGrid(sqrt(3)/2, FlatTopped)
            // So we find which cell we are in in the dual,
            // then find the corners of that cell and see which hexes they
            // correspond to.
            // Due to our choice of co-ordinate system, this is all fairly straightforward

            // Find cell in dual grid (see TriangleGrid.FindCell)
            var cellSize = new Vector2(Mathf.Sqrt(3) / 2, 0.75f);
            var x = position.x / cellSize.x;
            var y = position.y / cellSize.y;
            var a = x - 0.5f * y;
            var b = y;
            var c = -x - 0.5f * y;
            var cell = new Cell(
                Mathf.CeilToInt(a),
                Mathf.FloorToInt(b) + 1,
                Mathf.CeilToInt(c)
            );

            var s = cell.x + cell.y + cell.z;
            if (s == 1)
            {
                return new NearbyHexes
                {
                    Hex1 = new Cell(cell.x - 1, cell.y, cell.z),
                    Weight1 = 0 - (a - cell.x),
                    Hex2 = new Cell(cell.x, cell.y - 1, cell.z),
                    Weight2 = 0 - (b - cell.y),
                    Hex3 = new Cell(cell.x, cell.y, cell.z - 1),
                    Weight3 = 0 - (c - cell.z),
                };
            }
            else
            {
                return new NearbyHexes
                {
                    Hex1 = new Cell(cell.x, cell.y - 1, cell.z - 1),
                    Weight1 = 1 + (a - cell.x),
                    Hex2 = new Cell(cell.x - 1, cell.y, cell.z - 1),
                    Weight2 = 1 + (b - cell.y),
                    Hex3 = new Cell(cell.x - 1, cell.y - 1, cell.z),
                    Weight3 = 1 + (c - cell.z),
                };
            }
        }
    }
}


