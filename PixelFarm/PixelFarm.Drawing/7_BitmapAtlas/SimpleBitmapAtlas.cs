//MIT, 2016-present, WinterDev

using System;
using System.Collections.Generic;


namespace PixelFarm.CpuBlit.BitmapAtlas
{

    public class SimpleBitmapAtlas : IDisposable
    {
        readonly Dictionary<ushort, AtlasItem> _atlasItems = new Dictionary<ushort, AtlasItem>();

#if DEBUG
        static int s_totalDebugId;
        public readonly int dbugId = s_totalDebugId++;
#endif
        public SimpleBitmapAtlas()
        {

        }

        public TextureKind TextureKind { get; set; }

        public readonly struct UnicodeRange
        {
            public readonly int startCodepoint;
            public readonly int endCodepoint;
            public UnicodeRange(int startCodepoint, int endCodepoint)
            {
                this.startCodepoint = startCodepoint;
                this.endCodepoint = endCodepoint;
            }
#if DEBUG
            public override string ToString()
            {
                return startCodepoint + "-" + endCodepoint;
            }
#endif
        }

        public int Width { get; set; }
        public int Height { get; set; }

        public string FontName { get; set; }
        public float SizeInPts { get; set; }
        
        public List<uint> ScriptTags { get; set; } = new List<uint>();
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
        public bool TryGetItem(ushort uniqueUint16Name, out AtlasItem atlasItem) => _atlasItems.TryGetValue(uniqueUint16Name, out atlasItem);

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
                cloneDic.Add(kp.Key, new AtlasItem(orgMapData.UniqueUint16Name)
                {
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