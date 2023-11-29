using NUnit.Framework;
using System.Linq;
#if UNITY
using UnityEngine;
#endif

namespace Sylves.Test
{
    internal class AabbGridTest
    {
        [Test]
        public void TestBoundedRaycast()
        {
            var chunks = new AabbChunks(new Vector2(1, 0), new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 1));
            var g = new AabbGrid(chunks, new SquareBound(0, 0, 10, 10));
            var cells = g.Raycast(new Vector3(.5f, .5f, 0), Vector3.right).Select(x => x.cell).ToList();
            Assert.AreEqual(10, cells.Count);
        }

        [Test]
        public void TestBoundedIntersects()
        {
            var chunks = new AabbChunks(new Vector2(1, 0), new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 1));
            var g = new AabbGrid(chunks, new SquareBound(0, 0, 10, 10));
            var cells = g.GetCellsIntersectsApprox(new Vector3(-10, -10, 0), new Vector3(15, 15, 0)).ToList();
            Assert.AreEqual(100, cells.Count);
        }
    }
}
