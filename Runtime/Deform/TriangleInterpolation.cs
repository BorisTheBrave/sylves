using System;
using UnityEngine;

namespace Sylves
{
    /// <summary>
    /// Supplies various linear interpolation methods from a triangle.
    /// The conventions are based on a XY plane,
    /// with an equilateral triangle of side 1, vertices:
    ///  (0.5f, -0.5f / Sqrt3)
    ///  (0, 1 / Sqrt3)
    ///  (-0.5f, -0.5f / Sqrt3)
    /// </summary>
    public static class TriangleInterpolation
    {
        public static void GetCorners(MeshData mesh, int submesh, int face, bool invertWinding, float meshOffset1, float meshOffset2, out Vector3 v1, out Vector3 v2, out Vector3 v3, out Vector3 v4, out Vector3 v5, out Vector3 v6)
        {
            if (mesh.GetTopology(submesh) != MeshTopology.Triangles)
            {
                throw new Exception($"Mesh topology {mesh.GetTopology(submesh)} not supported.");
            }

            var indices = mesh.GetIndices(submesh);
            var vertices = mesh.vertices;
            var normals = mesh.normals;
            int i1, i2, i3;
            if(invertWinding)
            {
                i1 = indices[face * 3 + 2];
                i2 = indices[face * 3 + 1];
                i3 = indices[face * 3 + 0];
            }
            else
            {
                i1 = indices[face * 3 + 0];
                i2 = indices[face * 3 + 1];
                i3 = indices[face * 3 + 2];
            }
            // Find new bounding cage

            v1 = vertices[i1] + normals[i1] * meshOffset1;
            v2 = vertices[i2] + normals[i2] * meshOffset1;
            v3 = vertices[i3] + normals[i3] * meshOffset1;
            v4 = vertices[i1] + normals[i1] * meshOffset2;
            v5 = vertices[i2] + normals[i2] * meshOffset2;
            v6 = vertices[i3] + normals[i3] * meshOffset2;

        }

        public static Func<Vector3, Vector3> InterpolatePosition(MeshData mesh, int submesh, int face, bool invertWinding, float meshOffset1, float meshOffset2)
        {
            GetCorners(mesh, submesh, face, invertWinding, meshOffset1, meshOffset2, out Vector3 v1, out Vector3 v2, out Vector3 v3, out Vector3 v4, out Vector3 v5, out Vector3 v6);
            return Interpolate(v1, v2, v3, v4, v5, v6);
        }

        public static Func<Vector3, Matrix4x4> JacobiPosition(MeshData mesh, int submesh, int face, bool invertWinding, float meshOffset1, float meshOffset2)
        {
            GetCorners(mesh, submesh, face, invertWinding, meshOffset1, meshOffset2, out Vector3 v1, out Vector3 v2, out Vector3 v3, out Vector3 v4, out Vector3 v5, out Vector3 v6);
            return Jacobi(v1, v2, v3, v4, v5, v6);
        }

        public static void GetCorners(MeshData mesh, int submesh, int face, bool invertWinding, out Vector3 v1, out Vector3 v2, out Vector3 v3)
        {
            if (mesh.GetTopology(submesh) != MeshTopology.Triangles)
            {
                throw new Exception($"Mesh topology {mesh.GetTopology(submesh)} not supported.");
            }

            var indices = mesh.GetIndices(submesh);
            var vertices = mesh.vertices;
            var normals = mesh.normals;
            int i1, i2, i3;
            if (invertWinding)
            {
                i1 = indices[face * 3 + 2];
                i2 = indices[face * 3 + 1];
                i3 = indices[face * 3 + 0];
            }
            else
            {
                i1 = indices[face * 3 + 0];
                i2 = indices[face * 3 + 1];
                i3 = indices[face * 3 + 2];
            }
            // Find new bounding cage

            v1 = vertices[i1];
            v2 = vertices[i2];
            v3 = vertices[i3];
        }
        public static Func<Vector3, Vector3> InterpolatePosition(MeshData mesh, int submesh, int face, bool invertWinding)
        {
            GetCorners(mesh, submesh, face, invertWinding, out Vector3 v1, out Vector3 v2, out Vector3 v3);
            return Interpolate(v1, v2, v3);
        }

        public static Func<Vector3, Matrix4x4> JacobiPosition(MeshData mesh, int submesh, int face, bool invertWinding)
        {
            GetCorners(mesh, submesh, face, invertWinding, out Vector3 v1, out Vector3 v2, out Vector3 v3);
            return Jacobi(v1, v2, v3);
        }

