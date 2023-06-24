using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Sylves
{
    /// <summary>
    /// Periodic 2d grid of pentagons.
    /// https://en.wikipedia.org/wiki/Cairo_pentagonal_tiling
    /// This is an specialization of <see cref="PeriodicPlanarMeshGrid"/>.
    /// </summary>
    public class CairoGrid : PeriodicPlanarMeshGrid
    {
        private static float o = Mathf.Sqrt(3) / 2 + 0.5f;

        public CairoGrid():base(CairoMeshData(), new Vector2(o, o), new Vector2(-o, o))
        {

        }

        private static readonly Vector3 v0 = new Vector3(0, -1.36602533333333f, 0);
        private static readonly Vector3 v1 = new Vector3(0.6830125f,   -0.3943375f, 0);
        private static readonly Vector3 v2 = new Vector3(0,   -0.788675f, 0);
        private static readonly Vector3 v3 = new Vector3(0.288675f,    0.288675f, 0);
        private static readonly Vector3 v4 = new Vector3(-0.6830125f,  -0.3943375f, 0);
        private static readonly Vector3 v5 = new Vector3(-0.288675f,   0.288675f, 0);

        private static MeshData CairoMeshData()
        {
            var meshData = new MeshData();
            // TODO: Remove duplicates?
            meshData.vertices = new Vector3[]
            {
                v0,
                v1,
                v2,
                v3,
                v4,
                v5,
                v0 + new Vector3(o, o, 0),
                v1 + new Vector3(o, o, 0),
                v2 + new Vector3(o, o, 0),
                v3 + new Vector3(o, o, 0),
                v4 + new Vector3(o, o, 0),
                v5 + new Vector3(o, o, 0),
                v0 + new Vector3(-o, o, 0),
                v1 + new Vector3(-o, o, 0),
                v2 + new Vector3(-o, o, 0),
                v3 + new Vector3(-o, o, 0),
                v4 + new Vector3(-o, o, 0),
                v5 + new Vector3(-o, o, 0),
                v0 + new Vector3(0, 2 * o, 0),
                v1 + new Vector3(0, 2 * o, 0),
                v2 + new Vector3(0, 2 * o, 0),
                v3 + new Vector3(0, 2 * o, 0),
                v4 + new Vector3(0, 2 * o, 0),
                v5 + new Vector3(0, 2 * o, 0),

            };
            meshData.indices = new[]{new []
            {
                1, 3, 5, 4, ~2,
                6, 8, 10, 3, ~1,
                13, 14, 12, 4, ~5,
                10, 18, 13, 5, ~3,
            } };
            meshData.topologies = new[] { MeshTopology.NGon };
            return meshData;
        }

    }
}
