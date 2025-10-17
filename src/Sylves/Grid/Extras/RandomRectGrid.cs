using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylves
{
    // See https://www.boristhebrave.com/2025/10/17/infinite-random-rhombus-tilings/
    public class RandomRectGrid
    {
        private static SquareGrid s_squareGrid = new SquareGrid(1);
        private static IDualMapping s_dual = s_squareGrid.GetDual();

        private class Layer
        {
            Func<Cell, SquareDir> DefaultPairing;
            Dictionary<Cell, SquareDir> Pairings;
            SquareBound Bound;

            public Layer(Func<Cell, SquareDir> defaultPairing = null, SquareBound bound = null)
            {
                DefaultPairing = defaultPairing;
                Pairings = new Dictionary<Cell, SquareDir>();
                Bound = bound;
            }

            public static SquareDir BasicPairing(Cell cell)
            {
                return (Math.Abs(cell.y) % 2) == 0 ? SquareDir.Up : SquareDir.Down;
            }

            public SquareDir? GetPairing(Cell cell)
            {
                if (Bound != null && !Bound.Contains(cell))
                    return default;


                if (Pairings.TryGetValue(cell, out var pairing))
                {
                    return pairing;
                }
                else
                {
                    return Pairings[cell] = DefaultPairing(cell);
                }
            }

            public bool RotateAt(Cell dualCell)
            {
                var c_dr = s_dual.ToBasePair(dualCell, (CellCorner)SquareCorner.DownRight).Value.baseCell;
                var c_ur = s_dual.ToBasePair(dualCell, (CellCorner)SquareCorner.UpRight).Value.baseCell;
                var c_ul = s_dual.ToBasePair(dualCell, (CellCorner)SquareCorner.UpLeft).Value.baseCell;
                var c_dl = s_dual.ToBasePair(dualCell, (CellCorner)SquareCorner.DownLeft).Value.baseCell;

                var d_dr = GetPairing(c_dr);
                var d_ur = GetPairing(c_ur);
                var d_ul = GetPairing(c_ul);
                var d_dl = GetPairing(c_dl);

                if (d_dr == SquareDir.Up && d_ur == SquareDir.Down && d_dl == SquareDir.Up && d_ul == SquareDir.Down)
                {
                    Pairings[c_dr] = SquareDir.Left;
                    Pairings[c_ur] = SquareDir.Left;
                    Pairings[c_ul] = SquareDir.Right;
                    Pairings[c_dl] = SquareDir.Right;
                    return true;
                }
                if (d_dr == SquareDir.Left && d_ur == SquareDir.Left && d_dl == SquareDir.Right && d_ul == SquareDir.Right)
                {
                    Pairings[c_dr] = SquareDir.Up;
                    Pairings[c_ur] = SquareDir.Down;
                    Pairings[c_ul] = SquareDir.Down;
                    Pairings[c_dl] = SquareDir.Up;
                    return true;
                }

                return false;
            }

            public static SquareBound Shrink(SquareBound bound)
            {
                return new SquareBound(bound.Min + Vector2Int.one, bound.Mex - Vector2Int.one);
            }

            public void RandomRotate(SquareBound bound, float ratio = 10)
            {
                bound = Shrink(bound);

                var t = new SquareGrid(1f, bound);
                var dual = t.GetDual();
                var duals = dual.DualGrid.GetCells().ToList();

                // Randomly rotate the rhombuses until the pairs are good and shuffled
                var r = new Random();
                for (var i = 0; i < duals.Count * ratio; i++)
                {
                    var d = duals[(int)(r.NextDouble() * duals.Count)];
                    RotateAt(d);
                }
            }

            public MeshData GetMeshData(SquareBound bound, bool handleOverlap)
            {
                // Find all pairs
                var visited = new HashSet<Cell>();
                var unpaired = new HashSet<Cell>();
                var pairs = new List<(Cell, CellDir, Cell, CellDir)>();
                foreach (var cell in bound)
                {
                    if (visited.Contains(cell))
                        continue;
                    var dir = GetPairing(cell).Value;
                    s_squareGrid.TryMove(cell, (CellDir)dir, out var otherCell, out var inverseDir, out var _);
                    if(bound.Contains(otherCell) == false)
                    {
                        if (handleOverlap)
                        {
                            if(KruskalMinimumSpanningTree.LexCompare(cell, otherCell))
                            {
                                // Skip entirely - it'll get handled in the other chunk
                                continue;
                            }
                            else
                            {
                                // Add as usual
                                visited.Add(cell);
                                visited.Add(otherCell);
                            }
                        }
                        else
                        {
                            // Just leave as triangle and continue
                            unpaired.Add(cell);
                            visited.Add(cell);
                            continue;
                        }
                    }
                    pairs.Add((
                        cell,
                        (CellDir)(((int)dir)),
                        otherCell,
                        (CellDir)(((int)inverseDir))
                        ));
                    visited.Add(cell);
                    visited.Add(otherCell);
                }

                // Build the base meshgrid
                var t = s_squareGrid.Masked(visited);
                var meshData = t.ToMeshData();
                var meshGrid = new MeshGrid(meshData);

                // Find re-indexing into meshData
                var c = 0;
                var x = new Dictionary<Cell, Cell>();
                foreach (var cell in t.GetCells())
                {
                    x[cell] = new Cell(c++, 0, 0);
                }

                // Re-index pairs
                pairs = pairs.Select(p => (x[p.Item1], p.Item2, x[p.Item3], p.Item4)).ToList();
                unpaired = new HashSet<Cell>(unpaired.Select(p => x[p]));


                return MeshDataOperations.ApplyPairing(meshData, meshGrid, pairs, unpaired);
            }
        }

        class ChunkedLayer
        {
            public Layer Layer;
            HashSet<Cell> EvaluatedChunks;
            int ChunkSize;
            Vector3Int ChunkOffset;

            public ChunkedLayer(Func<Cell, SquareDir> defaultPairing, int chunkSize, Vector3Int chunkOffset)
            {
                Layer = new Layer(defaultPairing);
                EvaluatedChunks = new HashSet<Cell>();
                ChunkSize = chunkSize;
                ChunkOffset = chunkOffset;
            }
            Cell GetChunk(Cell cell)
            {
                cell -= ChunkOffset;
                // Group into triangles of side ChunkSize
                var x = Mathf.CeilToInt(cell.x * 1f / ChunkSize);
                var y = Mathf.CeilToInt(cell.y * 1f / ChunkSize);
                // As HexGrid.GetTriangleParent.
                return new Cell(
                    x, y
                );
            }
            public SquareBound GetChunkBound(Cell chunk)
            {
                // Should be an ivnerse of GetChunk
                return new SquareBound(
                    new Vector2Int(chunk.x * ChunkSize, chunk.y * ChunkSize) + new Vector2Int(ChunkOffset.x, ChunkOffset.y),
                    new Vector2Int((chunk.x + 1) * ChunkSize, (chunk.y + 1) * ChunkSize) + new Vector2Int(ChunkOffset.x, ChunkOffset.y)
                    );
            }
            public SquareDir GetPairing(Cell cell)
            {
                var chunk = GetChunk(cell);
                if (!EvaluatedChunks.Contains(chunk))
                {
                    var chunkBound = GetChunkBound(chunk);
                    Layer.RandomRotate(chunkBound, 10);
                    EvaluatedChunks.Add(chunk);
                }

                return Layer.GetPairing(cell) ?? throw new Exception();

            }
        }

        public static IGrid MakeFinite(SquareBound bound)
        {
            // Setup
            var layer = new Layer(Layer.BasicPairing, bound);

            layer.RandomRotate(bound, 10);
            var meshData2 = layer.GetMeshData(bound, false);

            return new MeshGrid(meshData2);
        }

        public static IGrid MakeInfinite()
        {
            var chunkSize = 4;
            var layer1 = new ChunkedLayer(Layer.BasicPairing, chunkSize, new Vector3Int(chunkSize / 2, 0, 0));
            var layer2 = new ChunkedLayer(layer1.GetPairing, chunkSize, new Vector3Int(chunkSize / 2, chunkSize / 2, 0));
            var layer3 = new ChunkedLayer(layer2.GetPairing, chunkSize, new Vector3Int(0, 0, 0));

            MeshData GetMeshData(Cell chunk)
            {
                var bound = layer3.GetChunkBound(chunk);
                var meshData = layer3.Layer.GetMeshData(bound, true);
                return meshData.Weld();
            }
            var chunkGrid = new SquareGrid(chunkSize);

            // We need a margin the size of one square to account for rectangles
            // that stick out of the chunk
            return new PlanarLazyMeshGrid(GetMeshData, chunkGrid, margin: 1f, meshGridOptions: new MeshGridOptions { Tolerance = 0.1f });
        }

}
}
