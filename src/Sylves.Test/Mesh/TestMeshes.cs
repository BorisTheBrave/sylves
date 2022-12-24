#if UNITY
using UnityEngine;
#endif

using System.Linq;

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
            topologies = new[] { MeshTopology.Quads }
        };

        public static MeshData Cube => MeshPrimitives.Cube;

        private const float Sqrt3 = 1.73205080756888f;

        // Corresponds to a triangle of incircle diamater 1, side length sqrt(3)
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
                        new Vector3(Sqrt3 / 2, -0.5f, 0),
                        new Vector3(0, 1.0f, 0),
                        new Vector3(-Sqrt3 / 2, -0.5f, 0),
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
                    topologies = new[] { MeshTopology.Triangles }
                };
            }
        }

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
                    topologies = new[] { MeshTopology.Triangles }
                };
            }
        }

        public static MeshData Lion
        {
            get
            {
                return new MeshData
                {
                    vertices = new[]
                    {
new Vector3(0.000000f, 0.172243f, 0.357000f),
new Vector3(0.000000f, 0.236443f, 0.145970f),
new Vector3(0.000000f, -0.039012f, 0.374603f),
new Vector3(0.000000f, 0.217880f, 0.242171f),
new Vector3(0.000000f, 0.185493f, 0.192853f),
new Vector3(0.000000f, 0.114093f, 0.278975f),
new Vector3(0.000000f, 0.033489f, 0.329320f),
new Vector3(0.000000f, 0.165100f, 0.217451f),
new Vector3(0.000000f, -0.014722f, 0.326820f),
new Vector3(0.000000f, 0.040485f, 0.262781f),
new Vector3(0.000000f, 0.128814f, 0.212728f),
new Vector3(0.000000f, 0.082441f, 0.206839f),
new Vector3(0.000000f, 0.016194f, 0.219352f),
new Vector3(0.000000f, -0.118509f, 0.345222f),
new Vector3(0.000000f, -0.029443f, 0.345222f),
new Vector3(0.000000f, -0.072848f, 0.302556f),
new Vector3(0.000000f, 0.007361f, 0.209783f),
new Vector3(0.000000f, -0.243643f, 0.319460f),
new Vector3(0.000000f, -0.083913f, 0.142064f),
new Vector3(0.000000f, -0.016930f, 0.144272f),
new Vector3(0.000000f, -0.213711f, 0.306381f),
new Vector3(0.000000f, -0.055765f, 0.162948f),
new Vector3(0.000000f, -0.124398f, 0.248060f),
new Vector3(-0.000000f, -0.284128f, 0.279711f),
new Vector3(0.000000f, -0.074437f, 0.149095f),
new Vector3(0.000000f, -0.253503f, 0.273643f),
new Vector3(0.000000f, -0.126606f, 0.189860f),
new Vector3(-0.000000f, -0.381781f, 0.193541f),
new Vector3(0.000000f, 0.230202f, -0.000000f),
new Vector3(0.000000f, 0.268360f, 0.097030f),
new Vector3(0.000000f, 0.245041f, 0.085371f),
new Vector3(0.000000f, 0.218542f, 0.042972f),
new Vector3(0.000000f, 0.151765f, 0.020713f),
new Vector3(0.000000f, 0.117846f, 0.054632f),
new Vector3(0.000000f, 0.145405f, 0.102330f),
new Vector3(0.000000f, 0.136925f, 0.165928f),
new Vector3(0.000000f, -0.063916f, 0.142723f),
new Vector3(0.000000f, 0.078951f, 0.043502f),
new Vector3(0.000000f, -0.013795f, 0.095764f),
new Vector3(-0.000000f, -0.119055f, 0.000000f),
new Vector3(0.000000f, -0.096236f, 0.141401f),
new Vector3(-0.000000f, -0.124207f, 0.111222f),
new Vector3(-0.000000f, -0.131568f, 0.045711f),
new Vector3(-0.000000f, -0.121205f, 0.007853f),
new Vector3(0.000000f, 0.004753f, 0.085313f),
new Vector3(-0.000000f, -0.083723f, 0.011851f),
new Vector3(-0.000000f, -0.074890f, 0.049391f),
new Vector3(0.000000f, -0.037350f, 0.072946f),
new Vector3(-0.000000f, -0.008642f, 0.000000f),
new Vector3(-0.000000f, -0.063849f, 0.000000f),
new Vector3(-0.000000f, -0.050599f, 0.012587f),
new Vector3(0.000000f, 0.042446f, 0.064073f),
new Vector3(0.000000f, 0.057605f, 0.095764f),
new Vector3(0.000000f, 0.071591f, 0.105333f),
new Vector3(0.000000f, 0.084840f, 0.094292f),
new Vector3(0.000000f, 0.088521f, 0.070001f),
new Vector3(0.000000f, 0.112075f, 0.081779f),
new Vector3(0.000000f, 0.114283f, 0.140665f),
new Vector3(0.000000f, -0.007838f, 0.149996f),
new Vector3(-0.000000f, -0.342055f, 0.192968f),
new Vector3(-0.000000f, -0.195607f, 0.000000f),
new Vector3(-0.000000f, -0.153651f, 0.102389f),
new Vector3(-0.000000f, -0.196343f, 0.039086f),
new Vector3(-0.000000f, -0.263327f, 0.000000f),
new Vector3(-0.000000f, -0.196094f, 0.025869f),
new Vector3(-0.000000f, -0.329244f, 0.188160f),
new Vector3(-0.000000f, -0.196247f, 0.033949f),
new Vector3(-0.000000f, -0.225051f, 0.073682f),
new Vector3(-0.000000f, -0.434098f, 0.121527f),
new Vector3(-0.000000f, -0.297187f, 0.000000f),
new Vector3(-0.000000f, -0.500000f, 0.081043f),
new Vector3(-0.000000f, -0.379618f, 0.109058f),
new Vector3(-0.000000f, -0.448084f, 0.087667f),
new Vector3(-0.000000f, -0.450292f, 0.059696f),
new Vector3(-0.000000f, -0.371531f, 0.064113f),
new Vector3(-0.000000f, -0.348004f, 0.043823f),
new Vector3(-0.000000f, -0.395822f, 0.000000f),
new Vector3(0.000000f, 0.304031f, 0.203033f),
new Vector3(0.000000f, 0.398495f, 0.278481f),
new Vector3(0.000000f, 0.391134f, 0.232476f),
new Vector3(0.000000f, 0.364758f, 0.200579f),
new Vector3(0.000000f, 0.350649f, 0.141079f),
new Vector3(0.000000f, 0.245758f, 0.137399f),
new Vector3(0.000000f, 0.248825f, 0.118997f),
new Vector3(0.000000f, 0.370422f, 0.256060f),
new Vector3(0.000000f, 0.247598f, 0.214688f),
new Vector3(0.000000f, 0.226129f, 0.263146f),
new Vector3(0.000000f, 0.217541f, 0.365585f),
new Vector3(0.000000f, 0.346356f, 0.312219f),
new Vector3(0.000000f, 0.221082f, 0.323351f),
new Vector3(0.000000f, 0.175805f, 0.348038f),
new Vector3(0.000000f, 0.293603f, -0.000000f),
new Vector3(0.000000f, 0.416897f, 0.116543f),
new Vector3(0.000000f, 0.490505f, 0.283389f),
new Vector3(0.000000f, 0.425484f, 0.272347f),
new Vector3(0.000000f, 0.339608f, 0.073605f),
new Vector3(0.000000f, 0.422680f, 0.047412f),
new Vector3(0.000000f, 0.318139f, -0.000000f),
new Vector3(0.000000f, 0.430392f, 0.050909f),
new Vector3(0.000000f, 0.467809f, 0.110409f),
new Vector3(0.000000f, 0.469649f, 0.133105f),
new Vector3(0.000000f, 0.459222f, 0.113476f),
new Vector3(0.000000f, 0.380093f, -0.000000f),
new Vector3(0.000000f, 0.402605f, 0.038307f),
new Vector3(-0.000000f, 0.172243f, -0.357000f),
new Vector3(0.000000f, 0.236443f, -0.145970f),
new Vector3(-0.000000f, -0.039012f, -0.374603f),
new Vector3(-0.000000f, 0.217880f, -0.242171f),
new Vector3(-0.000000f, 0.185493f, -0.192853f),
new Vector3(-0.000000f, 0.114093f, -0.278975f),
new Vector3(-0.000000f, 0.033489f, -0.329320f),
new Vector3(-0.000000f, 0.165100f, -0.217451f),
new Vector3(-0.000000f, -0.014722f, -0.326820f),
new Vector3(-0.000000f, 0.040485f, -0.262781f),
new Vector3(-0.000000f, 0.128814f, -0.212728f),
new Vector3(-0.000000f, 0.082441f, -0.206839f),
new Vector3(-0.000000f, 0.016194f, -0.219352f),
new Vector3(-0.000000f, -0.118509f, -0.345222f),
new Vector3(-0.000000f, -0.029443f, -0.345222f),
new Vector3(-0.000000f, -0.072848f, -0.302556f),
new Vector3(-0.000000f, 0.007361f, -0.209783f),
new Vector3(-0.000000f, -0.243643f, -0.319460f),
new Vector3(-0.000000f, -0.083913f, -0.142064f),
new Vector3(-0.000000f, -0.016930f, -0.144272f),
new Vector3(-0.000000f, -0.213711f, -0.306381f),
new Vector3(-0.000000f, -0.055765f, -0.162948f),
new Vector3(-0.000000f, -0.124398f, -0.248060f),
new Vector3(-0.000000f, -0.284128f, -0.279711f),
new Vector3(-0.000000f, -0.074437f, -0.149095f),
new Vector3(-0.000000f, -0.253503f, -0.273643f),
new Vector3(-0.000000f, -0.126606f, -0.189860f),
new Vector3(-0.000000f, -0.381781f, -0.193541f),
new Vector3(0.000000f, 0.268360f, -0.097030f),
new Vector3(0.000000f, 0.245041f, -0.085371f),
new Vector3(0.000000f, 0.218542f, -0.042972f),
new Vector3(0.000000f, 0.151765f, -0.020713f),
new Vector3(0.000000f, 0.117846f, -0.054632f),
new Vector3(0.000000f, 0.145405f, -0.102330f),
new Vector3(-0.000000f, 0.136925f, -0.165928f),
new Vector3(-0.000000f, -0.063916f, -0.142723f),
new Vector3(0.000000f, 0.078951f, -0.043502f),
new Vector3(-0.000000f, -0.013795f, -0.095764f),
new Vector3(-0.000000f, -0.096236f, -0.141401f),
new Vector3(-0.000000f, -0.124207f, -0.111222f),
new Vector3(-0.000000f, -0.131568f, -0.045711f),
new Vector3(-0.000000f, -0.121205f, -0.007853f),
new Vector3(-0.000000f, 0.004753f, -0.085313f),
new Vector3(-0.000000f, -0.083723f, -0.011851f),
new Vector3(-0.000000f, -0.074890f, -0.049391f),
new Vector3(-0.000000f, -0.037350f, -0.072946f),
new Vector3(-0.000000f, -0.050599f, -0.012587f),
new Vector3(-0.000000f, 0.042446f, -0.064073f),
new Vector3(-0.000000f, 0.057605f, -0.095764f),
new Vector3(-0.000000f, 0.071591f, -0.105333f),
new Vector3(-0.000000f, 0.084840f, -0.094292f),
new Vector3(0.000000f, 0.088521f, -0.070001f),
new Vector3(0.000000f, 0.112075f, -0.081779f),
new Vector3(-0.000000f, 0.114283f, -0.140665f),
new Vector3(-0.000000f, -0.007838f, -0.149996f),
new Vector3(-0.000000f, -0.342055f, -0.192968f),
new Vector3(-0.000000f, -0.153651f, -0.102389f),
new Vector3(-0.000000f, -0.196343f, -0.039086f),
new Vector3(-0.000000f, -0.196094f, -0.025869f),
new Vector3(-0.000000f, -0.329244f, -0.188160f),
new Vector3(-0.000000f, -0.196247f, -0.033949f),
new Vector3(-0.000000f, -0.225051f, -0.073682f),
new Vector3(-0.000000f, -0.434098f, -0.121527f),
new Vector3(-0.000000f, -0.500000f, -0.081043f),
new Vector3(-0.000000f, -0.379618f, -0.109058f),
new Vector3(-0.000000f, -0.448084f, -0.087667f),
new Vector3(-0.000000f, -0.450292f, -0.059696f),
new Vector3(-0.000000f, -0.371531f, -0.064113f),
new Vector3(-0.000000f, -0.348004f, -0.043823f),
new Vector3(0.000000f, 0.304031f, -0.203033f),
new Vector3(0.000000f, 0.398495f, -0.278481f),
new Vector3(0.000000f, 0.391134f, -0.232476f),
new Vector3(0.000000f, 0.364758f, -0.200579f),
new Vector3(0.000000f, 0.350649f, -0.141079f),
new Vector3(0.000000f, 0.245758f, -0.137399f),
new Vector3(0.000000f, 0.248825f, -0.118997f),
new Vector3(0.000000f, 0.370422f, -0.256060f),
new Vector3(0.000000f, 0.247598f, -0.214688f),
new Vector3(-0.000000f, 0.226129f, -0.263146f),
new Vector3(-0.000000f, 0.217541f, -0.365585f),
new Vector3(0.000000f, 0.346356f, -0.312219f),
new Vector3(-0.000000f, 0.221082f, -0.323351f),
new Vector3(-0.000000f, 0.175805f, -0.348038f),
new Vector3(0.000000f, 0.416897f, -0.116543f),
new Vector3(0.000000f, 0.490505f, -0.283389f),
new Vector3(0.000000f, 0.425484f, -0.272347f),
new Vector3(0.000000f, 0.339608f, -0.073605f),
new Vector3(0.000000f, 0.422680f, -0.047412f),
new Vector3(0.000000f, 0.430392f, -0.050909f),
new Vector3(0.000000f, 0.467809f, -0.110409f),
new Vector3(0.000000f, 0.469649f, -0.133105f),
new Vector3(0.000000f, 0.459222f, -0.113476f),
new Vector3(0.000000f, 0.402605f, -0.038307f),
                    }.Select(x => new Vector3(x.z, -x.y, 0)).ToArray(),
                    indices = new[] { new[]
                        {
3, 7, 6, 8, 5, 4, 91, ~1,
5, 2, 86, 87, 90, 91, ~4,
7, 9, 10, 11, 8, ~6,
9, 15, 14, 16, 13, 12, 11, ~10,
16, 18, 21, 17, ~13,
17, 22, 25, 19, 37, 20, 59, 12, ~13,
21, 24, 26, 23, 22, ~17,
26, 28, 60, 27, 25, 22, ~23,
85, 89, 88, 90, 87, 86, 2, 83, 84, ~78,
37, 39, 45, 52, 53, 59, ~20,
19, 41, 42, 43, 44, 46, 47, 48, 45, 39, ~37,
51, 50, 151, 150, 49, ~48,
40, 146, 148, 149, 150, 151, 50, 51, 48, 47, 46, ~44,
36, 58, 57, 56, 55, 54, 53, 52, 38, 34, ~35,
58, 55, 56, ~57,
59, 53, 54, 55, 58, 36, 8, 11, ~12,
60, 66, 42, 41, 19, 25, ~27,
42, 62, 63, 67, 65, 61, 163, 165, 162, 161, 144, 145, 146, 40, 44, ~43,
61, 65, 64, ~163,
66, 69, 72, 68, 67, 63, 62, ~42,
70, 76, 77, ~173,
8, 36, 35, 34, 33, 32, 31, 30, 82, 81, 80, 79, 85, 78, 84, 83, 2, ~5,
104, 98, 197, ~103,
107, 105, 187, 108, 109, 112, 110, ~111,
109, 108, 187, 186, 183, 182, ~106,
111, 110, 112, 115, 114, ~113,
113, 114, 115, 116, 117, 120, 118, ~119,
120, 117, 121, 125, ~122,
121, 117, 116, 159, 124, 140, 123, 129, ~126,
125, 121, 126, 127, 130, ~128,
130, 127, 126, 129, 131, 160, ~132,
181, 174, 180, 179, 106, 182, 183, 186, 184, ~185,
140, 124, 159, 153, 152, 147, ~142,
123, 140, 142, 147, 150, 149, 148, 146, 145, 144, ~143,
139, 138, 137, 141, 152, 153, 154, 155, 156, 157, ~158,
158, 157, 156, ~155,
159, 116, 115, 112, 139, 158, 155, 154, ~153,
160, 131, 129, 123, 143, 144, ~164,
164, 144, 161, 162, 165, 166, 169, ~167,
147, 152, 141, 137, 136, 135, 134, 133, 29, 30, 31, 32, 33, 34, 38, 52, 45, 48, 49, ~150,
112, 109, 106, 179, 180, 174, 181, 175, 176, 177, 178, 133, 134, 135, 136, 137, 138, ~139,
169, 166, 165, 163, 64, 65, 67, 68, 72, 73, 71, 74, 75, 76, 70, 173, 172, 171, 168, ~170,
188, 196, 195, 194, 193, 192, 197, 98, 104, 97, 99, 100, 101, 102, 93, 96, 92, ~191,
96, 93, 94, 95, 80, 81, 82, 30, 29, 133, 178, 177, 176, 190, 189, 188, 191, ~92,

                        }.Select(x=>x>=0?x-1 : ~(~x -1)).ToArray()
                    },
                    topologies = new[] { MeshTopology.NGon }
                };
            }
        }
        public static MeshData Icosahedron
        {
            get
            {
                return new MeshData
                {
                    vertices = new[]
                    {
                        new Vector3(0.000000f, 1.000000f, 0.000000f),
                        new Vector3(-0.000000f, 0.447214f, 0.894427f),
                        new Vector3(0.850651f, 0.447214f, 0.276393f),
                        new Vector3(0.525731f, 0.447214f, -0.723607f),
                        new Vector3(-0.525731f, 0.447214f, -0.723607f),
                        new Vector3(-0.850651f, 0.447214f, 0.276393f),
                        new Vector3(0.525731f, -0.447214f, 0.723607f),
                        new Vector3(0.850651f, -0.447214f, -0.276393f),
                        new Vector3(0.000000f, -0.447214f, -0.894427f),
                        new Vector3(-0.850651f, -0.447214f, -0.276393f),
                        new Vector3(-0.525731f, -0.447214f, 0.723607f),
                        new Vector3(-0.000000f, -1.000000f, -0.000000f),
                    }.ToArray(),
                    indices = new[] { new[]
                        {
                            1, 3, 2,
                            1, 4, 3,
                            1, 5, 4,
                            1, 6, 5,
                            1, 2, 6,
                            2, 3, 7,
                            3, 8, 7,
                            3, 4, 8,
                            4, 9, 8,
                            4, 5, 9,
                            5, 10, 9,
                            5, 6, 10,
                            6, 11, 10,
                            6, 2, 11,
                            2, 7, 11,
                            7, 8, 12,
                            8, 9, 12,
                            9, 10, 12,
                            10, 11, 12,
                            11, 7, 12,
                        }.Select(x=>x>=0?x-1 : ~(~x -1)).ToArray()
                    },
                    topologies = new[] { MeshTopology.Triangles }
                };
            }
        }
    }
}
