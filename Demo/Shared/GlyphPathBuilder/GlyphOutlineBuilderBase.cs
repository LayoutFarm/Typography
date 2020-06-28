//MIT, 2016-present, WinterDev

using Typography.OpenFont;

namespace Typography.Contours
{
    //-----------------------------------
    //sample GlyphPathBuilder :
    //for your flexiblity of glyph path builder.
    //-----------------------------------


    public abstract class GlyphOutlineBuilderBase
    {
        readonly Typeface _typeface;
        TrueTypeInterpreter _trueTypeInterpreter;
        protected GlyphPointF[] _outputGlyphPoints;
        protected ushort[] _outputContours;


        protected OpenFont.CFF.Cff1GlyphData _cffGlyphData;

        /// <summary>
        /// scale for converting latest glyph points to latest request font size
        /// </summary>
        float _recentPixelScale;

        Typography.OpenFont.CFF.CffEvaluationEngine _cffEvalEngine;

        public GlyphOutlineBuilderBase(Typeface typeface)
        {
            _typeface = typeface;
            this.UseTrueTypeInstructions = true;//default?
            _recentPixelScale = 1;

            if (typeface.IsCffFont)
            {
                _cffEvalEngine = new OpenFont.CFF.CffEvaluationEngine();
            }
        }
        public Typeface Typeface => _typeface;
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
            BuildFromGlyph(_typeface.GetGlyph(glyphIndex), sizeInPoints);
        }
        /// <summary>
        /// build glyph shape from glyph to be read
        /// </summary>
        /// <param name="glyphIndex"></param>
        /// <param name="sizeInPoints"></param>
        public void BuildFromGlyph(Glyph glyph, float sizeInPoints)
        {
            //for true type font
            _outputGlyphPoints = glyph.GlyphPoints;
            _outputContours = glyph.EndPoints;


            //------------
            //temp fix for Cff Font
            if (glyph.IsCffGlyph)
            {
                _cffGlyphData = glyph.GetCff1GlyphData();
            }

            //---------------



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
                    _typeface.HasPrepProgramBuffer &&
                    glyph.HasGlyphInstructions)
                {
                    if (_trueTypeInterpreter == null)
                    {
                        _trueTypeInterpreter = new TrueTypeInterpreter();
                        _trueTypeInterpreter.Typeface = _typeface;
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
            catch (System.Exception ex)
            {

            }
        }

        public virtual void ReadShapes(IGlyphTranslator tx)
        {
            //read output from glyph points
            if (_cffGlyphData != null)
            {
                _cffEvalEngine.Run(tx, _cffGlyphData, _recentPixelScale);
            }
            else
            {
                tx.Read(_outputGlyphPoints, _outputContours, _recentPixelScale);
            }
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