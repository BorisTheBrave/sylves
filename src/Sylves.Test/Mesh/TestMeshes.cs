#if UNITY
using UnityEngine;
#endif

namespace Sylves.Test
{
    // TODO: Double check these all have sensible normals / winding
    internal static class TestMeshes
    {
        public static MeshData PlaneXY = new MeshData
        {
            indices = new[] { new[] { 0, 1, 2, 3, } },
            vertices = new[]
                {
                    new Vector3(0.5f, -0.5f, 0.0f),
                    new Vector3(0.5f, 0.5f, 0.0f),
                    new Vector3(-0.5f, 0.5f, 0.0f),
                    new Vector3(-0.5f, -0.5f, 0.0f),
                },
            normals = new[]
                {
                    Vector3.forward,
                    Vector3.forward,
                    Vector3.forward,
                    Vector3.forward,
                },
            subMeshCount = 1,
            topologies = new[] { MeshTopology.Quads }
        };

        public static MeshData PlaneXZ = new MeshData
        {
            indices = new[] { new[] { 0, 1, 2, 3, } },
            vertices = new[]
                {
                    new Vector3(0.5f, 0.0f, -0.5f),
                    new Vector3(0.5f, 0.0f, 0.5f),
                    new Vector3(-0.5f, 0.0f, 0.5f),
                    new Vector3(-0.5f, 0.0f, -0.5f),
                },
            normals = new[]
                {
                    Vector3.up,
                    Vector3.up,
                    Vector3.up,
                    Vector3.up,
                },
            subMeshCount = 1,
            topologies = new[] { MeshTopology.Quads }
        };

        public static MeshData Cube
        {
            get
            {
                var meshData = new MeshData();
                Vector3[] vertices = {
                    new Vector3 (-0.5f, -0.5f, -0.5f),
                    new Vector3 (+0.5f, -0.5f, -0.5f),
                    new Vector3 (+0.5f, +0.5f, -0.5f),
                    new Vector3 (-0.5f, +0.5f, -0.5f),
                    new Vector3 (-0.5f, +0.5f, +0.5f),
                    new Vector3 (+0.5f, +0.5f, +0.5f),
                    new Vector3 (+0.5f, -0.5f, +0.5f),
                    new Vector3 (-0.5f, -0.5f, +0.5f),
                };
                int[] quads = {
                    0, 3, 2, 1,
                    2, 3, 4, 5,
                    1, 2, 5, 6,
                    0, 7, 4, 3,
                    5, 4, 7, 6,
                    0, 1, 6, 7,
                };

                meshData.subMeshCount = 1;
                meshData.vertices = vertices;
                meshData.indices = new[] { quads };
                meshData.topologies = new[] { MeshTopology.Quads };
                meshData.RecalculateNormals();

                return meshData;
            }
        }
    }
}
