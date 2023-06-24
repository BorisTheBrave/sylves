using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Sylves
{
    /// <summary>
    /// Periodic 2d grid of squares and triangles..
    /// https://en.wikipedia.org/wiki/Snub_square_tiling
    /// This is an specialization of <see cref="PeriodicPlanarMeshGrid"/>.
    /// </summary>
    public class SquareSnubGrid : PeriodicPlanarMeshGrid
    {
        private static float o = Mathf.Sqrt(3) / 2 + 0.5f;

        public SquareSnubGrid():base(SquareSnubMeshData(), new Vector2(o, o), new Vector2(-o, o))
        {

        }
        private static MeshData SquareSnubMeshData()
        {
            var meshData = new MeshData();
            // TODO: Remove duplicates?
            meshData.vertices = new Vector3[]
            {
                // Tri 1
                new Vector3(0, -1.943376f, 0),
                new Vector3(-0.5f, -1.077350f, 0),
                new Vector3(0.5f, -1.077350f, 0),
                // Square 1
                new Vector3(0.866025f, 0.288675f, 0.000000f),
                new Vector3(1.366025f, -0.577350f, 0.000000f),
                new Vector3(0.500000f, -1.077350f, 0.000000f),
                new Vector3(0.000000f, -0.211325f, 0.000000f),
                // Tri 2
                new Vector3(-0.500000f, -1.077350f, 0.000000f),
                new Vector3(0.000000f, -0.211325f, 0.000000f),
                new Vector3(0.500000f, -1.077350f, 0.000000f),
                // tri 3
                new Vector3(0.000000f, 0.788675f, 0.000000f),
                new Vector3(0.866025f, 0.288675f, 0.000000f),
                new Vector3(-0.000000f, -0.211325f, 0.000000f),
                // square 2
                new Vector3(0.000000f, -0.211325f, 0.000000f),
                new Vector3(-0.500000f, -1.077350f, 0.000000f),
                new Vector3(-1.366025f, -0.577350f, 0.000000f),
                new Vector3(-0.866025f, 0.288675f, 0.000000f),
                // tri 4
                new Vector3(0.000000f, 0.788675f, 0.000000f),
                new Vector3(-0.000000f, -0.211325f, 0.000000f),
                new Vector3(-0.866025f, 0.288675f, 0.000000f),
            };
            meshData.indices = new[]{new []
            {
                0, 1, ~2,
                3, 4, 5, ~6,
                7, 8, ~9,
                10, 11, ~12,
                13,14,15, ~16,
                17, 18, ~19,
            } };
            meshData.topologies = new[] { MeshTopology.NGon };
            return meshData;
        }

    }
}
