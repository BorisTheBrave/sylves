using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sylves.Test
{
    [TestFixture]
    public class AStarPathfindingTest
    {
        [Test]
        public void TestBasicPath()
        {
            var g = new SquareGrid(1);
            var dest = new Cell(2, 2);
            var pf = new AStarPathfinding(g, new Cell(), StepLengths.Uniform, Pathfinding.GetAdmissibleHeuristic(g, dest));

            pf.Run(dest);

            var path = pf.ExtractPathTo(dest);
            Assert.AreEqual(4, path.Length);
        }


        [Test]
        public void TestObstaclesPath()
        {
            var g = new SquareGrid(1);
            var dest = new Cell(2, 0);
            var walls = new[] { new Cell(1, -1), new Cell(1, 0), new Cell(1, 1) };
            var ew = StepLengths.Create(x => !walls.Contains(x));
            var pf = new AStarPathfinding(g, new Cell(), ew, Pathfinding.GetAdmissibleHeuristic(g, dest));

            pf.Run(dest);

            var path = pf.ExtractPathTo(dest);
            Assert.AreEqual(6.0f, path.Length);
        }
    }
}
