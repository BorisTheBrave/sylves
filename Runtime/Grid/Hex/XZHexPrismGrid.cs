using UnityEngine;


namespace Sylves
{
    /// <summary>
    /// Variant of HexPrismGrid that places hexes in the XZ Plane
    /// </summary>
    public class XZHexPrismGrid : XZModifier
    {
        public XZHexPrismGrid(float cellSize, float layerHeight, HexOrientation orientation = HexOrientation.PointyTopped, HexPrismBound bound = null)
            : base(new HexPrismGrid(cellSize, layerHeight, orientation, bound))
        {

        }
        public XZHexPrismGrid(Vector3 cellSize, HexOrientation orientation = HexOrientation.PointyTopped, HexPrismBound bound = null)
            : base(new HexPrismGrid(cellSize, orientation, bound))
        {

        }
    }
}
