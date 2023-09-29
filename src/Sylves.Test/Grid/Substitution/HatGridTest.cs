using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sylves.Test
{
    [TestFixture]
    internal class HatGridTest
    {
        [Test]
        public void Test()
        {
            var pts = HatGrid.ToPoints(HatGrid.MetaH);
            pts = HatGrid.ToPoints(HatGrid.MetaT);
            pts = HatGrid.ToPoints(HatGrid.MetaP);
            pts = HatGrid.ToPoints(HatGrid.MetaF);

            //var a1 = HatGrid.ToPoint("(A- 1)");
            //var a2 = HatGrid.ToPoint("(B- 1) (X- 1) (X+ 2)");
            //var a1 = HatGrid.ToPoint("(B- 1)");
            //var a2 = HatGrid.ToPoint("(X- 2) (X+ 1) (A- 1)");
            var a1 = HatGrid.ToPoint("(B- 2)");
            var a2 = HatGrid.ToPoint("(X- 3) (X+ 2) (A- 2)");
            var x = a2.x / a1.x;
            var y = a2.y / a1.y;
        }
    }
}
