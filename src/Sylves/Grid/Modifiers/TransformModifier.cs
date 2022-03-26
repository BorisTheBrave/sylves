﻿using System;
using System.Collections.Generic;
#if UNITY
using UnityEngine;
#endif

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

        protected override IGrid Rebind(IGrid underlying)
        {
            return new TransformModifier(underlying, transform, iTransform);
        }

        #region Position
        public override Vector3 GetCellCenter(Cell cell) => transform.MultiplyPoint3x4(Underlying.GetCellCenter(cell));

        public override TRS GetTRS(Cell cell) => throw new NotImplementedException();

        public override Deformation GetDeformation(Cell cell) => throw new NotImplementedException();
        #endregion

        #region Query
        public override bool FindCell(Vector3 position, out Cell cell) => Underlying.FindCell(iTransform.MultiplyPoint3x4(position), out cell);

        public override bool FindCell(
            Matrix4x4 matrix,
            out Cell cell,
            out CellRotation rotation) => Underlying.FindCell(matrix * iTransform, out cell, out rotation);

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
        #endregion


    }
}
