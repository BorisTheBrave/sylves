using System;
using System.Collections.Generic;
#if UNITY
using UnityEngine;
#endif

namespace Sylves
{
    /// <summary>
    /// Contains all the methods for querying a grid.
    /// A grid is a collection of cells, with each cell having various data associated with it
    /// such as neighbours, position in space.
    /// For more details see the basic concepts in the docs.
    /// </summary>
    public interface IGrid
    {
        #region Basics

        /// <summary>
        /// True if this grid uses 2d cell types 
        /// </summary>
        bool Is2D { get; }

        /// <summary>
        /// True if this grid uses 3d cell types 
        /// </summary>
        bool Is3D { get; }

        /// <summary>
        /// True if this grid uses 2d cell types, and all cells fit in the XY plane. 
        /// </summary>
        bool IsPlanar { get; }

        /// <summary>
        /// True for grids that are some fixed pattern repeated over and over. 
        /// </summary>
        bool IsRepeating { get; }

        /// <summary>
        /// True if tile connections never set Mirror to true. 
        /// </summary>
        bool IsOrientable { get; }

        /// <summary>
        /// True if there is a finite amout of cells in the grid.
        /// </summary>
        bool IsFinite { get; }

        /// <summary>
        /// True if GetCellTypes always returns a single value.
        /// </summary>
        bool IsSingleCellType { get; }

        // TODO: Supports MeshDistortion
        // TODO: Similar to Orientable for cell rotation

        /// <summary>
        /// Returns the full list of cell types that can be returned by <see cref="GetCellType(Cell)"/>
        /// </summary>
        IEnumerable<ICellType> GetCellTypes();

        #endregion

        #region Relatives

        IGrid Unbounded { get; }

        IGrid Unwrapped { get; }

        #endregion

        #region Cell info

        /// <summary>
        /// Gets a full list of cells in bounds.
        /// </summary>
        IEnumerable<Cell> GetCells();

        /// <summary>
        /// Returns the cell type associated with a given cell
        /// </summary>
        ICellType GetCellType(Cell cell);

        bool IsCellInGrid(Cell cell);
        #endregion

        #region Topology

        /// <summary>
        /// Attempts to move from a cell in a given direction, and returns information about the move if successful.
        /// </summary>
        /// <param name="cell">The cell to move from</param>
        /// <param name="dir">The direction to move in</param>
        /// <param name="dest">The cell moved to</param>
        /// <param name="inverseDir">The direction leading back from dest to cell.</param>
        /// <param name="connection">A descriptor of how cell-local space relates between cell and dest.</param>
        bool TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection);

        // TODO: Give the offset API some thought. Should probably be ParallelTransport?
        /// <summary>
        /// Maps between cell offsets and cells in the grid.
        /// This is normally done via <see cref="IGrid.FindBasicPath(Sylves.Cell,Sylves.Cell)"/>, but regular grids
        /// often have a more efficient implementation.
        /// </summary>
        bool TryMoveByOffset(Cell startCell, Vector3Int startOffset, Vector3Int destOffset, CellRotation startRotation, out Cell destCell, out CellRotation destRotation);


        /// <summary>
        /// Given a path in aGrid from aSrcCell to aDestCell
        /// follows the same path in the current grid, starting at srcCell, and with the whole path rotated by startRotation.
        /// Reports the final spot that the path ends at, and it's rotation.
        /// Returns false if this cannot be done (typically because an equivalent pathc annot be found in the grid).
        /// 
        /// This method is useful for translating co-ordinates between different grids that are similarish, at least having the same celltypes.
        /// For example, suppose
        ///   aGrid = new SquareGrid(1);
        ///   aSrcCell = new Cell(0, 0)
        ///   aDestCell = new Cell(5, 0)
        ///   srcCell = new Cell(100, 100)
        ///   startRotation = identity
        /// 
        /// Then the path in aGrid is a straight line moving 5 units to the right,
        /// and this method would attempt to move 5 units in the straight line that leads right out of (100, 100).
        /// </summary>
        bool ParallelTransport(IGrid aGrid, Cell aSrcCell, Cell aDestCell, Cell srcCell, CellRotation startRotation, out Cell destCell, out CellRotation destRotation);


