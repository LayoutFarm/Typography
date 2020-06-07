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

        public static bool GetUniCodeRangeFor(char c1, out int startCodePoint, out int endCodePoint, out UnicodeLangBits langBits)
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
                    langBits = UnicodeLangBits.Thai;
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

                    langBits = UnicodeLangBits.Lao;
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
                    langBits = UnicodeLangBits.Arabic;
                    return true;
                }
                else if (c1 >= 0x0750 && c1 <= 0x077F)
                {
                    //Arabic Supplement(0750–077F, 48 characters)
                    startCodePoint = 0x0750;
                    endCodePoint = 0x077F;
                    langBits = UnicodeLangBits.Arabic_Supplement;
                    return true;
                }
                else if (c1 >= 0x8A0 && c1 <= 0x08FF)
                {
                    //Arabic Extended-A(08A0–08FF, 84 characters)
                    startCodePoint = 0x8A0;
                    endCodePoint = 0x08FF;
                    langBits = UnicodeLangBits.Arabic; //???
                    return true;
                }
                else if (c1 >= 0xFB50 && c1 <= 0xFDFF)
                {
                    //Arabic Presentation Forms - A(FB50–FDFF, 611 characters)
                    startCodePoint = 0xFB50;
                    endCodePoint = 0xFDFF;
                    langBits = UnicodeLangBits.Arabic_Presentation_Forms_A; //???
                    return true;
                }
                else if (c1 >= 0xFE70 && c1 <= 0xFEFF)
                {
                    //Arabic Presentation Forms - B(FE70–FEFF, 141 characters)
                    startCodePoint = 0xFE70;
                    endCodePoint = 0xFEFF;
                    langBits = UnicodeLangBits.Arabic_Presentation_Forms_B; //???
                    return true;
                }
                else
                {
                    startCodePoint = 0;
                    endCodePoint = 0;
                    langBits = UnicodeLangBits.Unset;
                    return false;
                }
            }
        }
    }
}
