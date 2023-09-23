using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        public int MaxLayer { get; set; }

        public IBound PlanarBound { get; set; }

        public PlanarPrismBound Intersect(PlanarPrismBound other, IGrid planarGrid)
        {
            return new PlanarPrismBound
            {
                MinLayer = Math.Max(MinLayer, other.MinLayer),
                MaxLayer = Math.Min(MaxLayer, other.MaxLayer),
                PlanarBound = planarGrid.IntersectBounds(PlanarBound, other.PlanarBound),
            };
        }

        public PlanarPrismBound Union(PlanarPrismBound other, IGrid planarGrid)
        {
            return new PlanarPrismBound
            {
                MinLayer = Math.Min(MinLayer, other.MinLayer),
                MaxLayer = Math.Max(MaxLayer, other.MaxLayer),
                PlanarBound = planarGrid.UnionBounds(PlanarBound, other.PlanarBound),
            };
        }
    }

    // Doesn't use BaseModifier as so much has changed, everything needs overriding.
    /// <summary>
    /// Takes a 2d planar grid, and extends it into multiple layers along the third the dimension.
    /// </summary>
    public class PlanarPrismModifier : IGrid
    {
        private readonly IGrid underlying;
        private readonly PlanarPrismOptions planarPrismOptions;
        private readonly PlanarPrismBound bound;

        public PlanarPrismModifier(IGrid underlying, PlanarPrismOptions planarPrismOptions, int minLayer, int maxLayer)
            : this(underlying, planarPrismOptions, new PlanarPrismBound { MinLayer = minLayer, MaxLayer = maxLayer, PlanarBound = null })
        {
        }

        public PlanarPrismModifier(IGrid underlying, PlanarPrismOptions planarPrismOptions = null, PlanarPrismBound bound = null)
        {
            this.underlying = underlying;
            this.planarPrismOptions = planarPrismOptions ?? new PlanarPrismOptions();
            this.bound = bound;
            if (!underlying.Is2d)
            {
                throw new Exception("Underlying should be a 2d grid");
            }
            if (!underlying.IsPlanar)
            {
                throw new Exception("Underlying should be a planar grid");
            }
            if (underlying.CoordinateDimension >= 3)
            {
                throw new Exception("Underlying should be a grid that doesn't use the z coordinate (i.e. CoordinateDimension <= 2)");
            }
        }

        // Reduces a grid to only using the x-y co-ordinates, if necessary
        // TODO: Should this be a method on IGrid
        private (Func<Cell, Cell> toUnderlying, Func<Cell, Cell> fromUnderlying) CompressXY(IGrid grid)
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

        public int CoordinateDimension => 3;

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
                    MaxLayer = bound.MaxLayer + 1,
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
        #endregion

        #region Cell info

        public virtual IEnumerable<Cell> GetCells()
        {
            CheckBounded();
            foreach (var cell in underlying.GetCells())
            {
                for (var layer = bound.MinLayer; layer < bound.MaxLayer; layer++)
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
                return bound == null ? true : bound.MinLayer <= layer && layer < bound.MaxLayer;
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
                return underlying.IndexCount * (bound.MaxLayer - bound.MinLayer);
            }
        }

        public virtual int GetIndex(Cell cell) {
            CheckBounded();
            var (ucell, layer) = Split(cell);
            return underlying.GetIndex(ucell) * (bound.MaxLayer - bound.MinLayer) + (layer - bound.MinLayer);
        }

        public virtual Cell GetCellByIndex(int index)
        {
            var uindex = index / (bound.MaxLayer - bound.MinLayer);
            var layer = index % (bound.MaxLayer - bound.MinLayer) + bound.MinLayer;
            var ucell = underlying.GetCellByIndex(uindex);
            return Combine(ucell, layer);
        }
        #endregion

        #region Bounds

        public IBound GetBound() => bound;
        public virtual IBound GetBound(IEnumerable<Cell> cells) 
        {
            var uBound = underlying.GetBound(cells.Select(c=>Split(c).cell));
            var maxLayer = cells.Select(c => Split(c).layer).Max() + 1;
            var minLayer = cells.Select(c => Split(c).layer).Min();
            return new PlanarPrismBound
            {
                PlanarBound = uBound,
                MinLayer = minLayer,
                MaxLayer = maxLayer,
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
                for (var layer = planarPrismBound.MinLayer; layer < planarPrismBound.MaxLayer; layer++)
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
                return Underlying.IsCellInBound(uCell, ppb.PlanarBound) && ppb.MinLayer <= layer && layer < ppb.MaxLayer;
            }
            else
            {
                return true;
            }
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
            var (uCell, layer) = Split(cell);
            var deformation = underlying.GetDeformation(uCell);
            return Matrix4x4.Translate(GetOffset(layer)) * deformation;
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
            var indices = new int[n * 4 + n * 2];
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
            return bound == null ? true : bound.MinLayer <= layer && layer < bound.MaxLayer;
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
                return bound == null ? true : bound.MinLayer <= layer && layer < bound.MaxLayer;
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
                return bound == null ? true : bound.MinLayer <= layer && layer < bound.MaxLayer;
            }
        }

        public virtual IEnumerable<Cell> GetCellsIntersectsApprox(Vector3 min, Vector3 max)
        {
            var minLayer = GetLayer(min);
            var maxLayer = GetLayer(max);
            if(bound != null)
            {
                minLayer = Math.Max(minLayer, bound.MinLayer);
                maxLayer = Math.Min(maxLayer, bound.MaxLayer);
            }

            foreach (var uCell in underlying.GetCellsIntersectsApprox(GetPlanarPosition(min), GetPlanarPosition(max)))
            {
                for (var layer = minLayer; layer < maxLayer; layer++)
                {
                    yield return Combine(uCell, layer);
                }
            }
        }
        public IEnumerable<RaycastInfo> Raycast(Vector3 origin, Vector3 direction, float maxDistance = float.PositiveInfinity)
        {
            throw new NotImplementedException();
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
                MaxLayer = planarPrismBound.MaxLayer + layerOffset,
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
