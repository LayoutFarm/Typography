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
        Dictionary<int, TextureFontGlyphData> codePointLocations = new Dictionary<int, TextureFontGlyphData>();

        public int Width { get; set; }
        public int Height { get; set; }
        /// <summary>
        /// original font size in point unit
        /// </summary>
        public float OriginalFontSizePts { get; set; }
        public TextureKind TextureKind { get; set; }
        public void AddGlyph(int codePoint, TextureFontGlyphData glyphData)
        {
            codePointLocations.Add(codePoint, glyphData);
        }

        public GlyphImage TotalGlyph
        {
            get { return totalGlyphImage; }
            set { totalGlyphImage = value; }
        }
        public bool TryGetGlyphDataByCodePoint(int codepoint, out TextureFontGlyphData glyphdata)
        {
            if (!codePointLocations.TryGetValue(codepoint, out glyphdata))
            {
                glyphdata = null;
                return false;
            }
            return true;
        }


    }

}