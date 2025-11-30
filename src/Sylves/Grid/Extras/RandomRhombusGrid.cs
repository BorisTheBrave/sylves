using System;
using System.Collections.Generic;
using System.Linq;

#if UNITY
using UnityEngine;
#endif

namespace Sylves
{
    // See https://www.boristhebrave.com/2025/10/17/infinite-random-rhombus-tilings/
    public class RandomRhombusGrid
    {
        private static HexGrid s_parentHexGrid = new HexGrid(1f, HexOrientation.FlatTopped);
        private static TriangleGrid s_triangleGrid = new TriangleGrid(0.5f, TriangleOrientation.FlatTopped);
        private static IDualMapping s_dual = s_triangleGrid.GetDual();

        private class Layer
        {
            Func<Cell, FTTriangleDir> DefaultPairing;
            Dictionary<Cell, FTTriangleDir> Pairings;
            TriangleBound Bound;

            public Layer(Func<Cell, FTTriangleDir> defaultPairing = null, TriangleBound bound = null)
            {
                DefaultPairing = defaultPairing;
                Pairings = new Dictionary<Cell, FTTriangleDir>();
                Bound = bound;
            }

            public static FTTriangleDir RhombillePairing(Cell cell)
            {
                var hexCell = s_parentHexGrid.GetTriangleParent(cell);
                // TODO: There's an easy formula for this, surely?
                var dualCell = s_dual.DualGrid.FindCell(s_parentHexGrid.GetCellCenter(hexCell));
                var d = (Vector3Int)cell - (Vector3Int)dualCell;
                if (d == new Vector3Int(1, 0, 0))
                    return FTTriangleDir.Up;
                if (d == new Vector3Int(1, 1, 0))
                    return FTTriangleDir.Down;
                if (d == new Vector3Int(0, 1, 0))
                    return FTTriangleDir.DownLeft;
                if (d == new Vector3Int(0, 1, 1))
                    return FTTriangleDir.UpRight;
                if (d == new Vector3Int(0, 0, 1))
                    return FTTriangleDir.DownRight;
                if (d == new Vector3Int(1, 0, 1))
                    return FTTriangleDir.UpLeft;
                throw new Exception("Invalid pairing");
            }

            public FTTriangleDir? GetPairing(Cell cell)
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
                var c0 = s_dual.ToBasePair(dualCell, (CellCorner)PTHexCorner.DownRight).Value.baseCell;
                var c1 = s_dual.ToBasePair(dualCell, (CellCorner)PTHexCorner.UpRight).Value.baseCell;
                var c2 = s_dual.ToBasePair(dualCell, (CellCorner)PTHexCorner.Up).Value.baseCell;
                var c3 = s_dual.ToBasePair(dualCell, (CellCorner)PTHexCorner.UpLeft).Value.baseCell;
                var c4 = s_dual.ToBasePair(dualCell, (CellCorner)PTHexCorner.DownLeft).Value.baseCell;
                var c5 = s_dual.ToBasePair(dualCell, (CellCorner)PTHexCorner.Down).Value.baseCell;

                var d0 = GetPairing(c0);
                var d1 = GetPairing(c1);
                var d2 = GetPairing(c2);
                var d3 = GetPairing(c3);
                var d4 = GetPairing(c4);
                var d5 = GetPairing(c5);

                if (d0 == FTTriangleDir.Up && d1 == FTTriangleDir.Down &&
                    d2 == FTTriangleDir.DownLeft && d3 == FTTriangleDir.UpRight &&
                    d4 == FTTriangleDir.DownRight && d5 == FTTriangleDir.UpLeft)
                {
                    Pairings[c1] = FTTriangleDir.UpLeft;
                    Pairings[c2] = FTTriangleDir.DownRight;
                    Pairings[c3] = FTTriangleDir.Down;
                    Pairings[c4] = FTTriangleDir.Up;
                    Pairings[c5] = FTTriangleDir.UpRight;
                    Pairings[c0] = FTTriangleDir.DownLeft;
                    return true;
                }
                if (d1 == FTTriangleDir.UpLeft && d2 == FTTriangleDir.DownRight &&
                    d3 == FTTriangleDir.Down && d4 == FTTriangleDir.Up &&
                    d5 == FTTriangleDir.UpRight && d0 == FTTriangleDir.DownLeft)
                {
                    Pairings[c0] = FTTriangleDir.Up;
                    Pairings[c1] = FTTriangleDir.Down;
                    Pairings[c2] = FTTriangleDir.DownLeft;
                    Pairings[c3] = FTTriangleDir.UpRight;
                    Pairings[c4] = FTTriangleDir.DownRight;
                    Pairings[c5] = FTTriangleDir.UpLeft;
                    return true;
                }

