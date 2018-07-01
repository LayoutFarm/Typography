//MIT, 2016-present, WinterDev
using System.Collections.Generic;
using Typography.OpenFont;

namespace Typography.Contours
{

    //see also: PixelFarm's  class HintedVxsGlyphCollection 

    //TODO: review this class name again 

    public class GlyphMeshCollection<T>
    {
        //hint glyph collection        
        //per typeface
        Dictionary<ushort, T> _currentGlyphDic = null;
        Dictionary<GlyphKey, Dictionary<ushort, T>> _registerGlyphCollection = new Dictionary<GlyphKey, Dictionary<ushort, T>>();

        public void SetCacheInfo(Typeface typeface, float sizeInPts, HintTechnique hintTech)
        {
            //TODO: review key object again, if we need to store a typeface object ?
            //check if we have create the context for this request parameters?
            var key = new GlyphKey() { hintTech = hintTech, sizeInPts = sizeInPts, typeface = typeface };
            if (!_registerGlyphCollection.TryGetValue(key, out _currentGlyphDic))
            {
                //if not found 
                //create new
                _currentGlyphDic = new Dictionary<ushort, T>();
                _registerGlyphCollection.Add(key, _currentGlyphDic);
            }
        }
        public bool TryGetCacheGlyph(ushort glyphIndex, out T vxs)
        {
            return _currentGlyphDic.TryGetValue(glyphIndex, out vxs);
        }
        public void RegisterCachedGlyph(ushort glyphIndex, T vxs)
        {
            _currentGlyphDic[glyphIndex] = vxs;
        }
        public void ClearAll()
        {
            _currentGlyphDic = null;
            _registerGlyphCollection.Clear();
        }

        List<GlyphKey> tempKeys = new List<GlyphKey>();

        public void Clear(Typeface typeface)
        {
            //clear all registered typeface glyph
            tempKeys.Clear();
            foreach (var k in _registerGlyphCollection.Keys)
            {
                //collect ...
                if (k.typeface == typeface)
                {
                    tempKeys.Add(k);
                }
            }
            //
            for (int i = tempKeys.Count - 1; i >= 0; --i)
            {
                _registerGlyphCollection.Remove(tempKeys[i]);
            }
            tempKeys.Clear();
        }
        struct GlyphKey
        {
            public HintTechnique hintTech;
            public Typeface typeface;
            public float sizeInPts;
        }
    }

}