//MIT, 2016-2017, WinterDev
using System; 
using System.Collections.Generic;
//
using PixelFarm.Agg;
using Typography.OpenFont; 
using Typography.Rendering;

namespace PixelFarm.Drawing.Fonts
{

    class HintedVxsGlyphCollection
    {
        //hint glyph collection        
        //per typeface
        Dictionary<ushort, VertexStore> _currentGlyphDic = null;
        Dictionary<HintedVxsConvtextKey, Dictionary<ushort, VertexStore>> _hintedGlyphs = new Dictionary<HintedVxsConvtextKey, Dictionary<ushort, VertexStore>>();
        public void SetCacheInfo(Typeface typeface, float sizeInPts, HintTechnique hintTech)
        {
            //check if we have create the context for this request parameters?
            var key = new HintedVxsConvtextKey() { hintTech = hintTech, sizeInPts = sizeInPts, typeface = typeface };
            if (!_hintedGlyphs.TryGetValue(key, out _currentGlyphDic))
            {
                //if not found 
                //create new
                _currentGlyphDic = new Dictionary<ushort, VertexStore>();
                _hintedGlyphs.Add(key, _currentGlyphDic);
            }
        }
        public bool TryGetCacheGlyph(ushort glyphIndex, out VertexStore vxs)
        {
            return _currentGlyphDic.TryGetValue(glyphIndex, out vxs);
        }
        public void RegisterCachedGlyph(ushort glyphIndex, VertexStore vxs)
        {
            _currentGlyphDic[glyphIndex] = vxs;
        }
        struct HintedVxsConvtextKey
        {
            public HintTechnique hintTech;
            public Typeface typeface;
            public float sizeInPts;
        }
    }

}