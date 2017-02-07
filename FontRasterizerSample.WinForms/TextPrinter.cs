//MIT, 2016-2017, WinterDev
using System;
using System.Windows.Forms;
using System.Collections.Generic;
using NOpenType;
using NOpenType.Extensions;

using PixelFarm.Agg;

namespace SampleWinForms
{

    class GlyphPlan
    {
        public ushort glyphIndex;
        public float x;
        public float y;
        public float advX;
        public VertexStore vxs;
        public GlyphPlan(ushort glyphIndex)
        {
            this.glyphIndex = glyphIndex;
        }
    }

    class GlyphsCache
    {
        Dictionary<int, VertexStore> _glyphIndexToVxsDic = new Dictionary<int, VertexStore>();
        Typeface _typeface;
        public GlyphsCache(Typeface typeface)
        {
            _typeface = typeface;
        }



    }

    class TextPrinter
    {

        Dictionary<Typeface, GlyphsCache> _glyphCaches = new Dictionary<Typeface, GlyphsCache>();

        public TextPrinter()
        {
            //default
            EnableKerning = true;
        }
        public bool EnableKerning
        {
            get;
            set;
        }
        public bool EnableTrueTypeHint
        {
            get;
            set;
        }
        public bool UseAggVerticalHinting
        {
            get;
            set;
        }
    
        public void Print(Typeface typeface, float size, string str, List<GlyphPlan> glyphPlanBuffer)
        {
            Print(typeface, size, str.ToCharArray(), glyphPlanBuffer);
        }

        List<ushort> inputGlyphs = new List<ushort>(); //not thread safe***
        public void Print(Typeface typeface, float size, char[] str, List<GlyphPlan> glyphPlanBuffer)
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
            var glyphPathBuilder = new GlyphPathBuilderVxs(typeface);
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
                GlyphSubStitution glyphSubstitution = new GlyphSubStitution(typeface, "thai");
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
            if (j > 1)
            {
                //GlyphSetPosition glyphSetPos = new GlyphSetPosition(typeface, "thai");
                //glyphSetPos.DoGlyphPosition(glyphPositions);

            }
            //--------------
            float scale = typeface.CalculateScale(size);
            float cx = 0;
            float cy = 0;
            bool enable_kerning = this.EnableKerning;
            glyphPathBuilder.UseTrueTypeInterpreter = this.EnableTrueTypeHint;
            glyphPathBuilder.UseVerticalHinting = this.UseAggVerticalHinting;

            j = inputGlyphs.Count;
            
            for (int i = 0; i < j; ++i)
            {
                ushort glyIndex = inputGlyphs[i];
                GlyphPlan glyphPlan = new GlyphPlan(glyIndex);
                glyphPlanBuffer.Add(glyphPlan);

                //-----------------------------------
                //check if we static vxs/bmp for this glyph
                //if not, create and cache
                //-----------------------------------  
                glyphPathBuilder.BuildFromGlyphIndex(glyIndex, size);
                //----------------------------------- 
                var vxs = glyphPathBuilder.GetVxs();
                //this advWidth in font design unit 
                float advWidth = typeface.GetHAdvanceWidthFromGlyphIndex(glyIndex) * scale;
                //---------------------------------- 
                glyphPlan.x = cx;
                glyphPlan.y = 0;
                glyphPlan.advX = advWidth;
                glyphPlan.vxs = vxs;
                //

                if (enable_kerning && i > 0)
                {
                    //check kerning
                    advWidth += typeface.GetKernDistance(glyphPlanBuffer[i - 1].glyphIndex, glyphPlanBuffer[i].glyphIndex) * scale;
                }
                cx += advWidth;
            }

            //TODO:....
            //2.  
            //shaping, glyph substitution
            //3. layout glyph position
            //----------------------------------------------
            //4. actual render


        }

    }
}