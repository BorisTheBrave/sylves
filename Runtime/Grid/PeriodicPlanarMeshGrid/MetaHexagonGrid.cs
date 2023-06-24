using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Sylves
{
    // A hex grid which each hex subdivded into 12 right angle trianges.
    public class MetaHexagonGrid : PeriodicPlanarMeshGrid
    {
        public MetaHexagonGrid():base(MetaHexagonGridMeshData(), new Vector2(-0.4330127f, 0.75f), new Vector2(-0.8660254f, 0))
        {

        }
        private static MeshData MetaHexagonGridMeshData()
        {
            var hexGrid = new HexGrid(1);
            var meshData = new MeshData();
            hexGrid.GetPolygon(new Cell(), out var vertices, out var transform);
            meshData.vertices = vertices.Select(transform.MultiplyPoint3x4).ToArray();
            meshData.indices = new[]{new []
            {
                0, 1, 2, 3, 4, ~5,
            } };
            meshData.topologies = new[] { MeshTopology.NGon };
            return ConwayOperators.Meta(meshData);
        }

    }
}
