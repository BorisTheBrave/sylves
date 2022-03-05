using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sylves.Test
{
    [TestFixture]
    public class DefaultGridImplTest
    {
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
            Assert.AreEqual((CellRotation)SquareRotation.ReflectY, destRotation);


            success = DefaultGridImpl.ParallelTransport(sg, new Cell(0, 0), new Cell(10, 10), sg, new Cell(20, 0), SquareRotation.ReflectY, out destCell, out destRotation);
            Assert.IsTrue(success);
            Assert.AreEqual(new Cell(30, -10), destCell);
            Assert.AreEqual((CellRotation)SquareRotation.ReflectY, destRotation);

            success = DefaultGridImpl.ParallelTransport(sg, new Cell(0, 0), new Cell(10, 10), sg, new Cell(20, 0), SquareRotation.ReflectX, out destCell, out destRotation);
            Assert.IsTrue(success);
            Assert.AreEqual(new Cell(10, 10), destCell);
            Assert.AreEqual((CellRotation)SquareRotation.ReflectX, destRotation);
        }
    }
}
