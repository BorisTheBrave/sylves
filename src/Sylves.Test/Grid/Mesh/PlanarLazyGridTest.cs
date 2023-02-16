using NUnit.Framework;
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

        private static Vector2 ToVector2(Vector3 v) => new Vector2(v.x, v.y);

        private static IList<T> RandomShuffle<T>(IList<T> list, Func<double> randomDouble)
        {
            for(var i=0;i<list.Count;i++)
            {
                var j = (int)(randomDouble() * (list.Count - i)) + i;
                // Swap i and j
                (list[i], list[j]) = (list[j], list[i]);
            }
            return list;
        }

        private static MeshData RandomPairing(MeshData md, Func<double> randomDouble)
        {
            var meshGrid = new MeshGrid(md);
            var cells = meshGrid.GetCells().ToList();
            var unpaired = cells.ToHashSet();
            var pairs = new List<(Cell, CellDir, Cell, CellDir)>();
            var pairDict = new Dictionary<Cell, (CellDir, Cell, CellDir)>();

            RandomShuffle(cells, randomDouble);
            foreach (var cell in cells)
            {
                if (!unpaired.Contains(cell))
                    continue;
                var dirs = RandomShuffle(meshGrid.GetCellDirs(cell).ToList(), randomDouble);
                foreach (var dir in dirs)
                {
                    if (!meshGrid.TryMove(cell, dir, out Cell dest, out CellDir inverseDir, out var _))
                        continue;
                    if (!unpaired.Contains(dest))
                        continue;
                    pairs.Add((cell, dir, dest, inverseDir));
                    pairDict.Add(cell, (dir, dest, inverseDir));
                    pairDict.Add(dest, (inverseDir, cell, dir));
                    unpaired.Remove(cell);
                    unpaired.Remove(dest);
                    break;
                }
            }

            // New mesh data with pairs of triangles merged
            var indices = new List<int>();
            foreach (var (cell, dir, dest, inverseDir) in pairs)
            {
                // TODO: Support non-triangles
                var f1 = meshGrid.GetFaceIndices(cell);
                var f2 = meshGrid.GetFaceIndices(dest);
                int i1, i2, i3, i4;
                switch ((int)dir)
                {
                    case 0:
                        (i1, i2, i3) = (f1[1], f1[2], f1[0]);
                        break;
                    case 1:
                        (i1, i2, i3) = (f1[2], f1[0], f1[1]);
                        break;
                    case 2:
                        (i1, i2, i3) = (f1[0], f1[1], f1[2]);
                        break;
                    default:
                        throw new Exception();
                }
                switch ((int)inverseDir)
                {
                    case 0:
                        i4 = f2[2];
                        break;
                    case 1:
                        i4 = f2[0];
                        break;
                    case 2:
                        i4 = f2[1];
                        break;
                    default:
                        throw new Exception();
                }
                indices.Add(i1);
                indices.Add(i2);
                indices.Add(i3);
                indices.Add(~i4);
            }
            foreach (var cell in unpaired)
            {
                indices.AddRange(meshGrid.GetFaceIndices(cell));
                indices[indices.Count - 1] = ~indices[indices.Count - 1];
            }


            var result = md.Clone();
            result.indices = new[] { indices.ToArray() };
            result.topologies = new[] { MeshTopology.NGon };

            return result;
        }

        private static MeshData Weld(MeshData md, float tol = 1e-7f)
        {
            // TODO: More efficient implementation with spatial hash
            // TODO: Average welded points?

            int weldCount = 0;
            var map = new int?[md.vertices.Length];
            var invMap = new int[md.vertices.Length];
            for (var i = 0; i < md.vertices.Length; ++i)
            {
                // Already welded
                if (map[i] != null)
                    continue;
                map[i] = weldCount;
                invMap[weldCount] = i;
                for (var j = i + 1; j < md.vertices.Length; ++j)
                {
                    if (Vector3.Distance(md.vertices[i], md.vertices[j]) < tol)
                    {
                        map[j] = weldCount;
                        invMap[weldCount] = j;
                    }
                }
                weldCount++;
            }
            var result = new MeshData();
            result.topologies = md.topologies;
            result.indices = md.indices.Select(ix => ix.Select(i => (i >= 0 ? map[i] : ~map[~i]).Value).ToArray()).ToArray();
            T[] MapArray<T>(T[] data)
            {
                if (data == null)
                    return null;
                var r = new T[weldCount];
                for(var i=0;i<weldCount;++i)
                {
                    r[i] = data[invMap[i]];
                }
                return r;
            }
            result.vertices = MapArray(md.vertices);
            result.uv = MapArray(md.uv);
            result.normals = MapArray(md.normals);
            result.tangents= MapArray(md.tangents);

            return result;
        }

        private static MeshData Relax(MeshData md, int iterations = 3)
        {
            var adjacencies = Enumerable.Range(0, md.vertices.Length).Select(x => new List<int>()).ToArray();
            foreach (var face in MeshUtils.GetFaces(md))
            {
                var p = face.Count - 1;
                for (var i = 0; i < face.Count; i++)
                {
                    var i1 = face[i];
                    var i2 = face[p];
                    adjacencies[i1].Add(i2);
                    adjacencies[i2].Add(i1);
                    p = i;
                }
            }
            var vertices = md.vertices;
            for (var i = 0; i < iterations; i++)
            {
                var nextVertices = new Vector3[vertices.Length];
                for (var j = 0; j < vertices.Length; j++)
                {
                    foreach(var neighbour in adjacencies[j])
                    {
                        nextVertices[j] += vertices[neighbour];
                    }
                    nextVertices[j] /= adjacencies[j].Count;
                }
                vertices = nextVertices;
            }
            var result = md.Clone();
            result.vertices = vertices;
            return result;
        }

        [Test]
        public void TestTownscaperLike()
        {
            // Each chunk corresponds to a single cell in this hex grid
            var n = 4;
            var chunkGrid = new HexGrid(n);
            Cell ChunkToCell(Vector2Int chunk) => new Cell(chunk.x, chunk.y, -chunk.x -chunk.y);

            MeshData GetMeshData(Vector2Int chunk)
            {
                var offset = chunkGrid.GetCellCenter(ChunkToCell(chunk));

                // Make a triangle grid that fills the chunk
                var triangleGrid = new TriangleGrid(0.5f, TriangleOrientation.FlatSides, bound: TriangleBound.Hexagon(n));
                var meshData = Matrix4x4.Translate(offset) * triangleGrid.ToMeshData();

                // Randomly pair the triangles of that grid
                var seed = chunk.x * 1000 + chunk.y;
                var random = new Random(seed);
                meshData = RandomPairing(meshData, random.NextDouble);

                // Split into quads
                meshData = ConwayOperators.Ortho(meshData);

                // Weld vertices
                meshData = Weld(meshData, 1e-1f);

                // Relax mesh
                //meshData = Relax(meshData, 1);


                return meshData;
            }


            // Work out the dimensions of the chunk grid, needed for PlanarLazyGrid
            var strideX = ToVector2(chunkGrid.GetCellCenter(ChunkToCell(new Vector2Int(1, 0))));
            var strideY = ToVector2(chunkGrid.GetCellCenter(ChunkToCell(new Vector2Int(0, 1))));

            var polygon = chunkGrid.GetPolygon(ChunkToCell(new Vector2Int())).Select(ToVector2);
            var aabbBottomLeft = polygon.Aggregate(Vector2.Min);
            var aabbTopRight = polygon.Aggregate(Vector2.Max);
            var aabbSize = aabbTopRight - aabbBottomLeft;


            var hexGrid = new HexGrid(1);
            var triGrid = hexGrid.GetChildTriangleGrid();


            var g = new PlanarLazyGrid(
                GetMeshData,
                strideX,
                strideY,
                aabbBottomLeft,
                aabbSize,
                bound: new SquareBound(-2, -2, 2, 2)
                );

            GridDocsExportTest.Export(g, "ts_grid.svg", new GridDocsExportTest.Options
            {
                textScale = null
            });
        }
    }
}
