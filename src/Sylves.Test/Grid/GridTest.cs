using NUnit.Framework;
#if UNITY
using UnityEngine;
#endif


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
                    var start = new Cell(0, 0, 0);
                    var startOffset = new Vector3Int(0, 0, 0);
                    var endCell = grid.Move(start, dir).Value;
                    var endOffset = (Vector3Int)endCell;
                    var success = grid.TryMoveByOffset(start, startOffset, endOffset, r, out var destCell, out var destRotation);
                    Assert.IsTrue(success, $"Dir = {dir}, Rot = {r}");
                    var expectedDest = grid.Move(start, ct.Rotate(dir, r)).Value;
                    Assert.AreEqual(expectedDest, destCell, $"Dir = {dir}, Rot = {r}");
                }
            }
        }

        // Checks we can round trip from cell to center and back.
        public static void FindCell(IGrid grid, Cell cell)
        {
            // Check basic find cell
            var success = grid.FindCell(grid.GetCellCenter(cell), out var cell2);
            Assert.IsTrue(success);
            Assert.AreEqual(cell, cell2);

            // Check FindCell with rotations
            var ct = grid.GetCellType(cell);
            foreach (var r in ct.GetRotations(true))
            {
                var m = grid.GetTRS(cell).ToMatrix() * ct.GetMatrix(r);
                success = grid.FindCell(m, out cell2, out var r2);
                Assert.IsTrue(success, $"{cell} {r}");
                Assert.AreEqual(cell, cell2, $"{cell} {r}");
                Assert.AreEqual(r, r2, $"{cell} {r}");
            }
        }

        public static void FindBasicPath(IGrid grid, Cell startCell, Cell endCell)
        {
            var path = grid.FindBasicPath(startCell, endCell);
            var cell = startCell;
            foreach(var (c2, dir) in path)
            {
                Assert.AreEqual(cell, c2);
                var success = grid.TryMove(cell, dir, out cell, out var _, out var _);
            }
            Assert.AreEqual(endCell, cell);
        }

        public static void TryGetRotation(ICellType cellType)
        {
            foreach(var dir in cellType.GetCellDirs())
            {
                foreach(var rotation in cellType.GetRotations(true))
                {
                    cellType.Rotate(dir, rotation, out var dir2, out var connection);
                    var success = cellType.TryGetRotation(dir, dir2, connection, out var rotation2);
                    Assert.IsTrue(success);
                    Assert.AreEqual(rotation, rotation2, $"{dir} {rotation}");
                }
            }
        }
    }
}
