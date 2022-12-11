using System;
using System.Collections.Generic;
#if UNITY
using UnityEngine;
#endif

namespace Sylves
{
    public class DualMeshBuilder
    {
        private const float FAR = 1e10f;

        // Fundamental data about the primal mesh
        private readonly MeshData meshData;
        private IDictionary<(Cell, CellDir), (Cell, CellDir, Connection)> moves;
        private IDictionary<Cell, DataDrivenCellData> cellData;

        // Recorded output
        MeshEmitter meshEmitter;
        List<(int primeFace, int primeVert, int dualFace, int dualVert)> mapping;

        // Further info about the primal mesh
        private int[] faceCentroids;
        private bool[] isFarVertex;

        public DualMeshBuilder(MeshData meshData)
        {
            if (meshData.subMeshCount != 1)
                throw new ArgumentException("Can only build dual mesh for a mesh data with a single submesh");
            this.meshData = meshData;
            var ddd = MeshGridBuilder.Build(meshData, new MeshGridOptions());
            this.moves = ddd.Moves;
            this.cellData = ddd.Cells;

            meshEmitter = new MeshEmitter(meshData);
            mapping = new List<(int primeFace,int primeVert, int dualFace,int dualVert)>();

            faceCentroids = BuildFaceCentroids(meshData, cellData, meshEmitter);

            isFarVertex = BuildIsFarVertex(meshData);

            Build();
        }

        public MeshData DualMeshData => meshEmitter.ToMeshData();

        private static int[] BuildFaceCentroids(MeshData meshData, IDictionary<Cell, DataDrivenCellData> cellData, MeshEmitter meshEmitter)
        {
            var r = new int[cellData.Count];
            foreach (var kv in cellData)
            {
                var centroid = meshEmitter.Average(((MeshCellData)kv.Value).Face, meshData);
                r[kv.Key.x] = centroid;
            }
            return r;
        }

        private static bool[] BuildIsFarVertex(MeshData meshData)
        {
            var isFarVertex = new bool[meshData.vertices.Length];
            for (var i = 0; i < meshData.vertices.Length; i++)
            {
                isFarVertex[i] = meshData.vertices[i].magnitude >= FAR;
            }
            return isFarVertex;
        }

        // Move across an edge in primal mesh
        private (int face, int edge)? Flip((int face, int edge) halfEdge)
        {
            if (moves.TryGetValue((new Cell(halfEdge.face, 0), (CellDir)halfEdge.edge), out var t))
            {
                var (destCell, inverseDir, connection) = t;
                if (connection != new Connection())
                    throw new Exception("Cannot handle non-trivial connection");
                return (destCell.x, (int)inverseDir);
            }
            else
            {
                return null;
            }
        }

        // Moves one edge around a face in primal mesh
        private (int face, int edge) NextHalfEdge((int face, int edge) halfEdge)
        {
            var l = GetFace(halfEdge.face).Length;
            return (halfEdge.face, (halfEdge.edge + 1) % l);
        }
        private (int face, int edge) PrevHalfEdge((int face, int edge) halfEdge)
        {
            var l = GetFace(halfEdge.face).Length;
            return (halfEdge.face, (halfEdge.edge + l - 1) % l);
        }

        private IEnumerable<(int, MeshUtils.Face)> GetFaces()
        {
            foreach (var kv in cellData)
            {
                yield return (kv.Key.x, ((MeshCellData)kv.Value).Face);
            }
        }

        private MeshUtils.Face GetFace(int face)
        {
            return ((MeshCellData)cellData[new Cell(face, 0)]).Face;
        }

        private void Build()
        {
            // Initialize temporary arrays
            var dualFaceIndices = new List<int>(); // TODO: Eliminate this variable
            var outputIndices= new List<int>();

            var dualFaceCount = 0;
            var visited = new HashSet<(int face, int edge)>();

            // Do arcs first, then non-arcs
            foreach (var isArc in new[] { true, false })
            {
                // Loop over every half edge
                foreach (var (i, face) in GetFaces())
                {
                    for (var edge = 0; edge < face.Count; edge++)
                    {
                        var startHe = (face: i, edge: edge);
                        // Skip if not the start of arc, and we want it to be
                        if(isArc && Flip(startHe) != null)
                        {
                            continue;
                        }
                        // Skip if we've already explored this arc/loop
                        if (visited.Contains(startHe))
                        {
                            continue;
                        }
                        // Determine the vertex we are walking around
                        var vertex = face[edge];
                        bool isFar = isFarVertex[vertex];
                        // Walk forword
                        dualFaceIndices.Clear();
                        (int, int) endHe = default;
                        {
                            var currentHe = startHe;
                            var dualVertCount = 0;
                            while (true)
                            {
                                visited.Add(currentHe);
                                dualFaceIndices.Add(faceCentroids[currentHe.face]);
                                mapping.Add((currentHe.face, currentHe.edge, dualFaceCount, dualVertCount));
                                dualVertCount++;

                                currentHe = PrevHalfEdge(currentHe);
                                var nextHe = Flip(currentHe);
                                if (nextHe == null)
                                {
                                    if (!isArc) throw new Exception();
                                    endHe = currentHe;
                                    break;
                                }
                                currentHe = nextHe.Value;
                                if (currentHe == startHe)
                                {
                                    if (isArc) throw new Exception();
                                    break;
                                }
                            }
                        }
                        // Create face from arc/loop
                        if (!isFar)
                        {
                            // Create point "at infinity" for the back end of the arc
                            if (isArc)
                            {
                                // Find bisector of edge
                                var backFace = GetFace(startHe.Item1);
                                var i1 = backFace[startHe.Item2];
                                var i2 = backFace[(startHe.Item2 + 1) % face.Length];
                                var v = (meshData.vertices[i1] + meshData.vertices[i2]) / 2;
                                // Extend to "infinity"
                                var backCentroid = meshEmitter.vertices[dualFaceIndices[0]];
                                v = (v - backCentroid).normalized * FAR;
                                outputIndices.Add(meshEmitter.AddVertex(
                                    v,
                                    new Vector2(),
                                    new Vector3(),
                                    new Vector4()
                                    ));
                            }
                            // Copy points from the arc/loop
                            for (var j = 0; j < dualFaceIndices.Count; j++)
                            {
                                outputIndices.Add(dualFaceIndices[j]);
                            }
                            // Create point "at infinity" for the forward end of the arc
                            if (isArc)
                            {
                                var forwardFace = GetFace(endHe.Item1);
                                // Find bisector of edge
                                var i1 = forwardFace[(int)endHe.Item2];
                                var i2 = forwardFace[(int)(endHe.Item2 + 1) % forwardFace.Length];
                                var v = (meshData.vertices[i1] + meshData.vertices[i2]) / 2;
                                // Extend to "infinity"
                                var forwardCentroid = meshEmitter.vertices[dualFaceIndices[dualFaceIndices.Count - 1]];
                                v = (v - forwardCentroid).normalized * FAR;
                                outputIndices.Add(meshEmitter.AddVertex(
                                    v,
                                    new Vector2(),
                                    new Vector3(),
                                    new Vector4()
                                    ));
                            }
                            outputIndices[outputIndices.Count - 1] = ~outputIndices[outputIndices.Count - 1];
                            dualFaceCount++;
                        }
                    }
                }
            }
            meshEmitter.AddSubmesh(outputIndices, MeshTopology.NGon);
        }

    }
}
