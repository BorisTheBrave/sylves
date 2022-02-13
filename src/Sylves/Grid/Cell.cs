using System;
using System.Collections.Generic;
using System.Text;

namespace Sylves
{
    /// <summary>
    /// Represents a single cell in the grid.
    /// Cell is just a set of co-ordinates, the grid itself must be called to get any details about the cell.
    /// For more details see the basic concepts in the docs.
    /// </summary>
    public struct Cell
    {
        public Cell(int x, int y)
        {
            this.x = x;
            this.y = y;
            this.z = 0;
        }

        public Cell(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public int x;
        public int y;
        public int z;

        public static Cell operator +(Cell cell, Vector3Int offset)
        {
            return new Cell(cell.x + offset.x, cell.y + offset.y, cell.z + offset.z);
        }

        public static Cell operator +(Vector3Int offset, Cell cell)
        {
            return new Cell(cell.x + offset.x, cell.y + offset.y, cell.z + offset.z);
        }

        public static explicit operator Vector3Int(Cell c) => new Vector3Int(c.x, c.y, c.z);
        public static explicit operator Cell(Vector3Int c) => new Cell(c.x, c.y, c.z);

        public override string ToString() => $"({x}, {y}, {z})";
    }
}
