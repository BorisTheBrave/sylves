using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sylves
{
    public class TriangleBound : IBound, IEnumerable<Cell>
    {
        public Vector3Int min;
        public Vector3Int max;
        public TriangleBound(Vector3Int min, Vector3Int max)
        {
            this.min = min;
            this.max = max;
        }

        public static TriangleBound Hexagon(int radius)
        {
            var center = new Vector3Int(0, 0, 0);
            return new TriangleBound(
                new Vector3Int(center.x - radius + 1, center.y - radius + 1, center.z - radius + 1),
                new Vector3Int(center.x + radius + 1, center.y + radius + 1, center.z + radius + 1)
                );
        }

        public bool Contains(Cell v)
        {
            return v.x >= min.x && v.y >= min.y && v.z >= min.z && v.x < max.x && v.y < max.y && v.z < max.z;
        }

        public TriangleBound Intersect(TriangleBound other)
        {
            return new TriangleBound(Vector3Int.Max(min, other.min), Vector3Int.Min(max, other.max));
        }
        public TriangleBound Union(TriangleBound other)
        {
            return new TriangleBound(Vector3Int.Min(min, other.min), Vector3Int.Max(max, other.max));
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
                var maxY = Math.Min(max.y, -x - min.z + 3);
                for (var y = minY; y < maxY; y++)
                {
                    for (var s = 1; s <= 2; s++)
                    {
                        var z = -x - y + s;
                        if (min.z <= z && z < max.z)
                        {
                            yield return new Cell(x, y, z);
                        }
                    }
                }
            }
        }
    }
}
