//Apache2, 2014-present, WinterDev
using System;
using System.Collections.Generic;

using Typography.OpenFont;
using Typography.OpenFont.Extensions;

namespace Typography.Text
{
    static class InternalFontKey
    {
        //only typeface name
        static readonly Dictionary<string, int> s_registerFontNames = new Dictionary<string, int>();
        static InternalFontKey()
        {
            RegisterFontName(""); //blank font name
        }
        public static int RegisterFontName(string fontName)
        {
            fontName = fontName.ToUpper();//***
            if (!s_registerFontNames.TryGetValue(fontName, out int found))
            {
                int nameCrc32 = Typography.FontManagement.TinyCRC32Calculator.CalculateCrc32(fontName);
                s_registerFontNames.Add(fontName, nameCrc32);
                return nameCrc32;
            }
            return found;
        }
        public static int CalculateGetHasCode(int typefaceKey, float fontSize, int fontstyle)
        {
            //modified from https://stackoverflow.com/questions/1646807/quick-and-simple-hash-code-combinations
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + typefaceKey.GetHashCode();
                hash = hash * 31 + fontSize.GetHashCode();
                hash = hash * 31 + fontstyle.GetHashCode();
                return hash;
            }
        }
    }



    public sealed class ResolvedFont
    {
        readonly float _px_scale;
        readonly int _ws;
        public ResolvedFont(Typeface typeface, float sizeInPoints, int fontKey)
        {
            Typeface = typeface;
            if (Typeface != null)
            {
                _px_scale = Typeface.CalculateScaleToPixelFromPointSize(sizeInPoints);
                _ws = (int)Math.Round(Typeface.GetWhitespaceWidth() * _px_scale);
                Name = typeface.Name;
            }
            SizeInPoints = sizeInPoints;

            Typeface = typeface;
            FontKey = fontKey;
        }
        public ResolvedFont(Typeface typeface, float sizeInPoints)
        {
            Typeface = typeface;
            if (Typeface != null)
            {
                _px_scale = Typeface.CalculateScaleToPixelFromPointSize(sizeInPoints);
                _ws = (int)Math.Round(Typeface.GetWhitespaceWidth() * _px_scale);
                Name = typeface.Name;
            }
            SizeInPoints = sizeInPoints;

            Typeface = typeface;
            FontKey = InternalFontKey.CalculateGetHasCode(
                TypefaceExtensions.GetCustomTypefaceKey(typeface),
                sizeInPoints, 0);
        }
        public Typeface Typeface { get; }

        public float WhitespaceWidthF
        {
            get
            {
                if (Typeface != null)
                {
                    return Typeface.GetWhitespaceWidth() * _px_scale;
                }
                return 0;
            }
        }
        public int WhitespaceWidth => _ws;
        public float GetScaleToPixelFromPointInSize() => _px_scale;
        public float AscentInPixels => (Typeface != null) ? _px_scale * Typeface.Ascender : 0;
        public float DescentInPixels => (Typeface != null) ? _px_scale * Typeface.Descender : 0;
        public float LineGapInPx => (Typeface != null) ? (int)(Math.Round(Typeface.LineGap * _px_scale)) : 0;

        public int LineSpacingInPixels => (Typeface != null) ? (int)(Math.Round(Typeface.CalculateRecommendLineSpacing() * _px_scale)) : 0;
        public int MaxLineClipHeightInPixels => (Typeface != null) ? (int)(Math.Round(Typeface.CalculateMaxLineClipHeight() * _px_scale)) : 0;

#if DEBUG
        public override string ToString() => Typeface?.Name;
#endif

        public float SizeInPoints { get; private set; }
        public int FontKey { get; private set; }
        public string Name { get; }
    }


}