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
        public void TestTryGetRotation()
        {
            GridTest.TryGetRotation(HexPrismCellType.Get(HexOrientation.PointyTopped));
            GridTest.TryGetRotation(HexPrismCellType.Get(HexOrientation.FlatTopped));
        }
    }
}
