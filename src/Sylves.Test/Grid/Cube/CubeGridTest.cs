using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sylves.Test
{
    [TestFixture]
    public class CubeGridTest
    {
        [Test]
        public void TestTryMoveByOffset()
        {
            var g = new CubeGrid(1);
            GridTest.TryMoveByOffset(g, new Cell());
        }


        [Test]
        public void TestFindCell()
        {
            var g = new CubeGrid(1);
            GridTest.FindCell(g, new Cell(1, 0, -1));
        }
    }
}
