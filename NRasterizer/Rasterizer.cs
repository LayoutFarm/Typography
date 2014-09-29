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


        private void SetScanFlags(Glyph glyph, Raster scanFlags)
        {
            var pixels = scanFlags.Pixels;

            for (int contour = 0; contour < glyph.ContourCount; contour++)
            {
                foreach (var segment in glyph.GetContourIterator(contour))
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

        private void Rasterize(Glyph glyph, Raster raster)
        {
            var flags = new Raster(raster.Width, raster.Height, raster.Stride);
            //SetScanFlags(glyph, flags);
            //RenderScanlines(flags, raster);

            SetScanFlags(glyph, raster);
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
