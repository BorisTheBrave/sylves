using NUnit.Framework;
using Sylves.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sylves.Test
{
    [TestFixture]
    internal class DefaultDiagonalGridTest
    {

        [Test]
        public void TestTryMove()
        {
            var g = new DefaultDiagonalGrid(new SquareGrid(1), 8);

            void RoundTrip(Cell cell, CellDir dir)
            {
                var b = g.TryMove(cell, dir, out var dest, out var inverseDir, out var _);
                Assert.IsTrue(b);
                var b2 = g.TryMove(dest, inverseDir, out var dest2, out var inverseDir2, out var _);
                Assert.IsTrue(b2);
                Assert.AreEqual(cell, dest2);
                Assert.AreEqual(dir, inverseDir2);
            }
            void NoDir(Cell cell, CellDir dir)
            {
                var b = g.TryMove(cell, dir, out var dest, out var inverseDir, out var _);
                Assert.IsFalse(b);
            }

            Assert.AreEqual(new Cell(11, 11), g.Move(new Cell(10, 11), (CellDir)0));
            Assert.AreEqual(new Cell(11, 12), g.Move(new Cell(10, 11), (CellDir)1));
            Assert.AreEqual(new Cell(10, 12), g.Move(new Cell(10, 11), (CellDir)8));

            RoundTrip(new Cell(0, 0), (CellDir)(0+8*0));
            RoundTrip(new Cell(0, 0), (CellDir)(1+8*0));
            NoDir(new Cell(0, 0), (CellDir)(2+8*0));
            RoundTrip(new Cell(0, 0), (CellDir)(0 + 8 * 1));
            RoundTrip(new Cell(0, 0), (CellDir)(1 + 8 * 1));
            NoDir(new Cell(0, 0), (CellDir)(2 + 8 * 1));

        }

        [Test]
        public void TestDiagonals()
        {
            var g = new SquareGrid(1);
            GridTest.GetDiagonals(g, new Cell());
        }
    }
}
