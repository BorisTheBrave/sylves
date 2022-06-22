using System;
using System.Collections.Generic;
#if UNITY
using UnityEngine;
#endif

namespace Sylves
{
    // Experimentals
    internal class AabbChunks
    {
        private readonly Vector2 strideX;
        private readonly Vector2 strideY;
        private readonly Vector2 aabbBottomLeft;
        private readonly Vector2 aabbSize;
        private readonly Vector2 invStrideX;
        private readonly Vector2 invStrideY;

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
        }

        public (Vector2, Vector2) GetChunkBounds(Vector2Int chunk)
        {
            var bl = aabbBottomLeft + strideX * chunk.x + strideY * chunk.y;
            var tr = bl + aabbSize;
            return (bl, tr);
        }

        private Vector2 Inv(Vector2 v)
        {
            return invStrideX * v.x + invStrideY * v.y;
        }

        public IEnumerable<Vector2Int> GetChunkIntersects(Vector2 min, Vector2 max)
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
                for (var y = minY; y <= maxY; y++)
                {
                    yield return new Vector2Int(x, y);
                }
            }
        }
    }
}
