using UnityEngine;

namespace Sylves
{
    public enum MeshTopology
    {
        Triangles = 0,
        Quads = 2,
        //Lines = 3,
        //LineStrip = 4,
        //Points = 5

        // Represents ngons of arbitrary number of sides
        // The last index in each ngon is bit inverted (~index) to mark it.
        // There's no equivalent in Unity for this, it needs triangulation before
        // it can be used.
        NGon = -1,
    }
}
