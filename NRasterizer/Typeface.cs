using System.Collections.Generic;

namespace NRasterizer
{
    public class Typeface
    {
        private readonly Bounds _bounds;
        private readonly List<Glyph> _glyphs;
        private readonly List<CharacterMap> _cmaps;

        internal Typeface(Bounds bounds, List<Glyph> glyphs, List<CharacterMap> cmaps)
        {
            _bounds = bounds;
            _glyphs = glyphs;
            _cmaps = cmaps;
        }

        public int LookupIndex(uint character)
        {
            // TODO: What if there are none or several tables?
            return _cmaps[0].CharacterToGlyphIndex(character);
            //return _glyphs[index];
        }

        public Bounds Bounds { get { return _bounds; } }
        public List<Glyph> Glyphs { get { return _glyphs; } }
    }
}
