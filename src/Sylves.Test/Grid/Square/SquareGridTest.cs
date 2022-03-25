using NUnit.Framework;
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
        public void TestFindBasicPath()
        {
            var g = new SquareGrid(1);
            GridTest.FindBasicPath(g, new Cell(0, 0, 0), new Cell(10, 10, 0));
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
    }
}
