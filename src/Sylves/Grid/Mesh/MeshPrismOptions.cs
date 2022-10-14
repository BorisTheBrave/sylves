#if UNITY
#endif


namespace Sylves
{
    public class MeshPrismGridOptions : MeshGridOptions
    {
        public float LayerHeight { get; set; } = 1;
        public float LayerOffset { get; set; }
        public int MinLayer { get; set; }
        public int MaxLayer { get; set; } = 1;
        public bool SmoothNormals { get; set; }
    }
}
