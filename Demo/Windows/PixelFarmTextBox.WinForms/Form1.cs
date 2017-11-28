using System;
using System.Collections.Generic;
using System.ComponentModel;

using System.Drawing;
using System.Text;
using System.Windows.Forms;


using PixelFarm.Agg;
using PixelFarm.Drawing.Fonts;


using Typography.Rendering;
using Typography.Contours;
using Typography.TextLayout;
using Typography.TextServices;

namespace PixelFarmTextBox.WinForms
{
    public partial class Form1 : Form
    {
        Graphics g;
        AggCanvasPainter painter;
        ImageGraphics2D imgGfx2d;
        ActualImage destImg;
        Bitmap winBmp;

        DevTextPrinterBase selectedTextPrinter = null;
        VxsTextPrinter _devVxsTextPrinter = null;
        SampleWinForms.UI.SampleTextBoxControllerForPixelFarm _controllerForPixelFarm = new SampleWinForms.UI.SampleTextBoxControllerForPixelFarm();

        TypographyTest.BasicFontOptions _basicOptions;
        TypographyTest.GlyphRenderOptions _renderOptions;

        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            if (DesignMode) return;
            //
            _basicOptions = basicFontOptionsUserControl1.Options;
            _basicOptions.TypefaceChanged += (s, e2) =>
            {
                if (_devVxsTextPrinter != null)
                {
                    _devVxsTextPrinter.Typeface = e2.SelectedTypeface;
                }
            };
            _basicOptions.UpdateRenderOutput += (s, e2) =>
             {
                 UpdateRenderOutput();
             };
            //
            //---------- 
            _renderOptions = glyphRenderOptionsUserControl1.Options;
            _renderOptions.UpdateRenderOutput += (s, e2) =>
            {
                UpdateRenderOutput();
            };

            //share text printer to our sample textbox
            //but you can create another text printer that specific to text textbox control

