using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sylves.Test
{
    [TestFixture]
    internal class OutlineCellsTest
    {
        [Test]
        public void OutlineSingleCell()
        {
            var g = new SquareGrid(1);

            var outlines = OutlineCells.Outline(g, new[] { new Cell() }.ToHashSet()).ToList();
            var outline = outlines.Single();
            Assert.IsTrue(outline.IsLoop);
            CollectionAssert.AreEqual(new[]
            {
                (new Cell(), (CellDir)SquareDir.Right),
                (new Cell(), (CellDir)SquareDir.Up),
                (new Cell(), (CellDir)SquareDir.Left),
                (new Cell(), (CellDir)SquareDir.Down),
            }, outline.Edges);
        }

        [Test]
        public void OutlineTwoCell()
        {
            var g = new SquareGrid(1);

            var outlines = OutlineCells.Outline(g, new[] { new Cell(0, 0), new Cell(-1, 0) }.ToHashSet()).ToList();
            var outline = outlines.Single();
            Assert.IsTrue(outline.IsLoop);
            CollectionAssert.AreEqual(new[]
            {
                (new Cell(0, 0), (CellDir)SquareDir.Right),
                (new Cell(0, 0), (CellDir)SquareDir.Up),
                (new Cell(-1, 0), (CellDir)SquareDir.Up),
                (new Cell(-1, 0), (CellDir)SquareDir.Left),
                (new Cell(-1, 0), (CellDir)SquareDir.Down),
                (new Cell(0, 0), (CellDir)SquareDir.Down),
            }, outline.Edges);
        }


        [Test]
        public void OutlineArcs()
        {
            var g = new SquareGrid(1).Masked(c => c.x >= -1 && c.x <= 0);

            var outlines = OutlineCells.Outline(g, new[] { new Cell(0, 0), new Cell(-1, 0) }.ToHashSet(), includeGridBorder: false).ToList();
            Assert.AreEqual(2, outlines.Count);
            var outline = outlines[0];
            Assert.IsTrue(outline.IsArc);
            CollectionAssert.AreEqual(new[]
            {
                (new Cell(0, 0), (CellDir)SquareDir.Up),
                (new Cell(-1, 0), (CellDir)SquareDir.Up),
            }, outline.Edges);
            outline = outlines[1];
            Assert.IsTrue(outline.IsArc);
            CollectionAssert.AreEqual(new[]
            {
                (new Cell(-1, 0), (CellDir)SquareDir.Down),
                (new Cell(0, 0), (CellDir)SquareDir.Down),
            }, outline.Edges);
        }

        [Test]
        public void OutlineGridBorder()
        {
            var g = new SquareGrid(1).Masked(c => c.x >= -1 && c.x <= 0);

            var outlines = OutlineCells.Outline(g, new[] { new Cell(0, 0), new Cell(-1, 0) }.ToHashSet(), includeGridBorder: true).ToList();
            var outline = outlines.Single();
            Assert.IsTrue(outline.IsLoop);
            CollectionAssert.AreEqual(new[]
            {
                (new Cell(0, 0), (CellDir)SquareDir.Right),
                (new Cell(0, 0), (CellDir)SquareDir.Up),
                (new Cell(-1, 0), (CellDir)SquareDir.Up),
                (new Cell(-1, 0), (CellDir)SquareDir.Left),
                (new Cell(-1, 0), (CellDir)SquareDir.Down),
                (new Cell(0, 0), (CellDir)SquareDir.Down),
            }, outline.Edges);
        }
    }
}
