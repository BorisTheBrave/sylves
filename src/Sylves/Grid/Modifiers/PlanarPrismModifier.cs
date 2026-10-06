using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY
using UnityEngine;
#endif

namespace Sylves
{
    
    public class PlanarPrismOptions
    {
        public float LayerHeight { get; set; } = 1;
        public float LayerOffset { get; set; }
    }

    public class PlanarPrismBound : IBound
    {
        public int MinLayer { get; set; }
        public int MexLayer { get; set; }

        public IBound PlanarBound { get; set; }

        public int MaxLayer {
            get => MexLayer - 1;
            set => MexLayer = value + 1;
        }

        public PlanarPrismBound Intersect(PlanarPrismBound other, IGrid planarGrid)
        {
            return new PlanarPrismBound
            {
                MinLayer = Math.Max(MinLayer, other.MinLayer),
                MexLayer = Math.Min(MexLayer, other.MexLayer),
                PlanarBound = planarGrid.IntersectBounds(PlanarBound, other.PlanarBound),
            };
        }

        public PlanarPrismBound Union(PlanarPrismBound other, IGrid planarGrid)
        {
            return new PlanarPrismBound
            {
                MinLayer = Math.Min(MinLayer, other.MinLayer),
                MexLayer = Math.Max(MexLayer, other.MexLayer),
                PlanarBound = planarGrid.UnionBounds(PlanarBound, other.PlanarBound),
            };
        }
    }

    // Doesn't use BaseModifier as so much has changed, everything needs overriding.
    /// <summary>
    /// Takes a 2d planar grid, and extends it into multiple layers along the third dimension.
    /// </summary>
    public class PlanarPrismModifier : IGrid
    {
        private readonly IGrid underlying;
        private readonly PlanarPrismOptions planarPrismOptions;
        private readonly PlanarPrismBound bound;

        public PlanarPrismModifier(IGrid underlying, PlanarPrismOptions planarPrismOptions, int minLayer, int mexLayer)
            : this(underlying, planarPrismOptions, new PlanarPrismBound { MinLayer = minLayer, MexLayer = mexLayer, PlanarBound = null })
        {
        }

        public PlanarPrismModifier(IGrid underlying, PlanarPrismOptions planarPrismOptions = null, PlanarPrismBound bound = null)
        {
            this.underlying = underlying;
            this.planarPrismOptions = planarPrismOptions ?? new PlanarPrismOptions();
            this.bound = bound;
            if (!underlying.Is2d)
            {
                throw new ArgumentException("Underlying should be a 2d grid");
            }
            if (!underlying.IsPlanar)
            {
                throw new ArgumentException("Underlying should be a planar grid");
            }
            if (underlying.CoordinateDimension >= 3)
            {
                throw new ArgumentException("Underlying should be a grid that doesn't use the z coordinate (i.e. CoordinateDimension <= 2). Consider using GetCompactGrid or RavelModifier before the PlanarPrismModifier.");
            }
        }

        // Reduces a grid to only using the x-y co-ordinates, if necessary
        // TODO: Should this be a method on IGrid
        private static (Func<Cell, Cell> toUnderlying, Func<Cell, Cell> fromUnderlying) CompressXY(IGrid grid)
        {
            if (grid is TransformModifier tf)
            {
                return CompressXY(tf.Underlying);
            }
            if(grid is TriangleGrid tg)
            {
                return (TrianglePrismGrid.ToTriangleGrid, TrianglePrismGrid.FromTriangleGrid);
            }
            if(grid is HexGrid hg)
            {
                // Strictly speaking, this is not needed due to some hexgrid magic that ignores z coords
                // but we do it anyway as it's more convenient
                return (c => new Cell(c.x, c.y, -c.x-c.y), c => new Cell(c.x, c.y));
            }
            if (grid.CoordinateDimension <= 2)
            {
                return (null, null);
            }

            Cell Compress(Cell c)
            {
                checked {
                    var i = IntUtils.Zip((short)c.y, (short)c.z);
                    return new Cell(c.x, i);
                }
            }
            Cell Uncompress(Cell c)
            {
                var (y, z) = IntUtils.Unzip(c.y);
                return new Cell(c.x, y, z);
            }
            return (Uncompress, Compress);
        }

        internal (Cell cell, int layer) Split(Cell cell)
        {
            return (new Cell(cell.x, cell.y), cell.z);
        }

        internal Cell Combine(Cell cell, int layer)
        {
            return new Cell(cell.x, cell.y, layer);
        }

        private static ICellType PrismCellType(ICellType underlyingCellType) => PrismInfo.Get(underlyingCellType).PrismCellType;

        private void GetAxialDirs(ICellType underlyingCellType, out CellDir forwardDir, out CellDir backDir)
        {
            var prismInfo = PrismInfo.Get(underlyingCellType);
            forwardDir = prismInfo.ForwardDir;
            backDir = prismInfo.BackDir;
        }
        
