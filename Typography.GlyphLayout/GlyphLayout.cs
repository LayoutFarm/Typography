//MIT, 2016-2017, WinterDev
using System;
using System.Collections.Generic;
using Typography.OpenFont;
namespace Typography.TextLayout
{
    public interface IPixelScaleLayout
    {
        void SetFont(Typeface typeface, float fontSizeInPoints);
        void Layout(IGlyphPositions posStream, List<GlyphPlan> outputGlyphPlanList);
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
        public float AdvanceX { get; private set; }
        public float ExactY { get; private set; }
        public float ExactX { get; private set; }

        public float ExactRight { get { return ExactX + AdvanceX; } }
        public bool AdvanceMoveForward { get { return this.AdvanceX > 0; } }

#if DEBUG
        public override string ToString()
        {
            return "(" + ExactX + "," + ExactY + "), adv:" + AdvanceX;
        }
#endif
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
                var glyphSubstitution = (typeface.GSUBTable != null) ? new GlyphSubStitution(typeface, scriptLang.shortname) : null;
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
        public readonly GlyphSubStitution _glyphSub;
        public readonly GlyphSetPosition _glyphPos;
        public GlyphLayoutPlanContext(GlyphSubStitution _glyphSub, GlyphSetPosition glyphPos)
        {
            this._glyphSub = _glyphSub;
            this._glyphPos = glyphPos;
        }
    }

    /// <summary>
    /// text span's glyph layout engine
    /// </summary>
    public class GlyphLayout
    {

        GlyphLayoutPlanCollection _layoutPlanCollection = new GlyphLayoutPlanCollection();
        Typeface _typeface;
        ScriptLang _scriptLang;
        GlyphSubStitution _gsub;
        GlyphSetPosition _gpos;
        bool _needPlanUpdate;
        IPixelScaleLayout _pxscaleLayout;

        internal GlyphIndexList _inputGlyphs = new GlyphIndexList();
        internal GlyphPosStream _glyphPositions = new GlyphPosStream();
        internal List<GlyphPlan> _myGlyphPlans = new List<GlyphPlan>();


