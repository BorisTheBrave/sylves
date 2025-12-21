using System.Collections.Generic;
using UnityEngine;
using static Sylves.Delaunator;

namespace Sylves
{
    internal class SphericalVoronator
    {
        private readonly IList<Vector3> points;
        
        private readonly Delaunator d;

        private readonly int[] inedges;
        private readonly List<Vector3> circumcenters;
        private readonly Vector3[] hullCircumcenters;
        private readonly List<Vector3> hullPolygon;

        public SphericalVoronator(IList<Vector3> points, bool useCentroids = false)
        {
            IList<Vector2> points2d = new Vector2[points.Count - 1];
            var proj = points[points.Count - 1].normalized;
            GetPlaneBasis(proj, out Vector3 u, out Vector3 v);
            for (int i = 0; i < points.Count - 1; i++)
            {
                points2d[i] = StereographicProject(points[i].normalized, proj, u, v);
            }

            d = new Delaunator(points2d);


            circumcenters = new List<Vector3>();
            var triangleCount = d.Triangles.Length / 3;
            for (var triangleId = 0; triangleId < triangleCount; triangleId++)
            {
                var t = d.PointIndiciesAroundTriangle(triangleId);
                if(useCentroids)
                    circumcenters.Add(GetCentroid(points[t.Item1], points[t.Item2], points[t.Item3]));
                else
                    circumcenters.Add(GetCircumcenter(points[t.Item1], points[t.Item2], points[t.Item3]).normalized);
            }

            // Find the voronoi cells, storing the "starting" half edge.
            inedges = new int[points.Count];
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

            hullCircumcenters = new Vector3[d.Points.Count];
            hullPolygon = new List<Vector3>();
            var h = d.Hull[d.Hull.Length - 1];
            for (var i = 0; i < d.Hull.Length; i++)
            {
                var next = d.Hull[i];
                var circumcenter = GetCircumcenter(points[h], points[next], points[points.Count - 1]).normalized;
                hullCircumcenters[h] = circumcenter;
                hullPolygon.Add(circumcenter);
                h = next;
            }

            this.points = points;
        }

        public bool GetPolygon(int i, List<Vector3> p)
        {
            // if(collinearNormal != null)
            // {
            // 	// There's nothing sensible to return here except for the ends
            // 	// of the hull, it's not worth worrying abou
            // 	ray1 = ray2 = default;
            // 	return false;
            // }

            if(i == d.Points.Count)
            {
                // This is the pole point, return the hull polygon
                p.Clear();
                p.AddRange(hullPolygon);
                return true;
            }

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

            if (e == -1)
            {
                // This is an open polygon,
                p.Add(hullCircumcenters[d.Triangles[prevE]]);
                p.Add(hullCircumcenters[d.Triangles[e0]]);
            }
            return true;
        }

        /// <summary>
        /// Returns the centroid of each voronoi cell.
        /// This is suitable for use with Lloyd relaxation.
        /// Unbounded cells return their original point.
        /// </summary>
        public List<Vector3> GetRelaxedPoints()
        {
            var relaxedPoints = new List<Vector3>();
            var polygon = new List<Vector3>();
            for (var i = 0; i < points.Count; i++)
            {
                var relaxedPoint = GetPolygon(i, polygon) ? GetCentroid(polygon) : points[i];
                relaxedPoints.Add(relaxedPoint);
            }
            return relaxedPoints;
        }


        public static Vector3 GetCentroid(List<Vector3> vertices)
        {
            if (vertices == null || vertices.Count < 3)
                throw new System.ArgumentException("At least three vertices are required");

            Vector3 n = Vector3.zero;
            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 current = vertices[i];
                Vector3 next = vertices[(i + 1) % vertices.Count];
                n.x += (current.y - next.y) * (current.z + next.z);
                n.y += (current.z - next.z) * (current.x + next.x);
                n.z += (current.x - next.x) * (current.y + next.y);
            }
            n.Normalize();

            Vector3 u, v;
            if (Mathf.Abs(n.z) >= Mathf.Abs(n.x) && Mathf.Abs(n.z) >= Mathf.Abs(n.y))
            {
                u = Vector3.right;
                v = Vector3.up;
            }
            else if (Mathf.Abs(n.y) >= Mathf.Abs(n.x))
            {
                u = Vector3.right;
                v = Vector3.forward;
            }
            else
            {
                u = Vector3.up;
                v = Vector3.forward;
            }

            // Project to 2d
            List<Vector2> projected = new List<Vector2>();
            foreach (var vert in vertices)
                projected.Add(new Vector2(Vector3.Dot(vert, u), Vector3.Dot(vert, v)));

            var centroid2d = Voronator.GetCentroid(projected);

            // Reconstruct
            var centroid = centroid2d.x * u + centroid2d.y * v;
            float avgOffset = 0f;
            foreach (var vert in vertices)
                avgOffset += Vector3.Dot(vert, n);
            avgOffset /= vertices.Count;

            centroid += n * (avgOffset - Vector3.Dot(centroid, n));
            return centroid;
        }

        private static Vector3 AveragePosition(List<Vector3> vertices)
        {
            Vector3 sum = Vector3.zero;
            foreach (var v in vertices)
                sum += v;
            return sum / vertices.Count;
        }

        private static Vector3 GetCircumcenter(Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 ac = c - a;
            Vector3 ab = b - a;
            Vector3 abXac = Vector3.Cross(ab, ac);

            // this is the vector from a TO the circumsphere center
            Vector3 toCircumsphereCenter = (Vector3.Cross(abXac, ab) * ac.sqrMagnitude + Vector3.Cross(ac, abXac) * ab.sqrMagnitude) / (2.0f * abXac.sqrMagnitude);
            float circumsphereRadius = toCircumsphereCenter.magnitude;

            // The 3 space coords of the circumsphere center then:
            Vector3 ccs = a + toCircumsphereCenter; // now this is the actual 3space location
            return ccs;
        }

        private static Vector3 GetCentroid(Vector3 a, Vector3 b, Vector3 c) => (a + b + c).normalized;

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
            u = Vector3.Cross(q, temp).normalized;
            v = Vector3.Cross(q, u).normalized;
        }
        #endregion
    }
}

