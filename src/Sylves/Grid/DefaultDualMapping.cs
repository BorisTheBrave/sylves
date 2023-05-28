using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY
using UnityEngine;
#endif


namespace Sylves
{
    // Converts any planar grid into a dual mapping.
    // It does this lazily in chungs, so it supports infinite grids
    internal class DefaultDualMapping : PlanarLazyGrid, IDualMapping
    {
        private IGrid baseGrid;

        private Func<Vector2Int, IEnumerable<Cell>> getCellsByChunk;

        // Stores the mesh data, and also some details of the mapping
        private IDictionary<Cell, (MeshData meshData, List<(Cell primalCell, CellCorner primalCorner, Cell dualCell, CellCorner dualCorner)> mapping)> meshDatas;
        private IDictionary<Cell, Dictionary<(Cell, CellCorner), (Cell, CellCorner)>> toDual;
        private IDictionary<Cell, Dictionary<(Cell, CellCorner), (Cell, CellCorner)>> toBase;


        public DefaultDualMapping(IGrid planarGrid, float chunkSize, ICachePolicy cachePolicy)
        {
            if (!planarGrid.IsPlanar)
            {
                throw new ArgumentException("Grid should be planar");
            }
            if (!planarGrid.Is2d)
            {
                throw new ArgumentException("Grid should be 2d");
            }
            var squareGrid = new SquareGrid(chunkSize);
            IEnumerable<Cell> GetCellsByChunk(Vector2Int chunk)
            {
                var min = new Vector3((chunk.x - 0.5f) * chunkSize, (chunk.y - 0.5f) * chunkSize, 0);
                var max = min + new Vector3(chunkSize, chunkSize, 0);
                return planarGrid.GetCellsIntersectsApprox(min, max)
                    .Where(cell =>
                    {
                        var center = planarGrid.GetCellCenter(cell);
                        return min.x <= cell.x && cell.x < max.x && min.y <= cell.y && cell.y < max.y;
                    } );
            }
            getCellsByChunk = GetCellsByChunk;
            MakeCaches(cachePolicy);

            Setup(squareGrid, chunkSize / 2, cachePolicy: cachePolicy);
        }

        public DefaultDualMapping(PlanarLazyGrid grid, ICachePolicy cachePolicy)
        {
            baseGrid = grid;
            getCellsByChunk = chunk => grid.GetCellsInBounds(new SquareBound(chunk, chunk + Vector2Int.one));
            MakeCaches(cachePolicy);

            // Dual cells are put in a chunk based off a primal cell's chunk,
            // so this bound always works.
            SquareBound bound = grid.GetBound() as SquareBound;

            base.Setup(grid.StrideX, grid.StrideY, grid.AabbBottomLeft - grid.AabbSize * 0.5f, grid.AabbSize * 2, bound, cachePolicy: cachePolicy);
        }

        private DefaultDualMapping(DefaultDualMapping other, SquareBound bound):base(other, bound)
        {
            baseGrid = other.baseGrid;
            getCellsByChunk = other.getCellsByChunk;
            meshDatas = other.meshDatas;
        }


        private static Vector2 ToVector2(Vector3 v) => new Vector2(v.x, v.y);

        private void MakeCaches(ICachePolicy cachePolicy)
        {
            if (cachePolicy == null)
                cachePolicy = CachePolicy.Always;
            meshDatas = cachePolicy.GetDictionary<(MeshData meshData, List<(Cell primalCell, CellCorner primalCorner, Cell dualCell, CellCorner dualCorner)> mapping)>(this);
            toDual = cachePolicy.GetDictionary<Dictionary<(Cell, CellCorner), (Cell, CellCorner)>>(this);
            toBase = cachePolicy.GetDictionary<Dictionary<(Cell, CellCorner), (Cell, CellCorner)>>(this);

        }

