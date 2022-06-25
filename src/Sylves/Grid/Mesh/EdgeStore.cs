using System.Collections.Generic;
using System.Linq;
#if UNITY
using UnityEngine;
#endif


namespace Sylves
{
    /// <summary>
    /// Utility class represented a bundle of half edges extracted from a mesh.
    /// Only unpaired half-edges are stored - any paired off edges are immediately recorded in the moves array.
    /// </summary>
    internal class EdgeStore
    {
        private const float tolerance = 1e-6f;

        // the face, submesh and edge ids, stored by start/end points of the edge.
        private Dictionary<(Vector3Int, Vector3Int), (Vector3, Vector3, Cell, CellDir)> unmatchedEdges;

        public EdgeStore()
        {
            unmatchedEdges = new Dictionary<(Vector3Int, Vector3Int), (Vector3, Vector3, Cell, CellDir)>();
        }

        private EdgeStore(Dictionary<(Vector3Int, Vector3Int), (Vector3, Vector3, Cell, CellDir)> unmatchedEdges)
        {
            this.unmatchedEdges = unmatchedEdges;
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
        public bool MatchEdge(Vector3 v1, Vector3 v2, Cell cell, CellDir dir, IDictionary<(Cell, CellDir), (Cell, CellDir, Connection)> moves)
        {
            var v1i = Vector3Int.FloorToInt(v1 / tolerance);
            var v2i = Vector3Int.FloorToInt(v2 / tolerance);
            foreach (var o1 in Offsets) {
                foreach (var o2 in Offsets) {
                    var w1 = v1i + o1;
                    var w2 = v2i + o2;
                    if (unmatchedEdges.TryGetValue((w2, w1), out var match))
                    {
                        // Edges match, add moves in both directions
                        var (_, _, cell2, dir2) = match;
                        moves.Add((cell, dir), (cell2, dir2, new Connection()));
                        moves.Add((cell2, dir2), (cell, dir, new Connection()));
                        unmatchedEdges.Remove((w2, w1));
                        return true;
                    }
                    else if (unmatchedEdges.TryGetValue((w1, w2), out match))
                    {
                        // Same as above, but with a mirrored connection
                        var (_, _, cell2, dir2) = match;
                        moves.Add((cell, dir), (cell2, dir2, new Connection { Mirror = true }));
                        moves.Add((cell2, dir2), (cell, dir, new Connection { Mirror = true }));
                        unmatchedEdges.Remove((w1, w2));
                        return true;
                    }
                }
            }
            return false;
        }


        // MatchEdge, and if it fails, adds the edge to unmatchedEdges
        public void AddEdge(Vector3 v1, Vector3 v2, Cell cell, CellDir dir, IDictionary<(Cell, CellDir), (Cell, CellDir, Connection)> moves)
        {
            if(!MatchEdge(v1, v2, cell, dir, moves))
            {
                var v1i = Vector3Int.FloorToInt(v1 / tolerance);
                var v2i = Vector3Int.FloorToInt(v2 / tolerance);
                unmatchedEdges.Add((v1i, v2i), (v1, v2, cell, dir));
            }
        }

        public EdgeStore Clone()
        {
            return new EdgeStore(unmatchedEdges.ToDictionary(x => x.Key, x => x.Value));
        }
    }
}
