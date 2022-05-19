using System;
using System.Collections.Generic;
using System.Text;

namespace Sylves
{
    /// <summary>
    /// Provides information about promoting a 2d cell type to a 3d cell type via extrusion.
    /// </summary>
    public class PrismInfo
    {
        private static PrismInfo squarePrismInfo = new PrismInfo
        {
            BaseCellType = SquareCellType.Instance,
            PrismCellType = CubeCellType.Instance,
            ForwardDir = (CellDir)CubeDir.Forward,
            BackDir = (CellDir)CubeDir.Back,
            BaseToPrismDict = new Dictionary<CellDir, CellDir>
            {
                {(CellDir)SquareDir.Left, (CellDir)CubeDir.Left},
                {(CellDir)SquareDir.Right, (CellDir)CubeDir.Right},
                {(CellDir)SquareDir.Up, (CellDir)CubeDir.Up},
                {(CellDir)SquareDir.Down, (CellDir)CubeDir.Down},
            },
        };

        private static PrismInfo xzSquarePrismInfo = new PrismInfo
        {
            BaseCellType = SquareCellType.Instance,
            PrismCellType = CubeCellType.Instance,
            ForwardDir = (CellDir)CubeDir.Up,
            BackDir = (CellDir)CubeDir.Down,
            BaseToPrismDict = new Dictionary<CellDir, CellDir>
            {
                {(CellDir)SquareDir.Left, (CellDir)CubeDir.Left},
                {(CellDir)SquareDir.Right, (CellDir)CubeDir.Right},
                {(CellDir)SquareDir.Up, (CellDir)CubeDir.Back},
                {(CellDir)SquareDir.Down, (CellDir)CubeDir.Forward},
            },
        };

        private static PrismInfo ftHexPrismInfo = new PrismInfo
        {

            BaseCellType = HexCellType.Get(HexOrientation.FlatTopped),
            PrismCellType = HexPrismCellType.Get(HexOrientation.FlatTopped),
            ForwardDir = (CellDir)PTHexPrismDir.Forward,
            BackDir = (CellDir)PTHexPrismDir.Back,
        };

        private static PrismInfo ptHexPrismInfo = new PrismInfo
        {

            BaseCellType = HexCellType.Get(HexOrientation.PointyTopped),
            PrismCellType = HexPrismCellType.Get(HexOrientation.PointyTopped),
            ForwardDir = (CellDir)PTHexPrismDir.Forward,
            BackDir = (CellDir)PTHexPrismDir.Back,
        };



        private PrismInfo() { }

        public ICellType BaseCellType { get; set; }
        public ICellType PrismCellType { get; set; }

        public Dictionary<CellDir, CellDir> BaseToPrismDict { get; set; }

        public CellDir ForwardDir { get; set; }

        public CellDir BackDir { get; set; }

        public CellDir BaseToPrism(CellDir baseDir)
        {
            if(BaseToPrismDict == null)
            {
                return baseDir;
            }
            return BaseToPrismDict[baseDir];
        }

        public static PrismInfo Get(ICellType baseCellType)
        {
            if (baseCellType == SquareCellType.Instance)
            {
                return squarePrismInfo;
            }
            if(baseCellType == HexCellType.Get(HexOrientation.FlatTopped))
            {
                return ftHexPrismInfo;
            }
            if(baseCellType == HexCellType.Get(HexOrientation.PointyTopped))
            {
                return ptHexPrismInfo;
            }
            if(baseCellType is XZCellModifier xzCellModifier)
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
                    PrismCellType = XZCellModifier.Get(uPrismInfo.PrismCellType),
                    BackDir = uPrismInfo.BackDir,
                    ForwardDir = uPrismInfo.ForwardDir,
                    BaseToPrismDict = uPrismInfo.BaseToPrismDict,
                };
            }
            throw new NotImplementedException($"No prism info for cell type {baseCellType}");
        }
    }
}
