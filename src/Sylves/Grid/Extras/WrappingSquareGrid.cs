using System;
using System.Collections.Generic;
using System.Text;
#if UNITY
using UnityEngine;
#endif

using static Sylves.MathUtils;

namespace Sylves
{
    public class WrappingSquareGrid : WrapModifier
    {
        public WrappingSquareGrid(float cellSize, Vector2Int size)
            :this(new Vector2(cellSize, cellSize), size)
        { }

        public WrappingSquareGrid(Vector2 cellSize, Vector2Int size) 
            : base(
                  new SquareGrid(cellSize, new SquareBound(Vector2Int.zero, size)), 
                  c => new Cell(PMod(c.x, size.x), PMod(c.y, size.y)))
        {
        }
    }
}
