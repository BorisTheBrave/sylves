#if UNITY
using UnityEngine;
#endif

namespace Sylves
{
    public struct RaycastInfo
    {
        public Cell cell;
        public Vector3 point;
        public float distance;
        public CellDir? cellDir;
    }
}
