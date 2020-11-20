//MIT, 2016-present, WinterDev
using System;
using System.Collections.Generic;
using Typography.OpenFont;


namespace Typography.TextLayout
{


    /// <summary>
    /// unscaled glyph-plan
    /// </summary>
    public readonly struct UnscaledGlyphPlan
    {
        public readonly ushort input_cp_offset;
        public readonly ushort glyphIndex;
        public UnscaledGlyphPlan(ushort input_cp_offset, ushort glyphIndex, short advanceW, short offsetX, short offsetY)
        {
            this.input_cp_offset = input_cp_offset;
            this.glyphIndex = glyphIndex;
            this.OffsetX = offsetX;
            this.OffsetY = offsetY;
            this.AdvanceX = advanceW;
        }
        public readonly short AdvanceX;
        /// <summary>
        /// x offset from current position
        /// </summary>
        public readonly short OffsetX;
        /// <summary>
        /// y offset from current position
        /// </summary>
        public readonly short OffsetY;

        public bool AdvanceMoveForward => this.AdvanceX > 0;

#if DEBUG
        public override string ToString()
        {
            return " adv:" + AdvanceX;
        }
#endif
    }


    public interface IUnscaledGlyphPlanList
    {
        void Append(UnscaledGlyphPlan glyphPlan);
        int Count { get; }
        UnscaledGlyphPlan this[int index] { get; }

    }

    public class UnscaledGlyphPlanList : IUnscaledGlyphPlanList
    {
        readonly List<UnscaledGlyphPlan> _list = new List<UnscaledGlyphPlan>();
        public int Count => _list.Count;
        public UnscaledGlyphPlan this[int index] => _list[index];
        public void Clear()
        {
            _list.Clear();
        }
        public void Append(UnscaledGlyphPlan glyphPlan)
        {
            _list.Add(glyphPlan);
        }
    }

    /// <summary>
    /// unscaled glyph-plan sequence
    /// </summary>
    public struct GlyphPlanSequence
    {
        //
        public static GlyphPlanSequence Empty = new GlyphPlanSequence();
        //
        readonly IUnscaledGlyphPlanList _glyphPlanList;
        public readonly int startAt;
        public readonly ushort len;

        bool _isRTL;
        public GlyphPlanSequence(IUnscaledGlyphPlanList glyphPlanList)
        {
            _glyphPlanList = glyphPlanList;
            this.startAt = 0;
            this.len = (ushort)glyphPlanList.Count;
            _isRTL = false;
        }
        public GlyphPlanSequence(IUnscaledGlyphPlanList glyphPlanList, int startAt, int len)
        {
            _glyphPlanList = glyphPlanList;
            this.startAt = startAt;
            this.len = (ushort)len;
            _isRTL = false;
        }
        public bool IsRightToLeft
        {
            get => _isRTL;
            set => _isRTL = value;
        }

        public UnscaledGlyphPlan this[int index]
        {
            get
            {
                if (index >= 0 && index < (startAt + len))
                {
                    return _glyphPlanList[startAt + index];
                }
                else
                {
                    throw new IndexOutOfRangeException();
                }
            }
        }
        //
        public int Count => (_glyphPlanList != null) ? len : 0;
        //
        public float CalculateWidth()
        {
            if (_glyphPlanList == null) return 0;
            //
            IUnscaledGlyphPlanList plans = _glyphPlanList;
            int end = startAt + len;
            float width = 0;
            for (int i = startAt; i < end; ++i)
            {
                width += plans[i].AdvanceX;
            }
            return width;
        }
        public bool IsEmpty() => _glyphPlanList == null;

    }

    public enum PositionTechnique
    {
        None,
        /// <summary>
        /// use kerning table (old)
        /// </summary>
        Kerning, //old technique-- TODO: review and remove this 
        /// <summary>
        /// use openfont gpos table
        /// </summary>
        OpenFont,
    }



