using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylves
{
    public static class KruskalMinimumSpanningTree
    {
        public static bool LexCompare(Cell a, Cell b)
        {
            if (a.x < b.x) return true;
            if (a.x > b.x) return false;
            if (a.y < b.y) return true;
            if (a.y > b.y) return false;
            if (a.z < b.z) return true;
            if (a.z > b.z) return false;
            return false;
        }

        private class Data
        {
            public Cell parent;
            public int rank;
        }

        public static IEnumerable<Step> Calculate(IGrid grid, Func<Step, float?> stepLengths)
        {
            // Disjoint set data structure
            var data = new Dictionary<Cell, Data>();
            foreach (var cell in grid.GetCells())
                data[cell] = new Data { parent = cell };
            Cell FindSet(Cell cell)
            {
                Data d;
                while ((d = data[cell]).parent!= cell)
                {
                    (cell, d.parent) = (d.parent, data[d.parent].parent);
                }
                return cell;
            }
            void Union(Cell x, Cell y)
            {
                // Replace nodes by roots
                x = FindSet(x);
                y = FindSet(y);

                if (x == y) return;

                var dx = data[x];
                var dy = data[y];

                // If necessary, swap variables to ensure that
                // x has at least as many descendants as y
                if (data[x].rank < data[y].rank)
                {
                    (x, y) = (y, x);
                    (dx, dy) = (dy, dx);
                }


                // Make x the new root
                dy.parent = x;
                // Update the size of x
                if (dx.rank == dy.rank)
                    dx.rank += 1;
            }


            var steps = grid.GetCells()
                // Collect all steps in the grid
                .SelectMany(cell => grid.GetCellDirs(cell).Select(d => Step.Create(grid, cell, d, stepLengths)))
                .OfType<Step>()
                // De-duplicate step and inverse step
                .Where(x=> LexCompare(x.Src, x.Dest))
                .OrderBy(x => x.Length)
                // Sort by smallest length
                .ToList();
            foreach(var step in steps)
            {
                if(FindSet(step.Src) != FindSet(step.Dest))
                {
                    yield return step;
                    Union(step.Src, step.Dest);
                }
            }
        }
    }
}
