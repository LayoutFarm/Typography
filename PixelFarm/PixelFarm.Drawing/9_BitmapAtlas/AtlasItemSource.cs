//MIT, 2016-present, WinterDev
using System;

using PixelFarm.Drawing;
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


    /// <summary>
    /// Input for AtlasBuilder
    /// </summary>
    public class BitmapAtlasItemSource : AtlasItemSource<int[]>
    {
        public BitmapAtlasItemSource(int w, int h) : base(0, 0, w, h) { }
        public int[] GetImageBuffer() => Source;
        public bool IsBigEndian { get; set; }

        /// <summary>
        /// name of this item in int16 (eg. glyph index)
        /// </summary>
        public ushort UniqueInt16Name { get; set; }
        /// <summary>
        /// name of this item in string( eg. bitmap unqiue name)
        /// </summary>
        public string UniqueName { get; set; }
        public Rectangle area;
        public void SetImageBuffer(int[] imgBuffer, bool isBigEndian = false)
        {
            Source = imgBuffer;
            IsBigEndian = isBigEndian;
        }
    }
     

}