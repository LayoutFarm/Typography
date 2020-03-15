//MIT, 2016-present, WinterDev

namespace PixelFarm.CpuBlit.BitmapAtlas
{
    /// <summary>
    /// This class contains input data for AtlasBuilder
    /// </summary>
    /// <typeparam name="T">data source of </typeparam>
    public class AtlasItemSource<T>
    {
        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public float TextureXOffset { get; set; }
        public float TextureYOffset { get; set; }
        public T Source { get; set; }

        public AtlasItemSource(int left, int top, int width, int height)
        {
            Left = left;
            Top = top;
            Width = width;
            Height = height;
        }
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