using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sylves
{
    public class MeshEmitter
    {
        private readonly MeshData originalData;
        private List<List<int>> indices;
        private List<Sylves.MeshTopology> topologies;
        public List<Vector3> vertices;
        private List<Vector2> uv;
        private List<Vector3> normals;
        private List<Vector4> tangents;

        public MeshEmitter(MeshData originalData)
        {
            this.originalData = originalData;
            indices = new List<List<int>>();
            topologies = new List<MeshTopology>();
            vertices = new List<Vector3>();
            uv = originalData.uv == null ? null : new List<Vector2>();
            normals = originalData.normals == null ? null : new List<Vector3>();
            tangents = originalData.tangents == null ? null : new List<Vector4>();
        }

        public void AddSubmesh(List<int> indices, MeshTopology meshTopology)
        {
            this.indices.Add(indices);
            topologies.Add(meshTopology);
        }

        public void StartSubmesh(MeshTopology meshTopology)
        {
            indices.Add(new List<int>());
            topologies.Add(meshTopology);
        }

        public void EndSubMesh()
        {

        }

        public void CopyAllVertices()
        {
            if (vertices.Count != 0)
                throw new Exception();
            vertices = originalData.vertices.ToList();
            uv = originalData.uv?.ToList();
            normals = originalData.normals?.ToList();
            tangents = originalData.tangents?.ToList();
        }

        public int AddVertex(Vector3 v, Vector2? uv = null, Vector3? normal = null, Vector4? tangent = null)
        {
            vertices.Add(v);
            this.uv?.Add(uv.Value);
            normals?.Add(normal.Value);
            tangents?.Add(tangent.Value);
            return vertices.Count - 1;
        }

        public int Average(int i1, int i2)
        {
            var v = (vertices[i1] + vertices[i2]) / 2;
            var uv = this.uv == null ? null : (Vector2?)(this.uv[i1] + this.uv[i2]) / 2;
            var normal = normals == null ? null : (Vector3?)(normals[i1] + normals[i2]).normalized;
            var tangent = tangents == null ? null : (Vector4?)(tangents[i1] + tangents[i2]) / 2;
            return AddVertex(v, uv, normal, tangent);
        }

        public int Average(IEnumerable<int> indices)
        {
            var n = 0;
            var v = Vector3.zero;
            var uv = Vector2.zero;
            var normal = Vector3.zero;
            var tangent = Vector4.zero;
            foreach (var i in indices)
            {
                n += 1;
                v += vertices[i];
                if (this.uv != null)
                    uv += this.uv[i];
                if (normals != null)
                    normal += normals[i];
                if (tangents != null)
                    tangent += tangents[i];
            }
            return AddVertex(v / n, uv / n, normal.normalized, tangent / n);
        }

        public int Average(IEnumerable<int> indices, MeshData meshData)
        {
            var n = 0;
            var v = Vector3.zero;
            var uv = Vector2.zero;
            var normal = Vector3.zero;
            var tangent = Vector4.zero;
            foreach (var i in indices)
            {
                n += 1;
                v += meshData.vertices[i];
                if (this.uv != null)
                    uv += meshData.uv[i];
                if (normals != null)
                    normal += meshData.normals[i];
                if (tangents != null)
                    tangent += meshData.tangents[i];
            }
            return AddVertex(v / n, uv / n, normal.normalized, tangent / n);
        }

        public void AddFace(int i1, int i2, int i3)
        {
            if (topologies.Count == 0)
                throw new Exception("Must first add a submesh");

            if (topologies[topologies.Count - 1] == MeshTopology.Triangles)
            {
                indices[indices.Count - 1].Add(i1);
                indices[indices.Count - 1].Add(i2);
                indices[indices.Count - 1].Add(i3);
            }
            else if (topologies[topologies.Count - 1] == MeshTopology.NGon)
            {
                indices[indices.Count - 1].Add(i1);
                indices[indices.Count - 1].Add(i2);
                indices[indices.Count - 1].Add(~i3);
            }
            else
            {
                throw new Exception($"Cannot add a triangle to topology {topologies[topologies.Count - 1]}");
            }
        }
        public void AddFace(int i1, int i2, int i3, int i4)
        {
            if (topologies.Count == 0)
                throw new Exception("Must first add a submesh");

            if (topologies[topologies.Count - 1] == MeshTopology.Quads)
            {
                indices[indices.Count - 1].Add(i1);
                indices[indices.Count - 1].Add(i2);
                indices[indices.Count - 1].Add(i3);
                indices[indices.Count - 1].Add(i4);
            }
            else if (topologies[topologies.Count - 1] == MeshTopology.NGon)
            {
                indices[indices.Count - 1].Add(i1);
                indices[indices.Count - 1].Add(i2);
                indices[indices.Count - 1].Add(i3);
                indices[indices.Count - 1].Add(~i4);
            }
            else
            {
                throw new Exception($"Cannot add a triangle to topology {topologies[topologies.Count - 1]}");
            }
        }

        public MeshData ToMeshData()
        {
            return new MeshData
            {
                indices = indices.Select(x => x.ToArray()).ToArray(),
                vertices = vertices.ToArray(),
                topologies = topologies.ToArray(),
                uv = uv?.ToArray(),
                normals = normals?.ToArray(),
                tangents = tangents?.ToArray(),
            };
        }
    }
}
