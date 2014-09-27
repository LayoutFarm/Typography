using System.Collections.Generic;

namespace NRasterizer
{
    public class Typeface
    {
        private readonly Bounds _bounds;
        private readonly List<Glyph> _glyphs;

        internal Typeface(Bounds bounds, List<Glyph> glyphs)
        {
            _bounds = bounds;
            _glyphs = glyphs;                        
        }

        public Bounds Bounds { get { return _bounds; } }
        public List<Glyph> Glyphs { get { return _glyphs; } }
    }
}
