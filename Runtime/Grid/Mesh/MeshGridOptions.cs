

namespace Sylves
{
    public class MeshGridOptions
    {
        public MeshGridOptions()
        {

        }

        public MeshGridOptions(MeshGridOptions other)
        {
            UseXZPlane = other.UseXZPlane;
            InvertWinding = other.InvertWinding;
            DoubleOddFaces = other.DoubleOddFaces;
            Tolerance = other.Tolerance;
        }

        /// <summary>
        /// If set, assumes the 2d plane that a face maps from is in the XZ axis.
        /// </summary>
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

        /// <summary>
        /// If set, odd faces become cells with twice as many edges.
        /// It's often more convenient to work with even polygons as an about face is possible.
        /// </summary>
        public bool DoubleOddFaces { get; set; }

        /// <summary>
        /// Snap distance for vertices.
        /// </summary>
        public float Tolerance { get; set; } = MeshDataOperations.DefaultTolerance;
    }
}
