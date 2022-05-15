#if UNITY
using UnityEngine;
#endif


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
            base(MeshGridBuilder.Build(meshData, meshPrismOptions), false)
        {
            this.meshPrismOptions = meshPrismOptions;
        }

        #region Shape

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
                ((CellDir)(CubeDir.Down), 0),
                ((CellDir)(CubeDir.Up), 1),
                ((CellDir)(CubeDir.Left), 2),
                ((CellDir)(CubeDir.Right), 3),
                ((CellDir)(CubeDir.Back), 4),
                ((CellDir)(CubeDir.Forward), 5),
            }.ToLookup(x => x.Item1, x => x.Item2);

        private static readonly TRS s_trsIdentity = new TRS();

        public void GetMesh(Cell cell, out MeshData meshData, out TRS trs, out ILookup<CellDir, int> faces)
        {
            GetCubeCellVertices(cell, out var v1, out var v2, out var v3, out var v4, out var v5, out var v6, out var v7, out var v8);
            var vertices = new[] { v1, v2, v3, v4, v5, v6, v7, v8 };
            var indices = new[]
            {
                0,1,2,3,
                6,5,4,7,
                4,5,1,0,
                6,7,3,2,
                0,3,7,4,
                5,6,2,1,
            };

            meshData = new MeshData();
            meshData.vertices = vertices;
            meshData.indices = new[] { indices };
            meshData.topologies = new[] { MeshTopology.Quads };

            faces = s_faces;
            trs = s_trsIdentity;
        }
        #endregion
    }
}
