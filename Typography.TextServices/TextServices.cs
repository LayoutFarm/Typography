//MIT, 2014-present, WinterDev    
using System;
using System.Collections.Generic;
using Typography.OpenFont;
using Typography.TextLayout;
using Typography.FontManagement;
using System.IO;

namespace Typography.TextLayout
{
    public struct BreakSpan
    {
        public int startAt;
        public ushort len;
        public short flags;
        public ScriptLang scLang;
    }
}

namespace Typography.TextServices
{

    public class TextServices
    {
        //user can do text shaping by their own
        //this class is optional
        //it provide cache for previous 'used/ wellknown' Word-glyphPlans for a specific font 
        // 

        GlyphPlanCacheForTypefaceAndScriptLang _currentGlyphPlanSeqCache;
        Dictionary<TextShapingContextKey, GlyphPlanCacheForTypefaceAndScriptLang> _registerShapingContexts = new Dictionary<TextShapingContextKey, GlyphPlanCacheForTypefaceAndScriptLang>();
        GlyphLayout _glyphLayout;

        Typeface _currentTypeface;
        float _fontSizeInPts;
        ScriptLang _defaultScriptLang;
        ActiveTypefaceCache typefaceStore;
        InstalledTypefaceCollection _installedTypefaceCollection;
        ScriptLang scLang;


        public TextServices()
        {

            typefaceStore = ActiveTypefaceCache.GetTypefaceStoreOrCreateNewIfNotExist(); 
            _glyphLayout = new GlyphLayout();
        }
        public void SetDefaultScriptLang(ScriptLang scLang)
        {
            this.scLang = _defaultScriptLang = scLang;
        }
        public InstalledTypefaceCollection InstalledFontCollection
        {
            get
            {
                return _installedTypefaceCollection;
            }
            set
            {
                _installedTypefaceCollection = value;
            }
        }

        public ScriptLang CurrentScriptLang
        {
            get { return scLang; }
            set { this.scLang = _glyphLayout.ScriptLang = value; }
        }

        public void SetCurrentFont(Typeface typeface, float fontSizeInPts)
        {
            //check if we have the cache-key or create a new one.
            var key = new TextShapingContextKey(typeface, _glyphLayout.ScriptLang);
            if (!_registerShapingContexts.TryGetValue(key, out _currentGlyphPlanSeqCache))
            {
                //not found
                //the create the new one 
                var shapingContext = new GlyphPlanCacheForTypefaceAndScriptLang(typeface, _glyphLayout.ScriptLang);
                //shaping context setup ...
                _registerShapingContexts.Add(key, shapingContext);
                _currentGlyphPlanSeqCache = shapingContext;
            }

            _currentTypeface = _glyphLayout.Typeface = typeface;
            _fontSizeInPts = fontSizeInPts;

            //_glyphLayout.FontSizeInPoints = _fontSizeInPts = fontSizeInPts;
        }
        public Typeface GetTypeface(string name, TypefaceStyle installedFontStyle)
        {
            InstalledTypeface inst = _installedTypefaceCollection.GetInstalledTypeface(name, InstalledTypefaceCollection.GetSubFam(installedFontStyle));
            if (inst != null)
            {
                return typefaceStore.GetTypeface(inst);
            }
            return null;

        }

        public GlyphPlanSequence GetUnscaledGlyphPlanSequence(TextBuffer buffer, int start, int len)
        {
            //under current typeface + scriptlang setting 
            return _currentGlyphPlanSeqCache.GetUnscaledGlyphPlanSequence(_glyphLayout, buffer, start, len);
        }
        internal void ClearAllRegisteredShapingContext()
        {
            _registerShapingContexts.Clear();
        }

