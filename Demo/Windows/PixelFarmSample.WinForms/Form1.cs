//MIT, 2016-2017, WinterDev
using System;
using System.Collections.Generic;

using System.Drawing;
using System.IO;
using System.Windows.Forms;

using PixelFarm.Agg;
using PixelFarm.Drawing.Fonts;

using Typography.OpenFont;
using Typography.Rendering;
using Typography.Contours;
using Typography.TextLayout;


namespace SampleWinForms
{
    public partial class Form1 : Form
    {
        Graphics g;
        AggCanvasPainter painter;
        ImageGraphics2D imgGfx2d;
        ActualImage destImg;
        Bitmap winBmp;

        OpenFontStore _openFontStore;

        DevTextPrinterBase selectedTextPrinter = null;
        VxsTextPrinter _devVxsTextPrinter = null;
        DevGdiTextPrinter _devGdiTextPrinter = null;

        UI.SampleTextBoxControllerForGdi _controllerForGdi = new UI.SampleTextBoxControllerForGdi();
        //
        UI.SampleTextBoxControllerForPixelFarm _controllerForPixelFarm = new UI.SampleTextBoxControllerForPixelFarm();

        InstalledFontCollection installedFontCollection;
        TypefaceStore _typefaceStore;
        float _fontSizeInPts = 14;//default
        InstalledFont _selectedInstallFont;

        UI.DebugGlyphVisualizer debugGlyphVisualizer = new UI.DebugGlyphVisualizer();
        Typography.OpenFont.ScriptLang _current_script;

