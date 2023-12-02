using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Sylves
{
    /// <summary>
    /// Specialization of MeshGrid specifically for planar meshes.
    /// </summary>
    internal class PlanarMeshGrid : MeshGrid
    {
        public PlanarMeshGrid(MeshData meshData, MeshGridOptions meshGridOptions = null) : base(meshData, meshGridOptions)
        {
            if(!IsPlanar)
            {
                throw new Exception("MeshData is not planar");
            }
        }

        protected override bool IsPointInCell(Vector3 position, Cell cell)
        {
            // Currently does fan detection
            // Doesn't work for convex faces
            var cellData = (MeshCellData)CellData[cell];
            var face = cellData.Face;
            var v0 = meshData.vertices[face[0]];
            var prev = meshData.vertices[face[1]];
            for (var i = 2; i < face.Count; i++)
            {
                var v = meshData.vertices[face[i]];
                if (GeometryUtils.IsPointInTrianglePlanar(position, v0, prev, v))
                    return true;
                prev = v;
            }
            return false;
        }
    }
}
