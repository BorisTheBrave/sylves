using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylves
{
    // Experimental
    internal class PeriodicPlanarMeshGrid
    {
        public static void Create(MeshData meshData, Vector2 strideX, Vector2 strideY)
        {
            if (meshData.subMeshCount != 1)
                throw new Exception();

            var meshMin = meshData.vertices.Aggregate(Vector3.Min);
            var meshMax = meshData.vertices.Aggregate(Vector3.Max);
            var meshMin2 = new Vector2(meshMin.x, meshMin.y);
            var meshMax2 = new Vector2(meshMax.x, meshMax.y);
            var meshSize = meshMax2 - meshMin2;

            var aabbChunks = new AabbChunks(strideX, strideY, meshMin2, meshSize);

            var dataDrivenData = MeshGridBuilder.Build(meshData, out var edgeStore);

            var originalEdges = edgeStore.UnmatchedEdges.ToList();

            foreach(var chunk in aabbChunks.GetChunkIntersects(meshMin2, meshMax2))
            {
                // Skip this chunk as it's already in edgeStore
                if (chunk == Vector2Int.zero)
                    continue;

                var chunkOffset2 = strideX * chunk.x + strideY * chunk.y;
                var chunkOffset = new Vector3(chunkOffset2.x, chunkOffset2.y, 0);

                foreach (var edgeTuple in originalEdges)
                {
                    var (v1, v2, face, submesh, edge) = edgeTuple;
                    v1 += chunkOffset;
                    v2 += chunkOffset;
                    //edgeStore.MatchEdge()
                }
            }
        }
    }
}
