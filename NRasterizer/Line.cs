namespace NRasterizer
{
    public class Line: Segment
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

        public void FillFlags(Raster target)
        {
            DrawLineFlags(target,
                x0,
                y0,
                x1,
                y1);
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
                //raster.SetPixel(x0, y0, 255);
                //raster.SetPixel(x1, y1, 255);
                return;
            }

            float error = 0;
            float deltaError = (float)deltax / (float)deltay;

            int x = x0;
            for (int y = y0; y < y1; y++)
            {
                raster.AddPixel(x, y, 1);
                error += deltaError;
                if (error > 0.5)
                {
                    x++;
                    error -= 1.0f;
                }
                if (error < 0.5)
                {
                    x--;
                    error += 1.0f;
                }
            }
        }

        public override string ToString()
        {
            return string.Format("Line ({0}, {1}) to ({2}, {3})", x0, y0, x1, y1);
        }
    }
}
