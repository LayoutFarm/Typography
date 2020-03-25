//MIT, 2016-present, WinterDev
#nullable enable
using Typography.OpenFont;

namespace Typography.Contours
{
    //-----------------------------------
    //sample GlyphPathBuilder :
    //for your flexiblity of glyph path builder.
    //-----------------------------------


    public abstract class GlyphOutlineBuilderBase
    {
        TrueTypeInterpreter? _trueTypeInterpreter;
        protected GlyphPointF[]? _outputGlyphPoints;
        protected ushort[]? _outputContours;

        protected OpenFont.CFF.Cff1GlyphData? _cff;

        /// <summary>
        /// scale for converting latest glyph points to latest request font size
        /// </summary>
        float _recentPixelScale;

        Typography.OpenFont.CFF.CffEvaluationEngine? _cffEvalEngine;

        public GlyphOutlineBuilderBase(Typeface typeface)
        {
            Typeface = typeface;
            this.UseTrueTypeInstructions = true;//default?
            _recentPixelScale = 1;
        }
        public Typeface Typeface { get; private set; }
        /// <summary>
        /// use Maxim's Agg Vertical Hinting
        /// </summary>
        public bool UseVerticalHinting { get; set; }
        /// <summary>
        /// process glyph with true type instructions
        /// </summary>
        public bool UseTrueTypeInstructions { get; set; }

        /// <summary>
        /// build glyph shape from glyphIndex to be read
        /// </summary>
        /// <param name="glyphIndex"></param>
        /// <param name="sizeInPoints"></param>
        public void BuildFromGlyphIndex(ushort glyphIndex, float sizeInPoints)
        {
            BuildFromGlyph(Typeface.GetGlyph(glyphIndex), sizeInPoints);
        }
        /// <summary>
        /// build glyph shape from glyph to be read
        /// </summary>
        /// <param name="glyphIndex"></param>
        /// <param name="sizeInPoints"></param>
        public void BuildFromGlyph(Glyph glyph, float sizeInPoints)
        {
            //for true type font
            if (glyph.TtfWoffInfo is { } ttf)
                (_outputContours, _outputGlyphPoints) = ttf;
            else if (glyph.CffInfo is { } cff)
            {
                //------------
                //temp fix for Cff Font
                _cff = cff;

                //---------------
            }
            else throw new System.NotImplementedException("Bitmap and SVG glyphs not implemented");


            if ((RecentFontSizeInPixels = Typeface.ConvPointsToPixels(sizeInPoints)) < 0)
            {
                //convert to pixel size
                //if size< 0 then set _recentPixelScale = 1;
                //mean that no scaling at all, we use original point value
                _recentPixelScale = 1;
            }
            else
            {
                _recentPixelScale = Typeface.CalculateScaleToPixel(RecentFontSizeInPixels);
                HasSizeChanged = true;
            }
            //-------------------------------------
            FitCurrentGlyph(glyph);
        }
        protected bool HasSizeChanged { get; set; }
        protected float RecentFontSizeInPixels { get; private set; }
        protected virtual void FitCurrentGlyph(Glyph glyph)
        {
            try
            {
                if (RecentFontSizeInPixels > 0 && UseTrueTypeInstructions &&
                    Typeface.HasPrepProgramBuffer &&
                    glyph.HasGlyphInstructions)
                {
                    if (_trueTypeInterpreter == null)
                    {
                        _trueTypeInterpreter = new TrueTypeInterpreter(Typeface);
                    }
                    else if (_trueTypeInterpreter.Typeface != Typeface)
                    {
                        _trueTypeInterpreter.Typeface = Typeface;
                    }
                    _trueTypeInterpreter.UseVerticalHinting = this.UseVerticalHinting;
                    //output as points,
                    _outputGlyphPoints = _trueTypeInterpreter.HintGlyph(glyph.GlyphIndex, RecentFontSizeInPixels);
                    //***
                    //all points are scaled from _trueTypeInterpreter, 
                    //so not need further scale.=> set _recentPixelScale=1
                    _recentPixelScale = 1;
                }
            }
            catch
            {

            }
        }

        public virtual void ReadShapes(IGlyphTranslator tx)
        {
            //read output from glyph points
            if (_cff is { } cff)
            {
                _cffEvalEngine ??= new OpenFont.CFF.CffEvaluationEngine();
                _cffEvalEngine.Run(tx, cff, _recentPixelScale);
            }
            else if (_outputGlyphPoints is { } points && _outputContours is { } contours)
            {
                tx.Read(points, contours, _recentPixelScale);
            }
            else throw new System.InvalidOperationException($"{nameof(BuildFromGlyph)} not called");
        }
    }

    public static class GlyphPathBuilderExtensions
    {
        public static void Build(this GlyphOutlineBuilderBase builder, char c, float sizeInPoints)
        {
            builder.BuildFromGlyphIndex((ushort)builder.Typeface.GetGlyphIndex(c), sizeInPoints);
        }
        public static void SetHintTechnique(this GlyphOutlineBuilderBase builder, HintTechnique hintTech)
        {

            builder.UseTrueTypeInstructions = false;//reset
            builder.UseVerticalHinting = false;//reset
            switch (hintTech)
            {
                case HintTechnique.TrueTypeInstruction:
                    builder.UseTrueTypeInstructions = true;
                    break;
                case HintTechnique.TrueTypeInstruction_VerticalOnly:
                    builder.UseTrueTypeInstructions = true;
                    builder.UseVerticalHinting = true;
                    break;
                case HintTechnique.CustomAutoFit:
                    //custom agg autofit 
                    builder.UseVerticalHinting = true;
                    break;
            }
        }
    }
    public enum HintTechnique : byte
    {
        /// <summary>
        /// no hinting
        /// </summary>
        None,
        /// <summary>
        /// truetype instruction
        /// </summary>
        TrueTypeInstruction,
        /// <summary>
        /// truetype instruction vertical only
        /// </summary>
        TrueTypeInstruction_VerticalOnly,
        /// <summary>
        /// custom hint
        /// </summary>
        CustomAutoFit,

        /// <summary>
        /// Cff instruction hint
        /// </summary>
        CffHintInstruction

    }
}