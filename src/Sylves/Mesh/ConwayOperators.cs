#if UNITY
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#endif

using System;
using System.Collections.Generic;

namespace Sylves
{

    // See https://en.wikipedia.org/wiki/Conway_polyhedron_notation
    public static class ConwayOperators
    {
        private const float FAR = 1e10f;

        /// <summary>
        /// Subdivides each edge, adds a vertex in the center of each face,
        /// then replaces each n-gon face with a fan of 2n triangles from the center.
        /// </summary>
        public static MeshData Meta(MeshData meshData)
        {
            var meshEmitter = new MeshEmitter(meshData);
            meshEmitter.CopyAllVertices();
            for (var submesh = 0; submesh < meshData.subMeshCount; submesh++)
            {
                meshEmitter.StartSubmesh(MeshTopology.Triangles);
                foreach (var face in MeshUtils.GetFaces(meshData, submesh))
                {
                    var centroid = meshEmitter.Average(face);
                    for (var i = 0; i < face.Count; i++)
                    {
                        var i1 = face[i];
                        var i2 = face[(i + 1) % face.Count];
                        var b = meshEmitter.Average(i1, i2);
                        meshEmitter.AddFace(centroid, i1, b);
                        meshEmitter.AddFace(centroid, b, i2);
                    }
                }
                meshEmitter.EndSubMesh();
            }
            return meshEmitter.ToMeshData();
        }

        /// <summary>
        /// Subdivides each edge, adds a vertex in the center of each face,
        /// then replaces each n-gon face with a fan of n quads from the center.
        /// </summary>
        public static MeshData Ortho(MeshData meshData)
        {
            var meshEmitter = new MeshEmitter(meshData);
            meshEmitter.CopyAllVertices();
            for (var submesh = 0; submesh < meshData.subMeshCount; submesh++)
            {
                meshEmitter.StartSubmesh(MeshTopology.Quads);
                foreach (var face in MeshUtils.GetFaces(meshData, submesh))
                {
                    var centroid = meshEmitter.Average(face);
                    for (var i = 0; i < face.Count; i++)
                    {
                        var i1 = face[i];
                        var i2 = face[(i + 1) % face.Count];
                        var b = meshEmitter.Average(i1, i2);
                        meshEmitter.AddFace(centroid, i1, b, i2);
                    }
                }
                meshEmitter.EndSubMesh();
            }
            return meshEmitter.ToMeshData();
        }

        /// <summary>
        /// Adds a vertex in the center of each face,
        /// then replaces each n-gon face with a fan of n triangles from the center.
        /// </summary>
        public static MeshData Kis(MeshData meshData)
        {
            var meshEmitter = new MeshEmitter(meshData);
            meshEmitter.CopyAllVertices();
            for (var submesh = 0; submesh < meshData.subMeshCount; submesh++)
            {
                meshEmitter.StartSubmesh(MeshTopology.Triangles);
                foreach (var face in MeshUtils.GetFaces(meshData, submesh))
                {
                    var centroid = meshEmitter.Average(face);
                    for (var i = 0; i < face.Count; i++)
                    {
                        var i1 = face[i];
                        var i2 = face[(i + 1) % face.Count];
                        meshEmitter.AddFace(centroid, i1, i2);
                    }
                }
                meshEmitter.EndSubMesh();
            }
            return meshEmitter.ToMeshData();
        }

