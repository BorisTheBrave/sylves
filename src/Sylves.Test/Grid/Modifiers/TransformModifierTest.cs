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
    }
}
