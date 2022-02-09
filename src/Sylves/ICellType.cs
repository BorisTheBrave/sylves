using System;
using System.Collections.Generic;
using System.Text;

namespace Sylves
{
    public interface ICellType
    {


        // Directions
        IEnumerable<CellDir> GetCellDirs();

        CellDir? Invert(CellDir dir);

        // Rotations

        IList<CellRotation> GetRotations(bool includeReflections = false);

        CellRotation Multiply(CellRotation a, CellRotation b);

        CellRotation Invert(CellRotation a);

        CellRotation GetIdentity();

        CellDir Rotate(CellDir dir, CellRotation rotation);

        void Rotate(CellDir dir, CellRotation rotation, out CellDir resultDir, out Connection connection);

        Matrix4x4 GetMatrix(CellRotation cellRotation);
    }
}
