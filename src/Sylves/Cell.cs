using System;
using System.Collections.Generic;
using System.Text;

namespace Sylves
{
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
    }
}
