using UnityEngine;

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
