using System;
#if UNITY
using UnityEngine;
#endif

namespace Sylves
{
    /// <summary>
    /// Supplies various linear interpolation methods from a triangle
    /// For historic reasons, the conventions are based on a XZ plane,
    /// with an equilateral triangle of side sqrt(3), vertices, top, bottomright, bottomleft
    /// z axis extends from -0.5 to 1.0
    /// x axis from -sqrt(3)/2 to sqrt(3)/2
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



        public static Func<Vector3, Matrix4x4> Jacobi(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 v5, Vector3 v6)
        {
            Matrix4x4 JacobiPoint(Vector3 p)
            {
                var b = StdBarycentric(new Vector2(p.x, p.z));
                var (dbdx, dbdz) = StdBarycentricDiff();
                // Linear interpolate on each axis in turn
                var u1 = b.x * v1 + b.y * v2 + b.z * v3;
                var u2 = b.x * v4 + b.y * v5 + b.z * v6;
                var du1dx = dbdx.x * v1 + dbdx.y * v2 + dbdx.z * v3;
                var du2dx = dbdx.x * v4 + dbdx.y * v5 + dbdx.z * v6;
                var du1dz = dbdz.x * v1 + dbdz.y * v2 + dbdz.z * v3;
                var du2dz = dbdz.x * v4 + dbdz.y * v5 + dbdz.z * v6;
                var y1 = 0.5f - p.y;
                var y2 = 0.5f + p.y;
                var o = y1 * u1 + y2 * u2;
                var dodx = y1 * du1dx + y2 * du2dx;
                var dodz = y1 * du1dz + y2 * du2dz;
                var dody = u2 - u1;
                return VectorUtils.ToMatrix(dodx, dody, dodz, o);
            }

            return JacobiPoint;
        }

        /// <summary>
        /// Linear interpolates from a triangle of size Sqrt(3) in the XZ plane to the triangle supplied by v1 to v3
        /// The z value of p is unused.
        /// </summary>
        /// <param name="v1">Final location of (0, 0, 1)</param>
        /// <param name="v2">Final location of (sqrt(3) / 2, 0, -0.5)</param>
        /// <param name="v3">Final location of (-sqrt(3) / 2, 0, 0.5)</param>
        public static Func<Vector3, Vector3> Interpolate(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            Vector3 InterpolatePoint(Vector3 p)
            {
                var b = StdBarycentric(new Vector2(p.x, p.z));
                return b.x * v1 + b.y * v2 + b.z * v3;
            }

            return InterpolatePoint;
        }

        /// <summary>
        /// Linear interpolates from a triangle of size Sqrt(3) in the XZ plane to the triangle supplied by v1 to v3, returning the jacobi
        /// The z value of p is unused.
        /// </summary>
        /// <param name="v1">Final location of (0, 0, 1)</param>
        /// <param name="v2">Final location of (sqrt(3) / 2, 0, -0.5)</param>
        /// <param name="v3">Final location of (-sqrt(3) / 2, 0, 0.5)</param>
        public static Func<Vector3, Matrix4x4> Jacobi(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            Matrix4x4 JacobiPoint(Vector3 p)
            {
                var b = StdBarycentric(new Vector2(p.x, p.z));
                var (dbdx, dbdz) = StdBarycentricDiff();
                var o = b.x * v1 + b.y * v2 + b.z * v3;
                var dodx = dbdx.x * v1 + dbdx.y * v2 + dbdx.z * v3;
                var dodz = dbdz.x * v1 + dbdz.y * v2 + dbdz.z * v3;
                return VectorUtils.ToMatrix(dodx, Vector3.zero, dodz, o);

            }

            return JacobiPoint;
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

        private static (Vector3, Vector3) StdBarycentricDiff()
        {
            const float a = 1 / 3f;
            const float b = 2 / 3f;
            const float c = 0.57735026919f; // 1 / sqrt(3)

            return (new Vector3(0, c, -c), new Vector3(b, -a, -a));
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
