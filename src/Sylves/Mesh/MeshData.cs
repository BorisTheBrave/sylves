using System.Linq;
#if UNITY
using UnityEngine;
#endif

namespace Sylves
{
    /// <summary>
    /// A replacement for UnityEngine.Mesh that stores all the data in memory, for fast access from C#.
    /// </summary>
    public class MeshData
    {
        public int subMeshCount;
        public int[][] indices;
        public Sylves.MeshTopology[] topologies;
        public Vector3[] vertices;
        public Vector2[] uv;
        public Vector3[] normals;
        public Vector4[] tangents;

        public MeshData()
        {

        }

#if UNITY
        public MeshData(Mesh mesh)
        {
            this.subMeshCount = mesh.subMeshCount;
            this.indices = Enumerable.Range(0, subMeshCount).Select(mesh.GetIndices).ToArray();
            this.topologies = Enumerable.Range(0, subMeshCount).Select(x => (Sylves.MeshTopology)mesh.GetTopology(x)).ToArray();

            this.vertices = mesh.vertices;
            this.uv = mesh.uv;
            this.normals = mesh.normals;
            this.tangents = mesh.tangents;
        }
#endif

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
                    for(var i=0;i<face.Count;i++)
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
    }
}
