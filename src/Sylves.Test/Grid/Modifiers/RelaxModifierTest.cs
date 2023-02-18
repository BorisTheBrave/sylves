﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sylves.Test
{
    [TestFixture]
    public class RelaxModifierTest
    {
        [Test]
        public void TestFindNearbyHexes()
        {
            var v = new Vector3(-11.883191f, -5.9979167f, 0) / 4;
            var g = new HexGrid(1);


            var hex = new Cell(-2, -2, 4);
            var hexAndNeighbours = new[] { hex }.Concat(g.GetNeighbours(hex)).ToList();
            Assert.AreEqual(hex, g.FindCell(v));

            var nearby = RelaxModifier.FindNearbyHexes(v);

            var actual = new[] {nearby.Hex1, nearby.Hex2, nearby.Hex3};
            var expected = hexAndNeighbours.OrderBy(x => (g.GetCellCenter(x) - v).magnitude).Take(3).ToList();

            CollectionAssert.AreEquivalent(expected, actual);

            Assert.IsTrue(0 <= nearby.Weight1 && nearby.Weight1 <= 1);
            Assert.IsTrue(0 <= nearby.Weight2 && nearby.Weight2 <= 1);
            Assert.IsTrue(0 <= nearby.Weight3 && nearby.Weight3 <= 1);

            Assert.AreEqual(1, nearby.Weight1 + nearby.Weight2 + nearby.Weight3, 1e-2);
        }
    }
}