using System;
using System.Collections.Generic;
using System.Text;
#if UNITY
using UnityEngine;
#endif

namespace Sylves
{
    internal class PlanarMeshGrid : MeshGrid
    {
        public PlanarMeshGrid(MeshData meshData, MeshGridOptions meshGridOptions = null) : base(meshData, meshGridOptions)
        {
            if(!IsPlanar)
            {
                throw new Exception("MeshData is not planar");
            }
        }


        /// <summary>
        /// Returns true if p is in the triangle po, p1,p2
        /// Ignores the z-axis
        /// </summary>
        private static bool IsPointInTrianglePlanar(Vector3 p, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            // s = cross(p0-p2, p-p2)
            // t = cross(p1-p0, p-p0)
            var s = (p0.x - p2.x) * (p.y - p2.y) - (p0.y - p2.y) * (p.x - p2.x);
            var t = (p1.x - p0.x) * (p.y - p0.y) - (p1.y - p0.y) * (p.x - p0.x);

            if ((s < 0) != (t < 0) && s != 0 && t != 0)
                return false;

            // d = cross(p2 - p1, p - p1)
            var d = (p2.x - p1.x) * (p.y - p1.y) - (p2.y - p1.y) * (p.x - p1.x);
            return d == 0 || (d < 0) == (s + t <= 0);
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
                if (IsPointInTrianglePlanar(position, v0, prev, v))
                    return true;
                prev = v;
            }
            return false;
        }
    }
}
