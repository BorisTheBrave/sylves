using System;
using NUnit.Framework;

namespace Sylves.Test
{
	[TestFixture]
	public class SubstitutionTilingGridTest
	{
		public static Cell[] Cells = new[]
		{
			new Cell(0, 0, 0),
			new Cell(1, 0, 0),
			new Cell(4, 0, 0),
			new Cell(17, 0, 0),
			new Cell(-1, 0, 0),
			new Cell(0, 1, 0),
			new Cell(0, 0, 1),
		};

		[Test]
		[TestCaseSource(nameof(Cells))]
		public void TestRoundTripParse(Cell cell)
		{
			var g = new DominoGrid();
			var (childTile, path) = g.Parse(cell);

			Assert.AreEqual(cell, g.Format(childTile, path));
		}

		[Test]
		public void TestTryMove()
		{
			var g = new DominoGrid();
			bool r;
			Cell dest;
			CellDir inverseDir;
			Connection connection;
			// tile interior
			r = g.TryMove(new Cell(0, 0, 0), (CellDir)0, out dest, out inverseDir, out connection);
			Assert.IsTrue(r);
			Assert.AreEqual(new Cell(1, 0, 0), dest);
			Assert.AreEqual((CellDir)4, inverseDir);

			// tile exterior, but no prototile heirarch
			r = g.TryMove(new Cell(0, 0, 0), (CellDir)4, out dest, out inverseDir, out connection);
            Assert.IsTrue(r);
            Assert.AreEqual(new Cell(5, 0, 0), dest);
            Assert.AreEqual((CellDir)0, inverseDir);

            // tile exterior, also prototile exterior
            r = g.TryMove(new Cell(1, 0, 0), (CellDir)0, out dest, out inverseDir, out connection);
            Assert.IsTrue(r);
            Assert.AreEqual(new Cell(20, 0, 0), dest);
            Assert.AreEqual((CellDir)4, inverseDir);

        }
    }
}

