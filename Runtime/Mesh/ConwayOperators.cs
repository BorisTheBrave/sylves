using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
                    var last = meshEmitter.Average(face[face.Count - 1], face[0]);
                    var prev = last;
                    for (var i = 0; i < face.Count; i++)
                    {
                        var i1 = face[i];
                        var i2 = face[(i + 1) % face.Count];
                        var b = i == face.Count - 1 ? last : meshEmitter.Average(i1, i2);
                        meshEmitter.AddFace(centroid, prev, i1, b);
                        prev = b;
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
            var dmb = new DualMeshBuilder(meshData);
            return dmb.DualMeshData;
        }
    }
}
