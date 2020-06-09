//MIT, 2014-present, WinterDev    
using System;
using System.Collections.Generic;
using Typography.OpenFont;
using Typography.FontManagement;
using System.IO;

namespace Typography.TextServices
{
    using Typography.TextBreak;
    using Typography.TextLayout;


    public class TextServices
    {
        //user can do text shaping by their own
        //this class is optional
        //it provide cache for previous 'used/ wellknown' Word-glyphPlans for a specific font 
        // 

        GlyphPlanCacheForTypefaceAndScriptLang _currentGlyphPlanSeqCache;
        Dictionary<TextShapingContextKey, GlyphPlanCacheForTypefaceAndScriptLang> _registerShapingContexts = new Dictionary<TextShapingContextKey, GlyphPlanCacheForTypefaceAndScriptLang>();

        readonly GlyphLayout _glyphLayout;


        float _fontSizeInPts;
        ScriptLang _defaultScriptLang;
        ActiveTypefaceCache _typefaceStore;
        InstalledTypefaceCollection _installedTypefaceCollection;
        ScriptLang _scLang;

        public TextServices()
        {

            _typefaceStore = ActiveTypefaceCache.GetTypefaceStoreOrCreateNewIfNotExist();
            _glyphLayout = new GlyphLayout();
        }
        public void SetDefaultScriptLang(ScriptLang scLang)
        {
            _scLang = _defaultScriptLang = scLang;
        }
        public InstalledTypefaceCollection InstalledFontCollection
        {
            get => _installedTypefaceCollection;
            set => _installedTypefaceCollection = value;
        }

        public ScriptLang CurrentScriptLang
        {
            get => _scLang;
            set => _scLang = _glyphLayout.ScriptLang = value;
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

            _glyphLayout.Typeface = typeface;
            _fontSizeInPts = fontSizeInPts;

            //_glyphLayout.FontSizeInPoints = _fontSizeInPts = fontSizeInPts;
        }
        public Typeface GetTypeface(string name, TypefaceStyle installedFontStyle)
        {
            InstalledTypeface inst = _installedTypefaceCollection.GetInstalledTypeface(name, InstalledTypefaceCollection.GetSubFam(installedFontStyle));
            if (inst != null)
            {

                return _typefaceStore.GetTypeface(inst);
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


        CustomBreaker _textBreaker;
        public void BreakToLineSegments(char[] str, int startAt, int len, WordVisitor visitor)
        {
            //user must setup the CustomBreakerBuilder before use      
            if (len < 1)
            {
                return;
            }

#if DEBUG
            if (len > 2)
            {

            }
#endif
            if (_textBreaker == null)
            {
                //setup 
                _textBreaker = Typography.TextBreak.CustomBreakerBuilder.NewCustomBreaker();
            }

            _textBreaker.UseUnicodeRangeBreaker = true;
            _textBreaker.CurrentVisitor = visitor;
            _textBreaker.BreakWords(str, startAt, len);
        }

        public void MeasureString(char[] str, int startAt, int len, out int w, out int h)
        {
            //measure string 
            //check if we use cache feature or not
            MeasuredStringBox measureStringBox = _glyphLayout.LayoutAndMeasureString(str, startAt, len, _fontSizeInPts);
            w = (int)measureStringBox.width;
            h = (int)Math.Ceiling(measureStringBox.ClipHeightInPx);
        }
        public void MeasureString(char[] str, int startAt, int len, int limitWidth, out int charFit, out int charFitWidth)
        {
            MeasuredStringBox measureStringBox = _glyphLayout.LayoutAndMeasureString(str, startAt, len, _fontSizeInPts, limitWidth);
            int w = (int)measureStringBox.width;
            int h = (int)Math.Ceiling(measureStringBox.ClipHeightInPx);
            charFit = measureStringBox.StopAt;
            charFitWidth = (int)Math.Ceiling(measureStringBox.width);
        }



        struct TextShapingContextKey
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

#if DEBUG
        public bool dbug_disableCache = false;
#endif

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
#if DEBUG
            if (seqLen > _glyphPlanSeqSet.MaxCacheLen)
            {
                //layout string is too long to be cache
                //it need to split into small buffer 
            }
#endif
            GlyphPlanSequence planSeq = GlyphPlanSequence.Empty;
            GlyphPlanSeqCollection seqCol = _glyphPlanSeqSet.GetSeqCollectionOrCreateIfNotExist(seqLen);

            if (

#if DEBUG
                dbug_disableCache ||
#endif

                !seqCol.TryGetCacheGlyphPlanSeq(seqHashValue, out planSeq))
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
                planSeq = new GlyphPlanSequence(_reusableGlyphPlanList, pre_count, post_count - pre_count);//**                //

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
            if (!_loadedTypefaces.TryGetValue(installedFont, out Typeface typeface))
            {
                //TODO: review how to load font here 
                if (Typography.FontManagement.InstalledTypefaceCollectionExtensions.CustomFontStreamLoader != null)
                {
                    using (var fontStream = Typography.FontManagement.InstalledTypefaceCollectionExtensions.CustomFontStreamLoader(installedFont.FontPath))
                    {
                        var reader = new OpenFontReader();

                        typeface = reader.Read(fontStream, installedFont.ActualStreamOffset);
                        typeface.Filename = installedFont.FontPath;

                    }

                }
                else
                {

                    using (var fs = new FileStream(installedFont.FontPath, FileMode.Open, FileAccess.Read))
                    {
                        var reader = new OpenFontReader();
                        typeface = reader.Read(fs, installedFont.ActualStreamOffset);
                        typeface.Filename = installedFont.FontPath;
                    }

                }
                return _loadedTypefaces[installedFont] = typeface;

            }
            return typeface;
        }

    }

}