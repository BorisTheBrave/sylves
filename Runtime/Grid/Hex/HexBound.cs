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
        public Vector3Int Min { get; set; }
        
        /// <summary>
        /// Exclusive upper bound for each coordinate
        /// </summary>
        public Vector3Int Mex { get; set; }

        /// <summary>
        /// Inclusive upper bound for each coordinate
        /// </summary>
        public Vector3Int Max
        {
            get
            {
                return Mex - Vector3Int.one;
            }
            set
            {
                Mex = value + Vector3Int.one;
            }
        }

        public HexBound(Vector3Int min, Vector3Int mex)
        {
            this.Min = min;
            this.Mex = mex;
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
            return v.x >= Min.x && v.y >= Min.y && v.z >= Min.z && v.x < Mex.x && v.y < Mex.y && v.z < Mex.z;
        }

        public HexBound Intersect(HexBound other)
        {
            return new HexBound(Vector3Int.Max(Min, other.Min), Vector3Int.Min(Mex, other.Mex));
        }
        public HexBound Union(HexBound other)
        {
            return new HexBound(Vector3Int.Min(Min, other.Min), Vector3Int.Max(Mex, other.Mex));
        }

        internal IEnumerable<Cell> GetCorners()
        {
            {
                var x = Min.x;
                var minY = Math.Max(Min.y, -x - Mex.z + 1);
                var maxY = Math.Min(Mex.y, -x - Min.z + 1) - 1;
                if (minY <= maxY)
                {
                    yield return new Cell(x, minY, -x - minY);
                    yield return new Cell(x, maxY, -x - maxY);
                }
            }
            {
                var x = Mex.x - 1;
                var minY = Math.Max(Min.y, -x - Mex.z + 1);
                var maxY = Math.Min(Mex.y, -x - Min.z + 1) - 1;
                if (minY <= maxY)
                {
                    yield return new Cell(x, minY, -x - minY);
                    yield return new Cell(x, maxY, -x - maxY);
                }
            }
            {
                var y = Min.y;
                var minX = Math.Max(Min.x, -y - Mex.z + 1);
                var maxX = Math.Min(Mex.x, -y - Min.z + 1) - 1;
                if (minX <= maxX)
                {
                    yield return new Cell(minX, y, -y - minX);
                    yield return new Cell(maxX, y, -y - maxX);
                }
            }
            {
                var y = Mex.y - 1;
                var minX = Math.Max(Min.x, -y - Mex.z + 1);
                var maxX = Math.Min(Mex.x, -y - Min.z + 1) - 1;
                if (minX <= maxX)
                {
                    yield return new Cell(minX, y, -y - minX);
                    yield return new Cell(maxX, y, -y - maxX);
                }
            }
            // No need to do z, every corner must be min/max of 2 co-ords
            // so one of x or y must be relevant
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<Cell> GetEnumerator()
        {
            for (var x = Min.x; x < Mex.x; x++)
            {
                var minY = Math.Max(Min.y, -x - Mex.z + 1);
                var maxY = Math.Min(Mex.y, -x - Min.z + 1);
                for (var y = minY; y < maxY; y++)
                {
                    yield return new Cell(x, y, -x - y);
                }
            }
        }
    }
}
