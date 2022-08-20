#if UNITY
using UnityEngine;
#endif

namespace Sylves.Test
{
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

        public static MeshData PlaneXZ = new MeshData
        {
            indices = new[] { new[] { 0, 1, 2, 3, } },
            // Edge 0 points Right.
            // Edge 1 points Forward
            vertices = new[]
                {
                    new Vector3(0.5f, 0.0f, 0.5f),
                    new Vector3(0.5f, 0.0f, -0.5f),
                    new Vector3(-0.5f, 0.0f, -0.5f),
                    new Vector3(-0.5f, 0.0f, 0.5f),
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

        // Same as PlaneXY, but with InverseWinding
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
                    // Vertex order matches PlaneXY repeated twice
                    // This is called z-forward convention.
                    new Vector3 (+0.5f, -0.5f, -0.5f),
                    new Vector3 (+0.5f, +0.5f, -0.5f),
                    new Vector3 (-0.5f, +0.5f, -0.5f),
                    new Vector3 (-0.5f, -0.5f, -0.5f),
                    new Vector3 (+0.5f, -0.5f, +0.5f),
                    new Vector3 (+0.5f, +0.5f, +0.5f),
                    new Vector3 (-0.5f, +0.5f, +0.5f), 
                    new Vector3 (-0.5f, -0.5f, +0.5f),
                };

                // Faces in same order as CubeDir
                // They are arranged so that 2nd edge points Up ( or Forward), matching CubeDir.Up().
                int[] quads = {
                    7, 6, 2, 3, // Left
                    0, 1, 5, 4, // Right
                    2, 6, 5, 1, // Up
                    0, 4, 7, 3, // Down
                    4, 5, 6, 7, // Forward
                    3, 2, 1, 0, // Back
                };

                meshData.subMeshCount = 1;
                meshData.vertices = vertices;
                meshData.indices = new[] { quads };
                meshData.topologies = new[] { MeshTopology.Quads };
                meshData.RecalculateNormals();

                return meshData;
            }
        }

        private const float Sqrt3 = 1.73205080756888f;

        // Corresponds to a triangle of size 1 as defined by triangle grid
        public static MeshData Equilateral
        {
            get
            {
                return new MeshData
                {
                    vertices = new[]
                    {
                        // Edge 0 points UpRight
                        // Edge 1 points UpLeft
                        // Edge 2 points Down
                        // I.e. the CellDir is double the edge
                        new Vector3(0.5f, -0.3333333333f * Sqrt3 / 2, 0),
                        new Vector3(0, 0.6666666667f * Sqrt3 / 2, 0),
                        new Vector3(-0.5f, -0.3333333333f * Sqrt3 / 2, 0),
                    },
                    indices = new[] { new[]
                        {
                            0,1,2,
                        }
                    },
                    normals = new[]
                    {
                        Vector3.forward,
                        Vector3.forward,
                        Vector3.forward,
                    },
                    subMeshCount = 1,
                    topologies = new[] { MeshTopology.Triangles }
                };
            }
        }

        /*
        public static MeshData EquilateralXZ
        {
            get
            {
                return new MeshData
                {
                    vertices = new[]
                    {
                        new Vector3(0.5f, 0, 0.3333333333f * Sqrt3 / 2),
                        new Vector3(0, 0, -0.6666666667f * Sqrt3 / 2),
                        new Vector3(-0.5f, 0, 0.3333333333f * Sqrt3 / 2),
                    },
                    indices = new[] { new[]
                        {
                            0,1,2,
                        }
                    },
                    normals = new[]
                    {
                        Vector3.up,
                        Vector3.up,
                        Vector3.up,
                    },
                    subMeshCount = 1,
                    topologies = new[] { MeshTopology.Triangles }
                };
            }
        }
        */

        public static MeshData TrianglePlane
        {
            get
            {
                return new MeshData
                {
                    vertices = new[]
                    {
                        new Vector3(1.5f, 0,1.5f),
                        new Vector3(1.5f, 0,-1.5f),
                        new Vector3(-1.5f, 0,-1.5f),
                        new Vector3(-1.5f, 0,1.5f),
                    },
                    indices = new[] { new[]
                        {
                            0,1,2,
                            0,2,3,
                        }
                    },
                    normals = new[]
                    {
                        Vector3.up,
                        Vector3.up,
                        Vector3.up,
                        Vector3.up,
                    },
                    subMeshCount = 1,
                    topologies = new[] { MeshTopology.Triangles }
                };
            }
        }
    }
}
