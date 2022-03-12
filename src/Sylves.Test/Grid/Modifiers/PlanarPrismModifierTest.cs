using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sylves.Test
{
    [TestFixture]
    internal class PlanarPrismModifierTest
    {

        private PlanarPrismModifier GetGrid()
        {
            return new PlanarPrismModifier(new SquareGrid(1), new PlanarPrismOptions { }); ;
        }

        // Checks TryMoveByOffset gives same results as CellType.Roate
        [Test]
        public void TestTryMoveByOffset()
        {
            var g = GetGrid();
            GridTest.TryMoveByOffset(g, new Cell());
        }


        [Test]
        public void TestFindCell()
        {
            var g = GetGrid();
            GridTest.FindCell(g, new Cell(0, 0, 10));
        }


        [Test]
        public void TestFindBasicPath()
        {
            var g = GetGrid();
            GridTest.FindBasicPath(g, new Cell(1, 0, -1), new Cell(10, -5, -5));
        }
    }
}
