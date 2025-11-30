using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
#if UNITY
using UnityEngine;
#endif

namespace Sylves
{
    // The townscaper grid, without the extra relaxation step
    internal class UnrelaxedTownscaperGrid : PlanarLazyMeshGrid
    {
        private const float TriangleSize = 0.5f;

        private readonly Int32 n;
        private readonly float tolerance;
        private readonly HexGrid chunkGrid;
        private readonly Int32 seed;

        // Clone constructor
        public UnrelaxedTownscaperGrid(UnrelaxedTownscaperGrid original, SquareBound bound) : base(original, bound)
        {
            this.n = original.n;
            this.seed = original.seed;
            this.tolerance = original.tolerance;
            this.chunkGrid = original.chunkGrid;
        }

        public UnrelaxedTownscaperGrid(Int32 n, Int32 seed, float tolerance) : base()
        {
            this.n = n;
            this.seed = seed;
            this.tolerance = tolerance;
            chunkGrid = new HexGrid(n * 2 * TriangleSize);

            base.Setup(GetMeshData, chunkGrid, translateMeshData: true, meshGridOptions: new MeshGridOptions { Tolerance = tolerance });
        }

        private MeshData GetMeshData(Cell hex)
        {
            // Make a triangle grid that fills the chunk
            var triangleGrid = new TriangleGrid(TriangleSize, TriangleOrientation.FlatSides, bound: TriangleBound.Hexagon(n));
            var meshData = triangleGrid.ToMeshData();

            // Randomly pair the triangles of that grid
            var hexSeed = HashUtils.Hash(hex, seed);
            var random = new System.Random(hexSeed);
            meshData = meshData.RandomPairing(random.NextDouble);

            // Split into quads
            meshData = ConwayOperators.Ortho(meshData);

            // Weld vertices
            meshData = meshData.Weld(tolerance);

            return meshData;
        }

        private static ICellType[] s_cellTypes = new[] { SquareCellType.Instance };

        public override IEnumerable<ICellType> GetCellTypes() => s_cellTypes;

        public override IGrid GetCompactGrid()
        {
            // There's probably a nice formula for this, based on area
            var maxCellsPerHex = HexBound.Hexagon(n).Count();
            return DefaultGridImpl.GetCompactGridFiniteX(this, maxCellsPerHex);
        }

        public override IGrid Unbounded => new UnrelaxedTownscaperGrid(this, null);

        public override IGrid BoundBy(IBound bound) => bound == null ? this : new UnrelaxedTownscaperGrid(this, (SquareBound)IntersectBounds(this.GetBound(), bound));
    }

    /// <summary>
    /// A grid closely modelled after the grid used in Townscaper.
    /// See the corresponding tutorial.
    /// </summary>
    public class TownscaperGrid : RelaxModifier
    {
        const float tolerance = 1e-2f;

        public TownscaperGrid(Int32 n, Int32? seed = null, Int32 relaxIterations = 10) : base(new UnrelaxedTownscaperGrid(n, seed ?? new System.Random().Next(), tolerance), n, relaxIterations: relaxIterations, weldTolerance: tolerance)
        {
        }
    }
}
