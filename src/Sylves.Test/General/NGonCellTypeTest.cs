using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Sylves.Test.TestUtils;

namespace Sylves.Test
{
    [TestFixture]
    public class NGonCellTypeTest
    {
        [Test]
        public void TestToMatrix()
        {
            var a = new NGonCellType(4);
            var b = SquareCellType.Instance;

            foreach(var r in b.GetRotations())
            {
                AssertAreEqual(a.GetMatrix(r), b.GetMatrix(r), 1e-6);
            }
        }

        [Test]
        public void TestMultiply()
        {
            var a = new NGonCellType(4);
            var b = SquareCellType.Instance;

            foreach (var r1 in b.GetRotations(true))
            {
                foreach (var r2 in b.GetRotations(true))
                {
                    Assert.AreEqual(a.Multiply(r1, r2), b.Multiply(r1, r2), $"{r1}, {r2}");
                }
            }
        }

        [Test]
        public void TestRotate()
        {
            var a = new NGonCellType(4);
            var b = SquareCellType.Instance;

            foreach (var r1 in b.GetRotations(true))
            {
                foreach (var d in b.GetCellDirs())
                {
                    Assert.AreEqual(a.Rotate(d, r1), b.Rotate(d, r1), $"{d}, {r1}");
                }
            }
        }

        [Test]
        public void TestInvert()
        {
            var a = new NGonCellType(4);
            Assert.AreEqual((CellDir)SquareDir.Left, a.Invert((CellDir)SquareDir.Right));
        }
    }
}
