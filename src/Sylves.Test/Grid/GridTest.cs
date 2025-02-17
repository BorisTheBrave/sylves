using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY
using UnityEngine;
#endif


namespace Sylves.Test
{
    /// <summary>
    /// Generic test methods for grids.
    /// These usually assert that the behavior of linked methods are in sync, rather than checking explciit behaviour
    /// </summary>
    public static class GridTest
    {

        // Checks TryMoveByOffset gives same results as CellType.Rotate
        public static void TryMoveByOffset(IGrid grid, Cell cell)
        {
            var ct = grid.GetCellType(cell);
            foreach (var dir in ct.GetCellDirs())
            {
                foreach (var r in ct.GetRotations(true))
                {
                    var start = cell;
                    var startOffset = (Vector3Int)cell;
                    var endCell = grid.Move(start, dir);
                    if (endCell == null) continue;
                    var endOffset = (Vector3Int)endCell.Value;
                    var success = grid.TryMoveByOffset(start, startOffset, endOffset, r, out var destCell, out var destRotation);
                    var expectedSuccess = grid.TryMove(start, ct.Rotate(dir, r), out var expectedDest, out var _, out var _);
                    Assert.AreEqual(expectedSuccess, success, $"Dir = {dir}, Rot = {r}");
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
                Assert.IsTrue(success, $"Cell={cell} Rotation={r}");
                Assert.AreEqual(cell, cell2, $"Cell={cell} Rotation={r}");
                Assert.AreEqual(r, r2, $"Cell={cell} Rotation={r}");
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
                    Assert.AreEqual(rotation, rotation2, $"Dir={dir} Rotation={rotation}");
                }
            }
        }

        public static void Raycast(IGrid grid, Vector3 origin, Vector3 direction, float maxDistance, float cellSize = 1.0f, float delta = 0.001f)
        {
            var i = 0;
            var actualRis = grid.Raycast(origin, direction, maxDistance).GetEnumerator();
            var expectedRis = DefaultGridImpl.Raycast(grid, origin, direction, maxDistance, cellSize).GetEnumerator();

            while(expectedRis.MoveNext())
            {
                if(!actualRis.MoveNext())
                {
                    Assert.Fail($"Actual raycast terminated after {i} elements, expected more, e.g. {expectedRis.Current.cell}");
                }

                var expected = expectedRis.Current;
                var actual = actualRis.Current;
                var msg = $"{i}";
                Assert.AreEqual(expected.cell, actual.cell, msg);
                Assert.AreEqual(expected.cellDir, actual.cellDir, msg);
                TestUtils.AssertAreEqual(expected.point, actual.point, delta, msg);
                Assert.AreEqual(expected.distance, actual.distance, delta, msg);
                i++;
            }

            if (actualRis.MoveNext())
            {
                Assert.Fail($"Actual raycast has more than {i} elements, e.g. {actualRis.Current.cell}");
            }
        }

        // expectedSrcDest: atm, FindGridSymmetry doesn't guarantee that src of the output
        // matches srcCell of the input. Perhaps it should?
        public static void FindGridSymmetry(IGrid grid, Cell cell, Cell? expectedSrcDest = null)
        {
            var cells = new HashSet<Cell>(new[] { cell });
            var cellType = grid.GetCellType(cell);
            var s = grid.FindGridSymmetry(cells, cells, cell, cellType.GetIdentity());
            Assert.IsNotNull(s);
            Assert.AreEqual(cellType.GetIdentity(), s.Rotation);
            Assert.AreEqual(expectedSrcDest ?? cell, s.Src);
            Assert.AreEqual(expectedSrcDest ?? cell, s.Dest);
        }

