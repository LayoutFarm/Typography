//MIT, 2019-present, WinterDev
using System;

namespace Typography.OpenFont.Tables
{
    class BitmapFontGlyphSource
    {
        CBLC _cblc;
        CBDT _cbdt;
        public BitmapFontGlyphSource(CBLC cblc, CBDT cbdt)
        {
            _cblc = cblc;
            _cbdt = cbdt;
        }
        public void CopyBitmapContent(Glyph glyph, System.IO.Stream outputStream)
        {
            _cbdt.CopyBitmapContent(glyph, outputStream);
        }
        public Glyph[] BuildGlyphList()
        {
            Glyph[] glyphs = _cblc.BuildGlyphList();
            for (int i = 0; i < glyphs.Length; ++i)
            {
                _cbdt.FillGlyphInfo(glyphs[i]);
            }
            return glyphs;
        }
    }
}