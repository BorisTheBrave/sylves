using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
#if UNITY
using UnityEngine;
#endif

using static Sylves.MathUtils;

namespace Sylves
{
    public struct EisensteinInteger
    {
        /// <summary>
        /// Adds 1 to the complex numer 
        /// </summary>
        public int x;

        /// <summary>
        /// Adds (1 + sqrt(-3)) / 2 to the complex number
        /// </summary>
        public int y;

        public EisensteinInteger(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static implicit operator EisensteinInteger(int x) => new EisensteinInteger(x, 0);
        public static implicit operator EisensteinInteger(Vector2Int v) => new EisensteinInteger(v.x, v.y);
        public static implicit operator EisensteinInteger(Cell v) => new EisensteinInteger(v.x, v.y);
        public static implicit operator Vector2Int(EisensteinInteger e) => new Vector2Int(e.x, e.y);
        public static implicit operator Cell(EisensteinInteger e) => new Cell(e.x, e.y, -e.x - e.y);
        public static implicit operator Complex(EisensteinInteger e) => new Complex(e.x + 0.5 * e.y, 0.5 * Mathf.Sqrt(3) * e.y);
        

        public static EisensteinInteger operator +(EisensteinInteger a, EisensteinInteger b) => new EisensteinInteger(a.x + b.x, a.y + b.y);
        public static EisensteinInteger operator -(EisensteinInteger a, EisensteinInteger b) => new EisensteinInteger(a.x - b.x, a.y - b.y);
        public static EisensteinInteger operator *(EisensteinInteger a, EisensteinInteger b) => new EisensteinInteger(a.x*b.x + -a.y*b.y, a.y*b.x + a.x*b.y + a.y*b.y);
        public static bool operator ==(EisensteinInteger a, EisensteinInteger b) => a.x == b.x && a.y == b.y;
        public static bool operator !=(EisensteinInteger a, EisensteinInteger b) => a.x != b.x || a.y != b.y;
        public override bool Equals(object obj) => obj is EisensteinInteger e && this == e;
        public override int GetHashCode() => (x, y).GetHashCode();

        public override string ToString() => $"({x}, {y})";

        public int Norm() =>  x*x + x*y +y*y;

        public static EisensteinInteger RoundedDivision(EisensteinInteger a, EisensteinInteger b)
        {
            var bNorm = b.Norm();
            var unscaled = a * new EisensteinInteger(b.x + b.y, -b.y);
            return new EisensteinInteger(
                (int)Math.Round((double)unscaled.x / bNorm),
                (int)Math.Round((double)unscaled.y / bNorm));
        }

        /// <summary>
        /// Gets the EisensteinInteger closest to zero that equals a in the given modulus.
        /// </summary>
        public static EisensteinInteger GetRepresentative(EisensteinInteger a, EisensteinInteger modulus)
        {
            var rounded_quotient = RoundedDivision(a,modulus);
            var nearest_multiple = modulus * rounded_quotient;
            return a - nearest_multiple;
        }
    }


    /// <summary>
    /// A hex grid where each cell is treated as an eisenstein integer, a form of complex numbers.
    /// These are then wrapped given a modulus, so that the grid is topologically a torus.
    /// 
    /// Essentially, these are just hex grids that wrap with a rhombus shape, but you can do some neat maths with them
    /// as explained in this video https://www.youtube.com/watch?v=8_WPBuYYz9M&t=206s
    /// Use the EisensteinInteger struct if you want to work with the cells in their complex number form.
    /// </summary>
    public class ModHexGrid : WrapModifier
    {
        public ModHexGrid(float cellSize, EisensteinInteger modulus, HexOrientation orientation = HexOrientation.PointyTopped) 
            : base(
                  CreateMasked(cellSize, modulus, orientation), 
                  c =>(Cell)EisensteinInteger.GetRepresentative(c, modulus))
        {
        }

        private static IGrid CreateMasked(float cellSize, EisensteinInteger modulus, HexOrientation orientation = HexOrientation.PointyTopped)
        {
            var hexGrid = new HexGrid(cellSize, orientation);
            var cover = HexBound.Hexagon(modulus.Norm() + 1); // I haven't really validated this, seems sensible.
            var cells = new HashSet<Cell>(cover.Select(c => (Cell)EisensteinInteger.GetRepresentative(c, modulus)));
            return new MaskModifier(hexGrid, cells);
        }
    }
}
