using System;
using System.Collections.Generic;
using System.Text;

namespace Sylves
{
    // Very similar to Unity.Bounds but we don't use that term as it classes with Sylves.IBound
    public struct Aabb
    {
        private Aabb(Vector3 min, Vector3 max)
        {
            Min = min;
            Max = max;
        }
        public static Aabb FromMinMax(Vector3 min, Vector3 max) => new Aabb(min, max);

        public static Aabb? FromVectors(IEnumerable<Vector3> vectors)
        {
            var first = true;
            Vector3 localMin = default, localMax = default;
            foreach (var v in vectors)
            {
                if (first)
                {
                    localMin = localMin = v;
                    first = false;
                }
                else
                {
                    localMin = Vector3.Min(localMin, v);
                    localMax = Vector3.Min(localMax, v);
                }
            }
            if(first)
            {
                return null;
            }
            else
            {
                return FromMinMax(localMin, localMax);
            }
        }


        public Vector3 Min { get; }
        public Vector3 Max { get; }

        public Vector3 Center => (Max + Min) * 0.5f;
        public Vector3 Size => (Max - Min);
        public Vector3 Extents => (Max - Min) * 0.5f;



        public static Aabb Union(IEnumerable<Aabb> aabbs)
        {
            var i = aabbs.GetEnumerator();
            i.MoveNext();
            var first = i.Current;
            var min = first.Min;
            var max = first.Max;
            while (i.MoveNext())
            {
                var current = i.Current;
                min = Vector3.Min(min, current.Min);
                max = Vector3.Max(max, current.Max);
            }
            return FromMinMax(min, max);
        }

        public bool Intersects(Aabb other)
        {
            if (this.Max.x < other.Min.x ||
                this.Min.x > other.Max.x ||
                this.Max.y < other.Min.y ||
                this.Min.y > other.Max.y ||
                this.Max.z < other.Min.z ||
                this.Min.z > other.Max.z)
            {
                return false;
            }
            return true;
        }

        public float? Raycast(Vector3 origin, Vector3 direction, float maxDistance)
        {
            if (MeshRaycast.RaycastAabbPlanar(origin, direction, Min, Max, out var distance) && distance <= maxDistance)
            {
                return distance;
            }
            else
            {
                return null;
            }
        }

        public static Aabb operator*(Matrix4x4 m, Aabb aabb)
        {
            var min = aabb.Min;
            var max = aabb.Max;
            Transform(m, ref min, ref max);
            return new Aabb(min, max);
        }

        internal static void Transform(Matrix4x4 m, ref Vector3 min, ref Vector3 max)
        {
            var center = (min + max) * 0.5f;
            var extents = (max - min) * 0.5f;
            extents = new Vector3(Mathf.Abs(extents.x), Mathf.Abs(extents.y), Mathf.Abs(extents.z));
            min = center - extents;
            max = center + extents;
        }

    }
}
