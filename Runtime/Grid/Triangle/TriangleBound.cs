using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sylves
{
    public class TriangleBound : IBound, IEnumerable<Cell>
    {
        public Vector3Int Min { get; set; }
        public Vector3Int Mex { get; set; }

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

        public TriangleBound(Vector3Int min, Vector3Int mex)
        {
            this.Min = min;
            this.Mex = mex;
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
            return v.x >= Min.x && v.y >= Min.y && v.z >= Min.z && v.x < Mex.x && v.y < Mex.y && v.z < Mex.z;
        }

        public TriangleBound Intersect(TriangleBound other)
        {
            return new TriangleBound(Vector3Int.Max(Min, other.Min), Vector3Int.Min(Mex, other.Mex));
        }
        public TriangleBound Union(TriangleBound other)
        {
            return new TriangleBound(Vector3Int.Min(Min, other.Min), Vector3Int.Max(Mex, other.Mex));
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
                var maxY = Math.Min(Mex.y, -x - Min.z + 3);
                for (var y = minY; y < maxY; y++)
                {
                    for (var s = 1; s <= 2; s++)
                    {
                        var z = -x - y + s;
                        if (Min.z <= z && z < Mex.z)
                        {
                            yield return new Cell(x, y, z);
                        }
                    }
                }
            }
        }
    }
}
