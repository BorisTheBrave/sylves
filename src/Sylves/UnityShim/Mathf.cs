using System;
using System.Collections.Generic;
using System.Text;

namespace Sylves
{
    public static class Mathf
    {
        public static float Sqrt(float v)
        {
            return (float)Math.Sqrt(v);
        }

        public static int CeilToInt(float x)
        {
            return (int)Math.Ceiling(x);
        }

        public static int FloorToInt(float x)
        {
            return (int)Math.Floor(x);
        }

        public static int RoundToInt(float x)
        {
            return (int)Math.Round(x);
        }
    }
}
