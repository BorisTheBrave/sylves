using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
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
        public void TestRaycast()
        {
            var g = new TriangleGrid(1);
            var start = g.GetCellCenter(new Cell(1, 0, 1));
            var end = g.GetCellCenter(new Cell(2, 0, 0));
            var infos = g.Raycast(start, end - start, 1).ToList();
            Assert.AreEqual(new Cell(1, 0, 1), infos[0].cell);
            Assert.AreEqual(null, infos[0].cellDir);
            Assert.AreEqual(new Cell(1, 0, 0), infos[1].cell);
            Assert.AreEqual(FTHexDir.DownLeft, (FTHexDir?)infos[1].cellDir);
            Assert.AreEqual(new Cell(2, 0, 0), infos[2].cell);
            Assert.AreEqual(FTHexDir.UpLeft, (FTHexDir?)infos[2].cellDir);
            Assert.AreEqual(3, infos.Count);
            
            // Test bad direction doesn't break things by looping infinitely
            g.Raycast(new Vector3(1.23f, 4.56f, 0), new Vector3(), 1).ToList();

            // Test corner case doesn't break things
            var cells = g.Raycast(new Vector3(0, 0, 0), new Vector3(2, 0, 0), 1).Select(x=>x.cell).ToList();
            Assert.IsTrue(cells.All(c => c.x + c.y + c.z == 2 || c.x + c.y + c.z == 1), string.Join(",", cells));
        }

        [Test]
        [TestCase(TriangleOrientation.FlatTopped)]
        [TestCase(TriangleOrientation.FlatSides)]
        public void TestFindGridSymmetry(TriangleOrientation orientation)
        {
            var g = new TriangleGrid(1, orientation);
            GridTest.FindGridSymmetry(g, new Cell(0, 0, 1));

            {
                var cells = new HashSet<Cell>(new[] { new Cell(0, 0, 1) });
                var s = g.FindGridSymmetry(cells, cells, new Cell(0, 0, 1), (CellRotation)2);
                Assert.IsNotNull(s);
                Assert.AreEqual(new Cell(0, 0, 1), s.Src);
                Assert.AreEqual(new Cell(0, 0, 1), s.Dest);
                Assert.AreEqual((CellRotation)2, s.Rotation);
            }

            {
                var cells = new HashSet<Cell>(new[] { new Cell(0, 0, 1) });
                var s = g.FindGridSymmetry(cells, cells, new Cell(0, 0, 1), (CellRotation)1);
                // This symmetry is not possible as it maps a tile onto itself at a 60 degree rotation
                Assert.IsNull(s);
            }

            {
                var cells = new HashSet<Cell>(new[] { new Cell(0, 0, 1) });
                var cells2 = new HashSet<Cell>(new[] { new Cell(0, 1, 1) });
                var s = g.FindGridSymmetry(cells, cells2, new Cell(0, 0, 1), (CellRotation)0);
                // This symmetry is not possible as it maps up tiles onto down tiles and vv
                Assert.IsNull(s);
            }
        }


        [Test]
        public void TestGridSymmetry()
        {
            var g = new TriangleGrid(1);
            var s = new GridSymmetry
            {
                Src = new Cell(0, 0, 1),
                Dest = new Cell(0, 0, 1),
                Rotation = HexRotation.Rotate60(2),
            };
            var success = g.TryApplySymmetry(s, new Cell(0, 0, 1), out var dest, out var r);
            Assert.IsTrue(success);
            Assert.AreEqual(new Cell(0, 0, 1), dest);
            Assert.AreEqual(s.Rotation, r);
        }


        [Test]
        public void TestBound()
        {
            var size = new Vector3Int(10, 0, 10);
            var b = new TriangleBound(new Vector3Int(-size.x - size.z + 1, 0, 0), new Vector3Int(3, size.x, size.z));
            var cells = new HashSet<Cell>(b);
            // Check the corners
            Assert.IsTrue(b.Contains(new Cell(1, 0, 0)));
            Assert.IsTrue(b.Contains(new Cell(2, 0, 0)));
            Assert.IsTrue(b.Contains(new Cell(1 - (size.x - 1) - (size.z - 1), size.x - 1, size.z - 1)));
            Assert.IsTrue(b.Contains(new Cell(2 - (size.x - 1) - (size.z - 1), size.x - 1, size.z - 1)));
            // Contains matches GetEnumerator
            for (var x = -size.x - size.z - 10; x < size.x + 10; x++)
            {
                for (var y = -size.x - size.z - 10; y < size.y + 10; y++)
                {
                    for (var z = -size.x - size.z - 10; z < size.z + 10; z++)
                    {
                        var s = x + y + z;
                        if (s == 1 || s == 2)
                        {
                            var cell = new Cell(x, y, z);
                            Assert.AreEqual(b.Contains(cell), cells.Contains(cell));
                        }
                    }
                }
            }
            Assert.AreEqual(size.x * size.z * 2, b.Count());
        }

        [Test]
        public void TestDualMapping()
        {
            var g = new TriangleGrid(1, TriangleOrientation.FlatTopped);
            var dual = g.GetDual();
            GridTest.DualMapping(dual, new Cell(1, 0, 0));
            GridTest.DualMapping(dual, new Cell(1, 1, 0));
            GridTest.DualMapping(dual, new Cell(8, -4, -2));


            g = new TriangleGrid(1, TriangleOrientation.FlatSides);
            dual = g.GetDual();
            GridTest.DualMapping(dual, new Cell(1, 0, 0));
            GridTest.DualMapping(dual, new Cell(1, 1, 0));
            GridTest.DualMapping(dual, new Cell(8, -4, -2));
        }
    }
}
