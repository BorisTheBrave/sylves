using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sylves.Test
{
    [TestFixture]
    public class SquareCellTypeTest
    {
        [Test]
        public void TestToMatrix()
        {
            var vs = new Vector3Int[] { new(0, 0, 0), new(1, 0, 0), new(0, 1, 0) };
            foreach(var r in SquareCellType.Instance.GetRotations(false))
            {
                var sr = (SquareRotation)r;
                var m = sr.ToMatrix();

                foreach (var v in vs)
                {
                    Assert.AreEqual(m.MultiplyPoint(v), (Vector3)( sr * v), $"{r}, {v}");
                }

            }
        }

        [Test]
        public void TestInvert()
        {
            Assert.AreEqual((CellDir)SquareDir.Left, SquareCellType.Instance.Invert((CellDir)SquareDir.Right));
        }


        [Test]
        public void TestTryGetRotation()
        {
            GridTest.TryGetRotation(SquareCellType.Instance);
        }
    }
}
