using System;
using System.Collections.Generic;

namespace Sylves
{
    /// <summary>
    /// Computes Dijkstra's Algorithm.
    /// 
    /// This class to find paths starting at source, and terminating at a single point, or a range of points.
    /// </summary>
    public class DijkstraPathfinding
    {
        // Options
        Cell src;
        IGrid grid;
        Func<Step, float?> stepLengths;

        // Internal data
        Dictionary<Cell, float> distances = new Dictionary<Cell, float>();
        Dictionary<Cell, Step> steps = new Dictionary<Cell, Step>();

        public DijkstraPathfinding(IGrid grid, Cell src, Func<Step, float?> stepLengths)
        {
            this.src = src;
            this.grid = grid;
            this.stepLengths = stepLengths;
        }

        public void Run(Cell? target = null, float maxRange = float.PositiveInfinity)
        {
            // We use a simple binary heap, and simply insert duplicate entries if a distance decreases.
            // Wikipedia: 
            // "Chen et al.[11] examined priority queues specifically for use with Dijkstra's algorithm
            // and concluded that in normal cases using a d-ary heap without decrease-key (instead duplicating
            // nodes on the heap and ignoring redundant instances) resulted in better performance, despite
            // the inferior theoretical performance guarantees. 
            var heap = new Heap<Cell, float>();
            heap.Insert(src, 0);
            distances[src] = 0;
            while(heap.Count > 0)
            {
                var ld = heap.PeekKey();
                var cell = heap.Pop();
                var d = distances[cell];

                if(ld > maxRange)
                {
                    // Found everything in a given distance
                    break;
                }

                if (cell == target)
                {
                    // Found the given cell
                    break;
                }

                if (d < ld)
                {
                    // This entry is redundant, we've already visited with a lower priority.
                    continue;
                }

                foreach(var dir in grid.GetCellDirs(cell))
                {
                    if (Step.Create(grid, cell, dir, 0) is Step step)
                    {
                        var length = stepLengths(step);
                        if (length == null)
                            continue;
                        step.Length = length.Value;

                        var d2 = d + length.Value;
                        var dest = step.Dest;
                        if ((!distances.TryGetValue(dest, out var d3) || d2 < d3) && d2 <= maxRange)
                        {
                            distances[dest] = d2;
                            steps[dest] = step;
                            heap.Insert(dest, d2);
                        }
                    }
                }
            }
        }

        public Dictionary<Cell, float> Distances => distances;

        public CellPath ExtractPathTo(Cell target)
        {
            if(target != src && !steps.ContainsKey(target))
            {
                return null;
            }

            var pathSteps = new List<Step>();
            var cell = target;
            while(cell != src)
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
