using UnityEngine;


namespace Sylves
{
    /// <summary>
    /// Variant of TrianglePrismGrid that places triangles in the XZ Plane
    /// </summary>
    public class XZTrianglePrismGrid : XZModifier
    {
        public XZTrianglePrismGrid(float cellSize, float layerHeight, TriangleOrientation orientation = TriangleOrientation.FlatTopped, TrianglePrismBound bound = null)
            : base(new TrianglePrismGrid(cellSize, layerHeight, orientation, bound))
        {

        }
        public XZTrianglePrismGrid(Vector3 cellSize, TriangleOrientation orientation = TriangleOrientation.FlatTopped, TrianglePrismBound bound = null)
            : base(new TrianglePrismGrid(cellSize, orientation, bound))
        {

        }
    }
}
