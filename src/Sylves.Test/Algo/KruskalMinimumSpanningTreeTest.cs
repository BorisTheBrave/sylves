using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sylves.Test
{
    public class KruskalMinimumSpanningTreeTest
    {
        [Test]
        public void TestSquareGrid()
        {
            var g = new SquareGrid(1, new SquareBound(0, 0, 10, 10));
            var tree = KruskalMinimumSpanningTree.Calculate(g, _ => 1).ToList();

            // Check minimal
            Assert.AreEqual(9 * 10 + 9, tree.Count);

            // Check spanning
            var d = Pathfinding.FindDistances(g, new Cell(), stepLengths: s => tree.Any(x=>x.Src == s.Dest && x.Dest == s.Src || x.Src == s.Src && x.Dest == s.Dest) ? 1 : (float?)null);
            Assert.AreEqual(g.GetCells().Count(), d.Count);
        }
    }
}
