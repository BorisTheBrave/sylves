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
        private bool Flip(int face, int edge, out int destFace, out int backEdge)
        {
            if (moves.TryGetValue((new Cell(face, 0), (CellDir)edge), out var t))
            {
                var (destCell, inverseDir, connection) = t;
                if (connection != new Connection())
                    throw new Exception("Cannot handle non-trivial connection");
                destFace = destCell.x;
                backEdge = (int)edge;
                return true;
            }
            else
            {
                destFace = default;
                backEdge = default;
                return false;
            }
        }

        // Moves one edge around a face in primal mesh
        private int NextEdge(int face, int edge)
        {
            return (edge + 1) % GetFace(face).Length;
        }
        private int PrevEdge(int face, int edge)
        {
            var l = GetFace(face).Length;
            return (edge + l - 1) % l;
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
            var forwardCentroids = new List<int>();
            var backwardCentroids = new List<int>();
            var outputIndices= new List<int>();
            // Loop over every half edge
            var dualFaceCount = 0;
            var visited = new HashSet<(int face, int edge)>();
            foreach(var (i, face) in GetFaces())
            {
                for(var edge = 0;edge<face.Count;edge++)
                {
                    // Check if we've already explored this arc/loop
                    if (visited.Contains((i, edge)))
                    {
                        continue;
                    }
                    // Determine the vertex we are walking around
                    bool isFar = isFarVertex[face[edge]];
                    // Walk forword
                    forwardCentroids.Clear();
                    backwardCentroids.Clear();
                    bool isLoop;
                    (int, int) forwardHalfEdge = default;
                    (int, int) backHalfEdge = default;
                    {
                        var currentFace = i;
                        var currentEdge = edge;
                        var dualVertCount = 0;
                        while (true)
                        {
                            visited.Add((currentFace, currentEdge));
                            forwardCentroids.Add(faceCentroids[currentFace]);
                            //mapping.Add((currentCell.x, currentCell.y, (int)currentDir, dualFaceCount, dualVertCount));

                            if(!Flip(currentFace, currentEdge, out var nextFace, out var nextEdge))
                            {
                                isLoop = false;
                                forwardHalfEdge = (currentFace, currentEdge);
                                break;
                            }
                            currentFace = nextFace;
                            currentEdge = NextEdge(nextFace, nextFace);
                            if (currentFace == i && currentEdge == edge)
                            {
                                isLoop = true;
                                break;
                            }
                            dualVertCount++;
                        }
                    }
                    // Walk back if necessary
                    if (!isLoop)
                    {
                        var currentFace = i;
                        var currentEdge = edge;
                        while (true)
                        {
                            //var currentFace = ((MeshCellData)cellData[cell]).Face;
                            currentEdge = PrevEdge(currentFace, currentEdge);
                            if (!Flip(currentFace, currentEdge, out var nextFace, out var nextEdge))
                            {
                                backHalfEdge = (currentFace, currentEdge);
                                break;
                            }
                            currentFace = nextFace;
                            currentEdge = nextEdge;
                            visited.Add((currentFace, currentEdge));
                            backwardCentroids.Add(faceCentroids[currentFace]);
                            //TODO: update mapping
                        }
                    }
                    // Create face from arc/loop
                    if (!isFar)
                    {
                        // Create point "at infinity" for the back end of the arc
                        if (!isLoop)
                        {
                            // Find bisector of edge
                            var backFace = GetFace(backHalfEdge.Item1);
                            var i1 = backFace[backHalfEdge.Item2];
                            var i2 = backFace[(backHalfEdge.Item2 + 1) % face.Length];
                            var v = (meshData.vertices[i1] + meshData.vertices[i2]) / 2;
                            // Extend to "infinity"
                            var backCentroid = meshEmitter.vertices[backwardCentroids.Count > 0 ? backwardCentroids[backwardCentroids.Count - 1] : forwardCentroids[0]];
                            v = (v - backCentroid).normalized * FAR;
                            outputIndices.Add(meshEmitter.AddVertex(
                                v,
                                new Vector2(),
                                new Vector3(),
                                new Vector4()
                                ));
                        }
                        // Copy points from the arc/loop
                        for (var j = backwardCentroids.Count - 1; j >= 0; j--)
                        {
                            outputIndices.Add(backwardCentroids[j]);
                        }
                        for (var j = 0; j < forwardCentroids.Count; j++)
                        {
                            outputIndices.Add(forwardCentroids[j]);
                        }
                        // Create point "at infinity" for the forward end of the arc
                        if (!isLoop)
                        {
                            var forwardFace = GetFace(forwardHalfEdge.Item1);
                            // Find bisector of edge
                            var i1 = forwardFace[(int)forwardHalfEdge.Item2];
                            var i2 = forwardFace[(int)(forwardHalfEdge.Item2 + 1) % forwardFace.Length];
                            var v = (meshData.vertices[i1] + meshData.vertices[i2]) / 2;
                            // Extend to "infinity"
                            var forwardCentroid = meshEmitter.vertices[forwardCentroids[forwardCentroids.Count - 1]];
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
            meshEmitter.AddSubmesh(outputIndices, MeshTopology.NGon);
        }

    }
}
