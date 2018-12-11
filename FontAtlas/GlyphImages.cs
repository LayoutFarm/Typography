//MIT, 2016-present, WinterDev
using System;
using Typography.Contours;
namespace Typography.Rendering
{
    public class GlyphImage
    {
        int[] _pixelBuffer;
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
        public int[] GetImageBuffer() => _pixelBuffer;
        //
        public void SetImageBuffer(int[] pixelBuffer, bool isBigEndian)
        {
            _pixelBuffer = pixelBuffer;
            this.IsBigEndian = isBigEndian;
        }
        /// <summary>
        /// texture offset X from original glyph
        /// </summary>
        public short TextureOffsetX { get; set; }
        /// <summary>
        /// texture offset Y from original glyph 
        /// </summary>
        public short TextureOffsetY { get; set; }
    }

    class CacheGlyph
    {
        internal GlyphImage img;
        public Rectangle area;
        public ushort glyphIndex;

#if DEBUG
        public CacheGlyph()
        {
        }
#endif
    }
    public class TextureGlyphMapData
    {
        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
         
        public float TextureXOffset { get; set; }
        public float TextureYOffset { get; set; }

        public void GetRect(out int x, out int y, out int w, out int h)
        {
            x = Left;
            y = Top;
            w = Width;
            h = Height;
        }
    }

}