        private void Setup(SquareGrid chunkGrid, float margin = 0.0f, SquareBound bound = null, IEnumerable<ICellType> cellTypes = null, ICachePolicy cachePolicy = null)
        {
            // Work out the dimensions of the chunk grid
            var strideX = ToVector2(chunkGrid.GetCellCenter(new Cell(1, 0)));
            var strideY = ToVector2(chunkGrid.GetCellCenter(new Cell(0, 1)));

            var polygon = chunkGrid.GetPolygon(new Cell()).Select(ToVector2);
            var aabbBottomLeft = polygon.Aggregate(Vector2.Min);
            var aabbTopRight = polygon.Aggregate(Vector2.Max);
            var aabbSize = aabbTopRight - aabbBottomLeft;


            base.Setup(strideX, strideY, aabbBottomLeft - margin * Vector2.one, aabbSize + 2 * margin * Vector2.one, bound, cellTypes, cachePolicy);
        }

        private static T GetOrAdd<T>(IDictionary<Cell, T> cache, Vector2Int v, Func<T> func)
        {
            var cell = new Cell(v.x, v.y);
            if (cache.TryGetValue(cell, out var x))
            {
                return x;
            }
            return cache[cell] = func();

        }

        // This conversion is only safe for 2d cells, as they have a specific convention that
        // dir n is between corners n and (n+1)
        private CellDir CornerToDir(CellCorner corner) => (CellDir)corner;

        // This conversion is only safe for 2d cells, as they have a specific convention that
        // dir n is between corners n and (n+1)
        private CellCorner DirToCorner(CellDir dir) => (CellCorner)dir;

