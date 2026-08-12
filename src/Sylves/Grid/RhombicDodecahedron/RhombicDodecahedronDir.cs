namespace Sylves
{
    /// <summary>
    /// Enum of the 12 faces of a rhombic dodecahedron.
    /// Faces correspond to the 12 nearest-neighbor directions of the FCC lattice:
    /// (±1, ±1, 0), (±1, 0, ±1), (0, ±1, ±1).
    /// Opposite faces are paired so that <see cref="RhombicDodecahedronDirExtensions.Inverted"/> is XOR 1.
    /// </summary>
    public enum RhombicDodecahedronDir
    {
        RightUp,
        LeftDown,
        RightDown,
        LeftUp,
        RightForward,
        LeftBack,
        RightBack,
        LeftForward,
        UpForward,
        DownBack,
        UpBack,
        DownForward,
    }
}
