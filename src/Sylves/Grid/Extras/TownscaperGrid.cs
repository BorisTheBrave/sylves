using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylves
{
    // The townscaper grid, without the extra relaxation step
    internal class UnrelaxedTownscaperGrid : PlanarLazyGrid
    {
        private readonly int n;
        private readonly float weldTolerance;
        private readonly HexGrid chunkGrid;

        public UnrelaxedTownscaperGrid(int n, float weldTolerance) : base()
        {
            this.n = n;
            this.weldTolerance = weldTolerance;
            chunkGrid = new HexGrid(n);

            base.Setup(GetMeshData, chunkGrid);
        }

        private MeshData GetMeshData(Cell hex)
        {
            var offset = chunkGrid.GetCellCenter(hex);

            // Make a triangle grid that fills the chunk
            var triangleGrid = new TriangleGrid(0.5f, TriangleOrientation.FlatSides, bound: TriangleBound.Hexagon(n));
            var meshData = Matrix4x4.Translate(offset) * triangleGrid.ToMeshData();

            // Randomly pair the triangles of that grid
            var seed = hex.x * 1000 + hex.y;
            var random = new Random(seed);
            meshData = meshData.RandomPairing(random.NextDouble);

            // Split into quads
            meshData = ConwayOperators.Ortho(meshData);

            // Weld vertices
            meshData = meshData.Weld(weldTolerance);

            return meshData;
        }
    }

    /// <summary>
    /// A grid closely modelled after the grid used in Townscaper.
    /// See the corresponding tutorial.
    /// </summary>
    public class TownscaperGrid : RelaxModifier
    {
        const float weldTolerance = 1e-2f;

        public TownscaperGrid(int n) : base(new UnrelaxedTownscaperGrid(n, weldTolerance), n, weldTolerance: weldTolerance)
        {

        }
    }
}
