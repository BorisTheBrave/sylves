using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylves
{
    public static class OutlineCells
    {
        /// <summary>
        /// Describes a collection of (Cell, CellDir) edges which are arranged in order.
        /// </summary>
        public class OutlineLoop
        {
            /// <summary>
            /// If true, the first and last edges do not connect up.
            /// </summary>
            public bool IsArc { get; set; }
            /// <summary>
            /// If true, the first and last edges do connect up.
            /// </summary>
            public bool IsLoop
            {
                get
                {
                    return !IsArc;
                }
                set
                {
                    IsArc = !value;
                }
            }

            public List<(Cell Cell, CellDir CellDir)> Edges { get; set; }
        }

        /// <summary>
        /// Finds all the (Cell, CellDir)'s that start inside the provided set of cells, and point outside,
        /// and organizes them into connected arcs and loops that outline the provided set.
        /// </summary>
        /// <param name="grid">The grid to operate on</param>
        /// <param name="cells">The set of cells to outline.</param>
        /// <param name="includeGridBorder">If set, cell directions that don't connect to another cell are also part of the outline.</param>
        public static IEnumerable<OutlineLoop> Outline(IGrid grid, ISet<Cell> cells, bool includeGridBorder = true)
        {
            foreach(var outline in OutlineLowAlloc(grid, cells, includeGridBorder))
            {
                // Copy all the data so the object re-use is removed.
                yield return new OutlineLoop
                {
                    IsArc = outline.IsArc,
                    Edges = outline.Edges.ToList(),
                };
            }
        }

        /// <summary>
        /// Finds all the (Cell, CellDir)'s that start inside the provided set of cells, and point outside,
        /// and organizes them into connected arcs and loops that outline the provided set.
        /// This variant uses less allocations, but re-uses the same OutlineLoop object, so you must be careful
        /// accessing the enumerator.
        /// </summary>
        /// <param name="grid">The grid to operate on</param>
        /// <param name="cells">The set of cells to outline.</param>
        /// <param name="includeGridBorder">If set, cell directions that don't connect to another cell are also part of the outline.</param>
        public static IEnumerable<OutlineLoop> OutlineLowAlloc(IGrid grid, ISet<Cell> cells, bool includeGridBorder = true)
        {
            if (!grid.Is2d) throw new Grid3dException();
            var visited = new HashSet<(Cell, CellDir)>();
            
            // Re-use these to save allocations
            var results = new List<(Cell, CellDir)>();
            var results2 = new List<(Cell, CellDir)>();
            var outline = new OutlineLoop();


            foreach (var cell in cells)
            {
                foreach (var dir in grid.GetCellDirs(cell))
                {

                    var other = grid.Move(cell, dir);

                    if (other == null ? !includeGridBorder : cells.Contains(other.Value))
                        continue;

                    if (visited.Contains((cell, dir)))
                        continue;

                    results.Clear();
                    var currentCell = cell;
                    var currentDir = dir;
                    var isArc = false;
                    void TurnLeft()
                    {
                        var cellType = grid.GetCellType(cell);
                        var n = NGonCellType.Extract(cellType).Value;
                        currentDir = (CellDir)(((int)currentDir + 1) % n);
                    }
                    void TurnRight()
                    {
                        var cellType = grid.GetCellType(cell);
                        var n = NGonCellType.Extract(cellType).Value;
                        currentDir = (CellDir)(((int)currentDir + n - 1) % n);
                    }
                    // Once per edge
                    while (true)
                    {
                        results.Add((currentCell, currentDir));
                        visited.Add((currentCell, currentDir));
                        TurnLeft();
                        // Loop around a corner looking for next edge
                        while (true)
                        {
                            var canMove = grid.TryMove(currentCell, currentDir, out var dest, out var inverseDir, out var _);
                            if (!canMove && !includeGridBorder)
                            {
                                isArc = true;
                                goto a;
                            }
                            if (canMove && cells.Contains(dest))
                            {
                                currentCell = dest;
                                currentDir = inverseDir;
                                TurnLeft();
                                continue;
                            }
                            else
                            {

                                if ((currentCell, currentDir) == (cell, dir))
                                {
                                    // Full loop
                                    goto a;
                                }
                                break;
                            }
                        }
                    }
                    a:
                    if(isArc)
                    {
                        // Not a full loop. So we need to work *backwards*
                        results2.Clear();
                        currentCell = cell;
                        currentDir = dir;
                        // Once per edge
                        while(true)
                        {
                            TurnRight();
                            // Loop around a corner looking for the next edge
                            while(true)
                            {
                                var canMove = grid.TryMove(currentCell, currentDir, out var dest, out var inverseDir, out var _);
                                if (!canMove && !includeGridBorder)
                                {
                                    goto b;
                                }
                                if (canMove && cells.Contains(dest))
                                {
                                    currentCell = dest;
                                    currentDir = inverseDir;
                                    TurnRight();
                                    continue;
                                }
                                else
                                {
                                    if ((currentCell, currentDir) == (cell, dir))
                                    {
                                        // Full loop!?
                                        throw new Exception();
                                    }
                                    results2.Add((currentCell, currentDir));
                                    visited.Add((currentCell, currentDir));
                                    break;
                                }
                            }
                        }
                    b:
                        // results = results2.reverse() ++ results;
                        results2.Reverse();
                        results2.AddRange(results);
                        (results, results2) = (results2, results);
                    }
                    outline.IsArc = isArc;
                    outline.Edges = results;
                    yield return outline;
                }
            }
        }

        /*
        public static void FindEdges(IGrid grid, IDualMapping dualMapping, ISet<Cell> cells)

        {
            if (!grid.Is2d) throw new Grid3dException();
            foreach(var cell in cells) 
            { 
                foreach(var dir in grid.GetCellDirs(cell))
                {
                    var other = grid.Move(cell, dir);
                    if (other == null)
                        continue;
                    if (cells.Contains(other.Value))
                        continue;

                    var w = new DW(grid, cell, (CellCorner)dir);
                    if (!w.ToDual(dualMapping))
                        throw new Exception();

                    while (true)
                    {

                        // Turn to face next dual cell on boundary
                        while (true)
                        {
                            w.Right();
                            if (!cells.Contains(dualMapping.ToBaseCell(w.Cell, w.Corner1).Value))
                                break;
                        }
                        w.ForwardAndRight();
                    }


                    var dualPair = dualMapping.ToDualPair(cell, (CellCorner)dir);
                    if (dualPair == null)
                        throw new Exception();
                    var (dualCell, invCorner) = dualPair.Value;
                    var w = 
                    var m = 
                    var dualDir = (CellDir)invCorner;
                    var boundary = new List<Cell>();
                    while(true)
                    {
                        
                    }
                }
            }
        }

        private class DW
        {
            public DW(IGrid grid, Cell cell, CellCorner corner1)
            {
                Grid = grid;
                Cell = cell;
                Corner1 = corner1;
                N = NGonCellType.Extract(grid.GetCellType(cell)).Value;
            }

            public IGrid Grid { get; private set; }
            public Cell Cell { get; private set; }
            public CellCorner Corner1 { get; private set; }
            public CellDir Dir
            {
                get
                {
                    return (CellDir)Corner1;
                }
                set
                {
                    Corner1 = (CellCorner)value;
                }
            }

            public int N { get; }

            public void Left()
            {
                Corner1 = (CellCorner)(((int)Corner1 + 1) % N);
            }

            public void Right()
            {
                Corner1 = (CellCorner)(((int)Corner1 + N - 1) % N);
            }

            public bool ForwardAndRight()
            {
                if (!Grid.TryMove(Cell, Dir, out var dest, out var inverseDir, out var _))
                    return false;

                Cell = dest;
                Dir = inverseDir;
                Right();
                return true;
            }

            public bool ForwardAndLeft()
            {
                if (!Grid.TryMove(Cell, Dir, out var dest, out var inverseDir, out var _))
                    return false;

                Cell = dest;
                Dir = inverseDir;
                Left();
                return true;
            }

            public bool ToDual(IDualMapping mapping)
            {
                var pair = mapping.ToDualPair(Cell, Corner1);
                if (pair == null)
                    return false;
                (Cell, Corner1) = pair.Value;
                Grid = mapping.DualGrid;
                return true;
            }

            public bool ToBase(IDualMapping mapping)
            {
                var pair = mapping.ToBasePair(Cell, Corner1);
                if (pair == null)
                    return false;
                (Cell, Corner1) = pair.Value;
                Grid = mapping.BaseGrid;
                return true;
            }

        }
        */
    }
}
