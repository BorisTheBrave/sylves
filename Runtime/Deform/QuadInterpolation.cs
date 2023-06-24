using System;
using UnityEngine;

namespace Sylves
{

    /// <summary>
    /// Supplies various bilinear and trilinear interpolation methods.
    /// The conventions are based on a XY plane
    /// using either a unit square or unit cube.
    /// </summary>
    public static class QuadInterpolation
    {
        // Follows mesh conventions for vertex order (i.e. v1-v4 are the face vertices and v5-v8 are repeats at a different normal offset).
        public static void GetCorners(MeshData mesh, int submesh, int face, bool invertWinding, float meshOffset1, float meshOffset2, out Vector3 v1, out Vector3 v2, out Vector3 v3, out Vector3 v4, out Vector3 v5, out Vector3 v6, out Vector3 v7, out Vector3 v8)
        {
            if (mesh.GetTopology(submesh) != MeshTopology.Quads)
            {
                throw new Exception($"Mesh topology {mesh.GetTopology(submesh)} not supported. You need to select \"Keep Quads\"");
            }

            var indices = mesh.GetIndices(submesh);
            var vertices = mesh.vertices;
            var normals = mesh.normals;
            int i1, i2, i3, i4;
            if(invertWinding)
            {

                i1 = indices[face * 4 + 3];
                i2 = indices[face * 4 + 2];
                i3 = indices[face * 4 + 1];
                i4 = indices[face * 4 + 0];
            }
            else
            {
                i1 = indices[face * 4 + 0];
                i2 = indices[face * 4 + 1];
                i3 = indices[face * 4 + 2];
                i4 = indices[face * 4 + 3];
            }
            // Find new bounding cage

            v1 = vertices[i1] + normals[i1] * meshOffset1;
            v2 = vertices[i2] + normals[i2] * meshOffset1;
            v3 = vertices[i3] + normals[i3] * meshOffset1;
            v4 = vertices[i4] + normals[i4] * meshOffset1;
            v5 = vertices[i1] + normals[i1] * meshOffset2;
            v6 = vertices[i2] + normals[i2] * meshOffset2;
            v7 = vertices[i3] + normals[i3] * meshOffset2;
            v8 = vertices[i4] + normals[i4] * meshOffset2;
        }

        /// <summary>
        /// Sets up a function that does trilinear interpolation from a unit cube centered on the origin
        /// to a cube made by extruding a given face of the mesh by meshOffset1 (for y=-0.5) and meshOffset2 (for y=0.5)
        /// </summary>
        public static Func<Vector3, Vector3> InterpolatePosition(MeshData mesh, int submesh, int face, bool invertWinding, float meshOffset1, float meshOffset2)
        {
            GetCorners(mesh, submesh, face, invertWinding, meshOffset1, meshOffset2, out Vector3 v1, out Vector3 v2, out Vector3 v3, out Vector3 v4, out Vector3 v5, out Vector3 v6, out Vector3 v7, out Vector3 v8);
            return Interpolate(v1, v2, v3, v4, v5, v6, v7, v8);
        }

        /// <summary>
        /// Returns the Jacobi (derivative) of InterpolatePosition
        /// </summary>
        public static Func<Vector3, Matrix4x4> JacobiPosition(MeshData mesh, int submesh, int face, bool invertWinding, float meshOffset1, float meshOffset2)
        {
            GetCorners(mesh, submesh, face, invertWinding, meshOffset1, meshOffset2, out Vector3 v1, out Vector3 v2, out Vector3 v3, out Vector3 v4, out Vector3 v5, out Vector3 v6, out Vector3 v7, out Vector3 v8);
            return Jacobi(v1, v2, v3, v4, v5, v6, v7, v8);
        }

        public static void GetCorners(MeshData mesh, int submesh, int face, bool invertWinding, out Vector3 v1, out Vector3 v2, out Vector3 v3, out Vector3 v4)
        {
            if (mesh.GetTopology(submesh) != MeshTopology.Quads)
            {
                throw new Exception($"Mesh topology {mesh.GetTopology(submesh)} not supported. You need to select \"Keep Quads\"");
            }

            var indices = mesh.GetIndices(submesh);
            var vertices = mesh.vertices;
            int i1, i2, i3, i4;
            if (invertWinding)
            {

                i1 = indices[face * 4 + 3];
                i2 = indices[face * 4 + 2];
                i3 = indices[face * 4 + 1];
                i4 = indices[face * 4 + 0];
            }
            else
            {
                i1 = indices[face * 4 + 0];
                i2 = indices[face * 4 + 1];
                i3 = indices[face * 4 + 2];
                i4 = indices[face * 4 + 3];
            }

            v1 = vertices[i1];
            v2 = vertices[i2];
            v3 = vertices[i3];
            v4 = vertices[i4];
        }