                return false;
            }

            public static HexBound Shrink(HexBound bound)
            {
                return new HexBound(bound.Min + Vector3Int.one, bound.Mex - Vector3Int.one);
            }
            public static TriangleBound Shrink(TriangleBound bound)
            {
                return new TriangleBound(bound.Min + Vector3Int.one, bound.Mex - Vector3Int.one);
            }

            public void RandomRotate(TriangleBound bound, float ratio = 10)
            {
                bound = Shrink(bound);

                var t = new TriangleGrid(1f, TriangleOrientation.FlatTopped, bound);
                var dual = t.GetDual();
                var duals = dual.DualGrid.GetCells().ToList();

                // Randomly rotate the rhombuses until the pairs are good and shuffled
                var r = new System.Random();
                for (var i = 0; i < duals.Count * ratio; i++)
                {
                    var d = duals[(Int32)(r.NextDouble() * duals.Count)];
                    RotateAt(d);
                }
            }

            public MeshData GetMeshData(TriangleBound bound, bool handleOverlap)
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
                    s_triangleGrid.TryMove(cell, (CellDir)dir, out var otherCell, out var inverseDir, out var _);
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
                        (CellDir)(((int)dir) / 2),
                        otherCell,
                        (CellDir)(((int)inverseDir) / 2)
                        ));
                    visited.Add(cell);
                    visited.Add(otherCell);
                }

                // Build the base meshgrid
                var t = s_triangleGrid.Masked(visited);
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

            public ChunkedLayer(Func<Cell, FTTriangleDir> defaultPairing, int chunkSize, Vector3Int chunkOffset)
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
                var z = Mathf.CeilToInt(cell.z * 1f / ChunkSize);
                // As HexGrid.GetTriangleParent.
                return new Cell(
                    Mathf.RoundToInt((x - z) / 3f),
                    Mathf.RoundToInt((y - x) / 3f),
                    Mathf.RoundToInt((z - y) / 3f)
                );
            }
            public TriangleBound GetChunkBound(Cell chunk)
            {
                // Based on HexGrid.GetChildTriangles
                // Should be an ivnerse of GetChunk
                var a = chunk.x - chunk.y;
                var b = chunk.y - chunk.z;
                var c = chunk.z - chunk.x;
                var v = new Vector3Int(a, b, c);
                var bound = new TriangleBound(v * ChunkSize + Vector3Int.one * (-ChunkSize + 1) + ChunkOffset, v * ChunkSize + Vector3Int.one * (ChunkSize + 1) + ChunkOffset);
                return bound;
            }
            public FTTriangleDir GetPairing(Cell cell)
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

        public static IGrid MakeFinite(TriangleBound bound)
        {
            // Setup
            var layer = new Layer(Layer.RhombillePairing, bound);

            layer.RandomRotate(bound, 10);
            var meshData2 = layer.GetMeshData(bound, false);

            return new MeshGrid(meshData2);
        }

        public static IGrid MakeInfinite()
        {
            var chunkSize = 4;
            var layer1 = new ChunkedLayer(Layer.RhombillePairing, chunkSize, new Vector3Int(0, chunkSize, -chunkSize));
            var layer2 = new ChunkedLayer(layer1.GetPairing, chunkSize, new Vector3Int(0, -chunkSize, chunkSize));
            var layer3 = new ChunkedLayer(layer2.GetPairing, chunkSize, new Vector3Int(0, 0, 0));

            MeshData GetMeshData(Cell chunk)
            {
                var bound = layer3.GetChunkBound(chunk);
                var meshData = layer3.Layer.GetMeshData(bound, true);
                return meshData.Weld();
            }
            var chunkGrid = new HexGrid(chunkSize, HexOrientation.FlatTopped);

            // We need a margin the size of one triangle to account for rhombuses
            // that stick out of the chunk
            return new PlanarLazyMeshGrid(GetMeshData, chunkGrid, margin: 1.5f, meshGridOptions: new MeshGridOptions { Tolerance = 0.1f });
        }

}
}
