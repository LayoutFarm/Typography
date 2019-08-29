//MIT, 2019-present, WinterDev
//----------------------------------- 

using System;
using System.Collections.Generic;

namespace PixelFarm.Drawing
{
    public class AtlasImageBinder : LayoutFarm.ImageBinder
    {
        public AtlasImageBinder(string atlasName, string imgName)
        {
            AtlasName = atlasName;
            ImageName = imgName;
        }
        public string AtlasName { get; private set; }
        public string ImageName { get; private set; }
        public override bool IsAtlasImage => true;
        public PixelFarm.Drawing.BitmapAtlas.BitmapMapData MapData { get; set; }
    }
}

namespace PixelFarm.Drawing.BitmapAtlas
{
    public enum TextureKind : byte
    {
        StencilLcdEffect, //default
        StencilGreyScale,
        Msdf,
        Bitmap
    }


    public class SimpleBitmaptAtlas
    {
        AtlasItemImage _totalImg;
        Dictionary<ushort, BitmapMapData> _locations = new Dictionary<ushort, BitmapMapData>();


        public int Width { get; set; }
        public int Height { get; set; }
        /// <summary>
        /// original font size in point unit
        /// </summary>

        public TextureKind TextureKind { get; set; }
        public string BitmapFilename { get; set; }
        public Dictionary<string, ushort> ImgUrlDict { get; set; }

        public void AddBitmapMapData(ushort imgIndex, BitmapMapData bmpMapData)
        {
            _locations.Add(imgIndex, bmpMapData);
        }
        public AtlasItemImage TotalImg
        {
            get => _totalImg;
            set => _totalImg = value;
        }
        public bool TryGetBitmapMapData(ushort imgIndex, out BitmapMapData bmpMapData)
        {
            if (!_locations.TryGetValue(imgIndex, out bmpMapData))
            {
                bmpMapData = null;
                return false;
            }
            return true;
        }
        public bool TryGetBitmapMapData(string imgUrl, out BitmapMapData bmpMapData)
        {
            if (ImgUrlDict != null &&
                ImgUrlDict.TryGetValue(imgUrl, out ushort foundIndex))
            {
                return TryGetBitmapMapData(foundIndex, out bmpMapData);
            }
            //
            bmpMapData = null;
            return false;
        }
    }


}