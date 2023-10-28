using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sylves.Test
{
    [TestFixture]
    internal class OffGridTest
    {
        [Test]
        public void TestTryMove()
        {
            var g = new OffGrid(seed: 0);
            Assert.IsTrue(g.TryMove(new Cell(), (CellDir)SquareDir.Right, out var dest, out var invDir, out var connection));
            Assert.AreEqual(new Cell(1, 0), dest);

        }

        [Test]
        public void TestFindCell()
        {
            var g = new OffGrid(seed: 0);
            GridTest.FindCell(g, new Cell(100, 100));
        }
    }
}