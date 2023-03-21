using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY
using UnityEngine;
#endif

using static Sylves.VectorUtils;

namespace Sylves
{
    public static class DeformationUtils
    {
        private static readonly Matrix4x4 RotateZY = new Matrix4x4(new Vector4(1, 0, 0, 0), new Vector4(0, 0, -1, 0), new Vector4(0, 1, 0, 0), new Vector4(0, 0, 0, 1));

        /// <summary>
        /// Returns a deformation that transforms from cell co-ordinates to a prism defined by the mesh and the given parameters.
        /// For quad meshes, cell co-ordinates is a unit cube centered at the origin.
        /// For tri meshes, cell co-ordinates are a triangle prism centered at the origin.
        /// </summary>
        public static Deformation GetDeformation(MeshData surfaceMesh, float tileHeight, float surfaceOffset, bool smoothNormals, int face, int layer, int subMesh, bool invertWinding)
        {
            var isQuads = surfaceMesh.GetTopology(subMesh) == MeshTopology.Quads;
            var isTris = surfaceMesh.GetTopology(subMesh) == MeshTopology.Triangles;

            if (!isQuads && !isTris)
                return null;

            var interpolatePoint = isQuads
                ? QuadInterpolation.InterpolatePosition(surfaceMesh, subMesh, face, invertWinding, tileHeight * layer + surfaceOffset - tileHeight / 2, tileHeight * layer + surfaceOffset + tileHeight / 2)
                : TriangleInterpolation.InterpolatePosition(surfaceMesh, subMesh, face, invertWinding, tileHeight * layer + surfaceOffset - tileHeight / 2, tileHeight * layer + surfaceOffset + tileHeight / 2);

            var jacobiPoint = isQuads
                ? QuadInterpolation.JacobiPosition(surfaceMesh, subMesh, face, invertWinding, tileHeight * layer + surfaceOffset - tileHeight / 2, tileHeight * layer + surfaceOffset + tileHeight / 2)
                : TriangleInterpolation.JacobiPosition(surfaceMesh, subMesh, face, invertWinding, tileHeight * layer + surfaceOffset - tileHeight / 2, tileHeight * layer + surfaceOffset + tileHeight / 2);

            var interpolateNormal = !smoothNormals ? null : isQuads
                ? QuadInterpolation.InterpolateNormal(surfaceMesh, subMesh, face, invertWinding)
                : TriangleInterpolation.InterpolateNormal(surfaceMesh, subMesh, face, invertWinding);

            var interpolateTangent = !smoothNormals ? null : isQuads
                ? QuadInterpolation.InterpolateTangent(surfaceMesh, subMesh, face, invertWinding)
                : TriangleInterpolation.InterpolateTangent(surfaceMesh, subMesh, face, invertWinding);

            var interpolateUv = !smoothNormals ? null : isQuads
                ? QuadInterpolation.InterpolateUv(surfaceMesh, subMesh, face, invertWinding)
                : TriangleInterpolation.InterpolateUv(surfaceMesh, subMesh, face, invertWinding);

            void GetJacobi(Vector3 p, out Matrix4x4 jacobi)
            {
                var m = 1e-3f;

                var j = jacobiPoint(p);

                if (!smoothNormals)
                {
                    jacobi = j;
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


                    var normal = interpolateNormal(p).normalized;
                    var tangent4 = interpolateTangent(p);
                    var tangent3 = ToVector3(tangent4).normalized;
                    var bitangent = (tangent4.w * Vector3.Cross(normal, tangent3)).normalized;

                    // TODO: Do some actual differentation
                    var t2 = interpolateUv(p);
                    var dx2 = (interpolateUv(p + Vector3.right * m) - t2) / m;
                    //var dy2 = (trilinearInterpolateUv(p + Vector3.up * m) - t2) / m;// Always zero
                    var dz2 = (interpolateUv(p + Vector3.forward * m) - t2) / m;

                    var j3 = ToMatrix(
                        new Vector3(dx2.x, 0, dx2.y).normalized,
                        new Vector3(0, 1, 0),
                        new Vector3(dz2.x, 0, dz2.y).normalized
                        );

                    var j1 = ToMatrix(
                        tangent3 *  j.GetColumn(0).magnitude,
                        normal * j.GetColumn(1).magnitude,
                        bitangent * j.GetColumn(2).magnitude);

                    jacobi = j1 * j3;
                    jacobi = new Matrix4x4(jacobi.GetColumn(0), jacobi.GetColumn(1), jacobi.GetColumn(2), j.GetColumn(3));
                }
            }

            var deformation = new Deformation(interpolatePoint, GetJacobi, invertWinding);
            return deformation;
        }


        /// <summary>
        /// Returns a deformation that transforms from cell co-ordinates to a prism defined by the mesh and the given parameters.
        /// For quad meshes, cell co-ordinates is a unit cube centered at the origin.
        /// For tri meshes, cell co-ordinates are a triangle prism centered at the origin.
        /// </summary>
        public static Deformation GetDeformation(MeshData surfaceMesh, int face, int subMesh, bool invertWinding)
        {
            if(surfaceMesh.normals != null)
            {
                return GetDeformation(surfaceMesh, 1.0f, 0f, false, face, 0, subMesh, invertWinding);
            }

            var isQuads = surfaceMesh.GetTopology(subMesh) == MeshTopology.Quads;
            var isTris = surfaceMesh.GetTopology(subMesh) == MeshTopology.Triangles;

            if (!isQuads && !isTris)
            {
                throw new Exception($"Cannot handle topology of type {surfaceMesh.GetTopology(subMesh)}");
            }

            var interpolatePoint = isQuads
                ? QuadInterpolation.InterpolatePosition(surfaceMesh, subMesh, face, invertWinding)
                : TriangleInterpolation.InterpolatePosition(surfaceMesh, subMesh, face, invertWinding);

            var jacobiPoint = isQuads
                ? QuadInterpolation.JacobiPosition(surfaceMesh, subMesh, face, invertWinding)
                : TriangleInterpolation.JacobiPosition(surfaceMesh, subMesh, face, invertWinding);

            void GetJacobi(Vector3 p, out Matrix4x4 jacobi)
            {
                jacobi = jacobiPoint(p);
            }

            var deformation = new Deformation(interpolatePoint, GetJacobi, invertWinding: invertWinding);
            return deformation;
        }

    }
}
