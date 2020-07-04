//MIT, 2020, WinterDev
using System;
using System.Collections.Generic;
using Typography.OpenFont;

namespace Typography.TextBreak
{
    static class UnicodeRangeFinder
    {

        static readonly Dictionary<UnicodeRangeInfo, SpanBreakInfo> s_registerSpanBreakInfo;

        static UnicodeRangeFinder()
        {
            //

            s_registerSpanBreakInfo = new Dictionary<UnicodeRangeInfo, SpanBreakInfo>();
            SpanBreakInfo brk_thai = new SpanBreakInfo(false, ScriptTagDefs.Thai.Tag);
            SpanBreakInfo brk_lao = new SpanBreakInfo(false, ScriptTagDefs.Lao.Tag);

            SpanBreakInfo brk_arabic = new SpanBreakInfo(true, ScriptTagDefs.Arabic.Tag);
            SpanBreakInfo brk_hana = new SpanBreakInfo(false, ScriptTagDefs.Katakana.Tag);
            SpanBreakInfo brk_hangul = new SpanBreakInfo(false, ScriptTagDefs.Hangul.Tag);
            SpanBreakInfo brk_hangul_jumo = new SpanBreakInfo(false, ScriptTagDefs.Hangul_Jamo.Tag);
            SpanBreakInfo brk_hani = new SpanBreakInfo(false, ScriptTagDefs.CJK_Ideographic.Tag);


            //TODO: autogen
            s_registerSpanBreakInfo.Add(Unicode13RangeInfoList.Thai, brk_thai);
            s_registerSpanBreakInfo.Add(Unicode13RangeInfoList.Lao, brk_lao);

            s_registerSpanBreakInfo.Add(Unicode13RangeInfoList.Arabic, brk_arabic);
            s_registerSpanBreakInfo.Add(Unicode13RangeInfoList.Arabic_Extended_A, brk_arabic);
            s_registerSpanBreakInfo.Add(Unicode13RangeInfoList.Arabic_Presentation_Forms_A, brk_arabic);
            s_registerSpanBreakInfo.Add(Unicode13RangeInfoList.Arabic_Presentation_Forms_B, brk_arabic);
            s_registerSpanBreakInfo.Add(Unicode13RangeInfoList.Arabic_Mathematical_Alphabetic_Symbols, brk_arabic);
            s_registerSpanBreakInfo.Add(Unicode13RangeInfoList.Arabic_Supplement, brk_arabic);

            s_registerSpanBreakInfo.Add(Unicode13RangeInfoList.Hiragana, brk_hana);
            s_registerSpanBreakInfo.Add(Unicode13RangeInfoList.Katakana, brk_hana);
            s_registerSpanBreakInfo.Add(Unicode13RangeInfoList.Katakana_Phonetic_Extensions, brk_hana);



            s_registerSpanBreakInfo.Add(Unicode13RangeInfoList.Hangul_Syllables, brk_hangul);
            s_registerSpanBreakInfo.Add(Unicode13RangeInfoList.Hangul_Compatibility_Jamo, brk_hangul_jumo);//TODO:??


            s_registerSpanBreakInfo.Add(Unicode13RangeInfoList.CJK_Compatibility_Forms, brk_hani);
            s_registerSpanBreakInfo.Add(Unicode13RangeInfoList.CJK_Compatibility, brk_hani);
            s_registerSpanBreakInfo.Add(Unicode13RangeInfoList.CJK_Compatibility_Ideographs, brk_hani);
            s_registerSpanBreakInfo.Add(Unicode13RangeInfoList.CJK_Compatibility_Ideographs_Supplement, brk_hani);
            s_registerSpanBreakInfo.Add(Unicode13RangeInfoList.CJK_Radicals_Supplement, brk_hani);
            s_registerSpanBreakInfo.Add(Unicode13RangeInfoList.CJK_Strokes, brk_hani);
            s_registerSpanBreakInfo.Add(Unicode13RangeInfoList.CJK_Symbols_and_Punctuation, brk_hani);
            s_registerSpanBreakInfo.Add(Unicode13RangeInfoList.CJK_Unified_Ideographs, brk_hani);
            s_registerSpanBreakInfo.Add(Unicode13RangeInfoList.CJK_Unified_Ideographs_Extension_A, brk_hani);
            s_registerSpanBreakInfo.Add(Unicode13RangeInfoList.CJK_Unified_Ideographs_Extension_B, brk_hani);
            s_registerSpanBreakInfo.Add(Unicode13RangeInfoList.CJK_Unified_Ideographs_Extension_C, brk_hani);
            s_registerSpanBreakInfo.Add(Unicode13RangeInfoList.CJK_Unified_Ideographs_Extension_D, brk_hani);
            s_registerSpanBreakInfo.Add(Unicode13RangeInfoList.CJK_Unified_Ideographs_Extension_E, brk_hani);
            s_registerSpanBreakInfo.Add(Unicode13RangeInfoList.CJK_Unified_Ideographs_Extension_F, brk_hani);
            s_registerSpanBreakInfo.Add(Unicode13RangeInfoList.CJK_Unified_Ideographs_Extension_G, brk_hani);

        }
        public static bool GetUniCodeRangeFor(int c1, out UnicodeRangeInfo unicodeRangeInfo, out SpanBreakInfo spanBreakInfo)
        {

            if (Unicode13RangeInfoList.TryGetUnicodeRangeInfo(c1, out unicodeRangeInfo) &&
                s_registerSpanBreakInfo.TryGetValue(unicodeRangeInfo, out spanBreakInfo))
            {
                return true;
            }
            unicodeRangeInfo = null;
            spanBreakInfo = null;
            return false;
        }
    }
}
