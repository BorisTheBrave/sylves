using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Sylves
{
    /// <summary>
    /// Periodic 2d grid of diamond shapes.
    /// https://en.wikipedia.org/wiki/Rhombille_tiling
    /// This is an specialization of <see cref="PeriodicPlanarMeshGrid"/>.
    /// </summary>
    public class RhombilleGrid : PeriodicPlanarMeshGrid
    {

        public RhombilleGrid():base(RhombilleMeshData(), new Vector2(0, 1), new Vector2(0.75f, 0.5f))
        {

        }
        private static MeshData RhombilleMeshData()
        {
            var meshData = new MeshData();
            meshData.vertices = new Vector3[]
            {
                new Vector3(0.5f, 0, 0),
                new Vector3(0.25f, 0.5f, 0),
                new Vector3(-0.25f, 0.5f, 0),
                new Vector3(-0.5f, 0, 0),
                new Vector3(-0.25f, -0.5f, 0),
                new Vector3(0.25f, -0.5f, 0),
                new Vector3(0, 0 , 0),
            };
            meshData.indices = new[]{new []
            {
                0, 1, 2, 6,
                2, 3, 4, 6,
                4, 5, 0, 6,
            } };
            meshData.topologies = new[] { MeshTopology.Quads };
            return meshData;
        }

    }
}
