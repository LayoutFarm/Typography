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

    class TextPrinter
    {

        public void Print(Typeface typeface, float size, string str, GlyphPlan[] glyphPlanBuffer)
        {
            Print(typeface, size, str.ToCharArray(), glyphPlanBuffer);
        }
        public void Print(Typeface typeface, float size, char[] str, GlyphPlan[] glyphPlanBuffer)
        {
            //1. convert char[] to glyph[]
            //2. send to shaping engine
            //3. layout position of each glyph 
            //---------------------------------------------- 

            var glyphPathBuilder = new GlyphPathBuilderVxs(typeface);
            int j = str.Length;

            float scale = GlyphPathBuilder.GetFUnitToPixelsScale(size,
                glyphPathBuilder.Resolution,
                typeface.UnitsPerEm);


            //vxs cache
            //Dictionary<int, GlyphPathBuilderVxs> vxsDic = new Dictionary<int, GlyphPathBuilderVxs>();

            float cx = 0;
            float cy = 0;
            for (int i = 0; i < j; ++i)
            {
                ushort glyIndex = (ushort)typeface.LookupIndex(str[i]);
                //-----------------------------------
                //check if we static vxs/bmp for this glyph
                //if not, create and cache
                //-----------------------------------  

                glyphPathBuilder.BuildFromGlyphIndex(glyIndex, size);
                //----------------------------------- 
                var vxs = glyphPathBuilder.GetVxs();

                //this advWidth in font design unit 
                float advWidth = typeface.GetAdvanceWidthFromGlyphIndex(glyIndex) * scale;
                //----------------------------------- 
                GlyphPlan glyphPlan = new GlyphPlan();
                glyphPlan.glyphIndex = glyIndex;
                glyphPlan.x = cx;
                glyphPlan.y = 0;
                glyphPlan.advX = advWidth;
                glyphPlan.vxs = vxs;
                //
                glyphPlanBuffer[i] = glyphPlan;
                //
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