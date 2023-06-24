using System;
using System.Collections.Generic;
using UnityEngine;


namespace Sylves
{
    /// <summary>
    /// An ICellType gives summarized info about a single cell in isolation of the grid it comes from.
    /// ICellType allows you to enumerate the edges/faces/corners of a cell, and work with cell symmetries (called CellRotation).
    /// 
    /// ICellType's are always singletons, e.g. SquareCellType.Instance is used for all square cells.
    /// 
    /// Note that cells can share an cell type, even if they are different shapes. Thus any methods that refer to positions, such as GetMatrix,
    /// don't refer to the specific cell in the grid, but the "canonical" cell. 
    /// You must use IGrid methods like GetPolygon or GetDeformation to get the shape of a specific cell.
    /// 
    /// The canonical cell is usually a regular polygon or polyhedron of unit size centered on the origin. See the docs for more details
    /// </summary>
    public interface ICellType
    {
        // Directions

        /// <summary>
        /// Gets all the CellDir used by this cell type.
        /// </summary>
        IEnumerable<CellDir> GetCellDirs();


        /// <summary>
        /// Returns the dir pointing in the opposite direction, if it exists.
        /// </summary>
        CellDir? Invert(CellDir dir);

        // Corners

        /// <summary>
        /// Gets all the CellCorner used by this cell type.
        /// </summary>
        /// <returns></returns>
        IEnumerable<CellCorner> GetCellCorners();

        // Rotations

        /// <summary>
        /// Gets all possible rotations of this cell type.
        /// </summary>
        IList<CellRotation> GetRotations(bool includeReflections = false);

        /// <summary>
        /// Composes two rotations/reflections together, doing b first, then a.
        /// </summary>
        CellRotation Multiply(CellRotation a, CellRotation b);

        /// <summary>
        /// Gets the inverse rotation.
        /// i.e. GetIdentity() == Multiply(a, Invert(a)) == Multiply(Invert(a), a)
        /// </summary>
        CellRotation Invert(CellRotation a);

        /// <summary>
        /// Returns the rotation that leaves everything unchanged
        /// </summary>
        CellRotation GetIdentity();

        /// <summary>
        /// Rotates a dir by the given rotation.
        /// </summary>
        CellDir Rotate(CellDir dir, CellRotation rotation);

        CellCorner Rotate(CellCorner dir, CellRotation rotation);

        void Rotate(CellDir dir, CellRotation rotation, out CellDir resultDir, out Connection connection);

        CellRotation RotateCW { get; }
        CellRotation RotateCCW { get; }

        /// <summary>
        /// Inverse of <see cref="Rotate(CellDir, CellRotation, out CellDir, out Connection)"/>
        /// </summary>
        bool TryGetRotation(CellDir fromDir, CellDir toDir, Connection connection, out CellRotation cellRotation);


        /// <summary>
        /// Returns the matrix a given rotation corresponds to.
        /// 
        /// Note: This method works for the canonical cell shape, but many shapes of cell might reference this cell type.
        /// </summary>
        Matrix4x4 GetMatrix(CellRotation cellRotation);

        /// <summary>
        /// Returns the position of a given corner in the canonical cell shape.
        /// 
        /// Note: This method describes the canonical cell shape, but many shapes of cell might reference this cell type.
        /// </summary>
        Vector3 GetCornerPosition(CellCorner corner);

        string Format(CellRotation rotation);
        string Format(CellDir dir);
        string Format(CellCorner corner);
    }
}
