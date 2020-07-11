//MIT, 2016-present, WinterDev 
using System;
using System.Collections.Generic;
using System.IO;
using Typography.OpenFont;
using Typography.OpenFont.Tables;
using Typography.TextBreak;

namespace Typography.OpenFont
{
    public static class TypefaceExtension3
    {

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

        public static void CollectScriptLang(this FontManagement.InstalledTypeface typeface, Dictionary<string, ScriptLang> output)
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
                    for (int codePoint = rng.StarCodepoint; codePoint <= rng.EndCodepoint; ++codePoint)
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


namespace Typography.FontManagement
{

    public abstract class AlternativeTypefaceSelector
    {

#if DEBUG
        public AlternativeTypefaceSelector() { }
#endif

        public Typeface LatestTypeface { get; set; }
        public abstract SelectedTypeface Select(List<InstalledTypeface> choices, UnicodeRangeInfo unicodeRangeInfo, int codepoint);



        public readonly struct SelectedTypeface
        {
            public readonly InstalledTypeface InstalledTypeface;
            public readonly Typeface Typeface;
            public SelectedTypeface(InstalledTypeface installedTypeface)
            {
                Typeface = null;
                InstalledTypeface = installedTypeface;
            }
            public SelectedTypeface(Typeface typeface)
            {
                Typeface = typeface;
                InstalledTypeface = null;
            }
            public bool IsEmpty() => Typeface == null && InstalledTypeface == null;
        }
    }

    public class PreferredTypeface
    {
        public PreferredTypeface(string reqTypefaceName) => RequestTypefaceName = reqTypefaceName;
        public string RequestTypefaceName { get; }
        public InstalledTypeface InstalledTypeface { get; set; }
        public bool ResolvedInstalledTypeface { get; set; }
    }
    public class PreferredTypefaceList : List<PreferredTypeface>
    {
#if DEBUG
        //TODO: review this again
        public PreferredTypefaceList() { }
#endif
        public void AddTypefaceName(string typefaceName)
        {
            this.Add(new PreferredTypeface(typefaceName));
        }
    }

    public class InstalledTypeface
    {

        internal InstalledTypeface(Typeface typeface, TypefaceStyle style, string fontPath)
        {

            FontName = typeface.Name;
            FontSubFamily = typeface.FontSubFamily;

            NameEntry nameEntry = typeface.GetNameEntry();
            OS2Table os2Table = typeface.GetOS2Table();

            TypographicFamilyName = nameEntry.TypographicFamilyName;
            TypographicFontSubFamily = nameEntry.TypographyicSubfamilyName;

            FontPath = fontPath;

            PostScriptName = typeface.PostScriptName;
            UniqueFontIden = typeface.UniqueFontIden;
#if DEBUG

            if (string.IsNullOrEmpty(UniqueFontIden))
            {

            }
#endif

            Languages = typeface.Languages;
            TypefaceStyle = style;
        }
        internal InstalledTypeface(PreviewFontInfo previewFontInfo, TypefaceStyle style, string fontPath)
        {
            FontName = previewFontInfo.Name;
            FontSubFamily = previewFontInfo.SubFamilyName;
            TypographicFamilyName = previewFontInfo.TypographicFamilyName;
            TypographicFontSubFamily = previewFontInfo.TypographicSubFamilyName;
            Weight = previewFontInfo.Weight;
            FontPath = fontPath;

            PostScriptName = previewFontInfo.PostScriptName;
            UniqueFontIden = previewFontInfo.UniqueFontIden;

#if DEBUG

            if (string.IsNullOrEmpty(UniqueFontIden))
            {

            }
#endif

            Languages = previewFontInfo.Languages;
            TypefaceStyle = style;

        }

        public string FontName { get; internal set; }
        public string FontSubFamily { get; internal set; }
        public string TypographicFamilyName { get; internal set; }
        public string TypographicFontSubFamily { get; internal set; }
        public string PostScriptName { get; internal set; }
        public string UniqueFontIden { get; internal set; }

        public TypefaceStyle TypefaceStyle { get; internal set; }
        public ushort Weight { get; internal set; }
        public Languages Languages { get; }
        public string FontPath { get; internal set; }
        public int ActualStreamOffset { get; internal set; }

        //TODO: UnicodeLangBits vs UnicodeLangBits5_1
        public bool DoesSupportUnicode(BitposAndAssciatedUnicodeRanges bitposAndAssocUnicode) => TypefaceExtension3.DoesSupportUnicode(Languages, bitposAndAssocUnicode.Bitpos);
        public bool DoesSupportUnicode(int bitpos) => TypefaceExtension3.DoesSupportUnicode(Languages, bitpos);

        /// <summary>
        /// check if this font has glyph for the given code point or not
        /// </summary>
        /// <returns></returns>
        public bool ContainGlyphForUnicode(int codepoint) => Languages.ContainGlyphForUnicode(codepoint);

