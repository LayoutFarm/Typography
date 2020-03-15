//MIT, 2016-present, WinterDev
//----------------------------------- 

using System;
using System.Collections.Generic;


namespace PixelFarm.CpuBlit.BitmapAtlas
{

    public class SimpleBitmapAtlas
    {

        Dictionary<ushort, TextureGlyphMapData> _glyphLocations = new Dictionary<ushort, TextureGlyphMapData>();

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
        public Dictionary<string, ushort> ImgUrlDict { get; set; }

        public void AddGlyph(ushort glyphIndex, TextureGlyphMapData glyphData)
        {
            _glyphLocations.Add(glyphIndex, glyphData);
        }
        public bool UseSharedGlyphImage { get; set; }
        public MemBitmap MainBitmap { get; set; }
        public bool TryGetGlyphMapData(ushort glyphIndex, out TextureGlyphMapData glyphdata)
        {
            if (!_glyphLocations.TryGetValue(glyphIndex, out glyphdata))
            {
                glyphdata = null;
                return false;
            }
            return true;
        }
        public bool TryGetGlyphMapData(string itemName, out TextureGlyphMapData glyphdata)
        {
            //get img item by unique name
            if (ImgUrlDict != null && 
                ImgUrlDict.TryGetValue(itemName, out ushort foundIndex) &&
                _glyphLocations.TryGetValue(foundIndex, out glyphdata))
            {
                return true;
            }
            glyphdata = null;
            return false;
        }
        public Dictionary<ushort, TextureGlyphMapData> GlyphDic => _glyphLocations;

        public static Dictionary<ushort, TextureGlyphMapData> CloneLocationWithOffset(SimpleBitmapAtlas org, int dx, int dy)
        {
            Dictionary<ushort, TextureGlyphMapData> cloneDic = new Dictionary<ushort, TextureGlyphMapData>();
            foreach (var kp in org._glyphLocations)
            {
                TextureGlyphMapData orgMapData = kp.Value;
                cloneDic.Add(kp.Key,
                    new TextureGlyphMapData()
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