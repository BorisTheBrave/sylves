using System;
using System.Diagnostics;
using UnityEngine;

namespace Sylves
{
    /// <summary>
    /// Represents a single cell in the grid.
    /// Cell is just a set of co-ordinates, the grid itself must be called to get any details about the cell.
    /// For more details see the basic concepts in the docs.
    /// </summary>
    public struct Cell : IEquatable<Cell>
    {
        [DebuggerStepThrough]
        public Cell(int x, int y)
        {
            this.x = x;
            this.y = y;
            this.z = 0;
        }

        [DebuggerStepThrough]
        public Cell(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public int x;
        public int y;
        public int z;

        public bool Equals(Cell other) => x == other.x && y == other.y && z == other.z;
        public override bool Equals(object other)
        {
            if (other is Cell v)
                return Equals(v);
            return false;
        }
        public override int GetHashCode() => (x, y, z).GetHashCode();

        public static Cell operator +(Cell cell, Vector3Int offset)
        {
            return new Cell(cell.x + offset.x, cell.y + offset.y, cell.z + offset.z);
        }

        public static Cell operator +(Vector3Int offset, Cell cell)
        {
            return new Cell(cell.x + offset.x, cell.y + offset.y, cell.z + offset.z);
        }

        public static Cell operator -(Cell cell, Vector3Int offset)
        {
            return new Cell(cell.x - offset.x, cell.y - offset.y, cell.z - offset.z);
        }

        public static Cell operator -(Vector3Int offset, Cell cell)
        {
            return new Cell(cell.x - offset.x, cell.y - offset.y, cell.z - offset.z);
        }

        public static explicit operator Vector3Int(Cell c) => new Vector3Int(c.x, c.y, c.z);
        public static explicit operator Cell(Vector3Int c) => new Cell(c.x, c.y, c.z);

        public static bool operator ==(Cell lhs, Cell rhs) => lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z;
        public static bool operator !=(Cell lhs, Cell rhs) => !(lhs == rhs);

        public override string ToString() => $"({x}, {y}, {z})";
    }
}
