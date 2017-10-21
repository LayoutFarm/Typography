//MIT, 2016-2017, WinterDev
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

        Typeface typeface;
        GPOS gposTable;
        List<GPOS.LookupTable> lookupTables;
        public GlyphSetPosition(Typeface typeface, string lang)
        {
            this.Lang = lang;
            this.typeface = typeface;
            //check if this lang has 
            this.gposTable = typeface.GPOSTable;

            if (gposTable == null) { return; }

            ScriptTable scriptTable = gposTable.ScriptList.FindScriptTable(lang);
            //---------
            if (scriptTable == null) { return; }   //early exit if no lookup tables      
                                                   //---------

            ScriptTable.LangSysTable defaultLang = scriptTable.defaultLang;

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
                    FeatureList.FeatureTable feature = gposTable.FeatureList.featureTables[defaultLang.featureIndexList[i]];

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

                lookupTables = new List<GPOS.LookupTable>();
                int j = features.Count;
                for (int i = 0; i < j; ++i)
                {
                    FeatureList.FeatureTable feature = features[i];
                    ushort[] lookupListIndices = feature.LookupListIndice;
                    foreach (ushort lookupIndex in lookupListIndices)
                    {
                        lookupTables.Add(gposTable.GetLookupTable(lookupIndex));
                    }
                }
            }

        }
        public string Lang { get; private set; }
        public void DoGlyphPosition(IGlyphPositions glyphPositions)
        {
            //early exit if no lookup tables
            //load
            if (lookupTables == null) { return; }
            //
            int j = lookupTables.Count;
            for (int i = 0; i < j; ++i)
            {
                lookupTables[i].DoGlyphPosition(glyphPositions, 0, glyphPositions.Count);
            }
        }
    }
}