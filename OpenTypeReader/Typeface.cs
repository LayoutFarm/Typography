//Apache2, 2014-2016, Samuel Carlsson, WinterDev

using System.Collections.Generic;
using NRasterizer.Tables;
namespace NRasterizer
{
    public class Typeface
    {
        readonly Bounds _bounds;
        readonly ushort _unitsPerEm;
        readonly List<Glyph> _glyphs;
        readonly List<CharacterMap> _cmaps;
        readonly HorizontalMetrics _horizontalMetrics;
        readonly NameEntry _nameEntry;
        readonly Kern _kern;
        internal Typeface(
            NameEntry nameEntry,
            Bounds bounds,
            ushort unitsPerEm,
            List<Glyph> glyphs,
            List<CharacterMap> cmaps,
            HorizontalMetrics horizontalMetrics,
            Kern kern)
        {
            _nameEntry = nameEntry;
            _bounds = bounds;
            _unitsPerEm = unitsPerEm;
            _glyphs = glyphs;
            _cmaps = cmaps;
            _horizontalMetrics = horizontalMetrics;

            _kern = kern;
        }
        public string Name
        {
            get { return _nameEntry.FontName; }
        }
        public string FontSubFamily
        {
            get { return _nameEntry.FontSubFamily; }
        }
        public int LookupIndex(char character)
        {
            // TODO: What if there are none or several tables?
            return _cmaps[0].CharacterToGlyphIndex(character);
        }

        public Glyph Lookup(char character)
        {
            return _glyphs[LookupIndex(character)];
        }

        public ushort GetAdvanceWidth(char character)
        {
            return _horizontalMetrics.GetAdvanceWidth(LookupIndex(character));
        }
        public short GetKernDistance(ushort leftGlyphIndex, ushort rightGlyphIndex)
        {
            return _kern.GetKerningDistance(leftGlyphIndex, rightGlyphIndex);
        }
        public Bounds Bounds { get { return _bounds; } }
        public ushort UnitsPerEm { get { return _unitsPerEm; } }
        public List<Glyph> Glyphs { get { return _glyphs; } }

        //-------------------------------------------------------
    }
}
