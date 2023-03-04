using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylves
{
    public static class ChisledPathfinding
    {
        /// <summary>
        /// Implements the chiseled paths algorith descibed here:
        /// https://www.boristhebrave.com/2022/03/20/chiseled-paths-revisited/
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <param name="isAccessible"></param>
        /// <param name="stepLengths"></param>
        /// <param name="randomDouble"></param>
        /// <returns></returns>
        public static CellPath FindPath(IGrid grid, Cell src, Cell dest, Func<Cell, bool> isAccessible = null, Func<Step, float?> stepLengths = null, Func<double> randomDouble = null)
        {
            randomDouble = randomDouble ?? new Random().NextDouble;

            var cellStates = grid.GetCells().ToDictionary(x => x, x => State.Open);
            cellStates[src] = State.Forced;
            cellStates[dest] = State.Forced;

            // Invariant of cellStates
            var openCells = new HashSet<Cell>(cellStates.Keys);
            openCells.Remove(src);
            openCells.Remove(dest);


            bool IsAccessible(Cell cell)
            {
                return cellStates[cell] != State.Blocked && (isAccessible == null || isAccessible(cell));
            }

            CellPath FindPath() => Pathfinding.FindPath(grid, src, dest, IsAccessible, stepLengths);

            var witness = FindPath();
            var witnessSet = new HashSet<Cell>(witness.Cells); // Invariant of witness

            if (witness == null)
                return null;

            while(true)
            {
                if (openCells.Count == 0)
                    return witness;

                // Randomly pick an open cell
                var i = (int)(openCells.Count * randomDouble());
                var c = openCells.Skip(i).First();

                // Set it to blocked
                cellStates[c] = State.Blocked;
                openCells.Remove(c);

                // If it's currently on the witness, try to find a new witness
                if(witnessSet.Contains(c))
                {
                    var newPath = FindPath();
                    if(newPath == null)
                    {
                        cellStates[c] = State.Forced;
                    }
                    else
                    {
                        witness = newPath;
                        witnessSet = new HashSet<Cell>(witness.Cells);
                    }
                }
            }
        }

        private enum State
        {
            Open,
            Blocked,
            Forced,
        }

    }
}
