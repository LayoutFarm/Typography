//MIT, 2014-2017, WinterDev    
using System.Collections.Generic;
using Typography.OpenFont;
using Typography.TextLayout;
namespace Typography.TextServices
{
    public class TextShapingService
    {
        //user can do text shaping by their own
        //this class is optional
        //it provide cache for previous 'used/ wellknown' Word-glyphPlans for a specific font
        // 
        TextServiceHub _hub;
        TextShapingContext _currentShapingContext;
        Dictionary<TextShapingContextKey, TextShapingContext> _registerShapingContexts = new Dictionary<TextShapingContextKey, TextShapingContext>();
        GlyphLayout _glyphLayout;

        float _fontSizeInPts;

        internal TextShapingService(TextServiceHub hub)
        {
            this._hub = hub;
            //create glyph layout instance with default setting
            _glyphLayout = new GlyphLayout();
        }

        public ScriptLang CurrentScriptLang
        {
            get { return _glyphLayout.ScriptLang; }
        }

        public void SetCurrentFont(string fontname, InstalledFontStyle fontStyle, float fontSizeInPts, ScriptLang scLang = null)
        {
            InstalledFont installedFont = _hub._openFontStore.GetFont(fontname, fontStyle);
            if (installedFont == null) return;//not found request font

            if (scLang != null)
            {
                _glyphLayout.ScriptLang = scLang;
            }

            var key = new TextShapingContextKey(installedFont, _glyphLayout.ScriptLang);
            if (!_registerShapingContexts.TryGetValue(key, out _currentShapingContext))
            {
                //not found
                //the create the new one
                Typeface typeface;
                using (var fs = new System.IO.FileStream(installedFont.FontPath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    var reader = new OpenFontReader();
                    typeface = reader.Read(fs);
                }
                var shapingContext = new TextShapingContext(typeface, _glyphLayout.ScriptLang);
                //shaping context setup ...
                _registerShapingContexts.Add(key, shapingContext);
                _currentShapingContext = shapingContext;
            }
            _fontSizeInPts = fontSizeInPts;
        }
        public void SetCurrentScriptLang(ScriptLang scLang)
        {
            _glyphLayout.ScriptLang = scLang;
        }

        /// <summary>
        /// shaping input string with current font and current script         
        /// </summary>
        /// <param name="inputString"></param>
        public GlyphPlanSequence LayoutText(string inputString)
        {
            //output is glyph plan for this input string 
            //input string need to be splited into 'words'. 
            TextBuffer textBuffer = new TextBuffer(inputString.ToCharArray());
            return _currentShapingContext.Layout(_glyphLayout, textBuffer, 0, textBuffer.Len);
        }
        public GlyphPlanSequence LayoutText(TextBuffer buffer, int start, int len)
        {
            return _currentShapingContext.Layout(_glyphLayout, buffer, start, len);
        }


        internal void ClearAllRegisteredShapingContext()
        {
            _registerShapingContexts.Clear();
        }

        struct TextShapingContextKey
        {

            readonly InstalledFont _installedFont;
            readonly ScriptLang _scLang;

            public TextShapingContextKey(InstalledFont installedFont, ScriptLang scLang)
            {
                this._installedFont = installedFont;
                this._scLang = scLang;
            }
#if DEBUG
            public override string ToString()
            {
                return _installedFont + " " + _scLang;
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


    class TextShapingContext
    {
        GlyphPlanBuffer _glyphPlanBuffer;
        Typeface _typeface;
        ScriptLang _scLang;
        GlyphPlanSeqSet _glyphPlanSeqSet;

        public TextShapingContext(Typeface typeface, ScriptLang scLang)
        {
            _typeface = typeface;
            _scLang = scLang;
            _glyphPlanBuffer = new GlyphPlanBuffer(new GlyphPlanList());
            _glyphPlanSeqSet = new GlyphPlanSeqSet();

        }
        GlyphPlanSequence CreateGlyphPlanSeq(GlyphLayout glyphLayout, TextBuffer buffer, int startAt, int len)
        {
            GlyphPlanList planList = GlyphPlanBuffer.UnsafeGetGlyphPlanList(_glyphPlanBuffer);
            int pre_count = planList.Count;
            glyphLayout.Typeface = _typeface;
            glyphLayout.ScriptLang = _scLang;
            glyphLayout.Layout(
                TextBuffer.UnsafeGetCharBuffer(buffer),
                startAt,
                len);


            int post_count = planList.Count;
            return new GlyphPlanSequence(_glyphPlanBuffer, pre_count, post_count - pre_count);
        }
        static int CalculateHash(TextBuffer buffer, int startAt, int len)
        {
            //reference,
            //https://stackoverflow.com/questions/2351087/what-is-the-best-32bit-hash-function-for-short-strings-tag-names
            return CRC32.CalculateCRC32(TextBuffer.UnsafeGetCharBuffer(buffer), startAt, len);
        }

        public GlyphPlanSequence Layout(GlyphLayout glyphLayout, TextBuffer buffer, int startAt, int len)
        {
            //this func get the raw char from buffer
            //and create glyph list 
            //check if we have the string cache in specific value 
            //---------
            if (len > _glyphPlanSeqSet.MaxCacheLen)
            {
                //layout string is too long to be cache
                //it need to split into small buffer 
            }

            GlyphPlanSequence planSeq = GlyphPlanSequence.Empty;

            GlyphPlanSeqCollection seqCol = _glyphPlanSeqSet.GetSeqCollectionOrCreateIfNotExist(len);
            int hashValue = CalculateHash(buffer, startAt, len);
            if (!seqCol.TryGetCacheGlyphPlanSeq(hashValue, out planSeq))
            {
                ////not found then create glyph plan seq
                //bool useOutputScale = glyphLayout.UsePxScaleOnReadOutput;

                ////save 
                //some font may have 'special' glyph x,y at some font size(eg. for subpixel-rendering position)
                //but in general we store the new glyph plan seq with unscale glyph pos
                //glyphLayout.UsePxScaleOnReadOutput = false;
                planSeq = CreateGlyphPlanSeq(glyphLayout, buffer, startAt, len);
                //glyphLayout.UsePxScaleOnReadOutput = useOutputScale;//restore
                seqCol.Register(hashValue, planSeq);
            }
            //---
            //on unscale font=> we use original  
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
}