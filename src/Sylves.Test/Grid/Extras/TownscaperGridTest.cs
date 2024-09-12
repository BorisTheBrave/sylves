using NUnit.Framework;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sylves.Test
{
    [TestFixture]
    internal class TownscaperGridTest
    {
        [Test]
        public void TestPrisms()
        {
            Assert.Throws<ArgumentException>(() => new PlanarPrismModifier(new TownscaperGrid(4)));

            var g = new PlanarPrismModifier(new TownscaperGrid(4).GetCompactGrid());
            
            GridTest.FindCell(g, new Cell(0, 0));
        }
    }
}
