using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sylves
{
    /// <summary>
    /// A bounding box on a regular 2d grid of squares.
    /// </summary>
    public class SquareBound : IBound, IEnumerable<Cell>
    {
        /// <summary>
        /// Inclusive lower bound for each coordinate
        /// </summary>
        public Vector2Int min;
        
        /// <summary>
        /// Exclusive upper bound for each coordinate
        /// </summary>
        public Vector2Int max;

        public SquareBound(Vector2Int min, Vector2Int max)
        {
            this.min = min;
            this.max = max;
        }
        public SquareBound(int minX, int minY, int maxX, int maxY)
        {
            this.min = new Vector2Int(minX, minY);
            this.max = new Vector2Int(maxX, maxY);
        }

        public Vector2Int size => max - min;

        public bool Contains(Vector2Int v)
        {
            return v.x >= min.x && v.y >= min.y && v.x < max.x && v.y < max.y;
        }
        public bool Contains(Cell v)
        {
            return v.x >= min.x && v.y >= min.y && v.x < max.x && v.y < max.y;
        }

        public SquareBound Intersect(SquareBound other)
        {
            return new SquareBound(Vector2Int.Max(min, other.min), Vector2Int.Min(max, other.max));
        }
        public SquareBound Union(SquareBound other)
        {
            return new SquareBound(Vector2Int.Min(min, other.min), Vector2Int.Max(max, other.max));
        }

        public int IndexCount
        {
            get
            {
                return size.x * size.y;
            }
        }

        public int GetIndex(Vector2Int cell)
        {
            return (cell.x - min.x) + (cell.y - min.y) * size.x;
        }

        public Vector2Int GetCellByIndex(int index)
        {
            var x = index % size.x;
            var y = index / size.x;
            return new Vector2Int(x + min.x, y + min.y);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<Cell> GetEnumerator()
        {
            for (var x = min.x; x < max.x; x++)
            {
                for (var y = min.y; y < max.y; y++)
                {
                    yield return new Cell(x, y);
                }
            }
        }
    }
}
