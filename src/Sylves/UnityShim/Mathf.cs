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

        public static float Sin(float x)
        {
            return (float)Math.Sin(x);
        }
        public static float Cos(float x)
        {
            return (float)Math.Cos(x);
        }

        public static float Asin(float x)
        {
            return (float)Math.Asin(x);
        }
        public static float Acos(float x)
        {
            return (float)Math.Acos(x);
        }

        public static float Atan2(float y, float x)
        {
            return (float)Math.Atan2(y, x);
        }

        public static float Clamp(float value, float min, float max) => value < min ? min : value > max ? max : value;
        public static float Clamp01(float value) => Clamp(value, 0.0f, 1.0f);

        public static float PI => (float)Math.PI;
    }
}