        public static Func<Vector3, Vector3> InterpolatePosition(MeshData mesh, int submesh, int face, bool invertWinding)
        {
            GetCorners(mesh, submesh, face, invertWinding, out Vector3 v1, out Vector3 v2, out Vector3 v3, out Vector3 v4);
            return Interpolate(v1, v2, v3, v4);
        }

        public static Func<Vector3, Matrix4x4> JacobiPosition(MeshData mesh, int submesh, int face, bool invertWinding)
        {
            GetCorners(mesh, submesh, face, invertWinding, out Vector3 v1, out Vector3 v2, out Vector3 v3, out Vector3 v4);
            return Jacobi(v1, v2, v3, v4);
        }

        public static Func<Vector3, Vector3> InterpolateNormal(MeshData mesh, int submesh, int face, bool invertWinding)
        {
            if (mesh.GetTopology(submesh) != MeshTopology.Quads)
            {
                throw new Exception($"Mesh topology {mesh.GetTopology(submesh)} not supported. You need to select \"Keep Quads\"");
            }

            var indices = mesh.GetIndices(submesh);
            var normals = mesh.normals;

            int i1, i2, i3, i4;
            if (invertWinding)
            {

                i1 = indices[face * 4 + 3];
                i2 = indices[face * 4 + 2];
                i3 = indices[face * 4 + 1];
                i4 = indices[face * 4 + 0];
            }
            else
            {
                i1 = indices[face * 4 + 0];
                i2 = indices[face * 4 + 1];
                i3 = indices[face * 4 + 2];
                i4 = indices[face * 4 + 3];
            }
            // Find new bounding cage

            var v1 = normals[i1];
            var v2 = normals[i2];
            var v3 = normals[i3];
            var v4 = normals[i4];

            return Interpolate(v1, v2, v3, v4);
        }

        public static Func<Vector3, Vector4> InterpolateTangent(MeshData mesh, int submesh, int face, bool invertWinding)
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
            int i1, i2, i3, i4;
            if (invertWinding)
            {

                i1 = indices[face * 4 + 3];
                i2 = indices[face * 4 + 2];
                i3 = indices[face * 4 + 1];
                i4 = indices[face * 4 + 0];
            }
            else
            {
                i1 = indices[face * 4 + 0];
                i2 = indices[face * 4 + 1];
                i3 = indices[face * 4 + 2];
                i4 = indices[face * 4 + 3];
            }
            // Find new bounding cage

            var v1 = tangents[i1];
            var v2 = tangents[i2];
            var v3 = tangents[i3];
            var v4 = tangents[i4];

            return Interpolate(v1, v2, v3, v4);
        }

        public static Func<Vector3, Vector2> InterpolateUv(MeshData mesh, int submesh, int face, bool invertWinding)
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
            int i1, i2, i3, i4;
            if (invertWinding)
            {

                i1 = indices[face * 4 + 3];
                i2 = indices[face * 4 + 2];
                i3 = indices[face * 4 + 1];
                i4 = indices[face * 4 + 0];
            }
            else
            {
                i1 = indices[face * 4 + 0];
                i2 = indices[face * 4 + 1];
                i3 = indices[face * 4 + 2];
                i4 = indices[face * 4 + 3];
            }
            // Find new bounding cage

            var v1 = uvs[i1];
            var v2 = uvs[i2];
            var v3 = uvs[i3];
            var v4 = uvs[i4];