        // Should this use PrismInfo?
        private bool IsAxial(ICellType underlyingCellType, CellDir cellDir, out bool isForward, out CellDir inverseDir)
        {
            if (underlyingCellType == SquareCellType.Instance)
            {
                isForward = (int)cellDir == (int)CubeDir.Forward;
                inverseDir = (CellDir)((int)CubeDir.Forward + (int)CubeDir.Back - (int)cellDir);
                return (int)cellDir >= (int)CubeDir.Forward;
            }
            else if (underlyingCellType == HexCellType.Get(HexOrientation.FlatTopped) ||
                     underlyingCellType == HexCellType.Get(HexOrientation.PointyTopped) ||
                     underlyingCellType == TriangleCellType.Get(TriangleOrientation.FlatTopped) ||
                     underlyingCellType == TriangleCellType.Get(TriangleOrientation.FlatSides))
            {
                isForward = (int)cellDir == (int)PTHexPrismDir.Forward;
                inverseDir = (CellDir)((int)PTHexPrismDir.Forward + (int)PTHexPrismDir.Back - (int)cellDir);
                return (int)cellDir >= (int)PTHexPrismDir.Forward;
            }
            else
            {
                throw new NotImplementedException($"Cell type {underlyingCellType.GetType()} not implemented yet");
            }
        }

        // Returns an offset through the center of the layer
        private Vector3 GetOffset(int layer)
        {
            return (planarPrismOptions.LayerOffset + planarPrismOptions.LayerHeight * layer) * Vector3.forward;
        }
        private Vector3 GetOffset(float layer)
        {
            return (planarPrismOptions.LayerOffset + planarPrismOptions.LayerHeight * layer) * Vector3.forward;
        }
        private int GetLayer(Vector3 position)
        {
            return Mathf.RoundToInt((position.z - planarPrismOptions.LayerOffset) / planarPrismOptions.LayerHeight);
        }

        private void CheckBounded()
        {
            if (bound == null)
            {
                throw new GridInfiniteException();
            }
        }

        private ISet<Cell> ToUnderlying(ISet<Cell> cells, int layer)
        {
            // Maybe later we could do this lazily.
            // Biject set isn't quite right though - this isn't a bijection
            var set = new HashSet<Cell>(cells.Select(c => Combine(c, layer)));
            return set;
        }

        private GridSymmetry FromPlanar(GridSymmetry s, int layerOffset)
        {
            return new GridSymmetry
            {
                Rotation = s.Rotation,
                Src = Combine(s.Src, 0),
                Dest = Combine(s.Dest, layerOffset),
            };
        }
        private GridSymmetry ToPlanar(GridSymmetry s)
        {
            return new GridSymmetry
            {
                Rotation = s.Rotation,
                Src = Split(s.Src).cell,
                Dest = Split(s.Dest).cell,
            };
        }

        #region Basics

        public virtual bool Is2d => false;

        public virtual bool Is3d => true;

        public virtual bool IsPlanar => false;

        public virtual bool IsRepeating => underlying.IsRepeating;

        public virtual bool IsOrientable => underlying.IsOrientable;

        public virtual bool IsFinite => bound != null && underlying.IsFinite;

        public virtual bool IsSingleCellType => underlying.IsSingleCellType;

        public Int32 CoordinateDimension => 3;

        public virtual IEnumerable<ICellType> GetCellTypes() => underlying.GetCellTypes().Select(PrismCellType);

        #endregion

        #region Relatives

        public virtual IGrid Unbounded => new PlanarPrismModifier(underlying.Unbounded, new PlanarPrismOptions
        {
            LayerHeight = planarPrismOptions.LayerHeight,
            LayerOffset = planarPrismOptions.LayerOffset,
        });

        public virtual IGrid Unwrapped => underlying.Unwrapped;
        public virtual IGrid Underlying => underlying;

        private static Func<Cell, Cell> Identity = x => x;

        public virtual IDualMapping GetDual()
        {
            var dm = underlying.GetDual();
            var (to, from) = CompressXY(dm.DualGrid);
            var dualGrid = new PlanarPrismModifier(
                from == null ? dm.DualGrid : new BijectModifier(dm.DualGrid, to, from, 2),
                new PlanarPrismOptions
                {
                    LayerHeight = planarPrismOptions.LayerHeight,
                    LayerOffset = planarPrismOptions.LayerOffset - 0.5f * planarPrismOptions.LayerHeight,
                }, bound == null ? null : new PlanarPrismBound
                {
                    MinLayer = bound.MinLayer,
                    MexLayer = bound.MexLayer + 1,
                    PlanarBound = dm.DualGrid.GetBound(),
                });

            return new DualMapping(this, dualGrid, dm, to ?? Identity, from ?? Identity);
        }


        private class DualMapping : BasicDualMapping
        {
            private readonly PlanarPrismModifier baseGrid;
            private readonly PlanarPrismModifier dualGrid;
            private readonly IDualMapping planarDualMapping;
            private readonly Func<Cell, Cell> toUnderlying;
            private readonly Func<Cell, Cell> fromUnderlying;

