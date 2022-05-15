#if UNITY
#endif


namespace Sylves
{
    public class MeshGridOptions
    {
        public bool UseXZPlane { get; set; }

        /// <summary>
        /// If false, vertices and edges of the mesh are assumed to be consistent with Sylves conventions,
        /// counter clockwise winding.
        /// E.g. for a quad, edges 0 => SquareDir.Right, 1 => SquareDir.Up, 2 => SquareDir.Left, 3 => SquareDir.Down
        ///                  verts 0 => DownRight, 1 => UpRight, 2 => UpLeft, 3=> DownLeft
        /// 
        /// If true, the order of vertices is swapped
        /// E.g. for a quad, edges 0 => Square.Left, 1 => SquareDir.Up, 2 => SquareDir.Right, 3 => SquareDir.Down
        ///                  verts 0 => DownLeft, 1 => UpLeft, 2=> UpRight, 3 = DownRight
        /// </summary>
        public bool InvertWinding { get; set; }
    }
}
