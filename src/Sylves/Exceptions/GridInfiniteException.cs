using System;
using System.Collections.Generic;
using System.Text;

namespace Sylves
{
    public class GridInfiniteException : Exception
    {
        public GridInfiniteException() : base("This operation is not supported on infinite grids") { }
    }
}
