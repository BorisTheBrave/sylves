#if UNITY
using UnityEngine;
#endif


using System.Collections.Generic;
using System.Linq;

namespace Sylves
{
    /// <summary>
    /// Represents a 3d grid, where each cell is an extrusion of a face along the normals, offset to a given height.
    /// </summary>
    public class MeshPrismGrid : MeshGrid
    {

        private readonly MeshPrismGridOptions meshPrismOptions;

        public MeshPrismGrid(MeshData meshData, MeshPrismGridOptions meshPrismOptions) :
            base(meshData, MeshGridBuilder.Build(meshData, meshPrismOptions), false)
        {
            this.meshPrismOptions = meshPrismOptions;
        }

        #region Query
        protected override RaycastInfo? RaycastCell(Cell cell, Vector3 rayOrigin, Vector3 direction)
        {
            var meshCellData = CellData[cell] as MeshCellData;
            if (meshCellData.CellType == CubeCellType.Instance)
            {
                if (meshCellData.PrismInfo.BackDir != (CellDir)CubeDir.Back)
                {
                    throw new System.NotImplementedException("UseXZPlane not supported");
                }

                GetCubeCellVertices(cell, out Vector3 v1, out Vector3 v2, out Vector3 v3, out Vector3 v4, out Vector3 v5, out Vector3 v6, out Vector3 v7, out Vector3 v8);
                var hit = MeshRaycast.RaycastCube(rayOrigin, direction, v1, v2, v3, v4, v5, v6, v7, v8);
                if (hit != null)
                {
                    var hit2 = hit.Value;
                    hit2.cell = cell;
                    return hit2;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                throw new System.NotImplementedException("Raycasts vs non-cube cell types not supported yet");
            }
        }

        protected override bool IsPointInCell(Vector3 position, Cell cell)
        {
            var c = 0;
            foreach(var (v0, v1, v2, dir) in GetCellTriangleMesh(cell))
            {
                System.Console.WriteLine($"{cell} {v0} {v1} {v2} {dir}");
                if(MeshRaycast.RaycastTri(position, Vector3.right, v0, v1, v2, out var _, out var _, out var side))
                {
                    System.Console.WriteLine($"hit {side}");
                    c += side ? 1 : -1;
                }
            }
            return c != 0;
        }

        #endregion

        #region Shape

        // Follows mesh conventions for vertex order (i.e. v1-v4 are the face vertices and v5-v8 are repeats at a different normal offset).
        private void GetCubeCellVertices(Cell cell, out Vector3 v1, out Vector3 v2, out Vector3 v3, out Vector3 v4, out Vector3 v5, out Vector3 v6, out Vector3 v7, out Vector3 v8)
        {
            // TODO: Share this code with QuadInterpolation.InterpolatePosition?
            var (face, submesh, layer) = (cell.x, cell.y, cell.z);

            var meshOffset1 = meshPrismOptions.LayerHeight * layer + meshPrismOptions.LayerOffset - meshPrismOptions.LayerHeight / 2;
            var meshOffset2 = meshPrismOptions.LayerHeight * layer + meshPrismOptions.LayerOffset + meshPrismOptions.LayerHeight / 2;
            QuadInterpolation.GetCorners(meshData, submesh, face, meshPrismOptions.InvertWinding, meshOffset1, meshOffset2, out v1, out v2, out v3, out v4, out v5, out v6, out v7, out v8);
        }

        private static readonly ILookup<CellDir, int> s_faces = new[]
        {
                ((CellDir)(CubeDir.Left), 0),
                ((CellDir)(CubeDir.Right), 1),
                ((CellDir)(CubeDir.Up), 2),
                ((CellDir)(CubeDir.Down), 3),
                ((CellDir)(CubeDir.Forward), 4),
                ((CellDir)(CubeDir.Back), 5),
            }.ToLookup(x => x.Item1, x => x.Item2);

        private static readonly TRS s_trsIdentity = new TRS();

        public IEnumerable<(Vector3, Vector3, Vector3, CellDir)> GetCellTriangleMesh(Cell cell)
        {
            var meshCellData = CellData[cell] as MeshCellData;
            var face = meshCellData.Face;
            var prismInfo = meshCellData.PrismInfo;
            var vertices = meshData.vertices;
            var normals = meshData.normals;
            var (faceIndex, submesh, layer) = (cell.x, cell.y, cell.z);


            var meshOffset1 = meshPrismOptions.LayerHeight * layer + meshPrismOptions.LayerOffset - meshPrismOptions.LayerHeight / 2;
            var meshOffset2 = meshPrismOptions.LayerHeight * layer + meshPrismOptions.LayerOffset + meshPrismOptions.LayerHeight / 2;

            // Explore all the square sides
            for (var i = 0; i < face.Length; i++)
            {
                var v1 = vertices[face[i]];
                var v2 = vertices[face[(i + 1) % face.Length]];
                var n1 = normals[face[i]];
                var n2 = normals[face[(i + 1) % face.Length]];
                var baseCellDir = MeshGridBuilder.EdgeIndexToCellDir(i, face.Count, meshPrismOptions.DoubleOddFaces);
                var cellDir = prismInfo.BaseToPrism(baseCellDir);
                yield return (v1 + n1 * meshOffset1, v1 + n1 * meshOffset2, v2 + n2 * meshOffset2, cellDir);
                yield return (v1 + n1 * meshOffset1, v2 + n2 * meshOffset2, v2 + n2 * meshOffset1, cellDir);
            }
            // Currently does fan detection
            // Doesn't work for convex faces
            {
                var v0 = vertices[face[0]];
                var n0 = normals[face[0]];
                var v1 = vertices[face[face.Count - 1]];
                var n1 = normals[face[face.Count - 1]];
                for (var i = 1; i < face.Count; i++)
                {
                    var v2 = vertices[face[i]];
                    var n2 = normals[face[i]];
                    yield return (v0 + n0 * meshOffset2, v1 + n1 * meshOffset2, v2 + n2 * meshOffset2, prismInfo.ForwardDir);
                    yield return (v0 + n0 * meshOffset1, v2 + n2 * meshOffset1, v1 + n1 * meshOffset1, prismInfo.BackDir);
                    v1 = v2;
                    n1 = n2;
                }
            }
        }

        public void GetCellMesh(Cell cell, out MeshData meshData, out TRS trs, out ILookup<CellDir, int> faces)
        {
            var meshCellData = CellData[cell] as MeshCellData;
            var cellType = meshCellData.CellType;
            if (cellType is CubeCellType)
            {
                if(meshCellData.PrismInfo.BackDir != (CellDir)CubeDir.Back)
                {
                    // TODO: This would be easy to support, just swap out s_faces.
                    throw new System.NotImplementedException("UseXZPlane not supported");
                }

                GetCubeCellVertices(cell, out var v1, out var v2, out var v3, out var v4, out var v5, out var v6, out var v7, out var v8);
                var vertices = new[] { v1, v2, v3, v4, v5, v6, v7, v8 };

                // z-forward convetion (see TestMeshes.Cube)
                var indices = new[]
                {
                    7, 6, 2, 3, // Left
                    0, 1, 5, 4, // Right
                    2, 6, 5, 1, // Up
                    0, 4, 7, 3, // Down
                    4, 5, 6, 7, // Forward
                    3, 2, 1, 0, // Back
                };

                meshData = new MeshData();
                meshData.vertices = vertices;
                meshData.indices = new[] { indices };
                meshData.topologies = new[] { MeshTopology.Quads };
                meshData.subMeshCount = 1;

                faces = s_faces;
                trs = s_trsIdentity;
            }
            else
            {
                throw new System.NotImplementedException($"Unimplemented celltype {cellType}");
            }
        }
        #endregion
    }
}
