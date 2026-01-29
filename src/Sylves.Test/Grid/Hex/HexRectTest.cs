using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
#if UNITY
using UnityEngine;
#endif


namespace Sylves.Test
{
    [TestFixture]
    public class HexRectTest
    {
        [Test]
        [TestCase(HexOrientation.PointyTopped, HexRectStagger.Even)]
        [TestCase(HexOrientation.PointyTopped, HexRectStagger.Odd)]
        [TestCase(HexOrientation.FlatTopped, HexRectStagger.Even)]
        [TestCase(HexOrientation.FlatTopped, HexRectStagger.Odd)]
        public void TestBottomLeftMapsToOrigin(HexOrientation orientation, HexRectStagger stagger)
        {
            var rect = new HexRect
            {
                Orientation = orientation,
                Stagger = stagger,
                BottomLeft = new Cell(5, 10, -15),
                Width = 3,
                Height = 4
            };

            var cartesian = rect.ToCartesian(rect.BottomLeft);
            Assert.AreEqual(0, cartesian.X);
            Assert.AreEqual(0, cartesian.Y);
        }

        [Test]
        [TestCase(HexOrientation.PointyTopped, HexRectStagger.Even)]
        [TestCase(HexOrientation.PointyTopped, HexRectStagger.Odd)]
        [TestCase(HexOrientation.FlatTopped, HexRectStagger.Even)]
        [TestCase(HexOrientation.FlatTopped, HexRectStagger.Odd)]
        public void TestRoundTripCartesian(HexOrientation orientation, HexRectStagger stagger)
        {
            var rect = new HexRect
            {
                Orientation = orientation,
                Stagger = stagger,
                BottomLeft = new Cell(0, 0, 0),
                Width = 5,
                Height = 5
            };

            // Test round-trip for all cells in the rectangle
            for (int x = 0; x < rect.Width; x++)
            {
                for (int y = 0; y < rect.Height; y++)
                {
                    var cell = rect.FromCartesian(x, y);
                    var (cartX, cartY) = rect.ToCartesian(cell);
                    Assert.AreEqual(x, cartX, $"Round-trip failed for cartesian ({x}, {y})");
                    Assert.AreEqual(y, cartY, $"Round-trip failed for cartesian ({x}, {y})");
                }
            }
        }

        [Test]
        [TestCase(HexOrientation.PointyTopped, HexRectStagger.Even)]
        [TestCase(HexOrientation.PointyTopped, HexRectStagger.Odd)]
        [TestCase(HexOrientation.FlatTopped, HexRectStagger.Even)]
        [TestCase(HexOrientation.FlatTopped, HexRectStagger.Odd)]
        public void TestContains(HexOrientation orientation, HexRectStagger stagger)
        {
            var rect = new HexRect
            {
                Orientation = orientation,
                Stagger = stagger,
                BottomLeft = new Cell(0, 0, 0),
                Width = 3,
                Height = 4
            };

            // Test cells that should be contained
            var allCells = rect.ToList();
            foreach (var cell in allCells)
            {
                Assert.IsTrue(rect.Contains(cell), $"Cell {cell} should be contained in rectangle");
            }

            // Test boundary cells
            var bottomLeft = rect.FromCartesian(0, 0);
            var topRight = rect.FromCartesian(rect.Width - 1, rect.Height - 1);
            Assert.IsTrue(rect.Contains(bottomLeft));
            Assert.IsTrue(rect.Contains(topRight));

            // Test cells that should NOT be contained
            var outsideCell1 = rect.FromCartesian(-1, 0);
            var outsideCell2 = rect.FromCartesian(0, -1);
            var outsideCell3 = rect.FromCartesian(rect.Width, 0);
            var outsideCell4 = rect.FromCartesian(0, rect.Height);
            var outsideCell5 = rect.FromCartesian(rect.Width, rect.Height);

            Assert.IsFalse(rect.Contains(outsideCell1), $"Cell {outsideCell1} should not be contained");
            Assert.IsFalse(rect.Contains(outsideCell2), $"Cell {outsideCell2} should not be contained");
            Assert.IsFalse(rect.Contains(outsideCell3), $"Cell {outsideCell3} should not be contained");
            Assert.IsFalse(rect.Contains(outsideCell4), $"Cell {outsideCell4} should not be contained");
            Assert.IsFalse(rect.Contains(outsideCell5), $"Cell {outsideCell5} should not be contained");
        }

        [Test]
        [TestCase(HexOrientation.PointyTopped, HexRectStagger.Even)]
        [TestCase(HexOrientation.PointyTopped, HexRectStagger.Odd)]
        [TestCase(HexOrientation.FlatTopped, HexRectStagger.Even)]
        [TestCase(HexOrientation.FlatTopped, HexRectStagger.Odd)]
        public void TestCount(HexOrientation orientation, HexRectStagger stagger)
        {
            var rect = new HexRect
            {
                Orientation = orientation,
                Stagger = stagger,
                BottomLeft = new Cell(0, 0, 0),
                Width = 3,
                Height = 4
            };

            Assert.AreEqual(12, rect.Count);
            Assert.AreEqual(rect.Width * rect.Height, rect.Count);

            var enumeratedCount = rect.Count();
            Assert.AreEqual(rect.Count, enumeratedCount);
        }

        [Test]
        [TestCase(HexOrientation.PointyTopped, HexRectStagger.Even)]
        [TestCase(HexOrientation.PointyTopped, HexRectStagger.Odd)]
        [TestCase(HexOrientation.FlatTopped, HexRectStagger.Even)]
        [TestCase(HexOrientation.FlatTopped, HexRectStagger.Odd)]
        public void TestGetEnumerator(HexOrientation orientation, HexRectStagger stagger)
        {
            var rect = new HexRect
            {
                Orientation = orientation,
                Stagger = stagger,
                BottomLeft = new Cell(0, 0, 0),
                Width = 3,
                Height = 2
            };

            var cells = rect.ToList();
            Assert.AreEqual(6, cells.Count);

            // Verify all cells are unique
            var uniqueCells = cells.Distinct().ToList();
            Assert.AreEqual(cells.Count, uniqueCells.Count, "All cells should be unique");

            // Verify all cells are contained
            foreach (var cell in cells)
            {
                Assert.IsTrue(rect.Contains(cell), $"Enumerated cell {cell} should be contained");
            }
        }
    }
}
