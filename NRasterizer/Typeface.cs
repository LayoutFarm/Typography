using notf.Tables;
using System.Collections.Generic;

namespace notf
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
