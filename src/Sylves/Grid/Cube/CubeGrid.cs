﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

#if UNITY
using UnityEngine;
#endif

using static Sylves.VectorUtils;

namespace Sylves
{
    public class CubeGrid : IGrid
    {
        private static readonly ICellType[] cellTypes = { CubeCellType.Instance };

        CubeBound bound;

        Vector3 cellSize;

        public CubeGrid(float cellSize, CubeBound bound = null)
            : this(new Vector3(cellSize, cellSize, cellSize), bound)
        {

        }

        public CubeGrid(Vector3 cellSize, CubeBound bound = null)
        {
            this.cellSize = cellSize;
            this.bound = bound;
        }

        private void CheckBounded()
        {
            if (bound == null)
            {
                throw new GridInfiniteException();
            }
        }

        private static Vector3Int ToVector3Int(Cell cell) => (Vector3Int)cell;
        private static Cell FromVector3Int(Vector3Int v) => (Cell)v;

        public Vector3 CellSize => cellSize;

        #region Basics
        public bool Is2d => false;

        public bool Is3d => true;

        public bool IsPlanar => false;

        public bool IsRepeating => true;

        public bool IsOrientable => true;

        public bool IsFinite => bound != null;

        public bool IsSingleCellType => true;

        public int CoordinateDimension => 3;

        public IEnumerable<ICellType> GetCellTypes()
        {
            return cellTypes;
        }
        #endregion

        #region Relatives
        public IGrid Unbounded
        {
            get
            {
                if (bound == null)
                {
                    return this;
                }
                else
                {
                    return new CubeGrid(cellSize, null);
                }
            }
        }

        public IGrid Unwrapped => this;

        public IDualMapping GetDual()
        {
            var dualBound = bound == null ? null :
                new CubeBound(bound.Min, bound.Mex + Vector3Int.one);

            var translation = Matrix4x4.Translate(new Vector3(-cellSize.x / 2, -cellSize.y / 2, -cellSize.z / 2));

            return new DualMapping(this, new CubeGrid(cellSize, dualBound).Transformed(translation));
        }

        private class DualMapping : BasicDualMapping
        {
            public DualMapping(CubeGrid baseGrid, IGrid dualGrid) : base(baseGrid, dualGrid)
            {

            }

            private (Cell cell, CellCorner inverseCorner)? ToPair(Cell cell, CellCorner corner, IGrid outGrid)
            {

                switch ((CubeCorner)corner)
                {
                    case CubeCorner.BackDownLeft:
                        break;
                    case CubeCorner.BackDownRight:
                        cell.x += 1;
                        break;
                    case CubeCorner.BackUpLeft:
                        cell.y += 1;
                        break;
                    case CubeCorner.BackUpRight:
                        cell.x += 1;
                        cell.y += 1;
                        break;
                    case CubeCorner.ForwardDownLeft:
                        cell.z += 1;
                        break;
                    case CubeCorner.ForwardDownRight:
                        cell.x += 1;
                        cell.z += 1;
                        break;
                    case CubeCorner.ForwardUpLeft:
                        cell.y += 1;
                        cell.z += 1;
                        break;
                    case CubeCorner.ForwardUpRight:
                        cell.x += 1;
                        cell.y += 1;
                        cell.z += 1;
                        break;
                    default:
                        throw new Exception($"Unexpected corner {corner}");
                }
                if (!outGrid.IsCellInGrid(cell))
                {
                    return null;
                }
                return (cell, (CellCorner)((int)corner ^ 7));
            }

            public override (Cell dualCell, CellCorner inverseCorner)? ToDualPair(Cell baseCell, CellCorner corner) => ToPair(baseCell, corner, DualGrid);

            public override (Cell baseCell, CellCorner inverseCorner)? ToBasePair(Cell dualCell, CellCorner corner) => ToPair(dualCell - Vector3Int.one, corner, BaseGrid);
        }

