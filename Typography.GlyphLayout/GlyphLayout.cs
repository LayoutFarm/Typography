//MIT, 2016-2017, WinterDev
using System;
using System.Collections.Generic;
using Typography.OpenFont;
namespace Typography.TextLayout
{
    public interface IPixelScaleLayout
    {
        void SetFont(Typeface typeface, float fontSizeInPoints);
        void Layout(IGlyphPositions posStream, GlyphPlanList outputGlyphPlanList);
    }

    public struct GlyphPlan
    {

        public readonly ushort glyphIndex;
        public GlyphPlan(ushort glyphIndex, float exactX, float exactY, float exactAdvX)
        {
            this.glyphIndex = glyphIndex;
            this.ExactX = exactX;
            this.ExactY = exactY;
            this.AdvanceX = exactAdvX;
        }
        public float AdvanceX { get; set; }
        public float ExactY { get; set; }
        public float ExactX { get; set; }

        public float ExactRight { get { return ExactX + AdvanceX; } }
        public bool AdvanceMoveForward { get { return this.AdvanceX > 0; } }

#if DEBUG
        public override string ToString()
        {
            return "(" + ExactX + "," + ExactY + "), adv:" + AdvanceX;
        }
#endif
    }

    public class GlyphPlanList
    {
        List<GlyphPlan> _glyphPlans = new List<GlyphPlan>();
        float _accumAdvanceX;

        public void Clear()
        {
            _glyphPlans.Clear();
            _accumAdvanceX = 0;
        }
        public void Append(GlyphPlan glyphPlan)
        {
            _glyphPlans.Add(glyphPlan);
            _accumAdvanceX += glyphPlan.AdvanceX;
        }
        public float AccumAdvanceX { get { return _accumAdvanceX; } }

        public GlyphPlan this[int index]
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
        Dictionary<GlyphLayoutPlanKey, GlyphLayoutPlanContext> collection = new Dictionary<GlyphLayoutPlanKey, GlyphLayoutPlanContext>();
        /// <summary>
        /// get glyph layout plan or create if not exists
        /// </summary>
        /// <param name="typeface"></param>
        /// <param name="scriptLang"></param>
        /// <returns></returns>
        public GlyphLayoutPlanContext GetPlanOrCreate(Typeface typeface, ScriptLang scriptLang)
        {
            GlyphLayoutPlanKey key = new GlyphLayoutPlanKey(typeface, scriptLang.internalName);
            GlyphLayoutPlanContext context;
            if (!collection.TryGetValue(key, out context))
            {
                var glyphSubstitution = (typeface.GSUBTable != null) ? new GlyphSubstitution(typeface, scriptLang.shortname) : null;
                var glyphPosition = (typeface.GPOSTable != null) ? new GlyphSetPosition(typeface, scriptLang.shortname) : null;
                collection.Add(key, context = new GlyphLayoutPlanContext(glyphSubstitution, glyphPosition));
            }
            return context;
        }

    }
    struct GlyphLayoutPlanKey
    {
        public Typeface t;
        public int scriptInternameName;
        public GlyphLayoutPlanKey(Typeface t, int scriptInternameName)
        {
            this.t = t;
            this.scriptInternameName = scriptInternameName;
        }
    }
    struct GlyphLayoutPlanContext
    {
        public readonly GlyphSubstitution _glyphSub;
        public readonly GlyphSetPosition _glyphPos;
        public GlyphLayoutPlanContext(GlyphSubstitution _glyphSub, GlyphSetPosition glyphPos)
        {
            this._glyphSub = _glyphSub;
            this._glyphPos = glyphPos;
        }
    }





    //TODO: rename this to ShapingEngine ?

    /// <summary>
    /// text span's glyph layout engine, 
    /// </summary>
    public class GlyphLayout
    {

        GlyphLayoutPlanCollection _layoutPlanCollection = new GlyphLayoutPlanCollection();
        Typeface _typeface;
        ScriptLang _scriptLang;
        GlyphSubstitution _gsub;
        GlyphSetPosition _gpos;
        bool _needPlanUpdate;

