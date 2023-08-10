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
	}
}

