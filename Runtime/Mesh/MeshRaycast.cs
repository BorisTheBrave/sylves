using UnityEngine;

namespace Sylves
{
    public static class MeshRaycast
    {
        private const float eps = 1e-7f;

        /// <summary>
        /// Performs a raycast in the XY plane of a ray vs a finite segment of a line.
        /// z-coordinates are completely ignored.
        /// </summary>
        public static bool RaycastSegmentPlanar(Vector3 rayOrigin, Vector3 direction, Vector3 v0, Vector3 v1, out Vector3 point, out float distance, out bool side)
        {
            var v = v1 - v0;
            var o = rayOrigin - v0;
            var denom = (-direction.x * v.y + direction.y * v.x);

            var t = (o.x * v.y - o.y * v.x) / denom;
            var u = (o.x * direction.y - o.y * direction.x) / denom;

            if(u < 0 || u > 1 || t < 0)
            {
                point = default;
                distance = default;
                side = default;
                return false;
            }
            else
            {
                point = rayOrigin + direction * t;
                distance = t;
                side = denom > 0;
                return true;
            }
        }

        public static bool RaycastPolygonPlanar(Vector3 rayOrigin, Vector3 direction, Vector3[] v, Matrix4x4 transform, out Vector3 point, out float distance, out int? side)
        {
            var i = transform.inverse;
            var success = RaycastPolygonPlanar(i.MultiplyPoint3x4(rayOrigin), i.MultiplyVector(direction), v, out var p, out distance, out side);
            point = transform.MultiplyPoint(p);
            return success;
        }

        public static bool RaycastPolygonPlanar(Vector3 rayOrigin, Vector3 direction, Vector3[] vs, out Vector3 point, out float distance, out int? side)
        {
            var bestDistance = float.PositiveInfinity;
            Vector3 bestPoint = default;
            int bestSide = default;
            bool bestS = default;
            for (var i = 0; i < vs.Length; i++)
            {
                var v0 = vs[i];
                var v1 = vs[(i + 1) % vs.Length];

                if(RaycastSegmentPlanar(rayOrigin, direction, v0, v1, out var p, out var d, out var s) && d < bestDistance)
                {
                    bestDistance = d;
                    bestPoint = p;
                    bestSide = i;
                    bestS = s;
                }
            }

            // TODO: Worry about interior
            if(bestDistance < float.PositiveInfinity)
            {
                if(bestS)
                {
                    distance = bestDistance;
                    point = bestPoint;
                    side = bestSide;

                }
                else
                {
                    // Starts inside
                    distance = 0;
                    point = rayOrigin;
                    side = null;
                }
                return true;
            }
            else
            {
                distance = default;
                point = default;
                side = default;
                return false;
            }
        }

        public static bool RaycastAabbPlanar(Vector3 rayOrigin, Vector3 direction, Vector3 min, Vector3 max, out float distance)
        {

            var t1x = (min.x - rayOrigin.x) / direction.x;
            var t1y = (min.y - rayOrigin.y) / direction.y;
            var t2x = (max.x - rayOrigin.x) / direction.x;
            var t2y = (max.y - rayOrigin.y) / direction.y;
            if (direction.x < 0) (t1x, t2x) = (t2x, t1x);
            if (direction.y < 0) (t1y, t2y) = (t2y, t1y);

            var t1 = Mathf.Max(Mathf.Max(0, t1x), t1y);
            var t2 = Mathf.Min(t2x, t2y);

            if (t1 > t2)
            {
                distance = default;
                return false;
            }

            distance = t1;
            return true;
        }

        public static bool RaycastAabb(Vector3 rayOrigin, Vector3 direction, Vector3 min, Vector3 max, out float distance)
        {
            var t1x = (min.x - rayOrigin.x) / direction.x;
            var t1y = (min.y - rayOrigin.y) / direction.y;
            var t1z = (min.z - rayOrigin.z) / direction.z;
            var t2x = (max.x - rayOrigin.x) / direction.x;
            var t2y = (max.y - rayOrigin.y) / direction.y;
            var t2z = (max.z - rayOrigin.z) / direction.z;
            if (direction.x < 0) (t1x, t2x) = (t2x, t1x);
            if (direction.y < 0) (t1y, t2y) = (t2y, t1y);
            if (direction.z < 0) (t1z, t2z) = (t2z, t1z);

            var t1 = Mathf.Max(Mathf.Max(0, t1x), Mathf.Max(t1y, t1z));
            var t2 = Mathf.Min(t2x, Mathf.Min(t2y, t2z));

            if (t1 > t2)
            {
                distance = default;
                return false;
            }

            distance = t1;
            return true;
        }

        /// <summary>
        /// Raycasts a ray vs a single triagnle.
        /// </summary>
        public static bool RaycastTri(Vector3 rayOrigin, Vector3 direction, Vector3 v0, Vector3 v1, Vector3 v2, out Vector3 point, out float distance)
        {
            return RaycastTri(rayOrigin, direction, v0, v1, v2, out point, out distance, out var _);
        }

