using System;
using System.Collections;
using System.Collections.Generic;

namespace Sylves
{
    // Specialized version of PlanarPrismBound
    public class TrianglePrismBound : IBound, IEnumerable<Cell>
    {
        public TriangleBound triangleBound;

        public int layerMin;

        public int layerMax;

        public TrianglePrismBound(TriangleBound triangleBound, int layerMin, int layerMax)
        {
            this.triangleBound = triangleBound;
            this.layerMin = layerMin;
            this.layerMax = layerMax;
        }

        public bool Contains(Cell v)
        {
            return triangleBound.Contains(HexPrismGrid.GetHexCell(v)) && layerMin <= v.z && v.z < layerMax;
        }

        public TrianglePrismBound Intersect(TrianglePrismBound other)
        {
            return new TrianglePrismBound(triangleBound.Intersect(other.triangleBound), Math.Max(layerMin, other.layerMin), Math.Min(layerMax, other.layerMax));
        }
        public TrianglePrismBound Union(TrianglePrismBound other)
        {
            return new TrianglePrismBound(triangleBound.Union(other.triangleBound), Math.Min(layerMin, other.layerMin), Math.Max(layerMax, other.layerMax));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<Cell> GetEnumerator()
        {
            foreach (var hex in triangleBound)
            {
                for (var z = layerMin; z < layerMax; z++)
                {
                    yield return HexPrismGrid.GetHexPrismCell(hex, z);
                }
            }
        }
    }
}
