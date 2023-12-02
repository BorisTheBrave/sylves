using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Sylves
{
    /// <summary>
    /// Utility class represented a bundle of half edges extracted from a mesh.
    /// Only unpaired half-edges are stored - any paired off edges are immediately recorded in the moves array.
    /// </summary>
    internal class EdgeStore
    {
        private readonly float tolerance;
        private readonly Vector3Int basePoint;

        // the face, submesh and edge ids, stored by start/end points of the edge.
        private Dictionary<(Vector3Int, Vector3Int), (Vector3, Vector3, Cell, CellDir)> unmatchedEdges;
        private Dictionary<Vector3Int, int> vertexCount;


        public EdgeStore(float tolerance = MeshDataOperations.DefaultTolerance, Vector3Int basePoint = default)
        {
            this.tolerance = tolerance;
            this.basePoint = basePoint;
            unmatchedEdges = new Dictionary<(Vector3Int, Vector3Int), (Vector3, Vector3, Cell, CellDir)>();
            vertexCount = new Dictionary<Vector3Int, int>();
        }

        private EdgeStore(Dictionary<(Vector3Int, Vector3Int), (Vector3, Vector3, Cell, CellDir)> unmatchedEdges,
            Dictionary<Vector3Int, int> vertexCount, float tolerance)
        {
            this.unmatchedEdges = unmatchedEdges;
            this.vertexCount = vertexCount;
            this.tolerance = tolerance;
        }

        public void MapCells(Func<Cell, Cell> f)
        {
            unmatchedEdges = unmatchedEdges.ToDictionary(kv => kv.Key, kv => (kv.Value.Item1, kv.Value.Item2, f(kv.Value.Item3), kv.Value.Item4));
        }

        public IEnumerable<(Vector3 v1, Vector3 v2, Cell cell, CellDir dir)> UnmatchedEdges
        {
            get
            {
                return unmatchedEdges.Values;
            }
        }

        private static readonly Vector3Int[] Offsets = {
            new Vector3Int(0, 0, 0),
            new Vector3Int(0, 0, 1),
            new Vector3Int(0, 1, 0),
            new Vector3Int(0, 1, 1),
            new Vector3Int(1, 0, 0),
            new Vector3Int(1, 0, 1),
            new Vector3Int(1, 1, 0),
            new Vector3Int(1, 1, 1),
        };

        // Attempts to pair the new edge with the unmapped edges.
        // On success, adds it to moves and returns true.
        public bool MatchEdge(Vector3 v1, Vector3 v2, Cell cell, CellDir dir, IDictionary<(Cell, CellDir), (Cell, CellDir, Connection)> moves, bool clearEdge = true)
        {
            var v1i = Vector3Int.FloorToInt((v1 - basePoint) / tolerance);
            var v2i = Vector3Int.FloorToInt((v2 - basePoint) / tolerance);
            foreach (var o1 in Offsets)
            {
                var w1 = v1i + o1;
                // Early exit so we don't need try every value of o2
                if (!vertexCount.TryGetValue(w1, out var c) || c <= 0)
                    continue;

                foreach (var o2 in Offsets)
                {
                    var w2 = v2i + o2;
                    if (unmatchedEdges.TryGetValue((w2, w1), out var match))
                    {
                        // Edges match, add moves in both directions
                        var (_, _, cell2, dir2) = match;
                        moves.Add((cell, dir), (cell2, dir2, new Connection()));
                        moves.Add((cell2, dir2), (cell, dir, new Connection()));
                        if (clearEdge)
                        {
                            unmatchedEdges.Remove((w2, w1));
                            vertexCount[w2]--;
                            vertexCount[w1]--;
                        }
                        return true;
                    }
                    else if (unmatchedEdges.TryGetValue((w1, w2), out match))
                    {
                        // Same as above, but with a mirrored connection
                        var (_, _, cell2, dir2) = match;
                        moves.Add((cell, dir), (cell2, dir2, new Connection { Mirror = true }));
                        moves.Add((cell2, dir2), (cell, dir, new Connection { Mirror = true }));
                        if (clearEdge)
                        {
                            unmatchedEdges.Remove((w1, w2));
                            vertexCount[w1]--;
                            vertexCount[w2]--;
                        }
                        return true;
                    }
                }
            }
            return false;
        }


        // MatchEdge, and if it fails, adds the edge to unmatchedEdges
        public void AddEdge(Vector3 v1, Vector3 v2, Cell cell, CellDir dir, IDictionary<(Cell, CellDir), (Cell, CellDir, Connection)> moves)
        {
            if (!MatchEdge(v1, v2, cell, dir, moves))
            {
                // We use an offset when *storing* the vertex, to avoid boundary issues
                var v1i = Vector3Int.FloorToInt((v1 - basePoint) / tolerance + 0.5f * Vector3.one);
                var v2i = Vector3Int.FloorToInt((v2 - basePoint) / tolerance + 0.5f * Vector3.one);
                unmatchedEdges.Add((v1i, v2i), (v1, v2, cell, dir));
                vertexCount[v1i] = 1 + (vertexCount.TryGetValue(v1i, out var c) ? c : 0);
                vertexCount[v2i] = 1 + (vertexCount.TryGetValue(v2i, out c) ? c : 0);
            }
        }

        public EdgeStore Clone()
        {
            return new EdgeStore(unmatchedEdges.ToDictionary(x => x.Key, x => x.Value),
                vertexCount.ToDictionary(x => x.Key, x => x.Value),
                tolerance);
        }
    }
}