            public DualMapping(PlanarPrismModifier baseGrid, PlanarPrismModifier dualGrid, IDualMapping planarDualMapping, Func<Cell, Cell> toUnderlying, Func<Cell, Cell> fromUnderlying) : base(baseGrid, dualGrid)
            {
                this.baseGrid = baseGrid;
                this.dualGrid = dualGrid;
                this.planarDualMapping = planarDualMapping;
                this.toUnderlying = toUnderlying;
                this.fromUnderlying = fromUnderlying;
            }
            public override (Cell dualCell, CellCorner inverseCorner)? ToDualPair(Cell baseCell, CellCorner corner)
            {
                var (uCell, layer) = baseGrid.Split(baseCell);
                var underlyingCellType = baseGrid.underlying.GetCellType(uCell);
                var prismInfo = PrismInfo.Get(underlyingCellType);
                var (uCorner, isForward) = prismInfo.PrismToBaseCorners[corner];
                var t = planarDualMapping.ToDualPair(uCell, uCorner);
                if (t == null)
                    return null;
                var (uDualCell, uInverseCorner) = t.Value;
                var underlyingDualCellType = dualGrid.underlying.GetCellType(uDualCell);
                var dualPrismInfo = PrismInfo.Get(underlyingDualCellType);
                var corners = dualPrismInfo.BaseToPrismCorners[uInverseCorner];
                var dualCell = dualGrid.Combine(fromUnderlying(uDualCell), layer + (isForward ? 1 : 0));
                if (!dualGrid.IsCellInGrid(dualCell))
                    return null;
                return (dualCell, isForward ? corners.Back : corners.Forward);

            }

            public override (Cell baseCell, CellCorner inverseCorner)? ToBasePair(Cell dualCell, CellCorner corner)
            {
                var (uDualCell, layer) = dualGrid.Split(dualCell);
                uDualCell = toUnderlying(uDualCell);
                var underlyingDualCellType = dualGrid.underlying.GetCellType(uDualCell);
                var dualPrismInfo = PrismInfo.Get(underlyingDualCellType);
                var (uDualCorner, isForward) = dualPrismInfo.PrismToBaseCorners[corner];
                var t = planarDualMapping.ToBasePair(uDualCell, uDualCorner);
                if (t == null)
                    return null;
                var (uCell, uInverseCorner) = t.Value;
                var underlyingCellType = baseGrid.underlying.GetCellType(uCell);
                var prismInfo = PrismInfo.Get(underlyingCellType);
                var corners = prismInfo.BaseToPrismCorners[uInverseCorner];
                var baseCell = baseGrid.Combine(uCell, layer + (isForward ? 0 : -1));
                if (!baseGrid.IsCellInGrid(baseCell))
                    return null;
                return (baseCell, isForward ? corners.Back : corners.Forward);
            }
        }

        public IGrid GetDiagonalGrid() => throw new NotImplementedException();

        public IGrid GetCompactGrid() => DefaultGridImpl.GetCompactGrid(this);

        public IGrid Recenter(Cell cell)
        {
            var (uCell, layer) = Split(cell);
            IGrid grid = new PlanarPrismModifier(Underlying.Recenter(uCell), planarPrismOptions, bound);
            grid = new CellTranslateModifier(grid, new Vector3Int(0, 0, layer));
            return DefaultGridImpl.Recenter(grid, cell);
        }
        #endregion

        #region Cell info

        public virtual IEnumerable<Cell> GetCells()
        {
            CheckBounded();
            foreach (var cell in underlying.GetCells())
            {
                for (var layer = bound.MinLayer; layer < bound.MexLayer; layer++)
                {
                    yield return Combine(cell, layer);
                }
            }

        }

        public virtual ICellType GetCellType(Cell cell) => PrismCellType(underlying.GetCellType(cell));
        public virtual bool IsCellInGrid(Cell cell) => IsCellInBound(cell, bound);
        #endregion

        #region Topology

        public virtual bool TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection)
        {
            if (IsAxial(underlying.GetCellType(cell), dir, out var isUp, out inverseDir))
            {
                var (uCell, layer) = Split(cell);
                layer += (isUp ? 1 : -1);
                dest = Combine(uCell, layer);
                connection = new Connection();
                return bound == null ? true : bound.MinLayer <= layer && layer < bound.MexLayer;
            }
            else
            {
                var (uCell, layer) = Split(cell);
                if (!underlying.TryMove(uCell, dir, out var destUCell, out inverseDir, out connection))
                {
                    dest = default;
                    return false;
                }
                dest = Combine(destUCell, layer);
                return true;
            }
        }

