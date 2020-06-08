//MIT, 2020, WinterDev
using System;
using System.Collections.Generic;
using System.Text;
using Typography.OpenFont;

namespace Typography.TextBreak
{
    static class UnicodeRangeFinder
    {
        //TODO: review this again, with AUTOGEN code

        static SpanBreakInfo s_thai = new SpanBreakInfo(false, ScriptTagDefs.Thai.Tag);
        static SpanBreakInfo s_lao = new SpanBreakInfo(false, ScriptTagDefs.Lao.Tag);

        static SpanBreakInfo s_arabic = new SpanBreakInfo(true, ScriptTagDefs.Arabic.Tag);
        static SpanBreakInfo s_arabic_supplement = new SpanBreakInfo(true, ScriptTagDefs.Arabic.Tag);
        static SpanBreakInfo s_arabic_presentation_form_a = new SpanBreakInfo(true, ScriptTagDefs.Arabic.Tag);
        static SpanBreakInfo s_arabic_presentation_form_b = new SpanBreakInfo(true, ScriptTagDefs.Arabic.Tag);


        static SpanBreakInfo s_hana = new SpanBreakInfo(false, ScriptTagDefs.Katakana.Tag);
        static SpanBreakInfo s_hangul = new SpanBreakInfo(false, ScriptTagDefs.Hangul.Tag);
        static SpanBreakInfo s_hangul_jumo = new SpanBreakInfo(false, ScriptTagDefs.Hangul_Jamo.Tag);
        static SpanBreakInfo s_hani = new SpanBreakInfo(false, ScriptTagDefs.CJK_Ideographic.Tag);

        //CJK_Symbols_And_Punctuation = (48L << 32) | (0x3000 << 16) | 0x303F,   
        //Enclosed_CJK_Letters_And_Months = (54L << 32) | (0x3200 << 16) | 0x32FF,
        //CJK_Compatibility = (55L << 32) | (0x3300 << 16) | 0x33FF, 
        //CJK_Unified_Ideographs = (59L << 32) | (0x4E00 << 16) | 0x9FFF,
        //CJK_Radicals_Supplement = (59L << 32) | (0x2E80 << 16) | 0x2EFF, 
        //Ideographic_Description_Characters = (59L << 32) | (0x2FF0 << 16) | 0x2FFF,
        //CJK_Unified_Ideographs_Extension_A = (59L << 32) | (0x3400 << 16) | 0x4DBF,
        //CJK_Unified_Ideographs_Extension_B = (59L << 32) | (0x20000 << 16) | 0x2A6DF, 
        //CJK_Strokes = (61L << 32) | (0x31C0 << 16) | 0x31EF,
        //CJK_Compatibility_Ideographs = (61L << 32) | (0xF900 << 16) | 0xFAFF,
        //CJK_Compatibility_Ideographs_Supplement = (61L << 32) | (0x2F800 << 16) | 0x2FA1F,        
        //CJK_Compatibility_Forms = (65L << 32) | (0xFE30 << 16) | 0xFE4F,

        readonly static int[] cjk_pairs = new[]
        {   0x3000,0x303F,
            0x3200,0x32FF,
            0x3300,0x33FF,
            0x4E00,0x9FFF,
            0x2E80,0x2EFF,
            0x2FF0,0x2FFF,
            0x3400,0x4DBF,
            0x20000,0x2A6DF,
            0x31C0,0x31EF,
            0xF900,0xFAFF,
            0x2F800,0x2FA1F,
            0xFE30,0xFE4F
        };

        public static bool GetUniCodeRangeFor(char c1, out int startCodePoint, out int endCodePoint, out SpanBreakInfo spanBreakInfo)
        {
            //find proper unicode range (and its lang)
            //Thai
            //TODO: review this again, with AUTOGEN code

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
            //Lao
            {
                const char s_firstChar = (char)0x0E80;
                const char s_lastChar = (char)0x0EFF;
                if (c1 >= s_firstChar && c1 <= s_lastChar)
                {
                    startCodePoint = s_firstChar;
                    endCodePoint = s_lastChar;
                    spanBreakInfo = s_lao;
                    return true;
                }
            }


            {
                //Katakana
                const char s_firstChar = (char)0x3040;
                const char s_lastChar = (char)0x30FF;
                if (c1 >= s_firstChar && c1 <= s_lastChar)
                {
                    startCodePoint = s_firstChar;
                    endCodePoint = s_lastChar;
                    spanBreakInfo = s_hana;
                    return true;
                }
                //CJK_Symbols_And_Punctuation = (48L << 32) | (0x3000 << 16) | 0x303F,
                //Hiragana = (49L << 32) | (0x3040 << 16) | 0x309F,
                //Katakana = (50L << 32) | (0x30A0 << 16) | 0x30FF,
                //Katakana_Phonetic_Extensions = (50L << 32) | (0x31F0 << 16) | 0x31FF, 
            }
            {
                //Hangul_Syllables
                const char s_firstChar = (char)0xAC00;
                const char s_lastChar = (char)0xD7AF;
                if (c1 >= s_firstChar && c1 <= s_lastChar)
                {//Hangul_Syllables = (56L << 32) | (0xAC00 << 16) | 0xD7AF,
                    startCodePoint = s_firstChar;
                    endCodePoint = s_lastChar;
                    spanBreakInfo = s_hangul;
                    return true;
                }
                else if (c1 >= 0x3130 && c1 <= 0x318F)
                {
                    //Hangul_Compatibility_Jamo = (52L << 32) | (0x3130 << 16) | 0x318F,
                    startCodePoint = 0x3130;
                    endCodePoint = 0x318F;
                    spanBreakInfo = s_hangul;
                    return true;
                }
            }

            {
                //Hangul_Compatibility_Jamo = (52L << 32) | (0x3130 << 16) | 0x318F,
                const char s_firstChar = (char)0x3130;
                const char s_lastChar = (char)0x318F;
                if (c1 >= s_firstChar && c1 <= s_lastChar)
                {
                    startCodePoint = s_firstChar;
                    endCodePoint = s_lastChar;
                    spanBreakInfo = s_hangul_jumo;
                    return true;
                }
            }
            {
                //cjk
                for (int i = 0; i < cjk_pairs.Length; i += 2)
                {
                    int s_firstChar = cjk_pairs[i];
                    int s_lastChar = cjk_pairs[i + 1];

                    if (c1 >= s_firstChar && c1 <= s_lastChar)
                    {
                        startCodePoint = s_firstChar;
                        endCodePoint = s_lastChar;
                        spanBreakInfo = s_hani;
                        return true;
                    }
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
