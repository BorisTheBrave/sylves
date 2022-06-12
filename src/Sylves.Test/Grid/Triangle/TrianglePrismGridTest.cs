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
    }
}