        internal Typeface ResolvedTypeface;

#if DEBUG
        public override string ToString()
        {
            return FontName + " " + FontSubFamily;
        }
#endif
    }
    [Flags]
    public enum TypefaceStyle
    {
        Others = 0,
        Regular = 1,
        Bold = 1 << 2,
        Italic = 1 << 3,
    }

    public interface IFontStreamSource
    {
        Stream ReadFontStream();
        string PathName { get; }
    }

    public class FontFileStreamProvider : IFontStreamSource
    {
        public FontFileStreamProvider(string filename)
        {
            this.PathName = filename;
        }
        public string PathName { get; private set; }
        public Stream ReadFontStream()
        {
            //TODO: don't forget to dispose this stream when not use
            return new FileStream(this.PathName, FileMode.Open, FileAccess.Read);
        }
    }

    public delegate void FirstInitFontCollectionDelegate(InstalledTypefaceCollection typefaceCollection);
    public delegate InstalledTypeface FontNotFoundHandler(InstalledTypefaceCollection typefaceCollection, string fontName, string fontSubFam);
    public delegate FontNameDuplicatedDecision FontNameDuplicatedHandler(InstalledTypeface existing, InstalledTypeface newAddedFont);
    public enum FontNameDuplicatedDecision
    {
        /// <summary>
        /// use existing, skip latest font
        /// </summary>
        Skip,
        /// <summary>
        /// replace with existing with the new one
        /// </summary>
        Replace
    }


    public interface IInstalledTypefaceProvider
    {
        InstalledTypeface GetInstalledTypeface(string fontName, TypefaceStyle style);
    }

    public class InstalledTypefaceCollection : IInstalledTypefaceProvider
    {
        class InstalledTypefaceGroup
        {
            public Dictionary<string, InstalledTypeface> _members = new Dictionary<string, InstalledTypeface>();

            public void AddFont(string registerName, InstalledTypeface installedFont)
            {
                _members.Add(registerName, installedFont);
            }
            public bool TryGetValue(string registerName, out InstalledTypeface found)
            {
                return _members.TryGetValue(registerName, out found);
            }
            public void Replace(string registerName, InstalledTypeface newone)
            {
                _members[registerName] = newone;
            }

#if DEBUG
            public string dbugGroupName;
            public override string ToString()
            {
                return dbugGroupName;
            }
#endif

        }

        /// <summary>
        /// map from font subfam to internal group name
        /// </summary>
        readonly Dictionary<string, InstalledTypefaceGroup> _subFamToFontGroup = new Dictionary<string, InstalledTypefaceGroup>();
        readonly Dictionary<string, bool> _onlyFontNames = new Dictionary<string, bool>();


        readonly InstalledTypefaceGroup _regular, _bold, _italic, _bold_italic;
        readonly List<InstalledTypefaceGroup> _allGroups = new List<InstalledTypefaceGroup>();

        readonly Dictionary<string, InstalledTypeface> _otherFontNames = new Dictionary<string, InstalledTypeface>();
        readonly Dictionary<string, InstalledTypeface> _postScriptNames = new Dictionary<string, InstalledTypeface>();

        FontNameDuplicatedHandler _fontNameDuplicatedHandler;
        FontNotFoundHandler _fontNotFoundHandler;

        public InstalledTypefaceCollection()
        {

            //-----------------------------------------------------
            //init wellknown subfam 
            _regular = CreateCreateNewGroup(TypefaceStyle.Regular, "regular", "normal");
            _italic = CreateCreateNewGroup(TypefaceStyle.Italic, "Italic", "italique");
            //
            _bold = CreateCreateNewGroup(TypefaceStyle.Bold, "bold");
            //
            _bold_italic = CreateCreateNewGroup(TypefaceStyle.Bold | TypefaceStyle.Italic, "bold italic");
            //
        }
        public void SetFontNameDuplicatedHandler(FontNameDuplicatedHandler handler)
        {
            _fontNameDuplicatedHandler = handler;
        }
        public void SetFontNotFoundHandler(FontNotFoundHandler fontNotFoundHandler)
        {
            _fontNotFoundHandler = fontNotFoundHandler;
        }

        static InstalledTypefaceCollection s_intalledTypefaces;
        public static InstalledTypefaceCollection GetSharedTypefaceCollection(FirstInitFontCollectionDelegate initdel)
        {
            if (s_intalledTypefaces == null)
            {
                //first time
                s_intalledTypefaces = new InstalledTypefaceCollection();
                initdel(s_intalledTypefaces);
            }
            return s_intalledTypefaces;
        }
        public static void SetAsSharedTypefaceCollection(InstalledTypefaceCollection installedTypefaceCollection)
        {
            s_intalledTypefaces = installedTypefaceCollection;
        }
        public static InstalledTypefaceCollection GetSharedTypefaceCollection()
        {
            return s_intalledTypefaces;
        }
        InstalledTypefaceGroup CreateCreateNewGroup(TypefaceStyle installedFontStyle, params string[] names)
        {
            //create font group
            var fontGroup = new InstalledTypefaceGroup();
            //single dic may be called by many names            
            foreach (string name in names)
            {
                string upperCaseName = name.ToUpper();
                //register name
                //should not duplicate 
                _subFamToFontGroup.Add(upperCaseName, fontGroup);
            }
            _allGroups.Add(fontGroup);
            return fontGroup;
        }

