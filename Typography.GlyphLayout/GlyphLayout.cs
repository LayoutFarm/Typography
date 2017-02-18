//MIT, 2016-2017, WinterDev
using System;
using System.Collections.Generic;
using Typography.OpenType;
using Typography.OpenType.Extensions;


namespace Typography.TextLayout
{

    public class GlyphPlan
    {
        public ushort glyphIndex;
        public float x;
        public float y;
        public float advX;
        public object vxs; /*VertexStore*/
        public GlyphPlan(ushort glyphIndex)
        {
            this.glyphIndex = glyphIndex;
        }
    }

    class GlyphsCache
    {
        Dictionary<int, object> _glyphIndexToVxsDic = new Dictionary<int, object>();
        Typeface _typeface;
        public GlyphsCache(Typeface typeface)
        {
            _typeface = typeface;
        }
    }

    public enum PositionTecnhique
    {
        None,
        /// <summary>
        /// use kerning table (old)
        /// </summary>
        Kerning, //old technique
        /// <summary>
        /// use opentype gpos table
        /// </summary>
        OpenType,
    }
    
    // 
    public class GlyphLayout
    {

        //glyph shaper engine? 
        Dictionary<Typeface, GlyphsCache> _glyphCaches = new Dictionary<Typeface, GlyphsCache>();
        public GlyphLayout()
        {
            PositionTechnique = PositionTecnhique.OpenType;
            ScriptLang = ScriptLangs.Latin;
        }
        public PositionTecnhique PositionTechnique { get; set; }
        public ScriptLang ScriptLang { get; set; }

        public void Layout(Typeface typeface, float size, string str, List<GlyphPlan> glyphPlanBuffer)
        {
            Layout(typeface, size, str.ToCharArray(), glyphPlanBuffer);
        }
        List<ushort> inputGlyphs = new List<ushort>(); //not thread safe***

        public void Layout(Typeface typeface, float size, char[] str, List<GlyphPlan> glyphPlanBuffer)
        {

            //check if we have created a glyph cache
            GlyphsCache glyphCache;
            if (!_glyphCaches.TryGetValue(typeface, out glyphCache))
            {
                //create new 
                glyphCache = new GlyphsCache(typeface);
                _glyphCaches.Add(typeface, glyphCache);
            }

            //---------------------------------------------- 
            //1. convert char[] to glyph[]
            //2. send to shaping engine
            //3. layout position of each glyph 
            //----------------------------------------------  

            int j = str.Length;

            //TODO:....
            //2.  
            //shaping,
            //glyph substitution

            inputGlyphs.Clear();
            for (int i = 0; i < j; ++i)
            {
                //1st 
                inputGlyphs.Add((ushort)typeface.LookupIndex(str[i]));
            }
            if (j > 1)
            {

                //for debug
                //test for thai lang 
                GlyphSubStitution glyphSubstitution = new GlyphSubStitution(typeface, this.ScriptLang.shortname);
                glyphSubstitution.DoSubstitution(inputGlyphs);
            }

            //set glyph position
            j = inputGlyphs.Count;
            List<GlyphPos> glyphPositions = new List<GlyphPos>(j);
            for (int i = 0; i < j; ++i)
            {
                ushort glyIndex = inputGlyphs[i];
                glyphPositions.Add(new GlyphPos(glyIndex));
            }
            //--------------
            //do gpos
            PositionTecnhique posTech = this.PositionTechnique;
            if (j > 1 && posTech == PositionTecnhique.OpenType)
            {
                GlyphSetPosition glyphSetPos = new GlyphSetPosition(typeface, ScriptLang.shortname);
                glyphSetPos.DoGlyphPosition(glyphPositions);
            }
            //--------------
            float scale = typeface.CalculateScale(size);
            float cx = 0;
            float cy = 0;

            j = inputGlyphs.Count;

            for (int i = 0; i < j; ++i)
            {
                ushort glyIndex = inputGlyphs[i];
                GlyphPlan glyphPlan = new GlyphPlan(glyIndex);
                glyphPlanBuffer.Add(glyphPlan);
                //this advWidth in font design unit 
                float advWidth = typeface.GetHAdvanceWidthFromGlyphIndex(glyIndex) * scale;
                //----------------------------------  

                switch (posTech)
                {
                    case PositionTecnhique.None:
                        {
                            glyphPlan.x = cx;
                            glyphPlan.y = cy;
                            glyphPlan.advX = advWidth;
                        }
                        break;
                    case PositionTecnhique.OpenType:
                        {
                            GlyphPos gpos_offset = glyphPositions[i];
                            glyphPlan.x = cx + (scale * glyphPositions[i].x);
                            glyphPlan.y = cy + (scale * glyphPositions[i].y);
                            glyphPlan.advX = advWidth;
                        }
                        break;
                    case PositionTecnhique.Kerning:
                        {
                            glyphPlan.x = cx;
                            glyphPlan.y = cy;
                            glyphPlan.advX = advWidth;
                            if (i > 0)
                            {
                                advWidth += typeface.GetKernDistance(glyphPlanBuffer[i - 1].glyphIndex, glyphPlanBuffer[i].glyphIndex) * scale;
                            }
                        }
                        break;
                }
                cx += advWidth;
            }
        }

    }
}