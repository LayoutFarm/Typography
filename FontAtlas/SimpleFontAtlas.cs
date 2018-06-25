//MIT, 2016-2017, WinterDev
//----------------------------------- 

using System;
using System.Collections.Generic;

using Typography.Rendering;

namespace PixelFarm.Drawing.Fonts
{

    public class SimpleFontAtlas
    {
        GlyphImage totalGlyphImage;
        Dictionary<ushort, TextureFontGlyphData> _glyphIndexLocationMap = new Dictionary<ushort, TextureFontGlyphData>();

        public int Width { get; set; }
        public int Height { get; set; }
        /// <summary>
        /// original font size in point unit
        /// </summary>
        public float OriginalFontSizePts { get; set; }
        public TextureKind TextureKind { get; set; }
        public void AddGlyph(ushort glyphIndex, TextureFontGlyphData glyphData)
        {
            _glyphIndexLocationMap.Add(glyphIndex, glyphData);
        }

        public GlyphImage TotalGlyph
        {
            get { return totalGlyphImage; }
            set { totalGlyphImage = value; }
        }
        public bool TryGetGlyphData(ushort glyphIndex, out TextureFontGlyphData glyphdata)
        {
            if (!_glyphIndexLocationMap.TryGetValue(glyphIndex, out glyphdata))
            {
                glyphdata = null;
                return false;
            }
            return true;
        }


    }

}