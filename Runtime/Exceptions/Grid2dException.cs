using System;

namespace Sylves
{
    /// <summary>
    /// This exception is thrown when you call a grid method that is only appropriate for grids with 3d cells.
    /// </summary>
    public class Grid2dException : NotSupportedException
    {
        public Grid2dException() : base("This operation is not supported on 2d grids") { }
    }
}
