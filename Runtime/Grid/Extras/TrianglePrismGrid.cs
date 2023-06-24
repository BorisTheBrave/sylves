using System.Text;
using UnityEngine;

namespace Sylves
{
    public class TrianglePrismGrid : PlanarPrismModifier
    {
        public TrianglePrismGrid(float cellSize, float layerHeight, TriangleOrientation orientation = TriangleOrientation.FlatTopped, TrianglePrismBound bound = null) :
            this(TriangleGrid.ComputeCellSize(cellSize, orientation), layerHeight, orientation, bound)
        {

        }


        public TrianglePrismGrid(Vector2 triangleCellSize, float layerHeight, TriangleOrientation orientation = TriangleOrientation.FlatTopped, TrianglePrismBound bound = null) :
            this(new Vector3(triangleCellSize.x, triangleCellSize.y, layerHeight), orientation, bound)
        {

        }

        public TrianglePrismGrid(Vector3 cellSize, TriangleOrientation orientation = TriangleOrientation.FlatTopped, TrianglePrismBound bound = null) :
            base(
                new BijectModifier(new TriangleGrid(new Vector2(cellSize.x, cellSize.y), orientation, bound?.triangleBound), ToTriangleGrid, FromTriangleGrid, 2),
                new PlanarPrismOptions {LayerHeight = cellSize.z },
                bound == null ? null : new PlanarPrismBound { PlanarBound = bound.triangleBound, MinLayer = bound.layerMin, MaxLayer = bound.layerMax }
            )
        {
        }

        internal static Cell ToTriangleGrid(Cell c)
        {
            var odd = c.x & 1;
            var x = (c.x + odd) / 2;
            var y = c.y;
            var z = -x - y + 1 + odd;
            return new Cell(x, y, z);
        }

        internal static Cell FromTriangleGrid(Cell c)
        {
            return new Cell(c.x * 2 - (c.x + c.y + c.z - 1), c.y, 0);
        }

        public override IGrid Unwrapped => this;
    }
}
