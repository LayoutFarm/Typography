//MIT, 2014-present, WinterDev 
using System;
using System.Collections.Generic;


using Typography.OpenFont;
using Typography.OpenFont.Extensions;
using Typography.Contours;

namespace PixelFarm.Drawing.Fonts
{
    class NOpenFontFace : FontFace
    {
        readonly string _name;
        readonly string _path;
        Typeface _typeface;

        //TODO: review again,
        //remove ...
        GlyphOutlineBuilder _glyphPathBuilder;

        public NOpenFontFace(Typeface typeface, string fontName, string fontPath)
        {
            _typeface = typeface;
            _name = fontName;
            _path = fontPath;

            _glyphPathBuilder = new GlyphOutlineBuilder(typeface);
        }
        public override string Name => _name;

        public override string FontPath => _path;

        protected override void OnDispose() { }
        public override ActualFont GetFontAtPointSize(float pointSize)
        {
            return new NOpenFont(this, pointSize, FontStyle.Regular);
        }
        public Typeface Typeface => _typeface;

        //TODO: review again,
        //remove ...
        internal GlyphOutlineBuilder VxsBuilder => _glyphPathBuilder;

        public override float GetScale(float pointSize) => _typeface.CalculateScaleToPixelFromPointSize(pointSize);

        public override int AscentInDzUnit => _typeface.Ascender;

        public override int DescentInDzUnit => _typeface.Descender;

        public override int LineGapInDzUnit => _typeface.LineGap;

        public override int RecommendedLineHeight => _typeface.CalculateRecommendLineSpacing();

        public override object GetInternalTypeface() => _typeface;

    }

    class NOpenFont : ActualFont
    {
        NOpenFontFace _ownerFace;
        readonly float _sizeInPoints;
        readonly FontStyle _style;
        readonly Typeface _typeFace;
        readonly float _scale;
        readonly Dictionary<uint, VertexStore> _glyphVxs = new Dictionary<uint, VertexStore>();

        float _recommendLineSpacing;

        public NOpenFont(NOpenFontFace ownerFace, float sizeInPoints, FontStyle style)
        {
            _ownerFace = ownerFace;
            _sizeInPoints = sizeInPoints;
            _style = style;
            _typeFace = ownerFace.Typeface;
            //calculate scale *** 
            _scale = _typeFace.CalculateScaleToPixelFromPointSize(sizeInPoints);
            _recommendLineSpacing = _typeFace.CalculateRecommendLineSpacing() * _scale;

        }
        public override float SizeInPoints => _sizeInPoints;

        public override float SizeInPixels => _sizeInPoints * _scale;

        public override float AscentInPixels => _typeFace.Ascender * _scale;

        public override float DescentInPixels => _typeFace.Descender * _scale;

        public override float LineGapInPixels => _typeFace.LineGap * _scale;

        public override float RecommendedLineSpacingInPixels => _recommendLineSpacing;

        public override FontFace FontFace => _ownerFace;

        public override string FontName => _typeFace.Name;

        public override FontStyle FontStyle => _style;

        public override FontGlyph GetGlyph(char c) => GetGlyphByIndex(_typeFace.LookupIndex(c));

        public override FontGlyph GetGlyphByIndex(ushort glyphIndex)
        {
            //1.  
            FontGlyph fontGlyph = new FontGlyph();
            fontGlyph.flattenVxs = GetGlyphVxs(glyphIndex);
            fontGlyph.horiz_adv_x = _typeFace.GetHAdvanceWidthFromGlyphIndex(glyphIndex);

            return fontGlyph;
        }
        protected override void OnDispose()
        {

        }
        public VertexStore GetGlyphVxs(uint codepoint)
        {
            if (_glyphVxs.TryGetValue(codepoint, out VertexStore found))
            {
                return found;
            }
            //not found
            //then build it
            _ownerFace.VxsBuilder.BuildFromGlyphIndex((ushort)codepoint, _sizeInPoints);

            var txToVxs = new Fonts.GlyphTranslatorToVxs();
            _ownerFace.VxsBuilder.ReadShapes(txToVxs);
            //
            //create new one
            found = new VertexStore();
            txToVxs.WriteOutput(found);
            _glyphVxs.Add(codepoint, found);
            return found;
        }

    }
}

