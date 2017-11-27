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
        ScriptLang _currentScriptLang;

        internal TextShapingService(TextServiceHub hub)
        {
            this._hub = hub;
            //create glyph layout instance with default setting
            _glyphLayout = new GlyphLayout();
            _currentScriptLang = ScriptLangs.Latin;//default?
        }

        public ScriptLang CurrentScriptLang
        {
            get { return _currentScriptLang; }
            set
            {
                //must not be null              
                _currentScriptLang = value;
            }
        }

        public void SetCurrentFont(string fontname, InstalledFontStyle fontStyle, ScriptLang scLang = null)
        {
            InstalledFont installedFont = _hub._openFontStore.GetFont(fontname, fontStyle);
            if (installedFont == null) return;//not found request font

            if (scLang != null)
            {
                _currentScriptLang = scLang;
            }

            var key = new TextShapingContextKey(installedFont, _currentScriptLang);
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
                var shapingContext = new TextShapingContext(typeface, _currentScriptLang);
                //shaping context setup ...
                _registerShapingContexts.Add(key, shapingContext);
                _currentShapingContext = shapingContext;
            }
        }
        public void SetCurrentScriptLang(ScriptLang scLang)
        {
            _glyphLayout.ScriptLang = scLang;
        }
        /// <summary>
        /// shaping input string with current font and current script         
        /// </summary>
        /// <param name="inputString"></param>
        public GlyphPlanSequence ShapeText(string inputString)
        {
            //output is glyph plan for this input string 
            //input string need to be splited into 'words'. 
            TextBuffer textBuffer = new TextBuffer(inputString.ToCharArray());
            return _currentShapingContext.Layout(_glyphLayout, textBuffer, 0, textBuffer.Len);
        }
        public GlyphPlanSequence ShapeText(TextBuffer buffer, int start, int len)
        {
            return _currentShapingContext.Layout(_glyphLayout, buffer, start, len);
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


    class TextShapingContext
    {
        GlyphPlanBuffer _glyphPlanBuffer;
        Typeface _typeface;
        ScriptLang _scLang;

        /// <summary>
        /// common len 0-10?
        /// </summary>
        GlyphPlanSeqCollection[] _cacheSeqCollection1;
        //other len
        Dictionary<int, GlyphPlanSeqCollection> _cacheSeqCollection2; //lazy init

        //TODO: consider this value, make this a variable (static int)
        const int PREDEFINE_LEN = 10;
        public TextShapingContext(Typeface typeface, ScriptLang scLang)
        {
            _typeface = typeface;
            _scLang = scLang;
            _glyphPlanBuffer = new GlyphPlanBuffer(new List<GlyphPlan>());
            _cacheSeqCollection1 = new GlyphPlanSeqCollection[PREDEFINE_LEN];

            //TODO:
            //what is the proper number of cache word ?
            //init free dic
            for (int i = PREDEFINE_LEN - 1; i >= 0; --i)
            {
                _cacheSeqCollection1[i] = new GlyphPlanSeqCollection(i);
            }
        }
        GlyphPlanSequence CreateGlyphPlanSeq(GlyphLayout glyphLayout, TextBuffer buffer, int startAt, int len)
        {
            List<GlyphPlan> planList = GlyphPlanBuffer.UnsafeGetGlyphPlanList(_glyphPlanBuffer);
            int pre_count = planList.Count;
            glyphLayout.Typeface = _typeface;
            glyphLayout.ScriptLang = _scLang;
            glyphLayout.Layout(
                TextBuffer.UnsafeGetCharBuffer(buffer),
                startAt,
                len);
            glyphLayout.ReadOutput(planList);
            int post_count = planList.Count;
            return new GlyphPlanSequence(_glyphPlanBuffer, pre_count, post_count - pre_count);
        }
        int CalculateHash(TextBuffer buffer, int startAt, int len)
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
            GlyphPlanSequence planSeq = GlyphPlanSequence.Empty;
            //look in the cache
            if (len < PREDEFINE_LEN)
            {
                GlyphPlanSeqCollection seqCol = _cacheSeqCollection1[len];
                //check if we have the cache plan or not
                int hashValue = CalculateHash(buffer, startAt, len);
                if (seqCol.TryGetCacheGlyphPlanSeq(hashValue, out planSeq))
                {
                    return planSeq;
                }
                //not found  then create glyph plan seq
                planSeq = CreateGlyphPlanSeq(glyphLayout, buffer, startAt, len);
                seqCol.Register(hashValue, planSeq);
                return planSeq;
            }
            else
            {
                //please look in the dic
                //if not found the create a new dic
                //and store the value 
                int hashValue = CalculateHash(buffer, startAt, len);
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

                if (seqCol.TryGetCacheGlyphPlanSeq(hashValue, out planSeq))
                {
                    return planSeq;
                }
                planSeq = CreateGlyphPlanSeq(glyphLayout, buffer, startAt, len);
                seqCol.Register(hashValue, planSeq);
                return planSeq;
            }

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