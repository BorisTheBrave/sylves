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
    /// <summary>
    /// Utility for working with meshes.
    /// </summary>
    public static class MeshUtils
    {
        private static readonly Matrix4x4 RotateZY = new Matrix4x4(new Vector4(1, 0, 0, 0), new Vector4(0, 0, -1, 0), new Vector4(0, 1, 0, 0), new Vector4(0, 0, 0, 1));

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

        internal static bool IsPointInCube(Vector3 p, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 v5, Vector3 v6, Vector3 v7, Vector3 v8)
        {
            // Assumes the "cube" at least is convex and has planar edges. Otherwise, a more robost routing is needed
            bool Check(Vector3 a, Vector3 b, Vector3 c) => Vector3.Dot(Vector3.Cross((b - a), (c - a)), (p - a)) >= 0;
            if (!Check(v1, v2, v3)) return false;// Down
            if (!Check(v7, v6, v5)) return false;// Up
            if (!Check(v5, v6, v1)) return false;// Left
            if (!Check(v7, v8, v4)) return false;// Right
            if (!Check(v1, v4, v8)) return false;// Forward
            if (!Check(v6, v7, v3)) return false;// Back
            return true;
        }

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
                throw new Exception($"Cannot handle topology of type {surfaceMesh.GetTopology(subMesh)}");

            var interpolatePoint = isQuads
                ? QuadInterpolation.InterpolatePosition(surfaceMesh, subMesh, face, invertWinding, tileHeight * layer + surfaceOffset - tileHeight / 2, tileHeight * layer + surfaceOffset + tileHeight / 2)
                : TriangleInterpolation.InterpolatePosition(surfaceMesh, subMesh, face, invertWinding, tileHeight * layer + surfaceOffset - tileHeight / 2, tileHeight * layer + surfaceOffset + tileHeight / 2);

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

                // TODO: Do some actual differentation
                var t = interpolatePoint(p);
                var dx = (interpolatePoint(p + Vector3.right * m) - t) / m;
                var dy = (interpolatePoint(p + Vector3.up * m) - t) / m;
                var dz = (interpolatePoint(p + Vector3.forward * m) - t) / m;

                if (!smoothNormals)
                {
                    jacobi = ToMatrix(dx, dy, dz);
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
                        tangent3 * dx.magnitude,
                        normal * dy.magnitude,
                        bitangent * dz.magnitude);

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

            var deformation = new Deformation(interpolatePoint, DeformNormal, DeformTangent, invertWinding);
            // Adjusts from Deformation (XZ) conventions to Mesh conventions (XY and a different scale)
            deformation = isQuads
                ? deformation * ((CubeRotation)(CellRotation)0x821).ToMatrix()
                : deformation * ((CubeRotation)(CellRotation)0x821).ToMatrix() * Matrix4x4.Rotate(Quaternion.Euler(0, 0, 120)) * Matrix4x4.Scale(new Vector3(Mathf.Sqrt(3), Mathf.Sqrt(3), 1));
            return deformation;
        }


        /// <summary>
        /// Returns a deformation that transforms from cell co-ordinates to a prism defined by the mesh and the given parameters.
        /// For quad meshes, cell co-ordinates is a unit cube centered at the origin.
        /// For tri meshes, cell co-ordinates are a triangle prism centered at the origin.
        /// </summary>
        public static Deformation GetDeformation(MeshData surfaceMesh, int face, int subMesh, bool invertWinding)
        {
            var isQuads = surfaceMesh.GetTopology(subMesh) == MeshTopology.Quads;
            var isTris = surfaceMesh.GetTopology(subMesh) == MeshTopology.Triangles;

            if (!isQuads && !isTris)
            {
                throw new Exception($"Cannot handle topology of type {surfaceMesh.GetTopology(subMesh)}");
            }

            var interpolatePoint = isQuads
                ? QuadInterpolation.InterpolatePosition(surfaceMesh, subMesh, face, invertWinding)
                : TriangleInterpolation.InterpolatePosition(surfaceMesh, subMesh, face, invertWinding);

            var deformation = new Deformation(interpolatePoint, null, null, invertWinding);
            // Adjusts from Deformation (XZ) conventions to Mesh conventions (XY).
            deformation = deformation * ((CubeRotation)(CellRotation)0x821).ToMatrix();
            return deformation;
        }

        public struct Face : IEnumerable<int>
        {
            public Face(int[] indices, int offset, int length, bool invertWinding, bool negateTail = false)
            {
                this.Indices = indices;
                this.Offset = offset;
                this.Length = length;
                this.InvertWinding = invertWinding;
                this.NegateTail = negateTail;
            }

            public int[] Indices { get; set; }
            public int Offset { get; set; }
            public int Length { get; set; }
            public bool InvertWinding { get; set; }
            public bool NegateTail { get; }

            public int Count => Length;

            public IEnumerator<int> GetEnumerator()
            {
                if (NegateTail)
                {
                    if (InvertWinding)
                    {
                        int i;
                        for (i = 0; i < Length - 1; i++)
                        {
                            yield return Indices[Offset - i];
                        }
                        yield return ~Indices[Offset - i];
                    }
                    else
                    {
                        int i;
                        for (i = 0; i < Length - 1; i++)
                        {
                            yield return Indices[Offset + i];
                        }
                        yield return ~Indices[Offset + i];
                    }
                }
                else
                {
                    if (InvertWinding)
                    {
                        for (var i = 0; i < Length; i++)
                        {
                            yield return Indices[Offset - i];
                        }
                    }
                    else
                    {
                        for (var i = 0; i < Length; i++)
                        {
                            yield return Indices[Offset + i];
                        }
                    }
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }

            public int this[int i]
            {
                get
                {
                    var index = InvertWinding ? Indices[Offset - i] : Indices[Offset + i];
                    if (NegateTail && i == Length - 1) index = ~index;
                    return index;
                }
            }
        }

        /// <summary>
        /// Returns the indices of the faces of a submesh of meshData.
        /// </summary>
        /// TODO: Should we make a low alloc version of this?
        public static IEnumerable<Face> GetFaces(MeshData meshData, int subMesh, bool invertWinding = false)
        {
            var indices = meshData.GetIndices(subMesh);

            switch (meshData.GetTopology(subMesh))
            {
                case MeshTopology.Quads:
                    for (var i = 0; i < indices.Length; i += 4)
                    {
                        yield return new Face(indices, i + (invertWinding ? 3 : 0), 4, invertWinding);
                    }
                    break;
                case MeshTopology.Triangles:
                    for (var i = 0; i < indices.Length; i += 3)
                    {
                        yield return new Face(indices, i + (invertWinding ? 2 : 0), 3, invertWinding);
                    }
                    break;
                case MeshTopology.NGon:
                    {
                        if (invertWinding)
                            throw new NotImplementedException();
                        var i = 0;
                        while (i < indices.Length)
                        {
                            var i2 = i;
                            while(i2 < indices.Length -1 && indices[i2] >= 0)
                            {
                                i2++;
                            }
                            i2++;
                            yield return new Face(indices, i, i2 - i, false, true);
                            i = i2;
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
