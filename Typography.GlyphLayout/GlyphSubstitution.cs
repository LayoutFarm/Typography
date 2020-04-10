//MIT, 2016-present, WinterDev

using System.Collections.Generic;
using Typography.OpenFont;
using Typography.OpenFont.Tables;

namespace Typography.TextLayout
{
    /// <summary>
    /// glyph substitution manager
    /// </summary>
    class GlyphSubstitution
    {
        public GlyphSubstitution(Typeface typeface, string lang)
        {
            _language = lang;
            _typeface = typeface;
            _mustRebuildTables = true;
        }

        public void DoSubstitution(IGlyphIndexList glyphIndexList)
        {
            // Rebuild tables if configuration changed
            if (_mustRebuildTables)
            {
                RebuildTables();
                _mustRebuildTables = false;
            }

            // Iterate over lookups, then over glyphs, as explained in the spec:
            // "During text processing, a client applies a lookup to each glyph
            // in the string before moving to the next lookup."
            // https://www.microsoft.com/typography/otspec/gsub.htm
            foreach (GSUB.LookupTable lookupTable in _lookupTables)
            {
                for (int pos = 0; pos < glyphIndexList.Count; ++pos)
                {
                    lookupTable.DoSubstitutionAt(glyphIndexList, pos, glyphIndexList.Count - pos);
                }
            }
        }
        public string Lang => _language;
        /// <summary>
        /// enable GSUB type 4, ligation (liga)
        /// </summary>
        public bool EnableLigation
        {
            get => _enableLigation;
            set
            {
                if (value != _enableLigation)
                {   //test change before accept value
                    _mustRebuildTables = true;
                }
                _enableLigation = value;

            }
        }

        /// <summary>
        /// enable GSUB glyph composition (ccmp)
        /// </summary>
        public bool EnableComposition
        {
            get => _enableComposition;
            set
            {
                if (value != _enableComposition)
                {
                    //test change before accept value
                    _mustRebuildTables = true;
                }
                _enableComposition = value;

            }
        }
        readonly string _language;
        bool _enableLigation = true; // enable by default
        bool _enableComposition = true;
        bool _mustRebuildTables = true;
        Typeface _typeface;


        internal List<GSUB.LookupTable> _lookupTables = new List<GSUB.LookupTable>();

        internal void RebuildTables()
        {
            _lookupTables.Clear();

            // check if this lang has
            var gsubTable = _typeface.GSUBTable;
            if (gsubTable == null) return;
            var scriptTable = gsubTable.ScriptList[_language];
            if (scriptTable == null)
            {
                //no script table for request lang-> no lookup process here
                return;
            }

            ScriptTable.LangSysTable selectedLang;
            if (scriptTable.langSysTables != null && scriptTable.langSysTables.Length > 0)
            {
                // TODO: review here
                selectedLang = scriptTable.langSysTables[0];
            }
            else if (scriptTable.defaultLang != null)
            {
                selectedLang = scriptTable.defaultLang;
            }
            else return;

            if (selectedLang.HasRequireFeature)
            {
                // TODO: review here
            }

            if (selectedLang.featureIndexList == null)
            {
                return;
            }

            //(one lang may has many features)
            //Enumerate features we want and add the corresponding lookup tables
            foreach (ushort featureIndex in selectedLang.featureIndexList)
            {
                FeatureList.FeatureTable feature = gsubTable.FeatureList.featureTables[featureIndex];
                bool featureIsNeeded = false;
                switch (feature.TagName)
                {
                    case "ccmp": // glyph composition/decomposition 
                        featureIsNeeded = EnableComposition;
                        break;
                    case "liga": // Standard Ligatures --enable by default
                        featureIsNeeded = EnableLigation;
                        break;
                }

                if (featureIsNeeded)
                {
                    foreach (ushort lookupIndex in feature.LookupListIndices)
                    {
                        _lookupTables.Add(gsubTable.LookupList[lookupIndex]);
                    }
                }
            }
        }

        /// <summary>
        /// collect all associate glyph index of specific input lang
        /// </summary>
        /// <param name="outputGlyphIndex"></param>
        public void CollectAdditionalSubstitutionGlyphIndices(List<ushort> outputGlyphIndices)
        {
            if (_mustRebuildTables)
            {
                RebuildTables();
                _mustRebuildTables = false;
            }
            //-------------
            //add some glyphs that also need by substitution process 

            foreach (GSUB.LookupTable subLk in _lookupTables)
            {
                subLk.CollectAssociatedSubstitutionGlyph(outputGlyphIndices);
            }
            //
            //WARN :not ensure glyph unique at this stage
            //please do it in later state
        }
    }


    public static class TypefaceExtensions
    {

        static UnicodeLangBits[] FilterOnlySelectedRange(UnicodeLangBits[] inputRanges, UnicodeLangBits[] userSpecificRanges)
        {
            List<UnicodeLangBits> selectedRanges = new List<UnicodeLangBits>();
            foreach (UnicodeLangBits range in inputRanges)
            {
                int foundAt = System.Array.IndexOf(userSpecificRanges, range);
                if (foundAt > 0)
                {
                    selectedRanges.Add(range);
                }
            }
            return selectedRanges.ToArray();
        }
        public static void CollectAllAssociateGlyphIndex(this Typeface typeface, List<ushort> outputGlyphIndexList, ScriptLang scLang, UnicodeLangBits[]? selectedRangs = null)
        {
            //-----------
            //general glyph index in the unicode range

            //if user dose not specific the unicode lanf bit ranges
            //the we try to select it ourself. 

            if (ScriptLangs.TryGetUnicodeLangBitsArray(scLang.shortname, out UnicodeLangBits[]? unicodeLangBitsRanges))
            {
                //one lang may contains may ranges
                if (selectedRangs != null)
                {
                    //select only in range 
                    unicodeLangBitsRanges = FilterOnlySelectedRange(unicodeLangBitsRanges, selectedRangs);
                }

                foreach (UnicodeLangBits unicodeLangBits in unicodeLangBitsRanges)
                {
                    UnicodeRangeInfo rngInfo = unicodeLangBits.ToUnicodeRangeInfo();
                    int endAt = rngInfo.EndAt;
                    for (int codePoint = rngInfo.StartAt; codePoint <= endAt; ++codePoint)
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

            //-----------
            if (typeface.GSUBTable != null)
            {
                var gsub = new GlyphSubstitution(typeface, scLang.shortname);
                gsub.CollectAdditionalSubstitutionGlyphIndices(outputGlyphIndexList);
            }
        }

    }
}


