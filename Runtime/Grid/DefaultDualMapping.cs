using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Sylves
{
    // Converts any planar grid into a dual mapping.
    // It does this lazily in chunks, so it supports infinite grids
    internal class DefaultDualMapping : PlanarLazyGrid, IDualMapping
    {
        private IGrid baseGrid;

        // Query methods on baseGrid

        // Get all the base cells in a given chunk
        private Func<Cell, IEnumerable<Cell>> getCellsByChunk;
        // Get the chunk from a base cell. Inverse of getCellsByChunk
        private Func<Cell, Cell> getChunkByCell;

        // Various caches

        // Stores the mesh data per chunk, and also some details of the mapping
        private IDictionary<Cell, (MeshData meshData, List<(Cell primalCell, CellCorner primalCorner, Cell dualCell, CellCorner dualCorner)> mapping)> meshDatas;
        // Stores the mapping from base (primal) to dual, per chunk
        private IDictionary<Cell, Dictionary<(Cell, CellCorner), (Cell, CellCorner)>> toDual;
        // Stores the mapping from dual to base (primal), per chunk
        private IDictionary<Cell, Dictionary<(Cell, CellCorner), (Cell, CellCorner)>> toBase;
        // Stores the cached moves
        private IDictionary<Cell, Dictionary<CellDir, (Cell, CellDir)>> moves;


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
            baseGrid = planarGrid;
            var squareGrid = new SquareGrid(chunkSize);
            IEnumerable<Cell> GetCellsByChunk(Cell chunk)
            {
                var min = new Vector3(chunk.x * chunkSize, chunk.y * chunkSize, 0);
                var max = min + new Vector3(chunkSize, chunkSize, 0);
                return planarGrid.GetCellsIntersectsApprox(min, max)
                    .Where(cell =>
                    {
                        var center = planarGrid.GetCellCenter(cell);
                        return
                          min.x <= center.x && center.x < max.x &&
                          min.y <= center.y && center.y < max.y;
                    });
            }
            getCellsByChunk = GetCellsByChunk;
            Cell GetChunkByCell(Cell cell)
            {
                var p = planarGrid.GetCellCenter(cell);
                return squareGrid.FindCell(p).Value;
            }
            getChunkByCell = GetChunkByCell;
            MakeCaches(cachePolicy);

            Setup(squareGrid, chunkSize / 2, cachePolicy: cachePolicy);
        }

        public DefaultDualMapping(PlanarLazyGrid grid, ICachePolicy cachePolicy)
        {
            baseGrid = grid;
            getCellsByChunk = chunk => grid.GetCellsInBounds(new SquareBound(chunk.x, chunk.y, chunk.x + 1, chunk.y + 1));
            getChunkByCell = cell => Split(cell).chunkCell;
            MakeCaches(cachePolicy);

            // Dual cells are put in a chunk based off a primal cell's chunk,
            // so this bound always works.
            SquareBound bound = grid.GetBound() as SquareBound;

            base.Setup(grid.StrideX, grid.StrideY, grid.AabbBottomLeft - grid.AabbSize * 0.5f, grid.AabbSize * 2, false, bound, cachePolicy: cachePolicy);
        }

        // Clone constructor. Clones share the same cache!
        private DefaultDualMapping(DefaultDualMapping other, SquareBound bound):base(other, bound)
        {
            baseGrid = other.baseGrid;
            getCellsByChunk = other.getCellsByChunk;
            meshDatas = other.meshDatas;
            toDual = other.toDual;
            toBase = other.toBase;
            moves = other.moves;
        }


        private static Vector2 ToVector2(Vector3 v) => new Vector2(v.x, v.y);

        private void MakeCaches(ICachePolicy cachePolicy)
        {
            if (cachePolicy == null)
                cachePolicy = Sylves.CachePolicy.Always;
            meshDatas = cachePolicy.GetDictionary<(MeshData meshData, List<(Cell primalCell, CellCorner primalCorner, Cell dualCell, CellCorner dualCorner)> mapping)>(this);
            toDual = cachePolicy.GetDictionary<Dictionary<(Cell, CellCorner), (Cell, CellCorner)>>(this);
            toBase = cachePolicy.GetDictionary<Dictionary<(Cell, CellCorner), (Cell, CellCorner)>>(this);
            moves = cachePolicy.GetDictionary<Dictionary<CellDir, (Cell, CellDir)>>(this);

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


            base.Setup(strideX, strideY, aabbBottomLeft - margin * Vector2.one, aabbSize + 2 * margin * Vector2.one, false, bound, cellTypes, cachePolicy);
        }

        private static T GetOrAdd<T>(IDictionary<Cell, T> cache, Cell v, Func<T> func)
        {
            if (cache.TryGetValue(v, out var x))
            {
                return x;
            }
            return cache[v] = func();

        }

        // This conversion is only safe for 2d cells, as they have a specific convention that
        // dir n is between corners n and (n+1)
        private CellDir CornerToDir(CellCorner corner) => (CellDir)corner;

        // This conversion is only safe for 2d cells, as they have a specific convention that
        // dir n is between corners n and (n+1)
        private CellCorner DirToCorner(CellDir dir) => (CellCorner)dir;


        // Compute the dual grid for a given chunk, and also the mapping between the base (primal) grid and the dual.
        // The mapping may refer to primal cells from several chunks, but it's exhaustive for the dual cells of the requested chunk.
        private (MeshData meshData, List<(Cell primalCell, CellCorner primalCorner, Cell dualCell, CellCorner dualCorner)> mapping) GetMeshData(Cell chunkCell)
        {
            var visited = new HashSet<(Cell cell, CellDir dir)>();
            var mapping = new List<(Cell primalCell, CellCorner primalCorner, Cell dualCell, CellCorner dualCorner)>();
            var dualCellCount = 0;
            var arcEnds = new List<((Cell cell, CellDir dir) startHe, (Cell cell, CellDir dir) endHe)?>();


            // Visit every corner of every cell in the primal chunk.
            var primalCells = getCellsByChunk(chunkCell);
            foreach(var primalCell in primalCells)
            {
                foreach(var primalCorner in baseGrid.GetCellCorners(primalCell))
                {
                    // Find the dual cell on corner by walking around the corner
                    var dualCell = Combine(new Cell(dualCellCount, 0), chunkCell);

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
                    var minChunk = getChunkByCell(currentHe.cell);
                    var oldMappingCount = mapping.Count;
                    bool isArc = false;
                    while(true)
                    {
                        visited.Add(currentHe);
                        mapping.Add((currentHe.cell, DirToCorner(currentHe.dir), dualCell, (CellCorner)dualCorner));
                        minChunk = LexMin(minChunk, getChunkByCell(currentHe.cell));
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
                            mapping.Add((currentHe.cell, DirToCorner(currentHe.dir), Combine(new Cell(dualCellCount, 0), chunkCell), (CellCorner)dualCorner2));
                            minChunk = LexMin(minChunk, getChunkByCell(currentHe.cell));
                            dualCorner2--;
                        }
                    }

                    // If a dual cell borders several chunks, 
                    // It'll be visited in several calls to GetMeshData.
                    // This ensures we keep only a unique one
                    var keepDualCell = minChunk == chunkCell;
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

        private (MeshData meshData, List<(Cell primalCell, CellCorner primalCorner, Cell dualCell, CellCorner dualCorner)> mapping) GetMeshDataCached(Cell chunkCell)
        {
            if(meshDatas.TryGetValue(chunkCell, out var x))
            {
                return x;
            }
            return meshDatas[chunkCell] = GetMeshData(chunkCell);
        }

        // Converts the meshData to a meshGrid.
        // Unlike PlanarLazyMeshGrid, there's no need for edge matching.
        protected override IGrid GetChildGrid(Cell chunkCell)
        {
            var meshData = GetMeshDataCached(chunkCell).meshData;
            // Builds MeshGrid, but skips edge matching as we don't need it.
            var meshGridOptions = new MeshGridOptions();
            var data = new DataDrivenData
            {
                Cells = new Dictionary<Cell, DataDrivenCellData>(),
                Moves = new Dictionary<(Cell, CellDir), (Cell, CellDir, Connection)>(),
            };
            MeshGridBuilder.BuildCellData(meshData, meshGridOptions, data.Cells);
            var meshGrid = new MeshGrid(meshData, new MeshGridOptions { }, data, true);
            meshGrid.BuildMeshDetails();
            return meshGrid;
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

        private static Cell LexMin(Cell a, Cell b)
        {
            if (a.x < b.x) return a;
            if (a.x > b.x) return b;
            if (a.y < b.y) return a;
            if (a.y > b.y) return b;
            if (a.z < b.z) return a;
            return b;
        }

        // Each move in the dual grid corresponds to moving from one corner to another of a particular primal cell.
        // We can use this to translate moves in the dual grid to operations in the primal grid.
        // This means we don't need worry about crossing chunk boundaries.
        public override bool TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection)
        {
            //  Unused
            connection = default;

            // Check if result is in cache
            if(!moves.TryGetValue(cell, out var moveDict))
            {
                moves[cell] = moveDict = new Dictionary<CellDir, (Cell, CellDir)>();
            }

            if(moveDict.TryGetValue(dir, out var t1))
            {
                (dest, inverseDir) = t1;
                return true;
            }

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
                    moveDict[dir] = (dest, inverseDir);
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
                    moveDict[dir] = (dest, inverseDir);
                    return true;
                }
            }

            dest = default;
            inverseDir = default;
            return false;
        }



        public override IGrid Unbounded => new DefaultDualMapping(this, null);



        public override IGrid BoundBy(IBound bound) => new DefaultDualMapping(this, (SquareBound)bound);

        #region IDualMapping implementation

        public IGrid BaseGrid => baseGrid;

        public IGrid DualGrid => this;

        public (Cell baseCell, CellCorner inverseCorner)? ToBasePair(Cell dualCell, CellCorner corner)
        {
            var chunk = Split(dualCell).chunkCell;
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
            var chunk = getChunkByCell(baseCell);
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
