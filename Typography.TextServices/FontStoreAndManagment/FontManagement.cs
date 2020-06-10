//MIT, 2016-present, WinterDev 
using System;
using System.Collections.Generic;
using System.IO;
using Typography.OpenFont;
using Typography.OpenFont.Tables;

namespace Typography.OpenFont
{
    public static class TypefaceExtension3
    {
        public static void CollectScriptLang(this FontManagement.InstalledTypeface typeface, Dictionary<string, ScriptLang> output)
        {
            typeface.Languages.CollectScriptLang(output);
        }

        public static bool DoesSupportUnicode(
               this PreviewFontInfo previewFontInfo,
               UnicodeLangBits unicodeLangBits)
        {
            return previewFontInfo.Languages.DoesSupportUnicode((UnicodeLangBits5_1)unicodeLangBits);
        }


        public static bool DoesSupportUnicode(
            this Typeface typeface,
            UnicodeLangBits unicodeLangBits)
        {
            return typeface.Languages.DoesSupportUnicode((UnicodeLangBits5_1)unicodeLangBits);
        }

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

        public static void CollectAllAssociateGlyphIndex(this Typeface typeface, List<ushort> outputGlyphIndexList, ScriptLang scLang, UnicodeLangBits[] selectedRangs = null)
        {
            //-----------
            //general glyph index in the unicode range

            //if user dose not specific the unicode lanf bit ranges
            //the we try to select it ourself. 

            if (ScriptLangs.TryGetUnicodeLangBitsArray(scLang.GetScriptTagString(), out UnicodeLangBits[] unicodeLangBitsRanges))
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
        public virtual InstalledTypeface Select(List<InstalledTypeface> choices, ScriptLangInfo scriptLangInfo, char hintChar)
        {
            if (choices.Count > 0)
            {
                return choices[0];//temp
            }
            return null;
        }
    }

    public class InstalledTypeface
    {
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
        public bool DoesSupportUnicode(UnicodeLangBits unicodeLangBits) => Languages.DoesSupportUnicode((UnicodeLangBits5_1)unicodeLangBits);

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
        Dictionary<string, InstalledTypefaceGroup> _subFamToFontGroup = new Dictionary<string, InstalledTypefaceGroup>();
        Dictionary<string, bool> _onlyFontNames = new Dictionary<string, bool>();


        InstalledTypefaceGroup _regular, _bold, _italic, _bold_italic;
        List<InstalledTypefaceGroup> _allGroups = new List<InstalledTypefaceGroup>();
        FontNameDuplicatedHandler _fontNameDuplicatedHandler;
        FontNotFoundHandler _fontNotFoundHandler;

        Dictionary<string, InstalledTypeface> _otherFontNames = new Dictionary<string, InstalledTypeface>();
        Dictionary<string, InstalledTypeface> _postScriptNames = new Dictionary<string, InstalledTypeface>();

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

