using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sylves
{
    public static class MeshDataOperations
    {
        /// <summary>
        /// Tolerance used for operations like Weld and EdgeStore.
        /// </summary>
        public const float DefaultTolerance = 1e-6f;

        private static IList<T> RandomShuffle<T>(IList<T> list, Func<double> randomDouble)
        {
            for (var i = 0; i < list.Count; i++)
            {
                var j = (int)(randomDouble() * (list.Count - i)) + i;
                // Swap i and j
                (list[i], list[j]) = (list[j], list[i]);
            }
            return list;
        }

        /// <summary>
        /// Randomly picks pairs of adjacent faces in the mesh, and merges them into one larger face.
        /// </summary>
        public static MeshData RandomPairing(this MeshData md, Func<double> randomDouble = null)
        {
            if(md.topologies.Length != 1)
            {
                throw new NotImplementedException("Method doesn't support submeshes");
            }
            if(!md.topologies.All(x => x == MeshTopology.Triangles))
            {
                // This would be quite easy to improve.
                throw new NotImplementedException("RandomPairing only supports triangular topology currently.");
            }

            randomDouble = randomDouble ?? new System.Random().NextDouble;
            var meshGrid = new MeshGrid(md);
            var cells = meshGrid.GetCells().ToList();
            var unpaired = new HashSet<Cell>(cells);
            var pairs = new List<(Cell, CellDir, Cell, CellDir)>();
            var pairDict = new Dictionary<Cell, (CellDir, Cell, CellDir)>();

            RandomShuffle(cells, randomDouble);
            foreach (var cell in cells)
            {
                if (!unpaired.Contains(cell))
                    continue;
                var dirs = RandomShuffle(meshGrid.GetCellDirs(cell).ToList(), randomDouble);
                foreach (var dir in dirs)
                {
                    if (!meshGrid.TryMove(cell, dir, out Cell dest, out CellDir inverseDir, out var _))
                        continue;
                    if (!unpaired.Contains(dest))
                        continue;
                    pairs.Add((cell, dir, dest, inverseDir));
                    pairDict.Add(cell, (dir, dest, inverseDir));
                    pairDict.Add(dest, (inverseDir, cell, dir));
                    unpaired.Remove(cell);
                    unpaired.Remove(dest);
                    break;
                }
            }

            return ApplyPairing(md, meshGrid, pairs, unpaired);
        }

        private static MeshData ApplyPairing(MeshData md, MeshGrid meshGrid, IEnumerable<(Cell, CellDir, Cell, CellDir)> pairs, HashSet<Cell> unpaired)
        {
            // New mesh data with pairs of triangles merged
            var indices = new List<int>();
            foreach (var (cell, dir, dest, inverseDir) in pairs)
            {
                // TODO: Support non-triangles
                var f1 = meshGrid.GetFaceIndices(cell);
                var f2 = meshGrid.GetFaceIndices(dest);
                int i1, i2, i3, i4;
                switch ((int)dir)
                {
                    case 0:
                        (i1, i2, i3) = (f1[1], f1[2], f1[0]);
                        break;
                    case 1:
                        (i1, i2, i3) = (f1[2], f1[0], f1[1]);
                        break;
                    case 2:
                        (i1, i2, i3) = (f1[0], f1[1], f1[2]);
                        break;
                    default:
                        throw new Exception();
                }
                switch ((int)inverseDir)
                {
                    case 0:
                        i4 = f2[2];
                        break;
                    case 1:
                        i4 = f2[0];
                        break;
                    case 2:
                        i4 = f2[1];
                        break;
                    default:
                        throw new Exception();
                }
                indices.Add(i1);
                indices.Add(i2);
                indices.Add(i3);
                indices.Add(~i4);
            }
            foreach (var cell in unpaired)
            {
                indices.AddRange(meshGrid.GetFaceIndices(cell));
                indices[indices.Count - 1] = ~indices[indices.Count - 1];
            }


            var result = md.Clone();
            result.indices = new[] { indices.ToArray() };
            result.topologies = new[] { MeshTopology.NGon };

            return result;
        }


        /// <summary>
        /// Randomly picks pairs of adjacent faces in the mesh, and merges them into one larger face.
        /// Always finds a maximal possible set of pairs via the "augmenting path" algorithm.
        /// </summary>
        public static MeshData MaxRandomPairing(this MeshData md, Func<double> randomDouble = null)
        {

            if (md.topologies.Length != 1)
            {
                throw new NotImplementedException("Method doesn't support submeshes");
            }
            if (!md.topologies.All(x => x == MeshTopology.Triangles))
            {
                // This would be quite easy to improve.
                throw new NotImplementedException("RandomPairing only supports triangular topology currently.");
            }

            randomDouble = randomDouble ?? new System.Random().NextDouble;
            var meshGrid = new MeshGrid(md);
            var cells = meshGrid.GetCells().ToList();
            var unpaired = new HashSet<Cell>(cells);
            var pairs = new Dictionary<Cell, (CellDir, Cell, CellDir)>();

            RandomShuffle(cells, randomDouble);
            foreach(var cell in cells)
            {
                if (!unpaired.Contains(cell))
                    continue;

                // Find random augmenting path
                var visited = new HashSet<Cell>();
                var prev = new Dictionary<Cell, (CellDir, Cell, CellDir)>();
                var queue = new Queue<Cell>();
                Cell? dest = null;
                queue.Enqueue(cell);
                while(queue.Count > 0)
                {
                    var c = queue.Dequeue();
                    var dirs = RandomShuffle(meshGrid.GetCellDirs(cell).ToList(), randomDouble);
                    foreach(var dir in dirs)
                    {
                        if(!meshGrid.TryMove(c, dir, out var next, out var invDir, out var _))
                            continue;

                        if(visited.Contains(next))
                            continue;

                        visited.Add(next);
                        prev[next] = (dir, c, invDir);

                        if(unpaired.Contains(next))
                        {
                            // End of augmenting path
                            dest = next;
                            break;
                        }
                        else
                        {
                            var nextPair = pairs[next].Item2;
                            queue.Enqueue(nextPair);
                        }
                    }
                    // Are we done
                    if (dest != null)
                        break;
                }

                if(dest == null)
                {
                    // No augmenting path, give up
                    // NB: This is only valid for connected graphs, perhaps we
                    // should not do it?
                    break;
                }
                else
                {
                    // Flip all pairs along the augmenting path
                    var curr = dest.Value;
                    while(true)
                    {
                        var (dir, p, invDir) = prev[curr];
                        if(unpaired.Contains(p))
                        {
                            // Walked back to start of path
                            // Set new pair
                            unpaired.Remove(p);
                            unpaired.Remove(curr);
                            pairs[p] = (dir, curr, invDir);
                            pairs[curr] = (invDir, p, dir);
                            // And we're done
                            break;
                        }
                        else
                        {
                            var (pDir, pp, pInvDir) = pairs[p];
                            // Unset old pair
                            pairs.Remove(p);
                            pairs.Remove(pp);
                            unpaired.Add(p);
                            unpaired.Add(pp);
                            // Set new pair
                            unpaired.Remove(p);
                            unpaired.Remove(curr);
                            pairs[p] = (dir, curr, invDir);
                            pairs[curr] = (invDir, p, dir);
                            // Move on
                            curr = pp;
                        }
                    }
                }
            }

            return ApplyPairing(md, meshGrid, pairs.Select(kv => (kv.Key, kv.Value.Item1, kv.Value.Item2, kv.Value.Item3)), unpaired);
        }

        private static readonly Vector3Int[] WeldOffsets = {
            new Vector3Int(0, 0, 0),
            new Vector3Int(0, 0, 1),
            new Vector3Int(0, 1, 0),
            new Vector3Int(0, 1, 1),
            new Vector3Int(1, 0, 0),
            new Vector3Int(1, 0, 1),
            new Vector3Int(1, 1, 0),
            new Vector3Int(1, 1, 1),
        };

        /// <summary>
        /// Merges all vertices that are within a given distance of each other
        /// </summary>
        public static MeshData Weld(this MeshData md, float tolerance = DefaultTolerance)
        {
            return Weld(md, out var _, tolerance);
        }

        /// <summary>
        /// Merges all vertices that are within a given distance of each other
        /// </summary>
        public static MeshData Weld(this MeshData md, out int[] indexMap, float tolerance = DefaultTolerance)
        {
            // TODO: Average welded points?

            // Pick a base point near the mesh, this avoids overflow.
            var basePoint = md.vertices.Length > 0 ? md.vertices[0] : default;

            var vertexLookup = new Dictionary<Vector3Int, int>();

            int weldCount = 0;
            var map = new int[md.vertices.Length];
            for (var i = 0; i < md.vertices.Length; ++i)
            {
                map[i] = -1;
            }

            var invMap = new int[md.vertices.Length];
            for (var i = 0; i < md.vertices.Length; ++i)
            {
                var vi = Vector3Int.FloorToInt((md.vertices[i] - basePoint) / tolerance);

                bool found = false;
                foreach (var offset in WeldOffsets)
                {
                    if (vertexLookup.TryGetValue(vi + offset, out var index))
                    {
                        // Weld to index
                        map[i] = index;
                        invMap[index] = i;
                        found = true;
                        break;
                    }
                }
                if (found)
                    continue;

                // We use an offset when *storing* the vertex, to avoid boundary issues
                var vi2 = Vector3Int.FloorToInt((md.vertices[i] - basePoint) / tolerance + 0.5f * Vector3.one);
                vertexLookup[vi2] = weldCount;
                map[i] = weldCount;
                invMap[weldCount] = i;
                weldCount++;
            }
            var result = new MeshData();
            result.topologies = md.topologies;
            result.indices = md.indices.Select(ix => ix.Select(i => (i >= 0 ? map[i] : ~map[~i])).ToArray()).ToArray();
            T[] MapArray<T>(T[] data)
            {
                if (data == null)
                    return null;
                var r = new T[weldCount];
                for (var i = 0; i < weldCount; ++i)
                {
                    r[i] = data[invMap[i]];
                }
                return r;
            }
            result.vertices = MapArray(md.vertices);
            result.uv = MapArray(md.uv);
            result.normals = MapArray(md.normals);
            result.tangents = MapArray(md.tangents);

            indexMap = map;
            return result;
        }

        // Performs Laplacian smoothing on the mesh with the given number of iteration
        // https://en.wikipedia.org/wiki/Laplacian_smoothing
        public static MeshData Relax(this MeshData md, int iterations = 3)
        {
            var adjacencies = Enumerable.Range(0, md.vertices.Length).Select(x => new List<int>()).ToArray();
            foreach (var face in MeshUtils.GetFaces(md))
            {
                var p = face.Count - 1;
                for (var i = 0; i < face.Count; i++)
                {
                    var i1 = face[i];
                    var i2 = face[p];
                    adjacencies[i1].Add(i2);
                    adjacencies[i2].Add(i1);
                    p = i;
                }
            }
            var vertices = md.vertices;
            for (var i = 0; i < iterations; i++)
            {
                var nextVertices = new Vector3[vertices.Length];
                for (var j = 0; j < vertices.Length; j++)
                {
                    foreach (var neighbour in adjacencies[j])
                    {
                        nextVertices[j] += vertices[neighbour];
                    }
                    nextVertices[j] /= adjacencies[j].Count;
                }
                vertices = nextVertices;
            }
            var result = md.Clone();
            result.vertices = vertices;
            return result;
        }

        // Smooths the vertices of each face to make it closer to a regular polygon.
        public static MeshData FaceRelax(this MeshData md, int iterations = 3)
        {
            var vertices = md.vertices;

            for (var it = 0; it < iterations; it++)
            {
                var nextVertices = new Vector3[vertices.Length];
                var nextVerticesN = new int[vertices.Length];
                foreach (var face in MeshUtils.GetFaces(md))
                {
                    var n = face.Count;
                    // TODO: Precompute this outside of iteration loop?
                    var centroid = new Vector3();
                    var normal = new Vector3();
                    for (var i = 0; i < face.Count; i++)
                    {
                        centroid += vertices[face[i]];
                        normal += Vector3.Cross(vertices[face[i]], vertices[face[(i + 1) % n]]);
                    }
                    centroid /= n;
                    normal.Normalize();
                    var p = new Vector3();
                    var r = Matrix4x4.Rotate(Quaternion.AngleAxis(360f / n, normal));
                    for (var i = 0; i < face.Count; i++)
                    {
                        p += vertices[face[i]] - centroid;
                        p = r.MultiplyVector(p);
                    }
                    p /= n;
                    for (var i = 0; i < face.Count; i++)
                    {
                        nextVertices[face[i]] += p + centroid;
                        nextVerticesN[face[i]] += 1;
                        p = r.MultiplyVector(p);
                    }
                }
                for (var i = 0; i < nextVertices.Length; i++)
                {
                    nextVertices[i] /= nextVerticesN[i];
                }
                vertices = nextVertices;
            }
            var result = md.Clone();
            result.vertices = vertices;
            return result;
        }

        public static MeshData Concat(IEnumerable<MeshData> mds, out List<int[]> indexMaps)
        {
            // TODO: Should this use MeshEmitter?
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uv = null;
            List<Vector3> normal = null;
            List<Vector4> tangents = null;
            var i = 0;
            void Fill<T>(ref List<T> dest, T[] src, string name)
            {
                if (src == null && dest == null)
                    return;
                if((src == null ^ dest == null) && i != 0)
                {
                    if (src == null)
                    {
                        throw new Exception($"Cannot concat mesh {i} as it has no {name} data but mesh 0 does.");
                    }
                    else
                    {
                        throw new Exception($"Cannot concat mesh {i} as it has {name} data but mesh 0 does not.");
                    }
                }
                if(dest == null && i == 0)
                {
                    dest = new List<T>();
                }
                dest.AddRange(src);
            }
            var indices = new List<int>();
            indexMaps = new List<int[]>();
            var topologies = new[] { MeshTopology.NGon };
            foreach (var md in mds)
            {
                if (md.subMeshCount != 1)
                {
                    throw new NotImplementedException("Concat doesn't support submeshes");
                }
                var indexMap = Enumerable.Range(vertices.Count, md.vertices.Length).ToArray();
                foreach(var face in MeshUtils.GetFaces(md, 0))
                {
                    foreach(var ii in face)
                    {
                        indices.Add(ii + vertices.Count);
                    }
                    indices[indices.Count - 1] = ~indices[indices.Count - 1];
                }
                indexMaps.Add(indexMap);

                Fill(ref vertices, md.vertices, "vertices");
                Fill(ref uv, md.uv, "uv");
                Fill(ref normal, md.normals, "normals");
                Fill(ref tangents, md.tangents, "tangents");
                i++;
            }
            return new MeshData
            {
                indices = new[] { indices.ToArray() },
                topologies = topologies,
                vertices = vertices.ToArray(),
                normals = normal?.ToArray(),
                uv = uv?.ToArray(),
                tangents = tangents?.ToArray(),
            };
        }

        public static MeshData FaceFilter(this MeshData md, Func<MeshUtils.Face, int, bool> filterFunc)
        {
            var indices = new int[md.subMeshCount][];
            for(var subMesh =0; subMesh < md.subMeshCount;subMesh++)
            {
                var ii = new List<int>();
                var faceIndex = 0;
                foreach(var face in MeshUtils.GetFaces(md, subMesh))
                {
                    if(filterFunc(face, faceIndex))
                    {
                        ii.AddRange(face);
                        if(md.topologies[subMesh] == MeshTopology.NGon)
                        {
                            ii[ii.Count - 1] = ~ii[ii.Count - 1];
                        }
                    }
                    faceIndex++;
                }
                indices[subMesh] = ii.ToArray();
            }
            var r = md.Clone();
            r.indices = indices;
            return r;
        }
    }
}
