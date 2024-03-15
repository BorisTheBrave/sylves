using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sylves
{
    // TODO: Use nested grid instead of PlanarLazyGrid?
    /// <summary>
    /// Variant of a Square grid with irregularly shaped rectangles.
    /// https://gitlab.com/chriscox/offgrid/-/wikis/home
    /// 
    /// Cell (0, 0, 0) is a random rectangle filling at most from (-1, -1, 0) to (1, 1, 0),
    /// and having height and width at least minSize
    /// </summary>
    internal class OffGrid : PlanarLazyGrid
    {
        private static readonly ICellType[] cellTypes = new[] { SquareCellType.Instance };
        private static readonly SquareGrid unitSquareGrid = new SquareGrid(1f);
        private static readonly SquareGrid boundedUnitSquareGrid = new SquareGrid(1f, new SquareBound(0, 0, 1, 1));

        private readonly float minSize;
        private readonly ICachePolicy cachePolicy;
        private readonly int seed;

        public OffGrid(float minSize = 0.2f, SquareBound bound = null, int? seed = null, ICachePolicy cachePolicy = null) 
            : base(Vector2.right, Vector2.up, Vector2.one * (1 - minSize / 2) * -1, Vector2.one * (2 - minSize), bound, cellTypes, cachePolicy)
        {
            if (minSize > 1 || minSize < 0)
                throw new ArgumentException($"{minSize} is an invalid value for {nameof(minSize)}", nameof(minSize));
            this.minSize = minSize;
            this.cachePolicy = cachePolicy;
            this.seed = seed ?? new System.Random().Next();
        }

        // InternalOffGrid has a single cell per-chunk, so we can p
        protected override (Cell childCell, Cell chunkCell) Split(Cell cell)
        {
            return (new Cell(), cell);
        }

        protected override Cell Combine(Cell childCell, Cell chunkCell)
        {
            return chunkCell;
        }

        // TODO: Intersect bounds
        public override IGrid BoundBy(IBound bound) => new OffGrid(minSize, (SquareBound)bound, seed, cachePolicy);

        public override IGrid Unbounded => new OffGrid(minSize, null, seed, cachePolicy);

        protected override IEnumerable<Cell> GetAdjacentChunks(Cell chunkCell)
        {
            yield return chunkCell + Vector3Int.left;
            yield return chunkCell + Vector3Int.right;
            yield return chunkCell + Vector3Int.up;
            yield return chunkCell + Vector3Int.down;
        }

        private float GetValue(int x, int y)
        {
            var rectSeed = HashUtils.Hash(x, y, seed);
            var pureRandom = (float)new System.Random(rectSeed).NextDouble();
            return minSize / 2 + pureRandom * (1 - minSize);
        }

        protected override IGrid GetChildGrid(Cell v)
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
            return boundedUnitSquareGrid.Transformed(
                // Position at min/max
                Matrix4x4.Translate(new Vector3((minX + maxX) / 2, (minY + maxY) / 2, 0)) * 
                // Correct width
                Matrix4x4.Scale(new Vector3(maxX - minX, maxY - minY, 1)) *
                // Center of cell at 0,0
                Matrix4x4.Translate(new Vector3(-0.5f, -0.5f, 0)));
        }

        public override bool TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection)
        {
            var (childCell, chunkCell) = Split(cell);
            if(unitSquareGrid.TryMove(chunkCell, dir, out dest, out inverseDir, out connection))
            {
                dest = Combine(childCell, dest);
                return true;
            }
            return false;
        }
    }
}
