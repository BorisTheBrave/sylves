using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sylves
{
    internal static class GeometryUtils
    {
        /// <summary>
        /// Returns true if p is in the triangle po, p1, p2
        /// </summary>
        public static bool IsPointInTriangle(Vector3 p, Vector3 p0, Vector3 p1, Vector3 p2, float planarThickness = 1e-35f)
        {
            var n = Vector3.Cross(p1 - p0, p2 - p0);

            var o = Vector3.Dot(p - p2, n) / n.magnitude;
            if (!(-planarThickness <= o && o <= planarThickness))
                return false;

            var s = Vector3.Dot(n, Vector3.Cross(p0 - p2, p - p2));
            var t = Vector3.Dot(n, Vector3.Cross(p1 - p0, p - p0));

            if ((s < 0) != (t < 0) && s != 0 && t != 0)
                return false;

            var d = Vector3.Dot(n, Vector3.Cross(p2 - p1, p - p1));
            return d == 0 || (d < 0) == (s + t <= 0);
        }


        /// <summary>
        /// Returns true if p is in the triangle po, p1,p2
        /// Ignores the z-axis
        /// </summary>
        public static bool IsPointInTrianglePlanar(Vector3 p, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            var n = Vector3.Cross(p1 - p0, p2 - p0);

            if (n.sqrMagnitude == 0)
                return false;

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

    }
}
