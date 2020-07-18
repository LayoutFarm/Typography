//MIT, 2016-present, WinterDev 
using System;
using System.Collections.Generic;
using Typography.OpenFont.Tables; 

namespace Typography.OpenFont
{
    public static class OpenFontUnicodeUtilExtensions
    {
        //TODO: rename this class

        internal static bool DoesSupportUnicode(Languages langs, int bitpos)
        {
            if (bitpos < 32)
            {
                //use range 1
                return (langs.UnicodeRange1 & (1 << bitpos)) != 0;
            }
            else if (bitpos < 64)
            {
                return (langs.UnicodeRange2 & (1 << (bitpos - 32))) != 0;
            }
            else if (bitpos < 96)
            {
                return (langs.UnicodeRange3 & (1 << (bitpos - 64))) != 0;
            }
            else if (bitpos < 128)
            {
                return (langs.UnicodeRange4 & (1 << (bitpos - 96))) != 0;
            }
            else
            {
                throw new System.NotSupportedException();
            }
        }

        public static void CollectScriptLang(this FontCollections.InstalledTypeface typeface, Dictionary<string, ScriptLang> output)
        {
            CollectScriptLang(typeface.Languages, output);
        }
        public static void CollectScriptLang(this Languages langs, Dictionary<string, ScriptLang> output)
        {
            CollectScriptTable(langs.GSUBScriptList, output);
            CollectScriptTable(langs.GPOSScriptList, output);
        }
        static void CollectScriptTable(ScriptList scList, Dictionary<string, ScriptLang> output)
        {
            if (scList == null) { return; }
            //
            foreach (var kv in scList)
            {

                ScriptTable scTable = kv.Value;
                //default and others
                {
                    ScriptTable.LangSysTable langSys = scTable.defaultLang;
                    uint langTag = 0;
                    if (langSys != null)
                    {
                        //no lang sys
                        langTag = langSys.langSysTagIden;
                    }
                    ScriptLang sclang = new ScriptLang(scTable.scriptTag, langTag);
                    string key = sclang.ToString();
                    if (!output.ContainsKey(key))
                    {
                        output.Add(key, sclang);
                    }
                }
                //
                if (scTable.langSysTables != null && scTable.langSysTables.Length > 0)
                {
                    foreach (ScriptTable.LangSysTable langSys in scTable.langSysTables)
                    {
                        var pair = new ScriptLang(scTable.ScriptTagName, langSys.LangSysTagIdenString);
                        string key = pair.ToString();
                        if (!output.ContainsKey(key))
                        {
                            output.Add(key, pair);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// if the typeface support specific range or not
        /// </summary>
        /// <param name="previewFontInfo"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static bool DoesSupportUnicode(
               this PreviewFontInfo previewFontInfo,
               BitposAndAssciatedUnicodeRanges bitposAndAssocUnicodeRange)
        {
            return DoesSupportUnicode(previewFontInfo.Languages, bitposAndAssocUnicodeRange.Bitpos);
        }
        public static bool DoesSupportUnicode(
            this Typeface typeface,
            BitposAndAssciatedUnicodeRanges bitposAndAssocUnicodeRange)
        {
            return DoesSupportUnicode(typeface.Languages, bitposAndAssocUnicodeRange.Bitpos);
        }

        static UnicodeRangeInfo[] FilterOnlySelectedRange(UnicodeRangeInfo[] inputRanges, UnicodeRangeInfo[] userSpecificRanges)
        {
            List<UnicodeRangeInfo> selectedRanges = new List<UnicodeRangeInfo>();
            foreach (UnicodeRangeInfo range in inputRanges)
            {
                int foundAt = System.Array.IndexOf(userSpecificRanges, range);
                if (foundAt > 0)
                {
                    selectedRanges.Add(range);
                }
            }
            return selectedRanges.ToArray();
        }

        public static void CollectAllAssociateGlyphIndex(this Typeface typeface, List<ushort> outputGlyphIndexList, ScriptLang scLang, UnicodeRangeInfo[] selectedRangs = null)
        {
            //-----------
            //general glyph index in the unicode range

            //if user dose not specific the unicode lanf bit ranges
            //the we try to select it ourself. 

            if (ScriptLangs.TryGetUnicodeLangRangesArray(scLang.GetScriptTagString(), out UnicodeRangeInfo[] unicodeLangRange))
            {
                //one lang may contains may ranges
                if (selectedRangs != null)
                {
                    //select only in range 
                    unicodeLangRange = FilterOnlySelectedRange(unicodeLangRange, selectedRangs);
                }

                foreach (UnicodeRangeInfo rng in unicodeLangRange)
                {
                    for (int codePoint = rng.StartCodepoint; codePoint <= rng.EndCodepoint; ++codePoint)
                    {
                        ushort glyphIndex = typeface.GetGlyphIndex(codePoint);
                        if (glyphIndex > 0)
                        {
                            //add this glyph index
                            outputGlyphIndexList.Add(glyphIndex);
                        }
                    }
                }
            }

            typeface.CollectAdditionalGlyphIndices(outputGlyphIndexList, scLang);
        }
    }
}