using NUnit.Framework;
#if UNITY
using UnityEngine;
#endif

using static Sylves.Test.TestUtils;


namespace Sylves.Test
{
    [TestFixture]
    internal class PeriodicPlanarMeshGridTest
    {
        [Test]
        public void TestPeriodicPlanarMeshGrid()
        {
            var g = new PeriodicPlanarMeshGrid(TestMeshes.PlaneXY, Vector2.right, Vector2.up);
            AssertAreEqual(Vector3.zero, g.GetCellCenter(new Cell(0, 0, 0)), 1e-6);
            AssertAreEqual(Vector3.right, g.GetCellCenter(new Cell(0, 1, 0)), 1e-6);
            AssertAreEqual(Vector3.up, g.GetCellCenter(new Cell(0, 0, 1)), 1e-6);

            Assert.AreEqual(new Cell(0, 1, 0), g.Move(new Cell(0, 0, 0), (CellDir)SquareDir.Right));
            Assert.AreEqual(new Cell(0, 0, 1), g.Move(new Cell(0, 0, 0), (CellDir)SquareDir.Up));
        }


        [Test]
        [Ignore("Not supported yet")]
        public void TestTryMoveByOffset()
        {
            var g = new PeriodicPlanarMeshGrid(TestMeshes.PlaneXY, Vector2.right, Vector2.up);
            GridTest.TryMoveByOffset(g, new Cell());
        }

        [Test]
        public void TestFindCell()
        {
            var g = new PeriodicPlanarMeshGrid(TestMeshes.PlaneXY, Vector2.right, Vector2.up);
            GridTest.FindCell(g, new Cell(0, 2, 3));
        }

        [Test]
        [Ignore("Not supported yet")]
        public void TestFindBasicPath()
        {
            var g = new PeriodicPlanarMeshGrid(TestMeshes.PlaneXY, Vector2.right, Vector2.up);
            GridTest.FindBasicPath(g, new Cell(0, 0, 0), new Cell(0, 10, 10));
        }
    }
}