        public virtual bool TryMoveByOffset(Cell startCell, Vector3Int startOffset, Vector3Int destOffset, CellRotation startRotation, out Cell destCell, out CellRotation destRotation)
        {
            var (startUCell, startLayer) = Split(startCell);
            Vector3Int Flatten(Vector3Int v) => new Vector3Int(v.x, v.y, 0);
            if (!underlying.TryMoveByOffset(startUCell, Flatten(startOffset), Flatten(destOffset), startRotation, out destCell, out destRotation))
            {
                destCell = default;
                return false;
            }
            destCell = Combine(destCell, startCell.z + (destOffset.z - startOffset.z));
            return bound == null ? true : IsCellInGrid(destCell);
        }

        public virtual bool ParallelTransport(IGrid aGrid, Cell aSrcCell, Cell aDestCell, Cell srcCell, CellRotation startRotation, out Cell destCell, out CellRotation destRotation)
        {
            return DefaultGridImpl.ParallelTransport(aGrid, aSrcCell, aDestCell, this, srcCell, startRotation, out destCell, out destRotation);
        }

        public virtual IEnumerable<CellDir> GetCellDirs(Cell cell)
        {
            foreach (var dir in underlying.GetCellDirs(cell))
            {
                yield return dir;
            }
            var cellType = underlying.GetCellType(cell);
            GetAxialDirs(cellType, out var forwardDir, out var backDir);
            yield return forwardDir;
            yield return backDir;
        }



        public virtual IEnumerable<CellCorner> GetCellCorners(Cell cell)
        {
            var (uCell, layer) = Split(cell);
            var underlyingCellType = underlying.GetCellType(uCell);
            var prismInfo = PrismInfo.Get(underlyingCellType);
            foreach(var corner in underlying.GetCellCorners(uCell))
            {
                var (f, b) = prismInfo.BaseToPrismCorners[corner];
                yield return f;
                yield return b;
            }
        }

        public virtual IEnumerable<(Cell, CellDir)> FindBasicPath(Cell startCell, Cell destCell)
        {
            var (startUCell, startLayer) = Split(startCell);
            var (destUCell, destLayer) = Split(destCell);
            var path = underlying.FindBasicPath(startUCell, destUCell);
            if (path == null)
                return null;
            IEnumerable<(Cell, CellDir)> DoPath()
            {
                var cellType = underlying.GetCellType(startUCell);
                GetAxialDirs(cellType, out var forwardDir, out var backDir);
                var layer = startLayer;
                while (layer < destLayer)
                {
                    yield return (Combine(startUCell, layer), forwardDir);
                    layer += 1;
                }
                while (layer > destLayer)
                {
                    yield return (Combine(startUCell, layer), backDir);
                    layer -= 1;
                }
                foreach (var (uCell, dir) in path)
                {
                    yield return (Combine(uCell, layer), dir);
                }
            }
            return DoPath();
        }

        #endregion

        #region Index
        public virtual int IndexCount
        {
            get
            {
                CheckBounded();
                return underlying.IndexCount * (bound.MexLayer - bound.MinLayer);
            }
        }

        public virtual int GetIndex(Cell cell) {
            CheckBounded();
            var (ucell, layer) = Split(cell);
            return underlying.GetIndex(ucell) * (bound.MexLayer - bound.MinLayer) + (layer - bound.MinLayer);
        }

        public virtual Cell GetCellByIndex(int index)
        {
            var uindex = index / (bound.MexLayer - bound.MinLayer);
            var layer = index % (bound.MexLayer - bound.MinLayer) + bound.MinLayer;
            var ucell = underlying.GetCellByIndex(uindex);
            return Combine(ucell, layer);
        }
        #endregion

        #region Bounds

        public IBound GetBound() => bound;
        public virtual IBound GetBound(IEnumerable<Cell> cells) 
        {
            var uBound = underlying.GetBound(cells.Select(c=>Split(c).cell));
            var first = true;
            int minLayer = 0;
            int maxLayer = 0;
            foreach (var cell in cells)
            {
                var layer = Split(cell).layer;
                if (first)
                {
                    minLayer = maxLayer = layer;
                    first = false;
                }
                else
                {
                    minLayer = Math.Min(layer, minLayer);
                    maxLayer = Math.Min(layer, maxLayer);
                }
            }
            if (first)
                throw new Exception("Enumerable empty");
            return new PlanarPrismBound
            {
                PlanarBound = uBound,
                MinLayer = minLayer,
                MexLayer = maxLayer + 1,
            };
        }

        public virtual IGrid BoundBy(IBound bound)
        {
            if (bound == null) return this;
            PlanarPrismBound planarPrismBound = (PlanarPrismBound)bound;
            return new PlanarPrismModifier(
                underlying.BoundBy(planarPrismBound.PlanarBound),
                planarPrismOptions,
                (PlanarPrismBound)IntersectBounds(bound, planarPrismBound)
                );
        }

        public virtual IBound IntersectBounds(IBound bound, IBound other)
        {
            if (bound == null) return other;
            if (other == null) return bound;
            return ((PlanarPrismBound)bound).Intersect((PlanarPrismBound)other, underlying);
        }

