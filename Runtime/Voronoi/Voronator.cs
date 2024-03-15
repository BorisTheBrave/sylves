using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Sylves.Delaunator;


namespace Sylves
{
    /// <summary>
    /// Computes a voronoi diagram in a 2d plane,
    /// optionally clipped to a rectangle.
    /// </summary>
    public class Voronator
    {
        private IList<Vector2> points;

        // Delauney triangulation dual to the voronoi cells
        private Delaunator d;

        // Inverse lookup of d.Hull
        private int[] hullIndex;

        // triangle circumcenters, plus some extra vertices on the end
        private List<Vector2> circumcenters;

        /// <summary>
        /// Map from point index to a half edge. 
        /// The starting half edge is the one with d.Halfedges[e] == -1.
        /// Or an arbitrary choice if there isn't one.
        /// </summary>
        private int[] inedges;

        // Vectors holds two vectors for each hull point, indicating the directions of the rays outwards
        Vector2[] vectors;

        // Clipping rectangle used for GetClippedPolygon.
        Vector2 clipMin, clipMax;

        // In collinear situations, gives the normal to the line the cells run in.
        // In this case, we don't use a delaunay triangulation at all.
        // Instead we use Delaunator.Hull which contains an ordered set of vertices.
        Vector2? collinearNormal;

        public Voronator(IList<Vector2> points, Vector2 clipMin, Vector2 clipMax)
        {
            Init(points);
            this.clipMin = clipMin;
            this.clipMax = clipMax;
        }

        public Voronator(IList<Vector2> points)
        {
            Init(points);

            // Set bounds.
            clipMin = clipMax = points[0];
            for (var i = 1; i < points.Count; i++)
            {
                clipMin = Vector2.Min(clipMin, points[i]);
                clipMax = Vector2.Max(clipMax, points[i]);
            }
            for (var i = 0; i < circumcenters.Count; i++)
            {
                clipMin = Vector2.Min(clipMin, circumcenters[i]);
                clipMax = Vector2.Max(clipMax, circumcenters[i]);
            }
            // Inflate the clipping area slightly
            // This ensures that unbounded cells will still share a border
            // after clipping.
            var expand = 1e-6f * (clipMax - clipMin);
            clipMin -= expand;
            clipMax += expand;
        }

