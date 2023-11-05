using System;
using System.Collections.Generic;
using System.Text;

namespace Sylves
{
#if !PURE_SYLVES
    public class VoronoiGridOptions
    {
        public Vector2? ClipMin { get; set; }
        public Vector2? ClipMax { get; set; }
    }

    public class VoronoiGrid : MeshGrid
    {
        public VoronoiGrid(IList<Vector2> points, VoronoiGridOptions voronoiGridOptions = null)
            :base(CreateMeshData(points, voronoiGridOptions))
        {
        }

        public static MeshData CreateMeshData(IList<Vector2> points, VoronoiGridOptions voronoiGridOptions = null, Func<int, bool> mask = null)
        {
            voronoiGridOptions = voronoiGridOptions ?? new VoronoiGridOptions();
            if (voronoiGridOptions.ClipMin != null ^ voronoiGridOptions.ClipMax != null)
            {
                throw new ArgumentException("ClipMin/ClipMax should be specified together");
            }
            var voronator = voronoiGridOptions.ClipMin == null ? new Voronator(points) : new Voronator(points, voronoiGridOptions.ClipMin.Value, voronoiGridOptions.ClipMax.Value);

            var indices = new List<int>();
            var vertices = new List<Vector3>();
            for (var i = 0; i < points.Count; i++)
            {
                if (mask != null && mask(i) == false)
                    continue;
                var polygon = voronator.GetClippedPolygon(i);
                for (var j = 0; j < polygon.Count; j++)
                {
                    indices.Add(vertices.Count);
                    vertices.Add(new Vector3(polygon[j].x, polygon[j].y, 0));
                }
                indices[indices.Count - 1] = ~indices[indices.Count - 1];
            }

            return new MeshData
            {
                vertices = vertices.ToArray(),
                indices = new[] { indices.ToArray() },
                topologies = new[] { MeshTopology.NGon },
            };
        }
    }
#endif
}
