using NUnit.Framework;
#if UNITY
using UnityEngine;
#endif


namespace Sylves.Test
{
    [TestFixture]
    public class HexPrismGridTest
    {
        [Test]
        [TestCase(HexOrientation.PointyTopped)]
        [TestCase(HexOrientation.FlatTopped)]
        public void TestTryMoveByOffset(HexOrientation orientation)
        {
            var h = new HexPrismGrid(1, 1, orientation);
            GridTest.TryMoveByOffset(h, new Cell());
        }


        [Test]
        [TestCase(HexOrientation.PointyTopped)]
        [TestCase(HexOrientation.FlatTopped)]
        public void TestFindCell(HexOrientation orientation)
        {
            var h = new HexPrismGrid(new Vector3(1, 1, 1), orientation);
            GridTest.FindCell(h, new Cell(1, 0, -1));
            GridTest.FindCell(h, new Cell(100, -100, 0));
            GridTest.FindCell(h, new Cell(0, 100, -100));
            GridTest.FindCell(h, new Cell(-100, 0, 100));
            GridTest.FindCell(h, new Cell(100, -50, -50));
        }


        [Test]
        [TestCase(HexOrientation.PointyTopped)]
        [TestCase(HexOrientation.FlatTopped)]
        public void TestFindBasicPath(HexOrientation orientation)
        {
            var h = new HexPrismGrid(new Vector3(1, 1, 1), orientation);

            GridTest.FindBasicPath(h, new Cell(1, 0, -1), new Cell(10, -5, -5));
        }
    }
}