        Typography.TextBreak.CustomBreaker _textBreaker;
        public IEnumerable<BreakSpan> BreakToLineSegments(char[] str, int startAt, int len)
        {
            //user must setup the CustomBreakerBuilder before use              
            if (_textBreaker == null)
            {
                _textBreaker = Typography.TextBreak.CustomBreakerBuilder.NewCustomBreaker();
            }
            int cur_startAt = startAt;
            _textBreaker.BreakWords(str, cur_startAt, len);
            foreach (TextBreak.BreakSpan sp in _textBreaker.GetBreakSpanIter())
            {
                //our service select a proper script lang info and add to the breakspan

                //at this point
                //we assume that 1 break span 
                //has 1 script lang, and we examine it
                //with sample char
                char sample = str[sp.startAt];

                ScriptLang selectedScriptLang;
                if (sample == ' ')
                {
                    //whitespace
                    selectedScriptLang = _defaultScriptLang;
                }
                else if (char.IsWhiteSpace(sample))
                {
                    //other whitespace
                    selectedScriptLang = _defaultScriptLang;
                }
                else
                {
                    //
                    Typography.OpenFont.ScriptLang scLang;
                    if (Typography.OpenFont.ScriptLangs.TryGetScriptLang(sample, out scLang))
                    {
                        //we should decide to use
                        //current typeface
                        //or ask for alternate typeface 
                        //if  the current type face is not support the request scriptLang
                        // 
                        selectedScriptLang = scLang;
                    }
                    else
                    {
                        //not found
                        //use default
                        selectedScriptLang = _defaultScriptLang;
                    }
                }

                BreakSpan breakspan = new BreakSpan();
                breakspan.startAt = sp.startAt;
                breakspan.len = sp.len;
                breakspan.scLang = selectedScriptLang;
                yield return breakspan;
            }
        }

        /// <summary>
        /// expandable list of glyph plan
        /// </summary>
        class UnscaledGlyphPlanList : IUnscaledGlyphPlanList
        {
            List<UnscaledGlyphPlan> _glyphPlans = new List<UnscaledGlyphPlan>();


            public void Clear()
            {
                _glyphPlans.Clear();

            }
            public void Append(UnscaledGlyphPlan glyphPlan)
            {
                _glyphPlans.Add(glyphPlan);

            }


            public UnscaledGlyphPlan this[int index]
            {
                get
                {
                    return _glyphPlans[index];
                }
            }
            public int Count
            {
                get
                {
                    return _glyphPlans.Count;
                }
            }

#if DEBUG
            public UnscaledGlyphPlanList()
            {

            }
#endif
        }



        List<MeasuredStringBox> _reusableMeasureBoxList = new List<MeasuredStringBox>();

        UnscaledGlyphPlanList _reusableGlyphPlanList = new UnscaledGlyphPlanList();

        public void MeasureString(char[] str, int startAt, int len, out int w, out int h)
        {
            //measure string 
            //check if we use cache feature or not

            if (str.Length < 1)
            {
                w = h = 0;
            }
            _reusableMeasureBoxList.Clear(); //reset 

            float pxscale = _currentTypeface.CalculateScaleToPixelFromPointSize(_fontSizeInPts);
            //NOET:at this moment, simple operation
            //may not be simple...  
            //-------------------
            //input string may contain more than 1 script lang
            //user can parse it by other parser
            //but in this code, we use our Typography' parser
            //-------------------
            //user must setup the CustomBreakerBuilder before use         

            int cur_startAt = startAt;
            float accumW = 0;
            float accumH = 0;


            foreach (BreakSpan breakSpan in BreakToLineSegments(str, startAt, len))
            {

                _glyphLayout.Layout(str, breakSpan.startAt, breakSpan.len);
                //
                _reusableGlyphPlanList.Clear();
                _glyphLayout.GenerateUnscaledGlyphPlans(_reusableGlyphPlanList);
                //create pixelscale...



                ////measure string size
                //var result = new MeasuredStringBox(
                //    _reusableGlyphPlanList.AccumAdvanceX,
                //    _currentTypeface.Ascender * pxscale,
                //    _currentTypeface.Descender * pxscale,
                //    _currentTypeface.LineGap * pxscale,
                //     Typography.OpenFont.Extensions.TypefaceExtensions.CalculateRecommendLineSpacing(_currentTypeface) * pxscale);
                //
                // ConcatMeasureBox(ref accumW, ref accumH, ref result);

            }

            w = (int)System.Math.Round(accumW);
            h = (int)System.Math.Round(accumH);
        }

