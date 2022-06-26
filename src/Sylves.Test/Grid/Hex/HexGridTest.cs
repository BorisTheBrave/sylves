using NUnit.Framework;
using System.Linq;
#if UNITY
using UnityEngine;
#endif


namespace Sylves.Test
{
    [TestFixture]
    public class HexGridTest
    {
        [Test]
        [TestCase(HexOrientation.PointyTopped)]
        [TestCase(HexOrientation.FlatTopped)]
        public void TestTryMoveByOffset(HexOrientation orientation)
        {
            var g = new HexGrid(1, orientation);
            GridTest.TryMoveByOffset(g, new Cell());
        }


        [TestCase(HexOrientation.PointyTopped)]
        [TestCase(HexOrientation.FlatTopped)]
        public void TestFindCell(HexOrientation orientation)
        {
            var g = new HexGrid(new Vector2(1, 1), orientation);
            GridTest.FindCell(g, new Cell(1, 0, -1));
            GridTest.FindCell(g, new Cell(100, -100, 0));
            GridTest.FindCell(g, new Cell(0, 100, -100));
            GridTest.FindCell(g, new Cell(-100, 0 , 100));
            GridTest.FindCell(g, new Cell(100, -50, -50));
        }


        [Test]
        public void TestGetCellsIntersectsApprox()
        {
            var g = new HexGrid(2, HexOrientation.FlatTopped);

            var p = new Vector3(0, 0.57f, 0);
            CollectionAssert.AreEquivalent(
                new[] {
                    new Cell(0, 0, 0),
                    new Cell(1, 0, -1),
                    new Cell(0, 1, -1),
                },
                g.GetCellsIntersectsApprox(p, p + new Vector3(0.6f, 0.3f, 0)));
            
            
            CollectionAssert.AreEquivalent(
                new[] {
                    new Cell(0, 0, 0),
                    new Cell(1, 0, -1),
                    new Cell(0, 1, -1),
                    new Cell(1, 1, -2),
                },
                g.GetCellsIntersectsApprox(p, p + new Vector3(0.6f, 2f, 0)));
            
            g = new HexGrid(2, HexOrientation.PointyTopped);
            p = new Vector3(0.57f, 0, 0);
            CollectionAssert.AreEquivalent(
                new[] {
                    new Cell(0, 0, 0),
                    new Cell(1, 0, -1),
                    new Cell(0, 1, -1),
                },
                g.GetCellsIntersectsApprox(p, p + new Vector3(0.3f, 0.6f, 0)));

            g = new HexGrid(1);
            CollectionAssert.AreEquivalent(
                new[] {
                    new Cell(1, -1, 0), 
                    new Cell(0, 0, 0),
                    new Cell(1, 0, -1),
                },
                g.GetCellsIntersectsApprox(new Vector3(0.28f, -0.36f, 0.00f), new Vector3(0.60f, -0.16f, 0.00f)));

        }


        [Test]
        [TestCase(HexOrientation.PointyTopped)]
        [TestCase(HexOrientation.FlatTopped)]
        public void TestFindBasicPath(HexOrientation orientation)
        {
            var g = new HexGrid(new Vector2(1, 1), orientation);

            GridTest.FindBasicPath(g, new Cell(1, 0, -1), new Cell(10, -5, -5));
        }

        [Test]
        public void TestRaycast()
        {
            var g = new HexGrid(1, HexOrientation.FlatTopped);
            var start = g.GetCellCenter(new Cell(0, 0, 0));
            var end = g.GetCellCenter(new Cell(-1, 1, 0));
            var infos = g.Raycast(start, end - start, 1).ToList();
            Assert.AreEqual(new Cell(0, 0, 0), infos[0].cell);
            Assert.AreEqual(null, infos[0].cellDir);
            Assert.AreEqual(new Cell(-1, 1, 0), infos[1].cell);
            Assert.AreEqual(FTHexDir.DownRight, (FTHexDir?)infos[1].cellDir);
            Assert.AreEqual(2, infos.Count);
        }

        [Test]
        public void TestGridSymmetry()
        {
            var g = new HexGrid(1);
            var s = new GridSymmetry
            {
                Src = new Cell(0, 0, 0),
                Dest = new Cell(10, -10, 0),
                Rotation = HexRotation.RotateCCW,
            };
            var success = g.TryApplySymmetry(s, new Cell(0, 0, 0), out var dest, out var r);
            Assert.IsTrue(success);
            Assert.AreEqual(new Cell(10, -10, 0), dest);
            Assert.AreEqual(s.Rotation, r);

            success = g.TryApplySymmetry(s, new Cell(3, 0, -3), out dest, out r);
            Assert.IsTrue(success);
            Assert.AreEqual(new Cell(10, -7, -3), dest);
            Assert.AreEqual(s.Rotation, r);
        }

        [Test]
        public void TestFindGridSymmetry()
        {
            var g = new HexGrid(1);
            GridTest.FindGridSymmetry(g, new Cell(0, 0, 0));
        }
    }
}
