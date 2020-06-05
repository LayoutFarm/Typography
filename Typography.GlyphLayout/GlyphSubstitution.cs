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


    static class TagUtils
    {
        static byte GetByte(char c)
        {
            if (c >= 0 && c < 256)
            {
                return (byte)c;
            }
            return 0;
        }
        public static uint StringToTag(string str)
        {
            if (string.IsNullOrEmpty(str) || str.Length != 4)
            {
                return 0;
            }

            char[] buff = str.ToCharArray();
            byte b0 = GetByte(buff[0]);
            byte b1 = GetByte(buff[1]);
            byte b2 = GetByte(buff[2]);
            byte b3 = GetByte(buff[3]);

            return (uint)((b0 << 24) | (b1 << 16) | (b2 << 8) | b3);
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

        readonly uint _langTagCode;
        public GlyphSubstitution(Typeface typeface, string scriptTag, string langSysIden)
        {
            LangTag = langSysIden;
            ScriptTag = scriptTag;

            _typeface = typeface;
            _mustRebuildTables = true;

            _langTagCode = TagUtils.StringToTag(langSysIden);
        }

        static string TagToString(uint tag)
        {
            byte[] bytes = BitConverter.GetBytes(tag);
            Array.Reverse(bytes);
            return System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
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

        public string ScriptTag { get; }
        public string LangTag { get; }

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
            if (LangTag == null)
            {
                //use default
                selectedLang = scriptTable.defaultLang;
            }
            else
            {
                if (_langTagCode == scriptTable.defaultLang.langSysTagIden)
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
                        if (s.langSysTagIden == _langTagCode)
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
                    default:
                        {
#if DEBUG
                            System.Diagnostics.Debug.WriteLine("gsub_skip_feature_tag:" + feature.TagName);
#endif
                        }
                        break;
                    case "ccmp": // glyph composition/decomposition 
                        includeThisFeature = EnableComposition;
                        break;
                    case "liga": // Standard Ligatures --enable by default
                        includeThisFeature = EnableLigation;
                        break;

                    //--------
                    case "calt": //Contextual Alternates
                        //In specified situations, replaces default glyphs with alternate forms which provide better joining behavior.
                        //Used in script typefaces which are designed to have some or all of their glyphs join.
                        includeThisFeature = true;
                        break;
                    case "dlig":
                        // Replaces a sequence of glyphs with a single glyph which is preferred for typographic purposes.
                        //This feature covers those ligatures which may be used for special effect, at the user’s preference.
                        includeThisFeature = true;
                        break;
                    case "falt":
                        //Replaces line final glyphs with alternate forms specifically designed for this purpose(they would have less or more advance width as need may be), 
                        //to help justification of text.
                        includeThisFeature = true;
                        break;
                    case "rclt":
                        includeThisFeature = true;
                        //In specified situations, replaces default glyphs with alternate forms which provide for better joining behavior or other glyph relationships.
                        //Especially important in script typefaces which are designed to have some or all of their glyphs join,
                        //but applicable also to e.g.variants to improve spacing.
                        //This feature is similar to 'calt', 
                        //but with the difference that it should not be possible to turn off 'rclt' substitutions: they are considered essential to correct layout of the font
                        break;
                    case "rlig":
                        includeThisFeature = true;
                        //Required Ligatures
                        //Replaces a sequence of glyphs with a single glyph which is preferred for typographic purposes. 
                        //This feature covers those ligatures, which the script determines as required to be used in normal conditions. 
                        //This feature is important for some scripts to insure correct glyph formation
                        break;
                    case "locl":
                        includeThisFeature = true;
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
                    case "isol":
                        includeThisFeature = true;
                        break;
                    //--------
                    //OpenType Layout tags for math processing:
                    //https://docs.microsoft.com/en-us/typography/opentype/spec/math
                    //'math', 'ssty','flac','dtls' 	
                    case "ssty":
                        includeThisFeature = EnableMathFeature;
                        break;
                    case "dlts"://'dtls' 	Dotless Forms 
                        includeThisFeature = EnableMathFeature;
                        break;
                    case "flac": //Flattened Accents over Capitals  
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

namespace Typography.OpenFont
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

