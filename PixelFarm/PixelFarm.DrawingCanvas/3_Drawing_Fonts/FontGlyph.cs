//MIT, 2014-2017, WinterDev
//----------------------------------- 

using System;
using System.Runtime.InteropServices;
using PixelFarm.Agg;
namespace PixelFarm.Drawing.Fonts
{
    /// <summary>
    /// provide information about a glyph
    /// </summary>
    public class FontGlyph
    {

        public GlyphMatrix glyphMatrix; 
        /// <summary>
        /// 32 bpp image for render
        /// </summary>
        public ActualImage glyphImage32;
        //----------------------------
        /// <summary>
        /// original glyph outline
        /// </summary>
        public VertexStore originalVxs;
        /// <summary>
        /// flaten version of original glyph outline
        /// </summary>
        public VertexStore flattenVxs;
        //----------------------------
        //metrics
        public int horiz_adv_x;
        public string glyphName;
        public int unicode;
        /// <summary>
        /// code point/glyph index?
        /// </summary>
        public int codePoint;
    }



    [StructLayout(LayoutKind.Sequential)]
    public struct GlyphMatrix
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
    }
}