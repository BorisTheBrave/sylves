using System;
using System.Collections.Generic;
using System.Text;

namespace Sylves
{
    public static class GridExtensions
    {
        /// <summary>
        /// Returns the cell that is in the given direction from cell, or null if that move is not possible.
        /// </summary>
        public static Cell? Move(this IGrid grid, Cell cell, CellDir dir)
        {
            if(grid.TryMove(cell, dir, out var dest, out var _, out var _))
            {
                return dest;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns all the cells that you can move to from a given cell.
        /// </summary>
        public static IEnumerable<Cell> GetNeighbours(this IGrid grid, Cell cell)
        {
            foreach(var dir in grid.GetCellDirs(cell))
            {
                if(grid.TryMove(cell, dir, out var dest, out var _, out var _))
                {
                    yield return dest;
                }
            }
        }

    }
}
