using System;
using System.Collections.Generic;
using System.Text;

namespace Sylves
{
    public static class IntUtils
    {
        public static int Zip(short a, short b)
        {
            return 
                (a & 0x000F) << 0 |
                (b & 0x000F) << 4 |
                (a & 0x00F0) << 4 |
                (b & 0x00F0) << 8 |
                (a & 0x0F00) << 8 |
                (b & 0x0F00) << 12 |
                (a & 0xF000) << 12 |
                (b & 0xF000) << 16;

        }

        public static (short, short) Unzip(int i)
        {
            return ((short)(
                (i & 0x0000000F) >> 0 |
                (i & 0x00000F00) >> 4 |
                (i & 0x000F0000) >> 8 |
                (i & 0x0F000000) >> 12), (short)(
                (i & 0x000000F0) >> 4 |
                (i & 0x0000F000) >> 8 |
                (i & 0x00F00000) >> 12 |
                (i & 0xF0000000) >> 16
                ));
        }
    }
}
