using System;
using System.Collections.Generic;
using System.Linq;

#if UNITY
using UnityEngine;
#endif

using static Sylves.VectorUtils;

namespace Sylves
{
    public class RhombicDodecahedronGrid : IGrid
    {
        private static readonly ICellType[] cellTypes = { RhombicDodecahedronCellType.Instance };

        private static readonly MeshData meshData = MeshPrimitives.RhombicDodecahedron;

        CubeBound bound;

        Vector3 cellSize;

        public RhombicDodecahedronGrid(float cellSize, CubeBound bound = null)
            : this(new Vector3(cellSize, cellSize, cellSize), bound)
        {

        }

        public RhombicDodecahedronGrid(Vector3 cellSize, CubeBound bound = null)
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

        private static bool IsEvenParity(Cell cell) => MathUtils.PMod(cell.x + cell.y + cell.z, 2) == 0;

        // Number of even-parity cells in a run of `volume` consecutive cells whose first cell has the given parity.
        private static int EvenCount(int volume, int startParity)
        {
            if (volume <= 0)
            {
                return 0;
            }
            return volume / 2 + ((volume % 2 == 1 && startParity == 0) ? 1 : 0);
        }

        // Number of even-parity cells in `count` consecutive slices of volume `sliceVolume`,
        // where the first slice starts at `firstSliceStartParity`.
        private static int CountEvenInRun(int count, int sliceVolume, int firstSliceStartParity)
        {
            if (count <= 0 || sliceVolume <= 0)
            {
                return 0;
            }
            if (sliceVolume % 2 == 0)
            {
                return count * (sliceVolume / 2);
            }
            var extra = firstSliceStartParity == 0 ? (count + 1) / 2 : count / 2;
            return count * (sliceVolume / 2) + extra;
        }

        public Vector3 CellSize => cellSize;

        #region Basics
        public bool Is2d => false;

        public bool Is3d => true;

        public bool IsPlanar => false;

        public bool IsRepeating => true;

        public bool IsOrientable => true;

        public bool IsFinite => bound != null;

        public bool IsSingleCellType => true;