        public void MeasureString(char[] str, int startAt, int len, int limitWidth, out int charFit, out int charFitWidth)
        {
            //measure string 
            if (str.Length < 1)
            {
                charFitWidth = 0;
            }

            _reusableMeasureBoxList.Clear(); //reset 

            float pxscale = _currentTypeface.CalculateScaleToPixelFromPointSize(_fontSizeInPts);
            //NOET:at this moment, simple operation
            //may not be simple...  
            //-------------------
            //input string may contain more than 1 script lang
            //user can parse it by other parser
            //but in this code, we use our Typography' parser
            //-------------------
            //user must setup the CustomBreakerBuilder before use         

            int cur_startAt = startAt;
            float accumW = 0;

            foreach (BreakSpan breakSpan in BreakToLineSegments(str, startAt, len))
            {

                //measure string at specific px scale 
                _glyphLayout.Layout(str, breakSpan.startAt, breakSpan.len);
                //

                _reusableGlyphPlanList.Clear();
                _glyphLayout.GenerateUnscaledGlyphPlans(_reusableGlyphPlanList);
                //measure ...


                //measure each glyph
                //limit at specific width
                int glyphCount = _reusableGlyphPlanList.Count;


                float acc_x = 0;//accum_x
                float acc_y = 0;//accum_y
                float g_x = 0;
                float g_y = 0;
                float x = 0;
                float y = 0;
                for (int i = 0; i < glyphCount; ++i)
                {
                    UnscaledGlyphPlan glyphPlan = _reusableGlyphPlanList[i];

                    float ngx = acc_x + (float)Math.Round(glyphPlan.OffsetX * pxscale);
                    float ngy = acc_y + (float)Math.Round(glyphPlan.OffsetY * pxscale);
                    //NOTE:
                    // -glyphData.TextureXOffset => restore to original pos
                    // -glyphData.TextureYOffset => restore to original pos 
                    //--------------------------
                    g_x = (float)(x + (ngx)); //ideal x
                    g_y = (float)(y + (ngy));
                    float g_w = (float)Math.Round(glyphPlan.AdvanceX * pxscale);
                    acc_x += g_w;
                    //g_x = (float)Math.Round(g_x);
                    g_y = (float)Math.Floor(g_y);

                    float right = g_x + g_w;

                    if (right >= accumW)
                    {
                        //stop here at this glyph
                        charFit = i - 1;
                        //TODO: review this
                        charFitWidth = (int)System.Math.Round(accumW);
                        return;
                    }
                    else
                    {
                        accumW = right;
                    }
                }
            }

            charFit = 0;
            charFitWidth = 0;
        }


        static void ConcatMeasureBox(ref float accumW, ref float accumH, ref MeasuredStringBox measureBox)
        {
            accumW += measureBox.width;
            float h = measureBox.CalculateLineHeight();
            if (h > accumH)
            {
                accumH = h;
            }
        }


        struct TextShapingContextKey
        {

            readonly Typeface _typeface;
            readonly ScriptLang _scLang;

            public TextShapingContextKey(Typeface typeface, ScriptLang scLang)
            {
                this._typeface = typeface;
                this._scLang = scLang;
            }
#if DEBUG
            public override string ToString()
            {
                return _typeface + " " + _scLang;
            }
#endif
        }



    }


    class GlyphPlanSeqSet
    {
        //TODO: consider this value, make this a variable (static int)
        const int PREDEFINE_LEN = 10;

        /// <summary>
        /// common len 0-10?
        /// </summary>
        GlyphPlanSeqCollection[] _cacheSeqCollection1;
        //other len
        Dictionary<int, GlyphPlanSeqCollection> _cacheSeqCollection2; //lazy init
        public GlyphPlanSeqSet()
        {
            _cacheSeqCollection1 = new GlyphPlanSeqCollection[PREDEFINE_LEN];

            this.MaxCacheLen = 20;//stop caching, please managed this ...
                                  //TODO:
                                  //what is the proper number of cache word ?
                                  //init free dic
            for (int i = PREDEFINE_LEN - 1; i >= 0; --i)
            {
                _cacheSeqCollection1[i] = new GlyphPlanSeqCollection(i);
            }
        }
        public int MaxCacheLen
        {
            get;
            private set;
        }

        public GlyphPlanSeqCollection GetSeqCollectionOrCreateIfNotExist(int len)
        {
            if (len < PREDEFINE_LEN)
            {
                return _cacheSeqCollection1[len];
            }
            else
            {
                if (_cacheSeqCollection2 == null)
                {
                    _cacheSeqCollection2 = new Dictionary<int, GlyphPlanSeqCollection>();
                }
                GlyphPlanSeqCollection seqCol;
                if (!_cacheSeqCollection2.TryGetValue(len, out seqCol))
                {
                    //new one if not exist
                    seqCol = new GlyphPlanSeqCollection(len);
                    _cacheSeqCollection2.Add(len, seqCol);
                }
                return seqCol;
            }
        }
    }




    /// <summary>
    /// glyph-cache based on typeface and script-lang with specific gsub/gpos features
    /// </summary>
    class GlyphPlanCacheForTypefaceAndScriptLang
    {


