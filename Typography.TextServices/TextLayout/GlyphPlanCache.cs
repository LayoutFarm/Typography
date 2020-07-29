//MIT, 2014-present, WinterDev    
using System;
using System.Collections.Generic;
using Typography.OpenFont;

namespace Typography.TextLayout
{

    /// <summary>
    /// glyph-cache based on typeface and script-lang with specific gsub/gpos features
    /// </summary>
    class GlyphPlanCacheForTypefaceAndScriptLang
    {

        readonly MultiLengthGlyphPlanSeqCache _multiLenSeqsCache = new MultiLengthGlyphPlanSeqCache(10);
        readonly UnscaledGlyphPlanList _reusableGlyphPlanList = new UnscaledGlyphPlanList();

        public GlyphPlanCacheForTypefaceAndScriptLang()
        {
        }
        static int CalculateHash(in Typography.Text.TextBufferSpan buffer)
        {
            //reference,
            //TODO: we can use other hash function here 
            //eg ..
            //https://stackoverflow.com/questions/114085/fast-string-hashing-algorithm-with-low-collision-rates-with-32-bit-integer
            //https://stackoverflow.com/questions/2351087/what-is-the-best-32bit-hash-function-for-short-strings-tag-names

            if (buffer.IsUtf32Buffer)
            {
                return CRC32.CalculateCRC32(buffer.GetRawUtf32Buffer(), buffer.start, buffer.len);
            }
            else
            {
                return CRC32.CalculateCRC32(buffer.GetRawUtf16Buffer(), buffer.start, buffer.len);
            }

        }
#if DEBUG
        internal Typeface dbug_typeface;
        internal ScriptLang dbug_scLang;
        public bool dbug_disableCache = false;
#endif

        public GlyphPlanSequence GetUnscaledGlyphPlanSequence(
            GlyphLayout glyphLayout,
            in Typography.Text.TextBufferSpan buffer)
        {
            //UNSCALED VERSION
            //use current typeface + scriptlang

            int seqHashValue = CalculateHash(buffer);

            //this func get the raw char from buffer
            //and create glyph list 
            //check if we have the string cache in specific value 
            //---------

            GlyphPlanSequence planSeq = GlyphPlanSequence.Empty;
            GlyphPlanSeqCollection seqCol = _multiLenSeqsCache.GetGlyphPlanSeqCollection(buffer.len);

            if (

#if DEBUG
                dbug_disableCache ||
#endif

                !seqCol.TryGetCacheGlyphPlanSeq(seqHashValue, out planSeq))
            {
                //create a new one if we don't has a cache
                //1. layout 
                if (buffer.IsUtf32Buffer)
                {
                    glyphLayout.Layout(
                        buffer.GetRawUtf32Buffer(),
                        buffer.start,
                        buffer.len);
                }
                else
                {
                    glyphLayout.Layout(
                        buffer.GetRawUtf16Buffer(),
                        buffer.start,
                        buffer.len);
                }


                int pre_count = _reusableGlyphPlanList.Count;
                //create glyph-plan ( UnScaled version) and add it to planList                

                glyphLayout.GenerateUnscaledGlyphPlans(_reusableGlyphPlanList);

                int post_count = _reusableGlyphPlanList.Count;
                planSeq = new GlyphPlanSequence(_reusableGlyphPlanList, pre_count, post_count - pre_count);//**

#if DEBUG
                if (!dbug_disableCache)
                {
#endif
                    seqCol.Register(seqHashValue, planSeq);
#if DEBUG
                }
#endif
            }
            return planSeq;

        }

        struct MultiLengthGlyphPlanSeqCache
        {
            //per-typeface and script lang 
            /// <summary>
            /// common len 0-10?
            /// </summary>
            GlyphPlanSeqCollection[] _cacheSeqCollection1;
            //other len
            Dictionary<int, GlyphPlanSeqCollection> _cacheSeqCollection2; //lazy init

            readonly int _predefinedLen;

            public MultiLengthGlyphPlanSeqCache(int predefineLen)
            {
                _predefinedLen = predefineLen;
                _cacheSeqCollection1 = new GlyphPlanSeqCollection[predefineLen];
                _cacheSeqCollection2 = null;

                this.MaxCacheLen = 20;//stop caching, please managed this ...
                                      //TODO:
                                      //what is the proper number of cache word ?
                                      //init free dic
            }
            public int MaxCacheLen { get; }

