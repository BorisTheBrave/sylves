using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Sylves
{
    /// <summary>
    /// Periodic 2d grid of triangles.
    /// https://en.wikipedia.org/wiki/Tetrakis_square_tiling
    /// This is an specialization of <see cref="PeriodicPlanarMeshGrid"/>.
    /// </summary>
    public class TetrakisSquareGrid : PeriodicPlanarMeshGrid
    {
        public TetrakisSquareGrid():base(TetrakisSquareGridMeshData(), new Vector2(1, 0), new Vector2(0, 1))
        {

        }
        private static MeshData TetrakisSquareGridMeshData()
        {
            var meshData = new MeshData();
            meshData.vertices = new Vector3[]
            {
                new Vector3(0, 0, 0),
                new Vector3(1, 0, 0),
                new Vector3(1, 1, 0),
                new Vector3(0, 1, 0),
            };
            meshData.indices = new[]{new []
            {
                0, 1, 2, 3,
            } };
            meshData.topologies = new[] { MeshTopology.Quads };
            return ConwayOperators.Kis(meshData);
        }

    }
}
