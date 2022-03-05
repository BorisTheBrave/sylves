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
        public void TestRotation()
        {
            var h = new SquareGrid(Vector2.one);
            GridTest.TryMoveByOffset(h, new Cell());
        }

        [Test]
        public void TestFindBasicPath()
        {
            var g = new SquareGrid(1);
            GridTest.FindBasicPath(g, new Cell(0, 0, 0), new Cell(10, 10, 0));
        }
    }
}
