//MIT, 2020-present, WinterDev
using System;
using System.Collections.Generic;
using Typography.OpenFont;

using static Typography.TextBreak.Unicode13RangeInfoList;

namespace Typography.TextBreak
{
    public static class UnicodeRangeFinder
    {

        static readonly Dictionary<UnicodeRangeInfo, SpanBreakInfo> s_registerSpanBreakInfo;

        static UnicodeRangeFinder()
        {
            //

            s_registerSpanBreakInfo = new Dictionary<UnicodeRangeInfo, SpanBreakInfo>();

            //TODO: codegen here
            RegisterSpanBreakInfo(new UnicodeRangeInfo[]
            {
                C0_Controls_and_Basic_Latin,
                C1_Controls_and_Latin_1_Supplement,
                Latin_Extended_Additional,
                Latin_Extended_A,
                Latin_Extended_B,
                Latin_Extended_C,
                Latin_Extended_D,
                Latin_Extended_E,
                Latin_Ligatures

            }, ScriptTagDefs.Latin.Tag);

            RegisterSpanBreakInfo(Thai, ScriptTagDefs.Thai.Tag);
            //
            RegisterSpanBreakInfo(Lao, ScriptTagDefs.Lao.Tag);
            //
            RegisterSpanBreakInfo(new UnicodeRangeInfo[]
            {
                Arabic, Arabic_Extended_A, Arabic_Presentation_Forms_A, Arabic_Presentation_Forms_B, Arabic_Mathematical_Alphabetic_Symbols, Arabic_Supplement

            }, ScriptTagDefs.Arabic.Tag, true);
            //
            RegisterSpanBreakInfo(new UnicodeRangeInfo[]
            {
                CJK_Compatibility_Forms,            CJK_Compatibility,
                CJK_Compatibility_Ideographs,       CJK_Compatibility_Ideographs_Supplement,
                CJK_Radicals_Supplement,            CJK_Strokes,
                CJK_Symbols_and_Punctuation,        CJK_Unified_Ideographs,
                CJK_Unified_Ideographs_Extension_A, CJK_Unified_Ideographs_Extension_B,
                CJK_Unified_Ideographs_Extension_C, CJK_Unified_Ideographs_Extension_D,
                CJK_Unified_Ideographs_Extension_E, CJK_Unified_Ideographs_Extension_F,
                CJK_Unified_Ideographs_Extension_G,

            }, ScriptTagDefs.CJK_Ideographic.Tag);
            //
            RegisterSpanBreakInfo(new UnicodeRangeInfo[]
            {
               Hiragana,Katakana,Katakana_Phonetic_Extensions

            }, ScriptTagDefs.Katakana.Tag);
            //
            RegisterSpanBreakInfo(new UnicodeRangeInfo[]
            {
               Hangul_Syllables
            }, ScriptTagDefs.Hangul.Tag);
            //
            RegisterSpanBreakInfo(new UnicodeRangeInfo[]
            {
              Hangul_Compatibility_Jamo

            }, ScriptTagDefs.Hangul_Jamo.Tag);

        }
        static void RegisterSpanBreakInfo(UnicodeRangeInfo unicodeRangeInfo, uint scriptTag, bool rightToLeft = false)
        {
            s_registerSpanBreakInfo.Add(unicodeRangeInfo, new SpanBreakInfo(unicodeRangeInfo, rightToLeft, scriptTag));
        }
        static void RegisterSpanBreakInfo(UnicodeRangeInfo[] unicodeRangeInfoArr, uint scriptTag, bool rightToLeft = false)
        {
            for (int i = 0; i < unicodeRangeInfoArr.Length; ++i)
            {
                s_registerSpanBreakInfo.Add(unicodeRangeInfoArr[i], new SpanBreakInfo(unicodeRangeInfoArr[i], rightToLeft, scriptTag));
            }
        }
        public static bool GetUniCodeRangeFor(int c1, out UnicodeRangeInfo unicodeRangeInfo, out SpanBreakInfo spanBreakInfo)
        {

            if (Unicode13RangeInfoList.TryGetUnicodeRangeInfo(c1, out unicodeRangeInfo) &&
                s_registerSpanBreakInfo.TryGetValue(unicodeRangeInfo, out spanBreakInfo))
            {
                return true;
            }
            //we may found unicodeRange info
            //but may not found register spanbreak info
            spanBreakInfo = null;
            return false;
        }
    }
}
