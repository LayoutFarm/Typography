namespace NRasterizer
{
    public interface Segment
    {
      
        SegmentKind Kind { get; }
    }
    public enum SegmentKind
    {
        Line,
        Bezier
    }
}
