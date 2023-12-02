using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Sylves
{

    /// <summary>
    /// An infinite planar grid. It is evaluated lazily by splitting the plane into overlapping period rectangles
    /// which then each has a grid associated.
    /// 
    /// This class requires you to compute inter-chunk neighbours yourself, which is often tricky.
    /// You are recommended to use PlanarLazyMeshGrid instead, which handles this automatically.
    /// 
    /// This class is simply a specialization of NestedModifier, applied to a grid of overlapping rectangles.
    /// </summary>
    public abstract class PlanarLazyGrid : NestedModifier
    {
        private Vector2 strideX;
        private Vector2 strideY;
        private Vector2 aabbBottomLeft;
        private Vector2 aabbSize;
        private bool translateMeshData;
        private AabbChunks aabbChunks;

        // Clone constructor. Clones share the same cache!
        protected PlanarLazyGrid(PlanarLazyGrid original, SquareBound bound)
            : base(original, original.ChunkGrid.Unbounded.BoundBy(bound))
        {
            strideX = original.strideX;
            strideY = original.strideY;
            aabbBottomLeft = original.aabbBottomLeft;
            aabbSize = original.aabbSize;
            translateMeshData = original.translateMeshData;
            aabbChunks = original.aabbChunks;
        }

        /// <summary>
        /// Constructs a planar lazy grid that calls getMeshData to fill a chunked plane with a mesh per chunk.
        /// </summary>
        /// <param name="strideX">The step from one chunk to the next.</param>
        /// <param name="strideY">The step from one chunk to the next.</param>
        /// <param name="aabbBottomLeft">The bottom left point of the central chunk. This should bound getMeshData(new Vector2(0, 0))</param>
        /// <param name="aabbSize">The size of each chunk. This should bound getMeshData(new Vector2(0, 0))</param>
        /// <param name="bound">Bounds which chunks are generated.</param>
        /// <param name="cellTypes">What should the response of GetCellType </param>
        /// <param name="cachePolicy">Configures how to store the cahced meshes.</param>
        public PlanarLazyGrid(Vector2 strideX, Vector2 strideY, Vector2 aabbBottomLeft, Vector2 aabbSize, SquareBound bound = null, IEnumerable<ICellType> cellTypes = null, ICachePolicy cachePolicy = null)
        {
            Setup(strideX, strideY, aabbBottomLeft, aabbSize, translateMeshData, bound, cellTypes, cachePolicy);
        }

        // You must call setup if using this constructor.
        protected PlanarLazyGrid()
        {

        }

        protected void Setup(Vector2 strideX, Vector2 strideY, Vector2 aabbBottomLeft, Vector2 aabbSize, bool translateMeshData = false, SquareBound bound = null, IEnumerable<ICellType> cellTypes = null, ICachePolicy cachePolicy = null)
        {
            this.strideX = strideX;
            this.strideY = strideY;
            this.aabbBottomLeft = aabbBottomLeft;
            this.aabbSize = aabbSize;
            this.translateMeshData = translateMeshData;
            aabbChunks = new AabbChunks(strideX, strideY, aabbBottomLeft, aabbSize);
            var chunkGrid = new AabbGrid(aabbChunks, bound);
            base.Setup(chunkGrid, cellTypes, cachePolicy);
        }

        internal bool TranslateMeshData => translateMeshData;

        // Returns the chunks that could have adjacencies to the current chunk
        protected virtual IEnumerable<Cell> GetAdjacentChunks(Cell chunk)
        {
            // By default, any two intersecting chunks could have adjacencies.
            var bound = (SquareBound)ChunkGrid.GetBound();
            return aabbChunks.GetChunkIntersects(new Vector2Int(chunk.x, chunk.y))
                .Where(x => bound == null || bound.Contains(x))
                .Select(x => new Cell(x.x, x.y));
        }

        protected Vector3 ChunkOffset(Cell chunk)
        {
            var chunkOffset2 = strideX * chunk.x + strideY * chunk.y;
            var chunkOffset = new Vector3(chunkOffset2.x, chunkOffset2.y, 0);
            return chunkOffset;
        }

        protected override Vector3 MeshTranslation(Cell chunk) => translateMeshData ? ChunkOffset(chunk) : new Vector3();

        public Vector2 StrideX => strideX;
        public Vector2 StrideY => strideY;
        public Vector2 AabbBottomLeft => aabbBottomLeft;
        public Vector2 AabbSize => aabbSize;

        #region Basics

        #endregion

        #region Relatives

        public override IDualMapping GetDual()
        {
            return new DefaultDualMapping(this, CachePolicy);
        }
        #endregion
    }
}