        bool AddFontPreview(PreviewFontInfo previewFont, string srcPath)
        {
            _onlyFontNames[previewFont.Name] = true;

            TypefaceStyle typefaceStyle = TypefaceStyle.Regular;
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
                    case "BOLD": typefaceStyle = TypefaceStyle.Bold; break;
                    case "ITALIC": typefaceStyle = TypefaceStyle.Italic; break;
                }
            }
            return Register(new InstalledTypeface(
                previewFont,
                typefaceStyle,
                srcPath)
            { ActualStreamOffset = previewFont.ActualStreamOffset });
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
                            if (!AddFontPreview(previewFont.GetMember(i), src.PathName))
                            {
                                totalResult = false;
                            }
                        }
                        return totalResult;
                    }
                    else
                    {
                        return AddFontPreview(previewFont, src.PathName);
                    }

                }
            }
            catch (IOException)
            {
                //TODO review here again
                return false;
            }
        }

        bool Register(InstalledTypeface newTypeface)
        {



            InstalledTypefaceGroup selectedFontGroup = null;

            string fontSubFamUpperCaseName = newTypeface.TypographicFontSubFamily;
            bool use_typographicSubFam = true;
            if (fontSubFamUpperCaseName == null)
            {
                //switch to FontSubFamily, this should not be null!
                fontSubFamUpperCaseName = newTypeface.FontSubFamily;
                use_typographicSubFam = false;
            }
            fontSubFamUpperCaseName = fontSubFamUpperCaseName.ToUpper();
            //--------------

            switch (newTypeface.TypefaceStyle)
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
            string register_name = newTypeface.TypographicFamilyName;
            bool use_typographicFontFam = true;
            if (register_name == null)
            {
                //switch to font name, this should not be null!
                register_name = newTypeface.FontName;
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
                    switch (_fontNameDuplicatedHandler(found, newTypeface))
                    {
                        default:
                            throw new NotSupportedException();
                        case FontNameDuplicatedDecision.Skip:
                            break;
                        case FontNameDuplicatedDecision.Replace:
                            selectedFontGroup.Replace(register_name, newTypeface);
                            register_result = true;
                            break;
                    }
                }
            }
            else
            {
                selectedFontGroup.AddFont(register_name, newTypeface);
                register_result = true;
            }

            if (use_typographicFontFam &&
                newTypeface.FontName != newTypeface.TypographicFamilyName &&
                newTypeface.TypefaceStyle == TypefaceStyle.Regular)
            {
                //in this case, the code above register the typeface with TypographicFamilyName
                //so we register this typeface with original name too
                if (_otherFontNames.ContainsKey(newTypeface.FontName.ToUpper()))
                {
                    System.Diagnostics.Debug.WriteLine("duplicated font name?:" + newTypeface.FontName.ToUpper());
                }
                else
                {
                    _otherFontNames.Add(newTypeface.FontName.ToUpper(), newTypeface);
                }
            }

            //register font
            if (newTypeface.PostScriptName != null)
            {
                string postScriptName = newTypeface.PostScriptName.ToUpper();
                if (!_postScriptNames.ContainsKey(postScriptName))
                {
                    _postScriptNames.Add(postScriptName, newTypeface);
                }
                else
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("duplicated postscriptname:" + postScriptName);
#endif
                }
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

            InstalledTypeface foundInstalledFont;

            //find font group  
            if (_subFamToFontGroup.TryGetValue(upperCaseSubFamName, out InstalledTypefaceGroup foundFontGroup) &&
                foundFontGroup.TryGetValue(upperCaseFontName, out foundInstalledFont))
            {
                return foundInstalledFont;
            }

            //
            if (_otherFontNames.TryGetValue(upperCaseFontName, out foundInstalledFont))
            {
                return foundInstalledFont;
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

        public void UpdateUnicodeRanges()
        {
            _registeredWithUniCodeLangBits.Clear();
            foreach (InstalledTypeface instFont in GetInstalledFontIter())
            {
                foreach (UnicodeLangBits unicodeLangBit in instFont.GetSupportedUnicodeLangBitIter())
                {

                    RegisterUnicodeSupport(unicodeLangBit, instFont);
                }
            }
        }
        public bool TryGetAlternativeTypefaceFromChar(char c, out ScriptLangInfo foundScriptLang, out List<InstalledTypeface> found)
        {
            //find a typeface that supported input char c
            //1. unicode to lang=> to script
            //2. then find typeface the support it 

            if (ScriptLangs.TryGetScriptLang(c, out foundScriptLang) && foundScriptLang.unicodeLangs != null)
            {
                foreach (UnicodeLangBits langBits in foundScriptLang.unicodeLangs)
                {
                    if (_registeredWithUniCodeLangBits.TryGetValue(langBits, out List<InstalledTypeface> typefaceList) && typefaceList.Count > 0)
                    {
                        //select a proper typeface                        
                        found = typefaceList;
                        return true;
                    }
                }
            }
            found = null;
            return false;
        }


        readonly Dictionary<UnicodeLangBits, List<InstalledTypeface>> _registeredWithUniCodeLangBits = new Dictionary<UnicodeLangBits, List<InstalledTypeface>>();
        void RegisterUnicodeSupport(UnicodeLangBits langBit, InstalledTypeface instFont)
        {

            if (!_registeredWithUniCodeLangBits.TryGetValue(langBit, out List<InstalledTypeface> found))
            {
                found = new List<InstalledTypeface>();
                _registeredWithUniCodeLangBits.Add(langBit, found);
            }
            found.Add(instFont);
        }

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

        public static IEnumerable<UnicodeLangBits> GetSupportedUnicodeLangBitIter(this InstalledTypeface instTypeface)
        {
            foreach (UnicodeLangBits unicodeLangBit in UnicodeLangs.GetUnicodeLangBitIter())
            {
                if (instTypeface.DoesSupportUnicode(unicodeLangBit))
                {
                    yield return unicodeLangBit;
                }
            }
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