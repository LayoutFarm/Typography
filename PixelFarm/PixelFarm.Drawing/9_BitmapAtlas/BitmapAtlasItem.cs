//MIT, 2016-present, WinterDev
using System;

using PixelFarm.Drawing;
namespace PixelFarm.CpuBlit.BitmapAtlas
{
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

        public void SetImageBuffer(int[] imgBuffer, bool isBigEndian = false)
        {
            Source = imgBuffer;
            IsBigEndian = isBigEndian;
        }


    }

    class RelocationAtlasItem
    {
        internal readonly BitmapAtlasItemSource atlasItem;
        public Rectangle area;
        public RelocationAtlasItem(BitmapAtlasItemSource atlasItem)
        {
            this.atlasItem = atlasItem;
        }
#if DEBUG
        public override string ToString()
        {
            return atlasItem.UniqueInt16Name.ToString();
        }
#endif
    }



}