        public IGrid GetDiagonalGrid() => throw new NotImplementedException();

        /*
        private enum DiagonalCubeDir
        {

        }

        private class DiagonalCellType : ICellType
        {
            Vector3Int Forward(DiagonalCubeDir dir)
            {
                var i = (int)dir;
                return new Vector3Int(
                    (i & (1 << 0)) != 0 ? 1 : (i & (1 << 1)) != 0 ? -1 : 0,
                    (i & (1 << 2)) != 0 ? 1 : (i & (1 << 3)) != 0 ? -1 : 0,
                    (i & (1 << 4)) != 0 ? 1 : (i & (1 << 5)) != 0 ? -1 : 0
                    );
            }

            DiagonalCubeDir FromForward(Vector3Int v)
            {
                return (DiagonalCubeDir)(
                    (v.x == 1 ? (1 << 0) : 0) +
                    (v.x == -1 ? (1 << 1) : 0) +
                    (v.y == 1 ? (1 << 2) : 0) +
                    (v.y == -1 ? (1 << 3) : 0) +
                    (v.z == 1 ? (1 << 4) : 0) +
                    (v.z == -1 ? (1 << 5) : 0) +
                    0);
            }

            void Rotate(CellDir dir, CellRotation rotation, out CellDir resultDir, out Connection connection)
            {
                connection = default;
                var cubeRotation = (CubeRotation)rotation;
                resultDir = (CellDir)FromForward(cubeRotation * Forward((DiagonalCubeDir)dir));
            }

        }

        private class DiagonalGrid : BaseOffsetDiagonalsModifier
        {
            private static OffsetCollection s_offsets = new OffsetCollection(new[]
            {
                new Vector3Int(1, 0, 0),
                new Vector3Int(1, 1, 0),
                new Vector3Int(0, 1, 0),
                new Vector3Int(-1, 1, 0),
                new Vector3Int(-1, 0, 0),
                new Vector3Int(-1, -1, 0),
                new Vector3Int(0, -1, 0),
                new Vector3Int(1, -1, 0),
            });
            private static ICellType s_cellType = new DiagonalCellType();
            private static ICellType[] s_cellTypes = new[] { s_cellType };

            public DiagonalGrid(IGrid underlying) : base(underlying)
            {
            }

            public override ICellType GetCellType(Cell cell) => s_cellType;
            public override IEnumerable<ICellType> GetCellTypes() => s_cellTypes;

            public override OffsetCollection GetOffsetCollection(Cell cell) => s_offsets;

            protected override IGrid Rebind(IGrid underlying) => new DiagonalGrid(underlying);
        }
        */

        public IGrid GetCompactGrid() => DefaultGridImpl.GetCompactGrid(this);
        #endregion

        #region Cell info

        public IEnumerable<Cell> GetCells()
        {
            CheckBounded();
            return bound;
        }

        public ICellType GetCellType(Cell cell)
        {
            return CubeCellType.Instance;
        }

        public bool IsCellInGrid(Cell cell) => IsCellInBound(cell, bound);

        public Aabb? GetBoundAabb(IBound bound)
        {
            if (bound is CubeBound sb)
            {
                return Aabb.FromMinMax(
                    new Vector3(sb.Min.x * cellSize.x, sb.Min.y * cellSize.y, sb.Min.z * cellSize.z),
                    new Vector3(sb.Mex.x * cellSize.x, sb.Mex.y * cellSize.y, sb.Mex.z * cellSize.z));
            }
            return null;
        }

        #endregion
        #region Topology

