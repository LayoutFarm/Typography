//MIT, 2016-2017, WinterDev 
using System;
using System.Collections.Generic;
using System.IO;
using Typography.OpenFont;

namespace Typography.Rendering
{

    public interface IFontface
    {
        string FontName { get; }
        string FontSubFamily { get; }
    }

    public class FontRequest
    {
        public string FontName { get; set; }
        public InstalledFontStyle Style { get; set; }
    }

    public class InstalledFont
    {
        public InstalledFont(string fontName, string fontSubFamily, string fontPath)
        {
            FontName = fontName;
            FontSubFamily = fontSubFamily;
            FontPath = fontPath;
        }

        public string FontName { get; set; }
        public string FontSubFamily { get; set; }
        public string FontPath { get; set; }

#if DEBUG
        public override string ToString()
        {
            return FontName + " " + FontSubFamily;
        }
#endif
    }
    public interface IInstalledFontProvider
    {
        IEnumerable<string> GetInstalledFontIter();
    }
    public interface IFontLoader
    {
        InstalledFont GetFont(string fontName, InstalledFontStyle style);
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
        Regular,
        Bold = 1 << 1,
        Italic = 1 << 2,
    }

    public delegate InstalledFont FontNotFoundHandler(InstalledFontCollection fontCollection, string fontName, InstalledFontStyle style);
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
        /// <summary>
        /// font collection of the store
        /// </summary>
        public InstalledFontCollection FontCollection { get; set; }


        //check if we have create this typeface or not 

