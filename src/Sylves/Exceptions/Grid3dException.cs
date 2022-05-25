using System;

namespace Sylves
{
    /// <summary>
    /// This exception is thrown when you call a grid method that is only appropriate for grids with 2d cells.
    /// </summary>
    public class Grid3dException : NotSupportedException
    {
        public Grid3dException() : base("This operation is not supported on 3d grids") { }
    }
}
