using System;
using System.Collections.Generic;

using UnityEngine;

namespace Sylves
{
    public class RadialGrids
    {
        private const float ISqrt2 = 1 / 1.41421356237f;
        private static List<HalfPlane> Quadrant = new List<HalfPlane>{
                new HalfPlane{A = 1, B = 0, C = 0},
                new HalfPlane{A = 0, B = 1, C = 0},
            };

        #region Rhombi
        // https://robertlovespi.net/2021/05/28/a-radial-tessellation-of-the-plane-using-squares-and-45-135-degree-rhombi/
        private static MeshData MeshData1 = new MeshData
        {
            vertices = new Vector3[]
            {
                new Vector3(0, 0, 0),
                new Vector3(1, 0, 0),
                new Vector3(ISqrt2, ISqrt2, 0),
                new Vector3(ISqrt2, -ISqrt2, 0),
                new Vector3(1 + ISqrt2, ISqrt2, 0),
                new Vector3(1 + ISqrt2, -ISqrt2, 0),
                new Vector3(1 + ISqrt2 + ISqrt2, 0, 0),
            },
            indices = new Int32[][]
            {
                new Int32 []{0, 1, 4, 2, 0, 3, 5, 1, 1, 5, 6, 4 },
            },
            topologies = new MeshTopology[]
            {
                MeshTopology.Quads,
            }
        };
        private static MeshData MeshData2 = Matrix4x4.Translate(new Vector3(ISqrt2, ISqrt2, 0))
            * Matrix4x4.Rotate(Quaternion.Euler(0, 0, 180 + 45))
            * Matrix4x4.Translate(new Vector3(-1 - ISqrt2 - ISqrt2, 0, 0))
            * MeshData1;
        private static CompoundSection Section1 = new CompoundSection(
            MeshData1,
            new Vector2(1 + ISqrt2, ISqrt2),
            new Vector2(1 + ISqrt2, -ISqrt2),
            Quadrant
            );
        private static CompoundSection Section2 = new CompoundSection(
            MeshData2,
            new Vector2(ISqrt2, ISqrt2 + 1),
            new Vector2(ISqrt2 + 1, ISqrt2),
            Quadrant
            );

        public static CompoundGrid Rhombic = new CompoundGrid(new List<CompoundSection>
        {
            Section1,
            Section2,
            Matrix4x4.Rotate(Quaternion.Euler(0, 0, 90)) * Section1,
            Matrix4x4.Rotate(Quaternion.Euler(0, 0, 90)) * Section2,
            Matrix4x4.Rotate(Quaternion.Euler(0, 0, 180)) * Section1,
            Matrix4x4.Rotate(Quaternion.Euler(0, 0, 180)) * Section2,
            Matrix4x4.Rotate(Quaternion.Euler(0, 0, 270)) * Section1,
            Matrix4x4.Rotate(Quaternion.Euler(0, 0, 270)) * Section2,
        });
        #endregion

        #region Octagon
        // https://robertlovespi.net/2016/01/02/octagons-can-tile-a-plane-iii/
        private static MeshData MeshData3 = new MeshData
        {
            vertices = new Vector3[] {
                new Vector3(0, 0, 0),
                new Vector3(1, 0, 0),
                new Vector3(1 + ISqrt2, -ISqrt2, 0),
                new Vector3(1 + 2 * ISqrt2, 0, 0),
                new Vector3(1 + 2 * ISqrt2, 1, 0),
                new Vector3(1 + ISqrt2, 1 + ISqrt2, 0),
                new Vector3(ISqrt2, 1+ISqrt2, 0 ),
                new Vector3(0, 1, 0),
            },
            indices = new Int32[][] { new Int32[] { 0, 1, 2, 3, 4, 5, 6, ~7 } },
            topologies = new[] { MeshTopology.NGon }
        };
        private static CompoundSection Section3 = new CompoundSection(
            MeshData3,
            new Vector2(1 + 2 * ISqrt2, 0),
            new Vector2(1 + ISqrt2, -1 - ISqrt2),
            Quadrant
        );
        private static CompoundSection Section4 = Matrix4x4.Translate(new Vector3(1 + 2 * ISqrt2, 1, 0))
            * Matrix4x4.Rotate(Quaternion.Euler(0, 0, 45))
            * Section3;

        public static CompoundGrid Octagonal = new CompoundGrid(new List<CompoundSection>
        {
            Section3,
            Section4,
            Matrix4x4.Rotate(Quaternion.Euler(0, 0, 90)) * Section3,
            Matrix4x4.Rotate(Quaternion.Euler(0, 0, 90)) * Section4,
            Matrix4x4.Rotate(Quaternion.Euler(0, 0, 180)) * Section3,
            Matrix4x4.Rotate(Quaternion.Euler(0, 0, 180)) * Section4,
            Matrix4x4.Rotate(Quaternion.Euler(0, 0, 270)) * Section3,
            Matrix4x4.Rotate(Quaternion.Euler(0, 0, 270)) * Section4,
        });
        #endregion



    }
}
