using NUnit.Framework;
using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sylves.Test
{
    [TestFixture]
    internal class XZModifierTest
    {
        [Test]
        public void Asdf()
        {
            var g = new XZModifier(new SquareGrid(1, new SquareBound(-10, -10, 10, 10)));
            var l = g.GetCells().Take(100).ToList();
            var a= g.GetCellCenter(new Cell(3, -9, 0));
            var b = g.FindCell(a, out var cell);
            Assert.IsNotNull(cell);
        }
    }
}
