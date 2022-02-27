using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sylves.Test
{
    [TestFixture]
    public class HexPrismGridTest
    {
        // Checks TryMoveByOffset gives same results as CellType.Roate
        [Test]
        [TestCase(HexOrientation.PointyTopped)]
        [TestCase(HexOrientation.FlatTopped)]
        public void TestTryMoveByOffset(HexOrientation orientation)
        {
            var h = new HexPrismGrid(1, 1, orientation);
            GridTest.TryMoveByOffset(h, new Cell());
        }


        [Test]
        [TestCase(HexOrientation.PointyTopped)]
        [TestCase(HexOrientation.FlatTopped)]
        public void TestFindCell(HexOrientation orientation)
        {
            var h = new HexPrismGrid(new Vector3(1, 1, 1), orientation);
            GridTest.FindCell(h, new Cell(1, 0, -1));
            GridTest.FindCell(h, new Cell(100, -100, 0));
            GridTest.FindCell(h, new Cell(0, 100, -100));
            GridTest.FindCell(h, new Cell(-100, 0, 100));
            GridTest.FindCell(h, new Cell(100, -50, -50));
        }
    }
}
