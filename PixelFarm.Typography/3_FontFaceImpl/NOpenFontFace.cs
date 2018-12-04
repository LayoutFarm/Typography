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
        GlyphPathBuilder _glyphPathBuilder;

        public NOpenFontFace(Typeface typeface, string fontName, string fontPath)
        {
            _typeface = typeface;
            _name = fontName;
            _path = fontPath;

            _glyphPathBuilder = new GlyphPathBuilder(typeface);
        }
        public override string Name
        {
            get { return _name; }
        }
        public override string FontPath
        {
            get { return _path; }
        }
        protected override void OnDispose() { }
        public override ActualFont GetFontAtPointSize(float pointSize)
        {
            NOpenFont actualFont = new NOpenFont(this, pointSize, FontStyle.Regular);
            return actualFont;
        }
        public Typeface Typeface { get { return _typeface; } }

        internal GlyphPathBuilder VxsBuilder
        {
            get
            {
                //TODO: review again,
                //remove ...
                return _glyphPathBuilder;
            }
        }
        public override float GetScale(float pointSize)
        {
            return _typeface.CalculateScaleToPixelFromPointSize(pointSize);
        }
        public override int AscentInDzUnit
        {
            get { return _typeface.Ascender; }
        }
        public override int DescentInDzUnit
        {
            get { return _typeface.Descender; }
        }
        public override int LineGapInDzUnit
        {
            get { return _typeface.LineGap; }
        }

        public override int RecommendedLineHeight
        {
            get
            {
                return _typeface.CalculateRecommendLineSpacing();
            }
        }

        public override object GetInternalTypeface()
        {
            return _typeface;
        }
    }

    class NOpenFont : ActualFont
    {
        NOpenFontFace ownerFace;
        float sizeInPoints;
        FontStyle style;
        Typeface typeFace;
        float scale;
        Dictionary<uint, VertexStore> glyphVxs = new Dictionary<uint, VertexStore>();

        float _recommendLineSpacing;

        public NOpenFont(NOpenFontFace ownerFace, float sizeInPoints, FontStyle style)
        {
            this.ownerFace = ownerFace;
            this.sizeInPoints = sizeInPoints;
            this.style = style;
            this.typeFace = ownerFace.Typeface;
            //calculate scale *** 
            scale = typeFace.CalculateScaleToPixelFromPointSize(sizeInPoints);
            _recommendLineSpacing = typeFace.CalculateRecommendLineSpacing() * scale;

        }
        public override float SizeInPoints
        {
            get { return this.sizeInPoints; }
        }
        public override float SizeInPixels
        {
            //font height 
            get { return sizeInPoints * scale; }
        }
        public override float AscentInPixels
        {
            get { return typeFace.Ascender * scale; }
        }
        public override float DescentInPixels
        {
            get { return typeFace.Descender * scale; }
        }
        public override float LineGapInPixels
        {
            get { return typeFace.LineGap * scale; }
        }
        public override float RecommendedLineSpacingInPixels
        {
            get
            {
                return _recommendLineSpacing;
            }
        }
        public override FontFace FontFace
        {
            get { return ownerFace; }
        }
        public override string FontName
        {
            get { return typeFace.Name; }
        }
        public override FontStyle FontStyle
        {
            get { return style; }
        }


        public override FontGlyph GetGlyph(char c)
        {
            return GetGlyphByIndex(typeFace.LookupIndex(c));
        }
        public override FontGlyph GetGlyphByIndex(ushort glyphIndex)
        {
            //1.  
            FontGlyph fontGlyph = new FontGlyph();
            fontGlyph.flattenVxs = GetGlyphVxs(glyphIndex);
            fontGlyph.horiz_adv_x = typeFace.GetHAdvanceWidthFromGlyphIndex(glyphIndex);

            return fontGlyph;
        }
        protected override void OnDispose()
        {

        }
        public VertexStore GetGlyphVxs(uint codepoint)
        {
            VertexStore found;
            if (glyphVxs.TryGetValue(codepoint, out found))
            {
                return found;
            }
            //not found
            //then build it
            ownerFace.VxsBuilder.BuildFromGlyphIndex((ushort)codepoint, this.sizeInPoints);

            var txToVxs = new Fonts.GlyphTranslatorToVxs();
            ownerFace.VxsBuilder.ReadShapes(txToVxs);
            //
            //create new one
            found = new VertexStore();
            txToVxs.WriteOutput(found);
            glyphVxs.Add(codepoint, found);
            return found;
        }

    }
}

