using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sylves
{
    // Triangle space is defined as a XZ oriented std equilateral triangle of size sqrt(3), vertices, top, bottomleft, bottomright
    // y axis extends from -1 to 1
    public static class TriangleInterpolation
    {
        public static Func<Vector3, Vector3> InterpolatePosition(MeshData mesh, int submesh, int face, float meshOffset1, float meshOffset2)
        {
            if (mesh.GetTopology(submesh) != MeshTopology.Triangles)
            {
                throw new Exception($"Mesh topology {mesh.GetTopology(submesh)} not supported.");
            }

            var indices = mesh.GetIndices(submesh);
            var vertices = mesh.vertices;
            var normals = mesh.normals;
            var i1 = indices[face * 3 + 0];
            var i2 = indices[face * 3 + 1];
            var i3 = indices[face * 3 + 2];
            // Find new bounding cage

            var v1 = vertices[i1] + normals[i1] * meshOffset1;
            var v2 = vertices[i2] + normals[i2] * meshOffset1;
            var v3 = vertices[i3] + normals[i3] * meshOffset1;
            var v4 = vertices[i1] + normals[i1] * meshOffset2;
            var v5 = vertices[i2] + normals[i2] * meshOffset2;
            var v6 = vertices[i3] + normals[i3] * meshOffset2;

            return Interpolate(v1, v2, v3, v4, v5, v6);
        }
        public static Func<Vector3, Vector3> InterpolatePosition(MeshData mesh, int submesh, int face)
        {
            if (mesh.GetTopology(submesh) != MeshTopology.Triangles)
            {
                throw new Exception($"Mesh topology {mesh.GetTopology(submesh)} not supported.");
            }

            var indices = mesh.GetIndices(submesh);
            var vertices = mesh.vertices;
            var normals = mesh.normals;
            var i1 = indices[face * 3 + 0];
            var i2 = indices[face * 3 + 1];
            var i3 = indices[face * 3 + 2];
            // Find new bounding cage

            var v1 = vertices[i1];
            var v2 = vertices[i2];
            var v3 = vertices[i3];

            return Interpolate(v1, v2, v3);
        }

        public static Func<Vector3, Vector3> InterpolateNormal(MeshData mesh, int submesh, int face)
        {
            if (mesh.GetTopology(submesh) != MeshTopology.Triangles)
            {
                throw new Exception($"Mesh topology {mesh.GetTopology(submesh)} not supported.");
            }

            var indices = mesh.GetIndices(submesh);
            var normals = mesh.normals;

            var i1 = indices[face * 3 + 0];
            var i2 = indices[face * 3 + 1];
            var i3 = indices[face * 3 + 2];
            // Find new bounding cage

            var v1 = normals[i1];
            var v2 = normals[i2];
            var v3 = normals[i3];

            return Interpolate(v1, v2, v3);
        }

        public static Func<Vector3, Vector4> InterpolateTangent(MeshData mesh, int submesh, int face)
        {
            if (mesh.GetTopology(submesh) != MeshTopology.Triangles)
            {
                throw new Exception($"Mesh topology {mesh.GetTopology(submesh)} not supported.");
            }
            //if (!mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Tangent))
            if (mesh.tangents.Length == 0)
            {
                throw new Exception($"Mesh needs tangents to calculate smoothed normals. You need to select \"Calculate\" the tangent field of the import options.");
            }

            var tangents = mesh.tangents;

            var indices = mesh.GetIndices(submesh);
            var i1 = indices[face * 3 + 0];
            var i2 = indices[face * 3 + 1];
            var i3 = indices[face * 3 + 2];
            // Find new bounding cage

            var v1 = tangents[i1];
            var v2 = tangents[i2];
            var v3 = tangents[i3];

            return Interpolate(v1, v2, v3);
        }

        public static Func<Vector3, Vector2> InterpolateUv(MeshData mesh, int submesh, int face)
        {
            if (mesh.GetTopology(submesh) != MeshTopology.Triangles)
            {
                throw new Exception($"Mesh topology {mesh.GetTopology(submesh)} not supported.");
            }
            //if (!mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Tangent))
            if (mesh.tangents.Length == 0)
            {
                throw new Exception($"Mesh needs tangents to calculate smoothed normals. You need to select \"Calculate\" the tangent field of the import options.");
            }

            var uvs = mesh.uv;

            var indices = mesh.GetIndices(submesh);
            var i1 = indices[face * 3 + 0];
            var i2 = indices[face * 3 + 1];
            var i3 = indices[face * 3 + 2];
            // Find new bounding cage

            var v1 = uvs[i1];
            var v2 = uvs[i2];
            var v3 = uvs[i3];

            return Interpolate(v1, v2, v3);
        }

        public static Func<Vector3, Vector2> Interpolate(Vector2 v1, Vector2 v2, Vector2 v3, Vector2 v4, Vector2 v5, Vector2 v6)
        {
            Vector2 InterpolatePoint(Vector3 p)
            {
                var b = StdBarycentric(new Vector2(p.x, p.z));
                // Linear interpolate on each axis in turn
                var u1 = b.x * v1 + b.y * v2 + b.z * v3;
                var u2 = b.x * v4 + b.y * v5 + b.z * v6;
                return (0.5f - p.y) * u1 + (0.5f + p.y) * u2;
            }

            return InterpolatePoint;
        }

        public static Func<Vector3, Vector2> Interpolate(Vector2 v1, Vector2 v2, Vector2 v3)
        {
            Vector2 InterpolatePoint(Vector3 p)
            {
                var b = StdBarycentric(new Vector2(p.x, p.z));
                return b.x * v1 + b.y * v2 + b.z * v3;
            }

            return InterpolatePoint;
        }

        public static Func<Vector3, Vector3> Interpolate(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 v5, Vector3 v6)
        {
            Vector3 InterpolatePoint(Vector3 p)
            {
                var b = StdBarycentric(new Vector2(p.x, p.z));
                // Linear interpolate on each axis in turn
                var u1 = b.x * v1 + b.y * v2 + b.z * v3;
                var u2 = b.x * v4 + b.y * v5 + b.z * v6;
                return (0.5f - p.y) * u1 + (0.5f + p.y) * u2;
            }

            return InterpolatePoint;
        }

        public static Func<Vector3, Vector3> Interpolate(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            Vector3 InterpolatePoint(Vector3 p)
            {
                var b = StdBarycentric(new Vector2(p.x, p.z));
                return b.x * v1 + b.y * v2 + b.z * v3;
            }

            return InterpolatePoint;
        }

        public static Func<Vector3, Vector4> Interpolate(Vector4 v1, Vector4 v2, Vector4 v3, Vector4 v4, Vector4 v5, Vector4 v6)
        {
            Vector4 InterpolatePoint(Vector3 p)
            {
                var b = StdBarycentric(new Vector2(p.x, p.z));
                // Linear interpolate on each axis in turn
                var u1 = b.x * v1 + b.y * v2 + b.z * v3;
                var u2 = b.x * v4 + b.y * v5 + b.z * v6;
                return (0.5f - p.y) * u1 + (0.5f + p.y) * u2;
            }

            return InterpolatePoint;
        }

        public static Func<Vector3, Vector4> Interpolate(Vector4 v1, Vector4 v2, Vector4 v3)
        {
            Vector4 InterpolatePoint(Vector3 p)
            {
                var b = StdBarycentric(new Vector2(p.x, p.z));
                return b.x * v1 + b.y * v2 + b.z * v3;
            }

            return InterpolatePoint;
        }

        // Assumes a standard equilateral triangle 
        private static Vector3 StdBarycentric(Vector2 p)
        {
            const float a = 1 / 3f;
            const float b = 2 / 3f;
            const float c = 0.57735026919f; // 1 / sqrt(3)

            return new Vector3(
                a +           b * p.y,
                a + c * p.x - a * p.y,
                a - c * p.x - a * p.y
                );
            /*
            return Barycentric(
                p,
                new Vector2(0, 1),
                new Vector2(Mathf.Sqrt(3) / 2, -.5f),
                new Vector2(-Mathf.Sqrt(3) / 2, -.5f));
            */
        }


        // https://gamedev.stackexchange.com/a/23745
        private static Vector3 Barycentric(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            Vector2 v0 = b - a, v1 = c - a, v2 = p - a;
            float d00 = Vector2.Dot(v0, v0);
            float d01 = Vector2.Dot(v0, v1);
            float d11 = Vector2.Dot(v1, v1);
            float d20 = Vector2.Dot(v2, v0);
            float d21 = Vector2.Dot(v2, v1);
            float denom = d00 * d11 - d01 * d01;
            var v = (d11 * d20 - d01 * d21) / denom;
            var w = (d00 * d21 - d01 * d20) / denom;
            var u = 1.0f - v - w;
            return new Vector3(u, v, w);
        }

    }
}