        public static Func<Vector3, Vector3> InterpolateNormal(MeshData mesh, int submesh, int face, bool invertWinding)
        {
            if (mesh.GetTopology(submesh) != MeshTopology.Triangles)
            {
                throw new Exception($"Mesh topology {mesh.GetTopology(submesh)} not supported.");
            }

            var indices = mesh.GetIndices(submesh);
            var normals = mesh.normals;

            int i1, i2, i3;
            if (invertWinding)
            {
                i1 = indices[face * 3 + 2];
                i2 = indices[face * 3 + 1];
                i3 = indices[face * 3 + 0];
            }
            else
            {
                i1 = indices[face * 3 + 0];
                i2 = indices[face * 3 + 1];
                i3 = indices[face * 3 + 2];
            }
            // Find new bounding cage

            var v1 = normals[i1];
            var v2 = normals[i2];
            var v3 = normals[i3];

            return Interpolate(v1, v2, v3);
        }

        public static Func<Vector3, Vector4> InterpolateTangent(MeshData mesh, int submesh, int face, bool invertWinding)
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
            int i1, i2, i3;
            if (invertWinding)
            {
                i1 = indices[face * 3 + 2];
                i2 = indices[face * 3 + 1];
                i3 = indices[face * 3 + 0];
            }
            else
            {
                i1 = indices[face * 3 + 0];
                i2 = indices[face * 3 + 1];
                i3 = indices[face * 3 + 2];
            }
            // Find new bounding cage

            var v1 = tangents[i1];
            var v2 = tangents[i2];
            var v3 = tangents[i3];

            return Interpolate(v1, v2, v3);
        }

        public static Func<Vector3, Vector2> InterpolateUv(MeshData mesh, int submesh, int face, bool invertWinding)
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
            int i1, i2, i3;
            if (invertWinding)
            {
                i1 = indices[face * 3 + 2];
                i2 = indices[face * 3 + 1];
                i3 = indices[face * 3 + 0];
            }
            else
            {
                i1 = indices[face * 3 + 0];
                i2 = indices[face * 3 + 1];
                i3 = indices[face * 3 + 2];
            }
            // Find new bounding cage

            var v1 = uvs[i1];
            var v2 = uvs[i2];
            var v3 = uvs[i3];

