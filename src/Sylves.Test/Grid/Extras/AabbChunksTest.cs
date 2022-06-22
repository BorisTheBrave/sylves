using NUnit.Framework;
using System.Linq;
#if UNITY
using UnityEngine;
#endif


namespace Sylves.Test
{
    [TestFixture]
    internal class AabbChunksTest
    {
        void CheckChunks(AabbChunks c, Vector2 min, Vector2 max)
        {
            var offsets = new Vector2Int[] { new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, -1) };
            var chunks = c.GetChunkIntersects(min, max).ToHashSet();
            Assert.IsNotEmpty(chunks);
            bool DoesIntersect(Vector2Int chunk)
            {
                var (cMin, cMax) = c.GetChunkBounds(chunk);
                return cMin.x <= max.x && cMin.y <= max.y && cMax.x >= min.x && cMax.y >= min.y;
            }
            foreach(var chunk in chunks)
            {
                Assert.IsTrue(DoesIntersect(chunk));
                foreach(var o in offsets)
                {
                    var c2 = o + chunk;
                    if (!chunks.Contains(c2))
                    {
                        Assert.IsFalse(DoesIntersect(c2));
                    }
                }
            }
        }

        [Test]
        public void TestChunks()
        {
            CheckChunks(new AabbChunks(new Vector2(1, 1), new Vector2(0, 1), Vector2.zero, Vector2.one), new Vector2(0, 0), new Vector2(10, 10));
            CheckChunks(new AabbChunks(new Vector2(3, 0), new Vector2(0, 1), Vector2.zero, Vector2.one), new Vector2(0, 0), new Vector2(10, 10));
            CheckChunks(new AabbChunks(new Vector2(1, 1), new Vector2(-1, 1), Vector2.zero, Vector2.one), new Vector2(0, 0), new Vector2(10, 10));
        }
    }
}