        public Form1()
        {
            InitializeComponent();



            _devGdiTextPrinter = new DevGdiTextPrinter();
            this.sampleTextBox1.Visible = false;
            _openFontStore = new OpenFontStore();

            //default
            //set script lang,
            //test with Thai for 'complex script' 
            _current_script = Typography.OpenFont.ScriptLangs.Latin;
            _devGdiTextPrinter.ScriptLang = _current_script;
            _devGdiTextPrinter.PositionTechnique = PositionTechnique.OpenFont;


            this.Load += new EventHandler(Form1_Load);

            this.txtGridSize.KeyDown += TxtGridSize_KeyDown;
            //----------
            txtInputChar.TextChanged += (s, e) => UpdateRenderOutput();
            //----------
            cmbRenderChoices.Items.Add(RenderChoice.RenderWithTextPrinterAndMiniAgg);
            cmbRenderChoices.Items.Add(RenderChoice.RenderWithMiniAgg_SingleGlyph);
            cmbRenderChoices.Items.Add(RenderChoice.RenderWithGdiPlusPath);
            cmbRenderChoices.Items.Add(RenderChoice.RenderWithMsdfGen);
            cmbRenderChoices.SelectedIndex = 0;
            cmbRenderChoices.SelectedIndexChanged += (s, e) => UpdateRenderOutput();
            //----------
            cmbPositionTech.Items.Add(PositionTechnique.OpenFont);
            cmbPositionTech.Items.Add(PositionTechnique.Kerning);
            cmbPositionTech.Items.Add(PositionTechnique.None);
            cmbPositionTech.SelectedIndex = 0;
            cmbPositionTech.SelectedIndexChanged += (s, e) => UpdateRenderOutput();
            //---------- 
            SetupScriptLangComboBox();

            //---------- 
            lstHintList.Items.Add(HintTechnique.None);
            lstHintList.Items.Add(HintTechnique.TrueTypeInstruction);
            lstHintList.Items.Add(HintTechnique.TrueTypeInstruction_VerticalOnly);
            lstHintList.Items.Add(HintTechnique.CustomAutoFit);
            lstHintList.SelectedIndex = 0;
            lstHintList.SelectedIndexChanged += (s, e) => UpdateRenderOutput();
            //edge offset
            lstEdgeOffset.Items.Add(0f);
            lstEdgeOffset.Items.Add(-10f);
            lstEdgeOffset.Items.Add(-8f);
            lstEdgeOffset.Items.Add(-6f);
            lstEdgeOffset.Items.Add(-4f);
            lstEdgeOffset.Items.Add(4f);
            lstEdgeOffset.Items.Add(6f);
            lstEdgeOffset.Items.Add(8f);
            lstEdgeOffset.Items.Add(10f);
            lstEdgeOffset.SelectedIndex = 0;
            lstEdgeOffset.SelectedIndexChanged += (s, e) => UpdateRenderOutput();

            //share text printer to our sample textbox
            //but you can create another text printer that specific to text textbox control
            Graphics gx = this.sampleTextBox1.CreateGraphics();
            _controllerForGdi.BindHostGraphics(gx);
            _controllerForGdi.TextPrinter = _devGdiTextPrinter;
            //---------- 
            _controllerForPixelFarm.BindHostGraphics(gx);
            _controllerForPixelFarm.TextPrinter = _devVxsTextPrinter;

            //---------- 
            this.sampleTextBox1.SetController(_controllerForPixelFarm);


            button1.Click += (s, e) => UpdateRenderOutput();
            chkShowGrid.CheckedChanged += (s, e) => UpdateRenderOutput();
            chkShowTess.CheckedChanged += (s, e) => UpdateRenderOutput();
            chkXGridFitting.CheckedChanged += (s, e) => UpdateRenderOutput();
            chkYGridFitting.CheckedChanged += (s, e) => UpdateRenderOutput();
            chkFillBackground.CheckedChanged += (s, e) => UpdateRenderOutput();
            chkLcdTechnique.CheckedChanged += (s, e) => UpdateRenderOutput();
            chkGsubEnableLigature.CheckedChanged += (s, e) => UpdateRenderOutput();
            //----------
            chkShowTess.CheckedChanged += (s, e) => UpdateRenderOutput();
            chkDrawCentroidBone.CheckedChanged += (s, e) => UpdateRenderOutput();
            chkDrawGlyphBone.CheckedChanged += (s, e) => UpdateRenderOutput();
            chkDynamicOutline.CheckedChanged += (s, e) => UpdateRenderOutput();
            chkSetPrinterLayoutForLcdSubPix.CheckedChanged += (s, e) => UpdateRenderOutput();
            chkDrawTriangles.CheckedChanged += (s, e) => UpdateRenderOutput();
            chkDrawRegenerateOutline.CheckedChanged += (s, e) => UpdateRenderOutput();
            chkBorder.CheckedChanged += (s, e) => UpdateRenderOutput();
            chkDrawLineHubConn.CheckedChanged += (s, e) => UpdateRenderOutput();
            chkDrawPerpendicularLine.CheckedChanged += (s, e) => UpdateRenderOutput();
            chkDrawGlyphPoint.CheckedChanged += (s, e) => UpdateRenderOutput();
            chkTestGridFit.CheckedChanged += (s, e) => UpdateRenderOutput();
            chkUseHorizontalFitAlign.CheckedChanged += (s, e) => UpdateRenderOutput();
            chkWriteFitOutputToConsole.CheckedChanged += (s, e) => UpdateRenderOutput();

            //---------- 
            //1. create font collection             
            installedFontCollection = new InstalledFontCollection();
            //2. set some essential handler
            installedFontCollection.SetFontNameDuplicatedHandler((f1, f2) => FontNameDuplicatedDecision.Skip);
            foreach (string file in Directory.GetFiles("../../../TestFonts", "*.ttf"))
            {
                //eg. this is our custom font folder  
                installedFontCollection.AddFont(new FontFileStreamProvider(file));
            }
            //3.
            //installedFontCollection.LoadSystemFonts();
            //---------- 
            //show result
            InstalledFont selectedFF = null;
            int selected_index = 0;
            int ffcount = 0;
            bool found = false;

            string defaultFont = "Tahoma";
            //string defaultFont = "Alef"; //test hebrew
            //string defaultFont = "Century";
            foreach (InstalledFont ff in installedFontCollection.GetInstalledFontIter())
            {
                if (!found && ff.FontName == defaultFont)
                {
                    selectedFF = ff;
                    selected_index = ffcount;
                    _selectedInstallFont = ff;
                    found = true;
                }
                lstFontList.Items.Add(ff);
                ffcount++;
            }
            //set default font for current text printer
            //
            _typefaceStore = new TypefaceStore();
            _typefaceStore.FontCollection = installedFontCollection;


            if (selected_index < 0) { selected_index = 0; }
            lstFontList.SelectedIndex = selected_index;
            lstFontList.SelectedIndexChanged += (s, e) =>
            {
                InstalledFont ff = lstFontList.SelectedItem as InstalledFont;
                if (ff != null)
                {
                    _selectedInstallFont = ff;
                    selectedTextPrinter.Typeface = _typefaceStore.GetTypeface(ff);
                    //sample text box 
                    UpdateRenderOutput();
                }
            };
            //----------

            lstFontSizes.Items.AddRange(
              new object[]{
                    8, 9,
                    10,11,
                    12,
                    14,
                    16,
                    18,20,22,24,26,28,36,48,72,
                    240,280,300,360,400,420,460,
                    620,720,860,920,1024
              });
            lstFontSizes.SelectedIndexChanged += (s, e) =>
            {
                //new font size
                _fontSizeInPts = (int)lstFontSizes.SelectedItem;
                UpdateRenderOutput();
            };

            //----------------
            //string inputstr = "ก้า";
            //string inputstr = "น้ำน้ำ";
            //string inputstr = "example";
            //string inputstr = "lllll";
            //string inputstr = "e";
            //string inputstr = "T";
            //string inputstr = "u";
            //string inputstr = "t";
            //string inputstr = "2";
            //string inputstr = "3";
            //string inputstr = "o";
            //string inputstr = "l";
            //string inputstr = "k";
            //string inputstr = "8";
            //string inputstr = "#";
            //string inputstr = "a";
            //string inputstr = "e";
            //string inputstr = "l";
            //string inputstr = "t";
            //string inputstr = "i";
            string inputstr = "ma";
            //string inputstr = "po";
            //string inputstr = "Å";
            //string inputstr = "fi";
            //string inputstr = "ก่นกิ่น";
            //string inputstr = "ญญู";
            //string inputstr = "ป่า"; //for gpos test 
            //string inputstr = "快速上手";
            //string inputstr = "啊";

            //----------------
            this.txtInputChar.Text = inputstr;
            this.chkFillBackground.Checked = true;
            _readyToRender = true;
        }



