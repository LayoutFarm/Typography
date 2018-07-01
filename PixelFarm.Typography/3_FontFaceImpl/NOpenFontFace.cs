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
        readonly string name, path;
        Typeface typeface;
        //TODO: review again,
        //remove ...
        GlyphPathBuilder glyphPathBuilder;

        public NOpenFontFace(Typeface typeface, string fontName, string fontPath)
        {
            this.typeface = typeface;
            this.name = fontName;
            this.path = fontPath;

            glyphPathBuilder = new GlyphPathBuilder(typeface);
        }
        public override string Name
        {
            get { return name; }
        }
        public override string FontPath
        {
            get { return path; }
        }
        protected override void OnDispose() { }
        public override ActualFont GetFontAtPointSize(float pointSize)
        {
            NOpenFont actualFont = new NOpenFont(this, pointSize, FontStyle.Regular);
            return actualFont;
        }
        public Typeface Typeface { get { return this.typeface; } }

        internal GlyphPathBuilder VxsBuilder
        {
            get
            {
                //TODO: review again,
                //remove ...
                return this.glyphPathBuilder;
            }
        }
        public override float GetScale(float pointSize)
        {
            return typeface.CalculateScaleToPixelFromPointSize(pointSize);
        }
        public override int AscentInDzUnit
        {
            get { return typeface.Ascender; }
        }
        public override int DescentInDzUnit
        {
            get { return typeface.Descender; }
        }
        public override int LineGapInDzUnit
        {
            get { return typeface.LineGap; }
        }

        public override int RecommendedLineHeight
        {
            get
            {
                return typeface.CalculateRecommendLineSpacing();
            }
        }

        public override object GetInternalTypeface()
        {
            return typeface;
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

