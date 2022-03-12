using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sylves.Test
{
    [TestFixture]
    public class HexPrismCellTypeTest
    {
        [Test]
        [TestCase(HexOrientation.PointyTopped)]
        [TestCase(HexOrientation.FlatTopped)]
        public void TestTryGetRotation(HexOrientation orientation)
        {
            GridTest.TryGetRotation(HexPrismCellType.Get(orientation));
        }
    }
}
