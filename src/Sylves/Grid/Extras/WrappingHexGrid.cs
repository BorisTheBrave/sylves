using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
#if UNITY
using UnityEngine;
#endif

using static Sylves.MathUtils;

namespace Sylves
{
    /// <summary>
    /// WrapModifier applied to HexGrid.
    /// </summary>
    public class WrappingHexGrid : WrapModifier
    {
        public WrappingHexGrid(float cellSize, int radius, HexOrientation orientation = HexOrientation.PointyTopped)
            :this(HexGrid.ComputeCellSize(cellSize, orientation), radius, orientation)
        { }

        public WrappingHexGrid(Vector2 cellSize, int radius, HexOrientation orientation = HexOrientation.PointyTopped) 
            : base(
                  new HexGrid(cellSize, orientation, HexBound.Hexagon(radius)), 
                  c => Canonicalize(c, radius))
        {
        }

        private static Cell Canonicalize(Cell c, int r)
        {
            // See https://observablehq.com/@sanderevers/hexagon-tiling-of-an-hexagonal-grid
            int shift = 3 * r + 2;
            int area = r * shift + r + 1;

            var x = c.x;
            var y = c.y;
            var z = c.z;
            // Computes the co-ordinates of the big hex
            // this cell is in
            // https://observablehq.com/@sanderevers/hexagon-tiling-of-an-hexagonal-grid
            var xh = Mathf.FloorToInt((y + shift * x) / area);
            var yh = Mathf.FloorToInt((z + shift * y) / area);
            var zh = Mathf.FloorToInt((x + shift * z) / area);
            var i = Mathf.FloorToInt((1 + xh - yh) / 3f);
            var j = Mathf.FloorToInt((1 + yh - zh) / 3f);
            //var k = Mathf.FloorToInt((1 + zh - xh) / 3f);
            // Translates that hex back onto the central one
            x -= (2 * r + 1) * i + r * j;
            y -= -r * i + (r + 1) * j;
            z = -x - y;
            return new Cell(x, y, z);
        }
    }
}
