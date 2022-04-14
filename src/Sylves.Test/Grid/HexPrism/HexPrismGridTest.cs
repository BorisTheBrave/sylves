using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
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

        [Test]
        public void TestHexBound()
        {
            Assert.AreEqual(16, new HexBound(new Vector3Int(-8, 0, 0), new Vector3Int(1, 4, 4)).Count());
            Assert.AreEqual(32, new HexPrismBound(new HexBound(new Vector3Int(-100, 0, 0), new Vector3Int(100, 4, 4)), 0, 2).Count());
        }

        [Test]
        [TestCase(HexOrientation.PointyTopped)]
        [TestCase(HexOrientation.FlatTopped)]
        public void TestFindSymmetry(HexOrientation orientation)
        {
            var g = new HexPrismGrid(new Vector3(1, 1, 1), orientation);
            //GridTest.FindGridSymmetry(g, new Cell(0, 0, 0));


            {
                var cells = new HashSet<Cell> { new Cell(0, 0, 0), new Cell(0, 1, 0) };
                var s = g.FindGridSymmetry(cells, cells, new Cell(), HexRotation.Identity);
                Assert.IsNotNull(s);
            }
        }
    }
}
