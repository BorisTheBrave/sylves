using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sylves
{
    /// <summary>
    /// Changes the world space positioning of the grid by a linear transform,
    /// leaving everything else unchanged.
    /// </summary>
    public class TransformModifier : BaseModifier
    {
        private readonly Matrix4x4 transform;
        private readonly Matrix4x4 iTransform;

        public TransformModifier(IGrid underlying, Matrix4x4 transform)
            : base(underlying)
        {
            this.transform = transform;
            this.iTransform = transform.inverse;
        }

        private TransformModifier(IGrid underlying, Matrix4x4 transform, Matrix4x4 iTransform)
           : base(underlying)
        {
            this.transform = transform;
            this.iTransform = iTransform;
        }

        public Matrix4x4 Transform => transform;

        protected override IGrid Rebind(IGrid underlying)
        {
            return new TransformModifier(underlying, transform, iTransform);
        }

        #region Relatives

        public override IDualMapping GetDual()
        {
            var underlyingDualMapping = Underlying.GetDual();
            return new DualMapping(this, new TransformModifier(underlyingDualMapping.DualGrid, transform), underlyingDualMapping);
        }
        private class DualMapping : BasicDualMapping
        {
            private readonly IDualMapping underlyingDualMapping;

            public DualMapping(TransformModifier baseGrid, TransformModifier dualGrid, IDualMapping underlyingDualMapping) : base(baseGrid, dualGrid)
            {
                this.underlyingDualMapping = underlyingDualMapping;
            }

            public override (Cell baseCell, CellCorner inverseCorner)? ToBasePair(Cell dualCell, CellCorner corner)
            {
                return underlyingDualMapping.ToBasePair(dualCell, corner);
            }

            public override (Cell dualCell, CellCorner inverseCorner)? ToDualPair(Cell baseCell, CellCorner corner)
            {
                return underlyingDualMapping.ToDualPair(baseCell, corner);
            }
        }


        #endregion

        #region Position
        public override Vector3 GetCellCenter(Cell cell) => transform.MultiplyPoint3x4(Underlying.GetCellCenter(cell));
        public override Vector3 GetCellCorner(Cell cell, CellCorner corner) => transform.MultiplyPoint3x4(Underlying.GetCellCorner(cell, corner));

        public override TRS GetTRS(Cell cell) => new TRS(transform * Underlying.GetTRS(cell).ToMatrix());
        #endregion

        #region Shape
        public override Deformation GetDeformation(Cell cell) => throw new NotImplementedException();

        public override void GetPolygon(Cell cell, out Vector3[] vertices, out Matrix4x4 transform)
        {
            Underlying.GetPolygon(cell, out vertices, out var uTransform);
            transform = this.transform * uTransform;
        }

        public override IEnumerable<(Vector3, Vector3, Vector3, CellDir)> GetTriangleMesh(Cell cell)
        {
            foreach (var (v1, v2, v3, cellDir) in Underlying.GetTriangleMesh(cell))
            {
                yield return (transform.MultiplyPoint3x4(v1), transform.MultiplyPoint3x4(v2), transform.MultiplyPoint3x4(v3), cellDir);
            }
        }

        public override void GetMeshData(Cell cell, out MeshData meshData, out Matrix4x4 transform)
        {
            Underlying.GetMeshData(cell, out meshData, out var uTransform);
            transform = this.transform * uTransform;
        }

        #endregion

        #region Query
        public override bool FindCell(Vector3 position, out Cell cell) => Underlying.FindCell(iTransform.MultiplyPoint3x4(position), out cell);

        public override bool FindCell(
            Matrix4x4 matrix,
            out Cell cell,
            out CellRotation rotation) => Underlying.FindCell(iTransform * matrix, out cell, out rotation);

        public override IEnumerable<Cell> GetCellsIntersectsApprox(Vector3 min, Vector3 max)
        {
            var center = (min + max) / 2;
            var hsize = (max - min) / 2;
            var center2 = iTransform.MultiplyPoint3x4(center);
            var hsize2 = iTransform.MultiplyVector(hsize);
            hsize2 = new Vector3(Mathf.Abs(hsize2.x), Mathf.Abs(hsize2.y), Mathf.Abs(hsize2.z));
            return Underlying.GetCellsIntersectsApprox(
                center2 - hsize2,
                center2 + hsize2
                );
        }

        public override IEnumerable<RaycastInfo> Raycast(Vector3 origin, Vector3 direction, float maxDistance = float.PositiveInfinity)
        {
            origin = iTransform.MultiplyPoint3x4(origin);
            direction = iTransform.MultiplyVector(direction);
            // TODO: Worry about maxDistance?
            foreach(var info in Underlying.Raycast(origin, direction, maxDistance))
            {
                var info2 = info;
                info2.point = transform.MultiplyPoint3x4(info.point);
                yield return info2;
            }
        }
        #endregion


    }
}
