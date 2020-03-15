//MIT, 2016-present, WinterDev

namespace PixelFarm.CpuBlit.BitmapAtlas
{
    public class SpriteTextureMapData<T>
    {
        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public float TextureXOffset { get; set; }
        public float TextureYOffset { get; set; }
        public T Source { get; set; }

        public SpriteTextureMapData(int left, int top, int width, int height)
        {
            Left = left;
            Top = top;
            Width = width;
            Height = height;
        }        
    }

}