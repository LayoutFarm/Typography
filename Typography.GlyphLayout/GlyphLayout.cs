//MIT, 2016-2017, WinterDev
using System;
using System.Collections.Generic;
using Typography.OpenFont;


namespace Typography.TextLayout
{

    public struct GlyphPlan
    {
        public readonly ushort glyphIndex;
        public readonly float x;
        public readonly float y;
        public readonly float advX;
        public GlyphPlan(ushort glyphIndex, float x, float y, float advX)
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
        //glyph layout service


        //---------------------------
        GlyphLayoutPlanCollection _layoutPlanCollection = new GlyphLayoutPlanCollection();
        Typeface _typeface;
        ScriptLang _scriptLang;
        GlyphSubStitution _gsub;
        GlyphSetPosition _gpos;
        bool _needPlanUpdate;
        //--------------------------- 

        List<ushort> _inputGlyphs = new List<ushort>(); //not thread safe***
        List<GlyphPos> _glyphPositions = new List<GlyphPos>();//not thread safe*** 



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
        public void Layout(Typeface typeface,
            float size,
            char[] str,
            int startAt,
            int len,
            List<GlyphPlan> outputGlyphPlanList)
        {
            if (this._typeface != typeface)
            {
                this._typeface = typeface;
                _needPlanUpdate = true;
            }

            //get layout plan
            if (_needPlanUpdate)
            {
                UpdateLayoutPlan();
            }


            //GlyphLayoutPlan glyphCache;
            //if (!_glyphCaches.TryGetValue(typeface, out glyphCache))
            //{
            //    //create new 
            //    glyphCache = new GlyphLayoutPlan(typeface);
            //    _glyphCaches.Add(typeface, glyphCache);
            //}
            //----------------------------------------------  

            _inputGlyphs.Clear(); //clear before use
            for (int i = 0; i < len; ++i)
            {
                //convert input char to input glyphs
                _inputGlyphs.Add((ushort)typeface.LookupIndex(str[startAt + i]));
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
            //number of input glyph MAY changed (increase or decrease).

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
            //--------------
            float scale = typeface.CalculateFromPointToPixelScale(size);
            float cx = 0;
            float cy = 0;

            for (int i = 0; i < finalGlyphCount; ++i)
            {
                ushort glyIndex = _inputGlyphs[i];
                //this advWidth in font design unit   
                float advWidth = typeface.GetHAdvanceWidthFromGlyphIndex(glyIndex) * scale;
                //----------------------------------   
                switch (posTech)
                {
                    case PositionTechnique.None:
                        outputGlyphPlanList.Add(new GlyphPlan(glyIndex, cx, cy, advWidth));
                        break;
                    case PositionTechnique.OpenFont:
                        {
                            GlyphPos gpos_offset = _glyphPositions[i];
                            outputGlyphPlanList.Add(new GlyphPlan(
                                glyIndex,
                                cx + (scale * gpos_offset.xoffset),
                                cy + (scale * gpos_offset.yoffset),
                                advWidth));
                        }
                        break;
                    case PositionTechnique.Kerning:
                        {
                            outputGlyphPlanList.Add(new GlyphPlan(
                               glyIndex,
                               cx,
                               cy,
                               advWidth));
                            if (i > 0)
                            {
                                advWidth += typeface.GetKernDistance(outputGlyphPlanList[i - 1].glyphIndex, outputGlyphPlanList[i].glyphIndex) * scale;
                            }
                        }
                        break;
                }
                //--------
                cx += advWidth;
            }
        }

    }
}