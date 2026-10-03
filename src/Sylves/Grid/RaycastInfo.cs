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

        /// <summary>
        /// True when this hit is the ray leaving <see cref="cell"/>, rather than entering it.
        /// Set only for hits produced when exit info was requested.
        /// </summary>
        public bool isExit;
    }
}
