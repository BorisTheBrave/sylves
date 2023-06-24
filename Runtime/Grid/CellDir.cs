namespace Sylves
{
    /// <summary>
    /// Represents a particular edge (2d) or face (3d) of a generic cell.
    /// The enum is empty - to work with directions, you need to either:
    /// * Use the methods on <see cref="ICellType"/>.
    /// * Cast to the enum specific to a given cell type, e.g. <see cref="CubeDir"/>.
    /// </summary>
    public enum CellDir
    {

    }
}
