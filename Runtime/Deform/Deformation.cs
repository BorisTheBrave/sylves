using System;
using System.Linq;
using UnityEngine;

namespace Sylves
{
    /// <summary>
    /// A deformation is a continuous, differentable mapping from one space to another.
    /// It is used to warp meshes in arbitrary ways, by mapping the vertices, normals and tangents of the mesh.
    /// </summary>
    public class Deformation
    {
        public delegate void GetJacobiFunc(Vector3 p, out Matrix4x4 jacobi);

        public Deformation(Func<Vector3, Vector3> deformPoint, Func<Vector3, Vector3, Vector3> deformNormal, Func<Vector3, Vector4, Vector4> deformTangent, GetJacobiFunc getJacobi, bool invertWinding)
        {
            InnerDeformPoint = deformPoint;
            InnerDeformNormal = deformNormal;
            InnerDeformTangent = deformTangent;
            InnerGetJacobi = getJacobi;
            InnerInvertWinding = invertWinding;
        }

        // Variant constructors where we build the other functiosn needed

        public Deformation(Func<Vector3, Vector3> deformPoint, GetJacobiFunc getJacobi, bool invertWinding)
        {
            Vector3 DeformNormal(Vector3 p, Vector3 v)
            {
                getJacobi(p, out var j);
                return j.inverse.transpose.MultiplyVector(v).normalized;
            }

            Vector4 DeformTangent(Vector3 p, Vector4 v)
            {
                getJacobi(p, out var j);
                return j * v;
            }

            InnerDeformPoint = deformPoint;
            InnerDeformNormal = DeformNormal;
            InnerDeformTangent = DeformTangent;
            InnerGetJacobi = getJacobi;
            InnerInvertWinding = invertWinding;
        }

        public Deformation(Func<Vector3, Vector3> deformPoint, float step = 1e-3f, bool invertWinding = false)
        {
            void GetJacobi(Vector3 p, out Matrix4x4 j)
            {
                var m = step;

                // Numerical differentation
                var t = deformPoint(p);
                var dx = (deformPoint(p + Vector3.right * m) - t) / m;
                var dy = (deformPoint(p + Vector3.up * m) - t) / m;
                var dz = (deformPoint(p + Vector3.forward * m) - t) / m;

                j = VectorUtils.ToMatrix(dx, dy, dz, t);
            }

            Vector3 DeformNormal(Vector3 p, Vector3 v)
            {
                GetJacobi(p, out var j);
                return j.inverse.transpose.MultiplyVector(v).normalized;
            }

            Vector4 DeformTangent(Vector3 p, Vector4 v)
            {
                GetJacobi(p, out var j);
                return j * v;
            }

            InnerDeformPoint = deformPoint;
            InnerDeformNormal = DeformNormal;
            InnerDeformTangent = DeformTangent;
            InnerGetJacobi = GetJacobi;
            InnerInvertWinding = invertWinding;
        }

        private static void GetIdentityJacobi(Vector3 p, out Matrix4x4 j)
        {
            j = Matrix4x4.Translate(p);
        }

        public static Deformation Identity = new Deformation(p => p, (p, n) => n, (p, t) => t, GetIdentityJacobi, false);

        public Func<Vector3, Vector3> InnerDeformPoint { get; set; }
        public Func<Vector3, Vector3, Vector3> InnerDeformNormal { get; set; }
        public Func<Vector3, Vector4, Vector4> InnerDeformTangent { get; set; }
        public GetJacobiFunc InnerGetJacobi { get; set; }
        public bool InnerInvertWinding { get; set; }

        public Matrix4x4 PreDeform = Matrix4x4.identity;
        public Matrix4x4 PostDeform = Matrix4x4.identity;
        public Matrix4x4 PreDeformIT = Matrix4x4.identity;
        public Matrix4x4 PostDeformIT = Matrix4x4.identity;

        public bool InvertWinding => InnerInvertWinding ^ (PreDeform.determinant < 0) ^ (PostDeform.determinant < 0);

        public Deformation Clone()
        {
            return (Deformation)MemberwiseClone();
        }

        public Vector3 DeformPoint(Vector3 p)
        {
            return PostDeform.MultiplyPoint3x4(InnerDeformPoint(PreDeform.MultiplyPoint3x4(p)));
        }

        public Vector3 DeformNormal(Vector3 p, Vector3 v)
        {
            return PostDeformIT.MultiplyVector(InnerDeformNormal(PreDeform.MultiplyPoint3x4(p), PreDeformIT.MultiplyVector(v)));
        }

