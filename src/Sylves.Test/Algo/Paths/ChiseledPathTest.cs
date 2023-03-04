using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sylves.Test
{
    [TestFixture]
    public class ChiseledPathTest
    {
        [Test]
        public void TestChiseledPath()
        {
            var g = new SquareGrid(1, new SquareBound(0, 0, 10, 10));

            var path = ChisledPathfinding.FindPath(g, new Cell(1, 1), new Cell(8, 8));

            foreach(var c in path.Cells)
            {
                Console.WriteLine(c);
            }
        }
    }
}