        internal GlyphIndexList _inputGlyphs = new GlyphIndexList();
        internal GlyphPosStream _glyphPositions = new GlyphPosStream();


        public GlyphLayout()
        {
            PositionTechnique = PositionTechnique.OpenFont;
            EnableLigature = true;
            EnableComposition = true;
            ScriptLang = ScriptLangs.Latin;
        }
        public IGlyphPositions ResultUnscaledGlyphPositions
        {
            get { return _glyphPositions; }
        }
        public PositionTechnique PositionTechnique { get; set; }
        public ScriptLang ScriptLang
        {
            get { return _scriptLang; }
            set
            {
                if (_scriptLang != value)
                {
                    _needPlanUpdate = true;
                }
                _scriptLang = value;
            }
        }

        public bool EnableLigature { get; set; }
        public bool EnableComposition { get; set; }
        public Typeface Typeface
        {
            get { return _typeface; }
            set
            {
                if (_typeface != value)
                {
                    _typeface = value;
                    _needPlanUpdate = true;
                }
            }
        }
        /// <summary>
        /// reusable codepoint list buffer
        /// </summary>
        List<int> _codepoints = new List<int>();//not thread-safe*** 

        /// <summary>
        /// do glyph shaping and glyph out
        /// </summary>
        /// <param name="str"></param>
        /// <param name="startAt"></param>
        /// <param name="len"></param>
        public void Layout(
            char[] str,
            int startAt,
            int len)
        {
            if (_needPlanUpdate)
            {
                UpdateLayoutPlan();
            }

            // this is important!
            // -----------------------
            //  from @samhocevar's PR: (https://github.com/LayoutFarm/Typography/pull/56/commits/b71c7cf863531ebf5caa478354d3249bde40b96e)
            // In many places, "char" is not a valid type to handle characters, because it
            // only supports 16 bits.In order to handle the full range of Unicode characters,
            // we need to use "int".
            // This allows characters such as 🙌 or 𐐷 or to be treated as single codepoints even
            // though they are encoded as two "char"s in a C# string.
            _codepoints.Clear();
            for (int i = 0; i < len; ++i)
            {
                char ch = str[startAt + i];
                int codepoint = ch;
                if (Char.IsHighSurrogate(ch) && i + 1 < len)
                {
                    char nextCh = str[startAt + i + 1];
                    if (Char.IsLowSurrogate(nextCh))
                    {
                        ++i;
                        codepoint = char.ConvertToUtf32(ch, nextCh);
                    }
                }
                _codepoints.Add(codepoint);
            }

            // clear before use
            _inputGlyphs.Clear();

            // convert codepoints to input glyphs
            for (int i = 0; i < _codepoints.Count; ++i)
            {
                int codepoint = _codepoints[i];
                ushort glyphIndex = _typeface.LookupIndex(codepoint);
                if (i + 1 < _codepoints.Count)
                {
                    // Maybe this is a UVS sequence; in that case, skip the second codepoint
                    int nextCodepoint = _codepoints[i + 1];
                    ushort variationGlyphIndex = _typeface.LookupIndex(codepoint, nextCodepoint);
                    if (variationGlyphIndex > 0)
                    {
                        ++i;
                        glyphIndex = variationGlyphIndex;
                    }
                }
                _inputGlyphs.AddGlyph(codepoint, glyphIndex);
            }

            //----------------------------------------------  
            //glyph substitution            
            if (_gsub != null & len > 0)
            {
                //TODO: review perf here
                _gsub.EnableLigation = this.EnableLigature;
                _gsub.EnableComposition = this.EnableComposition;
                _gsub.DoSubstitution(_inputGlyphs);
                //
                _inputGlyphs.CreateMapFromUserCharToGlyphIndices();
            }
            //----------------------------------------------  
            //after glyph substitution,
            //number of input glyph MAY changed (increase or decrease).***
            //so count again.
            int finalGlyphCount = _inputGlyphs.Count;
            //----------------------------------------------  
            //glyph position
            _glyphPositions.Clear();
            _glyphPositions.Typeface = _typeface;
            for (int i = 0; i < finalGlyphCount; ++i)
            {
                //at this stage _inputGlyphs and _glyphPositions 
                //has member 1:1
                ushort glyIndex = _inputGlyphs[i];
                //
                Glyph orgGlyph = _typeface.GetGlyphByIndex(glyIndex);
                //this is original value WITHOUT fit-to-grid adjust
                _glyphPositions.AddGlyph(glyIndex, orgGlyph);
            }

            PositionTechnique posTech = this.PositionTechnique;
            if (_gpos != null && len > 1 && posTech == PositionTechnique.OpenFont)
            {
                _gpos.DoGlyphPosition(_glyphPositions);
            }
            //----------------------------------------------  
            //at this point, all position is layout at original scale ***
            //then we will scale it to target scale later 
            //----------------------------------------------  

        }

