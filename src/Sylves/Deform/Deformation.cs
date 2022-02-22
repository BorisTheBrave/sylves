using System;
using System.Linq;

namespace Sylves
{
    /// <summary>
    /// A deformation is a continuous, differentable mapping from one space to another.
    /// It is used to warp meshes in arbitrary ways, by mapping the vertices, normals and tangents of the mesh.
    /// </summary>
    public class Deformation
    {
        public Deformation(Func<Vector3, Vector3> deformPoint, Func<Vector3, Vector3, Vector3> deformNormal, Func<Vector3, Vector4, Vector4> deformTangent, bool invertWinding)
        {
            InnerDeformPoint = deformPoint;
            InnerDeformNormal = deformNormal;
            InnerDeformTangent = deformTangent;
            InnerInvertWinding = invertWinding;
        }

        public static Deformation Identity = new Deformation(p => p, (p, n) => n, (p, t) => t, false);

        public Func<Vector3, Vector3> InnerDeformPoint { get; set; }
        public Func<Vector3, Vector3, Vector3> InnerDeformNormal { get; set; }
        public Func<Vector3, Vector4, Vector4> InnerDeformTangent { get; set; }
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

        private Vector4 DeformTangent(Vector3 p, Vector4 t)
        {
            Vector3 t2 = PreDeform.MultiplyVector(new Vector3(t.x, t.y, t.z));
            Vector4 t3 = new Vector4(t2.x, t2.y, t2.z, t.w);
            Vector4 t4 = InnerDeformTangent(PreDeform.MultiplyPoint3x4(p), t3);
            Vector3 t5 = PostDeform.MultiplyVector(new Vector3(t4.x, t4.y, t4.z));
            return new Vector4(t5.x, t5.y, t5.z, t4.w);
        }

// TODO: Operate on MeshData isntead?
#if UNITY
        private Mesh Deform(Mesh mesh, int submeshStart, int submeshCount)
        {
            var newMesh = new Mesh();
            newMesh.subMeshCount = submeshCount;

            // Copy deformed data
            var vertices = mesh.vertices;
            var normals = mesh.normals;
            var tangents = mesh.tangents;
            var vertexCount = vertices.Length;
            var normalCount = normals.Length;
            var tangentCount = tangents.Length;
            var newVertices = new Vector3[vertexCount];
            var newNormals = new Vector3[normalCount];
            var newTangents = new Vector4[tangentCount];
            for (var i = 0; i < vertexCount; i++)
            {
                var p = vertices[i];
                newVertices[i] = DeformPoint(p);
                if (i < normalCount)
                {
                    newNormals[i] = DeformNormal(p, normals[i]);
                }
                if (i < tangentCount)
                {
                    newTangents[i] = DeformTangent(p, tangents[i]);
                }
            }
            newMesh.vertices = newVertices;
            newMesh.normals = newNormals;
            newMesh.tangents = newTangents;

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
#endif

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
