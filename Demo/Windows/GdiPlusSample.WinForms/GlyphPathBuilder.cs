//MIT, 2016-2017, WinterDev

using Typography.OpenFont;
namespace SampleWinForms
{
    //-----------------------------------
    //sample GlyphPathBuilder :
    //for your flexiblity of glyph path builder.
    //-----------------------------------

    public class GlyphPathBuilder
    {
        readonly Typeface _typeface;
        TrueTypeInterpreter _trueTypeInterpreter;
        GlyphPointF[] _outputGlyphPoints;
        ushort[] _outputContours;

        bool _useInterpreter;
        bool _passInterpreterModule;

        public GlyphPathBuilder(Typeface typeface)
        {
            _typeface = typeface;
            this.UseTrueTypeInstructions = false;//default?
            _trueTypeInterpreter = new TrueTypeInterpreter();
            _trueTypeInterpreter.SetTypeFace(typeface);
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
        public bool PassHintInterpreterModule
        {
            get { return this._passInterpreterModule; }
        }

        public void Build(char c, float sizeInPoints)
        {
            BuildFromGlyphIndex((ushort)_typeface.LookupIndex(c), sizeInPoints);
        }
        public void BuildFromGlyphIndex(ushort glyphIndex, float sizeInPoints)
        {
            this.SizeInPoints = sizeInPoints;
            Build(glyphIndex, _typeface.GetGlyphByIndex(glyphIndex));
        }

        void Build(ushort glyphIndex, Glyph glyph)
        {
            //-------------------------------------------
            //1. start with original points/contours from glyph
            this._outputGlyphPoints = glyph.GlyphPoints;
            this._outputContours = glyph.EndPoints;
            //-------------------------------------------  
            _passInterpreterModule = false;
            Typeface currentTypeFace = this._typeface;

            //2. process glyph points
            if (UseTrueTypeInstructions &&
                currentTypeFace.HasPrepProgramBuffer &&
                glyph.HasGlyphInstructions)
            {

                GlyphPointF[] newGlyphPoints = _trueTypeInterpreter.HintGlyph(glyphIndex, SizeInPoints);
                this._outputGlyphPoints = newGlyphPoints;
                _passInterpreterModule = true;
            }
            else
            {

                //not use interperter so we need to scale it with our machnism
                //this demonstrate our auto hint engine ***
                //you can change this to your own hint engine*** 
            }
        }
        public void ReadShapes(IGlyphTranslator tx)
        {
            tx.Read(this._outputGlyphPoints, this._outputContours);
        }

        public float GetPixelScale()
        {
            return _typeface.CalculateFromPointToPixelScale(SizeInPoints);
        }
        public GlyphPointF[] GetOutputPoints()
        {
            return this._outputGlyphPoints;
        }
        public ushort[] GetOutputContours()
        {
            return this._outputContours;
        }
    }

}