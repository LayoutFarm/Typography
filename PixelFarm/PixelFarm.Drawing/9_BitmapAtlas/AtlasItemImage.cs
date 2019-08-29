//MIT, 2016-present, WinterDev
using System;
using PixelFarm.CpuBlit;
namespace PixelFarm.Drawing.BitmapAtlas
{
    public class AtlasItemImage
    {
        MemBitmap _bmp;
        public AtlasItemImage(int w, int h)
        {
            this.Width = w;
            this.Height = h;
        }
        public RectangleF OriginalBounds
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
        public MemBitmap Bitmap => _bmp;
        //
        public void SetBitmap(MemBitmap bmp, bool isBigEndian)
        {
            _bmp = bmp;
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

    class CacheBmp
    {
        internal AtlasItemImage img;
        public Rectangle area;
        public ushort imgIndex;

#if DEBUG
        public CacheBmp()
        {
        }
#endif
    }

    public class BitmapMapData
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