        public InstalledTypeface AddFontPreview(PreviewFontInfo previewFont, string srcPath)
        {
            //replace?
            if (_onlyFontNames.ContainsKey(previewFont.Name))
            {
                //duplicated??
                //TODO: handle this
                //eg. Asana.ttc includes Asana-Regular.ttf and Asana-Math.ttf
                //and we may already have a Asana-math.ttf
                //How to handle this...
#if DEBUG
                System.Diagnostics.Debug.WriteLine("Duplicated Typeface: ");//TODO:

#endif
            }

            _onlyFontNames[previewFont.Name] = true;

            TypefaceStyle typefaceStyle = TypefaceStyle.Others; //init
            switch (previewFont.OS2TranslatedStyle)
            {
                case OpenFont.Extensions.TranslatedOS2FontStyle.BOLD:
                    typefaceStyle = TypefaceStyle.Bold;
                    break;
                case OpenFont.Extensions.TranslatedOS2FontStyle.ITALIC:
                case OpenFont.Extensions.TranslatedOS2FontStyle.OBLIQUE:
                    typefaceStyle = TypefaceStyle.Italic;
                    break;
                case OpenFont.Extensions.TranslatedOS2FontStyle.REGULAR:
                    typefaceStyle = TypefaceStyle.Regular;
                    break;
                case (OpenFont.Extensions.TranslatedOS2FontStyle.BOLD | OpenFont.Extensions.TranslatedOS2FontStyle.ITALIC):
                    typefaceStyle = TypefaceStyle.Bold | TypefaceStyle.Italic;
                    break;
            }
            //---------------
            //some font subfam="Bold Italic" but OS2TranslatedStyle is only Italic
            //so we should check the subfam name too!
            string[] fontSubFamUpperCaseName_split = previewFont.SubFamilyName.ToUpper().Split(' ');
            if (fontSubFamUpperCaseName_split.Length > 1)
            {
                if (typefaceStyle != (TypefaceStyle.Bold | TypefaceStyle.Italic))
                {
                    //translate more
                    if ((fontSubFamUpperCaseName_split[0] == "BOLD" && fontSubFamUpperCaseName_split[1] == "ITALIC") ||
                        (fontSubFamUpperCaseName_split[0] == "ITALIC" && fontSubFamUpperCaseName_split[1] == "BOLD"))
                    {
                        typefaceStyle = TypefaceStyle.Bold | TypefaceStyle.Italic;
                    }
                }
            }
            else
            {
                //=1
                switch (fontSubFamUpperCaseName_split[0])
                {
                    case "REGULAR": typefaceStyle = TypefaceStyle.Regular; break;
                    case "BOLD": typefaceStyle = TypefaceStyle.Bold; break;
                    case "ITALIC": typefaceStyle = TypefaceStyle.Italic; break;
                    case "COLOR": typefaceStyle = TypefaceStyle.Others; break;
                    case "LIGHT": typefaceStyle = TypefaceStyle.Others; break;
                }
            }

#if DEBUG
            if (typefaceStyle == TypefaceStyle.Others)
            {

            }
#endif

            InstalledTypeface installedTypeface = new InstalledTypeface(
                previewFont,
                typefaceStyle,
                srcPath)
            { ActualStreamOffset = previewFont.ActualStreamOffset };


            return Register(installedTypeface) ? installedTypeface : null;
        }
        public bool AddFontStreamSource(IFontStreamSource src)
        {
            //preview data of font
            try
            {
                using (Stream stream = src.ReadFontStream())
                {
                    var reader = new OpenFontReader();
                    PreviewFontInfo previewFont = reader.ReadPreview(stream);
                    if (previewFont == null || string.IsNullOrEmpty(previewFont.Name))
                    {
                        //err!
                        return false;
                    }
                    if (previewFont.IsFontCollection)
                    {
                        int mbCount = previewFont.MemberCount;
                        bool totalResult = true;
                        for (int i = 0; i < mbCount; ++i)
                        {
                            //extract and each members
                            InstalledTypeface instTypeface = AddFontPreview(previewFont.GetMember(i), src.PathName);
                            if (instTypeface == null)
                            {
                                totalResult = false;
                            }

                        }
                        return totalResult;
                    }
                    else
                    {
                        return AddFontPreview(previewFont, src.PathName) != null;
                    }

                }
            }
            catch (IOException)
            {
                //TODO review here again
                return false;
            }
        }


