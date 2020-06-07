//MIT, 2020, WinnterDev
using System;
using System.Collections.Generic;
using System.Text;
using Typography.OpenFont;

namespace Typography.TextBreak
{
    static class UnicodeRangeFinder
    {
        //TODO: review this again, with AUTOGEN code

        static SpanBreakInfo s_thai = new SpanBreakInfo(false, (char)0x0E00, "thai");
        static SpanBreakInfo s_loa = new SpanBreakInfo(false, (char)0x0E80, "loa");

        static SpanBreakInfo s_arabic = new SpanBreakInfo(true, (char)0x0600, "arab");
        static SpanBreakInfo s_arabic_supplement = new SpanBreakInfo(true, (char)0x0750, "arab");
        static SpanBreakInfo s_arabic_presentation_form_a = new SpanBreakInfo(true, (char)0xFB50, "arab");
        static SpanBreakInfo s_arabic_presentation_form_b = new SpanBreakInfo(true, (char)0xFE70, "arab");

        public static bool GetUniCodeRangeFor(char c1, out int startCodePoint, out int endCodePoint, out SpanBreakInfo spanBreakInfo)
        {
            //find proper unicode range (and its lang)
            //Thai

            {
                const char s_firstChar = (char)0x0E00;
                const char s_lastChar = (char)0xE7F;
                if (c1 >= s_firstChar && c1 <= s_lastChar)
                {
                    startCodePoint = s_firstChar;
                    endCodePoint = s_lastChar;
                    spanBreakInfo = s_thai;
                    return true;
                }
            }
            //Loa
            {
                const char s_firstChar = (char)0x0E80;
                const char s_lastChar = (char)0x0EFF;
                if (c1 >= s_firstChar && c1 <= s_lastChar)
                {
                    startCodePoint = s_firstChar;
                    endCodePoint = s_lastChar;
                    spanBreakInfo = s_loa;
                    return true;
                }
            }

            {
                //https://en.wikipedia.org/wiki/Arabic_script_in_Unicode             

                //Rumi Numeral Symbols(10E60–10E7F, 31 characters)
                //Indic Siyaq Numbers(1EC70–1ECBF, 68 characters)
                //Ottoman Siyaq Numbers(1ED00–1ED4F, 61 characters)
                //Arabic Mathematical Alphabetic Symbols(1EE00–1EEFF, 143 characters) 

                if (c1 >= 0x0600 && c1 <= 0x06FF)
                {
                    //Arabic (0600–06FF, 255 characters)
                    startCodePoint = 0x0600;
                    endCodePoint = 0x06FF;
                    spanBreakInfo = s_arabic;
                    return true;
                }
                else if (c1 >= 0x0750 && c1 <= 0x077F)
                {
                    //Arabic Supplement(0750–077F, 48 characters)
                    startCodePoint = 0x0750;
                    endCodePoint = 0x077F;
                    spanBreakInfo = s_arabic_supplement;
                    return true;
                }
                else if (c1 >= 0x8A0 && c1 <= 0x08FF)
                {
                    //Arabic Extended-A(08A0–08FF, 84 characters)
                    startCodePoint = 0x8A0;
                    endCodePoint = 0x08FF;
                    spanBreakInfo = s_arabic; //TODO: review here
                    return true;
                }
                else if (c1 >= 0xFB50 && c1 <= 0xFDFF)
                {
                    //Arabic Presentation Forms - A(FB50–FDFF, 611 characters)
                    startCodePoint = 0xFB50;
                    endCodePoint = 0xFDFF;
                    spanBreakInfo = s_arabic_presentation_form_a; //TODO: review here
                    return true;
                }
                else if (c1 >= 0xFE70 && c1 <= 0xFEFF)
                {
                    //Arabic Presentation Forms - B(FE70–FEFF, 141 characters)
                    startCodePoint = 0xFE70;
                    endCodePoint = 0xFEFF;
                    spanBreakInfo = s_arabic_presentation_form_b; //TODO: review here
                    return true;
                }
                else
                {
                    startCodePoint = 0;
                    endCodePoint = 0;
                    spanBreakInfo = null;
                    return false;
                }
            }
        }
    }
}
