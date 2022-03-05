using System;
#if UNITY
using UnityEngine;
#endif

namespace Sylves
{
    public static class QuadInterpolation
    {
        /// <summary>
        /// Sets up a function that does trilinear interpolation from a unit cube centered on the origin
        /// to a cube made by extruding a given face of the mesh by meshOffset1 (for y=-0.5) and meshOffset2 (for y=0.5)
        /// </summary>
        public static Func<Vector3, Vector3> InterpolatePosition(MeshData mesh, int submesh, int face, float meshOffset1, float meshOffset2)
        {
            if (mesh.GetTopology(submesh) != MeshTopology.Quads)
            {
                throw new Exception($"Mesh topology {mesh.GetTopology(submesh)} not supported. You need to select \"Keep Quads\"");
            }

            var indices = mesh.GetIndices(submesh);
            var vertices = mesh.vertices;
            var normals = mesh.normals;
            var i1 = indices[face * 4 + 0];
            var i2 = indices[face * 4 + 1];
            var i3 = indices[face * 4 + 2];
            var i4 = indices[face * 4 + 3];
            // Find new bounding cage

            var v1 = vertices[i1] + normals[i1] * meshOffset1;
            var v2 = vertices[i2] + normals[i2] * meshOffset1;
            var v3 = vertices[i3] + normals[i3] * meshOffset1;
            var v4 = vertices[i4] + normals[i4] * meshOffset1;
            var v5 = vertices[i1] + normals[i1] * meshOffset2;
            var v6 = vertices[i2] + normals[i2] * meshOffset2;
            var v7 = vertices[i3] + normals[i3] * meshOffset2;
            var v8 = vertices[i4] + normals[i4] * meshOffset2;

            return Interpolate(v1, v2, v3, v4, v5, v6, v7, v8);
        }

        public static Func<Vector3, Vector3> InterpolatePosition(MeshData mesh, int submesh, int face)
        {
            if (mesh.GetTopology(submesh) != MeshTopology.Quads)
            {
                throw new Exception($"Mesh topology {mesh.GetTopology(submesh)} not supported. You need to select \"Keep Quads\"");
            }

            var indices = mesh.GetIndices(submesh);
            var vertices = mesh.vertices;
            var normals = mesh.normals;
            var i1 = indices[face * 4 + 0];
            var i2 = indices[face * 4 + 1];
            var i3 = indices[face * 4 + 2];
            var i4 = indices[face * 4 + 3];

            var v1 = vertices[i1];
            var v2 = vertices[i2];
            var v3 = vertices[i3];
            var v4 = vertices[i4];

            return Interpolate(v1, v2, v3, v4);
        }

        public static Func<Vector3, Vector3> InterpolateNormal(MeshData mesh, int submesh, int face)
        {
            if (mesh.GetTopology(submesh) != MeshTopology.Quads)
            {
                throw new Exception($"Mesh topology {mesh.GetTopology(submesh)} not supported. You need to select \"Keep Quads\"");
            }

            var indices = mesh.GetIndices(submesh);
            var normals = mesh.normals;

            var i1 = indices[face * 4 + 0];
            var i2 = indices[face * 4 + 1];
            var i3 = indices[face * 4 + 2];
            var i4 = indices[face * 4 + 3];
            // Find new bounding cage

            var v1 = normals[i1];
            var v2 = normals[i2];
            var v3 = normals[i3];
            var v4 = normals[i4];

            return Interpolate(v1, v2, v3, v4);
        }

        public static Func<Vector3, Vector4> InterpolateTangent(MeshData mesh, int submesh, int face)
        {
            if (mesh.GetTopology(submesh) != MeshTopology.Quads)
            {
                throw new Exception($"Mesh topology {mesh.GetTopology(submesh)} not supported. You need to select \"Keep Quads\" in the import options.");
            }
            //if (!mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Tangent))
            if (mesh.tangents.Length == 0)
            {
                throw new Exception($"Mesh needs tangents to calculate smoothed normals. You need to select \"Calculate\" the tangent field of the import options.");
            }

            var tangents = mesh.tangents;

            var indices = mesh.GetIndices(submesh);
            var i1 = indices[face * 4 + 0];
            var i2 = indices[face * 4 + 1];
            var i3 = indices[face * 4 + 2];
            var i4 = indices[face * 4 + 3];
            // Find new bounding cage

            var v1 = tangents[i1];
            var v2 = tangents[i2];
            var v3 = tangents[i3];
            var v4 = tangents[i4];

            return Interpolate(v1, v2, v3, v4);
        }

        public static Func<Vector3, Vector2> InterpolateUv(MeshData mesh, int submesh, int face)
        {
            if (mesh.GetTopology(submesh) != MeshTopology.Quads)
            {
                throw new Exception($"Mesh topology {mesh.GetTopology(submesh)} not supported. You need to select \"Keep Quads\" in the import options.");
            }
            //if (!mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Tangent))
            if (mesh.tangents.Length == 0)
            {
                throw new Exception($"Mesh needs tangents to calculate smoothed normals. You need to select \"Calculate\" the tangent field of the import options.");
            }

            var uvs = mesh.uv;

            var indices = mesh.GetIndices(submesh);
            var i1 = indices[face * 4 + 0];
            var i2 = indices[face * 4 + 1];
            var i3 = indices[face * 4 + 2];
            var i4 = indices[face * 4 + 3];
            // Find new bounding cage

            var v1 = uvs[i1];
            var v2 = uvs[i2];
            var v3 = uvs[i3];
            var v4 = uvs[i4];


            // TODO: Bilienar interpolate
            return Interpolate(v1, v2, v3, v4);
        }

