using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylves
{
    // Works by splitting up the plane into hexes
    // Each hex is joined with its 6 neighbours and relaxed to make overlapping patches
    // Each point is a blend of the three nearest patches
    public class RelaxModifier : PlanarLazyGrid
    {
        private readonly IGrid underlying;
        private readonly float chunkSize;
        private readonly float weldTolerance;
        private readonly int relaxIterations;

        // Description of the chunking used
        HexGrid chunkGrid;

        // Caches

        // Unrelaxed chunks are just the raw mesh data taken from underlying
        IDictionary<Cell, MeshData> unrelaxedChunks;

        // Relaxed patches are the result of concatting several unrelaxed chunks together,
        // then relaxing them
        IDictionary<Cell, (MeshData, Dictionary<Cell, int[]>)> relaxedPatches;

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

            cachePolicy = cachePolicy ?? CachePolicy.Always;

            chunkGrid = new HexGrid(chunkSize);

            unrelaxedChunks = cachePolicy.GetDictionary<MeshData>(chunkGrid);
            relaxedPatches = cachePolicy.GetDictionary<(MeshData, Dictionary<Cell, int[]>)>(chunkGrid);
            this.underlying = underlying;
            this.chunkSize = chunkSize;
            this.weldTolerance = weldTolerance;
            this.relaxIterations = relaxIterations;

            // Infer celltypes from underlying if possible
            IEnumerable<ICellType> cellTypes = null;
            try
            {
                cellTypes = underlying.GetCellTypes();
            } catch (Exception ex)
            {

            }

            // Infer bound from underlying if possible
            // Perhaps this should be done lazily?
            SquareBound bound = null;
            if(underlying.IsFinite)
            {
                var chunkCells = underlying.GetCells()
                    .Select(underlying.GetCellCenter)
                    .Select(chunkGrid.FindCell)
                    .OfType<Cell>();
                var chunkBound = (HexBound)chunkGrid.GetBound(chunkCells);
                bound = new SquareBound(new Vector2Int(chunkBound.min.x, chunkBound.min.y), new Vector2Int(chunkBound.max.x, chunkBound.max.y));

            }

            base.Setup(GetRelaxedChunk, chunkGrid, bound: bound, cellTypes: cellTypes);
        }

        // Clone constructor. Clones share the same cache!
        private RelaxModifier(RelaxModifier original, SquareBound bound)
            :base(original, bound)
        {
            underlying = original.underlying;
            chunkSize = original.chunkSize;
            weldTolerance = original.weldTolerance;
            relaxIterations = original.relaxIterations;
            chunkGrid = original.chunkGrid;
            unrelaxedChunks = original.unrelaxedChunks;
            relaxedPatches = original.relaxedPatches;
        }

        #region Calculations

        private static Vector3 ToVector3(Vector2 v) => new Vector3(v.x, v.y, 0);

        // Unrelaxed chunks are just the raw mesh data taken from underlying
        MeshData GetUnrelaxedChunk(Cell hex)
        {
            if (unrelaxedChunks.ContainsKey(hex))
                return unrelaxedChunks[hex];

            // Get cells near the chunk
            var min = aabbBottomLeft + hex.x * strideX + hex.y * strideY;
            var max = min + aabbSize;
            var unfilteredCells = underlying.GetCellsIntersectsApprox(ToVector3(min), ToVector3(max));

            // Filter to precisely cells in this chunk
            var cells = unfilteredCells
                .Where(c => chunkGrid.FindCell(underlying.GetCellCenter(c)) == hex)
                .ToList();

            // To mesh data
            return unrelaxedChunks[hex] = underlying.ToMeshData(cells);
        }

        // Relaxed patches are the result of concatting several unrelaxed chunks together,
        // then relaxing them
        (MeshData meshData, Dictionary<Cell, int[]> indexMaps) GetRelaxedPatch(Cell hex)
        {
            if (relaxedPatches.ContainsKey(hex))
                return relaxedPatches[hex];

            var nearbyChunks = new[] { hex }.Concat(chunkGrid.GetNeighbours(hex));

            var md = MeshDataOperations.Concat(nearbyChunks.Select(c => GetUnrelaxedChunk(c)), out var concatIndexMaps);

            md = md.Weld(out var weldIndexMap, weldTolerance).Relax(relaxIterations);

            // For each nearby chunk, find the where each vertex corresponds to in the output md.
            var maps = nearbyChunks.Zip(concatIndexMaps, (a, b) => (a, b)).ToDictionary(x => x.a, x => x.b.Select(i => weldIndexMap[i]).ToArray());

            return relaxedPatches[hex] = (md, maps);
        }

        // The actual mesh data matches the unrelaxed chunk
        // but with position data interpolated from several relaxed patches.
        MeshData GetRelaxedChunk(Cell hex)
        {
            var unrelaxed = GetUnrelaxedChunk(hex);

            var result = unrelaxed.Clone();
            result.vertices = new Vector3[unrelaxed.vertices.Length];

            for (var i = 0; i < unrelaxed.vertices.Length; i++)
            {
                var v = unrelaxed.vertices[i];
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


        // Maybe more should be override to pass through stuff from underlying?

        #region Basics

        public virtual bool Is2d => underlying.Is2d;

        public virtual bool Is3d => underlying.Is3d;

        public virtual bool IsPlanar => underlying.IsPlanar;

        public virtual bool IsRepeating => underlying.IsRepeating;

        public virtual bool IsOrientable => underlying.IsOrientable;

        public virtual bool IsFinite => base.IsFinite || underlying.IsFinite;

        public virtual bool IsSingleCellType => underlying.IsSingleCellType;

        public virtual IEnumerable<ICellType> GetCellTypes() => underlying.GetCellTypes();

        #endregion

        #region Relatives
        public override IGrid Unbounded => new RelaxModifier(this, null);
        #endregion

        public override IGrid BoundBy(IBound bound) => new RelaxModifier(this, (SquareBound)bound);

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


