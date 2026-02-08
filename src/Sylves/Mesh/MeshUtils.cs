using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY
using UnityEngine;
#endif

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

        internal static bool IsPointInPolygonPlanar(Vector3 p, Vector3[] vs)
        {
            if (vs == null || vs.Length < 3)
                return false;
            var px = p.x;
            var py = p.y;
            var n = vs.Length;
            const float eps = 1e-6f;

            int crossings = 0;
            for (var i = 0; i < n; i++)
            {
                var a = vs[i];
                var b = vs[(i + 1) % n];
                if (IsPointOnSegmentPlanar(px, py, a.x, a.y, b.x, b.y, eps))
                    return true;
                if ((a.y < py && b.y > py) || (a.y > py && b.y < py))
                {
                    var t = (py - a.y) / (b.y - a.y);
                    var x = a.x + t * (b.x - a.x);
                    if (x > px)
                        crossings++;
                }
            }
            return (crossings % 2) != 0;
        }

        private static bool IsPointOnSegmentPlanar(float px, float py, float ax, float ay, float bx, float by, float eps = 1e-6f)
        {
            var dx = bx - ax;
            var dy = by - ay;
            var lenSq = dx * dx + dy * dy;
            if (lenSq <= eps * eps)
                return Math.Abs(px - ax) <= eps && Math.Abs(py - ay) <= eps;
            var cross = (px - ax) * dy - (py - ay) * dx;
            if (Math.Abs(cross) > eps * (Math.Abs(dx) + Math.Abs(dy) + 1))
                return false;
            var dot = (px - ax) * dx + (py - ay) * dy;
            return dot >= -eps * (lenSq + 1) && dot <= lenSq + eps * (lenSq + 1);
        }

        internal static bool IsPointInPolygon(Vector3 p, Vector3[] vs, float planarThickness=1e-35f)
        {
            // Currently does fan detection
            // Doesn't work for convex faces
            var v0 = vs[0];
            var prev = vs[1];
            for (var i = 2; i < vs.Length; i++)
            {
                var v = vs[i];
                if (GeometryUtils.IsPointInTriangle(p, v0, prev, v, planarThickness))
                    return true;
                prev = v;
            }
            return false;
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

        // Uses Unity's winding order
        // NB: Doesn't normalize magnitude
        internal static Vector3 GetNormalDirection(Vector3 v0, Vector3 v1, Vector3 v2)
        {
            return Vector3.Cross(v2 - v1, v0 - v1);
        }

        /// <summary>
        /// References a slice of indices for a face.
        /// </summary>
        public struct Face : IReadOnlyList<int>
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

            public IEnumerable<Vector3> GetFaceVertices(Vector3[] vertices)
            {
                foreach(var i in this)
                {
                    yield return vertices[i];
                }
            }

            public IEnumerable<(Vector3, Vector3)> GetFaceVertexPairs(Vector3[] vertices)
            {
                var c = Count;
                var prev = vertices[this[0]];
                var first = prev;
                for (var i = 1; i < c; i++)
                {
                    var curr = vertices[this[i]];
                    yield return (prev, curr);
                    prev = curr;
                }
                yield return (prev, first);
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

        /// <summary>
        /// Returns the indices of the faces of meshData.
        /// </summary>
        public static IEnumerable<Face> GetFaces(MeshData meshData, bool invertWinding = false)
        {
            return Enumerable.Range(0, meshData.subMeshCount).SelectMany(i => GetFaces(meshData, i, invertWinding));
        }

        public static MeshData ToMesh(IGrid grid)
        {
            if(!grid.Is2d)
            {
                throw new Exception("Can only make a mesh from a 2d grid");
            }
            var verticies = new List<Vector3>();
            var indices = new List<int>();
            foreach(var cell in grid.GetCells())
            {
                var l = verticies.Count;
                grid.GetPolygon(cell, out var v, out var t);
                verticies.AddRange(v.Select(t.MultiplyPoint3x4));
                for(;l < verticies.Count;l++)
                {
                    indices.Add(l);
                }
                indices[indices.Count - 1] = ~l;
            }
            return new MeshData
            {
                indices = new[] { indices.ToArray() },
                vertices = verticies.ToArray(),
                topologies = new[] { MeshTopology.NGon },
            };
        }
    }
}
