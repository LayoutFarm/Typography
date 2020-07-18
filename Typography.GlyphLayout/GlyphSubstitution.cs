//MIT, 2016-present, WinterDev

using System;
using System.Collections.Generic;
using Typography.OpenFont;
using Typography.OpenFont.Tables;

namespace Typography.TextLayout
{
    /// <summary>
    /// gsub lookup context
    /// </summary>
    class GSubLkContext
    {
        public readonly GSUB.LookupTable lookup;
        public GSubLkContextName ContextName;
#if DEBUG
        public string dbugFeatureName;
#endif
        public GSubLkContext(GSUB.LookupTable lookup)
        {
            this.lookup = lookup;
        }


        int _glyphCount;
        public void SetGlyphCount(int glyphCount)
        {
            _glyphCount = glyphCount;
        }
        public bool WillCheckThisGlyph(int pos)
        {
            switch (ContextName)
            {
                default: return true;
                case GSubLkContextName.Init: return _glyphCount > 1 && pos == 0; //the first one
                case GSubLkContextName.Medi: return _glyphCount > 2 && (pos > 0 && pos < _glyphCount); //in between
                case GSubLkContextName.Fina: return _glyphCount > 1 && pos == _glyphCount - 1;//the last one
            }
        }
    }

    //TODO: review here again
    enum GSubLkContextName : byte
    {
        None,

        Fina, //"fina"
        Init, //"init"
        Medi //"medi"
    }


    static class KnownLayoutTags
    {

        static readonly Dictionary<string, bool> s_knownGSubTags = new Dictionary<string, bool>();

        static KnownLayoutTags()
        {

            CollectTags(s_knownGSubTags, "ccmp");
            //arabic-related
            CollectTags(s_knownGSubTags, "liga,dlig,falt,rclt,rlig,locl,init,medi,fina,isol");
            //math-glyph related
            CollectTags(s_knownGSubTags, "math,ssty,dlts,flac");
            //indic script related
            CollectTags(s_knownGSubTags, "abvs,akhn,blwf,blws,cjct,half,haln,nukt,pres,psts,rkrf,rphf");
        }

        public static bool IsKnownGSUB_Tags(string tagName) => s_knownGSubTags.ContainsKey(tagName);

        static void CollectTags(Dictionary<string, bool> dic, string tags_str)
        {
            string[] tags = tags_str.Split(',');
            for (int i = 0; i < tags.Length; ++i)
            {
                dic[tags[i].Trim()] = true;//replace
            }
        }
    }



    /// <summary>
    /// glyph substitution manager
    /// </summary>
    class GlyphSubstitution
    {

        bool _enableLigation = true; // enable by default
        bool _enableComposition = true;
        bool _mustRebuildTables = true;
        bool _enableMathFeature = true;

        readonly Typeface _typeface;

        public GlyphSubstitution(Typeface typeface, uint scriptTag, uint langTag)
        {
            ScriptTag = scriptTag;
            LangTag = langTag;

            _typeface = typeface;
            _mustRebuildTables = true;
        }
#if DEBUG
        public string dbugScriptLang;
#endif

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
            foreach (GSubLkContext lookupCtx in _lookupTables)
            {
                GSUB.LookupTable lookupTable = lookupCtx.lookup;
                lookupCtx.SetGlyphCount(glyphIndexList.Count);

                for (int pos = 0; pos < glyphIndexList.Count; ++pos)
                {
                    if (!lookupCtx.WillCheckThisGlyph(pos))
                    {
                        continue;
                    }
                    lookupTable.DoSubstitutionAt(glyphIndexList, pos, glyphIndexList.Count - pos);
                }
            }
        }

        public uint ScriptTag { get; }
        public uint LangTag { get; }

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

        public bool EnableMathFeature
        {
            get => _enableMathFeature;
            set
            {
                if (value != _enableMathFeature)
                {
                    _mustRebuildTables = true;
                }
                _enableMathFeature = value;
            }
        }

        internal List<GSubLkContext> _lookupTables = new List<GSubLkContext>();