        // We directly provide a mesh grid, rather than a MeshData (the usualy way to use PlanarLazyMeshGrid)
        // That's because we don't want to do edge matching between meshes - we already know that data due to the 
        // dual structure
        private (MeshData meshData, List<(Cell primalCell, CellCorner primalCorner, Cell dualCell, CellCorner dualCorner)> mapping) GetMeshData(Vector2Int v)
        {
            var visited = new HashSet<(Cell cell, CellDir dir)>();
            var mapping = new List<(Cell primalCell, CellCorner primalCorner, Cell dualCell, CellCorner dualCorner)>();
            var dualCellCount = 0;
            var arcEnds = new List<((Cell cell, CellDir dir) startHe, (Cell cell, CellDir dir) endHe)?>();


            var primalCells = getCellsByChunk(v);
            foreach(var primalCell in primalCells)
            {
                foreach(var primalCorner in baseGrid.GetCellCorners(primalCell))
                {
                    // Find the dual cell on corner by walking around the corner
                    var dualCell = Combine(new Cell(dualCellCount, 0), v);

                    var startHe = (cell: primalCell, dir: CornerToDir(primalCorner));
                    // Skip if we've already explored this arc/loop
                    if (visited.Contains(startHe))
                    {
                        continue;
                    }

                    var currentHe = startHe;
                    (Cell cell, CellDir dir) endHe = default;
                    var dualCorner = 0;
                    var dualCorner2 = 0;
                    var minChunk = Split(currentHe.cell).chunk;
                    var oldMappingCount = mapping.Count;
                    bool isArc = false;
                    while(true)
                    {
                        visited.Add(currentHe);
                        mapping.Add((currentHe.cell, DirToCorner(currentHe.dir), dualCell, (CellCorner)dualCorner));
                        minChunk = LexMin(minChunk, Split(currentHe.cell).chunk);
                        dualCorner++;

                        currentHe = PrevHalfEdge(currentHe);
                        var nextHe = Flip(currentHe);
                        if (nextHe == null)
                        {
                            isArc = true;
                            endHe = currentHe;
                            break;
                        }
                        currentHe = nextHe.Value;
                        if (currentHe == startHe)
                        {
                            break;
                        }
                    }
                    if(isArc)
                    {
                        // Not a full loop. So we need to work *backwards* from startHe, too
                        currentHe = startHe;
                        while(true)
                        {
                            var nextHe = Flip(currentHe);
                            if(nextHe == null)
                            {
                                break;
                            }
                            currentHe = NextHalfEdge(currentHe);

                            visited.Add(currentHe);
                            mapping.Add((currentHe.cell, DirToCorner(currentHe.dir), Combine(new Cell(dualCellCount, 0), v), (CellCorner)dualCorner2));
                            minChunk = LexMin(minChunk, Split(currentHe.cell).chunk);
                            dualCorner2--;
                        }
                    }

                    // If a dual cell borders several chunks, 
                    // It'll be visited multiple times.
                    // This ensures we keep only a unique one
                    var keepDualCell = minChunk == v;
                    if (keepDualCell)
                    {
                        dualCellCount++;
                        if (isArc)
                        {
                            arcEnds.Add((currentHe, endHe));
                        }
                        else
                        {
                            arcEnds.Add(null);
                        }

                        if(dualCorner2 < 0)
                        {
                            // Mapping has some negative indices in it, fix them up
                            var totalCorners = (dualCorner - dualCorner2);
                            for(var i=oldMappingCount + dualCorner;i<mapping.Count;i++)
                            {
                                mapping[i] = (mapping[i].primalCell, mapping[i].primalCorner, mapping[i].dualCell, mapping[i].dualCorner + totalCorners);
                            }
                        }
                    }
                    else
                    {
                        // Revert changes to mapping
                        mapping.RemoveRange(oldMappingCount, mapping.Count - oldMappingCount);
                    }
                }
            }

            // At this point, we have mapping which fully gives the details of the dual cells
            // We can build a mesh from those mappings


            // Make a vertex for every primal cell mentioned in mappings (this includes cells not in current chunk)
            var allPrimalCells = mapping.Select(x => x.primalCell).Distinct().ToList();
            var vertices = new List<Vector3>(allPrimalCells.Count);
            var primalCellToVertex = new Dictionary<Cell, int>();
            for(var i=0;i<allPrimalCells.Count;i++)
            {
                var p = baseGrid.GetCellCenter(allPrimalCells[i]);
                vertices.Add(p);
                primalCellToVertex[allPrimalCells[i]] = i;
            }

            // For each dual cell, make a face
            var indices = new List<int>();
            // TODO: Mapping is contiguous, we don't really need this group by or order by
            foreach(var mappingGroup in mapping.GroupBy(x=>x.dualCell))
            {
                foreach(var item in mappingGroup.OrderBy(x=>x.dualCorner))
                {
                    indices.Add(primalCellToVertex[item.primalCell]);
                }
                var arcEnd = arcEnds[mappingGroup.Key.x];
                if(arcEnd != null)
                {
                    // Add some extra vertices to terminate the arc
                    void AddArcPoint((Cell cell, CellDir dir) he)
                    {
                        var corner1 = DirToCorner(he.dir);
                        var corner2 = NextCorner(he.cell, corner1);

                        var p = (baseGrid.GetCellCorner(he.cell, corner1) + baseGrid.GetCellCorner(he.cell, corner2)) / 2;
                        // TODO: Extend to infinity like in DualMeshBuilder?
                        indices.Add(vertices.Count);
                        vertices.Add(p);
                    }
                    AddArcPoint(arcEnd.Value.endHe);
                    AddArcPoint(arcEnd.Value.startHe);
                }
                indices[indices.Count - 1] = ~indices[indices.Count - 1];
            }

            var meshData = new MeshData
            {
                vertices = vertices.ToArray(),
                indices = new[] { indices.ToArray() },
                topologies = new[] { MeshTopology.NGon },
            };

            return (meshData, mapping);
        }

        private (MeshData meshData, List<(Cell primalCell, CellCorner primalCorner, Cell dualCell, CellCorner dualCorner)> mapping) GetMeshDataCached(Vector2Int v)
        {
            var cell = new Cell(v.x, v.y);
            if(meshDatas.TryGetValue(cell, out var x))
            {
                return x;
            }
            return meshDatas[cell] = GetMeshData(v);
        }

        protected override MeshGrid GetMeshGrid(Vector2Int v)
        {
            var meshData = GetMeshDataCached(v).meshData;
            // TODO: Disable building Moves, we won't be using it.
            return new MeshGrid(meshData);
        }


        private (Cell cell, CellDir dir)? Flip((Cell cell, CellDir dir) halfEdge)
        {
            if (baseGrid.TryMove(halfEdge.cell, halfEdge.dir, out var destCell, out var inverseDir, out var connection))
            {
                if (connection != new Connection())
                    throw new Exception("Cannot handle non-trivial connection");
                return (destCell, inverseDir);
            }
            else
            {
                return null;
            }
        }