        int _defaultScriptLangComboBoxIndex = 0;
        void SetupScriptLangComboBox()
        {

            //for debug, set default script lang here

            _current_script = Typography.OpenFont.ScriptLangs.Latin;
            //
            int index = 0;
            foreach (Typography.OpenFont.ScriptLang scriptLang in Typography.OpenFont.ScriptLangs.GetRegiteredScriptLangIter())
            {
                this.cmbScriptLangs.Items.Add(scriptLang);
                //
                if (scriptLang == _current_script)
                {
                    //found default script lang
                    _defaultScriptLangComboBoxIndex = index;
                }
                index++;
            }

            this.cmbScriptLangs.SelectedIndex = _defaultScriptLangComboBoxIndex; //set before** attach event

            this.cmbScriptLangs.SelectedIndexChanged += (s, e) =>
            {
                _current_script = (Typography.OpenFont.ScriptLang)this.cmbScriptLangs.SelectedItem;
                UpdateRenderOutput();
            };
        }

        enum RenderChoice
        {
            RenderWithMiniAgg_SingleGlyph,//for test single glyph 
            RenderWithGdiPlusPath,
            RenderWithTextPrinterAndMiniAgg,
            RenderWithMsdfGen, //rendering with multi-channel signed distance field img
            RenderWithSdfGen//not support sdfgen
        }