        public Typeface GetTypeface(string fontname, InstalledFontStyle style)
        {
            InstalledFont installFont = FontCollection.GetFont(fontname, style);
            if (installFont == null) { return null; }
            //---------------
            //load 

            Typeface typeface = null;
            using (var fs = new FileStream(installFont.FontPath, FileMode.Open, FileAccess.Read))
            {
                var reader = new OpenFontReader();
                typeface = reader.Read(fs);
            }
            return typeface;
        }
    }

    public class InstalledFontCollection
    {

        Dictionary<string, InstalledFont> regular_Fonts = new Dictionary<string, InstalledFont>();
        Dictionary<string, InstalledFont> italic_Fonts = new Dictionary<string, InstalledFont>();
        Dictionary<string, InstalledFont> bold_Fonts = new Dictionary<string, InstalledFont>();

        Dictionary<string, InstalledFont> boldItalic_Fonts = new Dictionary<string, InstalledFont>();
        Dictionary<string, InstalledFont> gras_Fonts = new Dictionary<string, InstalledFont>();
        Dictionary<string, InstalledFont> grasItalic_Fonts = new Dictionary<string, InstalledFont>();
        //
        Dictionary<string, Dictionary<string, InstalledFont>> _fontGroups = new Dictionary<string, Dictionary<string, InstalledFont>>();

        FontNameDuplicatedHandler fontNameDuplicatedHandler;
        FontNotFoundHandler fontNotFoundHandler;
        //

        public InstalledFontCollection()
        {
            //init
            regular_Fonts = CreateNewFontGroup("normal", "regular");
            italic_Fonts = CreateNewFontGroup("italic", "italique");
            bold_Fonts = CreateNewFontGroup("bold");
            boldItalic_Fonts = CreateNewFontGroup("bold italic");
            gras_Fonts = CreateNewFontGroup("gras");
            grasItalic_Fonts = CreateNewFontGroup("gras italique");
        }
        Dictionary<string, InstalledFont> CreateNewFontGroup(params string[] names)
        {
            //single dic may be called by many names
            var fontGroup = new Dictionary<string, InstalledFont>();
            foreach (string name in names)
            {
                _fontGroups.Add(name.ToUpper(), fontGroup);
            }
            return fontGroup;
        }

        public void SetFontNotFoundHandler(FontNotFoundHandler handler)
        {
            fontNotFoundHandler = handler;
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
                if (previewFont.fontName == "" || previewFont.fontName.StartsWith("\0"))
                {
                    //err!
                    return false;
                }
                return RegisterFont(new InstalledFont(previewFont.fontName, previewFont.fontSubFamily, src.PathName));
            }
        }
        bool RegisterFont(InstalledFont f)
        {
            Dictionary<string, InstalledFont> selectedFontGroup;
            if (!_fontGroups.TryGetValue(f.FontSubFamily.ToUpper(), out selectedFontGroup))
            {
                //create new group
                selectedFontGroup = new Dictionary<string, InstalledFont>();
                _fontGroups.Add(f.FontSubFamily.ToUpper(), selectedFontGroup);
            }
            //
            string fontNameUpper = f.FontName.ToUpper();
            InstalledFont found;
            if (selectedFontGroup.TryGetValue(fontNameUpper, out found))
            {
                //TODO:
                //we already have this font name
                //(but may be different file
                //we let user to handle it        
                switch (fontNameDuplicatedHandler(found, f))
                {
                    default: throw new NotSupportedException();
                    case FontNameDuplicatedDecision.Skip:
                        return false;
                    case FontNameDuplicatedDecision.Replace:
                        selectedFontGroup[fontNameUpper] = f;
                        return true;
                }
            }
            else
            {
                selectedFontGroup.Add(fontNameUpper, f);
                return true;
            }
        }

        public InstalledFont GetFont(string fontName, InstalledFontStyle style)
        {
            //request font from installed font
            InstalledFont found;
            switch (style)
            {
                case (InstalledFontStyle.Bold | InstalledFontStyle.Italic):
                    {
                        //check if we have bold & italic 
                        //version of this font ?  
                        if (!boldItalic_Fonts.TryGetValue(fontName.ToUpper(), out found))
                        {
                            //if not found then goto italic 
                            goto case InstalledFontStyle.Italic;
                        }
                        return found;
                    }
                case InstalledFontStyle.Bold:
                    {

                        if (!bold_Fonts.TryGetValue(fontName.ToUpper(), out found))
                        {
                            //goto regular
                            goto default;
                        }
                        return found;
                    }
                case InstalledFontStyle.Italic:
                    {
                        //if not found then choose regular
                        if (!italic_Fonts.TryGetValue(fontName.ToUpper(), out found))
                        {
                            goto default;
                        }
                        return found;
                    }
                default:
                    {
                        //we skip gras style ?
                        if (!regular_Fonts.TryGetValue(fontName.ToUpper(), out found))
                        {

                            if (fontNotFoundHandler != null)
                            {
                                return fontNotFoundHandler(
                                    this,
                                    fontName,
                                    style);
                            }
                            return null;
                        }
                        return found;
                    }
            }
        }


        public IEnumerable<InstalledFont> GetInstalledFontIter()
        {
            foreach (InstalledFont f in regular_Fonts.Values)
            {
                yield return f;
            }
            //
            foreach (InstalledFont f in italic_Fonts.Values)
            {
                yield return f;
            }
            //
            foreach (InstalledFont f in bold_Fonts.Values)
            {
                yield return f;
            }
            foreach (InstalledFont f in boldItalic_Fonts.Values)
            {
                yield return f;
            }
            //
            foreach (InstalledFont f in gras_Fonts.Values)
            {
                yield return f;
            }
            foreach (InstalledFont f in grasItalic_Fonts.Values)
            {
                yield return f;
            }
        }
    }


    public static class InstalledFontCollectionExtension
    {

        public static void LoadWindowsSystemFonts(this InstalledFontCollection fontCollection)
        {
            //1. font dir
            foreach (string file in Directory.GetFiles("c:\\Windows\\Fonts"))
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
        }
        public static void LoadMacSystemFonts(this InstalledFontCollection fontCollection)
        {
            //implement
        }
        public static void LoadLinuxSystemFonts(this InstalledFontCollection fontCollection)
        {
            //implement
        }
    }
}