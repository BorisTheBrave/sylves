using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sylves
{
    // Experimentals
    internal class AabbChunks
    {
        const float eps = 1e-6f;

        private readonly Vector2 strideX;
        private readonly Vector2 strideY;
        private readonly Vector2 aabbBottomLeft;
        private readonly Vector2 aabbSize;
        private readonly Vector2 invStrideX;
        private readonly Vector2 invStrideY;

        private readonly Vector2Int[] chunksInFundamentalRhombus;

        internal AabbChunks(Vector2 strideX, Vector2 strideY, Vector2 aabbBottomLeft, Vector2 aabbSize)
        {
            this.strideX = strideX;
            this.strideY = strideY;
            this.aabbBottomLeft = aabbBottomLeft;
            this.aabbSize = aabbSize;
            // Treat strideX/Y as the columns of a 2 by matrix, and find the inverse columns
            var det = strideX.x * strideY.y - strideX.y * strideY.x;
            this.invStrideX = new Vector2(strideY.y, -strideX.y) / det;
            this.invStrideY = new Vector2(-strideY.x, strideX.x) / det;

            // The fundamental rhombus has sides equal to strideX and strideY, so it fully tiles plane
            // with every copy having the same relationship to the aabss

            var fundamentalRhombusCorners = new[]
            {
                Vector2.zero,
                strideX,
                strideX + strideY,
                strideY,
            };
            var minFR = fundamentalRhombusCorners.Aggregate(Vector2.Min);
            var maxFR = fundamentalRhombusCorners.Aggregate(Vector2.Max);
            // TODO: this could actually be a tighter bound
            chunksInFundamentalRhombus = GetChunkIntersects(minFR, maxFR).ToArray();

        }

        public (Vector2, Vector2) GetChunkBounds(Vector2Int chunk)
        {
            var bl = aabbBottomLeft + strideX * chunk.x + strideY * chunk.y;
            var tr = bl + aabbSize;
            return (bl, tr);
        }

        // Partitions the plane so that every point is associated with one chunk it belongs to
        public Vector2Int? GetUniqueChunk(Vector2 pos)
        {
            return GetChunkIntersects(pos, pos).Cast<Vector2Int?>().FirstOrDefault();
        }

        // Linear transformation that maps strideX to (1, 0) and strideY to (0, 1).
        private Vector2 Inv(Vector2 v)
        {
            return invStrideX * v.x + invStrideY * v.y;
        }

        // Returns all the chunks that overlap the given chunk.
        // It's inclusive on all sides.
        // Roughly equivalent to GetChunkIntersections(GetChunkBounds(chunk))
        // but less prone to rounding issues
        public IEnumerable<Vector2Int> GetChunkIntersects(Vector2Int chunk)
        {
            // Evaluate bounds on a central chunk
            // This cuts down on floating point issues as the chunks get further from the origin.
            var (min, max) = GetChunkBounds(new Vector2Int());
            return GetChunkIntersects(min, max).Select(x => x + chunk);
        }


        // Returns all the chunks that overlap min/max
        public IEnumerable<Vector2Int> GetChunkIntersects(Vector2 min, Vector2 max, SquareBound bound = null)
        {
            min -= aabbSize + aabbBottomLeft;
            max -= aabbBottomLeft;

            // Now just need to find the points on the lattice x * strideX + y * strideY 
            // inside min/max
            var center = (min + max) / 2;
            var axis1 = new Vector2(max.x - center.x, 0);
            var axis2 = new Vector2(0, max.y - center.y);

            center = Inv(center);
            axis1 = Inv(axis1);
            axis2 = Inv(axis2);

            // Now just need to find integer points inside the rhombus specified by center/axis1/axis2
            var s1 = axis1.x > 0 ? 1 : -1;
            var s2 = axis2.x > 0 ? 1 : -1;
            var right = center  + s1 * axis1 + s2 * axis2;
            var left = center - s1 * axis1 - s2 * axis2;
            var bottom= center - s1 * axis1 + s2 * axis2;
            var top = center + s1 * axis1 - s2 * axis2;
            if((axis1.x * axis2.y - axis1.y * axis2.x) * s1 * s2 > 0)
            {
                (top, bottom) = (bottom, top);
            }
            var minX = Mathf.CeilToInt(left.x);
            var maxX = Mathf.FloorToInt(right.x);
            if(bound != null)
            {
                minX = Math.Max(minX, bound.min.x);
                maxX = Math.Min(maxX, bound.max.x - 1);
            }
            for (var x = minX; x <= maxX; x++)
            {
                var minY = Mathf.CeilToInt(Math.Max(
                    bottom.x - left.x == 0 ? float.NegativeInfinity : (x - left.x) / (bottom.x - left.x) * (bottom.y - left.y) + left.y,
                    bottom.x - right.x == 0 ? float.NegativeInfinity : (x - right.x) / (bottom.x - right.x) * (bottom.y - right.y) + right.y
                    ));
                var maxY = Mathf.FloorToInt(Math.Min(
                    top.x - left.x == 0 ? float.PositiveInfinity : (x - left.x) / (top.x - left.x) * (top.y - left.y) + left.y,
                    top.x - right.x == 0 ? float.PositiveInfinity : (x - right.x) / (top.x - right.x) * (top.y - right.y) + right.y
                    ));
                if (bound != null)
                {
                    minY = Math.Max(minY, bound.min.y);
                    maxY = Math.Min(maxY, bound.max.y - 1);
                }
                for (var y = minY; y <= maxY; y++)
                {
                    yield return new Vector2Int(x, y);
                }
            }
        }

        private class RaycastInfoComparer : IComparer<RaycastInfo>
        {
            public int Compare(RaycastInfo x, RaycastInfo y)
            {
                return x.distance.CompareTo(y.distance);
            }
        }

        public IEnumerable<RaycastInfo> Raycast(Vector2 origin, Vector2 direction, float maxDistance, SquareBound bound = null)
        {
            // Raycast through a tiling of the fundamental rhombus
            // which in inverse space, is just a square grid
            var invOrigin = Inv(origin);
            var invDirection = Inv(direction);
            var fundRis = SquareGrid.Raycast(new Vector3(invOrigin.x, invOrigin.y, 0), new Vector3(invDirection.x, invDirection.y, 0), maxDistance, Vector2.one, bound);
            var comparer = new RaycastInfoComparer();

            var output = new List<RaycastInfo>(chunksInFundamentalRhombus.Length);
            HashSet<Cell> prevOutput = null;
            void Process(RaycastInfo fundamentalRi, double fundamentalMinDistance, double fundamentalMaxDistance)
            {
                output.Clear();
                foreach (var chunk in chunksInFundamentalRhombus)
                {
                    var actualChunk = new Vector2Int(fundamentalRi.cell.x + chunk.x, fundamentalRi.cell.y + chunk.y);
                    if(bound != null)
                    {
                        var inBounds = bound.min.x <= actualChunk.x && actualChunk.x < bound.max.x &&
                            bound.min.y <= actualChunk.y && actualChunk.y < bound.max.y;
                        if (!inBounds)
                        {
                            continue;
                        }
                    }

                    // Raycast vs one chunk. This could be optimized better
                    var (chunkMin, chunkMax) = GetChunkBounds(actualChunk);
                    var t1 = (chunkMin.x - origin.x) / direction.x;
                    var t2 = (chunkMax.x - origin.x) / direction.x;
                    var t3 = (chunkMin.y - origin.y) / direction.y;
                    var t4 = (chunkMax.y - origin.y) / direction.y;
                    if (direction.x < 0)
                        (t1, t2) = (t2, t1);
                    if (direction.y < 0)
                        (t3, t4) = (t4, t3);
                    var tmin = Math.Max(t1, t3);
                    var tmax = Math.Min(t2, t4);
                    if (
                        // No collision
                        tmin > tmax || 
                        // Collision is after ray segment
                        tmin > maxDistance || 
                        // Collision is before ray segment
                        tmax < 0 ||
                        // Collision is not in current fundamental rhombus
                        // We ignore for now for ordering reasons, it'll be found in a later rhombus
                        tmin >= fundamentalMaxDistance ||
                        tmin < fundamentalMinDistance)
                        continue;
                    output.Add(new RaycastInfo
                    {
                        cell = new Cell(actualChunk.x, actualChunk.y),
                        distance = tmin,
                    });
                }

                // There's actually a O(n) way of doing this by using aabbs presorted by each axis
                // but it's so fiddly that I feel it's not worth doing.
                output.Sort(comparer);
            }
            RaycastInfo? prev = null;
            var first = true;
            foreach(var ri in fundRis)
            {
                if (prev != null)
                {
                    Process(prev.Value, first ? float.NegativeInfinity : prev.Value.distance, ri.distance);
                    first = false;
                }
                prev = ri;
                foreach (var o in output)
                    yield return o;
            }
            if (prev != null)
            {
                Process(prev.Value, first ? float.NegativeInfinity : prev.Value.distance, maxDistance);
                foreach (var o in output)
                    yield return o;
            }
        }
    }
}
