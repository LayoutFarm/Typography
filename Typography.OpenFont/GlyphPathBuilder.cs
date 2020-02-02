using System;
namespace Typography.OpenFont
{
    [Flags]
    public enum TrueTypeHintTechnique : byte
    {
        /// <summary>no hinting</summary>
        None,
        /// <summary>process glyph with true type instructions</summary>
        Instructions,
        /// <summary>custom hint</summary>
        CustomAutoFit,
        /// <summary>
        /// use Maxim's Agg Vertical Hinting to process glyph with true type instructions
        /// </summary>
        Instructions_VerticalOnly
    }
    public class GlyphPathBuilder
    {
        TrueTypeInterpreter _trueTypeInterpreter;
        protected GlyphPointF[] _outputGlyphPoints;
        protected ushort[] _outputContours;

        protected CFF.Cff1Font _ownerCff;
        protected CFF.Cff1GlyphData _cffGlyphData;

        /// <summary>
        /// scale for converting latest glyph points to latest request font size
        /// </summary>
        float _recentPixelScale;
        readonly CFF.CffEvaluationEngine _cffEvalEngine;

        public GlyphPathBuilder(Typeface typeface)
        {
            Typeface = typeface;
            this.TrueTypeHintTechnique = TrueTypeHintTechnique.Instructions;//default?
            _recentPixelScale = 1;

            if (typeface.IsCffFont)
            {
                _cffEvalEngine = new CFF.CffEvaluationEngine();
            }
        }
        public Typeface Typeface { get; private set; }
        /// <summary>
        /// process glyph with true type instructions
        /// </summary>
        public bool UseTrueTypeInstructions =>
            (TrueTypeHintTechnique & TrueTypeHintTechnique.Instructions) != 0;
        /// <summary>
        /// use Maxim's Agg Vertical Hinting
        /// </summary>
        public bool UseTrueTypeVerticalHinting =>
            (TrueTypeHintTechnique & TrueTypeHintTechnique.CustomAutoFit) != 0;
        public TrueTypeHintTechnique TrueTypeHintTechnique { get; set; }
        public void Build(char c, float sizeInPoints) =>
            BuildFromGlyphIndex(Typeface.LookupIndex(c), sizeInPoints);
        /// <summary>
        /// build glyph shape from glyphIndex to be read
        /// </summary>
        /// <param name="glyphIndex"></param>
        /// <param name="sizeInPoints"></param>
        public void BuildFromGlyphIndex(ushort glyphIndex, float sizeInPoints) =>
            BuildFromGlyph(Typeface.GetGlyphByIndex(glyphIndex), sizeInPoints);
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
                _ownerCff = glyph.GetOwnerCff();
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
            if (RecentFontSizeInPixels > 0 &&
                UseTrueTypeInstructions &&
                Typeface.HasPrepProgramBuffer &&
                glyph.HasGlyphInstructions)
            {
                _trueTypeInterpreter ??= new TrueTypeInterpreter(Typeface);
                _trueTypeInterpreter.UseVerticalHinting = this.UseTrueTypeVerticalHinting;
                //output as points,
                _outputGlyphPoints = _trueTypeInterpreter.HintGlyph(glyph.GlyphIndex, RecentFontSizeInPixels);
                //***
                //all points are scaled from _trueTypeInterpreter, 
                //so not need further scale.=> set _recentPixelScale=1
                _recentPixelScale = 1;
            }
        }

        public virtual void ReadShapes(IGlyphTranslator tx) {
            //read output from glyph points
            if (_cffGlyphData != null)
            {
                _cffEvalEngine.Run(tx, _ownerCff, _cffGlyphData, _recentPixelScale);
            }
            else
            {
                tx.Read(_outputGlyphPoints, _outputContours, _recentPixelScale);
            }
        }
    }
}