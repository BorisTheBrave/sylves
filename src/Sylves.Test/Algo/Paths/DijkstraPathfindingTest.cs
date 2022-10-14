using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sylves.Test
{
    [TestFixture]
    public class DijkstraPathfindingTest
    {
        [Test]
        public void TestBasicPath()
        {
            var g = new SquareGrid(1);
            var pf = new DijkstraPathfinding(g, new Cell(), StepLengths.Uniform);

            pf.Run(new Cell(2, 2));

            var path = pf.ExtractPathTo(new Cell(2, 2));
            Assert.AreEqual(4, path.Length);
        }


        [Test]
        public void TestNoPath()
        {
            var g = new SquareGrid(1, new SquareBound(0, 0, 3, 3));
            var pf = new DijkstraPathfinding(g, new Cell(), StepLengths.Create(c => c.x != 1));

            pf.Run(new Cell(2, 2));

            var path = pf.ExtractPathTo(new Cell(2, 2));
            Assert.IsNull(path);
        }

        [Test]
        public void TestDistances()
        {
            var g = new SquareGrid(1);
            var pf = new DijkstraPathfinding(g, new Cell(), StepLengths.Uniform);

            pf.Run(maxRange: 4.5f);

            Assert.AreEqual(4, pf.Distances[new Cell(2, 2)]);
            Assert.IsFalse(pf.Distances.ContainsKey(new Cell(2, 3)));
        }
    }
}
