using NUnit.Framework;
using System;
#if UNITY
using UnityEngine;
#endif

namespace Sylves.Test
{
    [TestFixture]
    internal class WrapModifierTest
    {
        private WrapModifier GetGrid(int gridType)
        {
            switch (gridType)
            {
                case 0:
                    return new WrappingSquareGrid(1, new Vector2Int(10, 10));
                default:
                    throw new Exception();
            }
        }

        private static readonly int[] GridTypes = { 0 };

        // Checks TryMoveByOffset gives same results as CellType.Roate
        [Test]
        [TestCaseSource(nameof(GridTypes))]
        public void TestTryMoveByOffset(int gridType)
        {
            var g = GetGrid(gridType);
            GridTest.TryMoveByOffset(g, new Cell());
        }


        [Test]
        [TestCaseSource(nameof(GridTypes))]
        public void TestFindCell(int gridType)
        {
            var g = GetGrid(gridType);
            GridTest.FindCell(g, new Cell());
        }


        [Test]
        [TestCaseSource(nameof(GridTypes))]
        public void TestFindBasicPath(int gridType)
        {
            var g = GetGrid(gridType);
            GridTest.FindBasicPath(g, new Cell(0, 0), new Cell(2, 2));
        }
    }
}