    class GlyphLayoutPlanCollection
    {
        readonly Dictionary<int, GlyphLayoutPlanContext> _collection = new Dictionary<int, GlyphLayoutPlanContext>();
        /// <summary>
        /// get glyph layout plan or create if not exists
        /// </summary>
        /// <param name="typeface"></param>
        /// <param name="scriptLang"></param>
        /// <returns></returns>
        public GlyphLayoutPlanContext GetPlanOrCreate(Typeface typeface, ScriptLang scriptLang)
        {

            int hash_code = CalculateHash(typeface, scriptLang);
            if (!_collection.TryGetValue(hash_code, out GlyphLayoutPlanContext context))
            {
                var g_sub = (typeface.GSUBTable != null) ? new GlyphSubstitution(typeface, scriptLang.scriptTag, scriptLang.sysLangTag) : null;
                var g_pos = (typeface.GPOSTable != null) ? new GlyphSetPosition(typeface, scriptLang.scriptTag, scriptLang.sysLangTag) : null;

#if DEBUG
                if (g_sub != null)
                {
                    g_sub.dbugScriptLang = scriptLang.ToString();
                }
                if (g_pos != null)
                {
                    g_pos.dbugScriptLang = scriptLang.ToString();
                }
#endif

                _collection.Add(hash_code, context = new GlyphLayoutPlanContext(g_sub, g_pos));
            }
            return context;
        }
        static int CalculateHash(Typeface t, ScriptLang scriptLang)
        {
            int hash = 17;
            hash = hash * 31 + t.GetHashCode();
            hash = hash * 31 + scriptLang.scriptTag.GetHashCode();
            hash = hash * 31 + scriptLang.sysLangTag.GetHashCode();
            return hash;
        }

    }

    readonly struct GlyphLayoutPlanContext
    {
        public readonly GlyphSubstitution _glyphSub;
        public readonly GlyphSetPosition _glyphPos;
        public GlyphLayoutPlanContext(GlyphSubstitution glyphSub, GlyphSetPosition glyphPos)
        {
            _glyphSub = glyphSub;
            _glyphPos = glyphPos;
        }
    }

#if DEBUG
    readonly struct dbugCodePointFromUserChar
    {
        /// <summary>
        /// input codepoint
        /// </summary>
        public readonly int codePoint;
        /// <summary>
        /// offset from the start of input codepoint buffer
        /// </summary>
        public readonly ushort user_char_offset;
        public dbugCodePointFromUserChar(ushort user_char_offset, int codePoint)
        {
            this.user_char_offset = user_char_offset;
            this.codePoint = codePoint;
        }
    }

#endif

    //TODO: rename this to ShapingEngine ?

    /// <summary>
    /// text span's glyph layout engine, 
    /// </summary>
    public class GlyphLayout
    {

        readonly GlyphLayoutPlanCollection _layoutPlanCollection = new GlyphLayoutPlanCollection();
        Typeface _typeface;
        ScriptLang _scriptLang;
        GlyphSubstitution _gsub;
        GlyphSetPosition _gpos;
        bool _needPlanUpdate;

        readonly GlyphIndexList _inputGlyphs = new GlyphIndexList();//reusable input glyph
        readonly GlyphPosStream _glyphPositions = new GlyphPosStream();

        readonly static ScriptLang s_latin = new ScriptLang("latn");
        readonly static ScriptLang s_math = new ScriptLang("math");

        public GlyphLayout()
        {
            PositionTechnique = PositionTechnique.OpenFont;
            EnableLigature = true;
            EnableComposition = true;
            EnableGsub = EnableGpos = true;
            ScriptLang = s_latin;
        }


        //unscaled version
        internal IGlyphPositions ResultUnscaledGlyphPositions => _glyphPositions;
        //
        public PositionTechnique PositionTechnique { get; set; }
        public ScriptLang ScriptLang
        {
            get => _scriptLang;
            set
            {

                if (_scriptLang.scriptTag != value.scriptTag || _scriptLang.sysLangTag != value.sysLangTag)
                {
                    _needPlanUpdate = true;
                }
                _scriptLang = value;
            }
        }

