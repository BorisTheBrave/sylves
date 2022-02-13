using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sylves
{
    /// <summary>
    /// Generic test methods for grids.
    /// These usually assert that the behavior of linked methods are in sync, rather than checking explciit behaviour
    /// </summary>
    public static class GridTest
    {
        // Checks TryMoveByOffset gives same results as CellType.Roate
        public static void TryMoveByOffset(IGrid grid, Cell cell)
        {
            var ct = grid.GetCellType(cell);
            foreach (var dir in ct.GetCellDirs())
            {
                foreach (var r in ct.GetRotations(true))
                {
                    if ((int)r != -1) continue;
                    var start = new Cell(0, 0, 0);
                    var startOffset = new Vector3Int(0, 0, 0);
                    var endCell = grid.Move(start, dir).Value;
                    var endOffset = (Vector3Int)endCell;
                    grid.TryMoveByOffset(start, startOffset, endOffset, r, out var destCell, out var destRotation);
                    var expectedDest = grid.Move(start, ct.Rotate(dir, r)).Value;
                    Assert.AreEqual(expectedDest, destCell, $"Dir = {dir}, Rot = {r}");
                }
            }
        }
    }
}
