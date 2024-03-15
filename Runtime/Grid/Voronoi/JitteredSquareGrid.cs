using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sylves
{
    public class JitteredSquareGrid : PlanarLazyMeshGrid
    {
        // Extra border to add to each chunk to ensure they see every relevant point
        private const int b = 2;

        private readonly int chunkSize;
        private readonly int seed;

        public JitteredSquareGrid(int chunkSize = 10, int? seed = null, ICachePolicy cachePolicy = null)
            : base()
        {
            this.chunkSize = chunkSize;
            this.seed = seed ?? new System.Random().Next();

            Setup(GetMeshData, Vector2.right * chunkSize, Vector2.up * chunkSize, new Vector2(-b, -b), new Vector2(chunkSize + 2 * b, chunkSize + 2 * b), false, cachePolicy: cachePolicy);
        }

        protected JitteredSquareGrid(JitteredSquareGrid other, SquareBound bound)
            :base(other, bound)
        {
            this.chunkSize = other.chunkSize;
            this.seed = other.seed;
        }

        protected virtual Vector2 GetPointInCell(Cell cell)
        {
            var cellSeed = HashUtils.Hash(cell.x, cell.y, seed);
            var random = new System.Random(cellSeed);
            var p = new Vector2(cell.x + (float)random.NextDouble(), cell.y + (float)random.NextDouble());
            return p;
        }

        private static int FlooredDiv(int a, int b)
        {
            if (((a < 0) ^ (b < 0)) && (a % b != 0))
            {
                return (a / b - 1);
            }
            else
            {
                return (a / b);
            }
        }

        private static int PMod(int a, int b)
        {
            return ((a % b) + b) % b;
        }

        protected override (Cell childCell, Cell chunkCell) Split(Cell cell)
        {
            var x = PMod(cell.x, chunkSize);
            var y = PMod(cell.y, chunkSize);
            return (
                // Child grid is already raveled
                new Cell(x + y * chunkSize, 0),
                new Cell(FlooredDiv(cell.x, chunkSize), FlooredDiv(cell.y, chunkSize))
                );
        }

        protected override Cell Combine(Cell childCell, Cell chunkCell)
        {
            var x = childCell.x % chunkSize;
            var y = childCell.x / chunkSize;
            return new Cell(x + chunkCell.x * chunkSize, y + chunkCell.y * chunkSize);
        }

        private MeshData GetMeshData(Vector2Int chunk)
        {
            var chunkCell = new Cell(chunk.x, chunk.y);
            // Find all cells in chunk, plus border
            var children = new List<Vector3Int>();
            for (var y = -b; y < chunkSize + b; y++)
            {
                for (var x = -b; x < chunkSize + b; x++)
                {
                    children.Add(new Vector3Int(x, y, 0));
                }
            }
            var bottomLeft = Combine(new Cell(), chunkCell);
            var cells = children.Select(c => bottomLeft + c).ToList();
            var points = cells.Select(c => GetPointInCell(c)).ToList();
            var meshData = VoronoiGrid.CreateMeshData(points, mask: i => Split(cells[i]).chunkCell == chunkCell);
            return meshData;
        }

        public override IGrid BoundBy(IBound bound) => new JitteredSquareGrid(this, (SquareBound)IntersectBounds(this.GetBound(), bound));

        public override IGrid Unbounded => new JitteredSquareGrid(this, null);

        public override int CoordinateDimension => 2;
    }
}
