using System;
using System.Collections.Generic;

namespace Sylves
{
    /// <summary>
    /// Implementation class for the A* Pathfinding algorithm.
    /// This algorith takes an admissible heuristic, and uses it to find the shortest path
    /// 
    /// </summary>
    public class AStarPathfinding
    {
        // Options
        Cell src;
        IGrid grid;
        Func<Step, float?> stepLengths;
        Func<Cell, float> heuristic;

        // Internal data
        Dictionary<Cell, float> distances = new Dictionary<Cell, float>();
        Dictionary<Cell, float> fScores = new Dictionary<Cell, float>();
        Dictionary<Cell, Step> steps = new Dictionary<Cell, Step>();

        public AStarPathfinding(IGrid grid, Cell src, Func<Step, float?> stepLengths, Func<Cell, float> heuristic)
        {
            this.src = src;
            this.grid = grid;
            this.stepLengths = stepLengths;
            this.heuristic = heuristic;
        }

        public void Run(Cell target)
        {
            var heap = new Heap<Cell, float>();
            heap.Insert(src, 0);
            distances[src] = 0;
            fScores[src] = heuristic(src);
            while(heap.Count > 0)
            {
                var lf = heap.PeekKey();
                var cell = heap.Pop();
                var d = distances[cell];
                var f = fScores[cell];

                if (cell == target)
                {
                    // Found the given cell
                    break;
                }

                if (f < lf)
                {
                    // This entry is redundant, we've already visited with a lower priority.
                    continue;
                }

                foreach (var dir in grid.GetCellDirs(cell))
                {
                    if (Step.Create(grid, cell, dir, 0) is Step step)
                    {
                        var length = stepLengths(step);
                        if (length == null)
                            continue;
                        step.Length = length.Value;

                        var d2 = d + length.Value;
                        var dest = step.Dest;
                        if (!distances.TryGetValue(dest, out var d3) || d2 < d3)
                        {
                            distances[dest] = d2;
                            fScores[dest] = d2 + heuristic(dest);
                            steps[dest] = step;
                            heap.Insert(dest, fScores[dest]);
                        }
                    }
                }
            }
        }

        public CellPath ExtractPathTo(Cell target)
        {
            if (target != src && !steps.ContainsKey(target))
            {
                return null;
            }

            var pathSteps = new List<Step>();
            var cell = target;
            while (cell != src)
            {
                var step = steps[cell];
                pathSteps.Add(step);
                cell = step.Src;
            }
            pathSteps.Reverse();
            return new CellPath { Steps = pathSteps };
        }

    }
}