        public Int32 CoordinateDimension => 3;

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
                    return new RhombicDodecahedronGrid(cellSize, null);
                }
            }
        }

        public IGrid Unwrapped => this;

        public IDualMapping GetDual() => throw new NotImplementedException();

        public IGrid GetDiagonalGrid() => throw new NotImplementedException();

        public IGrid GetCompactGrid() => DefaultGridImpl.GetCompactGrid(this);

        public IGrid Recenter(Cell cell)
        {
            var grid = new CellTranslateModifier(this, (Vector3Int)cell);
            return DefaultGridImpl.Recenter(grid, cell);
        }
        #endregion

        #region Cell info

        public IEnumerable<Cell> GetCells()
        {
            CheckBounded();
            return GetCellsInBounds(bound);
        }

        public ICellType GetCellType(Cell cell)
        {
            return RhombicDodecahedronCellType.Instance;
        }

        public bool IsCellInGrid(Cell cell) => IsEvenParity(cell) && IsCellInBound(cell, bound);

        public Aabb? GetBoundAabb(IBound bound)
        {
            if (bound is CubeBound sb)
            {
                // Each cell extends 0.5 cell-units beyond its cube into the neighboring odd cubes.
                return Aabb.FromMinMax(
                    new Vector3((sb.Min.x - 0.5f) * cellSize.x, (sb.Min.y - 0.5f) * cellSize.y, (sb.Min.z - 0.5f) * cellSize.z),
                    new Vector3((sb.Mex.x + 0.5f) * cellSize.x, (sb.Mex.y + 0.5f) * cellSize.y, (sb.Mex.z + 0.5f) * cellSize.z));
            }
            return null;
        }

        #endregion

        #region Topology

        public bool TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection)
        {
            var rdDir = (RhombicDodecahedronDir)dir;
            dest = cell + rdDir.Forward();
            inverseDir = (CellDir)rdDir.Inverted();
            connection = new Connection();
            return IsCellInGrid(dest);
        }

        public bool TryMoveByOffset(Cell startCell, Vector3Int startOffset, Vector3Int destOffset, CellRotation startRotation, out Cell destCell, out CellRotation destRotation)
        {
            var rotation = (RhombicDodecahedronRotation)startRotation;
            destCell = startCell + rotation * (destOffset - startOffset);
            destRotation = rotation;
            return bound == null ? true : bound.Contains(destCell);
        }

        public bool ParallelTransport(IGrid aGrid, Cell aSrcCell, Cell aDestCell, Cell srcCell, CellRotation startRotation, out Cell destCell, out CellRotation destRotation)
        {
            return DefaultGridImpl.ParallelTransport(aGrid, aSrcCell, aDestCell, this, srcCell, startRotation, out destCell, out destRotation);
        }

        public IEnumerable<CellDir> GetCellDirs(Cell cell)
        {
            return RhombicDodecahedronCellType.Instance.GetCellDirs();
        }

        public IEnumerable<CellCorner> GetCellCorners(Cell cell)
        {
            return RhombicDodecahedronCellType.Instance.GetCellCorners();
        }

        public IEnumerable<(Cell, CellDir)> FindBasicPath(Cell startCell, Cell destCell)
        {
            var cell = startCell;
            var remaining = ToVector3Int(destCell) - ToVector3Int(startCell);
            while (remaining != Vector3Int.zero)
            {
                var ax = Math.Abs(remaining.x);
                var ay = Math.Abs(remaining.y);
                var az = Math.Abs(remaining.z);
                var sx = remaining.x == 0 ? 1 : Math.Sign(remaining.x);
                var sy = remaining.y == 0 ? 1 : Math.Sign(remaining.y);
                var sz = remaining.z == 0 ? 1 : Math.Sign(remaining.z);

                Vector3Int step;
                if (ax >= ay && ax >= az)
                {
                    step = ay >= az ? new Vector3Int(sx, sy, 0) : new Vector3Int(sx, 0, sz);
                }
                else if (ay >= az)
                {
                    step = az >= ax ? new Vector3Int(0, sy, sz) : new Vector3Int(sx, sy, 0);
                }
                else
                {
                    step = ax >= ay ? new Vector3Int(sx, 0, sz) : new Vector3Int(0, sy, sz);
                }

                var dir = (CellDir)RhombicDodecahedronDirExtensions.FromForward(step);
                yield return (cell, dir);
                cell += step;
                remaining -= step;
            }
        }

        #endregion

        #region Index
        public int IndexCount
        {
            get
            {
                CheckBounded();
                var size = bound.Size;
                var volume = size.x * size.y * size.z;
                return EvenCount(volume, MathUtils.PMod(bound.Min.x + bound.Min.y + bound.Min.z, 2));
            }
        }

        public int GetIndex(Cell cell)
        {
            CheckBounded();
            var min = bound.Min;
            var ny = bound.Size.y;
            var nz = bound.Size.z;
            var dx = cell.x - min.x;
            var dy = cell.y - min.y;
            var dz = cell.z - min.z;

            var index = CountEvenInRun(dx, ny * nz, MathUtils.PMod(min.x + min.y + min.z, 2));
            index += CountEvenInRun(dy, nz, MathUtils.PMod(cell.x + min.y + min.z, 2));
            index += EvenCount(dz, MathUtils.PMod(cell.x + cell.y + min.z, 2));
            return index;
        }

        public Cell GetCellByIndex(int index)
        {
            CheckBounded();
            var min = bound.Min;
            var ny = bound.Size.y;
            var nz = bound.Size.z;
            var yz = ny * nz;
            for (var x = min.x; x < bound.Mex.x; x++)
            {
                var slice = EvenCount(yz, MathUtils.PMod(x + min.y + min.z, 2));
                if (index < slice)
                {
                    for (var y = min.y; y < bound.Mex.y; y++)
                    {
                        var col = EvenCount(nz, MathUtils.PMod(x + y + min.z, 2));
                        if (index < col)
                        {
                            var zParity = MathUtils.PMod(x + y, 2);
                            var firstZ = min.z + MathUtils.PMod(zParity - min.z, 2);
                            return new Cell(x, y, firstZ + 2 * index);
                        }
                        index -= col;
                    }
                }
                index -= slice;
            }
            throw new ArgumentOutOfRangeException(nameof(index));
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
            return new RhombicDodecahedronGrid(cellSize, (CubeBound)IntersectBounds(this.bound, bound));
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
            foreach (var cell in (CubeBound)bound)
            {
                if (IsEvenParity(cell))
                {
                    yield return cell;
                }
            }
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
            return Vector3.Scale(cellSize, (Vector3Int)(cell) + new Vector3(0.5f, 0.5f, 0.5f) + ((RhombicDodecahedronCorner)corner).GetPosition());
        }

        /// <summary>
        /// Returns the appropriate transform for the cell.
        /// The translation will always be to GetCellCenter.
        /// Not inclusive of cell rotation, that should be applied first.
        /// </summary>
        public TRS GetTRS(Cell cell) => new TRS(GetCellCenter(cell), Quaternion.identity, cellSize);

        #endregion

        #region Shape

        public Deformation GetDeformation(Cell cell) => Deformation.Identity;

        public void GetPolygon(Cell cell, out Vector3[] vertices, out Matrix4x4 transform) => throw new Grid3dException();

        public IEnumerable<(Vector3, Vector3, Vector3, CellDir)> GetTriangleMesh(Cell cell)
        {
            var transform = Matrix4x4.Translate(GetCellCenter(cell)) * Matrix4x4.Scale(cellSize);
            var vertices = meshData.vertices;
            var quads = meshData.indices[0];
            for (var i = 0; i < 12; i++)
            {
                var v0 = transform.MultiplyPoint3x4(vertices[quads[i * 4 + 0]]);
                var v1 = transform.MultiplyPoint3x4(vertices[quads[i * 4 + 1]]);
                var v2 = transform.MultiplyPoint3x4(vertices[quads[i * 4 + 2]]);
                var v3 = transform.MultiplyPoint3x4(vertices[quads[i * 4 + 3]]);
                var dir = (CellDir)i;
                yield return (v0, v1, v2, dir);
                yield return (v0, v2, v3, dir);
            }
        }

        public void GetMeshData(Cell cell, out MeshData meshData, out Matrix4x4 transform)
        {
            meshData = RhombicDodecahedronGrid.meshData;
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
            var p = Divide(position, cellSize);
            cell = (Cell)Vector3Int.FloorToInt(p);
            if (IsEvenParity(cell))
            {
                return;
            }
            var local = p - ((Vector3Int)cell + new Vector3(0.5f, 0.5f, 0.5f));
            var ax = Math.Abs(local.x);
            var ay = Math.Abs(local.y);
            var az = Math.Abs(local.z);
            if (ax >= ay && ax >= az)
            {
                cell.x += local.x >= 0 ? 1 : -1;
            }
            else if (ay >= az)
            {
                cell.y += local.y >= 0 ? 1 : -1;
            }
            else
            {
                cell.z += local.z >= 0 ? 1 : -1;
            }
        }

        public bool FindCell(
            Matrix4x4 matrix,
            out Cell cell,
            out CellRotation rotation)
        {
            var rdRotation = RhombicDodecahedronRotation.FromMatrix(matrix);

            if (rdRotation != null)
            {
                rotation = rdRotation.Value;
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
            var minCell = Vector3Int.FloorToInt(Divide(min, cellSize)) - Vector3Int.one;
            var maxCell = Vector3Int.FloorToInt(Divide(max, cellSize)) + Vector3Int.one;
            if (bound != null)
            {
                minCell = Vector3Int.Max(minCell, bound.Min);
                maxCell = Vector3Int.Min(maxCell, bound.Max);
            }

            for (var x = minCell.x; x <= maxCell.x; x++)
            {
                for (var y = minCell.y; y <= maxCell.y; y++)
                {
                    for (var z = minCell.z; z <= maxCell.z; z++)
                    {
                        var cell = new Cell(x, y, z);
                        if (IsEvenParity(cell))
                        {
                            yield return cell;
                        }
                    }
                }
            }
        }

        // The 12 fa
        public IEnumerable<RaycastInfo> Raycast(Vector3 origin, Vector3 direction, float maxDistance = float.PositiveInfinity, bool exitInfo = false)
        {
            var x1 = origin.x / cellSize.x;
            var y1 = origin.y / cellSize.y;
            var z1 = origin.z / cellSize.z;
            var dx = direction.x / cellSize.x;
            var dy = direction.y / cellSize.y;
            var dz = direction.z / cellSize.z;

            if (dx == 0 && dy == 0 && dz == 0)
            {
                UnboundedFindCell(origin, out var c0);
                if (IsCellInGrid(c0))
                {
                    yield return new RaycastInfo
                    {
                        cell = c0,
                        point = origin,
                        cellDir = null,
                        distance = 0,
                    };
                }
                yield break;
            }

            var extraDistance = 0f;
            if (bound != null)
            {
                var minx = bound.Min.x - 0.5f;
                var miny = bound.Min.y - 0.5f;
                var minz = bound.Min.z - 0.5f;
                var maxx = bound.Mex.x + 0.5f;
                var maxy = bound.Mex.y + 0.5f;
                var maxz = bound.Mex.z + 0.5f;

                var tx1 = dx == 0 ? (minx > x1 ? 1 : -1) * float.PositiveInfinity : dx >= 0 ? (minx - x1) / dx : (maxx - x1) / dx;
                var tx2 = dx == 0 ? (maxx > x1 ? 1 : -1) * float.PositiveInfinity : dx >= 0 ? (maxx - x1) / dx : (minx - x1) / dx;
                var ty1 = dy == 0 ? (miny > y1 ? 1 : -1) * float.PositiveInfinity : dy >= 0 ? (miny - y1) / dy : (maxy - y1) / dy;
                var ty2 = dy == 0 ? (maxy > y1 ? 1 : -1) * float.PositiveInfinity : dy >= 0 ? (maxy - y1) / dy : (miny - y1) / dy;
                var tz1 = dz == 0 ? (minz > z1 ? 1 : -1) * float.PositiveInfinity : dz >= 0 ? (minz - z1) / dz : (maxz - z1) / dz;
                var tz2 = dz == 0 ? (maxz > z1 ? 1 : -1) * float.PositiveInfinity : dz >= 0 ? (maxz - z1) / dz : (minz - z1) / dz;

                var mint = Math.Max(tx1, Math.Max(ty1, tz1));
                var maxt = Math.Min(tx2, Math.Min(ty2, tz2));
                maxDistance = Math.Min(maxDistance, maxt);

                if (mint > 0)
                {
                    x1 += dx * mint;
                    y1 += dy * mint;
                    z1 += dz * mint;
                    maxDistance -= mint;
                    extraDistance = mint;
                    origin += direction * mint;
                }

                if (maxDistance < 0)
                    yield break;
                if (mint == float.PositiveInfinity)
                    yield break;
            }

            UnboundedFindCell(origin, out var cell);
            Cell? currentCell = default;
            if (IsCellInGrid(cell))
            {
                currentCell = cell;
                yield return new RaycastInfo
                {
                    cell = cell,
                    point = origin,
                    cellDir = null,
                    distance = extraDistance,
                };
            }

            // 6-family plane Digital Differential Analyzer.
            // Face of the cell lies a plane x±y, x±z, or y±z.
            // Like with CubeGrid.Raycast, we compute
            // "t" values, the time the ray crosses each plane.
            // And find the minimum in order to step to the next cell.
            // Then recompute all six t values and try again.
            var lastT = -1f;
            while (true)
            {
                var bestT = float.PositiveInfinity;
                var bestFamily = -1;
                var bestDuPositive = false;
                ConsiderFamily(0, dx + dy, x1 + y1);
                ConsiderFamily(1, dx - dy, x1 - y1);
                ConsiderFamily(2, dx + dz, x1 + z1);
                ConsiderFamily(3, dx - dz, x1 - z1);
                ConsiderFamily(4, dy + dz, y1 + z1);
                ConsiderFamily(5, dy - dz, y1 - z1);

                if (bestFamily < 0 || bestT > maxDistance)
                    yield break;

                FamilyStep(bestFamily, bestDuPositive, out var step, out var enterDir);
                cell += step;
                lastT = bestT;

                if (exitInfo && currentCell.HasValue)
                {
                    yield return new RaycastInfo
                    {
                        cell = currentCell.Value,
                        point = origin + bestT * direction,
                        cellDir = (CellDir)enterDir.Inverted(),
                        distance = bestT + extraDistance,
                        isExit = true,
                    };
                }

                if (IsCellInGrid(cell))
                {
                    currentCell = cell;
                    yield return new RaycastInfo
                    {
                        cell = cell,
                        point = origin + bestT * direction,
                        cellDir = (CellDir)enterDir,
                        distance = bestT + extraDistance,
                    };
                }
                else
                {
                    currentCell = null;
                }

                void ConsiderFamily(int family, float du, float u0)
                {
                    if (du == 0)
                        return;
                    var plane = FamilyExitPlane(family, cell, du > 0);
                    var t = (plane - u0) / du;
                    if (t > lastT + 1e-7f && t < bestT)
                    {
                        bestT = t;
                        bestFamily = family;
                        bestDuPositive = du > 0;
                    }
                }
            }
        }

        private static float FamilyExitPlane(int family, Cell cell, bool positive)
        {
            switch (family)
            {
                case 0: return positive ? cell.x + cell.y + 2 : cell.x + cell.y;
                case 1: return positive ? cell.x - cell.y + 1 : cell.x - cell.y - 1;
                case 2: return positive ? cell.x + cell.z + 2 : cell.x + cell.z;
                case 3: return positive ? cell.x - cell.z + 1 : cell.x - cell.z - 1;
                case 4: return positive ? cell.y + cell.z + 2 : cell.y + cell.z;
                case 5: return positive ? cell.y - cell.z + 1 : cell.y - cell.z - 1;
                default: throw new ArgumentOutOfRangeException(nameof(family));
            }
        }

        private static void FamilyStep(int family, bool positive, out Vector3Int step, out RhombicDodecahedronDir enterDir)
        {
            switch (family)
            {
                case 0:
                    step = positive ? new Vector3Int(1, 1, 0) : new Vector3Int(-1, -1, 0);
                    enterDir = positive ? RhombicDodecahedronDir.LeftDown : RhombicDodecahedronDir.RightUp;
                    return;
                case 1:
                    step = positive ? new Vector3Int(1, -1, 0) : new Vector3Int(-1, 1, 0);
                    enterDir = positive ? RhombicDodecahedronDir.LeftUp : RhombicDodecahedronDir.RightDown;
                    return;
                case 2:
                    step = positive ? new Vector3Int(1, 0, 1) : new Vector3Int(-1, 0, -1);
                    enterDir = positive ? RhombicDodecahedronDir.LeftBack : RhombicDodecahedronDir.RightForward;
                    return;
                case 3:
                    step = positive ? new Vector3Int(1, 0, -1) : new Vector3Int(-1, 0, 1);
                    enterDir = positive ? RhombicDodecahedronDir.LeftForward : RhombicDodecahedronDir.RightBack;
                    return;
                case 4:
                    step = positive ? new Vector3Int(0, 1, 1) : new Vector3Int(0, -1, -1);
                    enterDir = positive ? RhombicDodecahedronDir.DownBack : RhombicDodecahedronDir.UpForward;
                    return;
                case 5:
                    step = positive ? new Vector3Int(0, 1, -1) : new Vector3Int(0, -1, 1);
                    enterDir = positive ? RhombicDodecahedronDir.DownForward : RhombicDodecahedronDir.UpBack;
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(family));
            }
        }
        #endregion

        #region Symmetry

        public GridSymmetry FindGridSymmetry(ISet<Cell> src, ISet<Cell> dest, Cell srcCell, CellRotation cellRotation)
        {
            var rotation = (RhombicDodecahedronRotation)cellRotation;
            var srcMin = src.Select(ToVector3Int).Aggregate(Vector3Int.Min);
            var srcMax = src.Select(ToVector3Int).Aggregate(Vector3Int.Max);
            var r1 = rotation * srcMin;
            var r2 = rotation * srcMax;
            var newMin = Vector3Int.Min(r1, r2);
            var destMin = dest == src ? srcMin : dest.Select(ToVector3Int).Aggregate(Vector3Int.Min);
            var translation = destMin - newMin;
            if (!src.Select(c => FromVector3Int(translation + rotation * ToVector3Int(c))).All(dest.Contains))
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
            if (!TryApplySymmetry(s, FromVector3Int(cubeBound.Min), out var a, out var _))
            {
                return false;
            }
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
