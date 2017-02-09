//MIT, 2014-2017, WinterDev

using System;
using System.Collections.Generic;

namespace PixelFarm.Drawing.Fonts
{
    public struct FontKey
    {

        public readonly int FontNameIndex;
        public readonly float FontSize;
        public readonly FontStyle FontStyle;

        public FontKey(string fontname, float fontSize, FontStyle fs)
        {
            //font name/ not filename
            this.FontNameIndex = RegisterFontName(fontname.ToLower());
            this.FontSize = fontSize;
            this.FontStyle = fs;
        }

        static Dictionary<string, int> registerFontNames = new Dictionary<string, int>();
        static FontKey()
        {
            RegisterFontName(""); //blank font name
        }
        static int RegisterFontName(string fontName)
        {
            fontName = fontName.ToUpper();
            int found;
            if (!registerFontNames.TryGetValue(fontName, out found))
            {
                int nameIndex = registerFontNames.Count;
                registerFontNames.Add(fontName, nameIndex);
                return nameIndex;
            }
            return found;
        }
    }
}