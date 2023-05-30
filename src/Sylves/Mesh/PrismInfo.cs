using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylves
{
    /// <summary>
    /// Provides information about promoting a 2d cell type to a 3d cell type via extrusion.
    /// 
    /// Usually this results in NGonPrismCellType, but squares go to CubeCellType, which has additional symmetry.
    /// </summary>
    public class PrismInfo
    {
        private static PrismInfo squarePrismInfo = new PrismInfo
        {
            BaseCellType = SquareCellType.Instance,
            PrismCellType = CubeCellType.Instance,
            ForwardDir = (CellDir)CubeDir.Forward,
            BackDir = (CellDir)CubeDir.Back,
            BaseToPrismDirs = new Dictionary<CellDir, CellDir>
            {
                {(CellDir)SquareDir.Left, (CellDir)CubeDir.Left},
                {(CellDir)SquareDir.Right, (CellDir)CubeDir.Right},
                {(CellDir)SquareDir.Up, (CellDir)CubeDir.Up},
                {(CellDir)SquareDir.Down, (CellDir)CubeDir.Down},
            },
            BaseToPrismCorners = new Dictionary<CellCorner, (CellCorner, CellCorner)>
            {
                {(CellCorner)SquareCorner.DownLeft, ((CellCorner)CubeCorner.BackDownLeft, (CellCorner)CubeCorner.ForwardDownLeft)},
                {(CellCorner)SquareCorner.DownRight, ((CellCorner)CubeCorner.BackDownRight, (CellCorner)CubeCorner.ForwardDownRight)},
                {(CellCorner)SquareCorner.UpLeft, ((CellCorner)CubeCorner.BackUpLeft, (CellCorner)CubeCorner.ForwardUpLeft)},
                {(CellCorner)SquareCorner.UpRight, ((CellCorner)CubeCorner.BackUpRight, (CellCorner)CubeCorner.ForwardUpRight)},
            },
        }.Setup();

        private static PrismInfo xzSquarePrismInfo = new PrismInfo
        {
            BaseCellType = SquareCellType.Instance,
            PrismCellType = CubeCellType.Instance,
            ForwardDir = (CellDir)CubeDir.Up,
            BackDir = (CellDir)CubeDir.Down,
            BaseToPrismDirs = new Dictionary<CellDir, CellDir>
            {
                {(CellDir)SquareDir.Left, (CellDir)CubeDir.Left},
                {(CellDir)SquareDir.Right, (CellDir)CubeDir.Right},
                {(CellDir)SquareDir.Up, (CellDir)CubeDir.Back},
                {(CellDir)SquareDir.Down, (CellDir)CubeDir.Forward},
            },
            BaseToPrismCorners = new Dictionary<CellCorner, (CellCorner, CellCorner)>
            {
                {(CellCorner)SquareCorner.DownLeft, ((CellCorner)CubeCorner.ForwardDownLeft, (CellCorner)CubeCorner.ForwardUpLeft)},
                {(CellCorner)SquareCorner.DownRight, ((CellCorner)CubeCorner.ForwardDownRight, (CellCorner)CubeCorner.ForwardUpRight)},
                {(CellCorner)SquareCorner.UpLeft, ((CellCorner)CubeCorner.BackDownLeft, (CellCorner)CubeCorner.BackUpLeft)},
                {(CellCorner)SquareCorner.UpRight, ((CellCorner)CubeCorner.BackDownRight, (CellCorner)CubeCorner.BackUpRight)},
            },
        }.Setup();

        private static Dictionary<CellCorner, (CellCorner, CellCorner)> BaseToPrismCornersNGon(int n) =>
            Enumerable.Range(0, n).ToDictionary(i => (CellCorner)i, i => ((CellCorner)i, (CellCorner)(n + i)));

        private static IDictionary<int, PrismInfo> nGonPrismInfo = new Dictionary<int, PrismInfo>
        {
            // Despite being a different class, it should be identical
            [4] = squarePrismInfo,
        };

        private static PrismInfo ftHexPrismInfo = new PrismInfo
        {

            BaseCellType = HexCellType.Get(HexOrientation.FlatTopped),
            PrismCellType = HexPrismCellType.Get(HexOrientation.FlatTopped),
            ForwardDir = (CellDir)PTHexPrismDir.Forward,
            BackDir = (CellDir)PTHexPrismDir.Back,
            BaseToPrismCorners = BaseToPrismCornersNGon(6),
        }.Setup();

        private static PrismInfo ptHexPrismInfo = new PrismInfo
        {

            BaseCellType = HexCellType.Get(HexOrientation.PointyTopped),
            PrismCellType = HexPrismCellType.Get(HexOrientation.PointyTopped),
            ForwardDir = (CellDir)PTHexPrismDir.Forward,
            BackDir = (CellDir)PTHexPrismDir.Back,
            BaseToPrismCorners = BaseToPrismCornersNGon(6),
        }.Setup();

        private static PrismInfo ftTrianglePrismInfo = new PrismInfo
        {

            BaseCellType = TriangleCellType.Get(TriangleOrientation.FlatTopped),
            PrismCellType = TrianglePrismCellType.Get(TriangleOrientation.FlatTopped),
            ForwardDir = (CellDir)FSTrianglePrismDir.Forward,
            BackDir = (CellDir)FSTrianglePrismDir.Back,
            BaseToPrismCorners = BaseToPrismCornersNGon(6),
        }.Setup();

        private static PrismInfo fsTrianglePrismInfo = new PrismInfo
        {

            BaseCellType = TriangleCellType.Get(TriangleOrientation.FlatSides),
            PrismCellType = TrianglePrismCellType.Get(TriangleOrientation.FlatSides),
            ForwardDir = (CellDir)FSTrianglePrismDir.Forward,
            BackDir = (CellDir)FSTrianglePrismDir.Back,
            BaseToPrismCorners = BaseToPrismCornersNGon(6),
        }.Setup();



        private PrismInfo() { }

        public ICellType BaseCellType { get; set; }
        public ICellType PrismCellType { get; set; }

        public Dictionary<CellDir, CellDir> BaseToPrismDirs { get; set; }

        // (back, foward)
        public Dictionary<CellCorner, (CellCorner Back, CellCorner Forward)> BaseToPrismCorners { get; set; }
        public Dictionary<CellCorner, (CellCorner Corner, bool IsForward)> PrismToBaseCorners { get; set; }

        public CellDir ForwardDir { get; set; }

        public CellDir BackDir { get; set; }

        public CellDir BaseToPrism(CellDir baseDir)
        {
            if(BaseToPrismDirs == null)
            {
                return baseDir;
            }
            return BaseToPrismDirs[baseDir];
        }

        public CellCorner BaseToPrism(CellCorner baseCorner, bool isForward)
        {
            if (BaseToPrismCorners == null)
            {
                return baseCorner;
            }
            var t = BaseToPrismCorners[baseCorner];
            return isForward ? t.Item2 : t.Item1;
        }

        public static PrismInfo Get(ICellType baseCellType)
        {
            if (baseCellType == SquareCellType.Instance)
            {
                return squarePrismInfo;
            }
            if (baseCellType == HexCellType.Get(HexOrientation.FlatTopped))
            {
                return ftHexPrismInfo;
            }
            if (baseCellType == HexCellType.Get(HexOrientation.PointyTopped))
            {
                return ptHexPrismInfo;
            }
            if (baseCellType == TriangleCellType.Get(TriangleOrientation.FlatTopped))
            {
                return ftTrianglePrismInfo;
            }
            if (baseCellType == TriangleCellType.Get(TriangleOrientation.FlatSides))
            {
                return fsTrianglePrismInfo;
            }
            if (baseCellType is NGonCellType ngct)
            {
                var n = ngct.N;
                if(!nGonPrismInfo.TryGetValue(n, out var prismInfo))
                {
                    nGonPrismInfo[n] = prismInfo = new PrismInfo
                    {
                        ForwardDir = (CellDir)n,
                        BackDir = (CellDir)(n + 1),
                        BaseCellType = ngct,
                        PrismCellType = NGonPrismCellType.Get(n),
                        BaseToPrismCorners = BaseToPrismCornersNGon(6),
                    }.Setup();
                }
                return prismInfo;
            }
            if (baseCellType is XZCellTypeModifier xzCellModifier)
            {
                var underlying = xzCellModifier.Underlying;
                if(underlying == SquareCellType.Instance)
                {
                    return xzSquarePrismInfo;
                }
                // We assume that other cell types don't change under this modifier.
                var uPrismInfo = Get(underlying);
                // TODO: Avoid allocation?
                return new PrismInfo
                {
                    BaseCellType = baseCellType,
                    PrismCellType = XZCellTypeModifier.Get(uPrismInfo.PrismCellType),
                    BackDir = uPrismInfo.BackDir,
                    ForwardDir = uPrismInfo.ForwardDir,
                    BaseToPrismDirs = uPrismInfo.BaseToPrismDirs,
                }.Setup();
            }
            throw new NotImplementedException($"No prism info for cell type {baseCellType}");
        }

        private PrismInfo Setup()
        {
            if (BaseToPrismCorners != null)
            {
                PrismToBaseCorners = new Dictionary<CellCorner, (CellCorner Corner, bool IsForward)>();
                foreach (var kv in BaseToPrismCorners)
                {
                    PrismToBaseCorners[kv.Value.Back] = (kv.Key, false);
                    PrismToBaseCorners[kv.Value.Forward] = (kv.Key, true);
                }
            }
            return this;
        }
    }
}
