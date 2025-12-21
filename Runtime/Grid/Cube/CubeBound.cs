using System;
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

        public CubeBound(Vector3Int min, Vector3Int mex)
        {
            this.Min = min;
            this.Mex = mex;
        }

        public static CubeBound FromVectors(IEnumerable<Vector3Int> vs)
        {
            var enumerator = vs.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                throw new Exception($"Enumerator empty");
            }
            var min = enumerator.Current;
            var max = min;
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                min = Vector3Int.Min(min, current);
                max = Vector3Int.Max(max, current);
            }
            return new CubeBound(min, max + Vector3Int.one);
        }

        public Vector3Int Size => Mex - Min;

        public bool Contains(Cell v)
        {
            return v.x >= Min.x && v.y >= Min.y && v.z >= Min.z && v.x < Mex.x && v.y < Mex.y && v.z < Mex.z;
        }

        public CubeBound Intersect(CubeBound other)
        {
            return new CubeBound(Vector3Int.Max(Min, other.Min), Vector3Int.Min(Mex, other.Mex));
        }
        public CubeBound Union(CubeBound other)
        {
            return new CubeBound(Vector3Int.Min(Min, other.Min), Vector3Int.Max(Mex, other.Mex));
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<Cell> GetEnumerator()
        {
            for (var x = Min.x; x < Mex.x; x++)
            {
                for (var y = Min.y; y < Mex.y; y++)
                {
                    for (var z = Min.z; z < Mex.z; z++)
                    {
                        yield return new Cell(x, y, z);
                    }
                }
            }
        }
    }
}
