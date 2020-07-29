//Apache2, 2014-present, WinterDev
using System;

using Typography.OpenFont;
using Typography.OpenFont.Extensions;
using Typography.OpenFont.Tables;

namespace Typography.Text
{


    public sealed class ResolvedFont
    {
        readonly float _px_scale;
        readonly int _ws;
        public readonly int RuntimeResolvedKey;

        public ResolvedFont(Typeface typeface, float sizeInPoints)
        {
            Typeface = typeface;
            if (typeface != null)
            {
                _px_scale = typeface.CalculateScaleToPixelFromPointSize(sizeInPoints);
                _ws = (int)Math.Round(typeface.GetWhitespaceWidth() * _px_scale);
                Name = typeface.Name;
            }

            SizeInPoints = sizeInPoints;

            RuntimeResolvedKey = CalculateGetHasCode(
                TypefaceExtensions.GetCustomTypefaceKey(typeface),
                sizeInPoints);
        }
        public float SizeInPoints { get; }
        public string Name { get; }
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
        public float GetScaleToPixelFromPointUnit() => _px_scale;
        public float AscentInPixels => (Typeface != null) ? _px_scale * Typeface.Ascender : 0;
        public float DescentInPixels => (Typeface != null) ? _px_scale * Typeface.Descender : 0;
        public float LineGapInPx => (Typeface != null) ? (int)(Math.Round(Typeface.LineGap * _px_scale)) : 0;

        public int LineSpacingInPixels => (Typeface != null) ? (int)(Math.Round(Typeface.CalculateMaxLineClipHeight() * _px_scale)) : 0;
        public int MaxLineClipHeightInPixels => (Typeface != null) ? (int)(Math.Round(Typeface.CalculateMaxLineClipHeight() * _px_scale)) : 0;
        public float UsDescendingInPixels => (Typeface != null) ? _px_scale * Typeface.GetOS2Table().usWinDescent : 0;

#if DEBUG
        public override string ToString() => Typeface?.Name;
#endif

        static int CalculateGetHasCode(int typefaceKey, float fontSize)
        {
            //modified from https://stackoverflow.com/questions/1646807/quick-and-simple-hash-code-combinations
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + typefaceKey;
                hash = hash * 31 + fontSize.GetHashCode();
                return hash;
            }
        }
    }
}