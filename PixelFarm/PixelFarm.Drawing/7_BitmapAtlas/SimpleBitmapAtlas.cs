//MIT, 2016-present, WinterDev
//----------------------------------- 

using System;
using System.Collections.Generic;


namespace PixelFarm.CpuBlit.BitmapAtlas
{

    public class SimpleBitmapAtlas : IDisposable
    {

        Dictionary<ushort, AtlasItem> _atlasItems = new Dictionary<ushort, AtlasItem>();

#if DEBUG
        static int s_totalDebugId;
        public readonly int dbugId = s_totalDebugId++;
#endif
        public SimpleBitmapAtlas()
        {

        }
        public int Width { get; set; }
        public int Height { get; set; }
        //------------
        /// <summary>
        /// original font size in point unit
        /// </summary>
        public float OriginalFontSizePts { get; set; }
        public TextureKind TextureKind { get; set; }
        public string FontFilename { get; set; }
        public int FontKey { get; set; }
        //------------
        public Dictionary<ushort, AtlasItem> ItemDict => _atlasItems;
        public Dictionary<string, ushort> ImgUrlDict { get; set; }

        internal void AddAtlasItem(AtlasItem item)
        {
            _atlasItems.Add(item.UniqueUint16Name, item);
        }
        public bool UseSharedImage { get; set; }
        public MemBitmap MainBitmap { get; private set; }
        public bool IsMemBitmapOwner { get; private set; }
        public void SetMainBitmap(MemBitmap mainBmp, bool isOwner)
        {
            if (MainBitmap != null && MainBitmap != mainBmp && IsMemBitmapOwner)
            {
                //dispose current main bmp
                MainBitmap.Dispose();
                MainBitmap = null;
                IsMemBitmapOwner = false;
            }

            MainBitmap = mainBmp;
            IsMemBitmapOwner = isOwner;
        }

        public void Dispose()
        {
            _atlasItems.Clear();
            if (MainBitmap != null && IsMemBitmapOwner)
            {
                MainBitmap.Dispose();
                MainBitmap = null;
            }

        }
        /// <summary>
        /// try get atlas item by unique name
        /// </summary>
        /// <param name="uniqueUint16Name"></param>
        /// <param name="atlasItem"></param>
        /// <returns></returns>
        public bool TryGetItem(ushort uniqueUint16Name, out AtlasItem atlasItem)
        {
            if (!_atlasItems.TryGetValue(uniqueUint16Name, out atlasItem))
            {
                atlasItem = null;
                return false;
            }
            return true;
        }
        /// <summary>
        /// try get atlas item by unique name
        /// </summary>
        /// <param name="itemName"></param>
        /// <param name="atlasItem"></param>
        /// <returns></returns>
        public bool TryGetItem(string itemName, out AtlasItem atlasItem)
        {
            //get img item by unique name
            if (ImgUrlDict != null &&
                ImgUrlDict.TryGetValue(itemName, out ushort foundIndex) &&
                _atlasItems.TryGetValue(foundIndex, out atlasItem))
            {
                return true;
            }
            atlasItem = null;
            return false;
        }


        public static Dictionary<ushort, AtlasItem> CloneLocationWithOffset(SimpleBitmapAtlas org, int dx, int dy)
        {
            Dictionary<ushort, AtlasItem> cloneDic = new Dictionary<ushort, AtlasItem>();
            foreach (var kp in org._atlasItems)
            {
                AtlasItem orgMapData = kp.Value;
                cloneDic.Add(kp.Key, new AtlasItem(orgMapData.UniqueUint16Name) {
                    Left = orgMapData.Left + dx,
                    Top = orgMapData.Top + dy,
                    //
                    Width = orgMapData.Width,
                    Height = orgMapData.Height,
                    TextureXOffset = orgMapData.TextureXOffset,
                    TextureYOffset = orgMapData.TextureYOffset

                });
            }
            return cloneDic;
        }
    }

}