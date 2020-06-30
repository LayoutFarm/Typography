//Apache2, 2014-present, WinterDev
using System;
using System.Collections.Generic;

using Typography.OpenFont;
using Typography.OpenFont.Extensions;
 
namespace PixelFarm.Drawing
{
    public struct ResolvedFont2
    {
        public Typeface Typeface { get; }
        public float SizeInPoints { get; }
        public FontStyle Style { get; }
        public int FontKey { get; }
        public string Name => Typeface.Name;

        public ResolvedFont2(Typeface typeface, float sizeInPoints, FontStyle style, int fontKey)
        {
            Typeface = typeface;
            SizeInPoints = sizeInPoints;
            FontKey = fontKey;
            Style = style;
        }

        public ResolvedFont2(ResolvedFont resolvedFont)
        {
            Typeface = resolvedFont.Typeface;
            SizeInPoints = resolvedFont.SizeInPoints;
            FontKey = resolvedFont.FontKey;
            Style = resolvedFont.FontStyle;
        }
    }

    public class ResolvedFont
    {
        float _px_scale;
        public ResolvedFont(Typeface typeface, float sizeInPoints, FontStyle fontStyle, int fontKey)
        {
            Typeface = typeface;
            if (Typeface != null)
            {
                _px_scale = Typeface.CalculateScaleToPixelFromPointSize(sizeInPoints);
                Name = typeface.Name;
            }
            SizeInPoints = sizeInPoints;
            FontStyle = fontStyle;
            FontKey = fontKey;
            Typeface = typeface;
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
        public int WhitespaceWidth
        {
            get
            {
                if (Typeface != null)
                {
                    return (int)Math.Round(Typeface.GetWhitespaceWidth() * _px_scale);
                }
                return 0;
            }
        }
        public float GetScaleToPixelFromPointInSize() => _px_scale;
        public float AscentInPixels => (Typeface != null) ? _px_scale * Typeface.Ascender : 0;
        public float DescentInPixels => (Typeface != null) ? _px_scale * Typeface.Descender : 0;
        public float LineGapInPx => (Typeface != null) ? (int)(Math.Round(Typeface.LineGap * _px_scale)) : 0;

        public int LineSpacingInPixels => (Typeface != null) ? (int)(Math.Round(Typeface.CalculateRecommendLineSpacing() * _px_scale)) : 0;
        public int MaxLineClipHeightInPixels => (Typeface != null) ? (int)(Math.Round(Typeface.CalculateMaxLineClipHeight() * _px_scale)) : 0;

        class EmptyResolvedFont : ResolvedFont
        {
            public EmptyResolvedFont() : base(null, 0, FontStyle.Regular, 0) { }
#if DEBUG
            public override string ToString() => "EMPTY_RESOLVED_FONT";
#endif
        }

        internal static readonly ResolvedFont s_empty = new EmptyResolvedFont();
#if DEBUG
        public override string ToString() => Typeface?.Name;
#endif

        public float SizeInPoints { get; protected set; }
        public FontStyle FontStyle { get; protected set; }
        public int FontKey { get; protected set; }
        public string Name { get; }
    }


}