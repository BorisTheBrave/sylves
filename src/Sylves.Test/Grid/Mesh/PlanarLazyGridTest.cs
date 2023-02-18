using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sylves.Test
{
    [TestFixture]
    public class PlanarLazyGridTest
    {
        [Test]
        public void TestPlanarLazyGrid()
        {
            var g = new PlanarLazyGrid(
                chunk => Matrix4x4.Translate(new Vector3(chunk.x, chunk.y, 0)) * TestMeshes.PlaneXY,
                Vector2.right,
                Vector2.up,
                new Vector2(-.5f, -.5f),
                Vector2.one
                );

            Assert.AreEqual(new Cell(0, 1, 0), g.Move(new Cell(0, 0, 0), (CellDir)SquareDir.Right));
            Assert.AreEqual(new Cell(0, 0, 1), g.Move(new Cell(0, 0, 0), (CellDir)SquareDir.Up));
            Assert.AreEqual(new Cell(0, 0, 2), g.Move(new Cell(0, 0, 1), (CellDir)SquareDir.Up));
        }

        private static Vector2 ToVector2(Vector3 v) => new Vector2(v.x, v.y);


        [Test]
        public void TestTownscaperLike()
        {
            // Each chunk corresponds to a single cell in this hex grid
            var n = 4;
            var chunkGrid = new HexGrid(n);
            Cell ChunkToCell(Vector2Int chunk) => new Cell(chunk.x, chunk.y, -chunk.x -chunk.y);

            MeshData GetMeshData(Vector2Int chunk)
            {
                var offset = chunkGrid.GetCellCenter(ChunkToCell(chunk));

                // Make a triangle grid that fills the chunk
                var triangleGrid = new TriangleGrid(0.5f, TriangleOrientation.FlatSides, bound: TriangleBound.Hexagon(n));
                var meshData = Matrix4x4.Translate(offset) * triangleGrid.ToMeshData();

                // Randomly pair the triangles of that grid
                var seed = chunk.x * 1000 + chunk.y;
                var random = new Random(seed);
                meshData = meshData.RandomPairing(random.NextDouble);

                // Split into quads
                meshData = ConwayOperators.Ortho(meshData);

                // Weld vertices
                meshData = meshData.Weld(1e-1f);

                return meshData;
            }


            // Work out the dimensions of the chunk grid, needed for PlanarLazyGrid
            var strideX = ToVector2(chunkGrid.GetCellCenter(ChunkToCell(new Vector2Int(1, 0))));
            var strideY = ToVector2(chunkGrid.GetCellCenter(ChunkToCell(new Vector2Int(0, 1))));

            var polygon = chunkGrid.GetPolygon(ChunkToCell(new Vector2Int())).Select(ToVector2);
            var aabbBottomLeft = polygon.Aggregate(Vector2.Min);
            var aabbTopRight = polygon.Aggregate(Vector2.Max);
            var aabbSize = aabbTopRight - aabbBottomLeft;


            var hexGrid = new HexGrid(1);
            var triGrid = hexGrid.GetChildTriangleGrid();


            var g = new PlanarLazyGrid(
                GetMeshData,
                strideX,
                strideY,
                aabbBottomLeft,
                aabbSize,
                bound: new SquareBound(-2, -2, 2, 2)
                );

            GridDocsExportTest.Export(g, "ts_grid.svg", new GridDocsExportTest.Options
            {
                textScale = null,
                min = new Vector2(-3, -3),
                max = new Vector2(3, 3),
            });

            GridDocsExportTest.Export(RelaxModifier.Create(g, n, 1e-1f), "ts2_grid.svg", new GridDocsExportTest.Options
            {
                textScale = null,
                min = new Vector2(-3, -3),
                max = new Vector2(3, 3),
            });
        }
    }
}
