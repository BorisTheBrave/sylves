using System;
#if UNITY
using UnityEngine;
#endif

namespace Sylves
{
    // The townscaper grid, without the extra relaxation step
    internal class UnrelaxedTownscaperGrid : PlanarLazyMeshGrid
    {
        private readonly int n;
        private readonly float tolerance;
        private readonly HexGrid chunkGrid;

        public UnrelaxedTownscaperGrid(int n, float tolerance) : base()
        {
            this.n = n;
            this.tolerance = tolerance;
            chunkGrid = new HexGrid(n);

            base.Setup(GetMeshData, chunkGrid, meshGridOptions: new MeshGridOptions { Tolerance = tolerance });
        }

        private MeshData GetMeshData(Cell hex)
        {
            var offset = chunkGrid.GetCellCenter(hex);

            // Make a triangle grid that fills the chunk
            var triangleGrid = new TriangleGrid(0.5f, TriangleOrientation.FlatSides, bound: TriangleBound.Hexagon(n));
            var meshData = Matrix4x4.Translate(offset) * triangleGrid.ToMeshData();

            // Randomly pair the triangles of that grid
            var seed = HashUtils.Hash(hex);
            var random = new System.Random(seed);
            meshData = meshData.RandomPairing(random.NextDouble);

            // Split into quads
            meshData = ConwayOperators.Ortho(meshData);

            // Weld vertices
            meshData = meshData.Weld(tolerance);

            return meshData;
        }
    }

    /// <summary>
    /// A grid closely modelled after the grid used in Townscaper.
    /// See the corresponding tutorial.
    /// </summary>
    public class TownscaperGrid : RelaxModifier
    {
        const float tolerance = 1e-2f;

        public TownscaperGrid(int n, int relaxIterations = 10) : base(new UnrelaxedTownscaperGrid(n, tolerance), n, relaxIterations: relaxIterations, weldTolerance: tolerance)
        {

        }
    }
}
