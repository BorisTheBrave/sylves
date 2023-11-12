using NUnit.Framework;
using System;
using System.Linq;
#if UNITY
using UnityEngine;
#endif


namespace Sylves.Test
{
    [TestFixture]
    public class PlanarLazyMeshGridTest
    {
        [Test]
        public void TestPlanarLazyMeshGrid()
        {
            var g = new PlanarLazyMeshGrid(
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
        [Test]
        public void TestDual()
        {
            var g = new PlanarLazyMeshGrid(
                chunk => Matrix4x4.Translate(new Vector3(chunk.x, chunk.y, 0)) * TestMeshes.PlaneXY,
                Vector2.right,
                Vector2.up,
                new Vector2(-.5f, -.5f),
                Vector2.one
                );

            var dm = g.GetDual();

            GridTest.DualMapping(dm, new Cell(0, 0, 0));
            GridTest.DualMapping(dm, new Cell(0, 10, 10));
        }


        [Test]
        public void TestBoundedDual()
        {
            var g = new PlanarLazyMeshGrid(
                chunk => {
                    Assert.AreEqual(new Vector2Int(), chunk, $"Out of bounds chunk evaluated {chunk}");
                    return Matrix4x4.Translate(new Vector3(chunk.x, chunk.y, 0)) * TestMeshes.PlaneXY;
                    },
                Vector2.right,
                Vector2.up,
                new Vector2(-.5f, -.5f),
                Vector2.one,
                bound: new SquareBound(new Vector2Int(0, 0), new Vector2Int(1, 1))
                );

            var dm = g.GetDual();

            GridTest.DualMapping(dm, new Cell(0, 0, 0), checkPositions: false);

            var dualGrid = dm.DualGrid;
            Assert.AreEqual(4, dualGrid.GetCells().Count());

            foreach(var cell in dualGrid.GetCells())
            {
                Assert.AreEqual(2, dualGrid.GetNeighbours(cell).Count());
            }
        }

        [Test]
        public void TestConnectivity()
        {
            var g = new TownscaperGrid(1);
            var min = new Vector3(-10, -10, 0);
            var max = new Vector3(10, 10, 0);
            foreach (var cell in g.GetCellsIntersectsApprox(min, max))
            {
                //Assert.AreEqual(4, g.GetNeighbours(cell).Count(), $"Cell {cell} doesn't have 4 neighbours");
                foreach(var dir in SquareCellType.Instance.GetCellDirs())
                {
                    Assert.IsNotNull(g.Move(cell, dir), $"Cell {cell} doesn't have neighbour in dir {dir}");
                }
            }
        }

        [Test]
        public void TestGetMeshData()
        {
            var hexGrid = new HexGrid(4);
            var unrelaxedGrid = new PlanarLazyMeshGrid(GetMeshData, hexGrid);

            unrelaxedGrid.FindCell(Vector3.zero, out var cell);
            unrelaxedGrid.GetMeshData(cell, out var meshData, out var transform);

            MeshData GetMeshData(Cell hex)
            {
                var triangleGrid = new TriangleGrid(0.5f, TriangleOrientation.FlatSides, bound: TriangleBound.Hexagon(4));
                var meshData = triangleGrid.ToMeshData();
                meshData = Matrix4x4.Translate(hexGrid.GetCellCenter(hex)) * meshData;
                var seed = HashUtils.Hash(hex);
                meshData = meshData.RandomPairing(new Random(seed).NextDouble);
                meshData = ConwayOperators.Ortho(meshData);
                return meshData.Weld();
            }
        }
    }
}
