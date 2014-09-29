namespace NRasterizer
{
    public class Line: Segment
    {
        public Line(short x0, short y0, short x1, short y1, bool on)
        {
            this.x0 = x0;
            this.y0 = y0;
            this.x1 = x1;
            this.y1 = y1;
            this.on = on;
        }
        public readonly short x0;
        public readonly short y0;
        public readonly short x1;
        public readonly short y1;
        public readonly bool on;

        public void FillFlags(Raster target)
        {
            const int scaleShift = 3;
            const int yOffset = 256;
            DrawLineFlags(target,
                x0 >> scaleShift,
                yOffset + y0 >> scaleShift,
                x1 >> scaleShift,
                yOffset + y1 >> scaleShift);
        }

        private void Swap<T>(ref T a, ref T b)
        {
            T tmp = a;
            a = b;
            b = tmp;
        }
        private void DrawLineFlags(Raster raster, int x0, int y0, int x1, int y1)
        {
            if (y0 > y1)
            {
                Swap(ref x0, ref x1);
                Swap(ref y0, ref y1);
            }

            int deltax = x1 - x0;
            int deltay = y1 - y0;

            if (deltay == 0)
            {
                raster.SetPixel(x0, y0, 255);
                raster.SetPixel(x1, y1, 255);
                return;
            }

            float error = 0;
            float deltaError = (float)deltax / (float)deltay;

            int x = x0;
            for (int y = y0; y < y1; y++)
            {
                raster.SetPixel(x, y, 255);
                error += deltaError;
                if (error >= 0.5)
                {
                    x++;
                    error -= 1.0f;
                }
                if (error <= 0.5)
                {
                    x--;
                    error += 1.0f;
                }
            }
        }
    }
}