        internal void RebuildTables()
        {
            _lookupTables.Clear();

            // check if this lang has
            GSUB gsubTable = _typeface.GSUBTable;
            ScriptTable scriptTable = gsubTable.ScriptList[ScriptTag];
            if (scriptTable == null) return;

            //-------
            ScriptTable.LangSysTable selectedLang = null;
            if (LangTag == 0)
            {
                //use default
                selectedLang = scriptTable.defaultLang;

                if (selectedLang == null && scriptTable.langSysTables != null && scriptTable.langSysTables.Length > 0)
                {
                    //some font not defult lang
                    //so we use it from langSysTable
                    //find selected lang,
                    //if not => choose default
                    selectedLang = scriptTable.langSysTables[0];
                }
            }
            else
            {
                if (LangTag == scriptTable.defaultLang.langSysTagIden)
                {
                    //found
                    selectedLang = scriptTable.defaultLang;
                }

                if (scriptTable.langSysTables != null && scriptTable.langSysTables.Length > 0)
                {  //find selected lang,
                   //if not => choose default

                    for (int i = 0; i < scriptTable.langSysTables.Length; ++i)
                    {
                        ScriptTable.LangSysTable s = scriptTable.langSysTables[i];
                        if (s.langSysTagIden == LangTag)
                        {
                            //found
                            selectedLang = s;
                            break;
                        }
                    }
                }
            }

            //-----------------------------------
            //some lang need special management
            //TODO: review here again


#if DEBUG
            if (selectedLang == null)
            {
                //TODO:...
                throw new NotSupportedException();
            }
            if (selectedLang.HasRequireFeature)
            {
                // TODO: review here
            }
#endif

            if (selectedLang.featureIndexList == null)
            {
                return;
            }

            //(one lang may has many features)
            //Enumerate features we want and add the corresponding lookup tables
            foreach (ushort featureIndex in selectedLang.featureIndexList)
            {
                FeatureList.FeatureTable feature = gsubTable.FeatureList.featureTables[featureIndex];
                bool includeThisFeature = false;
                GSubLkContextName contextName = GSubLkContextName.None;

                switch (feature.TagName)
                {
                    case "ccmp": // glyph composition/decomposition 
                        includeThisFeature = EnableComposition;
                        break;
                    case "liga": // Standard Ligatures --enable by default
                        includeThisFeature = EnableLigation;
                        break;
                    case "init":
                        includeThisFeature = true;
                        contextName = GSubLkContextName.Init;
                        break;
                    case "medi":
                        includeThisFeature = true;
                        contextName = GSubLkContextName.Medi;
                        break;
                    case "fina":
                        //Replaces glyphs for characters that have applicable joining properties with an alternate form when occurring in a final context. 
                        //This applies to characters that have one of the following Unicode Joining_Type property 
                        includeThisFeature = true;
                        contextName = GSubLkContextName.Fina;
                        break;
                    default:
                        {
                            //other, TODO review here

                            includeThisFeature = true;
                            if (!KnownLayoutTags.IsKnownGSUB_Tags(feature.TagName))
                            {
                                includeThisFeature = false;

#if DEBUG

                                System.Diagnostics.Debug.WriteLine("gsub_skip_feature_tag:" + feature.TagName);
#endif
                            }
                            else
                            {

                            }
                        }
                        break;
                }



                if (includeThisFeature)
                {
                    foreach (ushort lookupIndex in feature.LookupListIndices)
                    {
                        var gsubcontext = new GSubLkContext(gsubTable.LookupList[lookupIndex]) { ContextName = contextName };
#if DEBUG
                        gsubcontext.dbugFeatureName = feature.TagName;
#endif

                        _lookupTables.Add(gsubcontext);
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

            foreach (GSubLkContext subLkctx in _lookupTables)
            {
                subLkctx.lookup.CollectAssociatedSubstitutionGlyph(outputGlyphIndices);
            }
            //
            //WARN :not ensure glyph unique at this stage
            //please do it in later state
        }
    }

}

namespace Typography.OpenFont.Extensions
{

    public static class TypefaceExtension5
    {
        public static void CollectAdditionalGlyphIndices(this Typeface typeface, List<ushort> outputGlyphs, ScriptLang scLang)
        {
            if (typeface.GSUBTable != null)
            {
                (new Typography.TextLayout.GlyphSubstitution(typeface, scLang.scriptTag, scLang.sysLangTag)).CollectAdditionalSubstitutionGlyphIndices(outputGlyphs);
            }
        }
    }
}

