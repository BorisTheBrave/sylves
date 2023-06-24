using System;
using System.Collections.Generic;
using System.Text;

namespace Sylves
{
    /// <summary>
    /// This exception is thrown when you call a grid method that is only appropriate for grids with a finite amount of cells.
    /// </summary>
    public class GridInfiniteException : NotSupportedException
    {
        public GridInfiniteException() : base("This operation is not supported on infinite grids") { }
    }
}