            public GlyphPlanSeqCollection GetGlyphPlanSeqCollection(int len)
            {
                if (len < _predefinedLen)
                {
                    GlyphPlanSeqCollection seq = _cacheSeqCollection1[len];
                    return seq ?? (_cacheSeqCollection1[len] = new GlyphPlanSeqCollection(len));
                }
                else
                {
                    if (_cacheSeqCollection2 == null)
                    {
                        _cacheSeqCollection2 = new Dictionary<int, GlyphPlanSeqCollection>();
                    }
                    if (!_cacheSeqCollection2.TryGetValue(len, out GlyphPlanSeqCollection seqCol))
                    {
                        //new one if not exist
                        seqCol = new GlyphPlanSeqCollection(len);
                        _cacheSeqCollection2.Add(len, seqCol);
                    }
                    return seqCol;
                }
            }
        }
    }

    class GlyphPlanSeqCollection
    {
        readonly int _seqLen;
        /// <summary>
        /// dic of hash string value and the cache seq
        /// </summary>
        readonly Dictionary<int, GlyphPlanSequence> _knownSeqs = new Dictionary<int, GlyphPlanSequence>();

        public GlyphPlanSeqCollection(int seqLen)
        {
            _seqLen = seqLen;
        }
        //
        public int SeqLen => _seqLen;
        //
        public void Register(int hashValue, GlyphPlanSequence seq)
        {
            _knownSeqs.Add(hashValue, seq);
        }
        public bool TryGetCacheGlyphPlanSeq(int hashValue, out GlyphPlanSequence seq)
        {
            return _knownSeqs.TryGetValue(hashValue, out seq);
        }
    }


    public class GlyphPlanCache
    {
        //user can do text-shaping by their own.
        //this class is optional.
        //it provide cache for previous 'used/ wellknown' Word-glyphPlans for a specific font 
        // 

        GlyphPlanCacheForTypefaceAndScriptLang _currentGlyphPlanSeqCache;
        readonly Dictionary<TextShapingContextKey, GlyphPlanCacheForTypefaceAndScriptLang> _registerShapingContexts = new Dictionary<TextShapingContextKey, GlyphPlanCacheForTypefaceAndScriptLang>();
        Typeface _latestTypeface;
        float _latestFontSizeInPts;
        public GlyphPlanCache()
        {

        }
        public void SetCurrentFont(Typeface typeface, float fontSizeInPts, ScriptLang sclang)
        {
            if (_latestTypeface == typeface && fontSizeInPts == _latestFontSizeInPts)
            {
                //no change
                return;
            }

            _latestTypeface = typeface;
            _latestFontSizeInPts = fontSizeInPts;

            //check if we have the cache-key or create a new one.
            var key = new TextShapingContextKey(typeface, sclang);
            if (!_registerShapingContexts.TryGetValue(key, out _currentGlyphPlanSeqCache))
            {
                //not found
                //the create the new one 
                var cache = new GlyphPlanCacheForTypefaceAndScriptLang();
#if DEBUG
                cache.dbug_scLang = sclang;
                cache.dbug_typeface = typeface;
#endif
                //shaping context setup ...
                _registerShapingContexts.Add(key, cache);
                _currentGlyphPlanSeqCache = cache;
            }
        }


        public GlyphPlanSequence GetUnscaledGlyphPlanSequence(in Typography.Text.TextBufferSpan buffer, GlyphLayout glyphLayout)
        {
            //under current typeface + scriptlang setting 
            return _currentGlyphPlanSeqCache.GetUnscaledGlyphPlanSequence(glyphLayout, buffer);
        }
        internal void ClearAllRegisteredShapingContext()
        {
            _registerShapingContexts.Clear();
        }
        readonly struct TextShapingContextKey
        {

            readonly Typeface _typeface;
            readonly ScriptLang _scLang;

            public TextShapingContextKey(Typeface typeface, ScriptLang scLang)
            {
                _typeface = typeface;
                _scLang = scLang;
            }
#if DEBUG
            public override string ToString()
            {
                return _typeface + " " + _scLang;
            }
#endif
        }
    }


}