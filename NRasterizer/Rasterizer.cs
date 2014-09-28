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

        private void Rasterize(Glyph glyph, Raster raster)
        {

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
