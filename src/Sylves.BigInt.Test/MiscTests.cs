using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sylves;
using System.Numerics;

namespace Sylves.Test
{
    [TestFixture]
    public class MiscTests
    {
        public void TestRoundTrip(BigInteger a, BigInteger b)
        {
            var (actualA, actualB) = Encoding.ZOrderDecode(Encoding.ZOrderEncode(a, b));    
            Assert.AreEqual(b, actualB);
            Assert.AreEqual(a, actualA);
        }

        [Test]
        public void TestEncodingZOrderRoundTrip()
        {
            TestRoundTrip(0, 0);
            TestRoundTrip(-1, -1);
            TestRoundTrip(250, -1);
            TestRoundTrip(250, 250);
        }

        BigInteger OneTrillion = BigInteger.Pow(10, 12);

        [Test]
        public void TestSquareGrid()
        {
            var cell1 = new Cell(OneTrillion, 0, 0);
            var grid = new SquareGrid(1);
            Assert.AreEqual(cell1 + new Vector3Int(1, 0, 0), grid.Move(cell1, (CellDir)SquareDir.Right));
        }

        [Test]
        public void TestRecenter()
        {
            var cell1 = new Cell(OneTrillion * OneTrillion, 0, 0);
            IGrid grid = new SquareGrid(1);
            var cell2 = cell1 + new Vector3Int(1, 0, 0);
            var offset1 = grid.GetCellCenter(cell2) - grid.GetCellCenter(cell1);
            grid = grid.Recenter(cell1);
            var offset2 = grid.GetCellCenter(cell2) - grid.GetCellCenter(cell1);
            Assert.AreNotEqual(new Vector3(1, 0, 0), offset1);
            Assert.AreEqual(new Vector3(1, 0, 0), offset2);
        }
    }
}
