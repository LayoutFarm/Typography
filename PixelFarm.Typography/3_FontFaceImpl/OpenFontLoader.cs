//MIT, 2014-2017, WinterDev 

using System.IO;
using Typography.OpenFont;

namespace PixelFarm.Drawing.Fonts
{
    public static class OpenFontLoader
    {
        public static FontFace LoadFont(
            Typeface typeface)
        {
            //read font file 
            //TODO:...
            //set shape engine ***  

            var openFont = new NOpenFontFace(typeface, typeface.Name, typeface.Filename);
            return openFont;
        }
        public static FontFace LoadFont(string fontpath)
        {

            using (FileStream fs = new FileStream(fontpath, FileMode.Open, FileAccess.Read))
            {
                var reader = new OpenFontReader();
                Typeface t = reader.Read(fs);
                t.Filename = fontpath;
                return LoadFont(t);
            }
        }
    }


}