        internal Dictionary<string, InstalledTypeface> _installedTypefacesByFilenames = new Dictionary<string, InstalledTypeface>();
        bool Register(InstalledTypeface instTypeface)
        {
            InstalledTypefaceGroup selectedFontGroup = null;
            string fontSubFamUpperCaseName = instTypeface.TypographicFontSubFamily;
            bool use_typographicSubFam = true;
            if (fontSubFamUpperCaseName == null)
            {
                //switch to FontSubFamily, this should not be null!
                fontSubFamUpperCaseName = instTypeface.FontSubFamily;
                use_typographicSubFam = false;
            }
            fontSubFamUpperCaseName = fontSubFamUpperCaseName.ToUpper();
            //--------------

            switch (instTypeface.TypefaceStyle)
            {
                default:
                    {

                        if (!_subFamToFontGroup.TryGetValue(fontSubFamUpperCaseName, out selectedFontGroup))
                        {
                            //create new group, we don't known this font group before 
                            //so we add to 'other group' list
                            selectedFontGroup = new InstalledTypefaceGroup();
#if DEBUG
                            selectedFontGroup.dbugGroupName = fontSubFamUpperCaseName;
#endif
                            _subFamToFontGroup.Add(fontSubFamUpperCaseName, selectedFontGroup);
                            _allGroups.Add(selectedFontGroup);

                        }
                    }
                    break;
                case TypefaceStyle.Bold:
                    selectedFontGroup = _bold;
                    break;
                case TypefaceStyle.Italic:
                    selectedFontGroup = _italic;
                    break;
                case TypefaceStyle.Regular:
                    {
                        selectedFontGroup = _regular;

                        if (fontSubFamUpperCaseName != "REGULAR" &&
                            !_subFamToFontGroup.TryGetValue(fontSubFamUpperCaseName, out selectedFontGroup))
                        {
                            //create new group, we don't known this font group before 
                            //so we add to 'other group' list
                            selectedFontGroup = new InstalledTypefaceGroup();
#if DEBUG
                            selectedFontGroup.dbugGroupName = fontSubFamUpperCaseName;
#endif
                            _subFamToFontGroup.Add(fontSubFamUpperCaseName, selectedFontGroup);
                            _allGroups.Add(selectedFontGroup);
                        }

                    }
                    break;
                case (TypefaceStyle.Bold | TypefaceStyle.Italic):
                    selectedFontGroup = _bold_italic;
                    break;
            }

            //------------------
            //for font management
            //we use 'typographic family name' if avaliable,            
            string register_name = instTypeface.TypographicFamilyName;
            bool use_typographicFontFam = true;
            if (register_name == null)
            {
                //switch to font name, this should not be null!
                register_name = instTypeface.FontName;
                use_typographicFontFam = false;
            }

            register_name = register_name.ToUpper(); //***  
            bool register_result = false;

            if (selectedFontGroup.TryGetValue(register_name, out InstalledTypeface found))
            {
                //TODO:
                //we already have this font name
                //(but may be different file
                //we let user to handle it        
                if (_fontNameDuplicatedHandler != null)
                {
                    switch (_fontNameDuplicatedHandler(found, instTypeface))
                    {
                        default:
                            throw new NotSupportedException();
                        case FontNameDuplicatedDecision.Skip:
                            break;
                        case FontNameDuplicatedDecision.Replace:
                            selectedFontGroup.Replace(register_name, instTypeface);
                            register_result = true;
                            break;
                    }
                }
            }
            else
            {
                selectedFontGroup.AddFont(register_name, instTypeface);
                register_result = true;
            }

            if (use_typographicFontFam &&
                instTypeface.FontName != instTypeface.TypographicFamilyName &&
                instTypeface.TypefaceStyle == TypefaceStyle.Regular)
            {
                //in this case, the code above register the typeface with TypographicFamilyName
                //so we register this typeface with original name too
                if (_otherFontNames.ContainsKey(instTypeface.FontName.ToUpper()))
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("duplicated font name?:" + instTypeface.FontName.ToUpper());
#endif
                }
                else
                {
                    _otherFontNames.Add(instTypeface.FontName.ToUpper(), instTypeface);
                }
            }

            //register font
            if (instTypeface.PostScriptName != null)
            {
                string postScriptName = instTypeface.PostScriptName.ToUpper();
                if (!_postScriptNames.ContainsKey(postScriptName))
                {
                    _postScriptNames.Add(postScriptName, instTypeface);
                }
                else
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("duplicated postscriptname:" + postScriptName);
#endif
                }
            }

            if (register_result && instTypeface.FontPath != null &&
                !_installedTypefacesByFilenames.ContainsKey(instTypeface.FontPath)) //beware case-sensitive!
            {
                _installedTypefacesByFilenames.Add(instTypeface.FontPath, instTypeface);
            }
            return register_result;
        }

