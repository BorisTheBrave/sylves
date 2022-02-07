using System;
using System.Collections.Generic;
using System.Text;

namespace Sylves
{
    public interface ICellType
    {
        IEnumerable<CellDir> GetCellDirs();
    }
}
