namespace NRasterizer
{
    public interface Segment
    {
        void FillFlags(Raster raster);
        SegmentKind Kind { get; }
    }
    public enum SegmentKind
    {
        Line,
        Bezier
    }
}
