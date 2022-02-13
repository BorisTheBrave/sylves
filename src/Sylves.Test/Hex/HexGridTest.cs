using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sylves.Test
{
    [TestFixture]
    public class HexGridTest
    {
        // Checks TryMoveByOffset gives same results as CellType.Roate
        [Test]
        [TestCase(HexOrientation.PointyTopped)]
        [TestCase(HexOrientation.FlatTopped)]
        public void TestRotation(HexOrientation orientation)
        {
            var h = new HexGrid(1, orientation);
            GridTest.TryMoveByOffset(h, new Cell());
        }
    }
}