        public InstalledTypeface GetFontByPostScriptName(string postScriptName)
        {
            _postScriptNames.TryGetValue(postScriptName.ToUpper(), out InstalledTypeface found);
            return found;
        }
        public InstalledTypeface GetInstalledTypeface(string fontName, string subFamName)
        {
            string upperCaseFontName = fontName.ToUpper();
            string upperCaseSubFamName = subFamName.ToUpper();


            //find font group  
            if (_subFamToFontGroup.TryGetValue(upperCaseSubFamName, out InstalledTypefaceGroup foundFontGroup) &&
                foundFontGroup.TryGetValue(upperCaseFontName, out InstalledTypeface foundInstalledFont))
            {
                return foundInstalledFont;
            }

            //
            if (_otherFontNames.TryGetValue(upperCaseFontName, out foundInstalledFont))
            {
                return foundInstalledFont;
            }
            if (upperCaseSubFamName == "")
            {
                //eg OTHERS...
                foreach (var kv in _subFamToFontGroup)
                {
                    switch (kv.Key)
                    {
                        case "BOLD ITALIC":
                        case "BOLD":
                        case "ITALIC":
                        case "ITALIQUE":
                        case "REGULAR": continue;
                        default:
                            {
                                if (kv.Value.TryGetValue(upperCaseFontName, out foundInstalledFont))
                                {
                                    return foundInstalledFont;
                                }
                            }
                            break;
                    }
                }

            }


            //not found
            if (_fontNotFoundHandler != null)
            {
                return _fontNotFoundHandler(this, fontName, subFamName);
            }

            return null; //not found
        }

        public InstalledTypeface GetInstalledTypeface(string fontName, TypefaceStyle wellknownSubFam)
        {
            //not auto resolve
            InstalledTypefaceGroup selectedFontGroup;
            InstalledTypeface _found;
            switch (wellknownSubFam)
            {
                default: return null;
                case TypefaceStyle.Regular: selectedFontGroup = _regular; break;
                case TypefaceStyle.Bold: selectedFontGroup = _bold; break;
                case TypefaceStyle.Italic: selectedFontGroup = _italic; break;
                case (TypefaceStyle.Bold | TypefaceStyle.Italic): selectedFontGroup = _bold_italic; break;
            }
            if (selectedFontGroup.TryGetValue(fontName.ToUpper(), out _found))
            {
                return _found;
            }
            //------------------------------------------- 
            //not found then ...


            //retry ....
            //if (wellknownSubFam == TypefaceStyle.Bold)
            //{
            //    //try get from Gras?
            //    //eg. tahoma
            //    if (_subFamToFontGroup.TryGetValue("GRAS", out selectedFontGroup))
            //    {

            //        if (selectedFontGroup.TryGetValue(fontName.ToUpper(), out _found))
            //        {
            //            return _found;
            //        }

            //    }
            //}
            //else if (wellknownSubFam == TypefaceStyle.Italic)
            //{
            //    //TODO: simulate oblique (italic) font???
            //    selectedFontGroup = _normal;

            //    if (selectedFontGroup.TryGetValue(fontName.ToUpper(), out _found))
            //    {
            //        return _found;
            //    }
            //}

            if (_found == null && _fontNotFoundHandler != null)
            {
                return _fontNotFoundHandler(this, fontName, GetSubFam(wellknownSubFam));
            }
            return _found;
        }

        internal static string GetSubFam(TypefaceStyle typefaceStyle)
        {
            switch (typefaceStyle)
            {
                case TypefaceStyle.Bold: return "BOLD";
                case TypefaceStyle.Italic: return "ITALIC";
                case TypefaceStyle.Regular: return "REGULAR";
                case TypefaceStyle.Bold | TypefaceStyle.Italic: return "BOLD ITALIC";
            }
            return "";
        }
        internal static TypefaceStyle GetWellknownFontStyle(string subFamName)
        {
            switch (subFamName.ToUpper())
            {
                default: return TypefaceStyle.Others;
                case "NORMAL": //normal weight?
                case "REGULAR":
                    return TypefaceStyle.Regular;
                case "BOLD":
                    return TypefaceStyle.Bold;
                case "ITALIC":
                case "ITALIQUE":
                    return TypefaceStyle.Italic;
                case "BOLD ITALIC":
                    return (TypefaceStyle.Bold | TypefaceStyle.Italic);
            }
        }

        public IEnumerable<InstalledTypeface> GetInstalledFontIter()
        {
            foreach (InstalledTypefaceGroup fontgroup in _allGroups)
            {
                foreach (InstalledTypeface f in fontgroup._members.Values)
                {
                    yield return f;
                }
            }
        }


        public IEnumerable<string> GetFontNameIter() => _onlyFontNames.Keys;
        public IEnumerable<InstalledTypeface> GetInstalledTypefaceIter(string fontName)
        {
            fontName = fontName.ToUpper();
            foreach (InstalledTypefaceGroup typefaceGroup in _subFamToFontGroup.Values)
            {
                if (typefaceGroup.TryGetValue(fontName, out InstalledTypeface found))
                {
                    yield return found;
                }
            }
        }

        readonly Dictionary<UnicodeRangeInfo, List<InstalledTypeface>> _registerWithUnicodeRangeDic = new Dictionary<UnicodeRangeInfo, List<InstalledTypeface>>();
        readonly List<InstalledTypeface> _emojiSupportedTypefaces = new List<InstalledTypeface>();
        readonly List<InstalledTypeface> _mathTypefaces = new List<InstalledTypeface>();

        //unicode 13:
        //https://unicode.org/emoji/charts/full-emoji-list.html
        //emoji start at U+1F600 	
        const int UNICODE_EMOJI_START = 0x1F600; //"😁" //first emoji
        const int UNICODE_EMOJI_END = 0x1F64F;

