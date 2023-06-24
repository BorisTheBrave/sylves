using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sylves
{

    /// <summary>
    /// A bounding box on a regular 2d grid of squares.
    /// </summary>
    public class CubeBound : IBound, IEnumerable<Cell>
    {
        /// <summary>
        /// Inclusive lower bound for each coordinate
        /// </summary>
        public Vector3Int min;

        /// <summary>
        /// Exclusive upper bound for each coordinate
        /// </summary>
        public Vector3Int max;

        public CubeBound(Vector3Int min, Vector3Int max)
        {
            this.min = min;
            this.max = max;
        }

        public Vector3Int size => max - min;

        public bool Contains(Cell v)
        {
            return v.x >= min.x && v.y >= min.y && v.z >= min.z && v.x < max.x && v.y < max.y && v.z < max.z;
        }

        public CubeBound Intersect(CubeBound other)
        {
            return new CubeBound(Vector3Int.Max(min, other.min), Vector3Int.Min(max, other.max));
        }
        public CubeBound Union(CubeBound other)
        {
            return new CubeBound(Vector3Int.Min(min, other.min), Vector3Int.Max(max, other.max));
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
                    for (var z = min.z; z < max.z; z++)
                    {
                        yield return new Cell(x, y, z);
                    }
                }
            }
        }
    }
}
