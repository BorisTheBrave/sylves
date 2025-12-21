using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Sylves
{
    // MeshGrid stores cell data in a very specific way, that we often need to pack/unpack
    // when working with meshes
    // Note there's a lot of explicit casting here, to and from Int32. That's because when BIGINT is on,
    // Grids use BigInt, but meshes use Int32.
    internal static class MeshGridUtils
    {
        public static Int32 GetFace(Cell cell)
        {
            return (Int32)cell.x;
        }

        public static (Int32 face, Int32 submesh, int layer) Unpack(Cell cell)
        {
            return ((Int32)cell.x, (Int32)cell.y, cell.z);
        }

        public static Cell Pack(Int32 face, Int32 submesh, int layer)
        {
            return new Cell(face, submesh, layer);

        }
    }
}