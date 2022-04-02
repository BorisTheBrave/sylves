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
        }

        [Test]
        public void TestRotatingConnectionParallelTransport()
        {
            // Cubius grid is a 3d grid which has a rotating connection
            var cbg = new CubiusGrid(100, 2);
            var cg = new CubeGrid(1);
            var cell1 = new Cell(99, 0, 1);
            var cell2 = new Cell(0, 1, 1);
            var rotation = CubeRotation.RotateZY;
            // Check that cell1 connects to cell2
            var success = DefaultGridImpl.ParallelTransport(cg, new Cell(0, 0, 0), new Cell(1, 0, 0), cbg, cell1, CubeRotation.Identity, out var destCell, out var destRotation);
            Assert.IsTrue(success);
            Assert.AreEqual(cell2, destCell);
            Assert.AreEqual((CellRotation)rotation, destRotation);

            // Check that the geometry actually matches the topology

            // p2 should be the neartest of these cells to p1
            var p1 = cbg.GetCellCenter(cell1);
            var p2 = cbg.GetCellCenter(cell2);
            var a1 = cbg.GetCellCenter(new Cell(99, 0, 0));
            var b1 = cbg.GetCellCenter(new Cell(99, 0, 1));
            var c1 = cbg.GetCellCenter(new Cell(99, 1, 1));
            var d1 = cbg.GetCellCenter(new Cell(99, 1, 0));
            Assert.IsTrue(
                (p1 - p2).magnitude <= (a1 - p2).magnitude && 
                (p1 - p2).magnitude <= (b1 - p2).magnitude && 
                (p1 - p2).magnitude <= (c1 - p2).magnitude && 
                (p1 - p2).magnitude <= (d1 - p2).magnitude
                );

            // destRotation should match the actual rotation difference
            // This is only aproximage as there is some twisting between the cells
            var trs1 = cbg.GetTRS(cell1);
            var trs2 = cbg.GetTRS(cell2);
            var expectedRot = (trs2.ToMatrix() * trs1.ToMatrix().inverse);
            expectedRot.column3 = new Vector4(0, 0, 0, 1);
            var actualRot = ((CubeRotation)destRotation).ToMatrix();
            // TODO: Revisit this
            //TestUtils.AssertAreEqual(expectedRot, actualRot, 1e-6);

        }
    }
}