        Typeface _typeface;
        ScriptLang _scLang;
        GlyphPlanSeqSet _glyphPlanSeqSet;
        UnscaledGlyphPlanList _reusableGlyphPlanList = new UnscaledGlyphPlanList();

        public GlyphPlanCacheForTypefaceAndScriptLang(Typeface typeface, ScriptLang scLang)
        {
            _typeface = typeface;
            _scLang = scLang;
            _glyphPlanSeqSet = new GlyphPlanSeqSet();
        }
        static int CalculateHash(TextBuffer buffer, int startAt, int len)
        {
            //reference,
            //https://stackoverflow.com/questions/2351087/what-is-the-best-32bit-hash-function-for-short-strings-tag-names
            return CRC32.CalculateCRC32(buffer.UnsafeGetInternalBuffer(), startAt, len);
        }
        public GlyphPlanSequence GetUnscaledGlyphPlanSequence(
            GlyphLayout glyphLayout,
            TextBuffer buffer, int start, int seqLen)
        {

            //UNSCALED VERSION
            //use current typeface + scriptlang
            int seqHashValue = CalculateHash(buffer, start, seqLen);

            //this func get the raw char from buffer
            //and create glyph list 
            //check if we have the string cache in specific value 
            //---------
            if (seqLen > _glyphPlanSeqSet.MaxCacheLen)
            {
                //layout string is too long to be cache
                //it need to split into small buffer 
            }

            GlyphPlanSequence planSeq = GlyphPlanSequence.Empty;
            GlyphPlanSeqCollection seqCol = _glyphPlanSeqSet.GetSeqCollectionOrCreateIfNotExist(seqLen);

            if (!seqCol.TryGetCacheGlyphPlanSeq(seqHashValue, out planSeq))
            {
                //create a new one if we don't has a cache
                //1. layout 
                glyphLayout.Layout(
                    buffer.UnsafeGetInternalBuffer(),
                    start,
                    seqLen);

                int pre_count = _reusableGlyphPlanList.Count;
                //create glyph-plan ( UnScaled version) and add it to planList                

                glyphLayout.GenerateUnscaledGlyphPlans(_reusableGlyphPlanList);

                int post_count = _reusableGlyphPlanList.Count;
                planSeq = new GlyphPlanSequence(_reusableGlyphPlanList, pre_count, post_count - pre_count);
                //
                seqCol.Register(seqHashValue, planSeq);
            }
            return planSeq;
        }


    }

    class GlyphPlanSeqCollection
    {
        int _seqLen;
        /// <summary>
        /// dic of hash string value and the cache seq
        /// </summary>
        Dictionary<int, GlyphPlanSequence> _knownSeqs = new Dictionary<int, GlyphPlanSequence>();

        public GlyphPlanSeqCollection(int seqLen)
        {
            this._seqLen = seqLen;
        }
        public int SeqLen
        {
            get { return _seqLen; }
        }
        public void Register(int hashValue, GlyphPlanSequence seq)
        {
            _knownSeqs.Add(hashValue, seq);
        }
        public bool TryGetCacheGlyphPlanSeq(int hashValue, out GlyphPlanSequence seq)
        {
            return _knownSeqs.TryGetValue(hashValue, out seq);
        }
    }


    class ActiveTypefaceCache
    {

        Dictionary<InstalledTypeface, Typeface> _loadedTypefaces = new Dictionary<InstalledTypeface, Typeface>();
        public ActiveTypefaceCache()
        {

        }
        static ActiveTypefaceCache s_typefaceStore;
        public static ActiveTypefaceCache GetTypefaceStoreOrCreateNewIfNotExist()
        {
            if (s_typefaceStore == null)
            {
                s_typefaceStore = new ActiveTypefaceCache();
            }
            return s_typefaceStore;
        }
        public Typeface GetTypeface(InstalledTypeface installedFont)
        {
            return GetTypefaceOrCreateNew(installedFont);
        }
        Typeface GetTypefaceOrCreateNew(InstalledTypeface installedFont)
        {
            //load 
            //check if we have create this typeface or not 
            Typeface typeface;
            if (!_loadedTypefaces.TryGetValue(installedFont, out typeface))
            {
                //TODO: review how to load font here
                using (var fs = new FileStream(installedFont.FontPath, FileMode.Open, FileAccess.Read))
                {
                    var reader = new OpenFontReader();
                    typeface = reader.Read(fs);
                    typeface.Filename = installedFont.FontPath;
                }
                return _loadedTypefaces[installedFont] = typeface;
            }
            return typeface;
        }

    }

}