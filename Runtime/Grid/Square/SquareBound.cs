using System;
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
        public Vector2Int Min { get; set; }
        
        /// <summary>
        /// Exclusive upper bound for each coordinate
        /// </summary>
        public Vector2Int Mex { get; set; }

        /// <summary>
        /// Inclusive upper bound for each coordinate
        /// </summary>
        public Vector2Int Max
        {
            get
            {
                return Mex - Vector2Int.one;
            }
            set
            {
                Mex = value + Vector2Int.one;
            }
        }

        public SquareBound(Vector2Int min, Vector2Int mex)
        {
            this.Min = min;
            this.Mex = mex;
        }
        public SquareBound(int minX, int minY, int maxX, int maxY)
        {
            this.Min = new Vector2Int(minX, minY);
            this.Mex = new Vector2Int(maxX, maxY);
        }
        public static SquareBound FromVectors(IEnumerable<Vector2Int> vs)
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
                min = Vector2Int.Min(min, current);
                max = Vector2Int.Max(max, current);
            }
            return new SquareBound(min, max + Vector2Int.one);
        }

        public Vector2Int Size => Mex - Min;

        public bool Contains(Vector2Int v)
        {
            return v.x >= Min.x && v.y >= Min.y && v.x < Mex.x && v.y < Mex.y;
        }
        public bool Contains(Cell v)
        {
            return v.x >= Min.x && v.y >= Min.y && v.x < Mex.x && v.y < Mex.y;
        }

        public SquareBound Intersect(SquareBound other)
        {
            return new SquareBound(Vector2Int.Max(Min, other.Min), Vector2Int.Min(Mex, other.Mex));
        }
        public SquareBound Union(SquareBound other)
        {
            return new SquareBound(Vector2Int.Min(Min, other.Min), Vector2Int.Max(Mex, other.Mex));
        }

        public int IndexCount
        {
            get
            {
                return Size.x * Size.y;
            }
        }

        public int GetIndex(Vector2Int cell)
        {
            return (cell.x - Min.x) + (cell.y - Min.y) * Size.x;
        }

        public Vector2Int GetCellByIndex(int index)
        {
            var x = index % Size.x;
            var y = index / Size.x;
            return new Vector2Int(x + Min.x, y + Min.y);
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
                    yield return new Cell(x, y);
                }
            }
        }
    }
}
