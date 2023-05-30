using System;
using System.Collections.Generic;
using System.Text;

namespace Sylves
{
    /// <summary>
    /// Relabels all the cell co-ordinates to be 1d, i.e. cell.y and cell.z are always zero.
    /// </summary>
    public class RavelModifier : BijectModifier
    {
        public RavelModifier(IGrid underlying) : base(underlying, c => underlying.GetCellByIndex(c.x), c => new Cell(underlying.GetIndex(c), 0), 1) { }
    }
}