        public bool TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection)
        {
            switch ((CubeDir)dir)
            {
                case CubeDir.Right:
                    dest = cell + Vector3Int.right;
                    inverseDir = (CellDir)CubeDir.Left;
                    break;
                case CubeDir.Left:
                    dest = cell + Vector3Int.left;
                    inverseDir = (CellDir)CubeDir.Right;
                    break;
                case CubeDir.Up:
                    dest = cell + Vector3Int.up;
                    inverseDir = (CellDir)CubeDir.Down;
                    break;
                case CubeDir.Down:
                    dest = cell + Vector3Int.down;
                    inverseDir = (CellDir)CubeDir.Up;
                    break;
                case CubeDir.Forward:
                    dest = cell + new Vector3Int(0, 0, 1);
                    inverseDir = (CellDir)CubeDir.Back;
                    break;
                case CubeDir.Back:
                    dest = cell + new Vector3Int(0, 0, -1);
                    inverseDir = (CellDir)CubeDir.Forward;
                    break;
                default:
                    throw new Exception($"Invalid dir {dir}");
            }
            connection = new Connection();
            return bound == null ? true : bound.Contains(dest);
        }

        public bool TryMoveByOffset(Cell startCell, Vector3Int startOffset, Vector3Int destOffset, CellRotation startRotation, out Cell destCell, out CellRotation destRotation)
        {
            var squareRotation = (CubeRotation)startRotation;
            destCell = startCell + squareRotation * (destOffset - startOffset);
            destRotation = squareRotation;
            return bound == null ? true : bound.Contains(destCell);
        }

        public virtual bool ParallelTransport(IGrid aGrid, Cell aSrcCell, Cell aDestCell, Cell srcCell, CellRotation startRotation, out Cell destCell, out CellRotation destRotation)
        {
            // TODO: Recognize CubeGrids
            return DefaultGridImpl.ParallelTransport(aGrid, aSrcCell, aDestCell, this, srcCell, startRotation, out destCell, out destRotation);
        }

        public IEnumerable<CellDir> GetCellDirs(Cell cell)
        {
            return CubeCellType.Instance.GetCellDirs();
        }

        public IEnumerable<CellCorner> GetCellCorners(Cell cell)
        {
            return CubeCellType.Instance.GetCellCorners();
        }

        public IEnumerable<(Cell, CellDir)> FindBasicPath(Cell startCell, Cell destCell)
        {
            var cell = startCell;
            while (cell.x < destCell.x)
            {
                yield return (cell, (CellDir)CubeDir.Right);
                cell.x += 1;
            }
            while (cell.x > destCell.x)
            {
                yield return (cell, (CellDir)CubeDir.Left);
                cell.x -= 1;
            }
            while (cell.y < destCell.y)
            {
                yield return (cell, (CellDir)CubeDir.Up);
                cell.y += 1;
            }
            while (cell.y > destCell.y)
            {
                yield return (cell, (CellDir)CubeDir.Down);
                cell.y -= 1;
            }
            while (cell.z < destCell.z)
            {
                yield return (cell, (CellDir)CubeDir.Forward);
                cell.z += 1;
            }
            while (cell.z > destCell.z)
            {
                yield return (cell, (CellDir)CubeDir.Back);
                cell.z -= 1;
            }
        }


        #endregion

        #region Index
        public int IndexCount
        {
            get
            {
                CheckBounded();
                return bound.Size.x * bound.Size.y * bound.Size.z;
            }
        }

        public int GetIndex(Cell cell)
        {
            CheckBounded();
            return (cell.x - bound.Min.x) + (cell.y - bound.Min.y) * bound.Size.x + (cell.z - bound.Min.z) * bound.Size.x * bound.Size.y;
        }

        public Cell GetCellByIndex(int index)
        {
            var x = index % bound.Size.x;
            var r = index / bound.Size.x;
            var y = r % bound.Size.y;
            var z = r / bound.Size.y;
            return new Cell(x + bound.Min.x, y + bound.Min.y, z + bound.Min.z);
        }
        #endregion

        #region Bounds
        public IBound GetBound() => bound;

        public IBound GetBound(IEnumerable<Cell> cells)
        {
            return CubeBound.FromVectors(cells.Select(x => (Vector3Int)x));
        }

