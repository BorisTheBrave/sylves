using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sylves
{
    /// <summary>
    /// A replacement for UnityEngine.Mesh that stores all the data in memory, for fast access from C#.
    /// </summary>
    public class MeshData
    {
        public int[][] indices;
        public Sylves.MeshTopology[] topologies;
        public Vector3[] vertices;
        public Vector2[] uv;
        public Vector3[] normals;
        public Vector4[] tangents;

        public int subMeshCount => topologies.Length;

        public MeshData()
        {

        }

        public MeshData(Mesh mesh)
        {
            this.indices = Enumerable.Range(0, mesh.subMeshCount).Select(mesh.GetIndices).ToArray();
            this.topologies = Enumerable.Range(0, mesh.subMeshCount).Select(x => (Sylves.MeshTopology)mesh.GetTopology(x)).ToArray();

            this.vertices = mesh.vertices;
            this.uv = mesh.uv == null || mesh.uv.Length == 0 ? null : mesh.uv;
            this.normals = mesh.normals == null || mesh.normals.Length == 0 ? null : mesh.normals;
            this.tangents = mesh.tangents == null || mesh.tangents.Length == 0 ? null : mesh.tangents;
        }

        public Mesh ToMesh()
        {
            var m = new Mesh();
            m.vertices = vertices;
            m.uv = uv;
            m.normals = normals;
            m.tangents = tangents;
            m.subMeshCount = subMeshCount;
            for (var i = 0; i < subMeshCount; i++)
            {
                switch (topologies[i])
                {
                    case MeshTopology.Triangles:
                    case MeshTopology.Quads:
                        m.SetIndices(indices[i], (UnityEngine.MeshTopology)topologies[i], i);
                        break;
                    default:
                        throw new System.Exception($"Topology {topologies[i]} not supported by unity");
                }
            }
            return m;
        }

        // Shallow clone
        public MeshData Clone()
        {
            return MemberwiseClone() as MeshData;
        }

        public int[] GetIndices(int submesh)
        {
            return indices[submesh];
        }

        public Sylves.MeshTopology GetTopology(int submesh)
        {
            return topologies[submesh];
        }

        public void RecalculateNormals()
        {
            if (normals == null || normals.Length != vertices.Length)
            {
                normals = new Vector3[vertices.Length];
            }

            // TODO: There's got to be an official way of doing this.
            // Implementation below is just a guess
            for (var submesh = 0; submesh < subMeshCount; submesh++)
            {
                foreach (var face in MeshUtils.GetFaces(this, submesh))
                {
                    for (var i = 0; i < face.Count; i++)
                    {
                        var i0 = face[i];
                        var i1 = face[(i + 1) % face.Count];
                        var i2 = face[(i + 2) % face.Count];
                        var v0 = vertices[i0];
                        var v1 = vertices[i1];
                        var v2 = vertices[i2];
                        normals[i1] += Vector3.Cross(v2 - v1, v0 - v1).normalized;
                    }
                }
            }
            for(var i = 0; i < normals.Length; i++)
            {
                normals[i] = normals[i].normalized;
            }

        }
        //public void RecalculateTangents();

        public static MeshData operator*(Matrix4x4 m, MeshData meshData)
        {
            var it = m.inverse.transpose;
            return new MeshData
            {
                indices = meshData.indices,
                topologies = meshData.topologies,
                vertices = meshData.vertices.Select(m.MultiplyPoint3x4).ToArray(),
                uv = meshData.uv,
                normals = meshData.normals?.Select(it.MultiplyVector).ToArray(),
                tangents = meshData.tangents?.Select(t =>
                {
                    var v = m.MultiplyVector(new Vector3(t.x, t.y, t.z));
                    return new Vector4(v.x, v.y, v.z, t.w);
                }).ToArray(),
            };
        }

        public MeshData Triangulate()
        {
            var indices = new List<IList<int>>();
            var topologies = new MeshTopology[subMeshCount];
            for (var i = 0; i < subMeshCount; i++)
            {
                if (this.topologies[i] == MeshTopology.Triangles)
                {
                    indices.Add(this.indices[i]);
                    topologies[i] = MeshTopology.Triangles;
                    continue;
                }
                // Convert faces
                var ii = new List<int>();
                foreach (var face in MeshUtils.GetFaces(this, i))
                {
                    // Fan. TODO: Check concavity?
                    for (var j = 2; j < face.Count; j++)
                    {
                        ii.Add(face[0]);
                        ii.Add(face[j - 1]);
                        ii.Add(face[j]);
                    }
                }
                indices.Add(ii);
                topologies[i] = MeshTopology.Triangles;
            }

            return new MeshData
            {
                indices = indices.Select(x => x.ToArray()).ToArray(),
                topologies = topologies,
                vertices = this.vertices,
                normals = this.normals,
                tangents = this.tangents,
                uv = this.uv,
            };
        }

        public MeshData InvertWinding()
        {
            var indices = new List<IList<int>>();
            for (var subMesh = 0; subMesh < subMeshCount; subMesh++)
            {
                var ii = new List<int>();
                foreach (var face in MeshUtils.GetFaces(this, subMesh, true))
                {
                    foreach(var i in face)
                    {
                        ii.Add(i);
                    }
                    if(topologies[subMesh] == MeshTopology.NGon)
                    {
                        ii[ii.Count - 1] = ~ii[ii.Count - 1];
                    }
                }
                indices.Add(ii);
            }

            return new MeshData
            {
                indices = indices.Select(x => x.ToArray()).ToArray(),
                topologies = this.topologies,
                vertices = this.vertices,
                normals = this.normals,
                tangents = this.tangents,
                uv = this.uv,
            };
        }
    }
}
