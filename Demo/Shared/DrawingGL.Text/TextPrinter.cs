//MIT, 2017, Zou Wei(github/zwcloud), WinterDev
using System.Collections.Generic;
using Typography.OpenFont;
using Typography.TextLayout;
using Typography.Contours;
using Tesselate;

namespace DrawingGL.Text
{
    /// <summary>
    /// text printer
    /// </summary>
    class TextPrinter : TextPrinterBase
    {
        //funcs:
        //1. layout glyph
        //2. measure glyph
        //3. generate glyph runs into textrun 
        GlyphTranslatorToPath _pathTranslator;
        string _currentFontFile;
        GlyphPathBuilder _currentGlyphPathBuilder;

        //
        // for tess
        // 
        SimpleCurveFlattener _curveFlattener;
        Tesselate.TessTool _tessTool;

        Typeface _currentTypeface;

        //-------------
        struct ProcessedGlyph
        {
            public readonly float[] tessData;
            public readonly ushort vertextCount;
            public ProcessedGlyph(float[] tessData, ushort vertextCount)
            {
                this.tessData = tessData;
                this.vertextCount = vertextCount;
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

            _tessTool = new Tesselate.TessTool();
        }


        public override void DrawFromGlyphPlans(GlyphPlanSequence glyphPlanList, int startAt, int len, float x, float y)
        {
            throw new System.NotImplementedException();
        }
        public override GlyphLayout GlyphLayoutMan { get; } = new GlyphLayout();

        public override Typeface Typeface
        {
            get { return _currentTypeface; }
            set
            {
                _currentTypeface = value;
                GlyphLayoutMan.Typeface = value;
            }
        }
        public MeasuredStringBox Measure(char[] textBuffer, int startAt, int len)
        {
            return GlyphLayoutMan.LayoutAndMeasureString(
                textBuffer, startAt, len,
                this.FontSizeInPoints
                );
        }

        /// <summary>
        /// Font file path
        /// </summary>
        public string FontFilename
        {
            get { return _currentFontFile; }
            set
            {
                if (_currentFontFile != value)
                {
                    _currentFontFile = value;

                    //TODO: review here
                    using (var stream = Utility.ReadFile(value))
                    {
                        var reader = new OpenFontReader();
                        Typeface = reader.Read(stream);
                    }

                    //2. glyph builder
                    _currentGlyphPathBuilder = new GlyphPathBuilder(Typeface);
                    _currentGlyphPathBuilder.TrueTypeHintTechnique = this.HintTechnique;

                    //3. glyph translater
                    _pathTranslator = new GlyphTranslatorToPath();

                    //4. Update GlyphLayout
                    GlyphLayoutMan.ScriptLang = this.ScriptLang;
                    GlyphLayoutMan.PositionTechnique = this.PositionTechnique;
                    GlyphLayoutMan.EnableLigature = this.EnableLigature;
                }
            }
        }


        UnscaledGlyphPlanList _resuableGlyphPlanList = new UnscaledGlyphPlanList();

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
            outputTextRun.typeface = this.Typeface;
            outputTextRun.sizeInPoints = sizeInPoints;

            //in this version we store original glyph into the mesh collection
            //and then we scale it later, so I just specific font size=0 (you can use any value)
            _glyphMeshCollection.SetCacheInfo(this.Typeface, 0, this.HintTechnique);


            GlyphLayoutMan.Typeface = this.Typeface;
            GlyphLayoutMan.Layout(charBuffer, start, len);

            float pxscale = this.Typeface.CalculateScaleToPixelFromPointSize(sizeInPoints);

            _resuableGlyphPlanList.Clear();
            GenerateGlyphPlan(charBuffer, 0, charBuffer.Length, _resuableGlyphPlanList);

            // render each glyph 
            int planCount = _resuableGlyphPlanList.Count;
            for (var i = 0; i < planCount; ++i)
            {

                _pathTranslator.Reset();
                //----
                //glyph path 
                //---- 
                UnscaledGlyphPlan glyphPlan = _resuableGlyphPlanList[i];
                //
                //1. check if we have this glyph in cache?
                //if yes, not need to build it again 
                ProcessedGlyph processGlyph;
                float[] tessData = null;

                if (!_glyphMeshCollection.TryGetCacheGlyph(glyphPlan.glyphIndex, out processGlyph))
                {
                    //if not found the  create a new one and register it
                    var writablePath = new WritablePath();
                    _pathTranslator.SetOutput(writablePath);
                    _currentGlyphPathBuilder.BuildFromGlyphIndex(glyphPlan.glyphIndex, sizeInPoints);
                    _currentGlyphPathBuilder.ReadShapes(_pathTranslator);

                    //-------
                    //do tess  
                    int[] endContours;
                    float[] flattenPoints = _curveFlattener.Flatten(writablePath._points, out endContours);

                    tessData = _tessTool.TessAsTriVertexArray(flattenPoints, endContours, out int vertexCount);
                    processGlyph = new ProcessedGlyph(tessData, (ushort)vertexCount);

                    _glyphMeshCollection.RegisterCachedGlyph(glyphPlan.glyphIndex, processGlyph);
                }

                outputTextRun.AddGlyph(
                    new GlyphRun(glyphPlan,
                        processGlyph.tessData,
                        processGlyph.vertextCount));
            }
        }
        public override void DrawString(char[] textBuffer, int startAt, int len, float x, float y)
        {

        }
        public override void DrawCaret(float x, float y)
        {

        }
    }


}