using System;
using System.Text;

namespace NRasterizer
{
    public class Rasterizer
    {
        private readonly Typeface _typeface;
        private const int pointsPerInch = 72;

        public Rasterizer(Typeface typeface)
        {
            _typeface = typeface;
        }

        private void SetScanFlags(Glyph glyph, Raster scanFlags, int size)
        {
            float scale = (float)(size * scanFlags.Resolution) / (pointsPerInch * _typeface.UnitsPerEm);
            var pixels = scanFlags.Pixels;
            for (int contour = 0; contour < glyph.ContourCount; contour++)
            {
                foreach (var segment in glyph.GetContourIterator(contour, 0, 120, scale, -scale))
                {
                    segment.FillFlags(scanFlags);
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

        private void Rasterize(Glyph glyph, int size, Raster raster)
        {
            var flags = new Raster(raster.Width, raster.Height, raster.Stride, raster.Resolution);
            SetScanFlags(glyph, flags, size);
            RenderScanlines(flags, raster);

            //SetScanFlags(glyph, raster);
        }

        public void Rasterize(string text, int size, Raster raster)
        {
            foreach (var character in text)
            {
                var glyph = _typeface.Lookup(character);
                Rasterize(glyph, size, raster);
            }
        }
    }
}
