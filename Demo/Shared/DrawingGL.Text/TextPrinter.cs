//MIT, 2017, Zou Wei(github/zwcloud), WinterDev
using System.Collections.Generic;
using Typography.OpenFont;
using Typography.TextLayout;
using Typography.Contours;

namespace DrawingGL.Text
{
    /// <summary>
    /// text printer
    /// </summary>
    class TextPrinter
    {
        //funcs:
        //1. layout glyph
        //2. measure glyph
        //3. generate glyph runs into textrun


        readonly GlyphLayout glyphLayout = new GlyphLayout();
        readonly GlyphPlanList outputGlyphPlans = new GlyphPlanList();
        GlyphTranslatorToPath pathTranslator;
        string currentFontFile;
        GlyphPathBuilder currentGlyphPathBuilder;

        //
        // for tess
        // 
        SimpleCurveFlattener _curveFlattener;
        TessTool _tessTool;

        //-------------
        struct ProcessedGlyph
        {
            public readonly float[] tessData;
            public readonly ushort tessNElements;
            public ProcessedGlyph(float[] tessData, ushort tessNElements)
            {
                this.tessData = tessData;
                this.tessNElements = tessNElements;
            }
        }
        GlyphMeshCollection<ProcessedGlyph> _glyphMeshCollection = new GlyphMeshCollection<ProcessedGlyph>();
        //-------------
        public TextPrinter()
        {
            FontSizeInPoints = 14;
            ScriptLang = ScriptLangs.Latin;

            //
            _curveFlattener = new SimpleCurveFlattener();

            _tessTool = new TessTool();
        }

        GlyphPlanList _reusableGlyphPlanList = new GlyphPlanList();
        public MeasuredStringBox Measure(char[] textBuffer, int startAt, int len)
        {
            glyphLayout.Typeface = this.CurrentTypeFace;
            float pxscale = CurrentTypeFace.CalculateScaleToPixelFromPointSize(this.FontSizeInPoints);
            glyphLayout.Layout(textBuffer, startAt, len);

            _reusableGlyphPlanList.Clear();
            IGlyphPositions glyphPositions = glyphLayout.ResultUnscaledGlyphPositions;
            GlyphLayoutExtensions.GenerateGlyphPlan(glyphLayout.ResultUnscaledGlyphPositions,
                pxscale,
                false, _reusableGlyphPlanList);
            return new MeasuredStringBox(
                 _reusableGlyphPlanList.AccumAdvanceX * pxscale,
                  CurrentTypeFace.Ascender * pxscale,
                  CurrentTypeFace.Descender * pxscale,
                  CurrentTypeFace.LineGap * pxscale,
                  Typography.OpenFont.Extensions.TypefaceExtensions.CalculateRecommendLineSpacing(CurrentTypeFace) * pxscale);


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

                    //TODO: review here
                    using (var stream = Utility.ReadFile(value))
                    {
                        var reader = new OpenFontReader();
                        CurrentTypeFace = reader.Read(stream);
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
        /// generate glyph run into a given textRun
        /// </summary>
        /// <param name="outputTextRun"></param>
        /// <param name="charBuffer"></param>
        /// <param name="start"></param>
        /// <param name="len"></param>
        public void GenerateGlyphRuns(TextRun outputTextRun, char[] charBuffer, int start, int len)
        {
            // layout glyphs with selected layout technique
            float sizeInPoints = this.FontSizeInPoints;
            outputTextRun.typeface = this.CurrentTypeFace;
            outputTextRun.sizeInPoints = sizeInPoints;

            //in this version we store original glyph into the mesh collection
            //and then we scale it later, so I just specific font size=0 (you can use any value)
            _glyphMeshCollection.SetCacheInfo(this.CurrentTypeFace, 0, this.HintTechnique);


            glyphLayout.Typeface = this.CurrentTypeFace;
            glyphLayout.Layout(charBuffer, start, len);

            float pxscale = this.CurrentTypeFace.CalculateScaleToPixelFromPointSize(sizeInPoints);

            outputGlyphPlans.Clear();
            GlyphLayoutExtensions.GenerateGlyphPlan(
                glyphLayout.ResultUnscaledGlyphPositions,
                pxscale, false, outputGlyphPlans);

            // render each glyph 
            int planCount = outputGlyphPlans.Count;
            for (var i = 0; i < planCount; ++i)
            {

                pathTranslator.Reset();
                //----
                //glyph path 
                //---- 
                GlyphPlan glyphPlan = outputGlyphPlans[i];
                //
                //1. check if we have this glyph in cache?
                //if yes, not need to build it again 
                ProcessedGlyph processGlyph;
                float[] tessData = null;

                if (!_glyphMeshCollection.TryGetCacheGlyph(glyphPlan.glyphIndex, out processGlyph))
                {
                    //if not found the  create a new one and register it
                    var writablePath = new WritablePath();
                    pathTranslator.SetOutput(writablePath);
                    currentGlyphPathBuilder.BuildFromGlyphIndex(glyphPlan.glyphIndex, sizeInPoints);
                    currentGlyphPathBuilder.ReadShapes(pathTranslator);

                    //-------
                    //do tess  
                    int[] endContours;
                    float[] flattenPoints = _curveFlattener.Flatten(writablePath._points, out endContours);
                    int nTessElems;
                    tessData = _tessTool.TessPolygon(flattenPoints, endContours, out nTessElems);
                    //-------
                    processGlyph = new ProcessedGlyph(tessData, (ushort)nTessElems);
                    _glyphMeshCollection.RegisterCachedGlyph(glyphPlan.glyphIndex, processGlyph);
                }

                outputTextRun.AddGlyph(
                    new GlyphRun(glyphPlan,
                        processGlyph.tessData,
                        processGlyph.tessNElements));
            }
        }
    }
}