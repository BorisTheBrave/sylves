using System;
using System.Collections.Generic;
#if UNITY
using UnityEngine;
#endif

namespace Sylves
{
    /// <summary>
    /// Variant of a Square grid with irregularly shaped rectangles.
    /// https://gitlab.com/chriscox/offgrid/-/wikis/home
    /// 
    /// Cell (0, 0, 0) is a random rectangle filling at most from (-1, -1, 0) to (1, 1, 0),
    /// and having height and width at least minSize
    /// </summary>
    internal class InternalOffGrid : PlanarLazyGrid
    {
        private static readonly ICellType[] cellTypes = new[] { SquareCellType.Instance };
        private static readonly SquareGrid unitSquareGrid = new SquareGrid(1f, new SquareBound(0, 0, 1, 1));

        private readonly float minSize;
        private readonly ICachePolicy cachePolicy;
        private readonly int seed;

        public InternalOffGrid(float minSize = 0.2f, SquareBound bound = null, int? seed = null, ICachePolicy cachePolicy = null) 
            : base(Vector2.right, Vector2.up, Vector2.one * (1 - minSize / 2) * -1, Vector2.one * (2 - minSize), bound, cellTypes, cachePolicy)
        {
            if (minSize > 1 || minSize < 0)
                throw new ArgumentException($"{minSize} is an invalid value for {nameof(minSize)}", nameof(minSize));
            this.minSize = minSize;
            this.cachePolicy = cachePolicy;
            this.seed = seed ?? new System.Random().Next();
        }

        // TODO: Intersect bounds
        public override IGrid BoundBy(IBound bound) => new InternalOffGrid(minSize, (SquareBound)bound, seed, cachePolicy);

        public override IGrid Unbounded => new InternalOffGrid(minSize, null, seed, cachePolicy);

        protected override IEnumerable<Vector2Int> GetAdjacentChunks(Vector2Int chunk)
        {
            yield return new Vector2Int(chunk.x-1, chunk.y);
            yield return new Vector2Int(chunk.x+1, chunk.y);
            yield return new Vector2Int(chunk.x, chunk.y-1);
            yield return new Vector2Int(chunk.x, chunk.y+1);
        }

        private float GetValue(int x, int y)
        {
            var rectSeed = HashUtils.Hash(x, y, seed);
            var pureRandom = (float)new System.Random(rectSeed).NextDouble();
            return minSize / 2 + pureRandom * (1 - minSize);
        }

        protected override IGrid GetChunkGrid(Vector2Int v)
        {
            float minY, maxY, minX, maxX;
            if(((v.x + v.y) & 1) == 0)
            {
                minY = GetValue(v.x + 0, v.y + 0) + v.y + 0 - 1;
                maxY = GetValue(v.x + 1, v.y + 1) + v.y + 1 - 1;
                minX = GetValue(v.x + 0, v.y + 1) + v.x + 0 - 1;
                maxX = GetValue(v.x + 1, v.y + 0) + v.x + 1 - 1;
            }
            else
            {
                minY = GetValue(v.x + 1, v.y + 0) + v.y + 0 - 1;
                maxY = GetValue(v.x + 0, v.y + 1) + v.y + 1 - 1;
                minX = GetValue(v.x + 0, v.y + 0) + v.x + 0 - 1;
                maxX = GetValue(v.x + 1, v.y + 1) + v.x + 1 - 1;
            }
            return unitSquareGrid.Transformed(
                // Position at min/max
                Matrix4x4.Translate(new Vector3((minX + maxX) / 2, (minY + maxY) / 2, 0)) * 
                // Correct width
                Matrix4x4.Scale(new Vector3(maxX - minX, maxY - minY, 1)) *
                // Center of cell at 0,0
                Matrix4x4.Translate(new Vector3(-0.5f, -0.5f, 0)));
        }

        public override bool TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection)
        {
            var (meshCell, chunk) = Split(cell);
            if(unitSquareGrid.TryMove(new Cell(chunk.x, chunk.y), dir, out dest, out inverseDir, out connection))
            {
                chunk.x = dest.x;
                chunk.y = dest.y;
                dest = Combine(meshCell, chunk);
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Variant of a Square grid with irregularly shaped rectangles.
    /// https://gitlab.com/chriscox/offgrid/-/wikis/home
    /// 
    /// For cellsSize (1, 1), then cell (0, 0, 0) is a random rectangle filling at most from (-1, -1, 0) to (1, 1, 0)
    /// </summary>
    public class OffGrid : BijectModifier
    {
        public OffGrid(float minSize = 0.2f, SquareBound bound = null, int? seed = null, ICachePolicy cachePolicy = null) :
            base(new InternalOffGrid(minSize, bound, seed, cachePolicy),
                c => new Cell(0, c.x, c.y),
                c => new Cell(c.y, c.z, 0),
                2
                )
        { }
    }
}
