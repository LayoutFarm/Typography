//MIT, 2016-2017, WinterDev
using System.Collections.Generic;
using Typography.OpenFont;
using Typography.OpenFont.Tables;
namespace Typography.TextLayout
{
    /// <summary>
    /// glyph subsitution manager
    /// </summary>
    class GlyphSubStitution
    {

        Typeface typeface;
        GSUB gsubTable;
        List<GSUB.LookupTable> lookupTables;
        public GlyphSubStitution(Typeface typeface, string lang)
        {
            this.EnableLigation = true;//enable by default
            this.Lang = lang;
            this.typeface = typeface;
            //check if this lang has 
            gsubTable = typeface.GSUBTable;
            ScriptTable scriptTable = gsubTable.ScriptList.FindScriptTable(lang);
            //---------
            if (scriptTable == null) { return; }   //early exit if no lookup tables      

            //---------
            ScriptTable.LangSysTable selectedLang = null;
            if (scriptTable.langSysTables != null && scriptTable.langSysTables.Length > 0)
            {
                //TODO: review here 
                selectedLang = scriptTable.langSysTables[0];
            }
            else
            {
                selectedLang = scriptTable.defaultLang;
            }

            if (selectedLang.HasRequireFeature)
            {
                //TODO: review here
            }
            //other feature
            if (selectedLang.featureIndexList != null)
            {
                //get features 
                var features = new List<FeatureList.FeatureTable>();
                for (int i = 0; i < selectedLang.featureIndexList.Length; ++i)
                {
                    FeatureList.FeatureTable feature = gsubTable.FeatureList.featureTables[selectedLang.featureIndexList[i]];
                    switch (feature.TagName)
                    {
                        case "ccmp": //glyph composition/decomposition
                                     //this version we implement ccmp
                            features.Add(feature);
                            break;
                        case "liga":
                            //Standard Ligatures --enable by default
                            features.Add(feature);
                            break;
                        default:
                            {

                            }
                            break;
                    }

                }
                //----------------------- 
                lookupTables = new List<GSUB.LookupTable>();
                int j = features.Count;
                for (int i = 0; i < j; ++i)
                {
                    FeatureList.FeatureTable feature = features[i];
                    ushort[] lookupListIndices = feature.LookupListIndice;
                    foreach (ushort lookupIndex in lookupListIndices)
                    {
                        GSUB.LookupTable lktable = gsubTable.LookupList[lookupIndex];
                        lktable.ForUseWithFeatureId = feature.TagName;
                        lookupTables.Add(gsubTable.LookupList[lookupIndex]);
                    }
                }
            }

        }
        public void DoSubstitution(IGlyphIndexList outputCodePoints)
        {
            if (lookupTables == null) { return; } //early exit if no lookup tables

            //
            //load
            int j = lookupTables.Count;
            for (int i = 0; i < j; ++i)
            {
                GSUB.LookupTable lookupTable = lookupTables[i];
                //
                if (!EnableLigation &&
                    lookupTable.ForUseWithFeatureId == Features.liga.shortname)
                {
                    //skip this feature
                    continue;
                }

                lookupTable.DoSubstitution(outputCodePoints, 0, outputCodePoints.Count);
            }


        }
        public string Lang { get; private set; }

        /// <summary>
        /// enable gsub type4, ligation
        /// </summary>
        public bool EnableLigation { get; set; }


    }


}