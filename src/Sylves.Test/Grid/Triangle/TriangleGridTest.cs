using NUnit.Framework;
#if UNITY
using UnityEngine;
#endif


namespace Sylves.Test
{
    [TestFixture]
    public class TriangleGridTest
    {
        [Test]
        [TestCase(TriangleOrientation.FlatTopped)]
        public void TestUpdown(TriangleOrientation orientation)
        {
            var h = new TriangleGrid(1, orientation);
            CollectionAssert.AreEquivalent(
                new[] { (CellDir)FTHexDir.Up, (CellDir)FTHexDir.DownLeft, (CellDir)FTHexDir.DownRight },
                h.GetCellDirs(new Cell(1, 0, 0))
                );

            Assert.IsFalse(h.TryMove(new Cell(1, 0, 0), (CellDir)FTHexDir.Down, out var _, out var _, out var _));
        }

        [Test]
        [TestCase(TriangleOrientation.FlatTopped)]
        [TestCase(TriangleOrientation.FlatSides)]
        public void TestRotation(TriangleOrientation orientation)
        {
            var h = new TriangleGrid(1, orientation);
            GridTest.TryMoveByOffset(h, new Cell(1, 0, 0));
            GridTest.TryMoveByOffset(h, new Cell(2, 0, 0));
        }

        [Test]
        [TestCase(TriangleOrientation.FlatTopped)]
        [TestCase(TriangleOrientation.FlatSides)]
        public void TestFindCell(TriangleOrientation orientation)
        {
            var t = new TriangleGrid(new Vector2(1, 1), orientation);
            GridTest.FindCell(t, new Cell(1, 0, 0));
            GridTest.FindCell(t, new Cell(100, -99, 0));
            GridTest.FindCell(t, new Cell(100, -99, 0));
            GridTest.FindCell(t, new Cell(0, 100, -99));
            GridTest.FindCell(t, new Cell(5, 4, -7));
            GridTest.FindCell(t, new Cell(100, -49, -49));
        }

        [Test]
        [TestCase(TriangleOrientation.FlatTopped)]
        public void TestGetCellsIntersectsApprox(TriangleOrientation orientation)
        {
            var t = new TriangleGrid(1, orientation);

            var p = t.GetCellCenter(new Cell(0, 1, 0));
            CollectionAssert.AreEquivalent(
                new[] { new Cell(0, 1, 0) },
                t.GetCellsIntersectsApprox(p, p));
            CollectionAssert.AreEquivalent(
                new[] {
                    new Cell(0, 1, 0),
                    new Cell(0, 2, 0),
                },
                t.GetCellsIntersectsApprox(p, p + new Vector3(0, 0.3f, 0)));
            CollectionAssert.AreEquivalent(
                new[] {
                    new Cell(0, 1, 0),
                    new Cell(1, 1, 0),
                    new Cell(1, 1, -1),
                    new Cell(0, 2, 0),
                    new Cell(0, 2, -1),
                    new Cell(1, 2, -1),
                },
                t.GetCellsIntersectsApprox(p, p + new Vector3(0.6f, 0.3f, 0)));
        }


        [Test]
        [TestCase(TriangleOrientation.FlatTopped)]
        [TestCase(TriangleOrientation.FlatSides)]
        public void TestFindBasicPath(TriangleOrientation orientation)
        {
            var t = new TriangleGrid(1, orientation);

            GridTest.FindBasicPath(t, new Cell(1, 0, 1), new Cell(10, -5, -4));
        }



        [Test]
        [TestCase(TriangleOrientation.FlatTopped)]
        [TestCase(TriangleOrientation.FlatSides)]
        public void TestFindGridSymmetry(TriangleOrientation orientation)
        {
            var g = new TriangleGrid(1, orientation);
            GridTest.FindGridSymmetry(g, new Cell(0, 0, 0));
        }
    }
}
