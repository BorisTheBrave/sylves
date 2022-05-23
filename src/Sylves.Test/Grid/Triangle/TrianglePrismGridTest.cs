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
        public void TestFindGridSymmetry()
        {
            var g = new TrianglePrismGrid(1, 1);

            var set = new HashSet<Cell>(new[] { new Cell() });

            g.FindGridSymmetry(set, set, new Cell(), HexRotation.Identity);
        }
    }
}
