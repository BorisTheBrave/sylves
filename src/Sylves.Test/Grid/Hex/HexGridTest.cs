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
        public void TestTryMoveByOffset(HexOrientation orientation)
        {
            var h = new HexGrid(1, orientation);
            GridTest.TryMoveByOffset(h, new Cell());
        }


        [TestCase(HexOrientation.PointyTopped)]
        [TestCase(HexOrientation.FlatTopped)]
        public void TestFindCell(HexOrientation orientation)
        {
            var h = new HexGrid(new Vector2(1, 1), orientation);
            //GridTest.FindCell(h, new Cell(1, 0, -1));
            GridTest.FindCell(h, new Cell(100, -100, 0));
            GridTest.FindCell(h, new Cell(0, 100, -100));
            GridTest.FindCell(h, new Cell(-100, 0 , 100));
            //GridTest.FindCell(h, new Cell(100, -50, -50));
        }


        [Test]
        public void TestGetCellsIntersectsApprox()
        {
            var h = new HexGrid(2, HexOrientation.FlatTopped);

            var p = new Vector3(0, 0.57f, 0);
            CollectionAssert.AreEquivalent(
                new[] {
                    new Cell(0, 0, 0),
                    new Cell(1, 0, -1),
                    new Cell(0, 1, -1),
                },
                h.GetCellsIntersectsApprox(p, p + new Vector3(0.6f, 0.3f, 0)));


            CollectionAssert.AreEquivalent(
                new[] {
                    new Cell(0, 0, 0),
                    new Cell(1, 0, -1),
                    new Cell(0, 1, -1),
                    new Cell(1, 1, -2),
                },
                h.GetCellsIntersectsApprox(p, p + new Vector3(0.6f, 2f, 0)));

            h = new HexGrid(2, HexOrientation.PointyTopped);
            p = new Vector3(0.57f, 0, 0);
            CollectionAssert.AreEquivalent(
                new[] {
                    new Cell(0, 0, 0),
                    new Cell(1, 0, -1),
                    new Cell(0, 1, -1),
                },
                h.GetCellsIntersectsApprox(p, p + new Vector3(0.3f, 0.6f, 0)));

        }


        [Test]
        [TestCase(HexOrientation.PointyTopped)]
        [TestCase(HexOrientation.FlatTopped)]
        public void TestFindBasicPath(HexOrientation orientation)
        {
            var h = new HexGrid(new Vector2(1, 1), orientation);

            GridTest.FindBasicPath(h, new Cell(1, 0, -1), new Cell(10, -5, -5));
        }
    }
}
