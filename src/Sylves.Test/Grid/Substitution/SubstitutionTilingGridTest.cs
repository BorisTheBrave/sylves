using System;
using NUnit.Framework;
#if UNITY
using UnityEngine;
#endif

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
			var l = (96 - 2) / 2;
			var cell2 = new Cell();

			cell2 = g.SetChildTileAt(cell2, g.GetChildTileAt(cell));
			for(var i= 0;i < l;i++)
			{
                cell2 = g.SetPathAt(cell2, i, g.GetPathAt(cell, i));

            }

			Assert.AreEqual(cell, cell2);

		}

		[Test]
		public void TestSetPathWithNoChildBits()
		{
			Assert.AreEqual(new Cell(2, 0, 0), new PenroseRhombGrid().SetPathAt(new Cell(), 0, 2));
        }

        [Test]
		public void TestTryMove_Domino()
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



        [Test]
        public void TestTryMove_PenroseRhomb()
        {
            var g = new PenroseRhombGrid();
			CollectionAssert.AreEquivalent(new[]{
				new Cell(1, 0), new Cell(10, 0), new Cell(5, 0), new Cell(4, 0)
			}, g.GetNeighbours(new Cell(0, 0, 0)));


            CollectionAssert.AreEquivalent(new[]{
                new Cell(40, 0), new Cell(84, 0), new Cell(5, 0), new Cell(2, 0)
            }, g.GetNeighbours(new Cell(41, 0, 0)));
        }

        [Test]
		public void FindCell_Domino()
		{
            // TODO: Replace with GridTest.FindCell once more operations are supported
            void FindCell(IGrid grid, Cell cell)
			{
				var center = grid.GetCellCenter(cell);
                var success = grid.FindCell(center, out var cell2);
				Assert.IsTrue(success);
				Assert.AreEqual(cell, cell2);
			}
            var g = new DominoGrid();
			//FindCell(g, new Cell(0, 0));
			//FindCell(g, new Cell(1, 0));
			//FindCell(g, new Cell(4, 0));
			FindCell(g, new Cell(10, 0));
			FindCell(g, new Cell(100, 0));
			FindCell(g, new Cell(1000, 0));
        }
    }
}

