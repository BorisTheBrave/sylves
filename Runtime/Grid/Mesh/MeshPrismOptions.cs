

namespace Sylves
{
    public class MeshPrismGridOptions : MeshGridOptions
    {
        public MeshPrismGridOptions() : base() { }

        public MeshPrismGridOptions(MeshPrismGridOptions other) : base(other)
        {
            LayerHeight = other.LayerHeight;
            LayerOffset = other.LayerOffset;
            MinLayer = other.MinLayer;
            MaxLayer = other.MaxLayer;
            SmoothNormals = other.SmoothNormals;
        }

        public float LayerHeight { get; set; } = 1;
        public float LayerOffset { get; set; }
        public int MinLayer { get; set; }
        public int MaxLayer { get; set; } = 1;
        public bool SmoothNormals { get; set; }
    }
}
