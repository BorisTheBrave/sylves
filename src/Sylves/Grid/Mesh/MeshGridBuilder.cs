using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY
using UnityEngine;
#endif

using static Sylves.VectorUtils;

namespace Sylves
{
    /// <summary>
    /// Utility class represented a bundle of half edges extracted from a mesh.
    /// Only unpaired half-edges are stored - any paired off edges are immediately recorded in the moves array.
    /// </summary>
    internal class EdgeStore
    {
        // the face, submesh and edge ids, stored by start/end points of the edge.
        private Dictionary<(Vector3, Vector3), (int, int, int)> unmatchedEdges;

        public EdgeStore()
        {
            unmatchedEdges = new Dictionary<(Vector3, Vector3), (int, int, int)>();
        }

        private EdgeStore(Dictionary<(Vector3, Vector3), (int, int, int)> unmatchedEdges)
        {
            this.unmatchedEdges = unmatchedEdges;
        }

        public IEnumerable<(Vector3 v1, Vector3 v2, int face, int submesh, int edge)> UnmatchedEdges
        {
            get
            {
                return unmatchedEdges.Select(x => (x.Key.Item1, x.Key.Item2, x.Value.Item1, x.Value.Item2, x.Value.Item3));
            }
        }

        // Attempts to pair the new edge with the unmapped edges.
        // On success, adds it to moves and returns true.
        public bool MatchEdge(Vector3 v1, Vector3 v2, int face, int submesh, int edge, IDictionary<(Cell, CellDir), (Cell, CellDir, Connection)> moves)
        {
            if (unmatchedEdges.TryGetValue((v2, v1), out var match))
            {
                // Edges match, add moves in both directions
                var (face2, submesh2, edge2) = match;
                var cell = new Cell(face, submesh);
                var cell2 = new Cell(face2, submesh2);
                moves.Add((cell, (CellDir)edge), (cell2, (CellDir)edge2, new Connection()));
                moves.Add((cell2, (CellDir)edge2), (cell, (CellDir)edge, new Connection()));
                unmatchedEdges.Remove((v2, v1));
                return true;
            }
            else if (unmatchedEdges.TryGetValue((v1, v2), out match))
            {
                // Same as above, but with a mirrored connection
                var (face2, submesh2, edge2) = match;
                var cell = new Cell(face, submesh);
                var cell2 = new Cell(face2, submesh2);
                moves.Add((cell, (CellDir)edge), (cell2, (CellDir)edge2, new Connection { Mirror = true }));
                moves.Add((cell2, (CellDir)edge2), (cell, (CellDir)edge, new Connection { Mirror = true }));
                unmatchedEdges.Remove((v1, v2));
                return true;
            }
            return false;
        }


        // MatchEdge, and if it fails, adds the edge to unmatchedEdges
        public void AddEdge(Vector3 v1, Vector3 v2, int face, int submesh, int edge, IDictionary<(Cell, CellDir), (Cell, CellDir, Connection)> moves)
        {
            if(!MatchEdge(v1, v2, face, submesh, edge, moves))
            {
                unmatchedEdges.Add((v1, v2), (face, submesh, edge));
            }
        }

        public EdgeStore Clone()
        {
            return new EdgeStore(unmatchedEdges.ToDictionary(x => x.Key, x => x.Value));
        }
    }


    /// <summary>
    /// Class contains utiliites for analysing a MeshData, specifically
    /// for use with MeshGrid.
    /// </summary>
    internal static class MeshGridBuilder
    {
        public static DataDrivenData Build(MeshData meshData)
        {
            return Build(meshData, out var _);
        }

        public static DataDrivenData Build(MeshData meshData, out EdgeStore edgeStore)
        {
            var data = new DataDrivenData
            {
                Cells = new Dictionary<Cell, DataDrivenCellData>(),
                Moves = new Dictionary<(Cell, CellDir), (Cell, CellDir, Connection)>(),
            };
            edgeStore = BuildMoves(meshData, data.Moves);
            BuildCellData(meshData, data.Cells);
            return data;
        }

        public static DataDrivenData Build(MeshData meshData, MeshPrismOptions meshPrismOptions)
        {
            var data = new DataDrivenData
            {
                Cells = new Dictionary<Cell, DataDrivenCellData>(),
                Moves = new Dictionary<(Cell, CellDir), (Cell, CellDir, Connection)>(),
            };
            BuildCellData(meshData, meshPrismOptions, data.Cells);
            BuildMoves(meshData, meshPrismOptions, data.Cells, data.Moves);
            return data;
        }

        private static void BuildCellData(MeshData data, MeshPrismOptions meshPrismOptions, IDictionary<Cell, DataDrivenCellData> cellData)
        {
            for (var layer = meshPrismOptions.MinLayer; layer < meshPrismOptions.MaxLayer; layer++)
            {
                for (var submesh = 0; submesh < data.subMeshCount; submesh++)
                {
                    var face = 0;
                    foreach (var faceIndices in MeshUtils.GetFaces(data, submesh))
                    {
                        var cell = new Cell(face, submesh, layer);
                        var deformation = MeshUtils.GetDeformation(data, meshPrismOptions.LayerHeight, meshPrismOptions.LayerOffset, meshPrismOptions.SmoothNormals, face, layer, submesh);
                        var count = faceIndices.Count;
                        var cellType = count == 4 ? CubeCellType.Instance : throw new NotImplementedException();
                        var trs = GetTRS(deformation, Vector3.zero);
                        cellData[cell] = new DataDrivenCellData
                        {
                            CellType = cellType,
                            Deformation = deformation,
                            TRS = trs,
                        };
                        face++;
                    }
                }
            }
        }

