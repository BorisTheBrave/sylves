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
        public MeshTopology[] topologies;
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
            this.topologies = Enumerable.Range(0, subMeshCount).Select(mesh.GetTopology).ToArray();

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

        public MeshTopology GetTopology(int submesh)
        {
            return topologies[submesh];
        }
    }
}
