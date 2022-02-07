namespace Sylves
{
    public class SquareBound : IBound
    {
        public Vector2Int min;
        public Vector2Int max;

        public SquareBound(Vector2Int min, Vector2Int max)
        {
            this.min = min;
            this.max = max;
        }

        public Vector2Int size => max - min;

        public bool Contains(Vector2Int v)
        {
            return v.x >= min.x && v.y >= min.y && v.x < max.x && v.y < max.y;
        }

        public SquareBound Intersect(SquareBound other)
        {
            return new SquareBound(Vector2Int.Max(min, other.min), Vector2Int.Min(max, other.max));
        }
        public SquareBound Union(SquareBound other)
        {
            return new SquareBound(Vector2Int.Min(min, other.min), Vector2Int.Max(max, other.max));
        }
    }
}
