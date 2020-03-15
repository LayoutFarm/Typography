//MIT, 2016-present, WinterDev

namespace PixelFarm.CpuBlit.BitmapAtlas
{
    public class AtlasItem
    {
        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public float TextureXOffset { get; set; }
        public float TextureYOffset { get; set; }


        public AtlasItem(ushort uniqueUint16Name)
        {
            UniqueUint16Name = uniqueUint16Name;
        }

        public ushort UniqueUint16Name { get; private set; }
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