            // TODO: Bilienar interpolate
            return Interpolate(v1, v2, v3, v4);
        }


        public static Func<Vector3, Matrix4x4> JacobiUv(MeshData mesh, int submesh, int face, bool invertWinding)
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
            int i1, i2, i3, i4;
            if (invertWinding)
            {

                i1 = indices[face * 4 + 3];
                i2 = indices[face * 4 + 2];
                i3 = indices[face * 4 + 1];
                i4 = indices[face * 4 + 0];
            }
            else
            {
                i1 = indices[face * 4 + 0];
                i2 = indices[face * 4 + 1];
                i3 = indices[face * 4 + 2];
                i4 = indices[face * 4 + 3];
            }
            // Find new bounding cage

            var v1 = uvs[i1];
            var v2 = uvs[i2];
            var v3 = uvs[i3];
            var v4 = uvs[i4];


            // TODO: Bilienar interpolate
            return Jacobi(v1, v2, v3, v4);
        }

        /// <summary>
        /// As the Vector3 Interpolate, only in 2 dimensions.
        /// </summary>
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
                var u1 = y1 * v4 + y2 * v3;
                var u2 = y1 * v1 + y2 * v2;
                var u3 = y1 * v8 + y2 * v7;
                var u4 = y1 * v5 + y2 * v6;
                var w1 = x1 * u1 + x2 * u2;
                var w2 = x1 * u3 + x2 * u4;
                var z = z1 * w1 + z2 * w2;
                return z;
            }

            return TrilinearInterpolatePoint;
        }

        /// <summary>
        /// As the Vector3 interpolate, only in 2 dimensions.
        /// </summary>
        public static Func<Vector3, Vector2> Interpolate(Vector2 v1, Vector2 v2, Vector2 v3, Vector2 v4)
        {
            Vector2 TrilinearInterpolatePoint(Vector3 p)
            {
                //Perform linear interpolation.
                var x1 = 0.5f - p.x;
                var x2 = 0.5f + p.x;
                var y1 = 0.5f - p.y;
                var y2 = 0.5f + p.y;
                var u1 = y1 * v4 + y2 * v3;
                var u2 = y1 * v1 + y2 * v2;
                return x1 * u1 + x2 * u2;
            }

            return TrilinearInterpolatePoint;
        }

        /// <summary>
        /// As the Vector3 jacobi, only in 2 dimensions.
        /// </summary>
        public static Func<Vector3, Matrix4x4> Jacobi(Vector2 v1, Vector2 v2, Vector2 v3, Vector2 v4)
        {
            Matrix4x4 TrilinearJacobiPoint(Vector3 p)
            {
                //Perform linear interpolation.
                var x1 = 0.5f - p.x;
                var x2 = 0.5f + p.x;
                var y1 = 0.5f - p.y;
                var y2 = 0.5f + p.y;
                var u1 = y1 * v4 + y2 * v3;
                var u2 = y1 * v1 + y2 * v2;
                var du1dy = v3 - v4;
                var du2dy = v2 - v1;
                var o = x1 * u1 + x2 * u2;
                var dodx = u2 - u1;
                var dody = x1 * du1dy + x2 * du2dy;
                return VectorUtils.ToMatrix(new Vector3(dodx.x, dodx.y, 0), new Vector3(dody.x, dody.y, 0), Vector3.zero, new Vector3(o.x, o.y, 0));
            }

            return TrilinearJacobiPoint;
        }


        /// <summary>
        /// Tiilinear interpolates from a unit cube to the polyhedron supplied by v1 to v8
        /// The z value of p is unused.
        /// </summary>
        /// <param name="v1">Final location of (-0.5, -0.5, -0.5)</param>
        /// <param name="v2">Final location of (-0.5, -0.5, 0.5)</param>
        /// <param name="v3">Final location of (0.5, -0.5, 0.5)</param>
        /// <param name="v4">Final location of (0.5, -0.5, -0.5)</param>
        /// <param name="v5">Final location of (-0.5, 0.5, -0.5)</param>
        /// <param name="v6">Final location of (-0.5, 0.5, 0.5)</param>
        /// <param name="v7">Final location of (0.5, 0.5, 0.5)</param>
        /// <param name="v8">Final location of (0.5, 0.5, -0.5)</param>
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
                var u1 = y1 * v4 + y2 * v3;
                var u2 = y1 * v1 + y2 * v2;
                var u3 = y1 * v8 + y2 * v7;
                var u4 = y1 * v5 + y2 * v6;
                var w1 = x1 * u1 + x2 * u2;
                var w2 = x1 * u3 + x2 * u4;
                var z = z1 * w1 + z2 * w2;
                return z;
            }

            return TrilinearInterpolatePoint;
        }


        /// <summary>
        /// Tiilinear interpolates from a unit cube to the polyhedron supplied by v1 to v8, returning the jacobi
        /// The z value of p is unused.
        /// </summary>
        /// <param name="v1">Final location of (-0.5, -0.5, -0.5)</param>
        /// <param name="v2">Final location of (-0.5, -0.5, 0.5)</param>
        /// <param name="v3">Final location of (0.5, -0.5, 0.5)</param>
        /// <param name="v4">Final location of (0.5, -0.5, -0.5)</param>
        /// <param name="v5">Final location of (-0.5, 0.5, -0.5)</param>
        /// <param name="v6">Final location of (-0.5, 0.5, 0.5)</param>
        /// <param name="v7">Final location of (0.5, 0.5, 0.5)</param>
        /// <param name="v8">Final location of (0.5, 0.5, -0.5)</param>
        public static Func<Vector3, Matrix4x4> Jacobi(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 v5, Vector3 v6, Vector3 v7, Vector3 v8)
        {
            Matrix4x4 TrilinearJacobiPoint(Vector3 p)
            {
                //Perform linear interpolation and it's derivative
                var x1 = 0.5f - p.x;
                var x2 = 0.5f + p.x;
                var y1 = 0.5f - p.y;
                var y2 = 0.5f + p.y;
                var z1 = 0.5f - p.z;
                var z2 = 0.5f + p.z;
                var u1 = y1 * v4 + y2 * v3;
                var u2 = y1 * v1 + y2 * v2;
                var u3 = y1 * v8 + y2 * v7;
                var u4 = y1 * v5 + y2 * v6;
                var du1dy = v3 - v4;
                var du2dy = v2 - v1;
                var du3dy = v7 - v8;
                var du4dy = v6 - v5;
                var w1 = x1 * u1 + x2 * u2;
                var w2 = x1 * u3 + x2 * u4;
                var dw1dy = x1 * du1dy + x2 * du2dy;
                var dw2dy = x1 * du3dy + x2 * du4dy;
                var dw1dx = u2 - u1;
                var dw2dx = u4 - u3;
                var z = z1 * w1 + z2 * w2;
                var dzdy = z1 * dw1dy + z2 * dw2dy;
                var dzdx = z1 * dw1dx + z2 * dw2dx;
                var dzdz = w2 - w1;
                return VectorUtils.ToMatrix(dzdx, dzdy, dzdz, z);
            }

            return TrilinearJacobiPoint;
        }

        /// <summary>
        /// Bilinear interpolates from a unit square in the XZ plane to the quad supplied by v1 to v4
        /// The z value of p is unused.
        /// </summary>
        /// <param name="v1">Final location of (-0.5, 0, -0.5)</param>
        /// <param name="v2">Final location of (-0.5, 0, 0.5)</param>
        /// <param name="v3">Final location of (0.5, 0, 0.5)</param>
        /// <param name="v4">Final location of (0.5, 0, -0.5)</param>
        public static Func<Vector3, Vector3> Interpolate(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
        {
            Vector3 TrilinearInterpolatePoint(Vector3 p)
            {
                //Perform linear interpolation.
                var x1 = 0.5f - p.x;
                var x2 = 0.5f + p.x;
                var y1 = 0.5f - p.y;
                var y2 = 0.5f + p.y;
                var u1 = y1 * v4 + y2 * v3;
                var u2 = y1 * v1 + y2 * v2;
                return x1 * u1 + x2 * u2;
            }

            return TrilinearInterpolatePoint;
        }

        /// <summary>
        /// Bilinear interpolates from a unit square in the XZ plane to the quad supplied by v1 to v4, returning the jacobi
        /// The z value of p is unused.
        /// </summary>
        /// <param name="v1">Final location of (-0.5, 0, -0.5)</param>
        /// <param name="v2">Final location of (-0.5, 0, 0.5)</param>
        /// <param name="v3">Final location of (0.5, 0, 0.5)</param>
        /// <param name="v4">Final location of (0.5, 0, -0.5)</param>
        public static Func<Vector3, Matrix4x4> Jacobi(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
        {
            Matrix4x4 TrilinearJacobiPoint(Vector3 p)
            {
                //Perform linear interpolation.
                var x1 = 0.5f - p.x;
                var x2 = 0.5f + p.x;
                var y1 = 0.5f - p.y;
                var y2 = 0.5f + p.y;
                var u1 = y1 * v4 + y2 * v3;
                var u2 = y1 * v1 + y2 * v2;
                var du1dy = v3 - v4;
                var du2dy = v2 - v1;
                var o = x1 * u1 + x2 * u2;
                var dodx = u2 - u1;
                var dody = x1 * du1dy + x2 * du2dy;
                return VectorUtils.ToMatrix(dodx, dody, Vector3.zero, o);
            }

            return TrilinearJacobiPoint;
        }


        /// <summary>
        /// As the Vector3 interpolate, only in 4 dimensions.
        /// </summary>
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
                var u1 = y1 * v4 + y2 * v3;
                var u2 = y1 * v1 + y2 * v2;
                var u3 = y1 * v8 + y2 * v7;
                var u4 = y1 * v5 + y2 * v6;
                var w1 = x1 * u1 + x2 * u2;
                var w2 = x1 * u3 + x2 * u4;
                var z = z1 * w1 + z2 * w2;
                return z;
            }

            return TrilinearInterpolatePoint;
        }

        /// <summary>
        /// As the Vector3 interpolate, only in 4 dimensions.
        /// </summary>
        public static Func<Vector3, Vector4> Interpolate(Vector4 v1, Vector4 v2, Vector4 v3, Vector4 v4)
        {
            Vector4 TrilinearInterpolatePoint(Vector3 p)
            {
                //Perform linear interpolation.
                var x1 = 0.5f - p.x;
                var x2 = 0.5f + p.x;
                var y1 = 0.5f - p.y;
                var y2 = 0.5f + p.y;
                var u1 = y1 * v4 + y2 * v3;
                var u2 = y1 * v1 + y2 * v2;
                return x1 * u1 + x2 * u2;
            }

            return TrilinearInterpolatePoint;
        }
    }
}