            destImg = new ActualImage(800, 600, PixelFormat.ARGB32);
            imgGfx2d = new ImageGraphics2D(destImg); //no platform
            painter = new AggCanvasPainter(imgGfx2d);
            winBmp = new Bitmap(destImg.Width, destImg.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            g = this.sampleTextBox1.CreateGraphics();

            painter.CurrentFont = new PixelFarm.Drawing.RequestFont("tahoma", 14);

            _devVxsTextPrinter = new VxsTextPrinter(painter, _basicOptions.OpenFontStore);

            _devVxsTextPrinter.ScriptLang = _basicOptions.ScriptLang;
            _devVxsTextPrinter.PositionTechnique = Typography.TextLayout.PositionTechnique.OpenFont;
            _devVxsTextPrinter.FontSizeInPoints = 10;

            _controllerForPixelFarm.BindHostGraphics(g);
            _controllerForPixelFarm.TextPrinter = _devVxsTextPrinter;

            this.sampleTextBox1.SetController(_controllerForPixelFarm);
            _readyToRender = true;
            _basicOptions.UpdateRenderOutput += (s, e2) => UpdateRenderOutput();
            //----------
            //txtInputChar.TextChanged += (s, e2) => UpdateRenderOutput();
            //----------
        }
        bool _readyToRender;
        void UpdateRenderOutput()
        {
            if (!_readyToRender) return;
            //

            //test option use be used with lcd subpixel rendering.
            //this demonstrate how we shift a pixel for subpixel rendering tech
            _devVxsTextPrinter.UseWithLcdSubPixelRenderingTechnique = true;

            //1. read typeface from font file 
            TypographyTest.RenderChoice renderChoice = _basicOptions.RenderChoice;
            switch (renderChoice)
            {

                case TypographyTest.RenderChoice.RenderWithGdiPlusPath:
                    //not render in this example
                    //see more at ...
                    break;
                case TypographyTest.RenderChoice.RenderWithTextPrinterAndMiniAgg:
                    {
                        //clear previous draw
                        painter.Clear(PixelFarm.Drawing.Color.White);
                        painter.UseSubPixelRendering = false;
                        painter.FillColor = PixelFarm.Drawing.Color.Black;

                        selectedTextPrinter = _devVxsTextPrinter;
                        selectedTextPrinter.Typeface = _basicOptions.Typeface;
                        selectedTextPrinter.FontSizeInPoints = _basicOptions.FontSizeInPoints;
                        selectedTextPrinter.ScriptLang = _basicOptions.ScriptLang;
                        selectedTextPrinter.PositionTechnique = _basicOptions.PositionTech;

                        selectedTextPrinter.HintTechnique = HintTechnique.None;
                        selectedTextPrinter.EnableLigature = true;
                        _devVxsTextPrinter.UpdateGlyphLayoutSettings();
                        //
                        _controllerForPixelFarm.ReadyToRender = true;
                        _controllerForPixelFarm.UpdateOutput();

                        //----------------
                        //copy from Agg's memory buffer to gdi 
                        PixelFarm.Agg.Imaging.BitmapHelper.CopyToGdiPlusBitmapSameSize(destImg, winBmp);
                        g.Clear(Color.White);
                        g.DrawImage(winBmp, new Point(10, 0));

                    }
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {


            selectedTextPrinter = _devVxsTextPrinter;
            selectedTextPrinter.Typeface = _basicOptions.Typeface;
            selectedTextPrinter.FontSizeInPoints = _basicOptions.FontSizeInPoints;
            selectedTextPrinter.ScriptLang = _basicOptions.ScriptLang;
            selectedTextPrinter.PositionTechnique = _basicOptions.PositionTech;
            selectedTextPrinter.HintTechnique = HintTechnique.None;
            selectedTextPrinter.EnableLigature = true;
            _devVxsTextPrinter.UpdateGlyphLayoutSettings();

            //------- 
            var editableTextBlockLayoutEngine = new EditableTextBlockLayoutEngine();
            editableTextBlockLayoutEngine.DefaultTypeface = _basicOptions.Typeface;
            editableTextBlockLayoutEngine.FontSizeInPts = _basicOptions.FontSizeInPoints;
            editableTextBlockLayoutEngine.LoadText("ABCD\r\n   EFGH!");
            editableTextBlockLayoutEngine.DoLayout();

            //then we render the output to the screen  
            //see UpdateRenderOutput() code 
            //clear previous draw
            //----------------

            //-------------
            //pre-render
            painter.Clear(PixelFarm.Drawing.Color.White);
            painter.UseSubPixelRendering = false;
            painter.FillColor = PixelFarm.Drawing.Color.Black;
            _devVxsTextPrinter.TargetCanvasPainter = painter; //*** essential ***

            //
            //render eachline with painter
            List<EditableTextLine> textlines = editableTextBlockLayoutEngine.UnsafeGetEditableTextLine();
            int lineCount = textlines.Count;
            float x = 0;
            int y = 200;
            int lineSpacing = (int)_devVxsTextPrinter.FontLineSpacingPx;

            for (int i = 0; i < lineCount; ++i)
            {
                EditableTextLine line = textlines[i];
                List<IRun> runs = line.UnsageGetTextRunList();
                int runCount = runs.Count;

                for (int r = 0; r < runCount; ++r)
                {
                    IRun run = runs[r];
                    TextRun textRun = run as TextRun;
                    if (textRun == null) continue;
                    //
                    GlyphPlanSequence seq = textRun.GetGlyphPlanSeq();
                    _devVxsTextPrinter.DrawFromGlyphPlans(
                        GlyphPlanSequence.UnsafeGetInteralGlyphPlanList(seq),
                        seq.startAt,
                        seq.len,
                        x, y);
                    x += run.Width;
                    y -= lineSpacing; //next line?
                }
                x = 0;//reset at newline
            }
            //---------- 
            //use this util to copy image from Agg actual image to System.Drawing.Bitmap 
            PixelFarm.Agg.Imaging.BitmapHelper.CopyToGdiPlusBitmapSameSize(painter.Graphics.DestActualImage, winBmp);
            //----------------
            //copy from Agg's memory buffer to gdi 
            //PixelFarm.Agg.Imaging.BitmapHelper.CopyToGdiPlusBitmapSameSize(destImg, winBmp);
            g.Clear(Color.White);
            g.DrawImage(winBmp, new Point(10, 0));
        }


        TextServiceHub _textService;
        private void button2_Click(object sender, EventArgs e)
        {
            //test text service
            if (_textService == null)
            {
                _textService = new TextServiceHub();
            }

            Typography.TextServices.TextShapingService shapingService = _textService.ShapingService;
            shapingService.SetCurrentFont(_basicOptions.Typeface.Name, InstalledFontStyle.Normal, 8);
            shapingService.SetCurrentScriptLang(Typography.OpenFont.ScriptLangs.Latin);
            GlyphPlanSequence seq = shapingService.LayoutText("Hello");


            //----
            // test render the output
            selectedTextPrinter = _devVxsTextPrinter;
            selectedTextPrinter.Typeface = _basicOptions.Typeface;
            selectedTextPrinter.FontSizeInPoints = _basicOptions.FontSizeInPoints;
            selectedTextPrinter.ScriptLang = _basicOptions.ScriptLang;
            selectedTextPrinter.PositionTechnique = _basicOptions.PositionTech;
            selectedTextPrinter.HintTechnique = HintTechnique.None;
            selectedTextPrinter.EnableLigature = true;
            _devVxsTextPrinter.UpdateGlyphLayoutSettings();

            //-------------
            //pre-render
            painter.Clear(PixelFarm.Drawing.Color.White);
            painter.UseSubPixelRendering = false;
            painter.FillColor = PixelFarm.Drawing.Color.Black;
            _devVxsTextPrinter.TargetCanvasPainter = painter; //*** essential ***

            //render!,
            float x = 20, y = 200;
            _devVxsTextPrinter.DrawFromGlyphPlans(
                        GlyphPlanSequence.UnsafeGetInteralGlyphPlanList(seq),
                        seq.startAt,
                        seq.len,
                        x, y);
            //---------- 
            //use this util to copy image from Agg actual image to System.Drawing.Bitmap 
            PixelFarm.Agg.Imaging.BitmapHelper.CopyToGdiPlusBitmapSameSize(painter.Graphics.DestActualImage, winBmp);
            //----------------
            //copy from Agg's memory buffer to gdi 
            //PixelFarm.Agg.Imaging.BitmapHelper.CopyToGdiPlusBitmapSameSize(destImg, winBmp);
            g.Clear(Color.White);
            g.DrawImage(winBmp, new Point(10, 0));
        }
    }
}
