using System.Collections.Generic;
#if UNITY
using UnityEngine;
#endif

using static Sylves.VectorUtils;

namespace Sylves
{
    internal static class MeshGridBuilder
    {
        public static DataDrivenData Build(MeshData meshData)
        {
            var data = new DataDrivenData
            {
                Cells = new Dictionary<Cell, DataDrivenCellData>(),
                Moves = new Dictionary<(Cell, CellDir), (Cell, CellDir, Connection)>(),
            };
            BuildMoves(meshData, data.Moves);
            BuildCellData(meshData, data.Cells);
            return data;
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
            var z = (deformation.DeformPoint(p + Vector3.forward * e) - center) / e;
            var y = Vector3.Cross(x, z).normalized;
            var m = ToMatrix(x, y, z, new Vector4(center.x, center.y, center.z, 1));

            return new TRS(m);
        }


        // Loop over every edge of every face, match them up pairwise, and marshal into moves array
        private static void BuildMoves(MeshData data, IDictionary<(Cell, CellDir), (Cell, CellDir, Connection)> moves)
        {
            var vertices = data.vertices;
            var unmatchedEdges = new Dictionary<(Vector3, Vector3), (int, int, int)>();
            void AddEdge(int face, int submesh, int edge, int index1, int index2)
            {
                var v1 = vertices[index1];
                var v2 = vertices[index2];
                // Attempt to match an existing edge
                if (unmatchedEdges.TryGetValue((v2, v1), out var match))
                {
                    // Edges match, add moves in both directions
                    var (face2, submesh2, edge2) = match;
                    var cell = new Cell(face, submesh);
                    var cell2 = new Cell(face2, submesh2);
                    moves.Add((cell, (CellDir)edge), (cell2, (CellDir)edge2, new Connection()));
                    moves.Add((cell2, (CellDir)edge2), (cell, (CellDir)edge, new Connection()));
                    unmatchedEdges.Remove((v2, v1));
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
                }
                else
                {
                    // New edge, add to unmatched edges
                    unmatchedEdges.Add((v1, v2), (face, submesh, edge));
                }

            }

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
                            AddEdge(face, submesh, indexCount - 1, prev, index);
                            prev = index;
                        }
                        indexCount++;
                    }
                    AddEdge(face, submesh, indexCount - 1, prev, first);
                    face++;
                }
            }
        }
    }
}
