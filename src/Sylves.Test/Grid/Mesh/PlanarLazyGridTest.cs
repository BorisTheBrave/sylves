﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sylves.Test
{
    [TestFixture]
    public class PlanarLazyGridTest
    {
        [Test]
        public void TestPlanarLazyGrid()
        {
            var g = new PlanarLazyGrid(
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
