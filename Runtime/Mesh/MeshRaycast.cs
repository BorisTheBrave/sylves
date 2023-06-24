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
        public static bool RaycastSegment(Vector3 rayOrigin, Vector3 direction, Vector3 v0, Vector3 v1, out Vector3 point, out float distance)
        {
            v1 -= v0;
            var o = rayOrigin - v0;
            var denom = (-direction.x * v1.y + direction.y * v1.x);

            var t = (o.x * v1.y - o.y * v1.x) / denom;
            var u = (o.x * direction.y - o.y * direction.x) / denom;

            if(u < 0 || u > 1 || t < 0)
            {
                point = default;
                distance = 0;
                return false;
            }
            else
            {
                point = rayOrigin + direction * t;
                distance = t;
                return true;
            }
        }

        /// <summary>
        /// Raycasts a ray vs a single triagnle.
        /// </summary>
        public static bool RaycastTri(Vector3 rayOrigin, Vector3 direction, Vector3 v0, Vector3 v1, Vector3 v2, out Vector3 point, out float distance)
        {
            return RaycastTri(rayOrigin, direction, v0, v1, v2, out point, out distance, out var _);
        }

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
