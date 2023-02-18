using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylves
{
    public static class MeshDataOperations
    {
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

            randomDouble = randomDouble ?? new Random().NextDouble;
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
        /// Merges all vertices that are within a given distance of each othher
        /// </summary>
        public static MeshData Weld(this MeshData md, float tol = 1e-7f)
        {
            // TODO: More efficient implementation with spatial hash
            // TODO: Average welded points?

            int weldCount = 0;
            var map = new int?[md.vertices.Length];
            var invMap = new int[md.vertices.Length];
            for (var i = 0; i < md.vertices.Length; ++i)
            {
                // Already welded
                if (map[i] != null)
                    continue;
                map[i] = weldCount;
                invMap[weldCount] = i;
                for (var j = i + 1; j < md.vertices.Length; ++j)
                {
                    if (Vector3.Distance(md.vertices[i], md.vertices[j]) < tol)
                    {
                        map[j] = weldCount;
                        invMap[weldCount] = j;
                    }
                }
                weldCount++;
            }
            var result = new MeshData();
            result.topologies = md.topologies;
            result.indices = md.indices.Select(ix => ix.Select(i => (i >= 0 ? map[i] : ~map[~i]).Value).ToArray()).ToArray();
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
    }
}
