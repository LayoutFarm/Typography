using System.Collections.Generic;
using Typography.OpenFont;
using Typography.TextLayout;

namespace Typography.Rendering
{
    /// <summary>
    /// text printer
    /// </summary>
    class TextPrinter
    {
        private readonly GlyphLayout glyphLayout = new GlyphLayout();

        private readonly List<GlyphPlan> outputGlyphPlans = new List<GlyphPlan>();
        private GlyphTranslatorToPath pathTranslator;
        private string currentFontFile;
        private GlyphPathBuilder currentGlyphPathBuilder;

        public TextPrinter()
        {
            FontSizeInPoints = 14;
            ScriptLang = ScriptLangs.Latin;
        }

        /// <summary>
        /// Font file path
        /// </summary>
        public string FontFilename
        {
            get { return currentFontFile; }
            set
            {
                if (currentFontFile != value)
                {
                    currentFontFile = value;

                    using (var fs = Xamarin.OpenGL.Utility.ReadFile(currentFontFile))
                    {
                        var reader = new OpenFontReader();
                        CurrentTypeFace = reader.Read(fs);
                    }

                    //2. glyph builder
                    currentGlyphPathBuilder = new GlyphPathBuilder(CurrentTypeFace);
                    currentGlyphPathBuilder.UseTrueTypeInstructions = false; //reset
                    currentGlyphPathBuilder.UseVerticalHinting = false; //reset
                    switch (this.HintTechnique)
                    {
                        case HintTechnique.TrueTypeInstruction:
                            currentGlyphPathBuilder.UseTrueTypeInstructions = true;
                            break;
                        case HintTechnique.TrueTypeInstruction_VerticalOnly:
                            currentGlyphPathBuilder.UseTrueTypeInstructions = true;
                            currentGlyphPathBuilder.UseVerticalHinting = true;
                            break;
                        case HintTechnique.CustomAutoFit:
                            //custom agg autofit 
                            break;
                    }

                    //3. glyph translater
                    pathTranslator = new GlyphTranslatorToPath();

                    //4. Update GlyphLayout
                    glyphLayout.ScriptLang = this.ScriptLang;
                    glyphLayout.PositionTechnique = this.PositionTechnique;
                    glyphLayout.EnableLigature = this.EnableLigature;
                }
            }
        }

        public HintTechnique HintTechnique { get; set; }
        public float FontSizeInPoints { get; set; }
        public ScriptLang ScriptLang { get; set; }
        public PositionTechnique PositionTechnique { get; set; }
        public bool EnableLigature { get; set; }
        public Typeface CurrentTypeFace { get; private set; }

        /// <summary>
        /// draw glyph as paths
        /// </summary>
        /// <param name="g">the path builder used to draw the path</param>
        /// <param name="text">text</param>
        /// <param name="x">offset x</param>
        /// <param name="y">offset y</param>
        public void Draw(ITextPathBuilder g, string text, float x, float y)
        {
            // layout glyphs with selected layout technique
            var sizeInPoints = this.FontSizeInPoints;
            outputGlyphPlans.Clear();
            glyphLayout.Typeface = this.CurrentTypeFace;
            glyphLayout.GenerateGlyphPlans(text.ToCharArray(), 0, text.Length, outputGlyphPlans, null);

            // render each glyph
            var scale = CurrentTypeFace.CalculateToPixelScaleFromPointSize(sizeInPoints);
            pathTranslator.PathBuilder = g;
            for (var i = 0; i < outputGlyphPlans.Count; ++i)
            {
                pathTranslator.Reset();
                var glyphPlan = outputGlyphPlans[i];
                currentGlyphPathBuilder.BuildFromGlyphIndex(glyphPlan.glyphIndex, sizeInPoints);
                currentGlyphPathBuilder.ReadShapes(pathTranslator, sizeInPoints, x + glyphPlan.x * scale, y + glyphPlan.y * scale);
            }
        }

        public void Measure(string text, int startAt, int len, out float width, out float height)
        {
            glyphLayout.Typeface = this.CurrentTypeFace;
            var scale = CurrentTypeFace.CalculateToPixelScaleFromPointSize(this.FontSizeInPoints);
            MeasuredStringBox strBox;
            glyphLayout.MeasureString(text.ToCharArray(), startAt, len, out strBox, scale);
            width = strBox.width;
            height = strBox.CalculateLineHeight();
        }

    }
}