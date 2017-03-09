//MIT, 2016-2017, WinterDev
using System;
using System.Collections.Generic;
using Typography.OpenFont;
using Typography.OpenFont.Extensions;


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

    class GlyphsCache
    {

        Typeface _typeface;
        public GlyphsCache(Typeface typeface)
        {
            _typeface = typeface;
        }
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


    public class GlyphLayout
    {

        Dictionary<Typeface, GlyphsCache> _glyphCaches = new Dictionary<Typeface, GlyphsCache>();
        public GlyphLayout()
        {
            PositionTechnique = PositionTechnique.OpenFont;
            ScriptLang = ScriptLangs.Latin;
        }
        public PositionTechnique PositionTechnique { get; set; }
        public ScriptLang ScriptLang { get; set; }
        public bool EnableLigature { get; set; }


        public void Layout(Typeface typeface, float size, string str, List<GlyphPlan> glyphPlanBuffer)
        {
            char[] buffer = str.ToCharArray();
            Layout(typeface, size, buffer, 0, buffer.Length, glyphPlanBuffer);
        }

        List<ushort> _inputGlyphs = new List<ushort>(); //not thread safe***
        List<GlyphPos> _glyphPositions = new List<GlyphPos>();//not thread safe***


        public void Layout(Typeface typeface, float size, char[] str, int startAt, int len, List<GlyphPlan> glyphPlanBuffer)
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

            _inputGlyphs.Clear(); //clear before use
            for (int i = 0; i < len; ++i)
            {
                //convert input char to input glyphs
                _inputGlyphs.Add((ushort)typeface.LookupIndex(str[startAt + i]));
            }
            //----------------------------------------------  
            //glyph substitution            
            if (len > 1)
            {
                //TODO: review perf here
                var glyphSubstitution = new GlyphSubStitution(typeface, this.ScriptLang.shortname);
                glyphSubstitution.EnableLigation = this.EnableLigature;
                glyphSubstitution.DoSubstitution(_inputGlyphs);
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
            if (len > 1 && posTech == PositionTechnique.OpenFont)
            {
                //TODO: review perf here
                GlyphSetPosition glyphSetPos = new GlyphSetPosition(typeface, ScriptLang.shortname);
                glyphSetPos.DoGlyphPosition(_glyphPositions);
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
                        glyphPlanBuffer.Add(new GlyphPlan(glyIndex, cx, cy, advWidth));
                        break;
                    case PositionTechnique.OpenFont:
                        {
                            GlyphPos gpos_offset = _glyphPositions[i];
                            glyphPlanBuffer.Add(new GlyphPlan(
                                glyIndex,
                                cx + (scale * gpos_offset.xoffset),
                                cy + (scale * gpos_offset.yoffset),
                                advWidth));
                        }
                        break;
                    case PositionTechnique.Kerning:
                        {
                            glyphPlanBuffer.Add(new GlyphPlan(
                               glyIndex,
                               cx,
                               cy,
                               advWidth));
                            if (i > 0)
                            {
                                advWidth += typeface.GetKernDistance(glyphPlanBuffer[i - 1].glyphIndex, glyphPlanBuffer[i].glyphIndex) * scale;
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