        public virtual IBound UnionBounds(IBound bound, IBound other)
        {
            if (bound == null) return other;
            if (other == null) return bound;
            return ((PlanarPrismBound)bound).Union((PlanarPrismBound)other, underlying);
        }
        public virtual IEnumerable<Cell> GetCellsInBounds(IBound bound)
        {
            var planarPrismBound = (PlanarPrismBound)bound;
            foreach (var uCell in underlying.GetCellsInBounds(planarPrismBound.PlanarBound))
            {
                for (var layer = planarPrismBound.MinLayer; layer < planarPrismBound.MexLayer; layer++)
                {
                    yield return Combine(uCell, layer);
                }
            }
        }
        public virtual bool IsCellInBound(Cell cell, IBound bound)
        {
            if (bound is PlanarPrismBound ppb)
            {
                var (uCell, layer) = Split(cell);
                return Underlying.IsCellInBound(uCell, ppb.PlanarBound) && ppb.MinLayer <= layer && layer < ppb.MexLayer;
            }
            else
            {
                return true;
            }
        }

        public Aabb? GetBoundAabb(IBound bound)
        {
            if(bound is PlanarPrismBound ppb)
            {
                if(Underlying.GetBoundAabb(ppb.PlanarBound) is Aabb aabb)
                {
                    return Aabb.FromMinMax(
                        aabb.Min + GetOffset(ppb.MinLayer - 0.5f),
                        aabb.Max + GetOffset(ppb.MexLayer + 0.5f));
                }
            }
            return null;
        }
        #endregion

        #region Position

        public virtual Vector3 GetCellCenter(Cell cell)
        {
            var (uCell, layer) = Split(cell);
            return underlying.GetCellCenter(uCell) + GetOffset(layer);
        }

        public virtual Vector3 GetCellCorner(Cell cell, CellCorner corner)
        {
            var (uCell, layer) = Split(cell);
            var underlyingCellType = underlying.GetCellType(uCell);
            var prismInfo = PrismInfo.Get(underlyingCellType);
            var (uCorner, isForward) = prismInfo.PrismToBaseCorners[corner];
            return underlying.GetCellCorner(uCell, uCorner) + GetOffset(layer + (isForward ? 0.5f : -0.5f));
        }


        public virtual TRS GetTRS(Cell cell)
        {

            var (uCell, layer) = Split(cell);
            var trs = underlying.GetTRS(uCell);
            return new TRS(trs.Position + GetOffset(layer), trs.Rotation, Vector3.Scale(trs.Scale, new Vector3(1, 1, planarPrismOptions.LayerHeight)));
        }
        #endregion

        #region Shape
        public virtual Deformation GetDeformation(Cell cell)
        {
            // Build a deformation that is the underlying deformation plus a z mapping.

            var (uCell, layer) = Split(cell);
            var uDeformation = underlying.GetDeformation(uCell);
            (Vector3, float) SplitV(Vector3 p) => (new Vector3(p.x, p.y, 0), p.z);
            var layerHeight = planarPrismOptions.LayerHeight;
            var layerOffset = planarPrismOptions.LayerOffset;
            Vector3 DeformPoint(Vector3 p)
            {
                var (w, z) = SplitV(p);
                return uDeformation.DeformPoint(w) + GetOffset(z);
            }
            // Do these have simple forms?
            //Vector3 DeformNormal(Vector3 p, Vector3 n)
            //Vector4 DeformTangent(Vector3 p, Vector4 t)
            void GetJacobi(Vector3 p, out Matrix4x4 jacobi)
            {
                var (w, z) = SplitV(p);
                uDeformation.GetJacobi(w, out jacobi);
                jacobi.SetColumn(2, new Vector4(0, 0, layerHeight, 0));
                jacobi.m23 += z * layerHeight + layerOffset;
            }

            return new Deformation(DeformPoint, GetJacobi, uDeformation.InvertWinding);
        }

        public void GetPolygon(Cell cell, out Vector3[] vertices, out Matrix4x4 transform) => throw new Grid3dException();

        public IEnumerable<(Vector3, Vector3, Vector3, CellDir)> GetTriangleMesh(Cell cell)
        {
            // Need some thought about how to do this.
            throw new NotImplementedException();
            //Underlying.GetPolygon(cell, out var vertices, out var transform);
        }

        public void GetMeshData(Cell cell, out MeshData meshData, out Matrix4x4 transform)
        {
            Underlying.GetPolygon(cell, out var polygon, out var polyTransform);
            meshData = ExtrudePolygonToPrism(
                polygon,
                polyTransform,
                GetOffset(cell.z - 0.5f),
                GetOffset(cell.z + 0.5f)
                );
            transform = Matrix4x4.identity;
        }

