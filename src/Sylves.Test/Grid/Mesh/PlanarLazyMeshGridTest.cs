using NUnit.Framework;
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
    }
}