        public IGrid BoundBy(IBound bound)
        {
            return new CubeGrid(cellSize, (CubeBound)IntersectBounds(this.bound, bound));
        }

        public IBound IntersectBounds(IBound bound, IBound other)
        {
            if (bound == null) return other;
            if (other == null) return bound;
            return ((CubeBound)bound).Intersect((CubeBound)other);
        }
        public IBound UnionBounds(IBound bound, IBound other)
        {
            if (bound == null) return null;
            if (other == null) return null;
            return ((CubeBound)bound).Union((CubeBound)other);
        }
        public IEnumerable<Cell> GetCellsInBounds(IBound bound)
        {
            if (bound == null) throw new Exception("Cannot get cells in null bound as it is infinite");
            return (CubeBound)bound;
        }

        public bool IsCellInBound(Cell cell, IBound bound) => bound is CubeBound cb ? cb.Contains(cell) : true;
        #endregion

        #region Position
        /// <summary>
        /// Returns the center of the cell in local space
        /// </summary>
        public Vector3 GetCellCenter(Cell cell)
        {
            return Vector3.Scale(cellSize, (Vector3Int)(cell) + new Vector3(0.5f, 0.5f, 0.5f));
        }

        public Vector3 GetCellCorner(Cell cell, CellCorner corner)
        {
            return Vector3.Scale(cellSize, (Vector3Int)(cell) + new Vector3(0.5f, 0.5f, 0.5f) + ((CubeCorner)corner).GetPosition());
        }


        /// <summary>
        /// Returns the appropriate transform for the cell.
        /// The translation will always be to GetCellCenter.
        /// Not inclusive of cell rotation, that should be applied first.
        /// </summary>
        public TRS GetTRS(Cell cell) => new TRS(GetCellCenter(cell), Quaternion.identity, cellSize);

        #endregion

        #region Shape

        // TODO: This is not right
        public Deformation GetDeformation(Cell cell) => Deformation.Identity;

        public void GetPolygon(Cell cell, out Vector3[] vertices, out Matrix4x4 transform) => throw new Grid3dException();

        public IEnumerable<(Vector3, Vector3, Vector3, CellDir)> GetTriangleMesh(Cell cell)
        {
            var center = GetCellCenter(cell);

            // Uses same conventions as MeshPrimitives.Cube, but inlined for efficiency
            var v0 = Vector3.Scale(cellSize, new Vector3(+0.5f, -0.5f, -0.5f)) + center;
            var v1 = Vector3.Scale(cellSize, new Vector3(+0.5f, +0.5f, -0.5f)) + center;
            var v2 = Vector3.Scale(cellSize, new Vector3(-0.5f, +0.5f, -0.5f)) + center;
            var v3 = Vector3.Scale(cellSize, new Vector3(-0.5f, -0.5f, -0.5f)) + center;
            var v4 = Vector3.Scale(cellSize, new Vector3(+0.5f, -0.5f, +0.5f)) + center;
            var v5 = Vector3.Scale(cellSize, new Vector3(+0.5f, +0.5f, +0.5f)) + center;
            var v6 = Vector3.Scale(cellSize, new Vector3(-0.5f, +0.5f, +0.5f)) + center;
            var v7 = Vector3.Scale(cellSize, new Vector3(-0.5f, -0.5f, +0.5f)) + center;

            yield return (v0, v1, v5, (CellDir)CubeDir.Right);
            yield return (v5, v4, v0, (CellDir)CubeDir.Right);
            yield return (v7, v6, v2, (CellDir)CubeDir.Left);
            yield return (v2, v3, v7, (CellDir)CubeDir.Left);
            yield return (v2, v6, v5, (CellDir)CubeDir.Up);
            yield return (v5, v1, v2, (CellDir)CubeDir.Up);
            yield return (v0, v4, v7, (CellDir)CubeDir.Down);
            yield return (v7, v3, v0, (CellDir)CubeDir.Down);
            yield return (v4, v5, v6, (CellDir)CubeDir.Forward);
            yield return (v6, v7, v4, (CellDir)CubeDir.Forward);
            yield return (v3, v2, v1, (CellDir)CubeDir.Back);
            yield return (v1, v0, v3, (CellDir)CubeDir.Back);
        }

