using System;
using System.Collections.Generic;
using System.Text;

namespace Sylves
{
    /// <summary>
    /// Supplies simple deterministic hashes.
    /// </summary>
    public static class HashUtils
    {
        public static int Hash(int v1, int v2)
        {
            unchecked
            {
                int hash = 5381;

                hash = ((hash << 5) + hash) ^ v1;
                hash = ((hash << 5) + hash) ^ v2;

                return hash;
            }
        }
        public static int Hash(int v1, int v2, int v3)
        {
            unchecked
            {
                int hash = 5381;

                hash = ((hash << 5) + hash) ^ v1;
                hash = ((hash << 5) + hash) ^ v2;
                hash = ((hash << 5) + hash) ^ v3;

                return hash;
            }
        }
        public static int Hash(int v1, int v2, int v3, int v4)
        {
            unchecked
            {
                int hash = 5381;

                hash = ((hash << 5) + hash) ^ v1;
                hash = ((hash << 5) + hash) ^ v2;
                hash = ((hash << 5) + hash) ^ v3;
                hash = ((hash << 5) + hash) ^ v4;

                return hash;
            }
        }

        public static int Hash(Cell cell)
        {
            return Hash(cell.x, cell.y, cell.z);
        }
    }
}
