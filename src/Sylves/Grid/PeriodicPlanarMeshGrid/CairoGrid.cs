using System;
using System.Collections.Generic;
using System.Text;
#if UNITY
using UnityEngine;
#endif

namespace Sylves
{
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
                v0 + new Vector3(o, o),
                v1 + new Vector3(o, o),
                v2 + new Vector3(o, o),
                v3 + new Vector3(o, o),
                v4 + new Vector3(o, o),
                v5 + new Vector3(o, o),
                v0 + new Vector3(-o, o),
                v1 + new Vector3(-o, o),
                v2 + new Vector3(-o, o),
                v3 + new Vector3(-o, o),
                v4 + new Vector3(-o, o),
                v5 + new Vector3(-o, o),
                v0 + new Vector3(0, 2 * o),
                v1 + new Vector3(0, 2 * o),
                v2 + new Vector3(0, 2 * o),
                v3 + new Vector3(0, 2 * o),
                v4 + new Vector3(0, 2 * o),
                v5 + new Vector3(0, 2 * o),

            };
            meshData.indices = new[]{new []
            {
               2, 4, 5, 3, ~1,
               1, 3, 10, 8, ~6,
               5, 4, 12, 14, ~13,
               3, 5, 13, 18, ~10,
            } };
            meshData.subMeshCount = 1;
            meshData.topologies = new[] { MeshTopology.NGon };
            return meshData;
        }

    }
}