        internal static MeshData Dual(MeshData meshData)
        {
            var ddd = MeshGridBuilder.Build(meshData, new MeshGridOptions());
            var moves = ddd.Moves;
            var cellData = ddd.Cells;
            var meshEmitter = new MeshEmitter(meshData);
            var otherData = new Dictionary<Cell, (int centroid, int other)>();
            var outputIndices = new List<int>();
            // Collect centroids
            foreach(var kv in cellData)
            {
                var centroid = meshEmitter.Average(((MeshCellData)kv.Value).Face, meshData);
                otherData[kv.Key] = (centroid, 0);
            }
            var forwardCentroids = new List<int>();
            var backwardCentroids = new List<int>();
            var visited = new HashSet<(Cell, CellDir)>();
            foreach (var cell in cellData.Keys)
            {
                var cellType = cellData[cell].CellType;
                foreach(var dir in cellType.GetCellDirs())
                {
                    // Check if we've already explored this arc/loop
                    if (visited.Contains((cell, dir)))
                    {
                        continue;
                    }
                    // Determine the vertex we are walking around
                    bool isFar;
                    {
                        var face = ((MeshCellData)cellData[cell]).Face;
                        var i1 = face[(int)dir];
                        var v = meshData.vertices[i1];
                        // Far vertices are treated as if points at infinity - they don't generate a face.
                        isFar = v.magnitude >= FAR;
                    }
                    // Walk forword
                    forwardCentroids.Clear();
                    backwardCentroids.Clear();
                    bool isLoop;
                    (Cell, CellDir) forwardHalfEdge = default;
                    (Cell, CellDir) backHalfEdge = default;
                    {
                        var currentCell = cell;
                        var currentDir = dir;
                        while (true)
                        {
                            visited.Add((currentCell, currentDir));
                            forwardCentroids.Add(otherData[currentCell].centroid);
                            if (!moves.TryGetValue((currentCell, currentDir), out var t))
                            {
                                isLoop = false;
                                forwardHalfEdge = (currentCell, currentDir);
                                break;
                            }
                            var (nextCell, iDir, connection) = t;
                            if (connection.Mirror)
                                throw new NotImplementedException();
                            currentCell = nextCell;
                            var currentFace = ((MeshCellData)cellData[cell]).Face;
                            currentDir = (CellDir)(((int)iDir + 1) % currentFace.Length);

                            if (currentCell == cell && currentDir == dir)
                            {
                                isLoop = true;
                                break;
                            }
                        }
                    }
                    // Walk back if necessary
                    if(!isLoop)
                    {
                        var currentCell = cell;
                        var currentDir = dir;
                        while(true)
                        {
                            var currentFace = ((MeshCellData)cellData[cell]).Face;
                            currentDir = (CellDir)(((int)currentDir - 1 + currentFace.Length) % currentFace.Length);
                            if (!moves.TryGetValue((currentCell, currentDir), out var t))
                            {
                                backHalfEdge = (currentCell, currentDir);
                                break;
                            }
                            var (nextCell, iDir, connection) = t;
                            if (connection.Mirror)
                                throw new NotImplementedException();
                            currentCell = nextCell;
                            currentDir = iDir;
                            visited.Add((currentCell, currentDir));
                            backwardCentroids.Add(otherData[currentCell].centroid);
                        }
                    }
                    // Create face from arc/loop
                    if (!isFar)
                    {
                        // Create point "at infinity" for the back end of the arc
                        if(!isLoop)
                        {
                            var face = ((MeshCellData)cellData[backHalfEdge.Item1]).Face;
                            // Find bisector of edge
                            var i1 = face[(int)backHalfEdge.Item2];
                            var i2 = face[(int)(backHalfEdge.Item2 + 1) % face.Length];
                            var v = (meshData.vertices[i1] + meshData.vertices[i2]) / 2;
                            // Extend to "infinity"
                            var backCentroid = meshEmitter.vertices[backwardCentroids.Count > 0 ? backwardCentroids[backwardCentroids.Count -1] : forwardCentroids[0]];
                            v = (v - backCentroid).normalized * FAR;
                            outputIndices.Add(meshEmitter.AddVertex(
                                v,
                                new Vector2(),
                                new Vector3(),
                                new Vector4()
                                ));
                        }
                        // Copy points from the arc/loop
                        for (var i = backwardCentroids.Count - 1; i >= 0; i--)
                        {
                            outputIndices.Add(backwardCentroids[i]);
                        }
                        for (var i = 0; i < forwardCentroids.Count; i++)
                        {
                            outputIndices.Add(forwardCentroids[i]);
                        }
                        // Create point "at infinity" for the forward end of the arc
                        if (!isLoop)
                        {
                            var face = ((MeshCellData)cellData[forwardHalfEdge.Item1]).Face;
                            // Find bisector of edge
                            var i1 = face[(int)forwardHalfEdge.Item2];
                            var i2 = face[(int)(forwardHalfEdge.Item2 + 1) % face.Length];
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
                    }
                }
            }
            meshEmitter.AddSubmesh(outputIndices, MeshTopology.NGon);
            return meshEmitter.ToMeshData();
        }
    }
}
