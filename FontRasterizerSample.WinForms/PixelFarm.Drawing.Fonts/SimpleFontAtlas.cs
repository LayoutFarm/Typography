//MIT, 2016-2017, WinterDev
//----------------------------------- 

using System;
using System.Collections.Generic;


namespace PixelFarm.Drawing.Fonts
{


    class SimpleFontAtlas
    {
        GlyphImage totalGlyphImage;
        Dictionary<int, TextureFontGlyphData> codePointLocations = new Dictionary<int, TextureFontGlyphData>();

        public int Width { get; set; }
        public int Height { get; set; }

        public void AddGlyph(int codePoint, TextureFontGlyphData glyphData)
        {
            codePointLocations.Add(codePoint, glyphData);
        }

        public GlyphImage TotalGlyph
        {
            get { return totalGlyphImage; }
            set { totalGlyphImage = value; }
        }
        public bool GetRectByCodePoint(int codepoint, out TextureFontGlyphData glyphdata)
        {
            if (!codePointLocations.TryGetValue(codepoint, out glyphdata))
            {
                glyphdata = null;
                return false;
            }
            return true;
        }

    }

    public class TextureFontGlyphData
    {
        public float BorderX { get; set; }
        public float BorderY { get; set; }
        public float AdvanceX { get; set; }
        public float AdvanceY { get; set; }
        public float BBoxXMin { get; set; }
        public float BBoxXMax { get; set; }
        public float BBoxYMin { get; set; }
        public float BBoxYMax { get; set; }
        public float ImgWidth { get; set; }
        public float ImgHeight { get; set; }
        //-----
        public float HAdvance { get; set; }
        public float HBearingX { get; set; }
        public float HBearingY { get; set; }
        //-----
        public float VAdvance { get; set; }
        public float VBearingX { get; set; }
        public float VBearingY { get; set; }


        public Rectangle Rect
        {
            get;
            set;
        }

    }
    public class GlyphData
    {
        public FontGlyph fontGlyph;
        public GlyphImage glyphImage;
        public Rectangle pxArea;
        public char character;
        public int codePoint;
        public GlyphData(int codePoint, char c, FontGlyph fontGlyph, GlyphImage glyphImage)
        {
            this.codePoint = codePoint;
            this.character = c;
            this.fontGlyph = fontGlyph;
            this.glyphImage = glyphImage;

        }
    }
    public struct GlyphMatrix2
    {
        public short unit_per_em;
        public short ascender;
        public short descender;
        public short height;
        public int advanceX;
        public int advanceY;
        public int bboxXmin;
        public int bboxXmax;
        public int bboxYmin;
        public int bboxYmax;
        public int img_width;
        public int img_height;
        public int img_horiBearingX;
        public int img_horiBearingY;
        public int img_horiAdvance;
        public int img_vertBearingX;
        public int img_vertBearingY;
        public int img_vertAdvance;
        public int bitmap_left;
        public int bitmap_top;
        public IntPtr bitmap;
        public IntPtr outline;
    }

}