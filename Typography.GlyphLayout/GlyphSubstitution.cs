//MIT, 2016-2017, WinterDev

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

        public void DoSubstitution(IGlyphIndexList codePoints)
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
                for (int pos = 0; pos < codePoints.Count; ++pos)
                {
                    lookupTable.DoSubstitutionAt(codePoints, pos, codePoints.Count - pos);
                }
            }
        }
        public string Lang
        {
            get { return _language; }
        }

        /// <summary>
        /// enable GSUB type 4, ligation (liga)
        /// </summary>
        public bool EnableLigation
        {
            get { return _enableLigation; }
            set
            {
                _mustRebuildTables = value != _enableLigation;//test change before accept value
                _enableLigation = value;
            }
        }

        /// <summary>
        /// enable GSUB glyph composition (ccmp)
        /// </summary>
        public bool EnableComposition
        {
            get { return _enableComposition; }
            set
            {
                _mustRebuildTables = value != _enableComposition;//test change before accept value
                _enableComposition = value;
            }
        }

        private readonly string _language;
        private bool _enableLigation = true; // enable by default
        private bool _enableComposition = true;

        private bool _mustRebuildTables = true;

        private Typeface _typeface;
        private List<GSUB.LookupTable> _lookupTables = new List<GSUB.LookupTable>();

        private void RebuildTables()
        {
            _lookupTables.Clear();

            // check if this lang has
            GSUB gsubTable = _typeface.GSUBTable;
            ScriptTable scriptTable = gsubTable.ScriptList[_language];
            if (scriptTable == null)
            {
                //no script table for request lang-> no lookup process here
                return;
            }

            ScriptTable.LangSysTable selectedLang = null;
            if (scriptTable.langSysTables != null && scriptTable.langSysTables.Length > 0)
            {
                // TODO: review here
                selectedLang = scriptTable.langSysTables[0];
            }
            else
            {
                selectedLang = scriptTable.defaultLang;
            }

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
    }
}
