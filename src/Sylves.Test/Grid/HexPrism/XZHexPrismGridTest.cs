using NUnit.Framework;
using Sylves;
#if UNITY
using UnityEngine;
#endif


namespace Sylves.Test
{
    [TestFixture]
    public class XZHexPrismGridTest
    {
        [Test]
        public void TestFindCell()
        {
            var g = new XZHexPrismGrid(1, 1);
            GridTest.FindCell(g, new Cell());
        }

        [Test]
        public void TestCellRotation()
        {
            var g = new XZHexPrismGrid(1, 1);
            var ct = g.GetCellType(new Cell());
            var m = ct.GetMatrix(ct.RotateCCW);
            Assert.AreEqual(Vector3.up, m.MultiplyPoint(Vector3.up));
        }

        [Test]
        public void TestCellPosition()
        {
            var g = new XZHexPrismGrid(1, 1);
            var c = g.GetCellCenter(new Cell(10, -5, 0));
            Assert.AreEqual(0, c.y);
        }

        [Test]
        public void TestParallelTransport()
        {
            var g = new XZHexPrismGrid(1, 1);
            var cellGrid = new XZHexPrismGrid(1, 1);
            var success = g.ParallelTransport(cellGrid, new Cell(), new Cell(), new Cell(-13, 8, 0), (CellRotation)1, out var destCell, out var destRotation);
            Assert.IsTrue(success);
        }
    }
}
