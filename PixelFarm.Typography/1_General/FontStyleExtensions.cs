//MIT, 2016-present, WinterDev 


namespace PixelFarm.Drawing
{
    static class FontStyleExtensions
    {

        public static Typography.FontCollection.TypefaceStyle ConvToInstalledFontStyle(this RequestFontStyle style)
        {
            switch (style)
            {
                default:
                case RequestFontStyle.Regular: return Typography.FontCollection.TypefaceStyle.Regular;

                case RequestFontStyle.Italic: return Typography.FontCollection.TypefaceStyle.Italic;
                case RequestFontStyle.Oblique: return Typography.FontCollection.TypefaceStyle.Italic;
            }

        }
    }

}