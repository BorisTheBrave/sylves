using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sylves.Test
{
    [TestFixture]
    public class TriangleInterpolationTest
    {
        public void Test()
        {
            var f = TriangleInterpolation.Interpolate(new Vector2(5, 0), new Vector2(10, 0), new Vector2(0, 5));
        }
    }
}
