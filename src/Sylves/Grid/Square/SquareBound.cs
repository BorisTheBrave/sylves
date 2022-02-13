using System.Collections;
using System.Collections.Generic;

namespace Sylves
{
    public class SquareBound : IBound, IEnumerable<Cell>
    {
        public Vector2Int min;
        public Vector2Int max;

        public SquareBound(Vector2Int min, Vector2Int max)
        {
            this.min = min;
            this.max = max;
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
