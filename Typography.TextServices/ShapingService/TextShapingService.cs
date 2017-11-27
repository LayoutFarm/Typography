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
        Dictionary<InstalledFont, TextShapingContext> _registerShapingContexts = new Dictionary<InstalledFont, TextShapingContext>();
        GlyphLayout _glyphLayout;
        internal TextShapingService(TextServiceHub hub)
        {
            this._hub = hub;
            //create glyph layout instance with default setting
            _glyphLayout = new GlyphLayout();
        }
        public void SetCurrentFont(string fontname, InstalledFontStyle fontStyle)
        {
            InstalledFont installedFont = _hub._openFontStore.GetFont(fontname, fontStyle);
            if (installedFont == null) return;//not found request font
            if (!_registerShapingContexts.TryGetValue(installedFont, out _currentShapingContext))
            {
                //not found
                //the create the new one
                Typeface typeface;
                using (var fs = new System.IO.FileStream(installedFont.FontPath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    var reader = new OpenFontReader();
                    typeface = reader.Read(fs);
                }
                TextShapingContext shapingContext = new TextShapingContext(typeface);
                //shaping context setup ...
                _registerShapingContexts.Add(installedFont, shapingContext);
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
    }


    class TextShapingContext
    {
        GlyphPlanBuffer _glyphPlanBuffer;
        Typeface _typeface; 
        /// <summary>
        /// common len 0-10?
        /// </summary>
        GlyphPlanSeqCollection[] _cacheSeqCollection1;
        //other len
        Dictionary<int, GlyphPlanSeqCollection> _cacheSeqCollection2; //lazy init
        CRC32 _myCRC32;

        //TODO: consider this value, make this a variable (static int)
        const int PREDEFINE_LEN = 10;
        public TextShapingContext(Typeface typeface)
        {
            _typeface = typeface;
            _glyphPlanBuffer = new GlyphPlanBuffer(new List<GlyphPlan>());
            _myCRC32 = new CRC32();

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
            return _myCRC32.CalculateCRC32(TextBuffer.UnsafeGetCharBuffer(buffer), startAt, len);
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