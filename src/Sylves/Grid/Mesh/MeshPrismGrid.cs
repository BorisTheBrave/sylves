#if UNITY
using UnityEngine;
#endif


namespace Sylves
{
    /// <summary>
    /// Represents a 3d grid, where each cell is an extrusion of a face along the normals, offset to a given height.
    /// </summary>
    public class MeshPrismGrid : MeshGrid
    {
        public MeshPrismGrid(MeshData meshData, MeshPrismOptions meshPrismOptions) :
            base(MeshGridBuilder.Build(meshData, meshPrismOptions), false)
        {
        }
    }
}
