//MIT, 2016-present, WinterDev
using System;

using PixelFarm.Drawing;
namespace PixelFarm.CpuBlit.BitmapAtlas
{

    public class BitmapAtlasItem : SpriteTextureMapData<int[]>
    {
        public BitmapAtlasItem(int w, int h) : base(0, 0, w, h) { }

        public int[] GetImageBuffer() => Source;
        public bool IsBigEndian { get; set; }
        public void SetImageBuffer(int[] imgBuffer, bool isBigEndian = false)
        {
            Source = imgBuffer;
            IsBigEndian = isBigEndian;
        }
    }

    class RelocationAtlasItem
    {
        public readonly ushort glyphIndex;
        internal readonly BitmapAtlasItem atlasItem;
        public Rectangle area;
        public RelocationAtlasItem(ushort glyphIndex, BitmapAtlasItem atlasItem)
        {
            this.glyphIndex = glyphIndex;
            this.atlasItem = atlasItem;
        }
#if DEBUG
        public override string ToString()
        {
            return glyphIndex.ToString();
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
#if DEBUG
        public override string ToString()
        {
            return "(" + Left + "," + Top + "," + Width + "," + Height + ")";
        }
#endif
    }

}