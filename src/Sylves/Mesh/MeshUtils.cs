using System;
using System.Collections.Generic;
using System.Linq;

namespace Sylves
{
    /// <summary>
    /// Utility for working with meshes.
    /// </summary>
    public static class MeshUtils
    {
        // Creates an axis aligned cube that corresponds with a box collider
        internal static MeshData CreateBoxMesh(Vector3 center, Vector3 size)
        {
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
            vertices = vertices.Select(v => center + Vector3.Scale(size, v)).ToArray();
            int[] triangles = {
                0, 2, 1,
	            0, 3, 2,
                2, 3, 4,
	            2, 4, 5,
                1, 2, 5,
	            1, 5, 6,
                0, 7, 4,
	            0, 4, 3,
                5, 4, 7,
	            5, 7, 6,
                0, 6, 7,
	            0, 1, 6
            };

            var mesh = new MeshData();
            mesh.vertices = vertices;
            mesh.indices = new[] { triangles };
            mesh.topologies = new [] { MeshTopology.Triangles };
            return mesh;
        }

        /// <summary>
        /// Returns a deformation that transforms from cell co-ordinates to a prism defined by the mesh and the given parameters.
        /// For quad meshes, cell co-ordinates is a unit cube centered at the origin.
        /// For tri meshes, cell co-ordinates are a triangle prism centered at the origin.
        /// </summary>
        public static Deformation GetDeformation(MeshData surfaceMesh, float tileHeight, float surfaceOffset, bool smoothNormals, int face, int layer, int subMesh)
        {
            var isQuads = surfaceMesh.GetTopology(subMesh) == MeshTopology.Quads;
            var isTris = surfaceMesh.GetTopology(subMesh) == MeshTopology.Triangles;

            if (!isQuads && !isTris)
                throw new Exception($"Cannot handle topology of type {surfaceMesh.GetTopology(subMesh)}");

            var trilinearInterpolatePoint = isQuads 
                ? QuadInterpolation.InterpolatePosition(surfaceMesh, subMesh, face, tileHeight * layer + surfaceOffset - tileHeight / 2, tileHeight * layer + surfaceOffset + tileHeight / 2)
                : TriangleInterpolation.InterpolatePosition(surfaceMesh, subMesh, face, tileHeight * layer + surfaceOffset - tileHeight / 2, tileHeight * layer + surfaceOffset + tileHeight / 2);

            var trilinearInterpolateNormal = !smoothNormals ? null : isQuads
                ? QuadInterpolation.InterpolateNormal(surfaceMesh, subMesh, face)
                : TriangleInterpolation.InterpolateNormal(surfaceMesh, subMesh, face);

            var trilinearInterpolateTangent = !smoothNormals ? null : isQuads
                ? QuadInterpolation.InterpolateTangent(surfaceMesh, subMesh, face)
                : TriangleInterpolation.InterpolateTangent(surfaceMesh, subMesh, face);

            var trilinearInterpolateUv = !smoothNormals ? null : isQuads
                ? QuadInterpolation.InterpolateUv(surfaceMesh, subMesh, face)
                : TriangleInterpolation.InterpolateUv(surfaceMesh, subMesh, face);

            void GetJacobi(Vector3 p, out Matrix4x4 jacobi)
            {
                var m = 1e-3f;

                // TODO: Do some actual differentation
                var t = trilinearInterpolatePoint(p);
                var dx = (trilinearInterpolatePoint(p + Vector3.right * m) - t) / m;
                var dy = (trilinearInterpolatePoint(p + Vector3.up * m) - t) / m;
                var dz = (trilinearInterpolatePoint(p + Vector3.forward * m) - t) / m;

                if (!smoothNormals)
                {
                    jacobi = new Matrix4x4(
                        ToVector4(dx), 
                        ToVector4(dy), 
                        ToVector4(dz), 
                        new Vector4(0, 0, 0, 1));
                }
                else
                {
                    // If you want normals that are continuous on the boundary between cells,
                    // we cannot use the actual jacobi matrix (above) as it is discontinuous.

                    // The same problem comes up for uv interpolation, which is why many meshes
                    // come with a precalculated tangent field for bump mapping etc.

                    // We can re-use that pre-computation by calculating the difference between
                    // the naive uv jacobi and the one given by the tangents, and then
                    // applying that to interpolation jacobi

                    // This code is not 100% correct, but it seems to give acceptable results.
                    // TODO: Do we really need all the normalization?


                    var normal = trilinearInterpolateNormal(p).normalized;
                    var tangent4 = trilinearInterpolateTangent(p);
                    var tangent3 = ToVector3(tangent4).normalized;
                    var bitangent = (tangent4.w * Vector3.Cross(normal, tangent3)).normalized;

                    // TODO: Do some actual differentation
                    var t2 = trilinearInterpolateUv(p);
                    var dx2 = (trilinearInterpolateUv(p + Vector3.right * m) - t2) / m;
                    //var dy2 = (trilinearInterpolateUv(p + Vector3.up * m) - t2) / m;// Always zero
                    var dz2 = (trilinearInterpolateUv(p + Vector3.forward * m) - t2) / m;

                    var j3 = new Matrix4x4(
                        ToVector4(new Vector3(dx2.x, 0, dx2.y).normalized),
                        ToVector4(new Vector3(0, 1, 0)),
                        ToVector4(new Vector3(dz2.x, 0, dz2.y).normalized),
                        new Vector4(0, 0, 0, 1)
                        );

                    var j1 = new Matrix4x4(
                        ToVector4(tangent3 * dx.magnitude),
                        ToVector4(normal * dy.magnitude),
                        ToVector4(bitangent * dz.magnitude),
                        new Vector4(0, 0, 0, 1));

                    jacobi = j3 * j1;
                }
            }

            Vector3 DeformNormal(Vector3 p, Vector3 v)
            {
                GetJacobi(p, out var jacobi);
                return jacobi.inverse.transpose.MultiplyVector(v).normalized;
            }

            Vector4 DeformTangent(Vector3 p, Vector4 v)
            {
                GetJacobi(p, out var jacobi);
                return jacobi * v;
            }

            return new Deformation(trilinearInterpolatePoint, DeformNormal, DeformTangent, false);
        }

        private static Vector4 ToVector4(Vector3 v) => new Vector4(v.x, v.y, v.z, 0);
        private static Vector3 ToVector3(Vector4 v) => new Vector3(v.x, v.y, v.z);
    }
}
