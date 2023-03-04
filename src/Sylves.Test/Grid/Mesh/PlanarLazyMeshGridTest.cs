using NUnit.Framework;
#if UNITY
using UnityEngine;
#endif


namespace Sylves.Test
{
    [TestFixture]
    public class PlanarLazyMeshGridTest
    {
        [Test]
        public void TestPlanarLazyMeshGrid()
        {
            var g = new PlanarLazyMeshGrid(
                chunk => Matrix4x4.Translate(new Vector3(chunk.x, chunk.y, 0)) * TestMeshes.PlaneXY,
                Vector2.right,
                Vector2.up,
                new Vector2(-.5f, -.5f),
                Vector2.one
                );

            Assert.AreEqual(new Cell(0, 1, 0), g.Move(new Cell(0, 0, 0), (CellDir)SquareDir.Right));
            Assert.AreEqual(new Cell(0, 0, 1), g.Move(new Cell(0, 0, 0), (CellDir)SquareDir.Up));
            Assert.AreEqual(new Cell(0, 0, 2), g.Move(new Cell(0, 0, 1), (CellDir)SquareDir.Up));
        }
    }
}
