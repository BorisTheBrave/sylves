using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Sylves
{
    /// <summary>
    /// Like BijectModifier, translates all cell coordinates by a fixed offset.
    /// </summary>
    internal class CellTranslateModifier : BijectModifier
    {
        Vector3Int offset;
        public CellTranslateModifier(IGrid underlying, Vector3Int offset) : base(underlying, c => c - offset, c => c + offset)
        {
            this.offset = offset;
        }

        protected override IGrid Rebind(IGrid underlying)
        {
            if(underlying is CellTranslateModifier ctm)
            {
                // Combine offsets
                return new CellTranslateModifier(ctm.Underlying, ctm.offset + offset);
            }
            return new CellTranslateModifier(underlying, offset);
        }
    }
}
