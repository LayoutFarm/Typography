//MIT, 2017, Zou Wei(github/zwcloud)
using System.IO;

namespace DrawingGL.Text
{

    public delegate Stream LoadFontDelegate(string fontFile);

    public static class Utility
    {

        static LoadFontDelegate s_loadFontDel;
        public static void SetLoadFontDel(LoadFontDelegate loadFontDel)
        {
            s_loadFontDel = loadFontDel;
        }
        internal static Stream ReadFile(string filePath)
        {
            return s_loadFontDel(filePath);
        }
    }
}