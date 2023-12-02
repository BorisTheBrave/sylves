using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

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
                int[] quads = {
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
    }
}
