using System;
using System.Collections.Generic;
using System.Text;

namespace Sylves
{
    public class SquareCellType : ICellType
    {
        private static readonly SquareCellType instance = new SquareCellType();

        public static SquareCellType Instance => instance;

        private SquareCellType(){}

        public IEnumerable<CellDir> GetCellDirs()
        {
            throw new NotImplementedException();
        }
    }
}