        private static void BuildCellData(MeshData data, IDictionary<Cell, DataDrivenCellData> cellData)
        {
            for (var submesh = 0; submesh < data.subMeshCount; submesh++)
            {
                var face = 0;
                foreach (var faceIndices in MeshUtils.GetFaces(data, submesh))
                {
                    var cell = new Cell(face, submesh);
                    var deformation = MeshUtils.GetDeformation(data, face, submesh);
                    var count = faceIndices.Count;
                    var cellType = count == 3 ? HexCellType.Get(HexOrientation.PointyTopped) : count == 4 ? SquareCellType.Instance : NGonCellType.Get(count);
                    var trs = GetTRS2d(deformation, Vector3.zero);
                    cellData[cell] = new DataDrivenCellData
                    {
                        CellType = cellType,
                        Deformation = deformation,
                        TRS = trs,
                    };
                    face++;
                }
            }
        }

        private static TRS GetTRS(Deformation deformation, Vector3 p)
        {
            var center = deformation.DeformPoint(p);
            var e = 1e-4f;
            var x = (deformation.DeformPoint(p + Vector3.right * e) - center) / e;
            var y = (deformation.DeformPoint(p + Vector3.up * e) - center) / e;
            var z = (deformation.DeformPoint(p + Vector3.forward * e) - center) / e;
            var m = ToMatrix(x, y, z, new Vector4(center.x, center.y, center.z, 1));

            return new TRS(m);
        }

        private static TRS GetTRS2d(Deformation deformation, Vector3 p)
        {
            var center = deformation.DeformPoint(p);
            var e = 1e-4f;
            var x = (deformation.DeformPoint(p + Vector3.right * e) - center) / e;
            /*
            var z = (deformation.DeformPoint(p + Vector3.forward * e) - center) / e;
            var y = Vector3.Cross(x, z).normalized;
            */
            var y = (deformation.DeformPoint(p + Vector3.up * e) - center) / e;
            var z = Vector3.Cross(x, y).normalized;
            var m = ToMatrix(x, y, z, new Vector4(center.x, center.y, center.z, 1));

            return new TRS(m);
        }

        // Given a single layer of moves,
        // converts it to moves on multiple layer, in a different cell type
        private static void BuildMoves(MeshData data, MeshPrismOptions meshPrismOptions, IDictionary<Cell, DataDrivenCellData> allCells, IDictionary<(Cell, CellDir), (Cell, CellDir, Connection)> moves)
        {
            var layerCellData = new Dictionary<Cell, DataDrivenCellData>();
            var layerMoves = new Dictionary<(Cell, CellDir), (Cell, CellDir, Connection)>();
            BuildCellData(data, layerCellData);
            BuildMoves(data, layerMoves);
            for (var layer = meshPrismOptions.MinLayer; layer <= meshPrismOptions.MaxLayer; layer++)
            {
                foreach (var kv in layerMoves)
                {
                    var fromPrismInfo = PrismInfo.Get(layerCellData[kv.Key.Item1].CellType);
                    var toPrismInfo = PrismInfo.Get(layerCellData[kv.Value.Item1].CellType);

                    var fromCell = new Cell(kv.Key.Item1.x, kv.Key.Item1.y, layer);
                    var toCell = new Cell(kv.Value.Item1.x, kv.Value.Item1.y, layer);
                    moves.Add((fromCell, fromPrismInfo.BaseToPrism(kv.Key.Item2)), (toCell, toPrismInfo.BaseToPrism(kv.Value.Item2), kv.Value.Item3));
                }
            }
            foreach(var kv in allCells)
            {
                var cell = kv.Key;
                var cellType = kv.Value.CellType;
                if (cell.z > meshPrismOptions.MinLayer)
                {
                    if (cellType == CubeCellType.Instance)
                    {
                        moves.Add((cell, (CellDir)CubeDir.Forward), (cell + new Vector3Int(0, 0, 1), (CellDir)CubeDir.Back, new Connection()));
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                if (cell.z < meshPrismOptions.MaxLayer - 1)
                {
                    if (cellType == CubeCellType.Instance)
                    {
                        moves.Add((cell, (CellDir)CubeDir.Back), (cell + new Vector3Int(0, 0, -1), (CellDir)CubeDir.Forward, new Connection()));
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }
        }


        // Loop over every edge of every face, match them up pairwise, and marshal into moves array
        // This relies on the fact that for 2d cell types, the number of the edge corresponds to the CellDir.
        // Returns any unmatched edges
        private static EdgeStore BuildMoves(MeshData data, IDictionary<(Cell, CellDir), (Cell, CellDir, Connection)> moves)
        {
            var vertices = data.vertices;
            var edgeStore = new EdgeStore();

            for (var submesh = 0; submesh < data.subMeshCount; submesh++)
            {
                var face = 0;
                foreach (var faceIndices in MeshUtils.GetFaces(data, submesh))
                {
                    int first = -1;
                    int prev = -1;
                    int indexCount = 0;
                    foreach (var index in faceIndices)
                    {
                        if (first == -1)
                        {
                            first = index;
                            prev = index;
                        }
                        else
                        {
                            edgeStore.AddEdge(vertices[prev], vertices[index], face, submesh, indexCount - 1, moves);
                            prev = index;
                        }
                        indexCount++;
                    }
                    edgeStore.AddEdge(vertices[prev], vertices[first], face, submesh, indexCount - 1, moves);
                    face++;
                }
            }

            return edgeStore;
        }
    }
}
