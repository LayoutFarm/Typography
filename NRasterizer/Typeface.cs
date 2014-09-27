using NRasterizer.Tables;
using System.Collections.Generic;

namespace NRasterizer
{
    public class Typeface
    {
        private readonly List<Glyph> _glyphs;

        internal Typeface(List<Glyph> glyphs)
        {
            _glyphs = glyphs;                        
        }

        public List<Glyph> Glyphs { get { return _glyphs; } }
    }
}
