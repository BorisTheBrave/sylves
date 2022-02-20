using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sylves.Test
{
    [TestFixture]
    public class TriangleGridTest
    {
        // Checks TryMoveByOffset gives same results as CellType.Roate
        [Test]
        [TestCase(TriangleOrientation.FlatTopped)]
        [TestCase(TriangleOrientation.FlatSides)]
        public void TestRotation(TriangleOrientation orientation)
        {
            var h = new TriangleGrid(1, orientation);
            GridTest.TryMoveByOffset(h, new Cell(1, 0, 0));
            GridTest.TryMoveByOffset(h, new Cell(2, 0, 0));
        }

        [Test]
        [TestCase(TriangleOrientation.FlatTopped)]
        [TestCase(TriangleOrientation.FlatSides)]
        public void FindCell(TriangleOrientation orientation)
        {
            var t = new TriangleGrid(new Vector2(1, 1), orientation);
            GridTest.FindCell(t, new Cell(1, 0, 0));
            GridTest.FindCell(t, new Cell(100, -99, 0));
            GridTest.FindCell(t, new Cell(100, -99, 0));
            GridTest.FindCell(t, new Cell(0, 100, -99));
            GridTest.FindCell(t, new Cell(5, 4, -7));
            GridTest.FindCell(t, new Cell(100, -49, -49));
        }
    }
}
