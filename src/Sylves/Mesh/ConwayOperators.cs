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
        public static MeshData Kis(MeshData meshData)
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
        public static MeshData Ortho(MeshData meshData)
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

        public static MeshData Dual(MeshData meshData)
        {
            var ddd = MeshGridBuilder.Build(meshData, new MeshGridOptions());
            var moves = ddd.Moves;
            var cellData = ddd.Cells;
            var meshEmitter = new MeshEmitter(meshData);
            var otherData = new Dictionary<Cell, (int centroid, int other)>();
            // Collect centroids
            foreach(var kv in cellData)
            {
                var centroid = meshEmitter.Average(((MeshCellData)kv.Value).Face, meshData);
                otherData[kv.Key] = (centroid, 0);
            }
            meshEmitter.StartSubmesh(MeshTopology.NGon);
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
                    // Walk forword
                    forwardCentroids.Clear();
                    backwardCentroids.Clear();
                    bool isLoop;
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
                    if (backwardCentroids.Count + forwardCentroids.Count > 2)
                    {
                        for (var i = backwardCentroids.Count - 1; i >= 0; i--)
                        {
                            meshEmitter.AddFaceIndex(backwardCentroids[i]);
                        }
                        for (var i = 0; i < forwardCentroids.Count - 1; i++)
                        {
                            meshEmitter.AddFaceIndex(forwardCentroids[i]);
                        }
                        meshEmitter.AddFaceIndex(~forwardCentroids[forwardCentroids.Count - 1]);
                    }
                }
            }
            meshEmitter.EndSubMesh();
            return meshEmitter.ToMeshData();
        }
    }
}