        public bool EnableLigature { get; set; }
        public bool EnableComposition { get; set; }


        //for some built-in mathlayout
        public bool EnableBuiltinMathItalicCorrection { get; set; } = true;


        public Typeface Typeface
        {
            get => _typeface;
            set
            {
                if (_typeface != value)
                {
                    _typeface = value;
                    _needPlanUpdate = true;
                }
            }
        }


        public delegate ushort GlyphNotFoundHandler(GlyphLayout glyphLayout, int codepoint, int nextcodepoint);
        GlyphNotFoundHandler _glyphNotFoundHandler;



        //not thread-safe*** 
        List<int> _reusableUserCodePoints = new List<int>();
#if DEBUG
        List<dbugCodePointFromUserChar> _dbugReusableCodePointFromUserCharList = new List<dbugCodePointFromUserChar>();
#endif

        public bool EnableGsub { get; set; }
        public bool EnableGpos { get; set; }

        /// <summary>
        /// do glyph shaping and glyph out, output is unscaled glyph-plan
        /// </summary>
        /// <param name="str"></param>
        /// <param name="startAt"></param>
        /// <param name="len"></param>
        public void Layout(
            char[] str,
            int startAt,
            int len)
        {

            //[A]
            //convert from char[] to codepoint-list
            // this is important!
            // -----------------------
            // from @samhocevar's PR: (https://github.com/LayoutFarm/Typography/pull/56/commits/b71c7cf863531ebf5caa478354d3249bde40b96e)
            // In many places, "char" is not a valid type to handle characters, because it
            // only supports 16 bits.In order to handle the full range of Unicode characters,
            // we need to use "int".
            // This allows characters such as 🙌 or 𐐷 or to be treated as single codepoints even
            // though they are encoded as two "char"s in a C# string.

            _reusableUserCodePoints.Clear();
#if DEBUG
            _dbugReusableCodePointFromUserCharList.Clear();
            if (str.Length > 2)
            {

            }

#endif
            for (int i = 0; i < len; ++i)
            {
                char ch = str[startAt + i];
                int codepoint = ch;
                if (ch >= 0xd800 && ch <= 0xdbff && i + 1 < len)//high surrogate
                {
                    char nextCh = str[startAt + i + 1];
                    if (nextCh >= 0xdc00 && nextCh <= 0xdfff) //low-surrogate 
                    {
                        //please note: 
                        //num of codepoint may be less than  original user input char 
                        ++i;
                        codepoint = char.ConvertToUtf32(ch, nextCh);
                    }
                }
                _reusableUserCodePoints.Add(codepoint);
#if DEBUG
                _dbugReusableCodePointFromUserCharList.Add(new dbugCodePointFromUserChar((ushort)i, codepoint));
#endif
            }

            Layout(_reusableUserCodePoints);
        }

        public void SetGlyphIndexNotFoundHandler(GlyphNotFoundHandler glyphNotFoundHandler)
        {
            _glyphNotFoundHandler = glyphNotFoundHandler;
        }
        public void Layout(IList<int> inputCodePoints)
        {
            Layout(inputCodePoints, 0, inputCodePoints.Count);
        }
        public void Layout(IList<int> inputCodePoints, int startAt, int len)
        {
            //
            //[B]
            // convert codepoint-list to input glyph-list 
            // clear before use
            _inputGlyphs.Clear();

            int end = startAt + len;
            int cur_codepoint, next_codepoint;

            for (int i = 0; i < end; ++i)
            {
                //find glyph index by specific codepoint  
                if (i + 1 < end)
                {
                    cur_codepoint = inputCodePoints[i];
                    next_codepoint = inputCodePoints[i + 1];
                }
                else
                {
                    cur_codepoint = inputCodePoints[i];
                    next_codepoint = 0;
                }

                ushort glyphIndex = _typeface.GetGlyphIndex(cur_codepoint, next_codepoint, out bool skipNextCodepoint);

                if (glyphIndex == 0 && _glyphNotFoundHandler != null)
                {
                    //handle glyph not found
                    glyphIndex = _glyphNotFoundHandler(this, cur_codepoint, next_codepoint);
                }

                _inputGlyphs.AddGlyph(i, glyphIndex);
                if (skipNextCodepoint)
                {
                    // Maybe this is a UVS sequence; in that case,
                    //***SKIP*** the second codepoint 
                    ++i;
                }
            }
            //continue below...
            Layout(_inputGlyphs);
        }