        /// <summary>
        /// Raycasts a ray vs a single triagnle.
        /// </summary>
        public static bool RaycastTri(Vector3 rayOrigin, Vector3 direction, Vector3 v0, Vector3 v1, Vector3 v2, out Vector3 point, out float distance, out bool side)
        {
            //  Cribbed from a tutorial
            // https://www.scratchapixel.com/lessons/3d-basic-rendering/ray-tracing-rendering-a-triangle/ray-triangle-intersection-geometric-solution

            point = default;
            distance = default;

            // compute plane's normal
            var v0v1 = v1 - v0;
            var v0v2 = v2 - v0;
            // no need to normalize
            var N = Vector3.Cross(v0v1, v0v2); // N 
            float area2 = N.magnitude;

            // Step 1: finding P

            // check if ray and plane are parallel ?
            float NdotRayDirection = Vector3.Dot(N, direction);
            side = NdotRayDirection > 0;
            if (Mathf.Abs(NdotRayDirection) < eps) // almost 0 
                return false; // they are parallel so they don't intersect ! 

            // compute d parameter using equation 2
            float d = Vector3.Dot(N, v0);

            // compute t (equation 3)
            var t = (-Vector3.Dot(N, rayOrigin) + d) / NdotRayDirection;
            // check if the triangle is in behind the ray
            if (t < 0) return false; // the triangle is behind 

            // compute the intersection point using equation 1
            var P = rayOrigin + t * direction;

            // Step 2: inside-outside test
            Vector3 C; // vector perpendicular to triangle's plane 

            // edge 0
            var edge0 = v1 - v0;
            var vp0 = P - v0;
            C = Vector3.Cross(edge0, vp0);
            if (Vector3.Dot(N, C) < 0) return false; // P is on the right side 

            // edge 1
            var edge1 = v2 - v1;
            var vp1 = P - v1;
            C = Vector3.Cross(edge1, vp1);
            if (Vector3.Dot(N, C) < 0) return false; // P is on the right side 

            // edge 2
            var edge2 = v0 - v2;
            var vp2 = P - v2;
            C = Vector3.Cross(edge2, vp2);
            if (Vector3.Dot(N, C) < 0) return false; // P is on the right side; 

            distance = t;
            point = P;
            return true;
        }

        // Uses z-forward convention, (see TestMeshes.Cube)
        public static RaycastInfo? RaycastCube(Vector3 rayOrigin, Vector3 direction, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 v5, Vector3 v6, Vector3 v7, Vector3 v8)
        {
            Vector3 point;
            float bestDistance = float.MaxValue;
            float distance;
            RaycastInfo? bestHit = null;
            if (RaycastTri(rayOrigin, direction, v8, v7, v3, out point, out distance) && distance < bestDistance)
            {
                bestDistance = distance;
                bestHit = new RaycastInfo
                {
                    cellDir = (CellDir)CubeDir.Left,
                    point = point,
                    distance = distance,
                };
            }
            if (RaycastTri(rayOrigin, direction, v8, v3, v4, out point, out distance) && distance < bestDistance)
            {
                bestDistance = distance;
                bestHit = new RaycastInfo
                {
                    cellDir = (CellDir)CubeDir.Left,
                    point = point,
                    distance = distance,
                };
            }

            if (RaycastTri(rayOrigin, direction, v1, v2, v6, out point, out distance) && distance < bestDistance)
            {
                bestDistance = distance;
                bestHit = new RaycastInfo
                {
                    cellDir = (CellDir)CubeDir.Right,
                    point = point,
                    distance = distance,
                };
            }

            if (RaycastTri(rayOrigin, direction, v1, v6, v5, out point, out distance) && distance < bestDistance)
            {
                bestDistance = distance;
                bestHit = new RaycastInfo
                {
                    cellDir = (CellDir)CubeDir.Right,
                    point = point,
                    distance = distance,
                };
            }

            if (RaycastTri(rayOrigin, direction, v3, v7, v6, out point, out distance) && distance < bestDistance)
            {
                bestDistance = distance;
                bestHit = new RaycastInfo
                {
                    cellDir = (CellDir)CubeDir.Up,
                    point = point,
                    distance = distance,
                };
            }

            if (RaycastTri(rayOrigin, direction, v3, v6, v2, out point, out distance) && distance < bestDistance)
            {
                bestDistance = distance;
                bestHit = new RaycastInfo
                {
                    cellDir = (CellDir)CubeDir.Up,
                    point = point,
                    distance = distance,
                };
            }

            if (RaycastTri(rayOrigin, direction, v1, v5, v8, out point, out distance) && distance < bestDistance)
            {
                bestDistance = distance;
                bestHit = new RaycastInfo
                {
                    cellDir = (CellDir)CubeDir.Down,
                    point = point,
                    distance = distance,
                };
            }

            if (RaycastTri(rayOrigin, direction, v1, v8, v4, out point, out distance) && distance < bestDistance)
            {
                bestDistance = distance;
                bestHit = new RaycastInfo
                {
                    cellDir = (CellDir)CubeDir.Down,
                    point = point,
                    distance = distance,
                };
            }

            if (RaycastTri(rayOrigin, direction, v5, v6, v7, out point, out distance) && distance < bestDistance)
            {
                bestDistance = distance;
                bestHit = new RaycastInfo
                {
                    cellDir = (CellDir)CubeDir.Forward,
                    point = point,
                    distance = distance,
                };
            }

            if (RaycastTri(rayOrigin, direction, v5, v7, v8, out point, out distance) && distance < bestDistance)
            {
                bestDistance = distance;
                bestHit = new RaycastInfo
                {
                    cellDir = (CellDir)CubeDir.Forward,
                    point = point,
                    distance = distance,
                };
            }

            if (RaycastTri(rayOrigin, direction, v4, v3, v2, out point, out distance) && distance < bestDistance)
            {
                bestDistance = distance;
                bestHit = new RaycastInfo
                {
                    cellDir = (CellDir)CubeDir.Back,
                    point = point,
                    distance = distance,
                };
            }

            if (RaycastTri(rayOrigin, direction, v4, v2, v1, out point, out distance) && distance < bestDistance)
            {
                bestDistance = distance;
                bestHit = new RaycastInfo
                {
                    cellDir = (CellDir)CubeDir.Back,
                    point = point,
                    distance = distance,
                };
            }

            return bestHit;
        }
    }
}
