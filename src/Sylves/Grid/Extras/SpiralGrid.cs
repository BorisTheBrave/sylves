using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
#if UNITY
using UnityEngine;
#endif

namespace Sylves
{
    /// <summary>
    /// Modifies any PeriodicPlanarGrid into a exponential spiral variant. 
    /// Uses the complex exponential function, see https://www.youtube.com/watch?v=ldxFjLJ3rVY.
    /// </summary>
    public class SpiralGrid : BaseModifier
    {
        /// <summary>
        /// Creates a new SpiralGrid from a PeriodicPlanarMeshGrid.
        /// The resulting grid is composed of two interlocking spirals.
        /// N1, N2 control the number of turns in the spirals: if you move N1 steps along one spiral, and then N2 steps along the second spiral, you get back to where you started.
        /// Thus, (0, N2) makes the spirals dengenerate: one is a series of rings, and the other radial lines.
        /// </summary>
        public SpiralGrid(PeriodicPlanarMeshGrid r, int N1, int N2) : base(MakeUnderlying(r, N1, N2))
        {
            this.N1 = N1;
            this.N2 = N2;
        }

        private SpiralGrid(int N1, int N2, IGrid underlying) : base(underlying)
        {
            this.N1 = N1;
            this.N2 = N2;
        }

        private static IGrid MakeUnderlying(PeriodicPlanarMeshGrid r, int N1, int N2)
        {

            if (r == null)
                throw new ArgumentNullException(nameof(r));
            if (N2 <= 0 && N1 <= 0)
            {
                N1 = -N1;
                N2 = -N2;
            }
            if (N1 == 0 && N2 == 0)
                throw new ArgumentOutOfRangeException(nameof(N2), "N2 or N1 must be non-zero.");


            var c0 = r.GetCellCenter(new Cell(0, 0, 0));
            var c1 = r.GetCellCenter(new Cell(0, N1, N2));
            var desired = new Vector3(0, 2 * Mathf.PI, 0);
            var delta = c1 - c0;
            var deltaMagnitude = delta.magnitude;
            if (deltaMagnitude <= 0)
                throw new ArgumentException("Could not compute spiral alignment transform from coincident reference cells.", nameof(r));

            var rotation = FromToRotation(delta, desired);
            var scale = desired.magnitude / deltaMagnitude;
            var alignment = Matrix4x4.Scale(new Vector3(scale, scale, scale)) * Matrix4x4.Rotate(rotation) * Matrix4x4.Translate(-c0);

            if (N2 > 0)
            {
                var masked = r.Masked(cell => cell.z >= 0 && cell.z < N2);
                var wrapped = new WrapModifier(masked, cell =>
                {
                    var k = -(int)Math.Floor((double)cell.z / N2);
                    return new Cell(cell.x, cell.y + k * N1, cell.z + k * N2);
                });
                var transformed = wrapped.Transformed(alignment);
                return transformed;
            }
            else
            {
                var masked = r.Masked(cell => cell.y >= 0 && cell.y < N1);
                var wrapped = new WrapModifier(masked, cell =>
                {
                    var k = -(int)Math.Floor((double)cell.y / N1);
                    return new Cell(cell.x, cell.y + k * N1, cell.z + k * N2);
                });
                var transformed = wrapped.Transformed(alignment);
                return transformed;
            }
        }

        protected override IGrid Rebind(IGrid underlying)
        {
            return new SpiralGrid(N1, N2, underlying);
        }

        public int N1 { get; }
        public int N2 { get; }

        internal Vector3 Transform(Vector3 v)
        {
            var ex = (float)Math.Exp(v.x);
            return new Vector3(ex * Mathf.Cos(v.y), ex * Mathf.Sin(v.y), v.z);
        }

        internal void GetTransformJacobi(Vector3 v, out Matrix4x4 jacobi)
        {
            // TODO: Does this need to translate too?
            var ex = (float)Math.Exp(v.x);
            var c = ex * Mathf.Cos(v.y);
            var s = ex * Mathf.Sin(v.y);
            jacobi = new Matrix4x4(
                new Vector4(c, s, 0, 0),
                new Vector4(-s, c, 0, 0),
                new Vector4(0, 0, 1, 0),
                new Vector4(0, 0, 0, 1)
            );
        }

        internal Vector3 Untransform(Vector3 v)
        {
            var r = Mathf.Sqrt(v.x * v.x + v.y * v.y);
            return new Vector3((float)Math.Log(r), Mathf.Atan2(v.y, v.x), v.z);
        }

        internal Matrix4x4 UntransformJacobi(Vector3 v)
        {
            var r = Mathf.Sqrt(v.x * v.x + v.y * v.y);
            if (r <= 0)
                throw new ArgumentException("Cannot compute inverse spiral Jacobian at the origin.", nameof(v));
            var invR = 1.0f / r;
            var c = v.x * invR * invR;
            var s = v.y * invR * invR;
            return new Matrix4x4(
                new Vector4(c, -s, 0, 0),
                new Vector4(s, c, 0, 0),
                new Vector4(0, 0, 1, 0),
                new Vector4(0, 0, 0, 1)
            );
        }