        public void GetMeshData(Cell cell, out MeshData meshData, out Matrix4x4 transform)
        {
            meshData = MeshPrimitives.Cube;
            transform = Matrix4x4.Translate(GetCellCenter(cell)) * Matrix4x4.Scale(cellSize);
        }

        public Aabb GetAabb(Cell cell) => GetBoundAabb(new CubeBound((Vector3Int)cell, (Vector3Int)cell + Vector3Int.one)).Value;

        public Aabb GetAabb(IEnumerable<Cell> cells) => GetBoundAabb(GetBound(cells)).Value;
        #endregion

        #region Query
        public bool FindCell(Vector3 position, out Cell cell)
        {
            UnboundedFindCell(position, out cell);
            return IsCellInGrid(cell);
        }
        private void UnboundedFindCell(Vector3 position, out Cell cell)
        {
            cell = (Cell)Vector3Int.FloorToInt(Divide(position, cellSize));
        }

        public bool FindCell(
            Matrix4x4 matrix,
            out Cell cell,
            out CellRotation rotation)
        {
            var cubeRotation = CubeRotation.FromMatrix(matrix);

            if(cubeRotation != null)
            {
                rotation = cubeRotation.Value;
                return FindCell(matrix.MultiplyPoint3x4(Vector3.zero), out cell);
            }
            else
            {
                cell = default;
                rotation = default;
                return false;
            }
        }

        public IEnumerable<Cell> GetCellsIntersectsApprox(Vector3 min, Vector3 max)
        {
            UnboundedFindCell(min, out var minCell);
            UnboundedFindCell(max, out var maxCell);
            // Filter to in bounds
            if (bound != null)
            {
                minCell.x = Math.Max(minCell.x, bound.Min.x);
                minCell.y = Math.Max(minCell.y, bound.Min.y);
                minCell.z = Math.Max(minCell.z, bound.Min.z);
                maxCell.x = Math.Min(maxCell.x, bound.Mex.x - 1);
                maxCell.y = Math.Min(maxCell.y, bound.Mex.y - 1);
                maxCell.z = Math.Min(maxCell.z, bound.Mex.z - 1);
            }

            // Loop over cells
            for (var x = minCell.x; x <= maxCell.x; x++)
            {
                for (var y = minCell.y; y <= maxCell.y; y++)
                {
                    for (var z = minCell.z; z <= maxCell.z; z++)
                    {
                        yield return new Cell(x, y, z);
                    }
                }
            }
        }

        public IEnumerable<RaycastInfo> Raycast(Vector3 origin, Vector3 direction, float maxDistance = float.PositiveInfinity)
        {
            return Raycast(origin, direction, maxDistance, cellSize, bound);
        }

