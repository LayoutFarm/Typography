using System;
using System.Text;

namespace NRasterizer
{
    public class Rasterizer
    {
        private readonly Typeface _typeface;

        public Rasterizer(Typeface typeface)
        {
            _typeface = typeface;
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

        private void SetScanFlags(Glyph glyph, Raster scanFlags)
        {
            var pixels = scanFlags.Pixels;

            var allX = glyph.X;
            var allY = glyph.Y;
            var allOn = glyph.On;

            for (int contour = 0; contour < glyph.ContourCount; contour++)
            {
                var begin = glyph.GetContourBegin(contour);
                var end = glyph.GetContourEnd(contour);
                var contourLength = end - begin;
                for (int i = 0; i < contourLength; i++)
                {
                    var x0 = allX[begin + i];
                    var y0 = allY[begin + i];
                    var on1 = allOn[begin + i];

                    var x1 = allX[begin + (i + 1) % contourLength];
                    var y1 = allY[begin + (i + 1) % contourLength];
                    var on2 = allOn[begin + (i + 1) % contourLength];

                    if (on2)
                    {
                        const int scaleShift = 3;
                        // draw line in flags
                        DrawLineFlags(scanFlags, x0 >> scaleShift, y0 >> scaleShift, x1 >> scaleShift, y1 >> scaleShift);
                    }
                    else
                    {
                        const int scaleShift = 3;
                        DrawLineFlags(scanFlags, x0 >> scaleShift, y0 >> scaleShift, x1 >> scaleShift, y1 >> scaleShift);
                        Console.WriteLine("bezier!");
                    }
                }
            }
        }

        private void RenderScanlines(Raster scanFlags, Raster target)
        {
            var source = scanFlags.Pixels;
            var destinataion = target.Pixels;
            var stride = target.Stride;
            
            for (int y = 0; y < target.Height; y++)
            {
                bool fill = false;
                int row = stride * y;
                for (int x = 0; x < target.Width; x++)
                {
                    if (source[row + x] > 0)
                    {
                        fill = !fill;
                    }
                    destinataion[row + x] = fill ? (byte)255 : (byte)0;
                }
            }
        }

        private void Rasterize(Glyph glyph, Raster raster)
        {
            var flags = new Raster(raster.Width, raster.Height, raster.Stride);
            SetScanFlags(glyph, flags);
            RenderScanlines(flags, raster);

            //SetScanFlags(glyph, raster);
        }

        public void Rasterize(string text, int size, Raster raster)
        {
            foreach (var character in text)
            {
                var glyph = _typeface.Lookup(character);
                Rasterize(glyph, raster);
            }
        }
    }
}
