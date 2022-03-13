using System;
using System.Collections.Generic;
using System.Text;

namespace Sylves
{
    internal static class MathUtils
    {
        public static int PMod(int a, int b) => ((a % b) + b) % b;
    }
}