            return Interpolate(v1, v2, v3);
        }

        public static Func<Vector3, Matrix4x4> JacobiUv(MeshData mesh, int submesh, int face, bool invertWinding)
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
            int i1, i2, i3;
            if (invertWinding)
            {
                i1 = indices[face * 3 + 2];
                i2 = indices[face * 3 + 1];
                i3 = indices[face * 3 + 0];
            }
            else
            {
                i1 = indices[face * 3 + 0];
                i2 = indices[face * 3 + 1];
                i3 = indices[face * 3 + 2];
            }
            // Find new bounding cage

            var v1 = uvs[i1];
            var v2 = uvs[i2];
            var v3 = uvs[i3];

            return Jacobi(v1, v2, v3);
        }

        public static Func<Vector3, Vector2> Interpolate(Vector2 v1, Vector2 v2, Vector2 v3, Vector2 v4, Vector2 v5, Vector2 v6)
        {
            Vector2 InterpolatePoint(Vector3 p)
            {
                var b = StdBarycentric(new Vector2(p.x, p.y));
                // Linear interpolate on each axis in turn
                var u1 = b.x * v1 + b.y * v2 + b.z * v3;
                var u2 = b.x * v4 + b.y * v5 + b.z * v6;
                return (0.5f - p.x) * u1 + (0.5f + p.x) * u2;
            }

            return InterpolatePoint;
        }

        public static Func<Vector3, Vector2> Interpolate(Vector2 v1, Vector2 v2, Vector2 v3)
        {
            Vector2 InterpolatePoint(Vector3 p)
            {
                var b = StdBarycentric(new Vector2(p.x, p.y));
                return b.x * v1 + b.y * v2 + b.z * v3;
            }

            return InterpolatePoint;
        }

        public static Func<Vector3, Matrix4x4> Jacobi(Vector2 v1, Vector2 v2, Vector2 v3)
        {
            Matrix4x4 JacobiPoint(Vector3 p)
            {
                var b = StdBarycentric(new Vector2(p.x, p.y));
                var (dbdx, dbdy) = StdBarycentricDiff();
                var o = b.x * v1 + b.y * v2 + b.z * v3;
                var dodx = dbdx.x * v1 + dbdx.y * v2 + dbdx.z * v3;
                var dody = dbdy.x * v1 + dbdy.y * v2 + dbdy.z * v3;
                return VectorUtils.ToMatrix(new Vector3(dodx.x, dodx.y, 0), new Vector3(dody.x, dody.y, 0), Vector3.zero, new Vector3(o.x, o.y, 0));

            }

            return JacobiPoint;
        }

        public static Func<Vector3, Vector3> Interpolate(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 v5, Vector3 v6)
        {
            Vector3 InterpolatePoint(Vector3 p)
            {
                var b = StdBarycentric(new Vector2(p.x, p.y));
                // Linear interpolate on each axis in turn
                var u1 = b.x * v1 + b.y * v2 + b.z * v3;
                var u2 = b.x * v4 + b.y * v5 + b.z * v6;
                return (0.5f - p.z) * u1 + (0.5f + p.z) * u2;
            }

            return InterpolatePoint;
        }



        public static Func<Vector3, Matrix4x4> Jacobi(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 v5, Vector3 v6)
        {
            Matrix4x4 JacobiPoint(Vector3 p)
            {
                var b = StdBarycentric(new Vector2(p.x, p.y));
                var (dbdx, dbdy) = StdBarycentricDiff();
                // Linear interpolate on each axis in turn
                var u1 = b.x * v1 + b.y * v2 + b.z * v3;
                var u2 = b.x * v4 + b.y * v5 + b.z * v6;
                var du1dx = dbdx.x * v1 + dbdx.y * v2 + dbdx.z * v3;
                var du2dx = dbdx.x * v4 + dbdx.y * v5 + dbdx.z * v6;
                var du1dy = dbdy.x * v1 + dbdy.y * v2 + dbdy.z * v3;
                var du2dy = dbdy.x * v4 + dbdy.y * v5 + dbdy.z * v6;
                var z1 = 0.5f - p.z;
                var z2 = 0.5f + p.z;
                var o = z1 * u1 + z2 * u2;
                var dodx = z1 * du1dx + z2 * du2dx;
                var dody = z1 * du1dy + z2 * du2dy;
                var dodz = u2 - u1;
                return VectorUtils.ToMatrix(dodx, dody, dodz, o);
            }

            return JacobiPoint;
        }

        /// <summary>
        /// Linear interpolates from a triangle of size 1 in the XY plane to the triangle supplied by v1 to v3
        /// The z value of p is unused.
        /// </summary>
        public static Func<Vector3, Vector3> Interpolate(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            Vector3 InterpolatePoint(Vector3 p)
            {
                var b = StdBarycentric(new Vector2(p.x, p.y));
                return b.x * v1 + b.y * v2 + b.z * v3;
            }

            return InterpolatePoint;
        }

        public static Func<Vector3, Matrix4x4> Jacobi(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            Matrix4x4 JacobiPoint(Vector3 p)
            {
                var b = StdBarycentric(new Vector2(p.x, p.y));
                var (dbdx, dbdy) = StdBarycentricDiff();
                var o = b.x * v1 + b.y * v2 + b.z * v3;
                var dodx = dbdx.x * v1 + dbdx.y * v2 + dbdx.z * v3;
                var dody = dbdy.x * v1 + dbdy.y * v2 + dbdy.z * v3;
                return VectorUtils.ToMatrix(dodx, dody, Vector3.zero, o);

            }

            return JacobiPoint;
        }

        public static Func<Vector3, Vector4> Interpolate(Vector4 v1, Vector4 v2, Vector4 v3, Vector4 v4, Vector4 v5, Vector4 v6)
        {
            Vector4 InterpolatePoint(Vector3 p)
            {
                var b = StdBarycentric(new Vector2(p.x, p.y));
                // Linear interpolate on each axis in turn
                var u1 = b.x * v1 + b.y * v2 + b.z * v3;
                var u2 = b.x * v4 + b.y * v5 + b.z * v6;
                return (0.5f - p.z) * u1 + (0.5f + p.z) * u2;
            }

            return InterpolatePoint;
        }

        public static Func<Vector3, Vector4> Interpolate(Vector4 v1, Vector4 v2, Vector4 v3)
        {
            Vector4 InterpolatePoint(Vector3 p)
            {
                var b = StdBarycentric(new Vector2(p.x, p.y));
                return b.x * v1 + b.y * v2 + b.z * v3;
            }

            return InterpolatePoint;
        }

        // Assumes a standard equilateral triangle (see TestMeshes.Equilateral)
        private static Vector3 StdBarycentric(Vector2 p)
        {
            const float sqrt3 = 1.73205080756888f;
            const float o = 1 / 3f;
            const float a = 1 / 3f * sqrt3;
            const float b = 2 / 3f * sqrt3;
            const float c = 0.57735026919f * sqrt3;

            return new Vector3(
                o + c * p.x - a * p.y,
                o +           b * p.y,
                o - c * p.x - a * p.y
                );
        }

        private static (Vector3, Vector3) StdBarycentricDiff()
        {
            const float sqrt3 = 1.73205080756888f;
            const float a = 1 / 3f * sqrt3;
            const float b = 2 / 3f * sqrt3;
            const float c = 0.57735026919f * sqrt3;

            return (new Vector3(c, 0, -c), new Vector3(-a, b, -a));
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
