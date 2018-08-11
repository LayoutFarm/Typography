//MIT, 2016-present, WinterDev 
using System;
using System.Collections.Generic;
using System.IO;
using Typography.OpenFont;


namespace Typography.FontManagement
{

    public class InstalledTypeface
    {
        internal InstalledTypeface(string fontName,
            string fontSubFamily,
            string fontPath,
            TypefaceStyle typefaceStyle)
        {
            FontName = fontName;
            FontSubFamily = fontSubFamily;
            FontPath = fontPath;
            TypefaceStyle = typefaceStyle;
        }

        public string FontName { get; internal set; }
        public string FontSubFamily { get; internal set; }
        public string FontPath { get; internal set; }
        public TypefaceStyle TypefaceStyle { get; internal set; }

        public override string ToString()
        {
            return FontName + " " + FontSubFamily;
        }
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

    public delegate void FirstInitFontCollectionDelegate(InstalledTypefaceCollection fontCollection);

    public delegate InstalledTypeface FontNotFoundHandler(InstalledTypefaceCollection fontCollection, string fontName, string fontSubFam);
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

            internal Dictionary<string, InstalledTypeface> _members = new Dictionary<string, InstalledTypeface>();
            public void AddFont(InstalledTypeface installedFont)
            {
                _members.Add(installedFont.FontName.ToUpper(), installedFont);
            }
            public bool TryGetValue(string fontName, out InstalledTypeface found)
            {
                return _members.TryGetValue(fontName, out found);
            }
            public void Replace(InstalledTypeface newone)
            {
                _members[newone.FontName.ToUpper()] = newone;
            }
        }

        /// <summary>
        /// map from font subfam to internal group name
        /// </summary>
        Dictionary<string, InstalledTypefaceGroup> _subFamToFontGroup = new Dictionary<string, InstalledTypefaceGroup>();


        InstalledTypefaceGroup _regular, _bold, _italic, _bold_italic;
        List<InstalledTypefaceGroup> _allGroups = new List<InstalledTypefaceGroup>();
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


        public bool AddFontStreamSource(IFontStreamSource src)
        {
            //preview data of font
            try
            {
                using (Stream stream = src.ReadFontStream())
                {
                    var reader = new OpenFontReader();
                    PreviewFontInfo previewFont = reader.ReadPreview(stream);
                    if (string.IsNullOrEmpty(previewFont.fontName))
                    {
                        //err!
                        return false;
                    }
                    //if (previewFont.fontName.StartsWith("Bungee"))
                    //{ 
                    //}


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
                    string[] fontSubFamUpperCaseName_split = previewFont.fontSubFamily.ToUpper().Split(' ');
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

                    return Register(new InstalledTypeface(previewFont.fontName, previewFont.fontSubFamily, src.PathName, typefaceStyle));
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
            switch (newTypeface.TypefaceStyle)
            {
                default:
                    {
                        string fontSubFamUpperCaseName = newTypeface.FontSubFamily.ToUpper();
                        if (!_subFamToFontGroup.TryGetValue(fontSubFamUpperCaseName, out selectedFontGroup))
                        {
                            //create new group, we don't known this font group before 
                            //so we add to 'other group' list
                            selectedFontGroup = new InstalledTypefaceGroup();
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
                    selectedFontGroup = _regular;
                    break;
                case (TypefaceStyle.Bold | TypefaceStyle.Italic):
                    selectedFontGroup = _bold_italic;
                    break;
            }

            //
            string fontNameUpper = newTypeface.FontName.ToUpper();

            InstalledTypeface found;
            if (selectedFontGroup.TryGetValue(fontNameUpper, out found))
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
                            return false;
                        case FontNameDuplicatedDecision.Replace:
                            selectedFontGroup.Replace(newTypeface);
                            return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                selectedFontGroup.AddFont(newTypeface);
                return true;
            }
        }

        public InstalledTypeface GetInstalledTypeface(string fontName, string subFamName)
        {
            string upperCaseFontName = fontName.ToUpper();
            string upperCaseSubFamName = subFamName.ToUpper();


            //find font group 

            if (_subFamToFontGroup.TryGetValue(upperCaseSubFamName, out InstalledTypefaceGroup foundFontGroup))
            {
                InstalledTypeface foundInstalledFont;
                if (foundFontGroup.TryGetValue(upperCaseFontName, out foundInstalledFont))
                {
                    return foundInstalledFont;
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
    }


    public static class InstalledTypefaceCollectionExtensions
    {
        public static void LoadFontsFromFolder(this InstalledTypefaceCollection fontCollection, string folder)
        {
            if (!Directory.Exists(folder)) return;
            //-------------------------------------

            // 1. font dir
            foreach (string file in Directory.GetFiles(folder))
            {
                //eg. this is our custom font folder
                string ext = Path.GetExtension(file).ToLower();
                switch (ext)
                {
                    default: break;
                    case ".ttf":
                    case ".otf":
                        fontCollection.AddFontStreamSource(new FontFileStreamProvider(file));
                        break;
                }
            }

            //2. browse recursively; on Linux, fonts are organised in subdirectories
            foreach (string subfolder in Directory.GetDirectories(folder))
            {
                LoadFontsFromFolder(fontCollection, subfolder);
            }

        }
        public static void LoadSystemFonts(this InstalledTypefaceCollection fontCollection)
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