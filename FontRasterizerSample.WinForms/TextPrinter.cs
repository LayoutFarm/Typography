//MIT, 2016,  WinterDev
using System;
using System.Windows.Forms;
using NRasterizer;
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
        public void Print(Typeface typeface, string str)
        {
            Print(typeface, str.ToCharArray());
        }
        public void Print(Typeface typeface, char[] str)
        {


        }
    }
}