        void Layout(GlyphIndexList glyphs)
        {
            if (_needPlanUpdate)
            {
                UpdateLayoutPlan();
            }

            //[C]
            //----------------------------------------------  
            //glyph substitution            
            if (EnableGsub && _gsub != null && glyphs.Count > 0)
            {
                //TODO: review perf here
                _gsub.EnableLigation = this.EnableLigature;
                _gsub.EnableComposition = this.EnableComposition;
                _gsub.DoSubstitution(glyphs);
            }

            //----------------------------------------------  
            //after glyph substitution,
            //number of input glyph MAY changed (increase or decrease).***
            //so count again.
            int finalGlyphCount = glyphs.Count;
            //----------------------------------------------  

            //[D]
            //glyph position
            _glyphPositions.Clear();
            _glyphPositions.Typeface = _typeface;
            for (int i = 0; i < finalGlyphCount; ++i)
            {
                //at this stage _inputGlyphs and _glyphPositions 
                //has member 1:1
                glyphs.GetGlyphIndexAndMap(i,
                    out ushort glyphIndex,
                    out ushort input_codepointOffset,
                    out ushort input_mapLen);
                //
                Glyph orgGlyph = _typeface.GetGlyph(glyphIndex);
                //this is original value WITHOUT fit-to-grid adjust
                _glyphPositions.AddGlyph(input_codepointOffset, glyphIndex, orgGlyph);
            }

            PositionTechnique posTech = this.PositionTechnique;
            if (EnableGpos && _gpos != null && glyphs.Count > 0 && posTech == PositionTechnique.OpenFont)
            {
                _gpos.DoGlyphPosition(_glyphPositions);
            }


            //----------------------------------------------  
            //[E] 
            //some math correction
            if (_scriptLang.scriptTag == s_math.scriptTag) //***
            {
                if (EnableBuiltinMathItalicCorrection)
                {
                    int pos_count = _glyphPositions.Count;

                    //from https://docs.microsoft.com/en-us/typography/opentype/spec/math

                    //Italics correction can be used in the following situations:
                    //...
                    //...
                    //When a run of slanted characters is followed by a straight character (such as an operator or a delimiter), the italics correction of the last glyph is added to its advance width.
                    //...
                    //...

                    //@prepare: note, by observation.
                    //in math font (eg. like latin modern) glyph some glyph look upright (regular glyph)
                    //but it has italic correction value
                    //but value is very small when compare to actual italic glyph
                    //so in this case we assume it is not italic

                    //so which is that cut-point value
                    //I use assumption that if the correction value is too small 
                    //after scale less than 1 px it should not be significant,

                    //but inside GlyphLayout, we use unscale version,
                    //so assume if font is 8pts, if correction give value less than 0.33px (subpixel width)=> NOT sig.                     

                    //none_sig_correction > scale_to_px * original_correction
                    //0.33f > scale_to_px(8pt) * original_correction
                    //(0.33f/ scale_to_px(8pt)) > original_correction

                    float none_sig_correction = 0.33f / _typeface.CalculateScaleToPixelFromPointSize(8);//assume at 8 pt size font

                    short prevGlyph_italic_correction = 0;

                    for (int i = 0; i < pos_count; ++i)
                    {
                        Glyph glyph = _typeface.GetGlyph(_glyphPositions[i].glyphIndex);

                        if (glyph?.MathGlyphInfo?.ItalicCorrection is OpenFont.MathGlyphs.MathValueRecord value &&
                            value.Value > none_sig_correction)
                        {
                            //sig correction 
                            prevGlyph_italic_correction = value.Value;
                        }
                        else
                        {
                            //no correct (or from nonsignificant correct above)
                            if (prevGlyph_italic_correction != 0)
                            {
                                _glyphPositions.AppendGlyphAdvance(i - 1, prevGlyph_italic_correction, 0);
                            }

                            prevGlyph_italic_correction = 0;
                        }
                    }
                }
                //other correction...

            }

            //----------------------------------------------  
            //at this point, all positions are layouted at its original scale ***
            //then we will scale it to target scale later 
            //----------------------------------------------   
        }

