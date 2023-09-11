using System;
using System.Collections.Generic;
using System.Text;

namespace Sylves
{
    internal static class BitUtils
    {
        // As BitOperations.LeadingZeroCount, which we should eventually replace this with
        public static int LeadingZeroCount(uint x)
        {
            // https://stackoverflow.com/a/10439333

            const int numIntBits = sizeof(int) * 8; //compile time constant
                                                    //do the smearing
            x |= x >> 1;
            x |= x >> 2;
            x |= x >> 4;
            x |= x >> 8;
            x |= x >> 16;
            //count the ones
            x -= x >> 1 & 0x55555555;
            x = (x >> 2 & 0x33333333) + (x & 0x33333333);
            x = (x >> 4) + x & 0x0f0f0f0f;
            x += x >> 8;
            x += x >> 16;
            return (int)(numIntBits - (x & 0x0000003f)); //subtract # of 1s from 32
        }
    }
}
