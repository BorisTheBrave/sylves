using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sylves.Test
{
    [TestFixture]
    internal class TrianglePrismGridTest
    {
        [Test]
        [TestCase(TriangleOrientation.FlatTopped)]
        [TestCase(TriangleOrientation.FlatSides)]
        public void TestFindGridSymmetry(TriangleOrientation orientation)
        {
            var g = new TrianglePrismGrid(1, 1, orientation);
            GridTest.FindGridSymmetry(g, new Cell(0, 0, 0));
        }

        [Test]
        public void TestFindGridSymmetry2()
        {
            var g = new TrianglePrismGrid(1, 1);

            var cell = new Cell(0, 0, 0);
            var cells = new HashSet<Cell>(new[] { cell });
            var s = g.FindGridSymmetry(cells, cells, cell, HexRotation.Rotate60(2));
            Assert.IsNotNull(s);
        }

        [Test]
        public void TestBijection()
        {
            // Check a few round trips
            Assert.AreEqual(new Cell(0, 0, 0), TrianglePrismGrid.FromTriangleGrid(TrianglePrismGrid.ToTriangleGrid(new Cell(0, 0, 0))));
            Assert.AreEqual(new Cell(1, 0, 0), TrianglePrismGrid.FromTriangleGrid(TrianglePrismGrid.ToTriangleGrid(new Cell(1, 0, 0))));
            Assert.AreEqual(new Cell(1, 1, 0), TrianglePrismGrid.FromTriangleGrid(TrianglePrismGrid.ToTriangleGrid(new Cell(1, 1, 0))));
            Assert.AreEqual(new Cell(-1, 0, 0), TrianglePrismGrid.FromTriangleGrid(TrianglePrismGrid.ToTriangleGrid(new Cell(-1, 0, 0))));
            Assert.AreEqual(new Cell(-2, 0, 0), TrianglePrismGrid.FromTriangleGrid(TrianglePrismGrid.ToTriangleGrid(new Cell(-2, 0, 0))));
            Assert.AreEqual(new Cell(-1, 0, 3), TrianglePrismGrid.ToTriangleGrid(TrianglePrismGrid.FromTriangleGrid(new Cell(-1, 0, 3))));

            // Check where (0, 0, 0) goes (it's convenient if this points down to suit conventions in tessera)
            Assert.AreEqual(new Cell(0, 0, 1), TrianglePrismGrid.ToTriangleGrid(new Cell(0, 0, 0)));
        }
    }
}