        /// <summary>
        /// generate map from user codepoint buffer to output glyph index, from latest layout result
        /// </summary>
        /// <param name="outputUserCharToGlyphIndexMapList"></param>
        public void CreateMapFromUserCharToGlyphIndices(List<UserCodePointToGlyphIndex> outputUserCharToGlyphIndexMapList)
        {
            //1. get map from user-input-codepoint to glyph-index 
            _inputGlyphs.CreateMapFromUserCodePointToGlyphIndices(outputUserCharToGlyphIndexMapList);

            ////TODO:
            ////2. 
            ////since some user-input-codepoints may be skiped in codepoint-to-glyph index lookup (see this.Layout(), [A])    
            //int j = outputUserCharToGlyphIndexMapList.Count;
            //for (int i = 0; i < j; ++i)
            //{ 
            //    UserCodePointToGlyphIndex userCodePointToGlyphIndex = outputUserCharToGlyphIndexMapList[i];
            //    CodePointFromUserChar codePointFromUserChar = _reusableCodePointFromUserCharList[userCodePointToGlyphIndex.userCodePointIndex]; 
            //}
        }
        void UpdateLayoutPlan()
        {
            GlyphLayoutPlanContext context = _layoutPlanCollection.GetPlanOrCreate(_typeface, _scriptLang);
            _gpos = context._glyphPos;
            _gsub = context._glyphSub;
            _needPlanUpdate = false;
        }

        /// <summary>
        /// fetch layout result, unscaled version, put to IUnscaledGlyphPlanList
        /// </summary>
        /// <param name="glyphPositions"></param>
        /// <param name="pxscale"></param>
        /// <param name="outputGlyphPlanList"></param>
        public void GenerateUnscaledGlyphPlans(IUnscaledGlyphPlanList outputGlyphPlanList)
        {

            IGlyphPositions glyphPositions = _glyphPositions;
            int finalGlyphCount = glyphPositions.Count;
            for (int i = 0; i < finalGlyphCount; ++i)
            {

                ushort glyphIndex = glyphPositions.GetGlyph(i,
                    out ushort input_offset,
                    out short offsetX,
                    out short offsetY,
                    out short advW);
                //
                outputGlyphPlanList.Append(new UnscaledGlyphPlan(
                    input_offset,
                    glyphIndex,
                    advW,
                    offsetX,
                    offsetY
                    ));
            }
        }
        public IEnumerable<UnscaledGlyphPlan> GetUnscaledGlyphPlanIter()
        {
            //this for iterator version
            IGlyphPositions glyphPositions = _glyphPositions;
            int finalGlyphCount = glyphPositions.Count;
            for (int i = 0; i < finalGlyphCount; ++i)
            {
                ushort glyphIndex = glyphPositions.GetGlyph(i,
                    out ushort input_offset,
                    out short offsetX,
                    out short offsetY,
                    out short advW);

                yield return new UnscaledGlyphPlan(
                    input_offset,
                    glyphIndex,
                    advW,
                    offsetX,
                    offsetY
                    );
            }
        }
    }



    /// <summary>
    /// glyph position stream
    /// </summary>
    class GlyphPosStream : IGlyphPositions
    {
        List<GlyphPos> _glyphPosList = new List<GlyphPos>();

