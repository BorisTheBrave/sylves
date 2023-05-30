using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
#if UNITY
using UnityEngine;
#endif


namespace Sylves.Test
{
    [TestFixture]
    internal class SquareGridTest
    {
        [Test]
        public void TestTryMoveByOffset()
        {
            var g = new SquareGrid(1);
            GridTest.TryMoveByOffset(g, new Cell());
        }

        [Test]
        public void TestFindCell()
        {
            var g = new SquareGrid(1);
            GridTest.FindCell(g, new Cell());
        }

        [Test]
        public void TestFindCellScaled()
        {
            var g = new SquareGrid(new Vector2(30, 30));
            var c = g.FindCell(new Vector3(20.1f, -13.1f, -3.4f));
            Assert.AreEqual(new Cell(0, -1, 0), c);
        }

        [Test]
        public void TestFindBasicPath()
        {
            var g = new SquareGrid(1);
            GridTest.FindBasicPath(g, new Cell(0, 0, 0), new Cell(10, 10, 0));
        }


        [Test]
        public void TestRaycast()
        {
            var g = new SquareGrid(1);
            var start = g.GetCellCenter(new Cell(0, 0, 0));
            var end = g.GetCellCenter(new Cell(2, 1, 0));
            var infos = g.Raycast(start, end - start, 1).ToList();
            Assert.AreEqual(new Cell(0, 0, 0), infos[0].cell);
            Assert.AreEqual(null, infos[0].cellDir);
            Assert.AreEqual(new Cell(1, 0, 0), infos[1].cell);
            Assert.AreEqual(SquareDir.Left, (SquareDir?)infos[1].cellDir);
            Assert.AreEqual(new Cell(1, 1, 0), infos[2].cell);
            Assert.AreEqual(SquareDir.Down, (SquareDir?)infos[2].cellDir);
            Assert.AreEqual(new Cell(2, 1, 0), infos[3].cell);
            Assert.AreEqual(SquareDir.Left, (SquareDir?)infos[3].cellDir);
            Assert.AreEqual(4, infos.Count);


            // Test bad direction doesn't break things
            g.Raycast(new Vector3(-1.23f, 4.56f, 0), new Vector3(), 1).ToList();
        }

        [Test]
        public void TestRaycast2()
        {
            var g = new SquareGrid(1000);
            var start = g.GetCellCenter(new Cell(0, 0, 0));
            var end = g.GetCellCenter(new Cell(2, 1, 0));
            var infos = g.Raycast(start, end - start, 1).ToList();
            Assert.AreEqual(4, infos.Count);
        }

        [Test]
        public void TestGridSymmetry()
        {
            var g = new SquareGrid(1);
            var s = new GridSymmetry
            {
                Src = new Cell(0, 0),
                Dest = new Cell(10, 0),
                Rotation = SquareRotation.Rotate90(1),
            };
            var success = g.TryApplySymmetry(s, new Cell(0, 0), out var dest, out var r);
            Assert.IsTrue(success);
            Assert.AreEqual(new Cell(10, 0), dest);
            Assert.AreEqual(s.Rotation, r);

            success = g.TryApplySymmetry(s, new Cell(3, 0), out dest, out r);
            Assert.IsTrue(success);
            Assert.AreEqual(new Cell(10, 3, 0), dest);
            Assert.AreEqual(s.Rotation, r);
        }



        [Test]
        public void TestFindGridSymmetry()
        {
            var g = new SquareGrid(1);
            GridTest.FindGridSymmetry(g, new Cell(0, 0, 0));

            {
                var cells = new HashSet<Cell>(new[] { new Cell(0, 1, 0) });
                var s = g.FindGridSymmetry(cells, cells, new Cell(0, 1, 0), (CellRotation)2);
                Assert.IsNotNull(s);
                Assert.AreEqual(new Cell(0, 0, 0), s.Src);
                Assert.AreEqual(new Cell(0, 2, 0), s.Dest);
                Assert.AreEqual((CellRotation)2, s.Rotation);
            }
        }

        [Test]
        public void TestDualMapping()
        {
            var g = new SquareGrid(1);
            var dual = g.GetDual();
            GridTest.DualMapping(dual, new Cell(0, 0, 0));
            GridTest.DualMapping(dual, new Cell(8, -4, 0));
        }
    }
}
