using NUnit.Framework;
#if UNITY
using UnityEngine;
#endif

namespace Sylves.Test
{
    [TestFixture]
    public class DefaultGridImplTest
    {
        [Test]
        public void TestParallelTransport()
        {
            var sg = new SquareGrid(1);
            var success = DefaultGridImpl.ParallelTransport(sg, new Cell(0, 0), new Cell(10, 10), sg, new Cell(20, 0), SquareRotation.Identity, out var destCell, out var destRotation);
            Assert.IsTrue(success);
            Assert.AreEqual(new Cell(30, 10), destCell);
            Assert.AreEqual((CellRotation)SquareRotation.Identity, destRotation);


            success = DefaultGridImpl.ParallelTransport(sg, new Cell(0, 0), new Cell(10, 10), sg, new Cell(20, 0), SquareRotation.RotateCCW, out destCell, out destRotation);
            Assert.IsTrue(success);
            Assert.AreEqual(new Cell(10, 10), destCell);
            Assert.AreEqual((CellRotation)SquareRotation.RotateCCW, destRotation);


            success = DefaultGridImpl.ParallelTransport(sg, new Cell(0, 0), new Cell(10, 10), sg, new Cell(20, 0), SquareRotation.ReflectY, out destCell, out destRotation);
            Assert.IsTrue(success);
            Assert.AreEqual(new Cell(30, -10), destCell);
            Assert.AreEqual((CellRotation)SquareRotation.ReflectY, destRotation);

            success = DefaultGridImpl.ParallelTransport(sg, new Cell(0, 0), new Cell(10, 10), sg, new Cell(20, 0), SquareRotation.ReflectX, out destCell, out destRotation);
            Assert.IsTrue(success);
            Assert.AreEqual(new Cell(10, 10), destCell);
            Assert.AreEqual((CellRotation)SquareRotation.ReflectX, destRotation);

            // Mobius grid is a 2d grid that has a reflecting connection
            var mg = new MobiusSquareGrid(10, 10);

            success = DefaultGridImpl.ParallelTransport(sg, new Cell(0, 0), new Cell(1, 0), mg, new Cell(5, 5), SquareRotation.Identity, out destCell, out destRotation);
            Assert.IsTrue(success);
            Assert.AreEqual(new Cell(6, 5), destCell);
            Assert.AreEqual((CellRotation)SquareRotation.Identity, destRotation);

            success = DefaultGridImpl.ParallelTransport(sg, new Cell(0, 0), new Cell(0, 1), mg, new Cell(5, 5), SquareRotation.Identity, out destCell, out destRotation);
            Assert.IsTrue(success);
            Assert.AreEqual(new Cell(5, 6), destCell);
            Assert.AreEqual((CellRotation)SquareRotation.Identity, destRotation);

            // Crossing from x=9 to x=1 induces a reflection
            success = DefaultGridImpl.ParallelTransport(sg, new Cell(0, 0), new Cell(1, 0), mg, new Cell(9, 1), SquareRotation.Identity, out destCell, out destRotation);
            Assert.IsTrue(success);
            Assert.AreEqual(new Cell(0, 8), destCell);
            Assert.AreEqual((CellRotation)SquareRotation.ReflectY, destRotation);

            success = DefaultGridImpl.ParallelTransport(sg, new Cell(0, 0), new Cell(0, 1), mg, new Cell(9, 1), SquareRotation.RotateCW, out destCell, out destRotation);
            Assert.IsTrue(success);
            Assert.AreEqual(new Cell(0, 8), destCell);
            Assert.AreEqual((CellRotation)(SquareRotation.ReflectY * SquareRotation.RotateCW), destRotation);

            success = DefaultGridImpl.ParallelTransport(sg, new Cell(0, 0), new Cell(1, 1), mg, new Cell(9, 1), SquareRotation.Identity, out destCell, out destRotation);
            Assert.IsTrue(success);
            Assert.AreEqual(new Cell(0, 7), destCell);
            Assert.AreEqual((CellRotation)SquareRotation.ReflectY, destRotation);

            // TODO: Make tests that deal with non trivial inverseDirs
            // TODO: Make a test that works with rotating connecionts


        }
    }
}
