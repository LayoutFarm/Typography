//MIT, 2016-2017, WinterDev 
using System;
using System.Collections.Generic;
using System.IO;
using Typography.OpenFont;


namespace Typography.TextServices
{
    public class InstalledFont
    {
        internal InstalledFont(string fontName,
            string fontSubFamily,
            string fontPath)
        {
            FontName = fontName;
            FontSubFamily = fontSubFamily;
            FontPath = fontPath;
        }

        public string FontName { get; internal set; }
        public string FontSubFamily { get; internal set; }
        public string FontPath { get; internal set; }

#if DEBUG
        public override string ToString()
        {
            return FontName + " " + FontSubFamily;
        }
#endif
    }


    public interface FontStreamSource
    {
        Stream ReadFontStream();
        string PathName { get; }
    }

    public class FontFileStreamProvider : FontStreamSource
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



    [Flags]
    public enum InstalledFontStyle
    {
        Others = 0,
        Normal = 1,
        Bold = 1 << 2,
        Italic = 1 << 3,
    }

    public delegate void FirstInitFontCollection(InstalledFontCollection fontCollection);

    public delegate InstalledFont FontNotFoundHandler(InstalledFontCollection fontCollection, string fontName, string fontSubFam, InstalledFontStyle wellknownStyle);
    public delegate FontNameDuplicatedDecision FontNameDuplicatedHandler(InstalledFont existing, InstalledFont newAddedFont);
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


    public class TypefaceStore
    {

        FontNotFoundHandler _fontNotFoundHandler;
        Dictionary<InstalledFont, Typeface> _loadedTypefaces = new Dictionary<InstalledFont, Typeface>();
        public TypefaceStore()
        {

        }
        /// <summary>
        /// font collection of the store
        /// </summary>
        public InstalledFontCollection FontCollection { get; set; }
        public void SetFontNotFoundHandler(FontNotFoundHandler fontNotFoundHandler)
        {
            _fontNotFoundHandler = fontNotFoundHandler;
        }
        public Typeface GetTypeface(InstalledFont installedFont)
        {
            return GetTypefaceOrCreateNew(installedFont);
        }
        public Typeface GetTypeface(string fontname, string fontSubFam)
        {

            InstalledFont installedFont = FontCollection.GetFont(fontname, fontSubFam);
            //convert from   
            if (installedFont == null && _fontNotFoundHandler != null)
            {
                installedFont = _fontNotFoundHandler(this.FontCollection, fontname, fontSubFam, FontCollection.GetWellknownFontStyle(fontSubFam));
            }
            if (installedFont == null)
            {
                return null;
            }
            return GetTypefaceOrCreateNew(installedFont);
        }
        /// <summary>
        /// get typeface from wellknown style
        /// </summary>
        /// <param name="fontname"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        public Typeface GetTypeface(string fontname, InstalledFontStyle style)
        {
            InstalledFont installedFont = FontCollection.GetFont(fontname, style);
            if (installedFont == null && _fontNotFoundHandler != null)
            {
                installedFont = _fontNotFoundHandler(this.FontCollection, fontname, null, style);
            }
            if (installedFont == null)
            {
                return null;
            }
            return GetTypefaceOrCreateNew(installedFont);
        }
        Typeface GetTypefaceOrCreateNew(InstalledFont installedFont)
        {
            //load 
            //check if we have create this typeface or not 
            Typeface typeface;
            if (!_loadedTypefaces.TryGetValue(installedFont, out typeface))
            {
                //TODO: review how to load font here
                using (var fs = new FileStream(installedFont.FontPath, FileMode.Open, FileAccess.Read))
                {
                    var reader = new OpenFontReader();
                    typeface = reader.Read(fs);
                }
                return _loadedTypefaces[installedFont] = typeface;
            }
            return typeface;
        }
    }

    public class InstalledFontCollection
    {


        class FontGroup
        {

            internal Dictionary<string, InstalledFont> _members = new Dictionary<string, InstalledFont>();
            public void AddFont(InstalledFont installedFont)
            {

                _members.Add(installedFont.FontName.ToUpper(), installedFont);
            }
            public bool TryGetValue(string fontName, out InstalledFont found)
            {
                return _members.TryGetValue(fontName, out found);
            }
            public void Replace(InstalledFont newone)
            {
                _members[newone.FontName.ToUpper()] = newone;
            }
        }

        /// <summary>
        /// map from font subfam to internal group name
        /// </summary>
        Dictionary<string, FontGroup> _subFamToFontGroup = new Dictionary<string, FontGroup>();