        public static void DualMapping(IDualMapping dualMapping, Cell cell, bool checkPositions = true)
        {
            var atLeastOne = false;
            foreach (var (corner, dualCell, invCorner) in dualMapping.DualNeighbours(cell))
            {
                // Check round trip
                Assert.AreEqual(cell, dualMapping.ToBaseCell(dualCell, invCorner), $"Couldn't round trip {cell},{corner} <-> {dualCell},{invCorner}");
                // Check corner positions
                if (checkPositions)
                {
                    TestUtils.AssertAreEqual(dualMapping.DualGrid.GetCellCenter(dualCell), dualMapping.BaseGrid.GetCellCorner(cell, corner), 1e-6, $"Dual Cell Center of {dualCell} does not match corner {corner} of {cell}");
                    TestUtils.AssertAreEqual(dualMapping.BaseGrid.GetCellCenter(cell), dualMapping.DualGrid.GetCellCorner(dualCell, invCorner), 1e-6, $"Cell Center of {cell} does not match dual corner {dualCell} of {invCorner}");
                }

                atLeastOne = true;
            }
            Assert.IsTrue(atLeastOne, $"Cell {cell} has no neighbours");
        }

        // Assumes that the grid is actually precise
        public static void GetCellsIntersectsApprox(IGrid grid, Vector3 min, Vector3 max)
        {
            var cells = grid.GetCells().ToList();
            var rect = grid.GetCellsIntersectsApprox(min, max).ToList();
            foreach(var cell in rect)
            {
                Assert.Contains(cell, cells);
            }
            var aabb = Aabb.FromMinMax(min, max);
            foreach(var cell in cells)
            {
                var cellAabb = Aabb.FromVectors(grid.GetPolygon(cell));
                if (cellAabb.Intersects(aabb))
                {
                    Assert.IsTrue(rect.Contains(cell), $"{cell} should be in range");
                }
                else
                {
                    Assert.IsFalse(rect.Contains(cell), $"{cell} should be out of range");
                }
            }
        }

        public static void GetDiagonals(IGrid grid, Cell cell)
        {
            GetDiagonals(grid, grid.GetDiagonalGrid(), cell);
        }


        public static void GetDiagonals(IGrid grid, IGrid diagonals, Cell cell)
        {
            var dualMapping = grid.GetDual();
            var dualGrid = dualMapping.DualGrid;
            var expectedDiagonals = new List<Cell>();
            var n = NGonCellType.Extract(grid.GetCellType(cell));

            // This is similar to DefaultDiagonalGrid
            for(var i = 0; i < n; i++)
            {
                // Start from corner 1 as the second loop effectively steps back by one
                var dualPair = dualMapping.ToDualPair(cell, (CellCorner)((i + 1) % n));
                if (dualPair == null)
                    continue;
                var (dualCell, inverseCorner) = dualPair.Value;
                var m = NGonCellType.Extract(dualGrid.GetCellType(dualCell));
                // Find all cells adjacent to this dual cell, starting from the original cell,
                // and skipping first (the original cell) and last (will be covered by next iteration of i)
                for(var j = 1; j < m - 1; j++)
                {
                    var basePair = dualMapping.ToBasePair(dualCell, (CellCorner)(((int)inverseCorner + j) % m));
                    if (basePair == null)
                        continue;
                    var (baseCell, _) = basePair.Value;
                    expectedDiagonals.Add(baseCell);

                }
            }

            var actualDiagonal = diagonals.GetNeighbours(cell).ToList();
            CollectionAssert.AreEqual(expectedDiagonals, actualDiagonal);
        }

        public static void TestTriangleMesh(IGrid grid, Cell cell, Func<CellDir, Vector3> expectedNormal, Func<CellDir, int> expectedCount)
        {
            var cellType = grid.GetCellType(cell);
            int i = 0;
            var counts = new Dictionary<CellDir, int>();
            foreach (var (v0, v1, v2, dir) in grid.GetTriangleMesh(cell))
            {
                var n = MeshUtils.GetNormalDirection(v0, v1, v2);
                var n2 = expectedNormal(dir);
                var d = Vector3.Dot(n, n2);
                Assert.IsTrue(d > 0, $"Normal = {n}, Expected {n2}, Dir = {cellType.Format(dir)}, Index={i}");
                counts[dir] = counts.GetValueOrDefault(dir, 0) + 1;
                i++;
            }
            foreach(var dir in grid.GetCellDirs(cell))
            {
                Assert.AreEqual(expectedCount(dir), counts.GetValueOrDefault(dir, 0), $"Dir = {cellType.Format(dir)}");
            }
        }
    }
}
