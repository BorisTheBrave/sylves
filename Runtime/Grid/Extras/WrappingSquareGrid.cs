using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

using static Sylves.MathUtils;

namespace Sylves
{
    /// <summary>
    /// WrapModifier applied to SquareGrid. This is a very common grid in games. 
    /// </summary>
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

        // TODO: Could do a better job on bounds?
    }
}
