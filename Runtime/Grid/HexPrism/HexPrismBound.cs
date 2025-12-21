using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Sylves
{
    // Specialized version of PlanarPrismBound
    public class HexPrismBound : IBound, IEnumerable<Cell>
    {
        public HexBound HexBound { get; set; }

        public int LayerMin { get; set; }

        public int LayerMex { get; set; }


        /// <summary>
        /// Inclusive upper bound for layer
        /// </summary>
        public int LayerMax
        {
            get
            {
                return LayerMex - 1;
            }
            set
            {
                LayerMex = value + 1;
            }
        }

        public HexPrismBound(HexBound hexBound, int layerMin, int layerMex)
        {
            this.HexBound = hexBound;
            this.LayerMin = layerMin;
            this.LayerMex = layerMex;
        }

        public bool Contains(Cell v)
        {
            return HexBound.Contains(HexPrismGrid.GetHexCell(v)) && LayerMin <= v.z && v.z < LayerMex;
        }

        public HexPrismBound Intersect(HexPrismBound other)
        {
            return new HexPrismBound(HexBound.Intersect(other.HexBound), Math.Max(LayerMin, other.LayerMin), Math.Min(LayerMex, other.LayerMex));
        }
        public HexPrismBound Union(HexPrismBound other)
        {
            return new HexPrismBound(HexBound.Union(other.HexBound), Math.Min(LayerMin, other.LayerMin), Math.Max(LayerMex, other.LayerMex));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<Cell> GetEnumerator()
        {
            foreach(var hex in HexBound)
            {
                for(var z=LayerMin;z<LayerMex;z++)
                {
                    yield return HexPrismGrid.GetHexPrismCell(hex, z);
                }
            }
        }
    }
}
