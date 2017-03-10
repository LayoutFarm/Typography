//MIT, 2016-2017, WinterDev

using Typography.OpenFont;
using System.Collections.Generic;

namespace Typography.Rendering
{
    //-----------------------------------
    //sample GlyphPathBuilder :
    //for your flexiblity of glyph path builder.
    //-----------------------------------



    public class GlyphPathBuilder
    {
        readonly Typeface _typeface;
        TrueTypeInterpreter _trueTypeInterpreter;
        GlyphFitOutlineAnalyzer _fitShapeAnalyzer = new GlyphFitOutlineAnalyzer();
        Dictionary<ushort, GlyphFitOutline> _fitoutlineCollection = new Dictionary<ushort, GlyphFitOutline>();

        GlyphPointF[] _outputGlyphPoints;
        GlyphFitOutline _fitOutline;
        ushort[] _outputContours;
        float _recentPixelScale;
        bool _useInterpreter;
        bool _useAutoHint;

        public GlyphPathBuilder(Typeface typeface)
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

        public bool MinorAdjustFitYForAutoFit
        {
            get;
            set;
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
            Typeface currentTypeFace = this._typeface;
            _recentPixelScale = currentTypeFace.CalculateFromPointToPixelScale(SizeInPoints); //***
            _useAutoHint = false;//reset             
            //-------------------------------------------  
            //2. process glyph points
            if (UseTrueTypeInstructions &&
                currentTypeFace.HasPrepProgramBuffer &&
                glyph.HasGlyphInstructions)
            {
                _trueTypeInterpreter.UseVerticalHinting = this.UseVerticalHinting;
                //output as points,
                this._outputGlyphPoints = _trueTypeInterpreter.HintGlyph(glyphIndex, SizeInPoints);
                //all points are scaled from _trueTypeInterpreter, 
                //so not need further scale.=> set _recentPixelScale=1
                _recentPixelScale = 1;
            }
            else
            {
                //not use interperter so we need to scale it with our machnism
                //this demonstrate our auto hint engine ***
                //you can change this to your own hint engine***  
                if (this.UseVerticalHinting)
                {
                    _useAutoHint = true;
                    if (!_fitoutlineCollection.TryGetValue(glyphIndex, out _fitOutline))
                    {
                        _fitOutline = _fitShapeAnalyzer.Analyze(
                            this._outputGlyphPoints,
                            this._outputContours);
                        _fitoutlineCollection.Add(glyphIndex, _fitOutline);
                    }
                }
            }
        }
        public void ReadShapes(IGlyphTranslator tx)
        {
            if (_useAutoHint)
            {
                //read from our auto hint fitoutline
                //need scale from original.
                _fitOutline.ReadOutput(tx, _recentPixelScale);
            }
            else
            {
                //read output from glyph points
                tx.Read(this._outputGlyphPoints, this._outputContours, _recentPixelScale);
            }
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