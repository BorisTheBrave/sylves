using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY
using UnityEngine;
#endif


namespace Sylves
{
    public enum HexRectStagger
    {
        Even,
        Odd,
    }

    /// <summary>
    /// Utility class for representing a rectangular collection of cells in a hexagonal grid
    /// </summary>
    public class HexRect : ISet<Cell>
    {
        public HexOrientation Orientation;

        public HexRectStagger Stagger;

        public Cell BottomLeft;

        public int Width;
        public int Height;

        /// <summary>
        /// Returns the cartesian position of this cell in the rectangle.
        /// GetCartesian(BottomLeft) returns (0, 0)
        /// </summary>
        public (int X, int Y) ToCartesian(Cell cell)
        {
            if (Orientation == HexOrientation.PointyTopped)
            {
                var dy = cell.y - BottomLeft.y;
                var dx = cell.x - BottomLeft.x + ((dy + 1 - (int)Stagger) >> 1);
                return (dx, dy);
            }
            else
            {
                var dx = cell.x - BottomLeft.x;
                var dy = cell.y - BottomLeft.y + ((dx + 1 - (int)Stagger) >> 1);
                return (dx, dy);
            }
        }

        public Cell FromCartesian(int x, int y)
        {
            int cellx, celly;
            if (Orientation == HexOrientation.PointyTopped)
            {
                celly = y + BottomLeft.y;
                cellx = x + BottomLeft.x - ((y + 1 - (int)Stagger) >> 1);
            }
            else
            {
                cellx = x + BottomLeft.x;
                celly = y + BottomLeft.y - ((x + 1 - (int)Stagger) >> 1);
            }
            return new Cell(cellx, celly, -cellx-celly);
        }

        public int Count => Width * Height;

        public bool IsReadOnly => true;

        bool ISet<Cell>.Add(Cell item)
        {
            throw new InvalidOperationException();
        }

        public bool Contains(Cell cell)
        {
            var (x, y) = ToCartesian(cell);
            return 0 <= x && x < Width && 0 <= y && y < Height;
        }

        public void ExceptWith(IEnumerable<Cell> other)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<Cell> GetEnumerator()
        {
            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    yield return FromCartesian(x, y);
                }
            }
        }

        void ISet<Cell>.IntersectWith(IEnumerable<Cell> other)
        {
            throw new InvalidOperationException();
        }

        bool ISet<Cell>.IsProperSubsetOf(IEnumerable<Cell> other)
        {
            throw new NotImplementedException();
        }

        bool ISet<Cell>.IsProperSupersetOf(IEnumerable<Cell> other)
        {
            throw new NotImplementedException();
        }

        bool ISet<Cell>.IsSubsetOf(IEnumerable<Cell> other)
        {
            throw new NotImplementedException();
        }

        bool ISet<Cell>.IsSupersetOf(IEnumerable<Cell> other)
        {
            throw new NotImplementedException();
        }

        bool ISet<Cell>.Overlaps(IEnumerable<Cell> other)
        {
            throw new NotImplementedException();
        }

        public bool SetEquals(IEnumerable<Cell> other)
        {
            throw new NotImplementedException();
        }

        public void SymmetricExceptWith(IEnumerable<Cell> other)
        {
            throw new InvalidOperationException();
        }

        public void UnionWith(IEnumerable<Cell> other)
        {
            throw new InvalidOperationException();
        }

        void ICollection<Cell>.Add(Cell item)
        {
            throw new InvalidOperationException();
        }

        void ICollection<Cell>.Clear()
        {
            throw new InvalidOperationException();
        }

        void ICollection<Cell>.CopyTo(Cell[] array, int arrayIndex)
        {
            foreach(var cell in this)
            {
                array[arrayIndex++] = cell;
            }
        }

        bool ICollection<Cell>.Remove(Cell item)
        {
            throw new InvalidOperationException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}