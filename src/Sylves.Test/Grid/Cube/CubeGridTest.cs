using NUnit.Framework;
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
