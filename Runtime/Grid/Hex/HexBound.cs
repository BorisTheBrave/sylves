using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        /// <summary>
        /// Returns a bound of a rhombus that bounds the X and Y axes.
        /// </summary>
        public static HexBound Rhombus(int minX, int minY, int maxX, int maxY)
        {
            return new HexBound(new Vector3Int(minX, minY, -maxX - maxY + 1), new Vector3Int(maxX, maxY, -minX - minY + 1));
        }


        /// <summary>
        /// Returns a rough hexagonal shape of cells with a given distance of center, inclusive.
        /// I.e. a radius 0 hexagon contains 1 cell, a radius 1 hexagon contains 7 cells.
        /// </summary>
        public static HexBound Hexagon(int radius, Cell center = new Cell())
        {
            return new HexBound(
                new Vector3Int(center.x - radius, center.y - radius, center.z - radius),
                new Vector3Int(center.x + radius + 1, center.y + radius + 1, center.z + radius + 1)
                );
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
                var maxY = Math.Min(max.y, -x - min.z + 1);
                for (var y = minY; y < maxY; y++)
                {
                    yield return new Cell(x, y, -x - y);
                }
            }
        }
    }
}
