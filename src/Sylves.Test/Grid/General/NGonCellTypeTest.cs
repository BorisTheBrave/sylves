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

            foreach(var r in b.GetRotations(true))
            {
                AssertAreEqual(a.GetMatrix(r), b.GetMatrix(r), 1e-6, $"{r}");
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

        [Test]
        public void TestReflection()
        {
            var a = new NGonCellType(6);
            Assert.AreEqual((CellRotation)~2, a.Multiply((CellRotation)2, (CellRotation)~0));
            Assert.AreEqual((CellRotation)~3, a.Multiply((CellRotation)3, (CellRotation)~0));
            TestUtils.AssertAreEqual(Matrix4x4.Scale(new Vector3(-1, 1, 1)), a.GetMatrix(a.ReflectX), 1e-6);
            TestUtils.AssertAreEqual(Matrix4x4.Scale(new Vector3(1, -1, 1)), a.GetMatrix(a.ReflectY), 1e-6);
        }

        [Test]
        public void TestTryGetRotation()
        {
            GridTest.TryGetRotation(new NGonCellType(6));
            GridTest.TryGetRotation(new NGonCellType(5));
        }
    }
}