        public static Func<Vector3, Vector2> Interpolate(Vector2 v1, Vector2 v2, Vector2 v3, Vector2 v4, Vector2 v5, Vector2 v6, Vector2 v7, Vector2 v8)
        {
            Vector2 TrilinearInterpolatePoint(Vector3 p)
            {
                //Perform linear interpolation.
                var x1 = 0.5f - p.x;
                var x2 = 0.5f + p.x;
                var y1 = 0.5f - p.y;
                var y2 = 0.5f + p.y;
                var z1 = 0.5f - p.z;
                var z2 = 0.5f + p.z;
                var u1 = z1 * v1 + z2 * v2;
                var u2 = z1 * v4 + z2 * v3;
                var u3 = z1 * v5 + z2 * v6;
                var u4 = z1 * v8 + z2 * v7;
                var w1 = x1 * u1 + x2 * u2;
                var w2 = x1 * u3 + x2 * u4;
                var z = y1 * w1 + y2 * w2;
                return z;
            }

            return TrilinearInterpolatePoint;
        }


        public static Func<Vector3, Vector2> Interpolate(Vector2 v1, Vector2 v2, Vector2 v3, Vector2 v4)
        {
            Vector2 TrilinearInterpolatePoint(Vector3 p)
            {
                //Perform linear interpolation.
                var x1 = 0.5f - p.x;
                var x2 = 0.5f + p.x;
                var y1 = 0.5f - p.y;
                var y2 = 0.5f + p.y;
                var z1 = 0.5f - p.z;
                var z2 = 0.5f + p.z;
                var u1 = z1 * v1 + z2 * v2;
                var u2 = z1 * v4 + z2 * v3;
                return x1 * u1 + x2 * u2;
            }

            return TrilinearInterpolatePoint;
        }

        public static Func<Vector3, Vector3> Interpolate(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 v5, Vector3 v6, Vector3 v7, Vector3 v8)
        {
            Vector3 TrilinearInterpolatePoint(Vector3 p)
            {
                //Perform linear interpolation.
                var x1 = 0.5f - p.x;
                var x2 = 0.5f + p.x;
                var y1 = 0.5f - p.y;
                var y2 = 0.5f + p.y;
                var z1 = 0.5f - p.z;
                var z2 = 0.5f + p.z;
                var u1 = z1 * v1 + z2 * v2;
                var u2 = z1 * v4 + z2 * v3;
                var u3 = z1 * v5 + z2 * v6;
                var u4 = z1 * v8 + z2 * v7;
                var w1 = x1 * u1 + x2 * u2;
                var w2 = x1 * u3 + x2 * u4;
                var z = y1 * w1 + y2 * w2;
                return z;
            }

            return TrilinearInterpolatePoint;
        }

        public static Func<Vector3, Vector3> Interpolate(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
        {
            Vector3 TrilinearInterpolatePoint(Vector3 p)
            {
                //Perform linear interpolation.
                var x1 = 0.5f - p.x;
                var x2 = 0.5f + p.x;
                var y1 = 0.5f - p.y;
                var y2 = 0.5f + p.y;
                var z1 = 0.5f - p.z;
                var z2 = 0.5f + p.z;
                var u1 = z1 * v1 + z2 * v2;
                var u2 = z1 * v4 + z2 * v3;
                return x1 * u1 + x2 * u2;
            }

            return TrilinearInterpolatePoint;
        }

        public static Func<Vector3, Vector4> Interpolate(Vector4 v1, Vector4 v2, Vector4 v3, Vector4 v4, Vector4 v5, Vector4 v6, Vector4 v7, Vector4 v8)
        {
            Vector4 TrilinearInterpolatePoint(Vector3 p)
            {
                //Perform linear interpolation.
                var x1 = 0.5f - p.x;
                var x2 = 0.5f + p.x;
                var y1 = 0.5f - p.y;
                var y2 = 0.5f + p.y;
                var z1 = 0.5f - p.z;
                var z2 = 0.5f + p.z;
                var u1 = z1 * v1 + z2 * v2;
                var u2 = z1 * v4 + z2 * v3;
                var u3 = z1 * v5 + z2 * v6;
                var u4 = z1 * v8 + z2 * v7;
                var w1 = x1 * u1 + x2 * u2;
                var w2 = x1 * u3 + x2 * u4;
                var z = y1 * w1 + y2 * w2;
                return z;
            }

            return TrilinearInterpolatePoint;
        }

        public static Func<Vector3, Vector4> Interpolate(Vector4 v1, Vector4 v2, Vector4 v3, Vector4 v4)
        {
            Vector4 TrilinearInterpolatePoint(Vector3 p)
            {
                //Perform linear interpolation.
                var x1 = 0.5f - p.x;
                var x2 = 0.5f + p.x;
                var y1 = 0.5f - p.y;
                var y2 = 0.5f + p.y;
                var z1 = 0.5f - p.z;
                var z2 = 0.5f + p.z;
                var u1 = z1 * v1 + z2 * v2;
                var u2 = z1 * v4 + z2 * v3;
                return x1 * u1 + x2 * u2;
            }

            return TrilinearInterpolatePoint;
        }
    }
}
