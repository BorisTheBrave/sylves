using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sylves;

namespace Sylves.Test
{
    [TestFixture]
    public class MiscTests
    {
        class TileAndRotation
        {
            public int tileIndex;
            public CellRotation rotation;
        }

        [Test]
        public void TestMCRotation()
        {

            // Setup
            var cellType = SquareCellType.Instance;
            var tileIndexCount = (int)Math.Pow(cellType.GetCellCorners().Count(), 2); // 16 for squares
            var rotationTable = new TileAndRotation[tileIndexCount];

            // Some utility methods, using the bitwise trick
            IList<CellCorner> FromTileIndex(int tileIndex)
            {
                var result = new List<CellCorner>();
                foreach (var cellCorner in cellType.GetCellCorners())
                {
                    if ((tileIndex & (1 << (int)cellCorner)) > 0)
                    {
                        result.Add(cellCorner);
                    }
                }
                return result;
            }

            int ToTileIndex(IEnumerable<CellCorner> corners)
            {
                int tileIndex = 0;
                foreach (var corner in corners)
                {
                    tileIndex += (1 << (int)corner);
                }
                return tileIndex;
            }

            // For each tile index, see if we can find something else it can rotate from
            for (var tileIndex = 0; tileIndex < tileIndexCount; tileIndex++)
            {
                var corners = FromTileIndex(tileIndex);
                TileAndRotation best = null;
                // Try each rotation
                foreach (var rotation in cellType.GetRotations(includeReflections: false))
                {
                    // Rotate all the corners by rotation
                    var rotatedCorners = corners.Select(corner => cellType.Rotate(corner, rotation));
                    var rotatedIndex = ToTileIndex(rotatedCorners);
                    // Is this a better choice?
                    if (rotatedIndex < tileIndex && (best == null || rotatedIndex < best.tileIndex))
                    {
                        best = new TileAndRotation
                        {
                            tileIndex = rotatedIndex,
                            rotation = cellType.Invert(rotation)
                        };
                    }
                }
                rotationTable[tileIndex] = best;
            }

        }
    }
}