        void UpdateLayoutPlan()
        {
            GlyphLayoutPlanContext context = _layoutPlanCollection.GetPlanOrCreate(this._typeface, this._scriptLang);
            this._gpos = context._glyphPos;
            this._gsub = context._glyphSub;
            _needPlanUpdate = false;
        }
    }




    public static class GlyphLayoutExtensions
    {

#if DEBUG
        public static float dbugSnapToFitInteger(float value)
        {
            int floor_value = (int)value;
            return (value - floor_value >= (1f / 2f)) ? floor_value + 1 : floor_value;
        }
        public static float dbugSnapHalf(float value)
        {
            int floor_value = (int)value;
            //round to int 0, 0.5,1.0
            return (value - floor_value >= (2f / 3f)) ? floor_value + 1 : //else->
                   (value - floor_value >= (1f / 3f)) ? floor_value + 0.5f : floor_value;
        }
        static int dbugSnapUpper(float value)
        {
            int floor_value = (int)value;
            return floor_value + 1;
        }
        /// <summary>
        /// read latest layout output into outputGlyphPlanList
        /// </summary>
        /// <param name="glyphLayout"></param>
        /// <param name="outputGlyphPlanList"></param>
        public static void dbugReadOutput(this GlyphLayout glyphLayout, List<UserCharToGlyphIndexMap> outputGlyphPlanList)
        {
            //TODO: review here 
            outputGlyphPlanList.AddRange(glyphLayout._inputGlyphs._mapUserCharToGlyphIndices);
        }
#endif 


        /// <summary>
        /// general scale, generate glyph plan, from unscale glyph size to specific scale
        /// </summary>
        /// <param name="glyphPositions"></param>
        /// <param name="pxscale"></param>
        /// <param name="outputGlyphPlanList"></param>
        public static void GenerateGlyphPlan(IGlyphPositions glyphPositions,
            float pxscale,
            bool snapToGrid,
            GlyphPlanList outputGlyphPlanList)
        {
            //user can implement this with some 'PixelScaleEngine'

            //double cx = 0;
            //short cy = 0;
            //the default OpenFont layout without fit-to-writing alignment
            int finalGlyphCount = glyphPositions.Count;
            double cx = 0;
            short cy = 0;

            for (int i = 0; i < finalGlyphCount; ++i)
            {
                short offsetX, offsetY, advW; //all from pen-pos
                ushort glyphIndex = glyphPositions.GetGlyph(i, out offsetX, out offsetY, out advW);

                float s_advW = advW * pxscale;

                if (snapToGrid)
                {
                    //TEST, 
                    //if you want to snap each glyph to grid (1px or 0.5px) by ROUNDING
                    //we can do it here,this produces a predictable caret position result
                    //
                    s_advW = (int)Math.Round(s_advW);
                }
                float exact_x = (float)(cx + offsetX * pxscale);
                float exact_y = (float)(cy + offsetY * pxscale);

                outputGlyphPlanList.Append(new GlyphPlan(
                   glyphIndex,
                    exact_x,
                    exact_y,
                    advW * pxscale));
                cx += s_advW;

            }
        }
       
    }

    /// <summary>
    /// glyph position stream
    /// </summary>
    class GlyphPosStream : IGlyphPositions
    {
        List<GlyphPos> _glyphs = new List<GlyphPos>();

