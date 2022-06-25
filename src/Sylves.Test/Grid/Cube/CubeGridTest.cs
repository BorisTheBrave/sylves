using NUnit.Framework;
using System.Linq;
#if UNITY
using UnityEngine;
#endif


namespace Sylves.Test
{
    [TestFixture]
    public class CubeGridTest
    {
        [Test]
        public void TestTryMoveByOffset()
        {
            var g = new CubeGrid(1);
            GridTest.TryMoveByOffset(g, new Cell());
        }


        [Test]
        public void TestFindCell()
        {
            var g = new CubeGrid(1);
            GridTest.FindCell(g, new Cell(1, 0, -1));
        }

        [Test]
        public void TestFindBasicPath()
        {
            var g = new CubeGrid(1);
            GridTest.FindBasicPath(g, new Cell(0, 0, 0), new Cell(10, 10, 10));
        }

        [Test]
        public void TestRaycast()
        {
            var g = new CubeGrid(1);
            var start = g.GetCellCenter(new Cell(0, 0, 0));
            var end = g.GetCellCenter(new Cell(2, 1, 0));
            var infos = g.Raycast(start, end-start, 1).ToList();
            Assert.AreEqual(new Cell(0, 0, 0), infos[0].cell);
            Assert.AreEqual(null, infos[0].cellDir);
            Assert.AreEqual(new Cell(1, 0, 0), infos[1].cell);
            Assert.AreEqual(CubeDir.Left, (CubeDir?)infos[1].cellDir);
            Assert.AreEqual(new Cell(1, 1, 0), infos[2].cell);
            Assert.AreEqual(CubeDir.Down, (CubeDir?)infos[2].cellDir);
            Assert.AreEqual(new Cell(2, 1, 0), infos[3].cell);
            Assert.AreEqual(CubeDir.Left, (CubeDir?)infos[3].cellDir);
            Assert.AreEqual(4, infos.Count);


            // Test bad direction doesn't break things
            g.Raycast(new Vector3(1.23f, 4.56f, 0), new Vector3(), 1).ToList();
        }

        [Test]
        public void TestGridSymmetry()
        {
            var g = new CubeGrid(1);
            var s = new GridSymmetry
            {
                Src = new Cell(0, 0, 0),
                Dest = new Cell(10, 0, 0),
                Rotation = CubeRotation.RotateXY,
            };
            var success = g.TryApplySymmetry(s, new Cell(0, 0, 0), out var dest, out var r);
            Assert.IsTrue(success);
            Assert.AreEqual(new Cell(10, 0, 0), dest);
            Assert.AreEqual(s.Rotation, r);

            success = g.TryApplySymmetry(s, new Cell(3, 0, 0), out dest, out r);
            Assert.IsTrue(success);
            Assert.AreEqual(new Cell(10, 3, 0), dest);
            Assert.AreEqual(s.Rotation, r);
        }

        [Test]
        public void TryApplySymmetry()
        {
            var g = new CubeGrid(1);
            var s = new GridSymmetry
            {
                Src = new Cell(),
                Dest = new Cell(),
                Rotation = CubeRotation.Identity,
            };
            var b = new CubeBound(Vector3Int.zero, new Vector3Int(6, 1, 7));
            var success = g.TryApplySymmetry(s, b, out var b2);
            Assert.IsTrue(success);
            Assert.AreEqual(new Vector3Int(6, 1, 7), ((CubeBound)b2).max);
        }

        [Test]
        public void TestFindGridSymmetry()
        {
            var g = new CubeGrid(1);
            GridTest.FindGridSymmetry(g, new Cell(0, 0, 0));
        }
    }
}
