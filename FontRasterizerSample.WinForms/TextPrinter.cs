//MIT, 2016,  WinterDev
using System;
using System.Windows.Forms;
using System.Collections.Generic;
using NRasterizer;
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
        public void Print(Typeface typeface, float size, string str, GlyphPlan[] glyphPlanBuffer)
        {
            Print(typeface, size, str.ToCharArray(), glyphPlanBuffer);
        }

        public void Print(Typeface typeface, float size, char[] str, GlyphPlan[] glyphPlanBuffer)
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
            //shaping, glyph substitution
            for (int i = 0; i < j; ++i)
            {
                var glyphPlan = new GlyphPlan();
                glyphPlan.glyphIndex = (ushort)typeface.LookupIndex(str[i]);
                glyphPlanBuffer[i] = glyphPlan;
            }


            float scale = GlyphPathBuilder.GetFUnitToPixelsScale(size,
                glyphPathBuilder.Resolution,
                typeface.UnitsPerEm);
            float cx = 0;
            float cy = 0;
            bool enable_kerning = this.EnableKerning;
            for (int i = 0; i < j; ++i)
            {
                GlyphPlan glyphPlan = glyphPlanBuffer[i];
                ushort glyIndex = glyphPlan.glyphIndex;
                //-----------------------------------
                //check if we static vxs/bmp for this glyph
                //if not, create and cache
                //-----------------------------------  
                glyphPathBuilder.BuildFromGlyphIndex(glyIndex, size);
                //----------------------------------- 
                var vxs = glyphPathBuilder.GetVxs();
                //this advWidth in font design unit 
                float advWidth = typeface.GetAdvanceWidthFromGlyphIndex(glyIndex) * scale;
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