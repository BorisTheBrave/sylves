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

        public void MoveForward()
        {
            if(!Grid.TryMove(Cell, Dir, out var dest, out var inverseDir, out var connection))
            {
                throw new Exception($"Walker cannot move forward as forward cell doesn't exist, {this}");
            }
            // TODO: Handle connection?
            var cellType = Grid.GetCellType(dest);
            var nextDir = cellType.Invert(inverseDir);
            if(nextDir == null)
            {
                throw new Exception($"Walker cannot move forward as forward cell has no opposing dir, {this}");
            }
            Cell = dest;
            Dir = nextDir.Value;
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
