using System;
using System.Collections.Generic;
using System.Text;
#if UNITY
using UnityEngine;
#endif

namespace Sylves
{
    // A hex grid which each hex subdivded into 12 right angle trianges.
    public class MetaHexagonGrid : PeriodicPlanarMeshGrid
    {
        public MetaHexagonGrid():base(MetaHexagonGridMeshData(), new Vector2(-0.4330127f, 0.75f), new Vector2(-0.8660254f, 0))
        {

        }
        private static MeshData MetaHexagonGridMeshData()
        {
            var meshData = new MeshData();
            // TODO: Remove duplicates?
            meshData.vertices = new Vector3[]
            {
                0.5f * new Vector3(0, 0, 0),
                0.5f * new Vector3(0.000000f, -1.000000f, 0),
                0.5f * new Vector3(-0.866025f, -0.500000f, 0),
                0.5f * new Vector3(-0.866025f, 0.500000f, 0),
                0.5f * new Vector3(0.000000f, 1.000000f, 0),
                0.5f * new Vector3(0.866025f, 0.500000f, 0),
                0.5f * new Vector3(0.866025f, -0.500000f, 0),
                0.5f * new Vector3(-0.433013f, -0.750000f, 0),
                0.5f * new Vector3(-0.866025f, 0.000000f, 0),
                0.5f * new Vector3(-0.433013f, 0.750000f, 0),
                0.5f * new Vector3(0.433013f, 0.750000f, 0),
                0.5f * new Vector3(0.866025f, 0.000000f, 0),
                0.5f * new Vector3(0.433013f, -0.750000f, 0),
                0.5f * new Vector3(0.000000f, 0.000000f, 0),
            };
            meshData.indices = new[]{new []
            {
                3, 8, 13,
                11, 5, 13,
                1, 12, 13,
                8, 2, 13,
                4, 9, 13,
                12, 6, 13,
                9, 3, 13,
                5, 10, 13,
                2, 7, 13,
                10, 4, 13,
                6, 11, 13,
                7, 1, 13,
            } };
            meshData.subMeshCount = 1;
            meshData.topologies = new[] { MeshTopology.Triangles };
            return meshData;
        }

    }
}
