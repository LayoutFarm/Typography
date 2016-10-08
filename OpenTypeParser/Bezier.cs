using System;

namespace NRasterizer
{
    public class Bezier : Segment
    {
        public readonly float x0;
        public readonly float y0;
        public readonly float x1;
        public readonly float y1;
        public readonly float x2;
        public readonly float y2;

        public Bezier(float x0, float y0, float x1, float y1, float x2, float y2)
        {
            this.x0 = x0;
            this.y0 = y0;
            this.x1 = x1;
            this.y1 = y1;
            this.x2 = x2;
            this.y2 = y2;
        }

        public SegmentKind Kind { get { return SegmentKind.Bezier; } }
    }
}
