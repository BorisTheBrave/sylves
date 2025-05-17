using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY
using UnityEngine;
#endif

namespace Sylves
{

    /// <summary>
    /// Sudivides the surface of a unit sphere into Voronoi cells,
    /// one per input point.
    /// </summary>
    public class VoronoiSphereGrid : MeshGrid
    {
        public VoronoiSphereGrid(IList<Vector3> points, VoronoiGridOptions voronoiGridOptions = null)
            :base(CreateMeshData(points, voronoiGridOptions))
        {
        }


        public static MeshData CreateMeshData(IList<Vector3> points, VoronoiGridOptions voronoiGridOptions = null, Func<int, bool> mask = null)
        {

            voronoiGridOptions = voronoiGridOptions ?? new VoronoiGridOptions();
            if (voronoiGridOptions.ClipMin != null || voronoiGridOptions.ClipMax != null)
            {
                throw new ArgumentException($"ClipMin/ClipMax cannot be used with {nameof(VoronoiSphereGrid)}");
            }
            var v = new SphericalVoronator(points);

            // TODO: Relax in 3d space
            for(var i=0; i< voronoiGridOptions.LloydRelaxationIterations; i++)
            {
                var relaxedPoints = v.GetRelaxedPoints();
                v = new SphericalVoronator(relaxedPoints);
            }
            //(voronator, points2d) = VoronoiGrid.LloydRelax(voronator, points2d, voronoiGridOptions);



            var indices = new List<int>();
            var vertices = new List<Vector3>();
            var polygon = new List<Vector3>();
            for (var i = 0; i < points.Count; i++)
            {
                if (mask != null && mask(i) == false)
                    continue;
                if (!v.GetPolygon(i, polygon))
                    continue;
                // Hmm, should we just fail here?
                if (polygon.Count == 0)
                    continue;
                for (var j = 0; j < polygon.Count; j++)
                {
                    indices.Add(vertices.Count);
                    vertices.Add(polygon[j]);
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
}

