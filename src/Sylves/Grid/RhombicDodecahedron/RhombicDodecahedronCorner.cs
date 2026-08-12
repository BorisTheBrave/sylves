namespace Sylves
{
    /// <summary>
    /// Enum of the 14 corners of a rhombic dodecahedron.
    /// 8 three-valent corners correspond to the vertices of a cube,
    /// and 6 four-valent corners correspond to the vertices of an octahedron.
    /// This is literally CubeCorner followed by CubeDir.
    /// </summary>
    public enum RhombicDodecahedronCorner
    {
        BackDownLeft,
        BackDownRight,
        BackUpLeft,
        BackUpRight,
        ForwardDownLeft,
        ForwardDownRight,
        ForwardUpLeft,
        ForwardUpRight,
        Right,
        Left,
        Up,
        Down,
        Forward,
        Back,
    }
}
