#if UNITY
using UnityEngine;
#endif

namespace Sylves
{
    public enum MeshTopology
    {
        Triangles = 0,
        Quads = 2,
        //Lines = 3,
        //LineStrip = 4,
        //Points = 5

        // Final index of each face is bit inverted.
        NGon = -1,
    }
}