        private void Init(IList<Vector2> points)
        {
            this.points = points;

            // Actually do delauney triangulation
            d = new Delaunator(points.ToArray());

            if(d.Hull.Length > 0 && d.Triangles.Length == 0)
            {
                // The points are collinear
                var first = points[d.Hull[0]];
                var last = points[d.Hull[(d.Hull.Length - 1)]];
                var l = last - first;
                collinearNormal = new Vector2(l.y, -l.x).normalized;

                /*
                // d3-delaunay uses this hack to get a roughly accurate triangulation despite triangulation
                if (d.Hull.Length >= 3)
                {
                    var r = (first - last).magnitude * 1e-6f;
                    var jittered = points
                        .Select(v => new Vector2(v.x + (float)Math.Sin(v.x + v.y) * r, v.y + (float)Math.Cos(v.x - v.y) * r))
                        .ToArray();
                    d = new Delaunator(jittered);
                }
                */
            }


            // Inverse lookup of hull
            hullIndex = new int[points.Count];
            for (var i = 0; i < hullIndex.Length; ++i)
            {
                hullIndex[i] = -1;
            }
            for (var i = 0; i < d.Hull.Length; ++i)
            {
                hullIndex[d.Hull[i]] = i;
            }


            // Load the vertices of the mesh
            circumcenters = new List<Vector2>();
            var triangleCount = d.Triangles.Length / 3;
            for (var triangleId = 0; triangleId < triangleCount; triangleId++)
            {
                circumcenters.Add(d.GetTriangleCircumcenter(triangleId));
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

            // Compute exterior rays
            vectors = new Vector2[d.Points.Count * 2];
            var h = d.Hull[d.Hull.Length - 1];
            int p0, p1 = h * 2;
            Vector2 v0, v1 = points[h];
            for (var i = 0; i < d.Hull.Length; i++)
            {
                h = d.Hull[i];
                (p0, v0) = (p1, v1);
                p1 = h * 2;
                v1 = points[h];
                vectors[p0 + 1] = vectors[p1] = new Vector2(v0.y - v1.y, v1.x - v0.x);
            }
        }

        // Delauney triangulation dual to the voronoi cells
        public Delaunator Delaunator => d;

        // triangle circumcenters
        public List<Vector2> TriangleVertices => circumcenters;

        // Map from point index to half edge.
        // The starting half edge is the one with d.Halfedges[e] == -1
        // Or an arbitrary choice if there isn't one.
        public int[] Inedges => inedges;

        #region d3-delauney

        /// <summary>
        /// Returns the vertices of the voronoi cell, without any clipping.
        /// This means that unbounded cells will be missing the edges that extend to infinity,
        /// and may have less than 3 vertices.
        /// </summary>
        public List<Vector2> GetPolygon(int i)
        {
            var vertices = new List<Vector2>();
            if(GetPolygon(i, vertices, out var _, out var _))
            {
                return vertices;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Supplies the vertices of the voronoi cell, without any clipping.
        /// This means that unbounded cells will be missing the edges that extend to infinity,
        /// and may have less than 3 vertices.
        /// </summary>
        /// <param name="i">The voronoi cell</param>
        /// <param name="vertices">Filled with the vertices of the polygon.</param>
        /// <param name="ray1">For unbounded cells, the direction of the ray extending from vertex 0. Otherwise, empty.</param>
        /// <param name="ray2">For unbounded cells, the direction of the ray extending from vertex 0. Otherwise, empty.</param>
        /// <returns>True if successful</returns>
        public bool GetPolygon(int i, List<Vector2> vertices, out Vector2 ray1, out Vector2 ray2)
        {
            if(collinearNormal != null)
            {
                // There's nothing sensible to return here except for the ends
                // of the hull, it's not worth worrying abou
                ray1 = ray2 = default;
                return false;
            }

            var e0 = inedges[i];
            if (e0 == -1)
            {
                ray1 = ray2 = default;
                return false;
            }
            vertices.Clear();
            var e = e0;
            do
            {
                vertices.Add(circumcenters[EdgeIndexToTriangleIndex(e)]);
                e = d.Halfedges[NextHalfedge(e)];
            } while (e != e0 && e != -1);
            ray1 = vectors[i * 2];
            ray2 = vectors[i * 2 + 1];
            return true;
        }

        /// <summary>
        /// Returns the vertices of the voronoi cell i after clipping to the clipping rectangle.
        /// Returns null if the polygon is fully outside the clipping rectangle.
        /// </summary>
        public List<Vector2> GetClippedPolygon(int i)
        {
            // degenerate case (1 valid point: return the box)
            if (i == 0 && d.Hull.Length == 1)
            {
                return new List<Vector2>
                {
                    new Vector2(clipMax.x, clipMin.y),
                    clipMax,
                    new Vector2(clipMin.x, clipMax.y),
                    clipMin,
                };
            }

            if(collinearNormal != null)
            {
                return ClipCollinear(i);
            }

            var points = GetPolygon(i);
            var v = i * 2;
            if(vectors[v] == default)
            {
                return ClipFinite(i, points);
            }
            else
            {
                return ClipInfinite(i, points, vectors[v], vectors[v+1]);
            }
        }

        /// Clips vornoi cell i, a polygon with the given points to the clipping rectangle.
        /// Returns null if the the polygon doesn't intersect the bounds.
        private List<Vector2> ClipFinite(int i, List<Vector2> points)
        {
            if (points == null)
                return null;
            var n = points.Count;
            List<Vector2> P = null;
            Vector2 v0;
            Vector2 v1 = points[n - 1];
            int c0;
            int c1 = RegionCode(v1);
            int e0, e1 = 0;
            for (var j = 0; j < n; j += 1)
            {
                v0 = v1; v1 = points[j];
                c0 = c1; c1 = RegionCode(v1);
                if (c0 == 0 && c1 == 0)
                {
                    e0 = e1; e1 = 0;
                    if (P != null) P.Add(v1);
                    else P = new List<Vector2> { v1 };
                }
                else
                {
                    (Vector2, Vector2)? S;
                    Vector2 s0, s1;
                    if (c0 == 0)
                    {
                        if ((S = ClipSegment(v0, v1, c0, c1)) == null) continue;
                        (s0, s1) = S.Value;
                    }
                    else
                    {
                        if ((S = ClipSegment(v1, v0, c1, c0)) == null) continue;
                        (s1, s0) = S.Value;
                        e0 = e1; e1 = EdgeCode(s0);
                        if (e0 > 0 && e1 > 0) Edge(i, e0, e1, P, P.Count);
                        if (P != null) P.Add(s0);
                        else P = new List<Vector2> { s0 };
                    }
                    e0 = e1; e1 = EdgeCode(s1);
                    if (e0 > 0 && e1 > 0) Edge(i, e0, e1, P, P.Count);
                    if (P != null) P.Add(s1);
                    else P = new List<Vector2> { s1 };
                }
            }
            if (P != null)
            {
                e0 = e1; e1 = EdgeCode(P[0]);
                if (e0 > 0 && e1 > 0) Edge(i, e0, e1, P, P.Count);
            }
            else if (Contains(i, (clipMin + clipMax) / 2))
            {
                return new List<Vector2>
                {
                    new Vector2(clipMax.x, clipMin.y),
                    clipMax,
                    new Vector2(clipMin.x, clipMax.y),
                    clipMin
                };
            }
            return P;
        }

        // Clips a segment against the bounding rect.
        // You must have already computed the regioncodes for each vertex
        private (Vector2, Vector2)? ClipSegment(Vector2 v0, Vector2 v1, int c0, int c1)
        {
            while (true)
            {
                if (c0 == 0 && c1 == 0) return (v0, v1);
                if ((c0 & c1) != 0) return null;
                var c = c0 != 0 ? c0 : c1;
                Vector2 v;
                if ((c & 0b1000) != 0) v = new Vector2(v0.x + (v1.x - v0.x) * (clipMax.y - v0.y) / (v1.y - v0.y), clipMax.y);
                else if ((c & 0b0100) != 0) v = new Vector2(v0.x + (v1.x - v0.x) * (clipMin.y - v0.y) / (v1.y - v0.y), clipMin.y);
                else if ((c & 0b0010) != 0) v = new Vector2(clipMax.x, v0.y + (v1.y - v0.y) * (clipMax.x - v0.x) / (v1.x - v0.x));
                else v = new Vector2(clipMin.x, v0.y + (v1.y - v0.y) * (clipMin.x - v0.x) / (v1.x - v0.x));

                if (c0 != 0)
                {
                    v0 = v;
                    c0 = RegionCode(v0);
                }
                else
                {
                    v1 = v;
                    c1 = RegionCode(v1);
                }
            }
        }

        /// Clips voronoi cell i, an open polygon with the given points and two open rays, to the clipping rectabgle.
        /// Returns null if the the polygon doesn't intersect the bounds.
        private List<Vector2> ClipInfinite(int i, List<Vector2> points, Vector2 v0, Vector2 vn)
        {
            if (points == null)
                return null;
            var P = new List<Vector2>(points);
            Vector2? p;
            if ((p = Project(P[0], v0)).HasValue) P.Insert(0, p.Value);
            if ((p = Project(P[P.Count - 1], vn)).HasValue) P.Add(p.Value);
            if ((P = ClipFinite(i, P)) != null)
            {
                var n = P.Count;
                int c0, c1 = EdgeCode(P[n - 1]);
                for (var j = 0; j < n; j += 1)
                {
                    c0 = c1;
                    c1 = EdgeCode(P[j]);
                    if (c0 != 0 && c1 != 0)
                    {
                        j = Edge(i, c0, c1, P, j);
                        n = P.Count;
                    }
                }
            }
            else if (Contains(i, (clipMin + clipMax) / 2))
            {
                return new List<Vector2>()
                {
                    clipMin,
                    new Vector2(clipMax.x, clipMin.y),
                    clipMax,
                    new Vector2(clipMin.x, clipMax.y),
                };
            }
            return P;
        }

        /// <summary>
        /// Clips the voronoi cell to the clipping rectangle.
        /// This handles the case that there's only collinear points,
        /// which means that the polygon can be open on both sides.
        /// </summary>
        private List<Vector2> ClipCollinear(int i)
        {
            var n = collinearNormal.Value;
            var hi = hullIndex[i];
            if (hi == 0)
            {
                var v0 = this.points[d.Hull[0]];
                var v1 = this.points[d.Hull[1]];
                var points = new List<Vector2> { (v0 + v1) / 2 };
                return ClipInfinite(i, points, n, -n);
            }
            else if (hi == d.Hull.Length - 1)
            {
                var v0 = this.points[d.Hull[hi - 1]];
                var v1 = this.points[d.Hull[hi]];
                var points = new List<Vector2> { (v0 + v1) / 2 };
                return ClipInfinite(i, points, -n, n);
            }
            else if (hi == -1)
            {
                return null;
            }
            else
            {
                var v0 = this.points[d.Hull[hi - 1]];
                var v1 = this.points[d.Hull[hi]];
                var v2 = this.points[d.Hull[hi + 1]];
                var m1 = (v0 + v1) / 2;
                var m2 = (v1 + v2) / 2;
                var P = new List<Vector2>();
                Vector2? p;
                if ((p = Project(m1, -n)).HasValue) P.Add(p.Value);
                if ((p = Project(m1, n)).HasValue) P.Add(p.Value);
                if ((p = Project(m2, n)).HasValue) P.Add(p.Value);
                Edge(i, EdgeCode(P[P.Count - 2]), EdgeCode(P[P.Count - 1]), P, P.Count - 1);

                if ((p = Project(m2, -n)).HasValue) P.Add(p.Value);
                Edge(i, EdgeCode(P[P.Count - 1]), EdgeCode(P[0]), P, P.Count);
                return P;
            }
        }

        // Given an clipped segment of cell i that goes from edge code e0 to e1,
        // finds the appropriate corner points to insert into P at j.
        // Also removes co-linear edges.
        private int Edge(int i, int e0, int e1, List<Vector2> P, int j)
        {
            // Walk round any corners needed
            while (e0 != e1)
            {
                Vector2 u;
                switch (e0)
                {
                    case 0b0101: e0 = 0b0100; continue; // top-left
                    case 0b0100: e0 = 0b0110; u = new Vector2(clipMax.x, clipMin.y);  break; // top
                    case 0b0110: e0 = 0b0010; continue; // top-right
                    case 0b0010: e0 = 0b1010; u = new Vector2(clipMax.x, clipMax.y); break; // right
                    case 0b1010: e0 = 0b1000; continue; // bottom-right
                    case 0b1000: e0 = 0b1001; u = new Vector2(clipMin.x, clipMax.y); break; // bottom
                    case 0b1001: e0 = 0b0001; continue; // bottom-left
                    case 0b0001: e0 = 0b0101; u = new Vector2(clipMin.x, clipMin.y); break; // left
                    default: throw new System.Exception();
                }
                if ((j < 0 || j >= P.Count || P[j] != u) && Contains(i, u))
                {
                    P.Insert(j, u); j++;
                }
            }

            // Look for collinear points to remove
            if (P.Count > 2)
            {
                for (var i2 = 0; i2 < P.Count; i2 += 1)
                {
                    var j2 = (i2 + 1) % P.Count;
                    var k = (i2 + 2) % P.Count;
                    if (P[i2].x == P[j2].x && P[j2].x == P[k].x || P[i2].y == P[j2].y && P[j2].y == P[k].y)
                    {
                        P.RemoveAt(j2);
                        i2 -= 1;
                    }
                }
            }
            return j;
        }

        /// <summary>
        /// Returns the Voronoi cells that border the given cell.
        /// This ignores clipping.
        /// This may give surprising results in degenerate cases that more than 3 cells meet at a point.
        /// </summary>
        public IEnumerable<int> Neighbors(int i)
        {
            if (collinearNormal != null)
            {
                var hi = hullIndex[i];
                if (hi > 0)
                {
                    yield return d.Hull[hi - 1];
                }
                if (hi < d.Hull.Length - 1)
                {
                    yield return d.Hull[hi + 1];
                }
            }
            else
            {
                if (inedges.Length <= i || inedges[i] == -1) yield break;
                var e0 = inedges[i];
                var e = e0;
                do
                {
                    var t = d.Triangles[e];
                    yield return t;
                    var e1 = NextHalfedge(e);
                    if (d.Triangles[e1] != i) break; // bad triangulation
                    e = d.Halfedges[e1];
                    if (e == -1)
                    {
                        yield return d.Triangles[NextHalfedge(e1)];
                        break;
                    }
                } while (e != e0);
            }
        }

        /// <summary>
        /// Returns the Voronoi cells that border the given cell.
        /// This uses clipping.
        /// </summary>
        public IEnumerable<int> ClippedNeighbors(int i)
        {
            var ci = GetClippedPolygon(i);
            if (ci == null)
                yield break;
            foreach (var j in Neighbors(i))
            {
                var cj = GetClippedPolygon(j);
                if (cj == null) continue;
                // find the common edge
                var li = ci.Count;
                var lj = cj.Count;
                for (var ai = 0; ai < li; ai++)
                {
                    for (var aj = 0; aj < lj; aj++)
                    {
                        if (ci[ai] == cj[aj]
                        && ci[(ai + 1) % li] == cj[(aj + lj - 1) % lj]
                        )
                        {
                            yield return j;
                            ai = li;
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Finds the voronoi cell that contains the given point, or equivalently,
        /// finds the point that is nearest the given point.
        /// This ignores clipping, so it always succeeds.
        /// </summary>
        /// <param name="u">The point to search for.</param>
        /// <param name="i">Optional, the voronoi cell to start the search at. Useful if you know the returned cell will be nearby.</param>
        /// <returns></returns>
        public int Find(Vector2 u, int i = 0)
        {
            // TODO: Is this a NaN check?
            //if ((x = +x, x !== x) || (y = +y, y !== y)) return -1;
            var i0 = i;
            int c;
            while ((c = Step(i, u)) >= 0 && c != i && c != i0) i = c;
            return c;
        }

        // Is u inside voronoi cell i?
        private bool Contains(int i, Vector2 u)
        {
            // TODO: Is this a NaN check?
            //if ((x = +x, x != x) || (y = +y, y != y)) return false;
            return Step(i, u) == i;
        }

        // Finds a point that is closer to u than points[i],
        // or returns i if there isn't one.
        private int Step(int i, Vector2 u)
        {
            if (collinearNormal != null)
            {
                var c = i;
                var dc = (u - points[i]).sqrMagnitude;

                var hi = hullIndex[i];
                if(hi > 0)
                {
                    var t = d.Hull[hi - 1];
                    var dt = (u - points[t]).sqrMagnitude;
                    if(dt < dc)
                    {
                        dc = dt;
                        c = t;
                    }
                }
                if(hi < d.Hull.Length - 1)
                {

                    var t = d.Hull[hi + 1];
                    var dt = (u - points[t]).sqrMagnitude;
                    if (dt < dc)
                    {
                        dc = dt;
                        c = t;
                    }
                }
                return c;
            }
            else
            {
                if (inedges[i] == -1 || points.Count == 0) return (i + 1) % (points.Count);
                var c = i;
                var dc = (u - points[i]).sqrMagnitude;
                var e0 = inedges[i];
                var e = e0;
                do
                {
                    var t = d.Triangles[e];
                    var dt = (u - points[t]).sqrMagnitude;
                    if (dt < dc)
                    {
                        dc = dt;
                        c = t;
                    }
                    e = NextHalfedge(e);
                    if (d.Triangles[e] != i) break; // bad triangulation
                    e = d.Halfedges[e];
                    if (e == -1)
                    {
                        e = d.Hull[(hullIndex[i] + 1) % d.Hull.Length];
                        if (e != t)
                        {
                            if ((u - points[e]).sqrMagnitude < dc) return e;
                        }
                        break;
                    }
                } while (e != e0);
                return c;
            }
        }

        // Given a ray of origin u, and direction v,
        // Finds the intersection point with the rectangle bounds.
        // Returns null if the origin starts outside the bounds.
        private Vector2? Project(Vector2 u, Vector2 v)
        {
            var t = float.PositiveInfinity;
            float c;
            Vector2 r = new Vector2();
            if (v.y < 0)
            { // top
                if (u.y <= clipMin.y) return null;
                if ((c = (clipMin.y - u.y) / v.y) < t) r = new Vector2(u.x + (t = c) * v.x, clipMin.y);
            }
            else if (v.y > 0)
            { // bottom
                if (u.y >= clipMax.y) return null;
                if ((c = (clipMax.y - u.y) / v.y) < t) r = new Vector2(u.x + (t = c) * v.x, clipMax.y);
            }
            if (v.x > 0)
            { // right
                if (u.x >= clipMax.x) return null;
                if ((c = (clipMax.x - u.x) / v.x) < t) r = new Vector2(clipMax.x, u.y + (t = c) * v.y);
            }
            else if (v.x < 0)
            { // left
                if (u.x <= clipMin.x) return null;
                if ((c = (clipMin.x - u.x) / v.x) < t) r = new Vector2(clipMin.x, u.y + (t = c) * v.y);
            }
            return r;
        }

        private int EdgeCode(Vector2 v)
        {
            return (v.x == clipMin.x ? 0b0001 : v.x == clipMax.x ? 0b0010 : 0b0000)
                 | (v.y == clipMin.y ? 0b0100 : v.y == clipMax.y ? 0b1000 : 0b0000);
        }

        private int RegionCode(Vector2 v)
        {
            return (v.x < clipMin.x ? 0b0001 : v.x > clipMax.x ? 0b0010 : 0b0000)
                 | (v.y < clipMin.y ? 0b0100 : v.y > clipMax.y ? 0b1000 : 0b0000);
        }
        #endregion


        public enum PolygonStatus
        {
            /// <summary>
            /// A normal polygon 
            /// </summary>
            Normal,
            /// <summary>
            /// A polygon on the boundary, it extends outwards indefinitely. 
            /// </summary>
            Infinite,
            /// <summary>
            /// Single point occupies all of space 
            /// </summary>
            Solo,
            /// <summary>
            /// All the points are in a line, and this is not at either end of the line.
            /// </summary>
            Collinear,
            /// <summary>
            /// Something else has gone wrong, e.g. duplicate point.
            /// </summary>
            Error,
        }

        public PolygonStatus GetPolygonStatus(int i)
        {
            if (i == 0 && d.Hull.Length == 1)
            {
                return PolygonStatus.Solo;
            }
            if (collinearNormal != null)
            {
                // Correspodns to ClipCollinear

                var n = collinearNormal.Value;
                var hi = hullIndex[i];
                if (hi == 0)
                {
                    return PolygonStatus.Infinite;
                }
                else if (hi == d.Hull.Length - 1)
                {
                    return PolygonStatus.Infinite;
                }
                else if (hi == -1)
                {
                    return PolygonStatus.Error;
                }
                else
                {
                    return PolygonStatus.Collinear;
                }
            }

            // Corresponds to null == GetPolygon(i)
            var e0 = inedges[i];
            if (e0 == -1)
            {
                return PolygonStatus.Error;
            }

            var v = i * 2;
            if (vectors[v] == default)
            {
                // Crresponds to ClipFinite
                return PolygonStatus.Normal;
            }
            else
            {
                // Crresponds to ClipInfinite
                return PolygonStatus.Infinite;
            }
        }

        /// <summary>
        /// Returns the centroid of each voronoi cell.
        /// This is suitable for use with Lloyd relaxation.
        /// Unbounded cells are clipped down, which tends to move them inowards.
        /// </summary>
        public List<Vector2> GetRelaxedPoints()
        {
            var relaxedPoints = new List<Vector2>();
            for (var i = 0; i < points.Count; i++)
            {
                relaxedPoints.Add(GetCentroid(GetPolygon(i)));
            }
            return relaxedPoints;
        }

        public static Vector2 GetCentroid(List<Vector2> points)
        {
            float accumulatedArea = 0.0f;
            float centerX = 0.0f;
            float centerY = 0.0f;

            for (int i = 0, j = points.Count - 1; i < points.Count; j = i++)
            {
                var temp = points[i].x * points[j].y - points[j].x * points[i].y;
                accumulatedArea += temp;
                centerX += (points[i].x + points[j].x) * temp;
                centerY += (points[i].y + points[j].y) * temp;
            }

            if (System.Math.Abs(accumulatedArea) < 1E-7f)
                return new Vector2();

            accumulatedArea *= 3f;
            return new Vector2(centerX / accumulatedArea, centerY / accumulatedArea);
        }

        private IEnumerable<(Vector2, Vector2)> GetVoronoiEdges()
        {
            /*
            for (var e = 0; e < d.Triangles.Length; e++)
            {
                var o = d.Halfedges[e];
                if (e < o)
                {
                    var p = triangleVertices[D.TriangleOfEdge(e)];
                    var q = triangleVertices[D.TriangleOfEdge(o)];
                    yield return new Edge(e, p, q);
                }
            }
            */
            // TODO: Think about this
            return null;
        }


    }
}
