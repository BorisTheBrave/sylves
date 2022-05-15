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
        // the face, submesh and edge ids, stored by start/end points of the edge.
        private Dictionary<(Vector3, Vector3), (Cell, CellDir)> unmatchedEdges;

        public EdgeStore()
        {
            unmatchedEdges = new Dictionary<(Vector3, Vector3), (Cell, CellDir)>();
        }

        private EdgeStore(Dictionary<(Vector3, Vector3), (Cell, CellDir)> unmatchedEdges)
        {
            this.unmatchedEdges = unmatchedEdges;
        }

        public IEnumerable<(Vector3 v1, Vector3 v2, Cell cell, CellDir dir)> UnmatchedEdges
        {
            get
            {
                return unmatchedEdges.Select(x => (x.Key.Item1, x.Key.Item2, x.Value.Item1, x.Value.Item2));
            }
        }

        // Attempts to pair the new edge with the unmapped edges.
        // On success, adds it to moves and returns true.
        public bool MatchEdge(Vector3 v1, Vector3 v2, Cell cell, CellDir dir, IDictionary<(Cell, CellDir), (Cell, CellDir, Connection)> moves)
        {
            if (unmatchedEdges.TryGetValue((v2, v1), out var match))
            {
                // Edges match, add moves in both directions
                var (cell2, dir2) = match;
                moves.Add((cell, dir), (cell2, dir2, new Connection()));
                moves.Add((cell2, dir2), (cell, dir, new Connection()));
                unmatchedEdges.Remove((v2, v1));
                return true;
            }
            else if (unmatchedEdges.TryGetValue((v1, v2), out match))
            {
                // Same as above, but with a mirrored connection
                var (cell2, dir2) = match;
                moves.Add((cell, dir), (cell2, dir2, new Connection { Mirror = true }));
                moves.Add((cell2, dir2), (cell, dir, new Connection { Mirror = true }));
                unmatchedEdges.Remove((v1, v2));
                return true;
            }
            return false;
        }


        // MatchEdge, and if it fails, adds the edge to unmatchedEdges
        public void AddEdge(Vector3 v1, Vector3 v2, Cell cell, CellDir dir, IDictionary<(Cell, CellDir), (Cell, CellDir, Connection)> moves)
        {
            if(!MatchEdge(v1, v2, cell, dir, moves))
            {
                unmatchedEdges.Add((v1, v2), (cell, dir));
            }
        }

        public EdgeStore Clone()
        {
            return new EdgeStore(unmatchedEdges.ToDictionary(x => x.Key, x => x.Value));
        }
    }
}
