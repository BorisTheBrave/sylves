using System;
using System.Collections.Generic;
using System.Text;
#if BIGINT
using System.Numerics;
#endif

namespace Sylves
{
    public static class Encoding
    {
#if BIGINT
        // Interleave byte-by-byte
        public static BigInteger ZOrderEncode(BigInteger a, BigInteger b)
        {
            var a1 = a.ToByteArray();
            var b1 = b.ToByteArray();
            var l = a1.Length > b1.Length ? a1.Length : b1.Length;
            var r = new byte[l * 2];
            var aSignExt = a < 0 ? (byte)255 : (byte)0;
            var bSignExt = b < 0 ? (byte)255 : (byte)0;
            for (var i=0;i<l;i++)
            {
                r[2 * i] = i < a1.Length ? a1[i] : aSignExt;
                r[2 * i + 1] = i < b1.Length ? b1[i] : bSignExt;
            }
            return new BigInteger(r);
        }
        public static (BigInteger, BigInteger) ZOrderDecode(BigInteger i)
        {
            throw new NotImplementedException();
        }
#else
        public static int ZigZagEncode(int i) => (i << 1) ^ (i >> 31);
        //public static int ZigZagDecode(int i) => (i >>> 1) ^ (-(i & 1));
        public static int ZigZagDecode(int i) => ((i >> 1) & 0x7FFFFFFF) ^ (-(i & 1));
        public static int ZOrderEncode(short a, short b)
        {
            return Interleave(a) | (Interleave(b) << 1);
        }
        public static (short, short) ZOrderDecode(int i)
        {
            var evenBits = ((uint)i) & (uint)0x55555555;
            var oddBits = ((uint)i) & (uint)0xAAAAAAAA;

            evenBits = (evenBits | (evenBits >> 1)) & 0x33333333;
            evenBits = (evenBits | (evenBits >> 2)) & 0x0F0F0F0F;
            evenBits = (evenBits | (evenBits >> 4)) & 0x00FF00FF;
            evenBits = (evenBits | (evenBits >> 8)) & 0x0000FFFF;

            oddBits = (oddBits >> 1) & 0x55555555;
            oddBits = (oddBits | (oddBits >> 1)) & 0x33333333;
            oddBits = (oddBits | (oddBits >> 2)) & 0x0F0F0F0F;
            oddBits = (oddBits | (oddBits >> 4)) & 0x00FF00FF;
            oddBits = (oddBits | (oddBits >> 8)) & 0x0000FFFF;

            return ((short)evenBits, (short)oddBits);
        }

        private static int Interleave(short input)
        {
            int word = input;
            word = (word ^ (word << 8)) &  0x00ff00ff;
            word = (word ^ (word << 4)) &  0x0f0f0f0f;
            word = (word ^ (word << 2)) &  0x33333333;
            word = (word ^ (word << 1)) &  0x55555555;
            return word;
        }
#endif
        public static int RavelEncode(int a, int b, int n)
        {
            if (a < 0 || a >= n)
                throw new Exception("Ravel encode can only encode argument a in range 0 to n-1");
            return a + n * b;
        }

        public static (int, int) RavelDecode(int i, int n)
        {
            var b = Math.DivRem(i, n, out var a);
            if (i < 0)
            {
                b -= 1;
                a += n;
            }
            return (a, b);
        }
    }
}
