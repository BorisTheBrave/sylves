using System;

namespace Sylves
{
#if !UNITY
    public struct Vector2Int
    {
        public Vector2Int(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public int x { get; set; }
        public int y { get; set; }

        public static Vector2Int down => new Vector2Int(0, -1);
        public static Vector2Int up => new Vector2Int(0, 1);
        public static Vector2Int one => new Vector2Int(1, 1);
        public static Vector2Int zero => new Vector2Int(0, 0);
        public static Vector2Int left => new Vector2Int(-1, 0);
        public static Vector2Int right => new Vector2Int(1, 0);

        public int this[int index]
        {
            get { switch (index) { case 0: return x; case 1: return y; default: throw new IndexOutOfRangeException(); } }
            set { switch (index) { case 0: x = value; break; case 1: y = value; break; default: throw new IndexOutOfRangeException(); } }
        }
        public float magnitude => Mathf.Sqrt(sqrMagnitude);
        public int sqrMagnitude => x * x + y * y;

        public static Vector2Int CeilToInt(Vector3 v) => new Vector2Int(Mathf.CeilToInt(v.x), Mathf.CeilToInt(v.y));
        public static float Distance(Vector2Int a, Vector2Int b) => (a - b).magnitude;
        public static Vector2Int FloorToInt(Vector3 v) => new Vector2Int(Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.y));
        public static Vector2Int Max(Vector2Int lhs, Vector2Int rhs) => new Vector2Int(Math.Max(lhs.x, rhs.x), Math.Max(lhs.y, rhs.y));
        public static Vector2Int Min(Vector2Int lhs, Vector2Int rhs) => new Vector2Int(Math.Min(lhs.x, rhs.x), Math.Min(lhs.y, rhs.y));
        public static Vector2Int RoundToInt(Vector3 v) => new Vector2Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));
        public static Vector2Int Scale(Vector2Int a, Vector2Int b) => new Vector2Int(a.x * b.x, a.y * b.y);
        public void Clamp(Vector2Int min, Vector2Int max) => Min(Max(this, min), max);
        public bool Equals(Vector2Int other) => x == other.x && y == other.y;
        public override bool Equals(object other)
        {
            if (other is Vector2Int v)
                return Equals(v);
            return false;
        }
        public override int GetHashCode() => (x, y).GetHashCode();
        public void Scale(Vector2Int scale) => Scale(this, scale);
        public void Set(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public override string ToString() => $"({x}, {y})";
        public string ToString(string format) => ToString();

        public static Vector2Int operator +(Vector2Int a, Vector2Int b) => new Vector2Int(a.x + b.x, a.y + b.y);
        public static Vector2Int operator -(Vector2Int a) => new Vector2Int(-a.x, -a.y);
        public static Vector2Int operator -(Vector2Int a, Vector2Int b) => new Vector2Int(a.x - b.x, a.y - b.y);
        public static Vector2Int operator *(Vector2Int a, Vector2Int b) => new Vector2Int(a.x * b.x, a.y * b.y);
        public static Vector2Int operator *(int a, Vector2Int b) => new Vector2Int(a * b.x, a * b.y);
        public static Vector2Int operator *(Vector2Int a, int b) => new Vector2Int(a.x * b, a.y * b);
        public static Vector2Int operator /(Vector2Int a, int b) => new Vector2Int(a.x / b, a.y / b);
        public static bool operator ==(Vector2Int lhs, Vector2Int rhs) => lhs.x == rhs.x && lhs.y == rhs.y;
        public static bool operator !=(Vector2Int lhs, Vector2Int rhs) => !(lhs == rhs);

        public static implicit operator Vector2(Vector2Int v) => new Vector2(v.x, v.y);

        // Ignoring this unity misfeature.
        //public static explicit operator Vector3Int(Vector2Int v);
    }
#endif
}
