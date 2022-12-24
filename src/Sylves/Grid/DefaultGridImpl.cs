﻿using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY
using UnityEngine;
#endif

namespace Sylves
{
    /// <summary>
    /// IGrid contains a lot of methods.
    /// This class contains default implementations for several of these methods,
    /// in terms of more fundamental methods of the grid.
    /// These are not extension methods as the grids may implement their own implementations
    /// to which have specific functionality or are more performant.
    /// </summary>
    internal static class DefaultGridImpl
    {
        public static IEnumerable<CellDir> GetCellDirs(IGrid grid, Cell cell)
        {
            return grid.GetCellType(cell).GetCellDirs();
        }

        #region Topology
        public static bool TryMoveByOffset(IGrid grid, Cell startCell, Vector3Int startOffset, Vector3Int destOffset, CellRotation startRotation, out Cell destCell, out CellRotation destRotation)
        {
            // TODO: Do parallel transport
            throw new NotImplementedException();
        }

        public static bool ParallelTransport(IGrid aGrid, Cell aSrcCell, Cell aDestCell, IGrid bGrid, Cell bSrcCell, CellRotation startRotation, out Cell bDestCell, out CellRotation destRotation)
        {
            bDestCell = bSrcCell;
            destRotation = startRotation;

            var path = aGrid.FindBasicPath(aSrcCell, aDestCell);
            if(path == null)
            {
                return false;
            }
            // Fast track checking cell types in a simple case
            var checkCellTypes = !aGrid.IsSingleCellType || !bGrid.IsSingleCellType;
            ICellType cellType = null;
            if(!checkCellTypes && (cellType = aGrid.GetCellTypes().First()) != bGrid.GetCellTypes().First())
            {
                return false;
            }
            // For each step of the path, recreate the same step in the right grid.
            foreach (var (aCell, aDir) in path)
            {
                // Check both a/b are on compatible cell types
                if(checkCellTypes && (cellType = aGrid.GetCellType(aCell)) != bGrid.GetCellType(bDestCell))
                {
                    return false;
                }
                // Move in a grid (to get inversedir/connection)
                if (!aGrid.TryMove(aCell, aDir, out _, out var aInverseDir, out var aConnection))
                {
                    return false;
                }

                // Move in b grid
                cellType.Rotate(aDir, destRotation, out var bDir, out var middleConnection);
                if(!bGrid.TryMove(bDestCell, bDir, out bDestCell, out var bInverseDir, out var bConnection))
                {
                    return false;
                }

                // Overall connection from next src to next dest
                var connection = bConnection * middleConnection * aConnection.GetInverse();
                var nextCellType = bGrid.GetCellType(bDestCell);// TODO: Avoid calling GetCellType so frequently?

                if(!nextCellType.TryGetRotation(aInverseDir, bInverseDir, connection, out destRotation))
                {
                    return false;
                }
            }
            return true;
        }


        public static IEnumerable<(Cell, CellDir)> FindBasicPath(IGrid grid, Cell startCell, Cell destCell)
        {
            // TODO: Do Dijkstra's algorithm
            throw new NotImplementedException();
        }

        #endregion


        // Default impl supports no bounds,
        // just returns null representing bounds that covers the whole grid.
        #region Bounds
        public static IBound GetBound(IGrid grid)
        {
            return null;
        }
        public static IBound GetBound(IGrid grid, IEnumerable<Cell> cells)
        {
            return null;
        }

        public static IGrid BoundBy(IGrid grid, IBound bound)
        {
            return grid;
        }

        public static IBound IntersectBounds(IGrid grid, IBound bound, IBound other)
        {
            return null;
        }
        public static IBound UnionBounds(IGrid grid, IBound bound, IBound other)
        {
            return null;
        }
        public static IEnumerable<Cell> GetCellsInBounds(IGrid grid, IBound bound)
        {
            return grid.GetCells();
        }
        public static bool IsCellInBound(IGrid grid, Cell cell, IBound bound)
        {
            return true;
        }
        #endregion

        #region Shape

        public static MeshData GetMeshData(IGrid grid, Cell cell)
        {
            var vertices = new List<Vector3>();
            foreach(var (v1, v2, v3, _) in grid.GetTriangleMesh(cell))
            {
                vertices.Add(v1);
                vertices.Add(v2);
                vertices.Add(v3);
            }
            return new MeshData
            {
                vertices = vertices.ToArray(),
                indices = new[] { Enumerable.Range(0, vertices.Count).ToArray() },
                topologies = new[] { MeshTopology.Triangles },
            };
        }

        #endregion

        #region Symmetry
        public static GridSymmetry FindGridSymmetry(IGrid grid, ISet<Cell> src, ISet<Cell> dest, Cell srcCell, CellRotation cellRotation)
        {
            // Technically, we could implement via exhaustive search. Is that wanted.
            return null;
        }

        public static bool TryApplySymmetry(IGrid grid, GridSymmetry s, IBound srcBound, out IBound destBound)
        {
            var mappedCells = grid.GetCellsInBounds(srcBound)
                .Select(c => grid.TryApplySymmetry(s, c, out var dest, out var _) ? dest : (Cell?)null)
                .OfType<Cell>();
            destBound = grid.GetBound(mappedCells);
            return true;
        }

        public static bool TryApplySymmetry(IGrid grid, GridSymmetry s, Cell src, out Cell dest, out CellRotation r)
        {
            return ParallelTransport(grid, s.Src, src, grid, s.Dest, s.Rotation, out dest, out r);
        }
        #endregion
    }
}
