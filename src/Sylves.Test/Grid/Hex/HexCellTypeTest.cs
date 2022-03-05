﻿using NUnit.Framework;
#if UNITY
using UnityEngine;
#endif


namespace Sylves.Test
{
    [TestFixture]
    public class HexCellTypeTest
    {
        [Test]
        [TestCase(HexOrientation.PointyTopped)]
        [TestCase(HexOrientation.FlatTopped)]
        public void TestReflection(HexOrientation orientation)
        {
            var a = HexCellType.Get(orientation);
            Assert.AreEqual((CellRotation)~2, a.Multiply((CellRotation)2, (CellRotation)~0));
            Assert.AreEqual((CellRotation)~3, a.Multiply((CellRotation)3, (CellRotation)~0));
            TestUtils.AssertAreEqual(Matrix4x4.Scale(new Vector3(-1, 1, 1)), a.GetMatrix(a.ReflectX), 1e-6);
            TestUtils.AssertAreEqual(Matrix4x4.Scale(new Vector3(1, -1, 1)), a.GetMatrix(a.ReflectY), 1e-6);
        }
        [Test]
        public void TestTryGetRotation()
        {
            GridTest.TryGetRotation(HexCellType.Get(HexOrientation.PointyTopped));
            GridTest.TryGetRotation(HexCellType.Get(HexOrientation.FlatTopped));
        }
    }
}
