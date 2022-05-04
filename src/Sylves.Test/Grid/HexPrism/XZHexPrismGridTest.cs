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
            var success = g.ParallelTransport(cellGrid, new Cell(0,1,0), new Cell(0,0,0), new Cell(-9, 3, 0), (CellRotation)3, out var destCell, out var destRotation);
            var success2 = g.TryMoveByOffset(new Cell(-9, 3, 0), new Vector3Int(0, 1, 0), new Vector3Int(0, 0, 0), (CellRotation)3, out var destCell2, out var destRotation2);
            Assert.IsTrue(success);
        }
    }
}