        private static Quaternion FromToRotation(Vector3 fromDirection, Vector3 toDirection)
        {
            var from = fromDirection.normalized;
            var to = toDirection.normalized;
            var dot = Vector3.Dot(from, to);

            if (dot > 0.999999f)
                return Quaternion.identity;

            if (dot < -0.999999f)
            {
                var axis = Vector3.Cross(from, Vector3.right);
                if (axis.sqrMagnitude < 0.000001f)
                    axis = Vector3.Cross(from, Vector3.up);
                axis.Normalize();
                return Quaternion.AngleAxis(180f, axis);
            }

            var cross = Vector3.Cross(from, to);
            var s = Mathf.Sqrt((1.0f + dot) * 2.0f);
            var invS = 1.0f / s;
            return new Quaternion(cross.x * invS, cross.y * invS, cross.z * invS, s * 0.5f);
        }

        #region Basics
        public override bool Is2d => true;

        public override bool Is3d => false;

        public override bool IsPlanar => true;

        public override bool IsRepeating => false;

        public override bool IsOrientable => false;

        public override bool IsFinite => false;

        public override bool IsSingleCellType => Underlying.IsSingleCellType;

        public override Int32 CoordinateDimension => 3;

        public override IEnumerable<ICellType> GetCellTypes() => Underlying.GetCellTypes();
        #endregion

        #region Relatives
        public override IGrid Unbounded => throw new NotImplementedException();

        public override IGrid Unwrapped => throw new NotImplementedException();

        public override IDualMapping GetDual() => throw new NotImplementedException();

        public override IGrid GetDiagonalGrid() => throw new NotImplementedException();

        public override IGrid GetCompactGrid() => throw new NotImplementedException();
        public override IGrid Recenter(Cell cell) => throw new NotImplementedException();

        #endregion

        #region Cell info

        #endregion

        #region Topology


        #endregion

        #region Index
        #endregion

        #region Bounds

        public override Aabb? GetBoundAabb(IBound bound) => throw new NotImplementedException();

        #endregion

        #region Position
        public override Vector3 GetCellCenter(Cell cell) => Transform(Underlying.GetCellCenter(cell));

        public override Vector3 GetCellCorner(Cell cell, CellCorner cellCorner) => Transform(Underlying.GetCellCorner(cell, cellCorner));

        public override TRS GetTRS(Cell cell)
        {
            var uTRS = Underlying.GetTRS(cell);
            var transformed = Transform(uTRS.Position);
            GetTransformJacobi(uTRS.Position, out var jacobi);
            var jTRS = new TRS(jacobi);
            var x = jTRS * uTRS;
            return new TRS(transformed, x.Rotation, x.Scale);
        }

        #endregion

        #region Shape

        private Deformation GetTransformDeformation(Cell cell)
        {
            return new Deformation(Transform, GetTransformJacobi, false);
        }

        public override Deformation GetDeformation(Cell cell)
        {
            return GetTransformDeformation(cell) * Underlying.GetDeformation(cell);
        }

        public override void GetPolygon(Cell cell, out Vector3[] vertices, out Matrix4x4 transform)
        {
            Underlying.GetPolygon(cell, out var uVertices, out var uTransform);
            vertices = new Vector3[uVertices.Length];
            for (var i = 0; i < vertices.Length; i++)
            {
                vertices[i] = Transform(uTransform.MultiplyPoint(uVertices[i]));
            }
            transform = Matrix4x4.identity;
        }

        public override IEnumerable<(Vector3, Vector3, Vector3, CellDir)> GetTriangleMesh(Cell cell) => throw new NotImplementedException();

        public override void GetMeshData(Cell cell, out MeshData meshData, out Matrix4x4 transform)
        {
            Underlying.GetMeshData(cell, out var uMeshData, out var uTransform);
            meshData = GetTransformDeformation(cell) * uTransform * uMeshData;
            transform = Matrix4x4.identity;
        }

        public override Aabb GetAabb(Cell cell) => throw new NotImplementedException();

        public override Aabb GetAabb(IEnumerable<Cell> cells) => throw new NotImplementedException();

        #endregion

        #region Query
        public override bool FindCell(Vector3 position, out Cell cell) => Underlying.FindCell(Untransform(position), out cell);

        public override bool FindCell(
            Matrix4x4 matrix,
            out Cell cell,
            out CellRotation rotation)
        {
            var position = VectorUtils.ToVector3(matrix.GetColumn(3));
            var inversePosition = Untransform(position);
            var inverseJacobi = UntransformJacobi(position);
            var inverseMatrix = VectorUtils.ToMatrix(
                inverseJacobi.MultiplyVector(VectorUtils.ToVector3(matrix.GetColumn(0))),
                inverseJacobi.MultiplyVector(VectorUtils.ToVector3(matrix.GetColumn(1))),
                inverseJacobi.MultiplyVector(VectorUtils.ToVector3(matrix.GetColumn(2))),
                inversePosition);
            return Underlying.FindCell(inverseMatrix, out cell, out rotation);
        }

        public override IEnumerable<Cell> GetCellsIntersectsApprox(Vector3 min, Vector3 max) => throw new NotImplementedException();

        public override IEnumerable<RaycastInfo> Raycast(Vector3 origin, Vector3 direction, float maxDistance = float.PositiveInfinity, bool exitInfo = false) => throw new NotImplementedException();
        #endregion

        #region Symmetry
        public override GridSymmetry FindGridSymmetry(ISet<Cell> src, ISet<Cell> dest, Cell srcCell, CellRotation cellRotation) => throw new NotImplementedException();

        public override bool TryApplySymmetry(GridSymmetry s, IBound srcBound, out IBound destBound) => throw new NotImplementedException();

        public override bool TryApplySymmetry(GridSymmetry s, Cell src, out Cell dest, out CellRotation r) => throw new NotImplementedException();
        #endregion
    }
}
