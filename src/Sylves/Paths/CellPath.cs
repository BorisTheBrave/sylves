using System.Collections.Generic;
using System.Linq;

namespace Sylves
{

    public struct Step
    {

        public Cell Src;
        public Cell Dest;
        public CellDir Dir;
        public CellDir InverseDir;
        public Connection Connection;
        public float Length;

        public static Step? Create(IGrid grid, Cell src, CellDir dir, float length = 0.0f)
        {
            if (grid.TryMove(src, dir, out var dest, out var inverseDir, out var connection))
            {
                return new Step
                {
                    Src = src,
                    Dest = dest,
                    Dir = dir,
                    InverseDir = inverseDir,
                    Connection = connection,
                    Length = length,
                };
            }
            else
            {
                return null;
            }
        }

        public Step Inverse => new Step
        {
            Src = Dest,
            Dest = Src,
            Dir = InverseDir,
            InverseDir = Dir,
            Connection = Connection.GetInverse(),
            Length = Length,
        };
    }

    public class CellPath
    {
        public IList<Step> Steps;

        public Cell Src => Steps[0].Src;
        
        public Cell Dest => Steps[Steps.Count - 1].Dest;

        public float Length => Steps.Select(x => x.Length).Sum();

        public IEnumerable<Cell> Cells
        {
            get
            {
                yield return Src;
                foreach (var s in Steps)
                {
                    yield return s.Dest;
                }
            }
        }

        public override string ToString()
        {
            return "[" + string.Join(",", Cells) + "]";
        }
    }
}
