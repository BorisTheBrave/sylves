using System;
using System.Collections.Generic;
using System.Text;

namespace Sylves
{
    public class Walker
    {

        public Walker(IGrid grid, Cell cell, CellDir dir)
        {
            Grid = grid;
            Cell = cell;
            Dir = dir;
        }

        public IGrid Grid { get; set; }

        public Cell Cell { get; set; }

        public CellDir Dir { get; set; }

        /// <summary>
        /// Moves the walker one step in the current direction, updating Cell and Dir.
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void MoveForward()
        {
            if (!Grid.TryMove(Cell, Dir, out var dest, out var inverseDir, out var connection))
            {
                throw new Exception($"Walker cannot move forward as forward cell doesn't exist, {this}");
            }
            // TODO: Handle connection?
            var cellType = Grid.GetCellType(dest);
            var nextDir = cellType.Invert(inverseDir);
            if (nextDir == null)
            {
                throw new Exception($"Walker cannot move forward as forward cell has no opposing dir, {this}");
            }
            Cell = dest;
            Dir = nextDir.Value;
        }

        /// <summary>
        /// As MoveForward, but handles the case of odd sided polygons by picking either the 
        /// left or right direction.
        /// </summary>
        public void MoveForward(bool bearLeft)
        {
            if (!Grid.TryMove(Cell, Dir, out var dest, out var inverseDir, out var connection))
            {
                throw new Exception($"Walker cannot move forward as forward cell doesn't exist, {this}");
            }
            // TODO: Handle connection?
            var cellType = Grid.GetCellType(dest);
            if (NGonCellType.Extract(cellType) is int n)
            {
                var nextDir = (int)inverseDir + (n / 2) + (bearLeft && (n % 2 == 1 ) ? 1 : 0);

                Cell = dest;
                Dir = (CellDir)(nextDir % n);
            }
            else
            {
                var nextDir = cellType.Invert(inverseDir);
                if (nextDir == null)
                {
                    throw new Exception($"Walker cannot move forward as forward cell has no opposing dir, {this}");
                }
                Cell = dest;
                Dir = nextDir.Value;
            }
        }

        public void TurnLeft()
        {
            var cellType = Grid.GetCellType(Cell);
            Dir = cellType.Rotate(Dir, cellType.RotateCCW);
        }

        public void TurnRight()
        {
            var cellType = Grid.GetCellType(Cell);
            Dir = cellType.Rotate(Dir, cellType.RotateCW);
        }

        public override string ToString() => $"(Cell={Cell}, Dir={Dir})";
    }
}
