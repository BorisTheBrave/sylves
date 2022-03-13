using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sylves.Test
{
    [TestFixture]
    internal class LangtonTutorial
    {
        [Test]
        [Ignore("Tutorial code")]
        public void Main()
        {
            var grid = new SquareGrid(1);
            var blackCells = new HashSet<Cell>();
            var startCell = new Cell(0, 0);
            var startDir = SquareDir.Up;
            var ant = new Walker(grid, startCell, (CellDir)startDir);

            void Step()
            {
                Console.WriteLine($"Ant is at {ant.Cell} and is facing {(SquareDir)ant.Dir}");
                ant.MoveForward();
                var isCurrentCellBlack = blackCells.Contains(ant.Cell);
                if (isCurrentCellBlack)
                {
                    ant.TurnRight();
                    blackCells.Remove(ant.Cell);
                }
                else
                {
                    ant.TurnLeft();
                    blackCells.Add(ant.Cell);
                }
            }

            for (var i = 0; i < 100; i++)
            {
                Step();
            }
        }
    }
}