        Typeface _typeface;
        public GlyphPosStream() { }

        public int Count => _glyphPosList.Count;
        //
        public void Clear()
        {
            _typeface = null;
            _glyphPosList.Clear();
        }
        public Typeface Typeface
        {
            get => _typeface;
            set => _typeface = value;
        }
        public void AddGlyph(ushort o_offset, ushort glyphIndex, Glyph glyph)
        {
            if (!Glyph.HasOriginalAdvancedWidth(glyph))
            {
                //TODO: review here, 
                //WHY? some glyph dose not have original advanced width
                Glyph.SetOriginalAdvancedWidth(glyph, _typeface.GetAdvanceWidthFromGlyphIndex(glyphIndex));
            }

            _glyphPosList.Add(new GlyphPos(o_offset, glyphIndex, glyph.GlyphClass, glyph.OriginalAdvanceWidth));
        }
        //
        public GlyphPos this[int index] => _glyphPosList[index];
        //
        public GlyphClassKind GetGlyphClassKind(int index)
        {
            return _glyphPosList[index].classKind;
        }
        /// <summary>
        /// get glyph-index (+ other info) at specific indexed-position, 
        /// </summary>
        /// <param name="index">glyph index</param>
        /// <param name="advW">advanced width</param>
        /// <returns></returns>
        public ushort GetGlyph(int index, out ushort advW)
        {
            GlyphPos pos = _glyphPosList[index];
            advW = (ushort)pos.advanceW;
            return pos.glyphIndex;
        }
        /// <summary>
        /// get glyph-index (+ other info) at specific indexed-position, 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="inputOffset"></param>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        /// <param name="advW"></param>
        /// <returns></returns>
        public ushort GetGlyph(int index, out ushort inputOffset, out short offsetX, out short offsetY, out short advW)
        {
            GlyphPos pos = _glyphPosList[index];
            offsetX = pos.xoffset;
            offsetY = pos.yoffset;
            advW = pos.advanceW;
            inputOffset = pos.o_offset;
            return pos.glyphIndex;
        }
        /// <summary>
        /// get glyph offset at specific indexed-position, 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        public void GetOffset(int index, out short offsetX, out short offsetY)
        {
            GlyphPos pos = _glyphPosList[index];
            offsetX = pos.xoffset;
            offsetY = pos.yoffset;
        }
        //
        public void AppendGlyphOffset(int index, short appendOffsetX, short appendOffsetY)
        {
            GlyphPos existing = _glyphPosList[index];
            existing.xoffset += appendOffsetX;
            existing.yoffset += appendOffsetY;
            _glyphPosList[index] = existing;
        }
        public void AppendGlyphAdvance(int index, short appendAdvX, short appendAdvY)
        {
            GlyphPos pos = _glyphPosList[index];
            pos.advanceW += appendAdvX;//TODO: review for appendY
            _glyphPosList[index] = pos;
        }
    }

    struct GlyphPos
    {
        public readonly ushort o_offset; //original user offset
        public readonly ushort glyphIndex;
        public short xoffset;
        public short yoffset;
        public short advanceW; // actually this value is ushort, TODO: review here
        public readonly GlyphClassKind glyphClass;

        public GlyphPos(ushort o_offset,
            ushort glyphIndex,
            GlyphClassKind glyphClass,
            ushort orgAdvanced
            )
        {
            this.o_offset = o_offset;
            this.glyphClass = glyphClass;
            this.glyphIndex = glyphIndex;
            this.advanceW = (short)orgAdvanced;
            xoffset = yoffset = 0;
        }
        public GlyphClassKind classKind => glyphClass;

        public short OffsetX => xoffset;
        public short OffsetY => yoffset;
#if DEBUG
        public override string ToString()
        {
            return glyphIndex.ToString() + "(" + xoffset + "," + yoffset + ")";
        }
#endif
    }
}
