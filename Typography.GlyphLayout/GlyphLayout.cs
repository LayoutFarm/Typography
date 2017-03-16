//MIT, 2016-2017, WinterDev
using System;
using System.Collections.Generic;
using Typography.OpenFont;


namespace Typography.TextLayout
{

    public struct GlyphPlan
    {
        public readonly ushort glyphIndex;//2
        public readonly int x;//4
        public readonly short y;//2
        public readonly ushort advX;//2
        public GlyphPlan(ushort glyphIndex, int x, short y, ushort advX)
        {
            this.glyphIndex = glyphIndex;
            this.x = x;
            this.y = y;
            this.advX = advX;
        }
#if DEBUG
        public override string ToString()
        {
            return "(" + x + "," + y + "), adv:" + advX;
        }
#endif
    }


    public enum PositionTechnique
    {
        None,
        /// <summary>
        /// use kerning table (old)
        /// </summary>
        Kerning, //old technique
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




    public class GlyphLayout
    {
        GlyphLayoutPlanCollection _layoutPlanCollection = new GlyphLayoutPlanCollection();
        Typeface _typeface;
        ScriptLang _scriptLang;
        GlyphSubStitution _gsub;
        GlyphSetPosition _gpos;
        bool _needPlanUpdate;

        GlyphIndexList _inputGlyphs = new GlyphIndexList();
        internal List<GlyphPos> _glyphPositions = new List<GlyphPos>();

        public GlyphLayout()
        {
            PositionTechnique = PositionTechnique.OpenFont;
            ScriptLang = ScriptLangs.Latin;
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

        void UpdateLayoutPlan()
        {
            GlyphLayoutPlanContext context = _layoutPlanCollection.GetPlanOrCreate(this._typeface, this._scriptLang);
            this._gpos = context._glyphPos;
            this._gsub = context._glyphSub;
            _needPlanUpdate = false;
        }

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
                //convert input char to input glyphs
                char c = str[startAt + i];
                _inputGlyphs.AddGlyph(c, (ushort)typeface.LookupIndex(c));
            }
            //----------------------------------------------  
            //glyph substitution            
            if (_gsub != null & len > 1)
            {
                //TODO: review perf here
                _gsub.EnableLigation = this.EnableLigature;
                _gsub.DoSubstitution(_inputGlyphs);
            }
            //----------------------------------------------  
            //after glyph substitution,
            //number of input glyph MAY changed (increase or decrease).***

            //so count again.
            int finalGlyphCount = _inputGlyphs.Count;
            //----------------------------------------------  
            //glyph position
            _glyphPositions.Clear();
            for (int i = 0; i < finalGlyphCount; ++i)
            {
                ushort glyIndex = _inputGlyphs[i];
                _glyphPositions.Add(new GlyphPos(
                    glyIndex,
                    typeface.GetGlyphByIndex(glyIndex).GlyphClass,
                    typeface.GetHAdvanceWidthFromGlyphIndex(glyIndex))
                   );
            }
            PositionTechnique posTech = this.PositionTechnique;
            if (_gpos != null && len > 1 && posTech == PositionTechnique.OpenFont)
            {
                _gpos.DoGlyphPosition(_glyphPositions);
            }
        }

    }


    public delegate void GlyphReadOutputDelegate(int index, GlyphPlan glyphPlan);

    public static class GlyphLayoutExtensions
    {

        /// <summary>
        /// read latest layout output
        /// </summary>
        public static void ReadOutput(this GlyphLayout glyphOut, List<GlyphPlan> outputGlyphPlanList)
        {
            Typeface typeface = glyphOut.Typeface;
            List<GlyphPos> glyphPositions = glyphOut._glyphPositions;
            //3.read back
            int finalGlyphCount = glyphPositions.Count;
            int cx = 0;
            short cy = 0;

            PositionTechnique posTech = glyphOut.PositionTechnique;
            ushort prev_index = 0;
            for (int i = 0; i < finalGlyphCount; ++i)
            {

                GlyphPos glyphPos = glyphPositions[i];
                //----------------------------------   
                switch (posTech)
                {
                    default: throw new NotSupportedException();
                    case PositionTechnique.None:
                        outputGlyphPlanList.Add(new GlyphPlan(glyphPos.glyphIndex, cx, cy, glyphPos.advWidth));
                        break;
                    case PositionTechnique.OpenFont:
                        outputGlyphPlanList.Add(new GlyphPlan(
                            glyphPos.glyphIndex,
                            cx + glyphPos.xoffset,
                            (short)(cy + glyphPos.yoffset),
                            glyphPos.advWidth));
                        break;
                    case PositionTechnique.Kerning:

                        if (i > 0)
                        {
                            cx += typeface.GetKernDistance(prev_index, glyphPos.glyphIndex);
                        }
                        outputGlyphPlanList.Add(new GlyphPlan(
                           prev_index = glyphPos.glyphIndex,
                           cx,
                           cy,
                           glyphPos.advWidth));

                        break;
                }
                cx += glyphPos.advWidth;
            }
        }
        /// <summary>
        /// read latest layout output
        /// </summary>
        /// <param name="glyphOut"></param>
        /// <param name="readDel"></param>
        public static void ReadOutput(this GlyphLayout glyphOut, GlyphReadOutputDelegate readDel)
        {
            Typeface typeface = glyphOut.Typeface;
            List<GlyphPos> glyphPositions = glyphOut._glyphPositions;
            //3.read back
            int finalGlyphCount = glyphPositions.Count;
            int cx = 0;
            short cy = 0;

            PositionTechnique posTech = glyphOut.PositionTechnique;
            ushort prev_index = 0;
            for (int i = 0; i < finalGlyphCount; ++i)
            {

                GlyphPos glyphPos = glyphPositions[i];
                //----------------------------------   
                switch (posTech)
                {
                    default: throw new NotSupportedException();
                    case PositionTechnique.None:
                        readDel(i, new GlyphPlan(glyphPos.glyphIndex, cx, cy, glyphPos.advWidth));
                        break;
                    case PositionTechnique.OpenFont:
                        readDel(i, new GlyphPlan(
                            glyphPos.glyphIndex,
                            cx + glyphPos.xoffset,
                            (short)(cy + glyphPos.yoffset),
                            glyphPos.advWidth));
                        break;
                    case PositionTechnique.Kerning:

                        if (i > 0)
                        {
                            cx += typeface.GetKernDistance(prev_index, glyphPos.glyphIndex);
                        }
                        readDel(i, new GlyphPlan(
                             prev_index = glyphPos.glyphIndex,
                           cx,
                           cy,
                           glyphPos.advWidth));

                        break;
                }
                cx += glyphPos.advWidth;
            }
        }




        public static void Layout(this GlyphLayout glyphOut, Typeface typeface, char[] str, int startAt, int len, List<GlyphPlan> outputGlyphList)
        {
            glyphOut.Typeface = typeface;
            glyphOut.Layout(str, startAt, len);
            glyphOut.ReadOutput(outputGlyphList);
        }
        public static void Layout(this GlyphLayout glyphOut, char[] str, int startAt, int len, List<GlyphPlan> outputGlyphList)
        {
            glyphOut.Layout(str, startAt, len);
            glyphOut.ReadOutput(outputGlyphList);
        }
        public static void Layout(this GlyphLayout glyphOut, char[] str, int startAt, int len, GlyphReadOutputDelegate readDel)
        {
            glyphOut.Layout(str, startAt, len);
            glyphOut.ReadOutput(readDel);

        }
    }

}