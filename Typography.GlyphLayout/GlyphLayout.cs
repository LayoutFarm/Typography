//MIT, 2016-2017, WinterDev
using System;
using System.Collections.Generic;
using Typography.OpenFont;
using Typography.OpenFont.Extensions;


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
#if DEBUG
        public override string ToString()
        {
            return "(" + x + "," + y + "), adv:" + advX;
        }
#endif
    }

    class GlyphsCache
    {

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
        /// use openfont gpos table
        /// </summary>
        OpenFont,
    }


    public class GlyphLayout
    {

        Dictionary<Typeface, GlyphsCache> _glyphCaches = new Dictionary<Typeface, GlyphsCache>();
        public GlyphLayout()
        {
            PositionTechnique = PositionTecnhique.OpenFont;
            ScriptLang = ScriptLangs.Latin;
        }
        public PositionTecnhique PositionTechnique { get; set; }
        public ScriptLang ScriptLang { get; set; }
        public bool EnableLigature { get; set; }


        public void Layout(Typeface typeface, float size, string str, List<GlyphPlan> glyphPlanBuffer)
        {
            Layout(typeface, size, str.ToCharArray(), glyphPlanBuffer);
        }
        List<ushort> inputGlyphs = new List<ushort>(); //not thread safe***



        public void Layout(Typeface typeface, float size, char[] str, List<GlyphPlan> glyphPlanBuffer)
        {
            //---------------------------------------------- 
            //1. convert char[] to glyph[]
            //2. send to shaping engine
            //3. layout position of each glyph 
            //----------------------------------------------   
            //check if we have created a glyph cache for the typeface
            GlyphsCache glyphCache;
            if (!_glyphCaches.TryGetValue(typeface, out glyphCache))
            {
                //create new 
                glyphCache = new GlyphsCache(typeface);
                _glyphCaches.Add(typeface, glyphCache);
            }

            //----------------------------------------------  
            int j = str.Length;
            inputGlyphs.Clear();
            for (int i = 0; i < j; ++i)
            {
                //1. convert char[] to glyphIndex[]
                inputGlyphs.Add((ushort)typeface.LookupIndex(str[i]));
            }
            //----------------------------------------------  
            //glyph substitution
            if (j > 1)
            {
                GlyphSubStitution glyphSubstitution = new GlyphSubStitution(typeface, this.ScriptLang.shortname);
                glyphSubstitution.EnableLigation = this.EnableLigature;
                glyphSubstitution.DoSubstitution(inputGlyphs);
            }
            //----------------------------------------------  
            //glyph position
            j = inputGlyphs.Count;
            List<GlyphPos> glyphPositions = new List<GlyphPos>(j);
            for (int i = 0; i < j; ++i)
            {
                ushort glyIndex = inputGlyphs[i];
                glyphPositions.Add(new GlyphPos(
                    glyIndex,
                    typeface.GetGlyphByIndex(glyIndex).GlyphClass,
                    typeface.GetHAdvanceWidthFromGlyphIndex(glyIndex))
                   );
            }

            PositionTecnhique posTech = this.PositionTechnique;
            if (j > 1 && posTech == PositionTecnhique.OpenFont)
            {
                GlyphSetPosition glyphSetPos = new GlyphSetPosition(typeface, ScriptLang.shortname);
                glyphSetPos.DoGlyphPosition(glyphPositions);
            }
            //--------------
            float scale = typeface.CalculateFromPointToPixelScale(size);
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
                    case PositionTecnhique.OpenFont:
                        {
                            GlyphPos gpos_offset = glyphPositions[i];
                            glyphPlan.x = cx + (scale * gpos_offset.xoffset);
                            glyphPlan.y = cy + (scale * gpos_offset.yoffset);
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