        internal static MeshData ExtrudePolygonToPrism(Vector3[] polygon, Matrix4x4 transform, Vector3 backLayer, Vector3 frontLayer)
        {
            var n = polygon.Length;
            var vertices = new Vector3[n * 2];
            var indices = new Int32[n * 4 + n * 2];
            // Find the vertices
            for (var i = 0; i < n; i++)
            {
                vertices[i] = transform.MultiplyPoint3x4(polygon[i]) + backLayer;
                vertices[i + n] = transform.MultiplyPoint3x4(polygon[i]) + frontLayer;
            }

            // Explore all the square sides
            for (var i = 0; i < n; i++)
            {
                indices[i * 4 + 0] = i;
                indices[i * 4 + 1] = (i + 1) % n;
                indices[i * 4 + 2] = (i + 1) % n + n;
                indices[i * 4 + 3] = ~(i + n);
            }
            // Top and bottom
            for (var i = 0; i < n; i++)
            {
                indices[n * 4 + i] = n - 1 - i;
                indices[n * 5 + i] = n + i;
            }
            indices[n * 5 - 1] = ~indices[n * 5 - 1];
            indices[n * 6 - 1] = ~indices[n * 6 - 1];

            return new MeshData
            {
                vertices = vertices,
                indices = new[] { indices },
                topologies = new MeshTopology[] { MeshTopology.NGon },
            };
        }

        public Aabb GetAabb(Cell cell)
        {
            var (uCell, layer) = Split(cell);
            var aabb = Underlying.GetAabb(uCell);
            return Aabb.FromMinMax(aabb.Min + GetOffset(layer - 0.5f), aabb.Max + GetOffset(layer + 0.5f));
        }

        public Aabb GetAabb(IEnumerable<Cell> cells)
        {
            var aabb = Underlying.GetAabb(cells.Select(c => Split(c).cell));
            var first = true;
            int minLayer = 0;
            int maxLayer = 0;
            foreach(var cell in cells)
            {
                var layer = Split(cell).layer;
                if(first)
                {
                    minLayer = maxLayer = layer;
                    first = false;
                }
                else
                {
                    minLayer = Math.Min(layer, minLayer);
                    maxLayer = Math.Max(layer, maxLayer);
                }
            }
            if (first)
                throw new Exception("Enumerable empty");
            return Aabb.FromMinMax(aabb.Min + GetOffset(minLayer - 0.5f), aabb.Max + GetOffset(maxLayer + 0.5f));
        }

        #endregion

        #region Query
        private Vector3 GetPlanarPosition(Vector3 position) => new Vector3(position.x, position.y, 0);
        public virtual bool FindCell(Vector3 position, out Cell cell)
        {
            if (!underlying.FindCell(GetPlanarPosition(position), out var uCell))
            {
                cell = default;
                return false;
            }
            var layer = GetLayer(position);
            cell = Combine(uCell, layer);
            return bound == null ? true : bound.MinLayer <= layer && layer < bound.MexLayer;
        }

        public virtual bool FindCell(
            Matrix4x4 matrix,
            out Cell cell,
            out CellRotation rotation)
        {
            var position = matrix.MultiplyPoint3x4(Vector3.zero);
            var layer = GetLayer(position);
            if (!underlying.FindCell(GetPlanarPosition(position), out var uCell))
            {
                cell = default;
                rotation = default;
                return false;
            }
            cell = Combine(uCell, layer);
            var underlyingCellType = underlying.GetCellType(uCell);
            if(underlyingCellType == SquareCellType.Instance)
            {
                var trs = underlying.GetTRS(uCell);
                var cubeRotation = CubeRotation.FromMatrix(trs.ToMatrix().inverse * matrix);
                if(cubeRotation == null)
                {
                    rotation = default;
                    return false;
                }
                rotation = cubeRotation.Value;
                return bound == null ? true : bound.MinLayer <= layer && layer < bound.MexLayer;
            }
            else
            {
                // All other cell types just inherit rotation from their underlying
                if(!underlying.FindCell(matrix, out var _, out rotation))
                {
                    cell = default;
                    rotation = default;
                    return false;
                }
                return bound == null ? true : bound.MinLayer <= layer && layer < bound.MexLayer;
            }
        }

