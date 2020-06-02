//MIT, 2016-present, WinterDev
using System.Collections.Generic;
using Typography.OpenFont;
using Typography.OpenFont.Tables;
namespace Typography.TextLayout
{


    /// <summary>
    /// glyph set position manager
    /// </summary>
    class GlyphSetPosition
    {

#if DEBUG
        readonly Typeface dbugTypeface;
#endif

        readonly GPOS _gposTable;
        internal List<GPOS.LookupTable> _lookupTables;
        public GlyphSetPosition(Typeface typeface, string lang)
        {
            this.Lang = lang;

            //check if this lang has 
            _gposTable = typeface.GPOSTable;

            if (_gposTable == null) { return; }

            ScriptTable scriptTable = _gposTable.ScriptList[lang];

            if (scriptTable == null) { return; }   // early exit if no lookup tables 

            ScriptTable.LangSysTable defaultLang = scriptTable.defaultLang;
            if (defaultLang == null) { return; }   // early exit if no default language

#if DEBUG
            dbugTypeface = typeface;
            if (defaultLang.HasRequireFeature)
            {

            }
#endif
            //other feature
            if (defaultLang.featureIndexList == null) { return; }// early exit

            //---------
            //get features 
            _lookupTables = new List<GPOS.LookupTable>();
            for (int i = 0; i < defaultLang.featureIndexList.Length; ++i)
            {
                FeatureList.FeatureTable feature = _gposTable.FeatureList.featureTables[defaultLang.featureIndexList[i]];
                bool includeThisFeature = false;
                switch (feature.TagName)
                {
                    case "mark"://mark=> mark to base
                    case "mkmk"://mkmk => mark to mask 
                                //current version we implement this 2 features
                        includeThisFeature = true;
                        break;
                    case "kern":
                        //test with Candara font
                        includeThisFeature = true;
                        //If palt is activated, there is no requirement that kern must also be activated. 
                        //If kern is activated, palt must also be activated if it exists.
                        //https://www.microsoft.com/typography/OTSpec/features_pt.htm#palt
                        break;
                    //case "palt":
                    //    break;
                    default:
                        System.Diagnostics.Debug.WriteLine("gpos_skip_tag:" + feature.TagName);
                        break;
                }

                if (includeThisFeature)
                {
                    foreach (ushort lookupIndex in feature.LookupListIndices)
                    {
                        _lookupTables.Add(_gposTable.LookupList[lookupIndex]);
                    }
                }
            }
        }
        public string Lang { get; private set; }
        public void DoGlyphPosition(IGlyphPositions glyphPositions)
        {
            //early exit if no lookup tables
            //load
            if (_lookupTables == null) { return; }
            //
            int j = _lookupTables.Count;
            for (int i = 0; i < j; ++i)
            {
                _lookupTables[i].DoGlyphPosition(glyphPositions, 0, glyphPositions.Count);
            }
        }
    }
}