        //https://www.unicode.org/charts/PDF/U1D400.pdf
        const int UNICODE_MATH_ALPHANUM_EXAMPLE = 0x1D400; //1D400–1D7FF;




        List<InstalledTypeface> GetExisitingOrCreateNewListForUnicodeRange(UnicodeRangeInfo range)
        {
            if (!_registerWithUnicodeRangeDic.TryGetValue(range, out List<InstalledTypeface> found))
            {
                found = new List<InstalledTypeface>();
                _registerWithUnicodeRangeDic.Add(range, found);
            }
            return found;
        }
        public void UpdateUnicodeRanges()
        {

            _registerWithUnicodeRangeDic.Clear();
            _emojiSupportedTypefaces.Clear();
            _mathTypefaces.Clear();

            foreach (InstalledTypeface instFont in GetInstalledFontIter())
            {
                foreach (BitposAndAssciatedUnicodeRanges bitposAndAssocUnicodeRanges in instFont.GetSupportedUnicodeLangIter())
                {
                    foreach (UnicodeRangeInfo range in bitposAndAssocUnicodeRanges.Ranges)
                    {

                        List<InstalledTypeface> typefaceList = GetExisitingOrCreateNewListForUnicodeRange(range);
                        typefaceList.Add(instFont);
                        //----------------
                        //sub range
                        if (range == BitposAndAssciatedUnicodeRanges.None_Plane_0)
                        {
                            //special search
                            //TODO: review here again
                            foreach (UnicodeRangeInfo rng in Unicode13RangeInfoList.GetNonePlane0Iter())
                            {
                                if (instFont.ContainGlyphForUnicode(rng.StarCodepoint))
                                {
                                    typefaceList = GetExisitingOrCreateNewListForUnicodeRange(rng);
                                    typefaceList.Add(instFont);
                                }
                            }
                            if (instFont.ContainGlyphForUnicode(UNICODE_EMOJI_START))
                            {
                                _emojiSupportedTypefaces.Add(instFont);
                            }
                            if (instFont.ContainGlyphForUnicode(UNICODE_MATH_ALPHANUM_EXAMPLE))
                            {
                                _mathTypefaces.Add(instFont);
                            }
                        }
                    }
                }
            }
            //------
            //select perfer unicode font

        }


        /// <summary>
        /// get alternative typeface from a given unicode codepoint
        /// </summary>
        /// <param name="codepoint"></param>
        /// <param name="selector"></param>
        /// <param name="found"></param>
        /// <returns></returns>
        public bool TryGetAlternativeTypefaceFromCodepoint(int codepoint, AlternativeTypefaceSelector selector, out Typeface selectedTypeface)
        {
            //find a typeface that supported input char c

            List<InstalledTypeface> installedTypefaceList = null;
            if (ScriptLangs.TryGetUnicodeRangeInfo(codepoint, out UnicodeRangeInfo unicodeRangeInfo))
            {
                if (_registerWithUnicodeRangeDic.TryGetValue(unicodeRangeInfo, out List<InstalledTypeface> typefaceList) &&
                    typefaceList.Count > 0)
                {
                    //select a proper typeface                        
                    installedTypefaceList = typefaceList;
                }
            }


            //not found
            if (installedTypefaceList == null && codepoint >= UNICODE_EMOJI_START && codepoint <= UNICODE_EMOJI_END)
            {
                unicodeRangeInfo = Unicode13RangeInfoList.Emoticons;
                if (_emojiSupportedTypefaces.Count > 0)
                {
                    installedTypefaceList = _emojiSupportedTypefaces;
                }
            }
            //-------------
            if (installedTypefaceList != null)
            {
                //select a prefer font 
                if (selector != null)
                {
                    AlternativeTypefaceSelector.SelectedTypeface result = selector.Select(installedTypefaceList, unicodeRangeInfo, codepoint);
                    if (result.InstalledTypeface != null)
                    {
                        selectedTypeface = this.ResolveTypeface(result.InstalledTypeface);
                        return selectedTypeface != null;
                    }
                    else if (result.Typeface != null)
                    {
                        selectedTypeface = result.Typeface;
                        return true;
                    }
                    else
                    {
                        selectedTypeface = null;
                        return false;
                    }
                }
                else if (installedTypefaceList.Count > 0)
                {
                    InstalledTypeface instTypeface = installedTypefaceList[0];//default
                    selectedTypeface = this.ResolveTypeface(installedTypefaceList[0]);
                    return selectedTypeface != null;
                }
            }

            selectedTypeface = null;
            return false;
        }
    }

