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
        /// specific output glyph size (in points)
        /// </summary>
        public float SizeInPoints
        {
            get;
            private set;
        }
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
        public void BuildFromGlyphIndex(ushort glyphIndex, float sizeInPoints)
        {

            this.SizeInPoints = sizeInPoints;
            //
            Glyph glyph = _typeface.GetGlyphByIndex(glyphIndex);
            //
            this._outputGlyphPoints = glyph.GlyphPoints;
            this._outputContours = glyph.EndPoints;
            //
            Typeface currentTypeFace = this._typeface;
            _recentPixelScale = this._typeface.CalculateFromPointToPixelScale(SizeInPoints); //***

            FitCurrentGlyph(glyphIndex, glyph);
        }
        protected virtual void FitCurrentGlyph(ushort glyphIndex, Glyph glyph)
        {
            //2. process glyph points
            if (UseTrueTypeInstructions &&
                this._typeface.HasPrepProgramBuffer &&
                glyph.HasGlyphInstructions)
            {
                _trueTypeInterpreter.UseVerticalHinting = this.UseVerticalHinting;
                //output as points,
                this._outputGlyphPoints = _trueTypeInterpreter.HintGlyph(glyphIndex, SizeInPoints);
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
        protected float RecentPixelScale { get { return _recentPixelScale; } }
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