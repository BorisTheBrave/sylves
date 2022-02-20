using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sylves;

namespace Sylves.Test
{
    // All these tests have been ported from Tessera. They should be moved somewhere move appropriate later
    internal class MiscTests
    {
        /*
            // Checks some things vs the default implementation
            void CheckCellType(ICellType cellType)
            {
                foreach (var r in cellType.GetRotations())
                {
                    var b1 = cellType.TryMoveByOffset(Vector3Int.zero, Vector3Int.zero, Vector3Int.right, r, out var dest1);
                    var b2 = DefaultCellTypeImpl.TryMoveByOffset(cellType, Vector3Int.zero, Vector3Int.zero, Vector3Int.right, r, out var dest2);
                    Assert.AreEqual(b1, b2, $"Failed TryMoveByOffset result for rotation {r}");
                    Assert.AreEqual(dest1, dest2, $"Failed TryMoveByOffset dest for rotation {r}");
                }

                foreach (var r in cellType.GetRotations())
                {
                    if ((int)r == 2)
                    {
                        cellType = cellType;
                    }
                    var shape = new HashSet<Vector3Int> { new Vector3Int(4, 0, 0), new Vector3Int(5, 0, 0) };
                    var d1 = cellType.Realign(shape, r);
                    var d2 = DefaultCellTypeImpl.Realign(cellType, shape, r);
                    var e1 = d1?.Select(kv => (kv.Key.x, kv.Key.y, kv.Key.z, kv.Value.x, kv.Value.y, kv.Value.z)).OrderBy(x => x).ToList();
                    var e2 = d2?.Select(kv => (kv.Key.x, kv.Key.y, kv.Key.z, kv.Value.x, kv.Value.y, kv.Value.z)).OrderBy(x => x).ToList();
                    Assert.AreEqual(e1 == null, e2 == null, $"Failed for rotation {r}");
                    if (e1 != null)
                        CollectionAssert.AreEquivalent(e1, e2, $"Failed for rotation {r}");
                }
            }

            [Test]
            public void TestCubeCellType()
            {
                Assert.AreEqual(CubeFaceDir.Back, CubeRotation.RotateXZ * CubeFaceDir.Right);
                Assert.AreEqual(CubeFaceDir.Down, CubeRotation.RotateXY * CubeFaceDir.Right);
                Assert.AreEqual(CubeFaceDir.Up, CubeRotation.RotateYZ * CubeFaceDir.Forward);
                Assert.AreEqual(CubeFaceDir.Back, CubeRotation.RotateXZ * CubeFaceDir.Right);
                Assert.AreEqual(CubeFaceDir.Left, CubeRotation.ReflectX * CubeFaceDir.Right);

                Assert.AreEqual(CubeRotation.Identity, CubeRotation.RotateXY * CubeRotation.RotateXY * CubeRotation.RotateXY * CubeRotation.RotateXY);

                Assert.AreEqual(CubeRotation.ReflectX * (CubeRotation.RotateXY * CubeRotation.ReflectX), (CubeRotation.ReflectX * CubeRotation.RotateXY) * CubeRotation.ReflectX);

                CheckCellType(CubeCellType.Instance);
            }

            private static FaceDetails MakeFaceDetails(int topLeft, int top, int topRight, int left, int center, int right, int bottomLeft, int bottom, int bottomRight)
            {
                return new FaceDetails { topLeft = topLeft, top = top, topRight = topRight, left = left, center = center, right = right, bottomLeft = bottomLeft, bottom = bottom, bottomRight = bottomRight };
            }

            private void Check(CubeFaceDir expectedFaceDir, FaceDetails expectedFaceDetails, (CellFaceDir, FaceDetails) actual)
            {
                Assert.AreEqual(expectedFaceDir, (CubeFaceDir)actual.Item1);
                var actualFaceDetails = actual.Item2;
                Assert.AreEqual(expectedFaceDetails.topLeft, actualFaceDetails.topLeft);
                Assert.AreEqual(expectedFaceDetails.top, actualFaceDetails.top);
                Assert.AreEqual(expectedFaceDetails.topRight, actualFaceDetails.topRight);
                Assert.AreEqual(expectedFaceDetails.left, actualFaceDetails.left);
                Assert.AreEqual(expectedFaceDetails.center, actualFaceDetails.center);
                Assert.AreEqual(expectedFaceDetails.right, actualFaceDetails.right);
                Assert.AreEqual(expectedFaceDetails.bottomLeft, actualFaceDetails.bottomLeft);
                Assert.AreEqual(expectedFaceDetails.bottom, actualFaceDetails.bottom);
                Assert.AreEqual(expectedFaceDetails.bottomRight, actualFaceDetails.bottomRight);
            }

            [Test]
            public void TestCubeRotateBy()
            {
                Check(CubeFaceDir.Forward, MakeFaceDetails(1, 2, 3, 4, 5, 6, 7, 8, 9),
                    CubeCellType.Instance.RotateBy((CellFaceDir)CubeFaceDir.Forward, MakeFaceDetails(1, 2, 3, 4, 5, 6, 7, 8, 9), CubeRotation.Identity));

                Check(CubeFaceDir.Forward, MakeFaceDetails(3, 6, 9, 2, 5, 8, 1, 4, 7),
                    CubeCellType.Instance.RotateBy((CellFaceDir)CubeFaceDir.Forward, MakeFaceDetails(1, 2, 3, 4, 5, 6, 7, 8, 9), CubeRotation.RotateXY));

                Check(CubeFaceDir.Right, MakeFaceDetails(1, 2, 3, 4, 5, 6, 7, 8, 9),
                    CubeCellType.Instance.RotateBy((CellFaceDir)CubeFaceDir.Forward, MakeFaceDetails(1, 2, 3, 4, 5, 6, 7, 8, 9), CubeRotation.RotateXZ));

                Check(CubeFaceDir.Forward, MakeFaceDetails(3, 2, 1, 6, 5, 4, 9, 8, 7),
                    CubeCellType.Instance.RotateBy((CellFaceDir)CubeFaceDir.Forward, MakeFaceDetails(1, 2, 3, 4, 5, 6, 7, 8, 9), CubeRotation.ReflectX));

                Check(CubeFaceDir.Forward, MakeFaceDetails(7, 8, 9, 4, 5, 6, 1, 2, 3),
                    CubeCellType.Instance.RotateBy((CellFaceDir)CubeFaceDir.Forward, MakeFaceDetails(1, 2, 3, 4, 5, 6, 7, 8, 9), CubeRotation.ReflectY));

                Check(CubeFaceDir.Back, MakeFaceDetails(3, 2, 1, 6, 5, 4, 9, 8, 7),
                    CubeCellType.Instance.RotateBy((CellFaceDir)CubeFaceDir.Forward, MakeFaceDetails(1, 2, 3, 4, 5, 6, 7, 8, 9), CubeRotation.ReflectZ));

                Check(CubeFaceDir.Down, MakeFaceDetails(15, 0, 1, 7, 7, 1, 1, 1, 1),
                    CubeCellType.Instance.RotateBy((CellFaceDir)CubeFaceDir.Forward, MakeFaceDetails(15, 7, 1, 0, 7, 1, 1, 1, 1), (CellRotation)521));

                Check(CubeFaceDir.Up, MakeFaceDetails(15, 7, 1, 7, 7, 1, 1, 1, 1),
                    CubeCellType.Instance.RotateBy((CellFaceDir)CubeFaceDir.Down, MakeFaceDetails(1, 1, 1, 7, 7, 1, 15, 7, 1), (CellRotation)656));
            }

            [Test]
            public void TestHexCellType()
            {
                Assert.AreEqual(HexPrismFaceDir.ForwardRight, HexRotation.RotateCCW * HexPrismFaceDir.Right);
                Assert.AreEqual(HexPrismFaceDir.Left, HexRotation.ReflectX * HexPrismFaceDir.Right);

                Assert.AreEqual(HexRotation.Identity, HexRotation.RotateCCW * HexRotation.RotateCCW * HexRotation.RotateCCW * HexRotation.RotateCCW * HexRotation.RotateCCW * HexRotation.RotateCCW);
                Assert.AreEqual(HexRotation.ReflectX * (HexRotation.RotateCCW * HexRotation.ReflectX), (HexRotation.ReflectX * HexRotation.RotateCCW) * HexRotation.ReflectX);

                CheckCellType(HexPrismCellType.Instance);
            }

            [Test]
            public void TestTriangleCellType()
            {
                Assert.AreEqual(TriangleRotation.RotateCCW, TriangleRotation.RotateCCW60(2));

                Assert.AreEqual(TrianglePrismFaceDir.ForwardRight, TriangleRotation.RotateCCW * TrianglePrismFaceDir.Back);
                Assert.AreEqual(TrianglePrismFaceDir.BackRight, TriangleRotation.RotateCCW60(1) * TrianglePrismFaceDir.Back);
                Assert.AreEqual(TrianglePrismFaceDir.ForwardRight, TriangleRotation.ReflectX * TrianglePrismFaceDir.ForwardLeft);


                Assert.AreEqual(TriangleRotation.Identity, TriangleRotation.RotateCCW * TriangleRotation.RotateCCW * TriangleRotation.RotateCCW);

                var r1 = TriangleRotation.RotateCCW;
                Assert.AreEqual(false, r1.IsReflection);
                Assert.AreEqual(2, r1.Rotation);
                var r2 = TriangleRotation.RotateCCW * TriangleRotation.ReflectX;
                Assert.AreEqual(true, r2.IsReflection);
                Assert.AreEqual(2, r2.Rotation);

                CheckCellType(TrianglePrismCellType.Instance);
            }



            [Test]
            public void TestSquareCellType()
            {
                Assert.AreEqual(SquareRotation.RotateCCW, SquareRotation.Rotate90(1));

                Assert.AreEqual(SquareFaceDir.Up, SquareRotation.RotateCCW * SquareFaceDir.Right);
                Assert.AreEqual(SquareFaceDir.Left, SquareRotation.RotateCCW * SquareFaceDir.Up);
                Assert.AreEqual(SquareFaceDir.Left, SquareRotation.ReflectX * SquareFaceDir.Right);
                Assert.AreEqual(SquareFaceDir.Up, SquareRotation.ReflectX * SquareFaceDir.Up);

                Assert.AreEqual(SquareFaceDir.Right, (SquareRotation.ReflectX * SquareRotation.RotateCCW) * SquareFaceDir.Up);

                Assert.AreEqual(SquareRotation.Identity, SquareRotation.RotateCCW * SquareRotation.RotateCCW * SquareRotation.RotateCCW * SquareRotation.RotateCCW);

                CheckCellType(SquareCellType.Instance);
            }


            [Test]
            public void TestCubeGrid()
            {
                //var grid = new CubeGrid();
            }

            [Test]
            public void TestAsdf()
            {
                Assert.AreEqual(Vector3.zero, TrianglePrismGeometryUtils.GetCellCenter(Vector3Int.zero, Vector3.zero, new Vector3(1, 1, 1)));
            }
        */
    }
}
