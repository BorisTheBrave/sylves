using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Sylves
{
    // Specialized version of PlanarPrismBound
    public class HexPrismBound : IBound, IEnumerable<Cell>
    {
        public HexBound hexBound;

        public int layerMin;

        public int layerMax;

        public HexPrismBound(HexBound hexBound, int layerMin, int layerMax)
        {
            this.hexBound = hexBound;
            this.layerMin = layerMin;
            this.layerMax = layerMax;
        }

        public bool Contains(Cell v)
        {
            return hexBound.Contains(HexPrismGrid.GetHexCell(v)) && layerMin <= v.z && v.z < layerMax;
        }

        public HexPrismBound Intersect(HexPrismBound other)
        {
            return new HexPrismBound(hexBound.Intersect(other.hexBound), Math.Max(layerMin, other.layerMin), Math.Min(layerMax, other.layerMax));
        }
        public HexPrismBound Union(HexPrismBound other)
        {
            return new HexPrismBound(hexBound.Union(other.hexBound), Math.Min(layerMin, other.layerMin), Math.Max(layerMax, other.layerMax));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<Cell> GetEnumerator()
        {
            foreach(var hex in hexBound)
            {
                for(var z=layerMin;z<layerMax;z++)
                {
                    yield return HexPrismGrid.GetHexPrismCell(hex, z);
                }
            }
        }
    }
}
