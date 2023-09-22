using System;
using System.Linq;
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

		public static bool[] Bools = new[] { false, true };

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
		public void TestDiff()
		{
			var g = new PenroseRhombGrid();
			Assert.AreEqual(2, g.GetPathDiffLength(new Cell(0b00111111, 0), new Cell(0b00110101, 0)));
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

		private IGrid MakeRaw(SubstitutionTilingGrid g, bool isRaw) => isRaw ? g.Raw : g;

        [Test]
        [TestCaseSource(nameof(Bools))]
        public void TestTryMove_PenroseRhomb(bool isRaw)
        {
            var g = MakeRaw(new PenroseRhombGrid(), isRaw);
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
			FindCell(g, new Cell(0, 0));
			FindCell(g, new Cell(1, 0));
			FindCell(g, new Cell(4, 0));
			FindCell(g, new Cell(10, 0));
			FindCell(g, new Cell(100, 0));
			FindCell(g, new Cell(1000, 0));
        }

		[Test]
		public void TestGetCellsIntersectsApprox()
		{
            var g = new PenroseRhombGrid();
			{
				var min = new Vector3(0.001f, 0.001f, 0);
				var max = new Vector3(5, 0.001f, 0);
				var cells = g.GetCellsIntersectsApprox(min, max).ToList();
				Assert.Contains(new Cell(0, 0, 0), cells);
				Assert.Contains(new Cell(10, 0, 0), cells);
				Assert.Contains(new Cell(164, 0, 0), cells);
				Assert.Contains(new Cell(160, 0, 0), cells);
			}
			{
				var min = new Vector3(0.001f, 0.001f, 0);
				var max = new Vector3(5.001f, 0.001f, 0);
				var cells = g.GetCellsIntersectsApprox(min, max).ToList();
				Assert.Contains(new Cell(170, 0, 0), cells);
			}

        }

        [Test]
		public void TestRaycast()
		{
			var g = new PenroseRhombGrid();

			var s = new Vector3(0.001f, 0.001f, 0);

            GridTest.Raycast(g, s, new Vector3(0.001f, 0, 0), 1);

            GridTest.Raycast(g, s, new Vector3(10, 0, 0), 1);
		}

		[Test]
		public void TestBounds()
		{
			var g = new DominoGrid();

			var b1 = (SubstitutionTilingBound)g.GetBound(new[] { new Cell(0, 0, 0), new Cell(1, 0, 0) });
			Assert.AreEqual(0, b1.Height);
			Assert.AreEqual(new Cell(), b1.Path);
			Assert.AreEqual(4, g.GetCellsInBounds(b1).Count());

            var b2 = (SubstitutionTilingBound)g.GetBound(new[] { new Cell(0, 0, 0), new Cell(4, 0, 0) });
            Assert.AreEqual(1, b2.Height);
            Assert.AreEqual(new Cell(), b2.Path);

            var b3 = (SubstitutionTilingBound)g.GetBound(new[] { new Cell(103, 0, 0)});
            Assert.AreEqual(0, b3.Height);
            Assert.AreEqual(new Cell(100, 0, 0), b3.Path);

			Assert.AreEqual(b1, g.IntersectBounds(b1, b2));
			Assert.AreEqual(0, g.GetCellsInBounds(g.IntersectBounds(b1, b3)).Count());

			{
				var b = (SubstitutionTilingBound)g.UnionBounds(b1, b2);
				Assert.AreEqual(b2.Height, b.Height);
				Assert.AreEqual(b2.Path, b.Path);
            }
        }

		[Test]
        public void TestIndex()
		{
			var g = new PenroseRhombGrid().BoundBy(new SubstitutionTilingBound { Height = 3, Path = new Cell(0b0101000000, 0)});
			Assert.AreEqual(g.GetCells().Count(), g.IndexCount);
			foreach(var cell in g.GetCells())
			{
				Assert.AreEqual(cell, g.GetCellByIndex(g.GetIndex(cell)), $"Round trip failed for {cell}");
            }
        }
    }
}