        // Moves one edge around a face in primal mesh
        private (Cell cell, CellDir dir) NextHalfEdge((Cell cell, CellDir dir) halfEdge)
        {
            var cellType = baseGrid.GetCellType(halfEdge.cell);
            var l = cellType.Rotate(halfEdge.dir, cellType.RotateCCW);
            return (halfEdge.cell, l);
        }
        private (Cell cell, CellDir dir) PrevHalfEdge((Cell cell, CellDir dir) halfEdge)
        {
            var cellType = baseGrid.GetCellType(halfEdge.cell);
            var l = cellType.Rotate(halfEdge.dir, cellType.RotateCW);
            return (halfEdge.cell, l);
        }

        private CellCorner NextCorner(Cell cell, CellCorner corner)
        {
            var cellType = baseGrid.GetCellType(cell);
            return cellType.Rotate(corner, cellType.RotateCCW);
        }

        private CellCorner PrevCorner(Cell cell, CellCorner corner)
        {
            var cellType = baseGrid.GetCellType(cell);
            return cellType.Rotate(corner, cellType.RotateCW);
        }

        private static Vector2Int LexMin(Vector2Int a, Vector2Int b)
        {
            if (a.x < b.x) return a;
            if (a.x > b.x) return b;
            if (a.y < b.y) return a;
            return b;
        }

        public override bool TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection)
        {
            // TODO: Cache this

            // Each move in the dual grid
            // corresponds to moving from one corner to another of a particular primal cell.
            // TODO: We have to check two different primal cells, to account for the boundary.

            // Try first corner
            var corner = DirToCorner(dir);
            var t = ToBasePair(cell, corner);
            if (t != null)
            {
                var (primalCell, primalCorner) = t.Value;
                primalCorner = PrevCorner(primalCell, primalCorner);
                var t2 = ToDualPair(primalCell, primalCorner);
                if (t2 != null)
                {
                    dest = t2.Value.dualCell;
                    inverseDir = CornerToDir(t2.Value.inverseCorner);
                    connection = new Connection();
                    return true;
                }
            }

            // Try other corner
            var cellType = GetCellType(cell);
            corner = cellType.Rotate(corner, cellType.RotateCW);
            t = ToBasePair(cell, corner);
            if (t != null)
            {
                var (primalCell, primalCorner) = t.Value;
                primalCorner = NextCorner(primalCell, primalCorner);
                var t2 = ToDualPair(primalCell, primalCorner);
                if (t2 != null)
                {
                    dest = t2.Value.dualCell;
                    inverseDir = CornerToDir(t2.Value.inverseCorner);
                    connection = new Connection();
                    return true;
                }
            }

            dest = default;
            inverseDir = default;
            connection = default;
            return false;
        }



        public override IGrid Unbounded => new DefaultDualMapping(this, null);



        public override IGrid BoundBy(IBound bound) => new DefaultDualMapping(this, (SquareBound)bound);

        #region IDualMapping implementation

        public IGrid BaseGrid => baseGrid;

        public IGrid DualGrid => this;

        public (Cell baseCell, CellCorner inverseCorner)? ToBasePair(Cell dualCell, CellCorner corner)
        {
            var chunk = Split(dualCell).chunk;
            var dict = GetOrAdd(toBase, chunk, () => GetMeshDataCached(chunk)
                .mapping
                .ToDictionary(x => (x.dualCell, x.dualCorner), x => (x.primalCell, x.primalCorner)));

            if(dict.TryGetValue((dualCell, corner), out var t))
            {
                return t;
            }
            return null;
        }

        public (Cell dualCell, CellCorner inverseCorner)? ToDualPair(Cell baseCell, CellCorner corner)
        {

            var chunk = Split(baseCell).chunk;
            // Because we only have mappings per-*dual*-chunk, we need to aggregate several mappings
            var dict = GetOrAdd(toDual, chunk, () => GetAdjacentChunks(chunk)
                .SelectMany(c=> GetMeshDataCached(c).mapping)
                .ToDictionary(x => (x.primalCell, x.primalCorner), x => (x.dualCell, x.dualCorner)));

            if (dict.TryGetValue((baseCell, corner), out var t))
            {
                return t;
            }
            return null;
        }
        #endregion
    }
}