        void Form1_Load(object sender, EventArgs e)
        {
            this.Text = "Render with PixelFarm";
            this.lstFontSizes.SelectedIndex = 0;// lstFontSizes.Items.Count - 3;

            var installedFont = lstFontList.SelectedItem as InstalledFont;
            if (installedFont != null)
            {
                _selectedInstallFont = installedFont;
            }


        }
        bool _readyToRender;
        void UpdateRenderOutput()
        {
            if (!_readyToRender) return;
            //
            if (g == null)
            {
                destImg = new ActualImage(800, 600, PixelFormat.ARGB32);
                imgGfx2d = new ImageGraphics2D(destImg); //no platform
                painter = new AggCanvasPainter(imgGfx2d);
                winBmp = new Bitmap(destImg.Width, destImg.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                g = this.CreateGraphics();

                painter.CurrentFont = new PixelFarm.Drawing.RequestFont("tahoma", 14);

                _devVxsTextPrinter = new VxsTextPrinter(painter, _openFontStore);
                _devVxsTextPrinter.TargetCanvasPainter = painter;
                _devVxsTextPrinter.ScriptLang = _current_script;
                _devVxsTextPrinter.PositionTechnique = _devGdiTextPrinter.PositionTechnique;
                _devGdiTextPrinter.TargetGraphics = g;
            }

            if (string.IsNullOrEmpty(this.txtInputChar.Text))
            {
                return;
            }

            //test option use be used with lcd subpixel rendering.
            //this demonstrate how we shift a pixel for subpixel rendering tech
            _devVxsTextPrinter.UseWithLcdSubPixelRenderingTechnique = chkSetPrinterLayoutForLcdSubPix.Checked;


            var hintTech = (HintTechnique)lstHintList.SelectedItem;

            //1. read typeface from font file 
            RenderChoice renderChoice = (RenderChoice)this.cmbRenderChoices.SelectedItem;
            switch (renderChoice)
            {

                case RenderChoice.RenderWithGdiPlusPath:
                    {
                        selectedTextPrinter = _devGdiTextPrinter;
                        selectedTextPrinter.Typeface = _typefaceStore.GetTypeface(_selectedInstallFont);
                        selectedTextPrinter.FontSizeInPoints = _fontSizeInPts;
                        selectedTextPrinter.HintTechnique = hintTech;
                        selectedTextPrinter.PositionTechnique = (PositionTechnique)cmbPositionTech.SelectedItem;
                        selectedTextPrinter.EnableLigature = this.chkGsubEnableLigature.Checked;
                        selectedTextPrinter.ScriptLang = _current_script;

#if DEBUG
                        GlyphDynamicOutline.dbugTestNewGridFitting = chkTestGridFit.Checked;
                        GlyphDynamicOutline.dbugActualPosToConsole = chkWriteFitOutputToConsole.Checked;
                        GlyphDynamicOutline.dbugUseHorizontalFitValue = chkUseHorizontalFitAlign.Checked;
#endif

                        selectedTextPrinter.DrawString(this.txtInputChar.Text.ToCharArray(), 0, 0);

                    }
                    break;
                case RenderChoice.RenderWithTextPrinterAndMiniAgg:
                    {
                        //clear previous draw
                        painter.Clear(PixelFarm.Drawing.Color.White);
                        painter.UseSubPixelRendering = chkLcdTechnique.Checked;
                        painter.FillColor = PixelFarm.Drawing.Color.Black;

                        selectedTextPrinter = _devVxsTextPrinter;
                        selectedTextPrinter.Typeface = _typefaceStore.GetTypeface(_selectedInstallFont);
                        selectedTextPrinter.FontSizeInPoints = _fontSizeInPts;
                        selectedTextPrinter.HintTechnique = hintTech;
                        selectedTextPrinter.PositionTechnique = (PositionTechnique)cmbPositionTech.SelectedItem;
                        selectedTextPrinter.EnableLigature = this.chkGsubEnableLigature.Checked;
                        selectedTextPrinter.ScriptLang = _current_script;
                        //test print 3 lines
#if DEBUG
                        GlyphDynamicOutline.dbugTestNewGridFitting = chkTestGridFit.Checked;
                        GlyphDynamicOutline.dbugActualPosToConsole = chkWriteFitOutputToConsole.Checked;
                        GlyphDynamicOutline.dbugUseHorizontalFitValue = chkUseHorizontalFitAlign.Checked;
#endif

                        char[] printTextBuffer = this.txtInputChar.Text.ToCharArray();
                        float x_pos = 0, y_pos = 200;
                        float lineSpacingPx = selectedTextPrinter.FontLineSpacingPx;
                        for (int i = 0; i < 1; ++i)
                        {
                            selectedTextPrinter.DrawString(printTextBuffer, x_pos, y_pos);
                            y_pos -= lineSpacingPx;
                        }


                        //copy from Agg's memory buffer to gdi 
                        PixelFarm.Agg.Imaging.BitmapHelper.CopyToGdiPlusBitmapSameSize(destImg, winBmp);
                        g.Clear(Color.White);
                        g.DrawImage(winBmp, new Point(10, 0));

                    }
                    break;

                //==============================================
                //render 1 glyph for debug and test
                case RenderChoice.RenderWithMsdfGen:
                case RenderChoice.RenderWithSdfGen:
                    {
                        char testChar = this.txtInputChar.Text[0];
                        Typeface typeFace = _typefaceStore.GetTypeface(_selectedInstallFont);
                        RenderWithMsdfImg(typeFace, testChar, _fontSizeInPts);

                    }
                    break;
                case RenderChoice.RenderWithMiniAgg_SingleGlyph:
                    {
                        selectedTextPrinter = _devVxsTextPrinter;
                        //for test only 1 char 
                        RenderSingleCharWithMiniAgg(
                            _typefaceStore.GetTypeface(_selectedInstallFont),
                            this.txtInputChar.Text[0],
                            _fontSizeInPts);
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        void RenderSingleCharWithMiniAgg(Typeface typeface, char testChar, float sizeInPoint)
        {

            //---------------
            //set up vinfo
            UI.DebugGlyphVisualizerInfoView vinfo = debugGlyphVisualizer.VisualizeInfoView;

            if (vinfo == null)
            {
                vinfo = new UI.DebugGlyphVisualizerInfoView();
                vinfo.SetTreeView(this.treeView1);
                vinfo.SetFlushOutputHander(() =>
                {
                    painter.SetOrigin(0, 0);
                    //6. use this util to copy image from Agg actual image to System.Drawing.Bitmap
                    PixelFarm.Agg.Imaging.BitmapHelper.CopyToGdiPlusBitmapSameSize(destImg, winBmp);
                    //--------------- 
                    //7. just render our bitmap
                    g.Clear(Color.White);
                    g.DrawImage(winBmp, new Point(30, 100));

                });
                debugGlyphVisualizer.VisualizeInfoView = vinfo;
            }

            //---------------
            //we use the debugGlyphVisualize the render it
            this.debugGlyphVisualizer.SetFont(typeface, sizeInPoint);

            debugGlyphVisualizer.UseLcdTechnique = this.chkLcdTechnique.Checked;
            debugGlyphVisualizer.CanvasPainter = painter;
            debugGlyphVisualizer.FillBackGround = chkFillBackground.Checked;
            debugGlyphVisualizer.DrawBorder = chkBorder.Checked;

            debugGlyphVisualizer.ShowTess = chkShowTess.Checked;
            debugGlyphVisualizer.WalkTrianglesAndEdges = this.chkDrawTriangles.Checked;
            debugGlyphVisualizer.DrawEndLineHub = this.chkDrawLineHubConn.Checked;
            debugGlyphVisualizer.DrawPerpendicularLine = this.chkDrawPerpendicularLine.Checked;
            debugGlyphVisualizer.WalkCentroidBone = this.chkDrawCentroidBone.Checked;
            debugGlyphVisualizer.WalkGlyphBone = this.chkDrawGlyphBone.Checked;
            debugGlyphVisualizer.GlyphEdgeOffset = (float)this.lstEdgeOffset.SelectedItem;
            debugGlyphVisualizer.DrawDynamicOutline = chkDynamicOutline.Checked;
            debugGlyphVisualizer.DrawRegenerateOutline = chkDrawRegenerateOutline.Checked;
            debugGlyphVisualizer.DrawGlyphPoint = chkDrawGlyphPoint.Checked;

#if DEBUG
            GlyphDynamicOutline.dbugTestNewGridFitting = chkTestGridFit.Checked;
            GlyphDynamicOutline.dbugActualPosToConsole = chkWriteFitOutputToConsole.Checked;
            GlyphDynamicOutline.dbugUseHorizontalFitValue = chkUseHorizontalFitAlign.Checked;
#endif


            //------------------------------------------------------

            debugGlyphVisualizer.RenderChar(testChar, (HintTechnique)lstHintList.SelectedItem);
            //---------------------------------------------------- 

            //--------------------------
            if (chkShowGrid.Checked)
            {
                //render grid
                RenderGrids(800, 600, _gridSize, painter);
            }
            painter.SetOrigin(0, 0);
            //6. use this util to copy image from Agg actual image to System.Drawing.Bitmap
            PixelFarm.Agg.Imaging.BitmapHelper.CopyToGdiPlusBitmapSameSize(destImg, winBmp);
            //--------------- 
            //7. just render our bitmap
            g.Clear(Color.White);
            g.DrawImage(winBmp, new Point(30, 100));
            //g.DrawRectangle(Pens.White, new System.Drawing.Rectangle(30, 20, winBmp.Width, winBmp.Height));
        }

        void RenderWithMsdfImg(Typeface typeface, char testChar, float sizeInPoint)
        {
            painter.FillColor = PixelFarm.Drawing.Color.Black;
            //p.UseSubPixelRendering = chkLcdTechnique.Checked;
            painter.Clear(PixelFarm.Drawing.Color.White);
            //----------------------------------------------------
            var builder = new GlyphPathBuilder(typeface);
            builder.SetHintTechnique((HintTechnique)lstHintList.SelectedItem);

            //----------------------------------------------------
            builder.Build(testChar, sizeInPoint);
            //----------------------------------------------------
            var glyphToContour = new GlyphContourBuilder();
            var msdfGenPars = new MsdfGenParams();

            builder.ReadShapes(glyphToContour);
            //glyphToContour.Read(builder.GetOutputPoints(), builder.GetOutputContours());
            MsdfGenParams genParams = new MsdfGenParams();
            GlyphImage glyphImg = MsdfGlyphGen.CreateMsdfImage(glyphToContour, genParams);

            var actualImg = ActualImage.CreateFromBuffer(glyphImg.Width, glyphImg.Height, PixelFormat.ARGB32, glyphImg.GetImageBuffer());
            painter.DrawImage(actualImg, 0, 0);

            //using (Bitmap bmp = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            //{
            //    var bmpdata = bmp.LockBits(new Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
            //    System.Runtime.InteropServices.Marshal.Copy(buffer, 0, bmpdata.Scan0, buffer.Length);
            //    bmp.UnlockBits(bmpdata);
            //    bmp.Save("d:\\WImageTest\\a001_xn2_" + n + ".png");
            //}

            if (chkShowGrid.Checked)
            {
                //render grid
                RenderGrids(800, 600, _gridSize, painter);
            }

            //6. use this util to copy image from Agg actual image to System.Drawing.Bitmap
            PixelFarm.Agg.Imaging.BitmapHelper.CopyToGdiPlusBitmapSameSize(destImg, winBmp);
            //--------------- 
            //7. just render our bitmap
            g.Clear(Color.White);
            g.DrawImage(winBmp, new Point(30, 20));
        }

        void RenderGrids(int width, int height, int sqSize, CanvasPainter p)
        {
            //render grid 
            p.FillColor = PixelFarm.Drawing.Color.Gray;

            float pointW = (sqSize >= 100) ? 2 : 1;

            for (int y = 0; y < height;)
            {
                for (int x = 0; x < width;)
                {
                    p.FillRectLBWH(x, y, pointW, pointW);
                    x += sqSize;
                }
                y += sqSize;
            }
        }






        VertexStorePool _vxsPool2 = new VertexStorePool();
        int _gridSize = 5;//default 
        private void TxtGridSize_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int result = this._gridSize;
                if (int.TryParse(this.txtGridSize.Text, out result))
                {
                    if (result < 5)
                    {
                        _gridSize = 5;
                    }
                    else if (result > 800)
                    {
                        _gridSize = 800;
                    }
                }
                this._gridSize = result;

                this.txtGridSize.Text = _gridSize.ToString();
#if DEBUG
                Typography.Contours.GlyphDynamicOutline.dbugGridHeight = _gridSize;
#endif
                UpdateRenderOutput();
            }

        }
        private void cmdBuildMsdfTexture_Click(object sender, EventArgs e)
        {

            //samples...
            //1. create texture from specific glyph index range
            string sampleFontFile = "../../../TestFonts/tahoma.ttf";
            CreateSampleMsdfTextureFont(
                sampleFontFile,
                18,
                0,
                255,
                "d:\\WImageTest\\sample_msdf.png");
            //---------------------------------------------------------
            //2. for debug, create from some unicode chars
            //
            //CreateSampleMsdfTextureFont(
            //   sampleFontFile,
            //   18,
            //  new char[] { 'I' },
            //  "d:\\WImageTest\\sample_msdf.png");
            //---------------------------------------------------------
            ////3.
            //GlyphTranslatorToContour tx = new GlyphTranslatorToContour();
            //tx.BeginRead(1);
            ////tx.MoveTo(10, 10);
            ////tx.LineTo(25, 25);
            ////tx.LineTo(15, 10);
            //tx.MoveTo(3.84f, 0);
            //tx.LineTo(1.64f, 0);
            //tx.LineTo(1.64f, 18.23f);
            //tx.LineTo(3.84f, 18.23f);
            //tx.CloseContour();
            //tx.EndRead();
            ////
            //CreateSampleMsdfImg(tx, "d:\\WImageTest\\tx_contour2.bmp");

        }
        static void CreateSampleMsdfTextureFont(
          string fontfile, float sizeInPoint,
          char[] chars, string outputFile)
        {
            //sample
            var reader = new OpenFontReader();
            using (var fs = new FileStream(fontfile, FileMode.Open))
            {
                //1. read typeface from font file
                Typeface typeface = reader.Read(fs);
                //sample: create sample msdf texture 
                //-------------------------------------------------------------
                var builder = new GlyphPathBuilder(typeface);
                //builder.UseTrueTypeInterpreter = this.chkTrueTypeHint.Checked;
                //builder.UseVerticalHinting = this.chkVerticalHinting.Checked;
                //-------------------------------------------------------------
                var atlasBuilder = new SimpleFontAtlasBuilder();

                MsdfGenParams msdfGenParams = new MsdfGenParams();

                int j = chars.Length;
                for (int i = 0; i < j; ++i)
                {
                    //build glyph
                    ushort gindex = typeface.LookupIndex(chars[i]);
                    builder.BuildFromGlyphIndex(gindex, -1);

                    var glyphToContour = new GlyphContourBuilder();
                    //glyphToContour.Read(builder.GetOutputPoints(), builder.GetOutputContours());
                    builder.ReadShapes(glyphToContour);
                    msdfGenParams.shapeScale = 1f / 64;
                    GlyphImage glyphImg = MsdfGlyphGen.CreateMsdfImage(glyphToContour, msdfGenParams);
                    atlasBuilder.AddGlyph(gindex, glyphImg);
                    int w = glyphImg.Width;
                    int h = glyphImg.Height;
                    using (Bitmap bmp = new Bitmap(glyphImg.Width, glyphImg.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                    {
                        var bmpdata = bmp.LockBits(new System.Drawing.Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
                        int[] imgBuffer = glyphImg.GetImageBuffer();
                        System.Runtime.InteropServices.Marshal.Copy(imgBuffer, 0, bmpdata.Scan0, imgBuffer.Length);
                        bmp.UnlockBits(bmpdata);
                        bmp.Save("d:\\WImageTest\\a001_xn2_" + (chars[i]) + ".png");
                    }
                }

                var glyphImg2 = atlasBuilder.BuildSingleImage();
                using (Bitmap bmp = new Bitmap(glyphImg2.Width, glyphImg2.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                {
                    var bmpdata = bmp.LockBits(new System.Drawing.Rectangle(0, 0, glyphImg2.Width, glyphImg2.Height),
                        System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
                    int[] intBuffer = glyphImg2.GetImageBuffer();

                    System.Runtime.InteropServices.Marshal.Copy(intBuffer, 0, bmpdata.Scan0, intBuffer.Length);
                    bmp.UnlockBits(bmpdata);
                    bmp.Save("d:\\WImageTest\\a_total.png");
                }
                atlasBuilder.SaveFontInfo("d:\\WImageTest\\a_info.xml");
            }
        }



        static void CreateSampleMsdfImg(GlyphContourBuilder tx, string outputFile)
        {
            //sample

            MsdfGenParams msdfGenParams = new MsdfGenParams();
            GlyphImage glyphImg = MsdfGlyphGen.CreateMsdfImage(tx, msdfGenParams);
            int w = glyphImg.Width;
            int h = glyphImg.Height;
            using (Bitmap bmp = new Bitmap(glyphImg.Width, glyphImg.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                var bmpdata = bmp.LockBits(new System.Drawing.Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
                int[] imgBuffer = glyphImg.GetImageBuffer();
                System.Runtime.InteropServices.Marshal.Copy(imgBuffer, 0, bmpdata.Scan0, imgBuffer.Length);
                bmp.UnlockBits(bmpdata);
                bmp.Save(outputFile);
            }

        }
        static void CreateSampleMsdfTextureFont(string fontfile, float sizeInPoint, ushort startGlyphIndex, ushort endGlyphIndex, string outputFile)
        {
            //sample
            var reader = new OpenFontReader();

            using (var fs = new FileStream(fontfile, FileMode.Open))
            {
                //1. read typeface from font file
                Typeface typeface = reader.Read(fs);
                //sample: create sample msdf texture 
                //-------------------------------------------------------------
                var builder = new GlyphPathBuilder(typeface);
                //builder.UseTrueTypeInterpreter = this.chkTrueTypeHint.Checked;
                //builder.UseVerticalHinting = this.chkVerticalHinting.Checked;
                //-------------------------------------------------------------
                var atlasBuilder = new SimpleFontAtlasBuilder();


                for (ushort gindex = startGlyphIndex; gindex <= endGlyphIndex; ++gindex)
                {
                    //build glyph
                    builder.BuildFromGlyphIndex(gindex, sizeInPoint);

                    var glyphToContour = new GlyphContourBuilder();
                    //glyphToContour.Read(builder.GetOutputPoints(), builder.GetOutputContours());
                    var genParams = new MsdfGenParams();
                    builder.ReadShapes(glyphToContour);
                    genParams.shapeScale = 1f / 64; //we scale later (as original C++ code use 1/64)
                    GlyphImage glyphImg = MsdfGlyphGen.CreateMsdfImage(glyphToContour, genParams);
                    atlasBuilder.AddGlyph(gindex, glyphImg);

                    //using (Bitmap bmp = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                    //{
                    //    var bmpdata = bmp.LockBits(new Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
                    //    System.Runtime.InteropServices.Marshal.Copy(buffer, 0, bmpdata.Scan0, buffer.Length);
                    //    bmp.UnlockBits(bmpdata);
                    //    bmp.Save("d:\\WImageTest\\a001_xn2_" + n + ".png");
                    //}
                }

                var glyphImg2 = atlasBuilder.BuildSingleImage();
                using (Bitmap bmp = new Bitmap(glyphImg2.Width, glyphImg2.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                {
                    var bmpdata = bmp.LockBits(new System.Drawing.Rectangle(0, 0, glyphImg2.Width, glyphImg2.Height),
                        System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
                    int[] intBuffer = glyphImg2.GetImageBuffer();

                    System.Runtime.InteropServices.Marshal.Copy(intBuffer, 0, bmpdata.Scan0, intBuffer.Length);
                    bmp.UnlockBits(bmpdata);
                    bmp.Save("d:\\WImageTest\\a_total.png");
                }
                atlasBuilder.SaveFontInfo("d:\\WImageTest\\a_info.xml");
            }
        }

        private void chkShowSampleTextBox_CheckedChanged(object sender, EventArgs e)
        {
            //if (this.sampleTextBox1.Visible = chkShowSampleTextBox.Visible)
            //{
            //    this.sampleTextBox1.Focus();
            //}
        }


    }
}