        public Vector4 DeformTangent(Vector3 p, Vector4 t)
        {
            Vector3 t2 = PreDeform.MultiplyVector(new Vector3(t.x, t.y, t.z));
            Vector4 t3 = new Vector4(t2.x, t2.y, t2.z, t.w);
            Vector4 t4 = InnerDeformTangent(PreDeform.MultiplyPoint3x4(p), t3);
            Vector3 t5 = PostDeform.MultiplyVector(new Vector3(t4.x, t4.y, t4.z));
            return new Vector4(t5.x, t5.y, t5.z, t4.w);
        }

        public void GetJacobi(Vector3 p, out Matrix4x4 j)
        {
            InnerGetJacobi(PreDeform.MultiplyPoint3x4(p), out var ij);
            var x = PostDeform.MultiplyVector(ij.MultiplyVector(PreDeform.MultiplyVector(Vector3.right)));
            var y = PostDeform.MultiplyVector(ij.MultiplyVector(PreDeform.MultiplyVector(Vector3.up)));
            var z = PostDeform.MultiplyVector(ij.MultiplyVector(PreDeform.MultiplyVector(Vector3.forward)));
            j = VectorUtils.ToMatrix(x, y, z, PostDeform.MultiplyPoint3x4(VectorUtils.ToVector3(ij.GetColumn(3))));
        }

        public Matrix4x4 GetJacobi(Vector3 p)
        {
            GetJacobi(p, out var j);
            return j;
        }

// TODO: Operate on MeshData isntead?
        private Mesh Deform(Mesh mesh, int submeshStart, int submeshCount)
        {
            var newMesh = new Mesh();
            newMesh.subMeshCount = submeshCount;

            // TODO: Use jacobi instead?

            // Copy deformed data
            var vertices = mesh.vertices;
            var vertexCount = vertices.Length;
            var newVertices = new Vector3[vertexCount];
            for (var i = 0; i < vertexCount; i++)
            {
                var p = vertices[i];
                newVertices[i] = DeformPoint(p);
            }
            newMesh.vertices = newVertices;

            var normals = mesh.normals;
            if (normals != null)
            {
                var normalCount = normals.Length;
                var newNormals = new Vector3[normalCount];
                for (var i = 0; i < vertexCount; i++)
                {
                    var p = vertices[i];
                    if (i < normalCount)
                    {
                        newNormals[i] = DeformNormal(p, normals[i]);
                    }
                }
                newMesh.normals = newNormals;
            }
            
            var tangents = mesh.tangents;
            if (tangents != null)
            {
                var tangentCount = tangents.Length;
                var newTangents = new Vector4[tangentCount];
                for (var i = 0; i < vertexCount; i++)
                {
                    var p = vertices[i];
                    if (i < tangentCount)
                    {
                        newTangents[i] = DeformTangent(p, tangents[i]);
                    }
                }
                newMesh.tangents = newTangents;
            }


            // Copy untransformed data
            newMesh.uv = mesh.uv;
            newMesh.uv2 = mesh.uv2;
            newMesh.uv3 = mesh.uv3;
            newMesh.uv4 = mesh.uv4;
            newMesh.uv5 = mesh.uv5;
            newMesh.uv6 = mesh.uv6;
            newMesh.uv7 = mesh.uv7;
            newMesh.uv8 = mesh.uv8;
            newMesh.colors = mesh.colors;
            newMesh.colors32 = mesh.colors32;

            // Copy indices
            for (var i = 0; i < submeshCount; i++)
            {
                var indices = mesh.GetIndices(submeshStart + i, false);
                indices = InvertWinding ? indices.Reverse().ToArray() : indices;
                newMesh.SetIndices(indices, mesh.GetTopology(submeshStart + i), i, true, (int)mesh.GetBaseVertex(submeshStart + i));
            }

            newMesh.name = mesh.name + "(Clone)";

            return newMesh;
        }

        /// <summary>
        /// Deforms the vertices and normals of a mesh as specified.
        /// </summary>
        public Mesh Deform(Mesh mesh)
        {
            return Deform(mesh, 0, mesh.subMeshCount);
        }

        /// <summary>
        /// Transforms the vertices and normals of a submesh mesh as specified.
        /// </summary>
        public Mesh Transform(Mesh mesh, int submesh)
        {
            return Deform(mesh, submesh, 1);
        }

        public static Deformation operator *(Deformation meshDeformation, Matrix4x4 m)
        {
            var r = meshDeformation.Clone();
            r.PreDeform = r.PreDeform * m;
            r.PreDeformIT = r.PreDeformIT * m.inverse.transpose;
            return r;
        }

        public static Deformation operator *(Matrix4x4 m, Deformation meshDeformation)
        {
            var r = meshDeformation.Clone();
            r.PostDeform = m * r.PostDeform;
            r.PostDeformIT = m.inverse.transpose * r.PostDeformIT;
            return r;
        }
    }
}
