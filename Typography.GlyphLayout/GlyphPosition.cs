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

        readonly Typeface _typeface;
        readonly GPOS? _gposTable;
        internal List<GPOS.LookupTable>? _lookupTables;
        public GlyphSetPosition(Typeface typeface, string lang)
        {
            this.Lang = lang;
            _typeface = typeface;
            //check if this lang has 
            _gposTable = typeface.GPOSTable;

            if (_gposTable == null) { return; }

            var scriptTable = _gposTable.ScriptList[lang];
            //---------
            if (scriptTable == null) { return; }   // early exit if no lookup tables
                                                   //---------

            var defaultLang = scriptTable.defaultLang;
            if (defaultLang == null) { return; }   // early exit if no default language

            if (defaultLang.HasRequireFeature)
            {

            }
            //other feature
            if (defaultLang.featureIndexList != null)
            {
                //get features 
                var features = new List<FeatureList.FeatureTable>();
                for (int i = 0; i < defaultLang.featureIndexList.Length; ++i)
                {
                    FeatureList.FeatureTable feature = _gposTable.FeatureList.featureTables[defaultLang.featureIndexList[i]];

                    switch (feature.TagName)
                    {
                        case "mark"://mark=> mark to base
                        case "mkmk"://mkmk => mark to mask 
                            //current version we implement this 2 features
                            features.Add(feature);
                            break;
                        case "kern":
                            //test with Candara font
                            features.Add(feature);
                            //If palt is activated, there is no requirement that kern must also be activated. 
                            //If kern is activated, palt must also be activated if it exists.
                            //https://www.microsoft.com/typography/OTSpec/features_pt.htm#palt
                            break;
                        case "palt":

                            break;
                        default:
                            break;
                    }

                }

                //-----------------------
                _lookupTables = new List<GPOS.LookupTable>();
                int j = features.Count;
                for (int i = 0; i < j; ++i)
                {
                    FeatureList.FeatureTable feature = features[i];
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