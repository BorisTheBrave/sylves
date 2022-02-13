using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Sylves
{
    /// <summary>
    /// Bounding boxes for cube coordinate hexes.
    /// This can represent rhombuses and hex shapes drawn on the hex grid.
    /// </summary>
    public class HexBound : IBound, IEnumerable<Cell>
    {

        /// <summary>
        /// Inclusive lower bound for each coordinate
        /// </summary>
        public Vector3Int min;
        
        /// <summary>
        /// Exclusive upper bound for each coordinate
        /// </summary>
        public Vector3Int max;
        public HexBound(Vector3Int min, Vector3Int max)
        {
            this.min = min;
            this.max = max;
        }

        public bool Contains(Cell v)
        {
            return v.x >= min.x && v.y >= min.y && v.z >= min.z && v.x < max.x && v.y < max.y && v.z < max.z;
        }

        public HexBound Intersect(HexBound other)
        {
            return new HexBound(Vector3Int.Max(min, other.min), Vector3Int.Min(max, other.max));
        }
        public HexBound Union(HexBound other)
        {
            return new HexBound(Vector3Int.Min(min, other.min), Vector3Int.Max(max, other.max));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<Cell> GetEnumerator()
        {
            for (var x = min.x; x < max.x; x++)
            {
                var minY = Math.Max(min.y, -x - max.z + 1);
                var maxY = Math.Min(max.y, -x - min.z);
                for (var y = minY; y < maxY; y++)
                {
                    yield return new Cell(x, y, -x - y);
                }
            }
        }
    }
}
