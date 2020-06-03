//MIT, 2016-present, WinterDev
using System.IO;
using System.Collections.Generic;
using Typography.OpenFont;
namespace Typography.FontManagement
{
    class TypefaceStore
    {

        Dictionary<InstalledTypeface, Typeface> _loadedTypefaces = new Dictionary<InstalledTypeface, Typeface>();
#if DEBUG
        public TypefaceStore()
        {

        }
#endif

        /// <summary>
        /// font collection of the store
        /// </summary>
        public InstalledTypefaceCollection FontCollection { get; set; }

        public Typeface GetTypeface(InstalledTypeface installedFont)
        {
            return GetTypefaceOrCreateNew(installedFont);
        }
        public Typeface GetTypeface(string fontname, string fontSubFam)
        {

            InstalledTypeface installedFont = FontCollection.GetInstalledTypeface(fontname, fontSubFam);
            //convert from    
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
        public Typeface GetTypeface(string fontname, TypefaceStyle style)
        {
            InstalledTypeface installedFont = FontCollection.GetInstalledTypeface(fontname, style);
            if (installedFont == null)
            {
                return null;
            }
            return GetTypefaceOrCreateNew(installedFont);
        }
        Typeface GetTypefaceOrCreateNew(InstalledTypeface installedFont)
        {
            //load 
            //check if we have create this typeface or not 
            if (!_loadedTypefaces.TryGetValue(installedFont, out Typeface typeface))
            {
                //TODO: review how to load font here
                using (var fs = new FileStream(installedFont.FontPath, FileMode.Open, FileAccess.Read))
                {
                    var reader = new OpenFontReader();
                    typeface = reader.Read(fs, installedFont.ActualStreamOffset);
                }
                return _loadedTypefaces[installedFont] = typeface;
            }
            return typeface;
        }
    }
}