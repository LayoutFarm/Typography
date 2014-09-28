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

        private void Swap(ref short a, ref short b)
        {
            short tmp = a;
            a = b;
            b = tmp;
        }

        // TODO: Use beshenham
        private void DrawLine(Raster raster, short x0, short y0, short x1, short y1)
        {
            int xDistance = Math.Abs(x1 - x0);
            int yDistance = Math.Abs(y1 - y0);

            if (xDistance >= yDistance)
            {
                if (x0 > x1)
                {
                    Swap(ref x0, ref x1);
                    Swap(ref y0, ref y1);
                }
                int dx = x1 - x0;
                int dy = y1 - y0;
                for (int x = x0; x < x1; x++)
                {
                    short y = (short)(y0 + (x-x0) * dy / dx);
                    raster.SetPixel(x >> 3, y >> 3, 255);
                }
            }
            else
            {
                if (y0 > y1)
                {
                    Swap(ref x0, ref x1);
                    Swap(ref y0, ref y1);
                }
                int dx = x1 - x0;
                int dy = y1 - y0;
                for (int y = y0; y < y1; y++)
                {
                    short x = (short)(x0 + (y - y0) * dx / dy);
                    raster.SetPixel(x >> 3, y >> 3, 255);
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
                for (int i = begin + 1; i < end; i++)
                {
                    var x0 = allX[i - 1];
                    var y0 = allY[i - 1];
                    var on1 = allOn[i - 1];

                    var x1 = allX[i];
                    var y1 = allY[i];
                    var on2 = allOn[i];

                    if (on2)
                    {
                        // draw line in flags
                        DrawLine(scanFlags, x0, y0, x1, y1);
                    }
                    else
                    {
                        DrawLine(scanFlags, x0, y0, x1, y1); // TODO: Draw bezier
                        Console.WriteLine("bezier!");
                    }
                }
            }
        }

        private void RenderScanlines(Raster scanFlags, Raster target)
        {
        }

        private void Rasterize(Glyph glyph, Raster raster)
        {
            var flags = new Raster(raster.Width, raster.Height, raster.Stride);
            SetScanFlags(glyph, raster);
            //RenderScanlines(flags, raster);
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
