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
            // Edge 0 points Right.
            // Edge 1 points Up.
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

        // Same as PlaneXY, but with InverseWinding
        // This is how a typical unity plane would look
        public static MeshData PlaneXY_I = new MeshData
        {
            indices = new[] { new[] { 0, 1, 2, 3, } },
            // Edge 0 points Left.
            // Edge 1 points Up.
            vertices = new[]
                {
                    new Vector3(-0.5f, -0.5f, 0.0f),
                    new Vector3(-0.5f, 0.5f, 0.0f),
                    new Vector3(0.5f, 0.5f, 0.0f),
                    new Vector3(0.5f, -0.5f, 0.0f),
                },
            normals = new[]
                {
                    Vector3.back,
                    Vector3.back,
                    Vector3.back,
                    Vector3.back,
                },
            subMeshCount = 1,
            topologies = new[] { MeshTopology.Quads }
        };

        public static MeshData PlaneXZ = new MeshData
        {
            indices = new[] { new[] { 0, 1, 2, 3, } },
            // Edge 0 points Right.
            // Edge 1 points Forward
            // (winds counter clockwise when camera pointing Down, camera up being Forward)
            vertices = new[]
                {
                    new Vector3(0.5f, 0.0f, -0.5f),
                    new Vector3(0.5f, 0.0f, 0.5f),
                    new Vector3(-0.5f, 0.0f, 0.5f),
                    new Vector3(-0.5f, 0.0f, -0.5f),
                },
            normals = new[]
                {
                    Vector3.down,
                    Vector3.down,
                    Vector3.down,
                    Vector3.down,
                },
            subMeshCount = 1,
            topologies = new[] { MeshTopology.Quads }
        };

        // Same as PlaneXY, but with InverseWinding
        // This is how a typical unity plane would look
        public static MeshData PlaneXZ_I = new MeshData
        {
            indices = new[] { new[] { 0, 1, 2, 3, } },
            // Edge 0 points Left.
            // Edge 1 points Forward
            // (winds counter clockwise when camera pointing Down, camera up being Forward)
            vertices = new[]
                {
                    new Vector3(-0.5f, 0.0f, -0.5f),
                    new Vector3(-0.5f, 0.0f, 0.5f),
                    new Vector3(0.5f, 0.0f, 0.5f),
                    new Vector3(0.5f, 0.0f, -0.5f),
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

                // Faces in same order as CubeDir
                // They are arranged so that 2nd edge points Up ( or Forward), matching CubeDir.Up().
                // TODO: Check winding orders
                int[] quads = {
                    7, 4, 3, 0, // Left
                    1, 2, 5, 6, // Right
                    3, 4, 5, 2, // Up
                    1, 6, 7, 0, // Down
                    6, 5, 4, 7, // Forward
                    0, 3, 2, 1, // Back
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