        FontGroup _normal, _bold, _italic, _bold_italic;
        List<FontGroup> _allFontGroups = new List<FontGroup>();
        FontNameDuplicatedHandler fontNameDuplicatedHandler;


        public InstalledFontCollection()
        {

            //-----------------------------------------------------
            //init wellknown subfam 
            _normal = CreateNewFontGroup(InstalledFontStyle.Normal, "regular", "normal");
            _italic = CreateNewFontGroup(InstalledFontStyle.Italic, "Italic", "italique");
            //
            _bold = CreateNewFontGroup(InstalledFontStyle.Bold, "bold");
            //
            _bold_italic = CreateNewFontGroup(InstalledFontStyle.Bold | InstalledFontStyle.Italic, "bold italic");
            //
        }


        static InstalledFontCollection s_sharedFontCollection;
        public static InstalledFontCollection GetSharedFontCollection(FirstInitFontCollection initdel)
        {
            if (s_sharedFontCollection == null)
            {
                //first time
                s_sharedFontCollection = new InstalledFontCollection();
                initdel(s_sharedFontCollection);
            }
            return s_sharedFontCollection;
        }




        public InstalledFontStyle GetWellknownFontStyle(string subFamName)
        {
            switch (subFamName.ToUpper())
            {
                default: return InstalledFontStyle.Others;
                case "NORMAL":
                case "REGULAR":
                    return InstalledFontStyle.Normal;
                case "BOLD":
                    return InstalledFontStyle.Bold;
                case "ITALIC":
                case "ITALIQUE":
                    return InstalledFontStyle.Italic;
                case "BOLD ITALIC":
                    return (InstalledFontStyle.Bold | InstalledFontStyle.Italic);
            }
        }
        FontGroup CreateNewFontGroup(InstalledFontStyle installedFontStyle, params string[] names)
        {
            //create font group
            var fontGroup = new FontGroup();
            //single dic may be called by many names            
            foreach (string name in names)
            {
                string upperCaseName = name.ToUpper();
                //register name
                //should not duplicate 
                _subFamToFontGroup.Add(upperCaseName, fontGroup);
            }
            _allFontGroups.Add(fontGroup);
            return fontGroup;
        }

        public void SetFontNameDuplicatedHandler(FontNameDuplicatedHandler handler)
        {
            fontNameDuplicatedHandler = handler;
        }
        public bool AddFont(FontStreamSource src)
        {
            //preview data of font
            using (Stream stream = src.ReadFontStream())
            {
                var reader = new OpenFontReader();
                PreviewFontInfo previewFont = reader.ReadPreview(stream);
                if (string.IsNullOrEmpty(previewFont.fontName))
                {
                    //err!
                    return false;
                }
                return RegisterFont(new InstalledFont(previewFont.fontName, previewFont.fontSubFamily, src.PathName));
            }
        }
        bool RegisterFont(InstalledFont newfont)
        {
            FontGroup selectedFontGroup;
            string fontsubFamUpperCaseName = newfont.FontSubFamily.ToUpper();

            if (!_subFamToFontGroup.TryGetValue(fontsubFamUpperCaseName, out selectedFontGroup))
            {
                //create new group, we don't known this font group before 
                //so we add to 'other group' list
                selectedFontGroup = new FontGroup();
                _subFamToFontGroup.Add(fontsubFamUpperCaseName, selectedFontGroup);
                _allFontGroups.Add(selectedFontGroup);
            }
            //
            string fontNameUpper = newfont.FontName.ToUpper();

            InstalledFont found;
            if (selectedFontGroup.TryGetValue(fontNameUpper, out found))
            {
                //TODO:
                //we already have this font name
                //(but may be different file
                //we let user to handle it        
                switch (fontNameDuplicatedHandler(found, newfont))
                {
                    default: throw new NotSupportedException();
                    case FontNameDuplicatedDecision.Skip:
                        return false;
                    case FontNameDuplicatedDecision.Replace:
                        selectedFontGroup.Replace(newfont);
                        return true;
                }
            }
            else
            {
                selectedFontGroup.AddFont(newfont);
                return true;
            }
        }

        public InstalledFont GetFont(string fontName, string subFamName)
        {
            string upperCaseFontName = fontName.ToUpper();
            string upperCaseSubFamName = subFamName.ToUpper();

            //find font group
            FontGroup foundFontGroup;
            if (_subFamToFontGroup.TryGetValue(upperCaseSubFamName, out foundFontGroup))
            {
                InstalledFont foundInstalledFont;
                foundFontGroup.TryGetValue(upperCaseFontName, out foundInstalledFont);
                return foundInstalledFont;
            }
            return null; //not found
        }