    public readonly struct TinyCRC32Calculator
    {

        /// <summary>
        /// Update the value for the running CRC32 using the given block of bytes.
        /// This is useful when using the CRC32() class in a Stream.
        /// </summary>
        /// <param name="block">block of bytes to slurp</param>
        /// <param name="offset">starting point in the block</param>
        /// <param name="count">how many bytes within the block to slurp</param>
        static int SlurpBlock(byte[] block, int offset, int count)
        {
            if (block == null)
            {
                throw new NotSupportedException("The data buffer must not be null.");
            }

            // UInt32 tmpRunningCRC32Result = _RunningCrc32Result;

            uint _runningCrc32Result = 0xFFFFFFFF;
            for (int i = 0; i < count; i++)
            {
#if DEBUG
                int x = offset + i;
#endif
                //_runningCrc32Result = ((_runningCrc32Result) >> 8) ^ s_crc32Table[(block[x]) ^ ((_runningCrc32Result) & 0x000000FF)];
                _runningCrc32Result = ((_runningCrc32Result) >> 8) ^ s_crc32Table[(block[offset + i]) ^ ((_runningCrc32Result) & 0x000000FF)];
                //tmpRunningCRC32Result = ((tmpRunningCRC32Result) >> 8) ^ crc32Table[(block[offset + i]) ^ ((tmpRunningCRC32Result) & 0x000000FF)];
            }
            return unchecked((Int32)(~_runningCrc32Result));
        }


        // pre-initialize the crc table for speed of lookup.
        static TinyCRC32Calculator()
        {
            unchecked
            {
                // PKZip specifies CRC32 with a polynomial of 0xEDB88320;
                // This is also the CRC-32 polynomial used bby Ethernet, FDDI,
                // bzip2, gzip, and others.
                // Often the polynomial is shown reversed as 0x04C11DB7.
                // For more details, see http://en.wikipedia.org/wiki/Cyclic_redundancy_check
                UInt32 dwPolynomial = 0xEDB88320;


                s_crc32Table = new UInt32[256];
                UInt32 dwCrc;
                for (uint i = 0; i < 256; i++)
                {
                    dwCrc = i;
                    for (uint j = 8; j > 0; j--)
                    {
                        if ((dwCrc & 1) == 1)
                        {
                            dwCrc = (dwCrc >> 1) ^ dwPolynomial;
                        }
                        else
                        {
                            dwCrc >>= 1;
                        }
                    }
                    s_crc32Table[i] = dwCrc;
                }
            }
        }


#if DEBUG
        //Int64 dbugTotalBytesRead;
#endif

        static readonly UInt32[] s_crc32Table;
        const int BUFFER_SIZE = 2048;

        [System.ThreadStatic]
        static byte[] s_buffer;

        public static int CalculateCrc32(string inputData)
        {
            if (s_buffer == null)
            {
                s_buffer = new byte[BUFFER_SIZE];
            }

            if (inputData.Length > 512)
            {
                byte[] utf8 = System.Text.Encoding.UTF8.GetBytes(inputData);
                return SlurpBlock(utf8, 0, utf8.Length);
            }
            else
            {
                int write = System.Text.Encoding.UTF8.GetBytes(inputData, 0, inputData.Length, s_buffer, 0);
                if (write >= BUFFER_SIZE)
                {
                    throw new System.NotSupportedException("crc32:");
                }
                return SlurpBlock(s_buffer, 0, write);
            }
        }

        public static int CalculateCrc32(byte[] buffer) => SlurpBlock(buffer, 0, buffer.Length);
    }
    public static class InstalledTypefaceCollectionExtensions
    {

        public delegate R MyFunc<T1, T2, R>(T1 t1, T2 t2);
        public delegate R MyFunc<T, R>(T t);

        public static Action<InstalledTypefaceCollection> CustomSystemFontListLoader;

        public static MyFunc<string, Stream> CustomFontStreamLoader;
        public static void LoadFontsFromFolder(this InstalledTypefaceCollection fontCollection, string folder, bool recursive = false)
        {
            if (!Directory.Exists(folder))
            {
#if DEBUG

                System.Diagnostics.Debug.WriteLine("LoadFontsFromFolder, not found folder:" + folder);

#endif
                return;
            }
            //-------------------------------------

            // 1. font dir
            foreach (string file in Directory.GetFiles(folder))
            {
                //eg. this is our custom font folder
                string ext = Path.GetExtension(file).ToLower();
                switch (ext)
                {
                    default: break;
                    case ".ttc":
                    case ".otc":
                    case ".ttf":
                    case ".otf":
                    case ".woff":
                    case ".woff2":
                        fontCollection.AddFontStreamSource(new FontFileStreamProvider(file));
                        break;
                }
            }

            //2. browse recursively; on Linux, fonts are organised in subdirectories
            if (recursive)
            {
                foreach (string subfolder in Directory.GetDirectories(folder))
                {
                    LoadFontsFromFolder(fontCollection, subfolder, recursive);
                }
            }
        }
        public static void LoadSystemFonts(this InstalledTypefaceCollection fontCollection, bool recursive = false)
        {

            if (CustomSystemFontListLoader != null)
            {
                CustomSystemFontListLoader(fontCollection);
                return;
            }
            // Windows system fonts
            LoadFontsFromFolder(fontCollection, "c:\\Windows\\Fonts");
            // These are reasonable places to look for fonts on Linux
            LoadFontsFromFolder(fontCollection, "/usr/share/fonts", true);
            LoadFontsFromFolder(fontCollection, "/usr/share/wine/fonts", true);
            LoadFontsFromFolder(fontCollection, "/usr/share/texlive/texmf-dist/fonts", true);
            LoadFontsFromFolder(fontCollection, "/usr/share/texmf/fonts", true);

            // OS X system fonts (https://support.apple.com/en-us/HT201722)

            LoadFontsFromFolder(fontCollection, "/System/Library/Fonts");
            LoadFontsFromFolder(fontCollection, "/Library/Fonts");

        }

