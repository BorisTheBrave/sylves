using System;
using System.Collections.Generic;

namespace Sylves
{
    public static class Pathfinding
    {
        // Simple variants for every day use

        public static float? FindDistance(IGrid grid, Cell src, Cell dest, Func<Cell, bool> isAccessible = null, Func<Step, float?> stepLengths = null)
        {
                return FindPath(grid, src, dest, isAccessible, stepLengths)?.Length;
        }

        public static CellPath FindPath(IGrid grid, Cell src, Cell dest, Func<Cell, bool> isAccessible = null, Func<Step, float?> stepLengths = null)
        {
            if(stepLengths == null)
            {
                var heuristic = GetAdmissibleHeuristic(grid, dest);
                if(heuristic != null)
                {
                    var pf = new AStarPathfinding(grid, src, StepLengths.Create(isAccessible, null), heuristic);
                    pf.Run(dest);
                    return pf.ExtractPathTo(dest);
                }
            }

            var dpf = new DijkstraPathfinding(grid, src, StepLengths.Create(isAccessible, stepLengths));
            dpf.Run(dest);
            return dpf.ExtractPathTo(dest);
        }
        public static Dictionary<Cell, float> FindDistances(IGrid grid, Cell src, Func<Cell, bool> isAccessible = null, Func<Step, float?> stepLengths = null)
        {
            return FindRange(grid, src, float.PositiveInfinity, isAccessible, stepLengths);
        }
        public static Dictionary<Cell, float> FindRange(IGrid grid, Cell src, float maxRange, Func<Cell, bool> isAccessible = null, Func<Step, float?> stepLengths = null)
        {
            var dpf = new DijkstraPathfinding(grid, src, StepLengths.Create(isAccessible, stepLengths));
            dpf.Run(maxRange: maxRange);
            return dpf.Distances;
        }

        // Heuristics for AStar

        /// <summary>
        /// Returns a heuristic that returns the absolute difference in co-ordinates.
        /// </summary>
        public static Func<Cell, float> GetCordinateDiffMetric(Cell target, float scale = 1.0f)
        {
            return (cell) => (Math.Abs(cell.x - target.x) + Math.Abs(cell.y - target.y) + Math.Abs(cell.z - target.z)) * scale;
        }

        /// <summary>
        /// Returns a heuristict that measures the distance between cell centers.
        /// </summary>
        public static Func<Cell, float> GetEuclidianDistanceMetric(IGrid grid, Cell target)
        {
            var targetCenter = grid.GetCellCenter(target);
            return (cell) => (grid.GetCellCenter(cell) - targetCenter).magnitude;
        }

        /// <summary>
        /// Returns an admissible heuristict for distances from a cell to target,
        /// assuming edge distances of 1.0.
        /// </summary>
        public static Func<Cell, float> GetAdmissibleHeuristic(IGrid grid, Cell target)
        {
            // TODO: Better dispatch?
            if(grid is SquareGrid || grid is CubeGrid || grid is TriangleGrid)
            {
                return GetCordinateDiffMetric(target);
            }
            if(grid is HexGrid)
            {
                return GetCordinateDiffMetric(target);

            }
            if (grid is PlanarPrismModifier ppm)
            {
                var uh = GetAdmissibleHeuristic(ppm.Underlying, target);
                throw new NotImplementedException();
            }
            if (grid is TransformModifier tm)
            {
                // As we're assuming 1.0 edge distances, the actual transform doesn't matter.
                return GetAdmissibleHeuristic(tm.Underlying, target);
            }
            if(grid is BijectModifier bm)
            {
                var uh = GetAdmissibleHeuristic(bm.Underlying, target);
                if (uh == null)
                    return null;
                throw new NotImplementedException();
            }
            if(grid is WrapModifier)
            {
                return null;
            }

            // Unrecognized grid
            return null;
        }
    }
}
