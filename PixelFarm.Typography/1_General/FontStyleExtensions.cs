//MIT, 2016-present, WinterDev 


namespace PixelFarm.Drawing
{
    static class FontStyleExtensions
    {

        public static Typography.FontCollections.TypefaceStyle ConvToInstalledFontStyle(this RequestFontStyle style)
        {
            switch (style)
            {
                default:
                case RequestFontStyle.Regular: return Typography.FontCollections.TypefaceStyle.Regular;

                case RequestFontStyle.Italic: return Typography.FontCollections.TypefaceStyle.Italic;
                case RequestFontStyle.Oblique: return Typography.FontCollections.TypefaceStyle.Italic;
            }

        }
    }

}