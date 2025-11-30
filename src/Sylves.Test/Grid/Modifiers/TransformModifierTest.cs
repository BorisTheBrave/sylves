using NUnit.Framework;
#if UNITY
using UnityEngine;
#endif


namespace Sylves.Test
{
    [TestFixture]
    internal class TransformModifierTest
    {


        [Test]
        public void TestFindCell()
        {
            var g = new TransformModifier(new SquareGrid(1), Matrix4x4.TRS(Vector3.right * 100, Quaternion.Euler(0, 90, 0), Vector3.one * 2));
            GridTest.FindCell(g, new Cell());
        }

        [Test]
        public void TestCombineTransforms()
        {
            Matrix4x4 tf1 = Matrix4x4.Scale(Vector3.one * 2);
            Matrix4x4 tf2 = Matrix4x4.Translate(Vector3.right * 5);
            IGrid g = new HexGrid(1);
            g = g.Transformed(tf1).Transformed(tf2);

            Assert.IsTrue(g is TransformModifier);
            Assert.IsTrue((g as TransformModifier).Underlying is HexGrid);
            var combinedTf = tf2 * tf1;
            Assert.AreEqual(new Vector3(5, 0, 0), g.GetCellCenter(new Cell()));
        }
    }
}