        public static IEnumerable<BitposAndAssciatedUnicodeRanges> GetSupportedUnicodeLangIter(this InstalledTypeface instTypeface)
        {
            //check all 0-125 bits 
            for (int i = 0; i <= OpenFontBitPosInfo.MAX_BITPOS; ++i)
            {
                if (instTypeface.DoesSupportUnicode(i))
                {
                    yield return OpenFontBitPosInfo.GetUnicodeRanges(i);
                }
            }
        }


        public static Typeface ResolveTypeface(this InstalledTypefaceCollection fontCollection, string fontName, TypefaceStyle style)
        {
            InstalledTypeface inst = fontCollection.GetInstalledTypeface(fontName, InstalledTypefaceCollection.GetSubFam(style));
            return (inst != null) ? fontCollection.ResolveTypeface(inst) : null;
        }
        public static Typeface ResolveTypeface(this InstalledTypefaceCollection fontCollection, InstalledTypeface installedFont)
        {

            if (installedFont.ResolvedTypeface != null) return installedFont.ResolvedTypeface;

            //load  
            Typeface typeface;
            if (CustomFontStreamLoader != null)
            {
                using (var fontStream = CustomFontStreamLoader(installedFont.FontPath))
                {
                    var reader = new OpenFontReader();
                    typeface = reader.Read(fontStream, installedFont.ActualStreamOffset);
                    typeface.Filename = installedFont.FontPath;
                    installedFont.ResolvedTypeface = typeface;//cache 
                }
            }
            else
            {
                using (var fs = new FileStream(installedFont.FontPath, FileMode.Open, FileAccess.Read))
                {

                    var reader = new OpenFontReader();
                    typeface = reader.Read(fs, installedFont.ActualStreamOffset);
                    typeface.Filename = installedFont.FontPath;
                    installedFont.ResolvedTypeface = typeface;//cache 
                }
            }

            //calculate typeface key for the new create typeface
            //custom key

            OpenFont.Extensions.TypefaceExtensions.SetCustomTypefaceKey(
                typeface,
                TinyCRC32Calculator.CalculateCrc32(typeface.Name.ToUpper()));

            return typeface;

        }

        public static Typeface ResolveTypefaceFromFile(this InstalledTypefaceCollection fontCollection, string filename)
        {
            //from user input typeface filename

            bool enable_absPath = false; //enable abs path or not
#if DEBUG
            enable_absPath = true;//in debug mode
#endif
            //TODO: review here again!!!
            if (!fontCollection._installedTypefacesByFilenames.TryGetValue(filename, out InstalledTypeface found))
            {
                if (!enable_absPath && Path.IsPathRooted(filename))
                {
                    return null;//***
                }

                //search for a file
                if (File.Exists(filename))
                {
                    //TODO: handle duplicated font!!
                    //try read this             
                    InstalledTypeface instTypeface;
                    using (FileStream fs = new FileStream(filename, FileMode.Open))
                    {
                        OpenFontReader reader = new OpenFontReader();
                        Typeface typeface = reader.Read(fs);

                        // 
                        OpenFont.Extensions.TypefaceExtensions.SetCustomTypefaceKey(
                            typeface,
                            TinyCRC32Calculator.CalculateCrc32(typeface.Name.ToUpper()));

                        instTypeface = new InstalledTypeface(typeface, TypefaceStyle.Regular, filename);
                        fontCollection._installedTypefacesByFilenames.Add(filename, instTypeface);

                        return instTypeface.ResolvedTypeface = typeface;//assign  and return                         
                    }
                }

            }
            else
            {
                //found inst type
                return ResolveTypeface(fontCollection, found);
            }
            return null;

        }
        //for Windows , how to find Windows' Font Directory from Windows Registry
        //        string[] localMachineFonts = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion\\Fonts", false).GetValueNames();
        //        // get parent of System folder to have Windows folder
        //        DirectoryInfo dirWindowsFolder = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.System));
        //        string strFontsFolder = Path.Combine(dirWindowsFolder.FullName, "Fonts");
        //        RegistryKey regKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion\\Fonts");
        //        //---------------------------------------- 
        //        foreach (string winFontName in localMachineFonts)
        //        {
        //            string f = (string)regKey.GetValue(winFontName);
        //            if (f.EndsWith(".ttf") || f.EndsWith(".otf"))
        //            {
        //                yield return Path.Combine(strFontsFolder, f);
        //            }
        //        }


    }
}