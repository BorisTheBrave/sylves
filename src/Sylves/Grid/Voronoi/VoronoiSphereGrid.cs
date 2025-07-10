using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY
using UnityEngine;
#endif
using static Sylves.Delaunator;

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
            IList<Vector2> points2d = new Vector2[points.Count-1];
            var proj = points[points.Count-1].normalized;
            GetPlaneBasis(proj, out Vector3 u, out Vector3 v);
            for (int i = 0; i < points.Count - 1; i++)
            {
                points2d[i] = StereographicProject(points[i], proj, u, v);
            }

            voronoiGridOptions = voronoiGridOptions ?? new VoronoiGridOptions();
            if (voronoiGridOptions.ClipMin != null ^ voronoiGridOptions.ClipMax != null)
            {
                throw new ArgumentException("ClipMin/ClipMax should be specified together");
            }
            var voronator = voronoiGridOptions.ClipMin == null ? new Voronator(points2d) : new Voronator(points2d, voronoiGridOptions.ClipMin.Value, voronoiGridOptions.ClipMax.Value);

            // TODO: Relax in 3d space
            //(voronator, points2d) = VoronoiGrid.LloydRelax(voronator, points2d, voronoiGridOptions);

            // Get circumcenters and inedges, like Voronator
            var d = voronator.Delaunator;
            var circumcenters = new List<Vector3>();
            var triangleCount = d.Triangles.Length / 3;
            for (var triangleId = 0; triangleId < triangleCount; triangleId++)
            {
                var t = d.PointIndiciesAroundTriangle(triangleId);
                circumcenters.Add(GetCircumcenter(points[t.Item1], points[t.Item2], points[t.Item3]).normalized);
            }

            // Find the voronoi cells, storing the "starting" half edge.
            var inedges = new int[points.Count];
            for (var e = 0; e < points.Count; ++e)
            {
                inedges[e] = -1;
            }
            for (var e = 0; e < d.Halfedges.Length; ++e)
            {
                var p = d.Triangles[NextHalfedge(e)];
                if (d.Halfedges[e] == -1 || inedges[p] == -1)
                {
                    inedges[p] = e;
                }
            }

            var hullCircumcenters = new Vector3[d.Points.Count];
            var hullPolygon = new List<Vector3>();
            var h = d.Hull[d.Hull.Length - 1];
            for (var i = 0; i < d.Hull.Length; i++)
            {
                var next = d.Hull[i];
                var circumcenter = GetCircumcenter(points[h], points[next], points[points.Count-1]).normalized;
                hullCircumcenters[h] = circumcenter;
                hullPolygon.Add(circumcenter);
                h = next;
            }

            // 3d equivalent of voronator.GetPolygon
            bool GetPolygon(int i, List<Vector3> p)
            {

                // if(collinearNormal != null)
                // {
                // 	// There's nothing sensible to return here except for the ends
                // 	// of the hull, it's not worth worrying abou
                // 	ray1 = ray2 = default;
                // 	return false;
                // }

                var e0 = inedges[i];
                if (e0 == -1)
                {
                    return false;
                }
                p.Clear();
                var e = e0;
                int prevE;
                do
                {
                    p.Add(circumcenters[EdgeIndexToTriangleIndex(e)]);
                    prevE = NextHalfedge(e);
                    e = d.Halfedges[prevE];
                } while (e != e0 && e != -1);

                if(e == -1)
                {
                    // This is an open polygon,
                    p.Add(hullCircumcenters[d.Triangles[prevE]]);
                    p.Add(hullCircumcenters[d.Triangles[e0]]);
                }
                return true;
            }

            var indices = new List<int>();
            var vertices = new List<Vector3>();
            var polygon = new List<Vector3>();
            for (var i = 0; i < points.Count - 1; i++)
            {
                if (mask != null && mask(i) == false)
                    continue;
                if (!GetPolygon(i, polygon))
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

            // Add the final polygon, for the pole point
            for (var i = 0; i < hullPolygon.Count; i++)
            {
                indices.Add(vertices.Count);
                vertices.Add(hullPolygon[i]);
            }
            indices[indices.Count - 1] = ~indices[indices.Count - 1];


            return new MeshData
            {
                vertices = vertices.ToArray(),
                indices = new[] { indices.ToArray() },
                topologies = new[] { MeshTopology.NGon },
            };
        }

        private static Vector3 GetCircumcenter(Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 ac = c - a ;
            Vector3 ab = b - a ;
            Vector3 abXac = Vector3.Cross(ab, ac) ;

            // this is the vector from a TO the circumsphere center
            Vector3 toCircumsphereCenter = (Vector3.Cross(abXac, ab) * ac.sqrMagnitude + Vector3.Cross(ac, abXac) * ab.sqrMagnitude) / (2.0f * abXac.sqrMagnitude) ;
            float circumsphereRadius = toCircumsphereCenter.magnitude ;

            // The 3 space coords of the circumsphere center then:
            Vector3 ccs = a  +  toCircumsphereCenter ; // now this is the actual 3space location
            return ccs;
        }

        #region Stereographic projection
        
        private static Vector2 StereographicProject(Vector3 p, Vector3 q, Vector3 u, Vector3 v)
        {
            float denom = 1.0f - Vector3.Dot(q, p);
            if (Mathf.Abs(denom) < 1e-6f)
                throw new System.ArgumentException("p is too close to q; projection goes to infinity.");

            Vector3 proj3D = q + (p - q) / denom;

            float x = Vector3.Dot(proj3D, u);
            float y = Vector3.Dot(proj3D, v);
            return new Vector2(x, y);
        }

        private static void GetPlaneBasis(Vector3 q, out Vector3 u, out Vector3 v)
        {
            // Pick an arbitrary vector not parallel to q
            Vector3 temp = (Mathf.Abs(Vector3.Dot(q, Vector3.up)) < 0.99f) ? Vector3.up : Vector3.right;
            u = Vector3.Normalize(Vector3.Cross(q, temp));
            v = Vector3.Normalize(Vector3.Cross(q, u));
        }
        #endregion
    }
}

