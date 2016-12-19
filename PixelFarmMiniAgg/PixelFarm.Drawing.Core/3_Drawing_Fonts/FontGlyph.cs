//MIT, 2014-2016, WinterDev
//-----------------------------------
//use FreeType and HarfBuzz wrapper
//native dll lib
//plan?: port  them to C#  :)
//-----------------------------------

using System;
using System.Runtime.InteropServices;
using PixelFarm.Agg;
namespace PixelFarm.Drawing.Fonts
{
    public class FontGlyph
    {
        public GlyphMatrix glyphMatrix;
        /// <summary>
        /// original 8bpp image buffer
        /// </summary>
        public byte[] glyImgBuffer8;
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
    public struct ProperGlyph
    {
        public uint codepoint;
        public int x_advance;
        public int y_advance;
        public int x_offset;
        public int y_offset;
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
        public IntPtr bitmap;
        public IntPtr outline;
    }

    public class GlyphImage
    {
        int[] pixelBuffer;
        public GlyphImage(int w, int h)
        {
            this.Width = w;
            this.Height = h;
        }
        public RectangleF OriginalGlyphBounds
        {
            get;
            set;
        }
        public int Width
        {
            get;
            private set;
        }
        public int Height
        {
            get;
            private set;
        }
        public bool IsBigEndian
        {
            get;
            private set;
        }
        public int BorderXY
        {
            get;
            set;
        }
        public int[] GetImageBuffer()
        {
            return pixelBuffer;
        }
        public void SetImageBuffer(int[] pixelBuffer, bool isBigEndian)
        {
            this.pixelBuffer = pixelBuffer;
            this.IsBigEndian = isBigEndian;
        }
    }

    struct CharAndGlyphMap
    {
        public readonly uint glyphIndex;
        public readonly char charcode;
        public CharAndGlyphMap(uint glyphIndex, char charcode)
        {
            this.charcode = charcode;
            this.glyphIndex = glyphIndex;
        }
        public override string ToString()
        {
            return glyphIndex + ":" + charcode.ToString() + "(" + ((int)charcode).ToString() + ")";
        }

    }
}