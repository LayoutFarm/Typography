using System;
using Typography.Contours;
namespace Typography.Rendering
{
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
        /// <summary>
        /// texture offset X from original glyph
        /// </summary>
        public double TextureOffsetX { get; set; }
        /// <summary>
        /// texture offset Y from original glyph 
        /// </summary>
        public double TextureOffsetY { get; set; }
    }

    public class CacheGlyph
    {
        public int borderX;
        public int borderY;
        public GlyphImage img;
        public Rectangle area;
        public char character;
        public int codePoint;
        public GlyphMatrix2 glyphMatrix;
    
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
        //---
        public double TextureXOffset { get; set; }
        public double TextureYOffset { get; set; }

        public Rectangle Rect
        {
            get;
            set;
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
        //public IntPtr bitmap;
        //public IntPtr outline;
    }
}