        public virtual IEnumerable<Cell> GetCellsIntersectsApprox(Vector3 min, Vector3 max)
        {
            var minLayer = GetLayer(min);
            var maxLayer = GetLayer(max);
            if(bound != null)
            {
                minLayer = Math.Max(minLayer, bound.MinLayer);
                maxLayer = Math.Min(maxLayer, bound.MexLayer - 1);
            }

            foreach (var uCell in underlying.GetCellsIntersectsApprox(GetPlanarPosition(min), GetPlanarPosition(max)))
            {
                for (var layer = minLayer; layer <= maxLayer; layer++)
                {
                    yield return Combine(uCell, layer);
                }
            }
        }
        public IEnumerable<RaycastInfo> Raycast(Vector3 origin, Vector3 direction, float maxDistance = float.PositiveInfinity, bool exitInfo = false)
        {
            var planarDirection = new Vector3(direction.x, direction.y, 0);

            var dz = direction.z;
            var layerHeight = planarPrismOptions.LayerHeight;
            var layerOffset = planarPrismOptions.LayerOffset;
            var layerStep = dz > 0 ? 1 : dz < 0 ? -1 : 0;

            // Check if we start outside the bound.
            // startOnLayerBorder: the ray enters the slab through a layer face, so the
            // first layer step crosses that face at t = 0 instead of starting inside a cell.
            var extraDistance = 0f;
            var startOnLayerBorder = false;
            var layerZ = origin.z;
            if (bound != null)
            {
                // Find the start and end values of t that the ray crosses the layers
                var zLo = layerOffset + layerHeight * (bound.MinLayer - 0.5f);
                var zHi = layerOffset + layerHeight * (bound.MexLayer - 0.5f);
                var tz1 = dz == 0 ? (zLo > origin.z ? 1 : -1) * float.PositiveInfinity : dz >= 0 ? (zLo - origin.z) / dz : (zHi - origin.z) / dz;
                var tz2 = dz == 0 ? (zHi > origin.z ? 1 : -1) * float.PositiveInfinity : dz >= 0 ? (zHi - origin.z) / dz : (zLo - origin.z) / dz;
                var mint = tz1;
                var maxt = tz2;

                // Don't go beyond maxt
                maxDistance = Math.Min(maxDistance, maxt);

                if (maxDistance < 0 || float.IsPositiveInfinity(mint))
                    yield break;

                if (mint > 0)
                {
                    // Advance things to mint. Snap the layer coordinate, not origin,
                    // so points stay on the ray (origin + t * direction).
                    origin += direction * mint;
                    maxDistance -= mint;
                    extraDistance = mint;
                    startOnLayerBorder = true;
                    layerZ = dz >= 0 ? zLo : zHi;
                }
            }

            // We walk along the underlying array in parallel with walking the layers
            // We rely on exitInfo: true to get the range of the ray in each cell
            var planarOrigin = new Vector3(origin.x, origin.y, 0);
            var planarHits = underlying.Raycast(planarOrigin, planarDirection, maxDistance, exitInfo: true).GetEnumerator();
            try
            {
                var hasPlanar = planarHits.MoveNext();
                Cell? uCell = null;
                var layer = 0;
                var hasCarriedLayer = false;
                var carriedLayer = 0;

                // Planar raycast starts inside a cell,
                // but the raycast started outside the slab and entered through a layer face.
                if (startOnLayerBorder
                    && hasPlanar
                    && planarHits.Current.distance == 0
                    && !planarHits.Current.isExit
                    && planarHits.Current.cellDir == null)
                {
                    // Skip the planar hit that claims we started inside a cell.
                    uCell = planarHits.Current.cell;
                    layer = dz > 0 ? bound.MinLayer - 1 : bound.MexLayer;
                    hasPlanar = planarHits.MoveNext();
                }
                else
                {
                    // The ray enters a planar cell through a side later on, so the walk
                    // starts normally and the layer face is not a special first step.
                    startOnLayerBorder = false;
                }

                while (true)
                {
                    // Time to next planar cell event (enter or exit)
                    var tPlanar = hasPlanar ? planarHits.Current.distance : float.PositiveInfinity;
                    // Time to next layer. Only computed when inside a planar cell.
                    var tLayer = float.PositiveInfinity;
                    if (uCell != null && layerStep != 0)
                    {
                        var boundaryZ = layerOffset + layerHeight * (layer + 0.5f * layerStep);
                        tLayer = (boundaryZ - layerZ) / dz;
                    }

                    if (tLayer <= tPlanar)
                    {
                        var t = tLayer;

                        if (float.IsInfinity(t) || t > maxDistance)
                            yield break;

                        // Move from one layer to the next
                        var cellType = underlying.GetCellType(uCell.Value);
                        GetAxialDirs(cellType, out var fwd, out var bck);

                        // On a border start, the current layer is outside the slab and was never entered.
                        if (exitInfo && !startOnLayerBorder)
                        {
                            yield return new RaycastInfo
                            {
                                cell = Combine(uCell.Value, layer),
                                point = origin + t * direction,
                                cellDir = dz > 0 ? fwd : bck,
                                distance = t + extraDistance,
                                isExit = true,
                            };
                        }
                        startOnLayerBorder = false;

                        layer += layerStep;
                        if (bound != null && (layer < bound.MinLayer || layer >= bound.MexLayer))
                            yield break;
                        yield return new RaycastInfo
                        {
                            cell = Combine(uCell.Value, layer),
                            point = origin + t * direction,
                            cellDir = dz > 0 ? bck : fwd,
                            distance = t + extraDistance,
                        };
                    }
                    else
                    {
                        var t = tPlanar;

                        if (float.IsInfinity(t) || t > maxDistance)
                            yield break;
                        // Either entering or exiting a planar cell
                        // Repeat the raycast info, and update state
                        var cellType = underlying.GetCellType(planarHits.Current.isExit ? uCell.Value : planarHits.Current.cell);
                        var prismInfo = PrismInfo.Get(cellType);

                        var info = planarHits.Current;
                        hasPlanar = planarHits.MoveNext();

                        if (info.isExit)
                        {
                            if (exitInfo)
                            {
                                yield return new RaycastInfo
                                {
                                    cell = Combine(uCell.Value, layer),
                                    point = origin + info.distance * direction,
                                    cellDir = info.cellDir.HasValue ? prismInfo.BaseToPrism(info.cellDir.Value) : (CellDir?)null,
                                    distance = info.distance + extraDistance,
                                    isExit = true,
                                };
                            }

                            carriedLayer = layer;
                            hasCarriedLayer = true;
                            uCell = null;
                        }
                        else
                        {
                            // The carried layer avoids rounding back across a layer face that was
                            // stepped at the same t as the planar exit. It only holds if no further
                            // layer face lies between that exit and this entry (i.e. no gap crossing).
                            var useCarried = false;
                            if (hasCarriedLayer)
                            {
                                var nextBoundaryZ = layerOffset + layerHeight * (carriedLayer + 0.5f * layerStep);
                                useCarried = layerStep == 0 || info.distance < (nextBoundaryZ - layerZ) / dz;
                            }
                            // Layers occupy [offset + height * (l - 0.5), offset + height * (l + 0.5)).
                            layer = useCarried ? carriedLayer : Mathf.FloorToInt((layerZ + info.distance * dz - layerOffset) / layerHeight + 0.5f);
                            hasCarriedLayer = false;
                            uCell = info.cell;
                            if (bound != null && (layer < bound.MinLayer || layer >= bound.MexLayer))
                                yield break;
                            yield return new RaycastInfo
                            {
                                cell = Combine(info.cell, layer),
                                point = origin + info.distance * direction,
                                cellDir = info.cellDir.HasValue ? prismInfo.BaseToPrism(info.cellDir.Value) : (CellDir?)null,
                                distance = info.distance + extraDistance,
                            };
                        }
                    }
                }
            }
            finally
            {
                planarHits.Dispose();
            }
        }
        #endregion
        #region Symmetry