        public GlyphLayout()
        {
            PositionTechnique = PositionTechnique.OpenFont;
            ScriptLang = ScriptLangs.Latin;
        }
        public float FontSizeInPoints { get; set; }
        public float PixelScale
        {
            get
            {
                //to pixel scale from size in point
                return _typeface.CalculateToPixelScaleFromPointSize(this.FontSizeInPoints);
            }
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
        public IPixelScaleLayout PxScaleLayout
        {
            get { return _pxscaleLayout; }
            set
            {
                _pxscaleLayout = value;
            }
        }
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

            Typeface typeface = this._typeface;
            //clear before use
            _inputGlyphs.Clear();
            for (int i = 0; i < len; ++i)
            {
                //this is important!
                //-----------------------
                // from @samhocevar's PR: (https://github.com/LayoutFarm/Typography/pull/56/commits/b71c7cf863531ebf5caa478354d3249bde40b96e)
                //In many places, "char" is not a valid type to handle characters, because it
                //only supports 16 bits.In order to handle the full range of Unicode characters,
                //we need to use "int".
                //This allows characters such as 🙌 or 𐐷 or to be treated as single codepoints even
                //though they are encoded as two "char"s in a C# string.
                //-----------------------

                //convert input char to input glyphs
                char ch = str[startAt + i];
                int codepoint = ch;
                if (ch >= 0xd800 && ch <= 0xdbff && i + 1 < len)
                {
                    ++i;
                    codepoint = char.ConvertToUtf32(ch, str[startAt + i]);
                }
                _inputGlyphs.AddGlyph(codepoint, typeface.LookupIndex(codepoint));
            }
            //----------------------------------------------  
            //glyph substitution            
            if (_gsub != null & len > 0)
            {
                //TODO: review perf here
                _gsub.EnableLigation = this.EnableLigature;
                _gsub.DoSubstitution(_inputGlyphs);
                //
                _inputGlyphs.CreateMapFromUserCharToGlyphIndics();
            }
            //----------------------------------------------  
            //after glyph substitution,
            //number of input glyph MAY changed (increase or decrease).***
            //so count again.
            int finalGlyphCount = _inputGlyphs.Count;
            //----------------------------------------------  
            //glyph position
            _glyphPositions.Clear();
            _glyphPositions.Typeface = typeface;
            for (int i = 0; i < finalGlyphCount; ++i)
            {
                //at this stage _inputGlyphs and _glyphPositions 
                //has member 1:1
                ushort glyIndex = _inputGlyphs[i];
                //
                Glyph orgGlyph = typeface.GetGlyphByIndex(glyIndex);
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


    public delegate void GlyphReadOutputDelegate(int index, GlyphPlan glyphPlan);

    public static class GlyphLayoutExtensions
    {

        public static float SnapToFitInteger(float value)
        {
            int floor_value = (int)value;
            return (value - floor_value >= (1f / 2f)) ? floor_value + 1 : floor_value;
        }
        public static float SnapHalf(float value)
        {
            int floor_value = (int)value;
            //round to int 0, 0.5,1.0
            return (value - floor_value >= (2f / 3f)) ? floor_value + 1 : //else->
                   (value - floor_value >= (1f / 3f)) ? floor_value + 0.5f : floor_value;
        }
        static int SnapUpper(float value)
        {
            int floor_value = (int)value;
            return floor_value + 1;
        }

        /// <summary>
        /// read latest layout output into outputGlyphPlanList
        /// </summary>
        /// <param name="glyphLayout"></param>
        /// <param name="outputGlyphPlanList"></param>
        public static void ReadOutput(this GlyphLayout glyphLayout, List<UserCharToGlyphIndexMap> outputGlyphPlanList)
        {
            outputGlyphPlanList.AddRange(glyphLayout._inputGlyphs._mapUserCharToGlyphIndics);
        }
        /// <summary>
        /// read latest layout output into outputGlyphPlanList
        /// </summary>
        public static void ReadOutput(this GlyphLayout glyphLayout, List<GlyphPlan> outputGlyphPlanList)
        {
            GlyphPosStream glyphPositions = glyphLayout._glyphPositions; //from opentype's layout result, 
            int finalGlyphCount = glyphPositions.Count;
            //------------------------ 
            IPixelScaleLayout pxscaleLayout = glyphLayout.PxScaleLayout;
            if (pxscaleLayout != null)
            {
                //use custom pixel scale layout engine 

                pxscaleLayout.SetFont(glyphLayout.Typeface, glyphLayout.FontSizeInPoints);
                pxscaleLayout.Layout(glyphPositions, outputGlyphPlanList);
            }
            else
            {
                //default scale
                float pxscale = glyphLayout.PixelScale;
                double cx = 0;
                short cy = 0;
                for (int i = 0; i < finalGlyphCount; ++i)
                {
                    GlyphPos glyph_pos = glyphPositions[i];
                    float advW = glyph_pos.advanceW * pxscale;
                    float exact_x = (float)(cx + glyph_pos.OffsetX * pxscale);
                    float exact_y = (float)(cy + glyph_pos.OffsetY * pxscale);

                    outputGlyphPlanList.Add(new GlyphPlan(
                        glyph_pos.glyphIndex,
                        exact_x,
                        exact_y,
                        advW));
                    cx += advW;
                }
            }
        }
        /// <summary>
        /// read latest layout output
        /// </summary>
        /// <param name="glyphLayout"></param>
        /// <param name="readDel"></param>
        public static void ReadOutput(this GlyphLayout glyphLayout, GlyphReadOutputDelegate readDel)
        {
            throw new NotSupportedException();

            //Typeface typeface = glyphLayout.Typeface;
            //List<GlyphPos> glyphPositions = glyphLayout._glyphPositions;
            ////3.read back
            //int finalGlyphCount = glyphPositions.Count;
            //int cx = 0;
            //short cy = 0;

            //PositionTechnique posTech = glyphLayout.PositionTechnique;
            //ushort prev_index = 0;
            //for (int i = 0; i < finalGlyphCount; ++i)
            //{

            //    GlyphPos glyphPos = glyphPositions[i];
            //    //----------------------------------   
            //    switch (posTech)
            //    {
            //        default: throw new NotSupportedException();
            //        case PositionTechnique.None:
            //            readDel(i, new GlyphPlan(glyphPos.glyphIndex, cx, cy, glyphPos.AdvWidth));
            //            break;
            //        case PositionTechnique.OpenFont:
            //            readDel(i, new GlyphPlan(
            //                glyphPos.glyphIndex,
            //                cx + glyphPos.xoffset,
            //                (short)(cy + glyphPos.yoffset),
            //                glyphPos.AdvWidth));
            //            break;
            //        case PositionTechnique.Kerning:

            //            if (i > 0)
            //            {
            //                cx += typeface.GetKernDistance(prev_index, glyphPos.glyphIndex);
            //            }
            //            readDel(i, new GlyphPlan(
            //                 prev_index = glyphPos.glyphIndex,
            //               cx,
            //               cy,
            //               glyphPos.AdvWidth));

            //            break;
            //    }
            //    cx += glyphPos.AdvWidth;
            //}
        }
        public static void Layout(this GlyphLayout glyphLayout, Typeface typeface, char[] str, int startAt, int len, List<GlyphPlan> outputGlyphList)
        {   
            glyphLayout.Typeface = typeface;
            glyphLayout.Layout(str, startAt, len);
            glyphLayout.ReadOutput(outputGlyphList);
        }
        public static void Layout(this GlyphLayout glyphLayout, char[] str, int startAt, int len, List<GlyphPlan> outputGlyphList)
        {
            glyphLayout.Layout(str, startAt, len);
            glyphLayout.ReadOutput(outputGlyphList);
        } 
        public static void GenerateGlyphPlans(this GlyphLayout glyphLayout,
                  char[] textBuffer,
                  int startAt,
                  int len,
                  List<GlyphPlan> userGlyphPlanList,
                  List<UserCharToGlyphIndexMap> charToGlyphMapList)
        {
            //generate glyph plan based on its current setting
            glyphLayout.Layout(textBuffer, startAt, len, userGlyphPlanList);
            //note that we print to userGlyphPlanList
            //---------------- 
            //3. user char to glyph index map
            if (charToGlyphMapList != null)
            {
                glyphLayout.ReadOutput(charToGlyphMapList);
            }

        }
        public static void MeasureString(
                this GlyphLayout glyphLayout,
                char[] textBuffer,
                int startAt,
                int len, out MeasuredStringBox strBox, float scale)
        {
            //TODO: consider extension method
            List<GlyphPlan> outputGlyphPlans = glyphLayout._myGlyphPlans;
            outputGlyphPlans.Clear();
            glyphLayout.Layout(textBuffer, startAt, len, outputGlyphPlans);

            //
            int j = outputGlyphPlans.Count;
            Typeface currentTypeface = glyphLayout.Typeface;

            if (j == 0)
            {
                //not scale
                strBox = new MeasuredStringBox(0,
                    currentTypeface.Ascender * scale,
                    currentTypeface.Descender * scale,
                    currentTypeface.LineGap * scale);

            }
            else
            {
                GlyphPlan lastOne = outputGlyphPlans[j - 1];
                strBox = new MeasuredStringBox((lastOne.ExactRight) * scale,
                        currentTypeface.Ascender * scale,
                        currentTypeface.Descender * scale,
                        currentTypeface.LineGap * scale);
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
        public short advanceW; //acutally this value is ushort, TODO: review here

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