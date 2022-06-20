using System;
using System.Collections.Generic;
using System.Text;
#if UNITY
using UnityEngine;
#endif

namespace Sylves
{
    public class TriHexGrid : PeriodicPlanarMeshGrid
    {

        public TriHexGrid():base(TriHexMeshData(), new Vector2(1.0f, 0), new Vector2(0.5f, 1.0f))
        {

        }
        private static MeshData TriHexMeshData()
        {
            var meshData = new MeshData();
            meshData.vertices = new Vector3[]
            {
                new Vector3(0.5f, 0, 0),
                new Vector3(0.25f, 0.5f, 0),
                new Vector3(-0.25f, 0.5f, 0),
                new Vector3(-0.5f, 0, 0),
                new Vector3(-0.25f, -0.5f, 0),
                new Vector3(0.25f, -0.5f, 0),
                new Vector3(0.75f, 0.5f, 0),
                new Vector3(0.75f, -0.5f, 0),
            };
            meshData.indices = new[]{new []
            {
                0, 1, 2, 3, 4, ~5,
                6, 1, ~0,
                7, 0, ~5,
            } };
            meshData.subMeshCount = 1;
            meshData.topologies = new[] { MeshTopology.NGon };
            return meshData;
        }

    }
}
