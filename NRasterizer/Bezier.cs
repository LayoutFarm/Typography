using System;

namespace NRasterizer
{
    public class Bezier: Segment
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

        // Hack to get WPF binding working properly
        public Bezier Me { get { return this; } }

        public void FillFlags(Raster raster)
        {
            if ((int)y0 == (int)y1 && (int)y1 == (int)y2)
            {
                // all on the same horizontal line -> discard
                return;
            }

            if ((int)x0 == (int)x1 && (int)x1 == (int)x2)
            {
                // all on same vertical line -> draw vertical line
                int start = (int)Math.Min(Math.Min(y0, y1), y2);
                int end = (int)Math.Max(Math.Max(y0, y1), y2);
                for (int y = start; y < end; y++)
                {
                    raster.AddPixel((int)x0, y, 1);
                }
                return;
            }

            // Subdivide
            float x01 = (x0 + x1) / 2;
            float y01 = (y0 + y1) / 2;
            float x12 = (x1 + x2) / 2;
            float y12 = (y1 + y2) / 2;

            float x012 = (x01 + x12) / 2;
            float y012 = (y01 + y12) / 2;

            new Bezier(x0, y0, x01, y01, x012, y012).FillFlags(raster);
            new Bezier(x012, y012, x12, y12, x2, y2).FillFlags(raster);
        }
    }
}
