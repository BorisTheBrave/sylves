using System;
using System.Collections.Generic;
using System.Text;
#if UNITY
using UnityEngine;
#endif

namespace Sylves
{
    public class TrianglePrismGrid : PlanarPrismModifier
    {
        public TrianglePrismGrid(float cellSize, float layerHeight) :
            base(
                new BijectModifier(new TriangleGrid(new Vector2(cellSize, cellSize)), ToTriangleGrid, FromTriangleGrid),
                        new PlanarPrismOptions { LayerHeight = layerHeight })
        {

        }


        public TrianglePrismGrid(Vector3 cellSize) :
            base(
                new BijectModifier(new TriangleGrid(new Vector2(cellSize.x, cellSize.y)), ToTriangleGrid, FromTriangleGrid),
                        new PlanarPrismOptions {LayerHeight = cellSize.z })
        {

        }

        private static Cell ToTriangleGrid(Cell c)
        {
            var odd = (c.x & 1);
            var x = (c.x - odd) / 2;
            var y = c.y;
            var z = -x - y + 1 + odd;
            return new Cell(x, y, z);
        }

        private static Cell FromTriangleGrid(Cell c) => new Cell(c.x * 2 + (c.x + c.y + c.z - 1), c.y, 0);
    }
}
