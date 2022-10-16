using System;
using System.Collections.Generic;
using System.Text;
#if UNITY
using UnityEngine;
#endif

namespace Sylves
{
    public static class GridExtensions
    {
        /// <summary>
        /// Returns the cell that is in the given direction from cell, or null if that move is not possible.
        /// </summary>
        public static Cell? Move(this IGrid grid, Cell cell, CellDir dir)
        {
            if(grid.TryMove(cell, dir, out var dest, out var _, out var _))
            {
                return dest;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns all the cells that you can move to from a given cell.
        /// </summary>
        public static IEnumerable<Cell> GetNeighbours(this IGrid grid, Cell cell)
        {
            foreach(var dir in grid.GetCellDirs(cell))
            {
                if(grid.TryMove(cell, dir, out var dest, out var _, out var _))
                {
                    yield return dest;
                }
            }
        }

        /// <summary>
        /// Applies a linear transformation to each of the cells of the grid.
        /// <see cref="TransformModifier"/>
        /// </summary>
        public static IGrid Transformed(this IGrid grid, Matrix4x4 transform) => new TransformModifier(grid, transform);

        /// <summary>
        /// Filters the grid cells to the given subset.
        /// <see cref="MaskModifier"/>
        /// </summary>
        public static IGrid Masked(this IGrid grid, ISet<Cell> allCells) => new MaskModifier(grid, allCells);

        /// <summary>
        /// Filters the grid cells to the given subset.
        /// <see cref="MaskModifier"/>
        /// </summary>
        public static IGrid Masked(this IGrid grid, Func<Cell, bool> containsFunc, IEnumerable<Cell> allCells = null) => new MaskModifier(grid, containsFunc, allCells);


        /// <summary>
        /// Converts a finite grid to a MeshData.
        /// </summary>
        public static MeshData ToMeshData(this IGrid grid)
        {
            var vertices = new List<Vector3>();
            var indices = new List<int>();
            bool allTris = true;
            bool allQuads = true;
            foreach(var cell in grid.GetCells())
            {
                grid.GetPolygon(cell, out var cellVerticies, out var transform);
                foreach(var v in cellVerticies)
                {
                    indices.Add(vertices.Count);
                    vertices.Add(transform.MultiplyPoint3x4(v));
                }
                indices[indices.Count - 1] = ~indices[indices.Count - 1];
                if (cellVerticies.Length != 3) allTris = false;
                if (cellVerticies.Length != 4) allQuads = false;
            }
            if(allTris || allQuads)
            {
                for(var i=0;i<indices.Count;i++)
                {
                    if (indices[i] < 0)
                        indices[i] = ~indices[i];
                }
            }
            return new MeshData
            {
                vertices = vertices.ToArray(),
                indices = new[] { indices.ToArray() },
                subMeshCount = 1,
                topologies = new MeshTopology[] { allTris ? MeshTopology.Triangles : allQuads ? MeshTopology.Quads : MeshTopology.NGon },
            };
        }

        public static Vector3[] GetPolygon(this IGrid grid, Cell cell)
        {
            grid.GetPolygon(cell, out var vertices, out var transform);
            var r = new Vector3[vertices.Length];
            for(var i=0; i<vertices.Length; i++)
            {
                r[i] = transform.MultiplyPoint3x4(vertices[i]);
            }
            return r;
        }


        public static MeshData GetMeshData(this IGrid grid, Cell cell)
        {
            grid.GetMeshData(cell, out var meshData, out var transform);
            return transform * meshData;
        }

    }
}
