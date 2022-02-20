using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sylves.Test
{
    [TestFixture]
    internal class SquareGridTest
    {
        [Test]
        public void TestRotation()
        {
            var h = new SquareGrid(Vector2.one);
            GridTest.TryMoveByOffset(h, new Cell());
        }
    }
}