        // TOOD: Move somewhere more appropriate?
        public static IEnumerable<RaycastInfo> Raycast(Vector3 origin, Vector3 direction, float maxDistance, Vector3 cellSize, CubeBound bound)
        { 
            // Normalize things into a space where each cell
            // occupies a unit cube.
            var x1 = origin.x / cellSize.x;
            var y1 = origin.y / cellSize.y;
            var z1 = origin.z / cellSize.z;
            var dx = direction.x / cellSize.x;
            var dy = direction.y / cellSize.y;
            var dz = direction.z / cellSize.z;

            var stepx = Math.Sign(dx);
            var stepy = Math.Sign(dy);
            var stepz = Math.Sign(dz);
            var idx = Math.Abs(1 / dx);
            var idy = Math.Abs(1 / dy);
            var idz = Math.Abs(1 / dz);
            var cellDirX = (CellDir)(dx >= 0 ? CubeDir.Left : CubeDir.Right);
            var cellDirY = (CellDir)(dy >= 0 ? CubeDir.Down : CubeDir.Up);
            var cellDirZ = (CellDir)(dz >= 0 ? CubeDir.Back : CubeDir.Forward);

            // -1 = in middle of cell, 0,1,2 = on x,y,z face
            int startOnBorder;
            float extraDistance;
            // Filter to bounds
            if (bound != null )
            {
                // Find the start and end values of t that the ray crosses each axis.
                var tx1 = dx == 0 ? (bound.Min.x > x1 ? 1 : -1) * float.PositiveInfinity : dx >= 0 ? (bound.Min.x - x1) / dx : (bound.Mex.x - x1) / dx;
                var tx2 = dx == 0 ? (bound.Mex.x > x1 ? 1 : -1) * float.PositiveInfinity : dx >= 0 ? (bound.Mex.x - x1) / dx : (bound.Min.x - x1) / dx;
                var ty1 = dy == 0 ? (bound.Min.y > y1 ? 1 : -1) * float.PositiveInfinity : dy >= 0 ? (bound.Min.y - y1) / dy : (bound.Mex.y - y1) / dy;
                var ty2 = dy == 0 ? (bound.Mex.y > y1 ? 1 : -1) * float.PositiveInfinity : dy >= 0 ? (bound.Mex.y - y1) / dy : (bound.Min.y - y1) / dy;
                var tz1 = dz == 0 ? (bound.Min.z > z1 ? 1 : -1) * float.PositiveInfinity : dz >= 0 ? (bound.Min.z - z1) / dz : (bound.Mex.z - z1) / dz;
                var tz2 = dz == 0 ? (bound.Mex.z > z1 ? 1 : -1) * float.PositiveInfinity : dz >= 0 ? (bound.Mex.z - z1) / dz : (bound.Min.z - z1) / dz;

                var mint = Math.Max(tx1, Math.Max(ty1, tz1));
                var maxt = Math.Min(tx2, Math.Min(ty2, tz2));
                // Don't go beyond maxt
                maxDistance = Math.Min(maxDistance, maxt);

                if (mint > 0)
                {
                    // Advance things to mint
                    x1 += dx * mint;
                    y1 += dy * mint;
                    z1 += dz * mint;
                    maxDistance -= mint;
                    extraDistance = mint;
                    origin += direction * mint;
                    if (tx1 == mint)
                    {
                        startOnBorder = 0;
                        x1 = dx == 0 ? (bound.Min.x > x1 ? 1 : -1) * float.PositiveInfinity : dx >= 0 ? bound.Min.x : bound.Mex.x;
                    }
                    else if (ty1 == mint)
                    {
                        startOnBorder = 1;
                        y1 = dy == 0 ? (bound.Min.y > y1 ? 1 : -1) * float.PositiveInfinity : dy >= 0 ? bound.Min.y : bound.Mex.y;
                    }
                    else
                    {
                        startOnBorder = 2;
                        z1 = dz == 0 ? (bound.Min.z > z1 ? 1 : -1) * float.PositiveInfinity : dz >= 0 ? bound.Min.z : bound.Mex.z;
                    }
                }
                else
                {
                    startOnBorder = -1;
                    extraDistance = 0;
                }

                if (maxDistance < 0)
                    yield break;
                if (mint == float.PositiveInfinity)
                    yield break;
            }
            else
            {
                startOnBorder = -1;
                extraDistance = 0;
            }

            var x = startOnBorder == 0 ? Mathf.RoundToInt(x1) + (dx > 0 ? -1 : 0) : Mathf.FloorToInt(x1);
            var y = startOnBorder == 1 ? Mathf.RoundToInt(y1) + (dy > 0 ? -1 : 0) : Mathf.FloorToInt(y1);
            var z = startOnBorder == 2 ? Mathf.RoundToInt(z1) + (dz > 0 ? -1 : 0) : Mathf.FloorToInt(z1);

            if (startOnBorder == -1)
            {
                yield return new RaycastInfo
                {
                    cell = new Cell(x, y, z),
                    point = origin,
                    cellDir = null,
                    distance = 0,
                };
            }

            var tx = (x + (dx >= 0 ? 1 : 0) - x1) / dx;
            var ty = (y + (dy >= 0 ? 1 : 0) - y1) / dy;
            var tz = (z + (dz >= 0 ? 1 : 0) - z1) / dz;

            while (true)
            {
                float t;
                CellDir cellDir;
                if (tx < ty && tx < tz)
                {
                    if (tx > maxDistance) yield break;
                    t = tx;
                    x += stepx;
                    tx += idx;
                    cellDir = cellDirX;
                    if (bound != null && (x >= bound.Mex.x || x < bound.Min.x)) yield break;
                }
                else if (ty < tz)
                {
                    if (ty > maxDistance) yield break;
                    t = ty;
                    y += stepy;
                    ty += idy;
                    cellDir = cellDirY;
                    if (bound != null && (y >= bound.Mex.y || y < bound.Min.y)) yield break;
                }
                else
                {
                    if (tz > maxDistance) yield break;
                    t = tz;
                    z += stepz;
                    tz += idz;
                    cellDir = cellDirZ;
                    if (bound != null && (z >= bound.Mex.z || z < bound.Min.z)) yield break;
                }
                yield return new RaycastInfo
                {
                    cell = new Cell(x, y, z),
                    point = origin + t * direction,
                    cellDir = cellDir,
                    distance = t + extraDistance,
                };
            }
        }
        #endregion



