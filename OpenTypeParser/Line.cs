namespace NRasterizer
{
    public class Line : Segment
    {
        public Line(int x0, int y0, int x1, int y1)
        {
            this.x0 = x0;
            this.y0 = y0;
            this.x1 = x1;
            this.y1 = y1;
        }
        public readonly int x0;
        public readonly int y0;
        public readonly int x1;
        public readonly int y1;

        public int X0 { get { return x0; } }
        public int Y0 { get { return y0; } }
        public int X1 { get { return x1; } }
        public int Y1 { get { return y1; } }

        public SegmentKind Kind { get { return SegmentKind.Line; } }
        public override string ToString()
        {
            return string.Format("Line ({0}, {1}) to ({2}, {3})", x0, y0, x1, y1);
        }

    }
}
