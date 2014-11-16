using System;
using System.Collections.Generic;
using System.Linq;
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

        private void SetScanFlags(Glyph glyph, Raster scanFlags, int fx, int fy, int size, int x, int y)
        {
            float scale = (float)(size * scanFlags.Resolution) / (pointsPerInch * _typeface.UnitsPerEm);
            var pixels = scanFlags.Pixels;
            for (int contour = 0; contour < glyph.ContourCount; contour++)
            {
                var aerg = glyph.GetContourIterator(contour, fx, fy, x, y, scale, -scale).ToList();
                foreach (var segment in glyph.GetContourIterator(contour, fx, fy, x, y, scale, -scale))
                {
                    segment.FillFlags(scanFlags);
                }
            }
        }

        private void RenderFlags(Raster scanFlags, Raster target)
        {            
            var source = scanFlags.Pixels;
            var destinataion = target.Pixels;
            var stride = target.Stride;
            for (int y = 0; y < target.Height; y++)
            {
                int row = stride * y;
                for (int x = 0; x < target.Width; x++)
                {
                    destinataion[row + x] = (byte)(source[row + x] * 128);
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
        
        public void Rasterize(string text, int size, Raster raster, bool toFlags = false)
        {
            var flags = new Raster(raster.Width, raster.Height, raster.Stride, raster.Resolution);
            
            // 
            int fx = 64;
            int fy = 0;
            foreach (var character in text)
            {
                var glyph = _typeface.Lookup(character);
                SetScanFlags(glyph, flags, fx, fy, size, 0, 70);
                fx += _typeface.GetAdvanceWidth(character);
            }

            if (toFlags)
            {
                RenderFlags(flags, raster);
            }
            else
            {
                RenderScanlines(flags, raster);
            }
        }

        // TODO: Duplicated code from Rasterize & SetScanFlags
        public IEnumerable<Segment> GetAllSegments(string text, int size, int resolution)
        {
            int x = 0;
            int y = 70;

            // 
            int fx = 64;
            int fy = 0;
            foreach (var character in text)
            {
                var glyph = _typeface.Lookup(character);

                float scale = (float)(size * resolution) / (pointsPerInch * _typeface.UnitsPerEm);
                for (int contour = 0; contour < glyph.ContourCount; contour++)
                {
                    var aerg = glyph.GetContourIterator(contour, fx, fy, x, y, scale, -scale).ToList();
                    foreach (var segment in glyph.GetContourIterator(contour, fx, fy, x, y, scale, -scale))
                    {
                        yield return segment;
                    }
                }

                fx += _typeface.GetAdvanceWidth(character);
            }
        }
    }
}