        #region Symmetry

        public GridSymmetry FindGridSymmetry(ISet<Cell> src, ISet<Cell> dest, Cell srcCell, CellRotation cellRotation)
        {
            var cubeRotation = (CubeRotation)cellRotation;
            var srcBound = GetBound(src);
            var srcMin = src.Select(ToVector3Int).Aggregate(Vector3Int.Min);
            var srcMax = src.Select(ToVector3Int).Aggregate(Vector3Int.Max);
            var r1 = cubeRotation * srcMin;
            var r2 = cubeRotation * srcMax;
            var newMin = Vector3Int.Min(r1, r2);
            var destMin = dest == src ? srcMin : dest.Select(ToVector3Int).Aggregate(Vector3Int.Min);
            var translation = destMin - newMin;
            // Check it actually works
            if (!src.Select(c => FromVector3Int(translation + cubeRotation * ToVector3Int(c))).All(dest.Contains))
            {
                return null;
            }
            return new GridSymmetry
            {
                Src = new Cell(),
                Dest = FromVector3Int(translation),
                Rotation = cellRotation,
            };
        }

        public bool TryApplySymmetry(GridSymmetry s, IBound srcBound, out IBound destBound)
        {
            destBound = null;
            if (srcBound == null)
            {
                return true;
            }
            var cubeBound = (CubeBound)srcBound;
            // TODO: Use operator*
            if (!TryApplySymmetry(s, FromVector3Int(cubeBound.Min), out var a, out var _))
            {
                return false;
            }
            // This trick works best with inclusive bounds.
            if (!TryApplySymmetry(s, FromVector3Int(cubeBound.Max), out var b, out var _))
            {
                return false;
            }
            destBound = new CubeBound(Vector3Int.Min(ToVector3Int(a), ToVector3Int(b)), Vector3Int.Max(ToVector3Int(a), ToVector3Int(b)) + Vector3Int.one);
            return true;
        }

        public bool TryApplySymmetry(GridSymmetry s, Cell src, out Cell dest, out CellRotation r)
        {
            return TryMoveByOffset(s.Dest, (Vector3Int)s.Src, (Vector3Int)src, s.Rotation, out dest, out r);
        }
        #endregion
    }
}