        public InstalledFont GetFont(string fontName, InstalledFontStyle wellknownSubFam)
        {
            //not auto resolve
            FontGroup selectedFontGroup;
            InstalledFont _found;
            switch (wellknownSubFam)
            {
                default: return null;
                case InstalledFontStyle.Normal: selectedFontGroup = _normal; break;
                case InstalledFontStyle.Bold: selectedFontGroup = _bold; break;
                case InstalledFontStyle.Italic: selectedFontGroup = _italic; break;
                case (InstalledFontStyle.Bold | InstalledFontStyle.Italic): selectedFontGroup = _bold_italic; break;
            }
            selectedFontGroup.TryGetValue(fontName.ToUpper(), out _found);
            return _found;
        }
        //public FindResult GetFont(string fontName, InstalledFontStyle style, out InstalledFont found)
        //{
        //    //request font from installed font
        //    string upperCaseFontName = fontName.ToUpper();
        //    FindResult result = FindResult.Matched;
        //    switch (style)
        //    {
        //        case (InstalledFontStyle.Bold | InstalledFontStyle.Italic):
        //            {
        //                //check if we have bold & italic 
        //                //version of this font ?  
        //                if (!_bold_italic.TryGetValue(upperCaseFontName, out found))
        //                {
        //                    //if not found then goto italic 
        //                    result = FindResult.Nearest;
        //                    goto case InstalledFontStyle.Italic;
        //                }
        //                return result;
        //            }
        //        case InstalledFontStyle.Bold:
        //            {

        //                if (!_bold.TryGetValue(upperCaseFontName, out found))
        //                {
        //                    //goto regular
        //                    result = FindResult.Nearest;
        //                    goto default;
        //                }
        //                return result;
        //            }
        //        case InstalledFontStyle.Italic:
        //            {
        //                //if not found then choose regular
        //                if (!_italic.TryGetValue(upperCaseFontName, out found))
        //                {
        //                    result = FindResult.Nearest;
        //                    goto default;
        //                }
        //                return result;
        //            }
        //        default:
        //            {
        //                //we skip gras style ?
        //                if (!_normal.TryGetValue(upperCaseFontName, out found))
        //                {
        //                    if (fontNotFoundHandler != null)
        //                    {
        //                        result = FindResult.Nearest;
        //                        found = fontNotFoundHandler(
        //                            this,
        //                            fontName,
        //                            style);
        //                        return (found == null) ? FindResult.NotFound : FindResult.Nearest;
        //                    }
        //                    return FindResult.NotFound;
        //                }
        //                return result;
        //            }
        //    }
        //}


        public IEnumerable<InstalledFont> GetInstalledFontIter()
        {
            foreach (FontGroup fontgroup in _allFontGroups)
            {
                foreach (InstalledFont f in fontgroup._members.Values)
                {
                    yield return f;
                }
            }
        }
    }


    public static class InstalledFontCollectionExtension
    {
        public static void LoadFontsFromFolder(this InstalledFontCollection fontCollection, string folder)
        {
            try
            {
                //font dir
                if (!Directory.Exists(folder))
                {
                    //not found
                    return;
                }
                
                foreach (string file in Directory.GetFiles(folder))
                {
                    //eg. this is our custom font folder
                    string ext = Path.GetExtension(file).ToLower();
                    switch (ext)
                    {
                        default: break;
                        case ".ttf":
                        case ".otf":
                            fontCollection.AddFont(new FontFileStreamProvider(file));
                            break;
                    }
                }

                //2. browse recursively; on Linux, fonts are organised in subdirectories
                foreach (string subfolder in Directory.GetDirectories(folder))
                {
                    LoadFontsFromFolder(fontCollection, subfolder);
                }
            }
            catch (DirectoryNotFoundException e)
            {
                return;
            }
        }
        public static void LoadSystemFonts(this InstalledFontCollection fontCollection)
        {
            // Windows system fonts
            LoadFontsFromFolder(fontCollection, "c:\\Windows\\Fonts");

            // These are reasonable places to look for fonts on Linux
            LoadFontsFromFolder(fontCollection, "/usr/share/fonts");
            LoadFontsFromFolder(fontCollection, "/usr/share/wine/fonts");
            LoadFontsFromFolder(fontCollection, "/usr/share/texlive/texmf-dist/fonts");
            LoadFontsFromFolder(fontCollection, "/usr/share/texmf/fonts");

            // OS X system fonts (https://support.apple.com/en-us/HT201722)
            LoadFontsFromFolder(fontCollection, "/System/Library/Fonts");
            LoadFontsFromFolder(fontCollection, "/Library/Fonts");




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
}