        /// <summary>
        /// Returns directions we might expect TryMove to work for.
        /// This usually just forwards to <see cref="ICellType.GetCellDirs()"/>
        /// </summary>
        IEnumerable<CellDir> GetCellDirs(Cell cell);

        /// <summary>
        /// Returns a ordered series of cells and directions, starting a startCell,
        /// such that moving in the given direction gives the next cell in the sequence,
        /// and the final cell then moves to destCell.
        /// Returns null if this is not possible.
        /// This method is not indended for path finding as it lacks any customization. It is intended
        /// for algorithms that need to work between any two connected cells, as provides a "proof"
        /// of connectivity.
        /// </summary>
        IEnumerable<(Cell, CellDir)> FindBasicPath(Cell startCell, Cell destCell);

        #endregion

        #region Index
        /// <summary>
        /// Finds a number one larger than the maximum index for an in bounds cell.
        /// </summary>
        int IndexCount { get; }

        /// <summary>
        /// Finds the index associated with a given cell. 
        /// </summary>
        int GetIndex(Cell cell);

        /// <summary>
        /// Finds the cell associated with a given index.
        /// </summary>
        Cell GetCellByIndex(int index);
        #endregion

        #region Bounds
        IBound GetBound();

        IBound GetBound(IEnumerable<Cell> cells);

        IGrid BoundBy(IBound bound);

        IBound IntersectBounds(IBound bound, IBound other);
        IBound UnionBounds(IBound bound, IBound other);
        // TODO: Decide if this should return cells outside of grid.
        IEnumerable<Cell> GetCellsInBounds(IBound bound);
        
        bool IsCellInBound(Cell cell, IBound bound);
        #endregion

        #region Position
        /// <summary>
        /// Returns the center of the cell in local space
        /// </summary>
        Vector3 GetCellCenter(Cell cell);

        /// <summary>
        /// Returns the appropriate transform for the cell.
        /// The translation will always be to GetCellCenter.
        /// Not inclusive of cell rotation, that should be applied first.
        /// </summary>
        TRS GetTRS(Cell cell);

        #endregion

        #region Shape

        Deformation GetDeformation(Cell cell);

        void GetPolygon(Cell cell, out Vector3[] vertices, out Matrix4x4 transform);

        #endregion

        #region Query
        /// <summary>
        /// Finds the cell containg the give position
        /// </summary>
        bool FindCell(Vector3 position, out Cell cell);

        /// <summary>
        /// Returns the cell and rotation corresponding to a given transform matrix.
        /// </summary>
        bool FindCell(
            Matrix4x4 matrix,
            out Cell cell,
            out CellRotation rotation);

        /// <summary>
        /// Gets the set of cells that potentially overlap bounds.
        /// </summary>
        IEnumerable<Cell> GetCellsIntersectsApprox(Vector3 min, Vector3 max);

        /// <summary>
        /// Returns the cells intersecting a ray starting at origin, of length direction.magnitude * maxDistance.
        /// </summary>
        IEnumerable<RaycastInfo> Raycast(Vector3 origin, Vector3 direction, float maxDistance = float.PositiveInfinity);
        #endregion

        #region Symmetry

        /// <summary>
        /// Finds a GridSymmetry that:
        /// 1) Maps from the cells of src into dest (in any order / rotation), and
        /// 2) Maps srcCell using cellRotation (to any cell in dest)
        /// 
        /// For simple, regular grids, srcCell is irrelevant, as every cell uses the same cellRotation.
        /// 
        /// Returns null if one cannot be found. Returns an arbitrary pick if there are multiple possibilities.
        /// </summary>
        GridSymmetry FindGridSymmetry(ISet<Cell> src, ISet<Cell> dest, Cell srcCell, CellRotation cellRotation);

        /// <summary>
        /// Finds a bound that would contain all the cells of srcBound after applying the grid symmetry to them.
        /// </summary>
        bool TryApplySymmetry(GridSymmetry s, IBound srcBound, out IBound destBound);

        /// <summary>
        /// Applies the mapping of s to cell, and also returns the rotation
        /// 
        /// For more details, see <see cref="GridSymmetry"/>.
        /// 
        /// For simple, regular grids, the output rotation is copied directly from <see cref="GridSymmetry.Rotation"/>.
        /// </summary>
        bool TryApplySymmetry(GridSymmetry s, Cell src, out Cell dest, out CellRotation r);
        #endregion

    }
}
