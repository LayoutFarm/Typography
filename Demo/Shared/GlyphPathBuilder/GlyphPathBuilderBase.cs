//MIT, 2016-2017, WinterDev

using Typography.OpenFont;


namespace Typography.Rendering
{
    //-----------------------------------
    //sample GlyphPathBuilder :
    //for your flexiblity of glyph path builder.
    //-----------------------------------


    public abstract class GlyphPathBuilderBase
    {
        readonly Typeface _typeface;
        TrueTypeInterpreter _trueTypeInterpreter;
        protected GlyphPointF[] _outputGlyphPoints;
        protected ushort[] _outputContours;
        /// <summary>
        /// scale for converting latest glyph points to latest request font size
        /// </summary>
        float _recentPixelScale;
        bool _useInterpreter;
        public GlyphPathBuilderBase(Typeface typeface)
        {
            _typeface = typeface;
            this.UseTrueTypeInstructions = false;//default?
            _trueTypeInterpreter = new TrueTypeInterpreter();
            _trueTypeInterpreter.SetTypeFace(typeface);
            _recentPixelScale = 1;
        }
        public Typeface Typeface { get { return _typeface; } }
        /// <summary>
        /// use Maxim's Agg Vertical Hinting
        /// </summary>
        public bool UseVerticalHinting { get; set; }
        /// <summary>
        /// process glyph with true type instructions
        /// </summary>
        public bool UseTrueTypeInstructions
        {
            get { return _useInterpreter; }
            set
            {
                _useInterpreter = value;
            }
        }

        /// <summary>
        /// build glyph shape from glyphIndex and 
        /// </summary>
        /// <param name="glyphIndex"></param>
        /// <param name="sizeInPoints"></param>
        public void BuildFromGlyphIndex(ushort glyphIndex, float sizeInPoints)
        {
            //
            Glyph glyph = _typeface.GetGlyphByIndex(glyphIndex);
            this._outputGlyphPoints = glyph.GlyphPoints;
            this._outputContours = glyph.EndPoints;

            if ((RecentFontSizeInPixels = Typeface.ConvPointsToPixels(sizeInPoints)) < 0)
            {
                //convert to pixel size
                //if size< 0 then set _recentPixelScale = 1;
                //mean that no scaling at all, we use original point value
                _recentPixelScale = 1;
            }
            else
            {
                _recentPixelScale = Typeface.CalculateToPixelScale(RecentFontSizeInPixels);
            }
            //-------------------------------------
            FitCurrentGlyph(glyphIndex, glyph);
        }
        protected float RecentFontSizeInPixels { get; private set; }
        protected virtual void FitCurrentGlyph(ushort glyphIndex, Glyph glyph)
        {
            if (RecentFontSizeInPixels > 0 && UseTrueTypeInstructions &&
                  this._typeface.HasPrepProgramBuffer &&
                  glyph.HasGlyphInstructions)
            {
                _trueTypeInterpreter.UseVerticalHinting = this.UseVerticalHinting;
                //output as points,
                this._outputGlyphPoints = _trueTypeInterpreter.HintGlyph(glyphIndex, RecentFontSizeInPixels);
                //***
                //all points are scaled from _trueTypeInterpreter, 
                //so not need further scale.=> set _recentPixelScale=1
                _recentPixelScale = 1;
            } 
        }
        public virtual void ReadShapes(IGlyphTranslator tx)
        {
            //read output from glyph points
            tx.Read(this._outputGlyphPoints, this._outputContours, _recentPixelScale);
        }
    }

    public static class GlyphPathBuilderExtensions
    {
        public static void Build(this GlyphPathBuilder builder, char c, float sizeInPoints)
        {
            builder.BuildFromGlyphIndex((ushort)builder.Typeface.LookupIndex(c), sizeInPoints);
        }
        public static void SetHintTechnique(this GlyphPathBuilder builder, HintTechnique hintTech)
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
}