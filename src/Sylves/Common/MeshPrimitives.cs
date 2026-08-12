using System;
using System.Collections.Generic;
using System.Text;
#if UNITY
using UnityEngine;
#endif

namespace Sylves
{
    /// <summary>
    /// Contains some constant meshes
    /// </summary>
    public static class MeshPrimitives
    {
        /// <summary>
        /// Vertices of a pointy-topped hexagon that fits inside an origin centered rectangle of size width by height.
        /// </summary>
        public static Vector3[] ShapedPtHexPolygon(float width, float height) => new []
        {
            new Vector3(width * 0.5f, height * -0.25f, 0),
            new Vector3(width * 0.5f, height * 0.25f, 0),
            new Vector3(width * 0, height * 0.5f, 0),
            new Vector3(width * -0.5f, height * 0.25f, 0),
            new Vector3(width * -0.5f, height * -0.25f, 0),
            new Vector3(width * 0, height * -0.5f, 0),
        };


        /// <summary>
        /// Vertices of a pointy-topped regular hexagon with 0.5 inradius.
        /// </summary>
        public static Vector3[] PtHexPolygon => ShapedPtHexPolygon(1, 2 / Mathf.Sqrt(3));

        /// <summary>
        /// Vertices of a flat-topped hexagon that fits inside an origin centered rectangle of size width by height.
        /// </summary>
        public static Vector3[] ShapedFtHexPolygon(float width, float height) => new []
        {
            new Vector3(width * 0.5f, height * 0, 0),
            new Vector3(width * 0.25f, height * 0.5f, 0),
            new Vector3(width * -0.25f, height * 0.5f, 0),
            new Vector3(width * -0.5f, height * 0, 0),
            new Vector3(width * -0.25f, height * -0.5f, 0),
            new Vector3(width * 0.25f, height * -0.5f, 0),
        };


        /// <summary>
        /// Vertices of a flat-topped polygon with with 0.5 inradius.
        /// </summary>
        public static Vector3[] FtHexPolygon => ShapedFtHexPolygon(2/ Mathf.Sqrt(3), 1);

        /// <summary>
        /// MeshData for a unity cube centered at the origin.
        /// </summary>
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
                Int32[] quads = {
                    0, 1, 5, 4, // Right
                    7, 6, 2, 3, // Left
                    2, 6, 5, 1, // Up
                    0, 4, 7, 3, // Down
                    4, 5, 6, 7, // Forward
                    3, 2, 1, 0, // Back
                };

                meshData.vertices = vertices;
                meshData.indices = new[] { quads };
                meshData.topologies = new[] { MeshTopology.Quads };
                meshData.RecalculateNormals();

                return meshData;
            }
        }

        /// <summary>
        /// MeshData for a rhombic dodecahedron centered at the origin.
        /// Vertices are in <see cref="RhombicDodecahedronCorner"/> order,
        /// and faces are in <see cref="RhombicDodecahedronDir"/> order.
        /// </summary>
        public static MeshData RhombicDodecahedron
        {
            get
            {
                var meshData = new MeshData();
                Vector3[] vertices = {
                    new Vector3(-0.5f, -0.5f, -0.5f), // BackDownLeft
                    new Vector3(+0.5f, -0.5f, -0.5f), // BackDownRight
                    new Vector3(-0.5f, +0.5f, -0.5f), // BackUpLeft
                    new Vector3(+0.5f, +0.5f, -0.5f), // BackUpRight
                    new Vector3(-0.5f, -0.5f, +0.5f), // ForwardDownLeft
                    new Vector3(+0.5f, -0.5f, +0.5f), // ForwardDownRight
                    new Vector3(-0.5f, +0.5f, +0.5f), // ForwardUpLeft
                    new Vector3(+0.5f, +0.5f, +0.5f), // ForwardUpRight
                    new Vector3(+1.0f,  0.0f,  0.0f), // Right
                    new Vector3(-1.0f,  0.0f,  0.0f), // Left
                    new Vector3( 0.0f, +1.0f,  0.0f), // Up
                    new Vector3( 0.0f, -1.0f,  0.0f), // Down
                    new Vector3( 0.0f,  0.0f, +1.0f), // Forward
                    new Vector3( 0.0f,  0.0f, -1.0f), // Back
                };

                Int32[] quads = {
                    8,  3, 10,  7, // RightUp
                    9,  0, 11,  4, // LeftDown
                    8,  5, 11,  1, // RightDown
                    9,  6, 10,  2, // LeftUp
                    8,  7, 12,  5, // RightForward
                    9,  2, 13,  0, // LeftBack
                    8,  1, 13,  3, // RightBack
                    9,  4, 12,  6, // LeftForward
                    10, 6, 12,  7, // UpForward
                    11, 0, 13,  1, // DownBack
                    10, 3, 13,  2, // UpBack
                    11, 5, 12,  4, // DownForward
                };

                meshData.vertices = vertices;
                meshData.indices = new[] { quads };
                meshData.topologies = new[] { MeshTopology.Quads };
                meshData.RecalculateNormals();

                return meshData;
            }
        }
    }
}
