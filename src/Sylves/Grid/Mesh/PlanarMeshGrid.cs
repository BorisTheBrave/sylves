using System;
using System.Collections.Generic;
using System.Text;
#if UNITY
using UnityEngine;
#endif

namespace Sylves
{
    /// <summary>
    /// Specialization of MeshGrid specifically for planar meshes.
    /// </summary>
    public class PlanarMeshGrid : MeshGrid
    {
        public PlanarMeshGrid(MeshData meshData, MeshGridOptions meshGridOptions = null) : base(meshData, meshGridOptions)
        {
            if(!IsPlanar)
            {
                throw new Exception("MeshData is not planar");
            }
        }

        // Internals constructor used for other meshgrid like grids.
        // You need to call BuildMeshDetails() after calling this.
        internal PlanarMeshGrid(MeshData meshData, MeshGridOptions meshGridOptions, DataDrivenData data, bool is2d) :
            base(meshData, meshGridOptions, data, true)
        {
        }

        protected override bool IsPointInCell(Vector3 position, Cell cell)
        {
            var cellData = (MeshCellData)CellData[cell];
            var face = cellData.Face;
            var vertices = meshData.vertices;
            var px = position.x;
            var py = position.y;
            var n = face.Count;

            // Ray casting (crossing number): cast ray in +X from (px, py); odd crossings = inside.
            // Use strict straddle (a.y < py && b.y > py) || (a.y > py && b.y < py) so a ray through a vertex is counted once.
            int crossings = 0;
            for (var i = 0; i < n; i++)
            {
                var a = vertices[face[i]];
                var b = vertices[face[(i + 1) % n]];

                // Point on segment (a,b)? Treat as inside.
                if (IsPointOnSegmentPlanar(px, py, a.x, a.y, b.x, b.y))
                    return true;

                if ((a.y < py && b.y > py) || (a.y > py && b.y < py))
                {
                    var t = (py - a.y) / (b.y - a.y);
                    var x = a.x + t * (b.x - a.x);
                    if (x > px)
                        crossings++;
                }
            }
            return (crossings % 2) != 0;
        }

        /// <summary>Point (px,py) on segment (ax,ay)-(bx,by) in XY, within tolerance.</summary>
        private static bool IsPointOnSegmentPlanar(float px, float py, float ax, float ay, float bx, float by, float eps = 1e-6f)
        {
            var dx = bx - ax;
            var dy = by - ay;
            var lenSq = dx * dx + dy * dy;
            if (lenSq <= eps * eps)
                return Math.Abs(px - ax) <= eps && Math.Abs(py - ay) <= eps;
            var cross = (px - ax) * dy - (py - ay) * dx;
            if (Math.Abs(cross) > eps * (Math.Abs(dx) + Math.Abs(dy) + 1))
                return false;
            var dot = (px - ax) * dx + (py - ay) * dy;
            return dot >= -eps * (lenSq + 1) && dot <= lenSq + eps * (lenSq + 1);
        }
        public override IEnumerable<Cell> GetCellsIntersectsApprox(Vector3 min, Vector3 max)
        {
            min.z = this.min.z;
            max.z = this.max.z;
            return base.GetCellsIntersectsApprox(min, max);
        }

    }
}