        Typeface _typeface;
        public GlyphPosStream() { }

        public int Count
        {
            get
            {
                return _glyphs.Count;
            }
        }
        public void Clear()
        {
            _typeface = null;
            _glyphs.Clear();
        }
        public Typeface Typeface
        {
            get { return this._typeface; }
            set { this._typeface = value; }
        }
        public void AddGlyph(ushort glyphIndex, Glyph glyph)
        {
            if (!glyph.HasOriginalAdvancedWidth)
            {
                glyph.OriginalAdvanceWidth = _typeface.GetHAdvanceWidthFromGlyphIndex(glyphIndex);
            }
            _glyphs.Add(new GlyphPos(glyphIndex, glyph.GlyphClass, glyph.OriginalAdvanceWidth));

        }
        public void AppendGlyphOffset(int index, short appendOffsetX, short appendOffsetY)
        {
            GlyphPos existing = _glyphs[index];
            existing.xoffset += appendOffsetX;
            existing.yoffset += appendOffsetY;
            _glyphs[index] = existing;
        }
        public GlyphPos this[int index]
        {

            get
            {
                return _glyphs[index];
            }
        }
        public GlyphClassKind GetGlyphClassKind(int index)
        {
            return _glyphs[index].classKind;
        }
        public ushort GetGlyph(int index, out ushort advW)
        {
            GlyphPos pos = _glyphs[index];
            advW = (ushort)pos.advanceW;
            return pos.glyphIndex;
        }
        public ushort GetGlyph(int index, out short offsetX, out short offsetY, out short advW)
        {
            GlyphPos pos = _glyphs[index];
            offsetX = pos.xoffset;
            offsetY = pos.yoffset;
            advW = pos.advanceW;
            return pos.glyphIndex;
        }
        public void GetOffset(int index, out short offsetX, out short offsetY)
        {
            GlyphPos pos = _glyphs[index];
            offsetX = pos.xoffset;
            offsetY = pos.yoffset;
        }

        public void AppendGlyphAdvance(int index, short appendAdvX, short appendAdvY)
        {
            GlyphPos pos = _glyphs[index];
            pos.advanceW += appendAdvX;//TODO: review for appendY
            _glyphs[index] = pos;
        }
        //public void FlushNewGlyphAdvance()
        //{
        //    int lim = _glyphs.Count - 1;
        //    short total_advW = 0;
        //    InternalGlyphPos p_next;
        //    for (int i = 0; i < lim; ++i)
        //    {
        //        //----------------------------------------
        //        //update advance i => affect the pos of i+1
        //        //----------------------------------------
        //        short advW_update = _updateAdvanceWList[i];
        //        p_next = _glyphs[i + 1];
        //        //TODO: review offset Y for vertical writing direction
        //        total_advW += advW_update;
        //        p_next.xoffset += total_advW;
        //        _glyphs[i + 1] = p_next;//set back
        //        _updateAdvanceWList[i] = 0;//clear
        //    }
        //    //and the last one
        //    p_next = _glyphs[lim];
        //    p_next.xoffset += total_advW;
        //    _glyphs[lim] = p_next;//set back
        //}
    }

    struct GlyphPos
    {

        public readonly GlyphClassKind glyphClass;
        public readonly ushort glyphIndex;
        public short xoffset;
        public short yoffset;
        public short advanceW; // actually this value is ushort, TODO: review here

        public GlyphPos(
            ushort glyphIndex,
            GlyphClassKind glyphClass,
            ushort orgAdvanced
            )
        {
            this.glyphClass = glyphClass;
            this.glyphIndex = glyphIndex;
            this.advanceW = (short)orgAdvanced;
            xoffset = yoffset = 0;
        }
        public GlyphClassKind classKind
        {
            get { return glyphClass; }
        }

        public short OffsetX { get { return xoffset; } }
        public short OffsetY { get { return yoffset; } }
#if DEBUG
        public override string ToString()
        {
            return glyphIndex.ToString() + "(" + xoffset + "," + yoffset + ")";
        }
#endif
    }
}