        public virtual GridSymmetry FindGridSymmetry(ISet<Cell> src, ISet<Cell> dest, Cell srcCell, CellRotation cellRotation)
        {
            var (uSrcCell, layer) = Split(srcCell);
            var underlyingCellType = underlying.GetCellType(uSrcCell);

            if(underlyingCellType == SquareCellType.Instance)
            {
                // Atm we assume CellRotation can be passed through unchanged
                throw new NotImplementedException();
            }

            var uSrc = ToUnderlying(src, 0);
            var uDest = ToUnderlying(dest, 0);
            var s = underlying.FindGridSymmetry(uSrc, uDest, uSrcCell, cellRotation);
            if (s == null)
            {
                return null;
            }
            var srcMinLayer = src.Select(c => Split(c).layer).Aggregate((a, b) => Math.Min(a, b));
            var destMinLayer = src == dest ? srcMinLayer : dest.Select(c => Split(c).layer).Aggregate((a, b) => Math.Min(a, b));
            var layerOffset = destMinLayer - srcMinLayer;
            // Check it actually works
            Cell? Map(Cell c)
            {
                var (uc, l) = Split(c);
                if (!underlying.TryApplySymmetry(s, uc, out var ud, out var _))
                    return null;
                return Combine(ud, l + layerOffset);
            }
            if (!src.Select(Map)
                .OfType<Cell>()
                .All(dest.Contains))
            {
                return null;
            }
            return FromPlanar(s, layerOffset);
        }

        public virtual bool TryApplySymmetry(GridSymmetry s, IBound srcBound, out IBound destBound)
        {
            if(srcBound == null)
            {
                destBound = null;
                return true;
            }
            var planarPrismBound = (PlanarPrismBound)srcBound;
            if(!underlying.TryApplySymmetry(ToPlanar(s), planarPrismBound.PlanarBound, out var destPlanarBound))
            {
                destBound = default;
                return false;
            }
            var layerOffset = Split(s.Dest).layer - Split(s.Src).layer;
            destBound = new PlanarPrismBound
            {
                PlanarBound = destPlanarBound,
                MinLayer = planarPrismBound.MinLayer + layerOffset,
                MexLayer = planarPrismBound.MexLayer + layerOffset,
            };
            return true;
        }
        public virtual bool TryApplySymmetry(GridSymmetry s, Cell src, out Cell dest, out CellRotation r)
        {
            var (uSrc, layer) = Split(src);
            
            var underlyingCellType = underlying.GetCellType(uSrc);
            if (underlyingCellType == SquareCellType.Instance)
            {
                // Atm we assume CellRotation can be passed through unchanged
                throw new NotImplementedException();
            }

            var success = underlying.TryApplySymmetry(ToPlanar(s), uSrc, out var uDest, out r);
            var layerOffset = Split(s.Dest).layer - Split(s.Src).layer;
            dest = Combine(uDest, layer + layerOffset);
            return success;
        }
        #endregion
    }
}
