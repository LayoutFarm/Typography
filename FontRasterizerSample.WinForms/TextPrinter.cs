//MIT, 2016,  WinterDev
using System;
using System.Windows.Forms;
using NRasterizer;
using PixelFarm.Agg;
namespace SampleWinForms
{

    struct GlyphPlan
    {
        public readonly ushort glyphIndeex;
        public readonly ushort x;
        public readonly ushort y;
        public readonly ushort advX;
    }

    class TextPrinter
    {
        public void Print(Typeface typeface, float size, string str)
        {
            Print(typeface, size, str.ToCharArray());
        }
        public void Print(Typeface typeface, float size, char[] str)
        {
            //1. convert char[] to glyph[]
            //2. send to shaping engine
            //3. layout position of each glyph 
            //---------------------------------------------- 

            var glyphPathBuilder = new GlyphPathBuilderVxs(typeface);
            int j = str.Length;
         
            //2. 
            //shaping, glyph substitution
            ushort[] glyphIndices = new ushort[j];
            float cx = 0;
            for (int i = 0; i < j; ++i)
            {
                ushort glyIndex = (ushort)typeface.LookupIndex(str[i]);
                //-----------------------------------
                //check if we static vxs/bmp for this glyph
                //if not, create and cache
                //----------------------------------- 
                glyphIndices[i] = glyIndex;
                glyphPathBuilder.BuildFromGlyphIndex(glyIndex, size );
                //----------------------------------- 
                var vxs = glyphPathBuilder.GetVxs();
                //this advWidth in font design unit 
                ushort advWidth = typeface.GetAdvanceWidthFromGlyphIndex(glyIndex);
                //
            }

            //3. layout glyph position
            //----------------------------------------------




        }
    }
}