namespace Sylves
{
    /// <summary>
    /// Defines a mapping that maps the cells of a grid onto themselves, potentially rotated.
    /// This is used to describe reflections and rotations of a grid (as apposed to CellRotation, which
    /// just talks about what you can do to a single cell alone).
    /// 
    /// Use IGrid.TryApplySymmetry to evaluate the map.
    /// 
    /// The mapping is *consistent* with the topology of the grid, i.e.
    /// * Let s by any grid symmetry,
    /// * Let a, and b be any cells, with b is a neighbour of a, in direction d.
    /// * Then if ma, ra is the cell and rotation from applying s to a, and likewise mb, rb for apply s to b.
    /// * Then mb is a neighbour of ma, in direciton ra * d.
    /// 
    /// The consistency property means that the mapping are fully specified over an entire connected grid once you know how it applies
    /// to a single cell. All the other cells can be computed via <see cref="DefaultGridImpl.ParallelTransport(IGrid, Cell, Cell, IGrid, Cell, CellRotation, out Cell, out CellRotation)"/>.
    /// 
    /// In practice, symmetries on regular grids can be easily computed using vector maths operations.
    /// </summary>
    public class GridSymmetry
    {
        public CellRotation Rotation { get; set; }
        public Cell Src { get; set; }
        public Cell Dest { get; set; }
    }
}
