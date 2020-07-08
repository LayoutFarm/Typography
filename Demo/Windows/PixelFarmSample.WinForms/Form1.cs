//MIT, 2016-present, WinterDev
using System;
using System.Collections.Generic;
using System.Drawing;

using System.IO;
using System.Windows.Forms;

using PixelFarm.CpuBlit;
using PixelFarm.CpuBlit.BitmapAtlas;
using PixelFarm.Contours;

using Typography.OpenFont;
using Typography.TextLayout;
using Typography.Contours;

namespace SampleWinForms
{
    public partial class Form1 : Form
    {
        Graphics _g;
        AggPainter _painter;
        MemBitmap _destImg;
        Bitmap _winBmp;

        TextPrinterBase _selectedTextPrinter = null;
        PixelFarm.Drawing.VxsTextPrinter _devVxsTextPrinter = null;

        UI.DebugGlyphVisualizer _debugGlyphVisualizer = new UI.DebugGlyphVisualizer();
        TypographyTest.BasicFontOptions _basicOptions;
        TypographyTest.GlyphRenderOptions _glyphRenderOptions;
        TypographyTest.ContourAnalysisOptions _contourAnalysisOpts;

        bool _readyToRender;
        PixelFarm.Drawing.OpenFontTextService _textService;
        PixelFarm.Drawing.Color _grayColor = new PixelFarm.Drawing.Color(0xFF, 0x80, 0x80, 0x80);


        public Form1()
        {
            InitializeComponent();

            lstTextBaseline.Items.AddRange(
               new object[] {
                   PixelFarm.Drawing.TextBaseline.Alphabetic,
                   PixelFarm.Drawing.TextBaseline.Bottom,
                   PixelFarm.Drawing.TextBaseline.Top,
                   //TODO: implement other types
               });
            lstTextBaseline.SelectedIndex = 0;//default
            lstTextBaseline.SelectedIndexChanged += (s, e) => UpdateRenderOutput();
            this.Load += new System.EventHandler(this.Form1_Load);
        }


        void RenderByGlyphIndex(ushort selectedGlyphIndex)
        {
            //---------------------------------------------
            //this version only render with MiniAgg**
            //---------------------------------------------

            _painter.Clear(PixelFarm.Drawing.Color.White);
            _painter.UseLcdEffectSubPixelRendering = _contourAnalysisOpts.LcdTechnique;
            _painter.FillColor = PixelFarm.Drawing.Color.Black;

            _selectedTextPrinter = _devVxsTextPrinter;
            _selectedTextPrinter.Typeface = _basicOptions.Typeface;
            _selectedTextPrinter.FontSizeInPoints = _basicOptions.FontSizeInPoints;
            _selectedTextPrinter.ScriptLang = _basicOptions.ScriptLang;
            _selectedTextPrinter.PositionTechnique = _basicOptions.PositionTech;

            _selectedTextPrinter.HintTechnique = _glyphRenderOptions.HintTechnique;
            _selectedTextPrinter.EnableLigature = _glyphRenderOptions.EnableLigature;
            _selectedTextPrinter.EnableMultiTypefaces = _basicOptions.EnableMultiTypefaces;
            //test print 3 lines
#if DEBUG
            DynamicOutline.dbugTestNewGridFitting = _contourAnalysisOpts.EnableGridFit;
            DynamicOutline.dbugActualPosToConsole = _contourAnalysisOpts.WriteFitOutputToConsole;
            DynamicOutline.dbugUseHorizontalFitValue = _contourAnalysisOpts.UseHorizontalFitAlignment;
#endif


            float x_pos = 0, y_pos = 100;
            var glyphPlanList = new Typography.TextLayout.UnscaledGlyphPlanList();


            //in this version
            //create a glyph-plan manully


            glyphPlanList.Append(
                new Typography.TextLayout.UnscaledGlyphPlan(0, selectedGlyphIndex, 0, 0, 0));

            var seq = new Typography.TextLayout.GlyphPlanSequence(
                glyphPlanList,
                0, 1);
            _selectedTextPrinter.DrawFromGlyphPlans(seq, x_pos, y_pos);

            char[] printTextBuffer = this.txtInputChar.Text.ToCharArray();
            float lineSpacingPx = _selectedTextPrinter.FontLineSpacingPx;
            for (int i = 0; i < 1; ++i)
            {
                _selectedTextPrinter.DrawString(printTextBuffer, x_pos, y_pos);
                y_pos -= lineSpacingPx;
            }


            //copy from Agg's memory buffer to gdi 
            PixelFarm.CpuBlit.BitmapHelper.CopyToGdiPlusBitmapSameSizeNotFlip(_destImg, _winBmp);
            _g.Clear(System.Drawing.Color.White);
            _g.DrawImage(_winBmp, new System.Drawing.Point(10, 0));
        }
        void RenderByGlyphName(string selectedGlyphName) => RenderByGlyphIndex(glyphNameListUserControl1.Typeface.GetGlyphIndexByName(selectedGlyphName));

       
        void InitGraphics()
        {
            //INIT ONCE
            if (_g != null) return;
            //

            _destImg = new MemBitmap(800, 600);
            _painter = AggPainter.Create(_destImg);
            _winBmp = new Bitmap(_destImg.Width, _destImg.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            _g = this.CreateGraphics();

            _painter.CurrentFont = new PixelFarm.Drawing.RequestFont("Source Sans Pro", 10);


            _textService = new PixelFarm.Drawing.OpenFontTextService();
            _textService.LoadFontsFromFolder("../../../TestFonts");
            _textService.UpdateUnicodeRanges();

            _devVxsTextPrinter = new PixelFarm.Drawing.VxsTextPrinter(_painter, _textService);
            _devVxsTextPrinter.SetSvgBmpBuilderFunc(PaintLab.SvgBuilderHelper.ParseAndRenderSvg);
            _devVxsTextPrinter.ScriptLang = _basicOptions.ScriptLang;
            _devVxsTextPrinter.PositionTechnique = Typography.TextLayout.PositionTechnique.OpenFont;


            //Alternative Typeface selector..
            var myAlternativeTypefaceSelector = new PixelFarm.Drawing.MyAlternativeTypefaceSelector();
            {
                //arabic

                //1. create prefer typeface list for arabic script
                var preferTypefaces = new PixelFarm.Drawing.MyAlternativeTypefaceSelector.PreferredTypefaceList();
                preferTypefaces.AddTypefaceName("Noto Sans Arabic UI");

                //2. set unicode ranges and prefered typeface list. 
                myAlternativeTypefaceSelector.SetPreferredTypefaces(
                     new[]{Typography.TextBreak.Unicode13RangeInfoList.Arabic,
                               Typography.TextBreak.Unicode13RangeInfoList.Arabic_Supplement,
                               Typography.TextBreak.Unicode13RangeInfoList.Arabic_Extended_A},
                    preferTypefaces);
            }
            {
                //latin

                var preferTypefaces = new PixelFarm.Drawing.MyAlternativeTypefaceSelector.PreferredTypefaceList();
                preferTypefaces.AddTypefaceName("Sarabun");

                myAlternativeTypefaceSelector.SetPreferredTypefaces(
                     new[]{Typography.TextBreak.Unicode13RangeInfoList.C0_Controls_and_Basic_Latin,
                               Typography.TextBreak.Unicode13RangeInfoList.C1_Controls_and_Latin_1_Supplement,
                               Typography.TextBreak.Unicode13RangeInfoList.Latin_Extended_A,
                               Typography.TextBreak.Unicode13RangeInfoList.Latin_Extended_B,
                     },
                    preferTypefaces);
            }

            _devVxsTextPrinter.AlternativeTypefaceSelector = myAlternativeTypefaceSelector;
        }

        void UpdateRenderOutput()
        {
            if (!_readyToRender) return;
            //
            if (_g == null)
            {
                InitGraphics();
            }

            if (string.IsNullOrEmpty(this.txtInputChar.Text))
            {
                return;
            }

            //test option use be used with lcd subpixel rendering.
            //this demonstrate how we shift a pixel for subpixel rendering tech

            if (_contourAnalysisOpts.SetupPrinterLayoutForLcdSubPix)
            {
                //TODO: set lcd or not here
            }
            else
            {
                //TODO: set lcd or not here

            }

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
                        _painter.Clear(PixelFarm.Drawing.Color.White);
                        _painter.UseLcdEffectSubPixelRendering = _contourAnalysisOpts.LcdTechnique;
                        _painter.FillColor = PixelFarm.Drawing.Color.Black;

                        _selectedTextPrinter = _devVxsTextPrinter;
                        _selectedTextPrinter.Typeface = _basicOptions.Typeface;
                        _selectedTextPrinter.FontSizeInPoints = _basicOptions.FontSizeInPoints;
                        _selectedTextPrinter.ScriptLang = _basicOptions.ScriptLang;
                        _selectedTextPrinter.PositionTechnique = _basicOptions.PositionTech;

                        _selectedTextPrinter.HintTechnique = _glyphRenderOptions.HintTechnique;
                        _selectedTextPrinter.EnableLigature = _glyphRenderOptions.EnableLigature;
                        _selectedTextPrinter.EnableMultiTypefaces = _basicOptions.EnableMultiTypefaces;
                        _selectedTextPrinter.SimulateSlant = _contourAnalysisOpts.SimulateSlant;


                        _selectedTextPrinter.TextBaseline = (PixelFarm.Drawing.TextBaseline)lstTextBaseline.SelectedItem;

                        //test print 3 lines
#if DEBUG
                        DynamicOutline.dbugTestNewGridFitting = _contourAnalysisOpts.EnableGridFit;
                        DynamicOutline.dbugActualPosToConsole = _contourAnalysisOpts.WriteFitOutputToConsole;
                        DynamicOutline.dbugUseHorizontalFitValue = _contourAnalysisOpts.UseHorizontalFitAlignment;
#endif

                        char[] printTextBuffer = this.txtInputChar.Text.ToCharArray();
                        float x_pos = 0, y_pos = 0;
                        float lineSpacingPx = _selectedTextPrinter.FontLineSpacingPx;

                        const int REF_LINE_LEN = 300;
                        for (int i = 0; i < 3; ++i)
                        {
                            _selectedTextPrinter.DrawString(printTextBuffer, x_pos, y_pos);
#if DEBUG
                            //show debug info...
                            var prevColor = _painter.FillColor;
                            var prevStrokColor = _painter.StrokeColor;
                            _painter.FillColor = PixelFarm.Drawing.Color.Red;
                            _painter.FillRect(x_pos, y_pos, 5, 5); // start point

                            //see   //https://developer.mozilla.org/en-US/docs/Web/API/CanvasRenderingContext2D/textBaseline
                            switch (_selectedTextPrinter.TextBaseline)
                            {
                                default:
                                    {
                                        System.Diagnostics.Debug.WriteLine("UNIMPLEMENTED" + _selectedTextPrinter.TextBaseline.ToString());
                                        goto case PixelFarm.Drawing.TextBaseline.Alphabetic;//
                                    }
                                case PixelFarm.Drawing.TextBaseline.Alphabetic:
                                    {
                                        //alphabetic baseline
                                        _painter.StrokeColor = _grayColor;
                                        _painter.DrawLine(x_pos,           /**/ y_pos,
                                                          x_pos + REF_LINE_LEN, y_pos);

                                        _painter.StrokeColor = PixelFarm.Drawing.Color.Blue;
                                        _painter.DrawLine(x_pos,           /**/ y_pos - _selectedTextPrinter.FontDescedingPx,
                                                          x_pos + REF_LINE_LEN, y_pos - _selectedTextPrinter.FontDescedingPx);//bottom most

                                    }
                                    break;
                                case PixelFarm.Drawing.TextBaseline.Top:
                                    {
                                        //alphabetic baseline
                                        _painter.StrokeColor = _grayColor;
                                        _painter.DrawLine(x_pos,           /**/ y_pos + _selectedTextPrinter.FontAscendingPx,
                                                          x_pos + REF_LINE_LEN, y_pos + _selectedTextPrinter.FontAscendingPx);
                                        //em bottom
                                        _painter.StrokeColor = PixelFarm.Drawing.Color.Blue;
                                        _painter.DrawLine(x_pos,           /**/ y_pos + (_selectedTextPrinter.FontAscendingPx - _selectedTextPrinter.FontDescedingPx),
                                                          x_pos + REF_LINE_LEN, y_pos + (_selectedTextPrinter.FontAscendingPx - _selectedTextPrinter.FontDescedingPx));//bottom most


                                    }
                                    break;
                                case PixelFarm.Drawing.TextBaseline.Bottom:
                                    {
                                        //alphabetic baseline
                                        _painter.StrokeColor = _grayColor;
                                        _painter.DrawLine(x_pos,           /**/ y_pos + _selectedTextPrinter.FontDescedingPx,
                                                          x_pos + REF_LINE_LEN, y_pos + _selectedTextPrinter.FontDescedingPx);
                                        //em bottom
                                        _painter.StrokeColor = PixelFarm.Drawing.Color.Blue;
                                        _painter.DrawLine(x_pos,           /**/ y_pos,
                                                          x_pos + REF_LINE_LEN, y_pos);//bottom most 
                                    }
                                    break;
                            }


                            _painter.FillColor = prevColor;
                            _painter.StrokeColor = prevColor;
#endif
                            y_pos += (_selectedTextPrinter.FontAscendingPx - _selectedTextPrinter.FontDescedingPx);

                        }


                        //copy from Agg's memory buffer to gdi 
                        PixelFarm.CpuBlit.BitmapHelper.CopyToGdiPlusBitmapSameSizeNotFlip(_destImg, _winBmp);
                        _g.Clear(Color.White);
                        _g.DrawImage(_winBmp, new Point(0, 0));
                    }
                    break;

                //==============================================
                //render 1 glyph for debug and test
                case TypographyTest.RenderChoice.RenderWithMsdfGen:
                case TypographyTest.RenderChoice.RenderWithSdfGen:
                    {
                        char testChar = this.txtInputChar.Text[0];
                        Typeface typeFace = _basicOptions.Typeface;
                        RenderWithMsdfImg(typeFace, testChar, _basicOptions.FontSizeInPoints);

                    }
                    break;
                case TypographyTest.RenderChoice.RenderWithMiniAgg_SingleGlyph:
                    {
                        _selectedTextPrinter = _devVxsTextPrinter;
                        //for test only 1 char 
                        RenderSingleCharWithMiniAgg(
                             _basicOptions.Typeface,
                            this.txtInputChar.Text[0],
                            _basicOptions.FontSizeInPoints);
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
            UI.DebugGlyphVisualizerInfoView vinfo = _debugGlyphVisualizer.VisualizeInfoView;

            if (vinfo == null)
            {
                vinfo = new UI.DebugGlyphVisualizerInfoView();
                vinfo.SetTreeView(glyphContourAnalysisOptionsUserControl1.DebugTreeView);
                vinfo.SetFlushOutputHander(() =>
                {
                    _painter.SetOrigin(0, 0);
                    //6. use this util to copy image from Agg actual image to System.Drawing.Bitmap
                    PixelFarm.CpuBlit.BitmapHelper.CopyToGdiPlusBitmapSameSize(_destImg, _winBmp);
                    //--------------- 
                    //7. just render our bitmap
                    _g.Clear(Color.White);
                    _g.DrawImage(_winBmp, new Point(30, 100));

                });
                _debugGlyphVisualizer.VisualizeInfoView = vinfo;
            }

            //---------------
            //we use the debugGlyphVisualize the render it
            _debugGlyphVisualizer.SetFont(typeface, sizeInPoint);
            _debugGlyphVisualizer.CanvasPainter = _painter;
            _debugGlyphVisualizer.UseLcdTechnique = _contourAnalysisOpts.LcdTechnique;
            _debugGlyphVisualizer.FillBackGround = _glyphRenderOptions.FillBackground;
            _debugGlyphVisualizer.DrawBorder = _glyphRenderOptions.DrawBorder;

            _debugGlyphVisualizer.ShowTess = _contourAnalysisOpts.ShowTess;
            _debugGlyphVisualizer.ShowTriangles = _contourAnalysisOpts.ShowTriangles;
            _debugGlyphVisualizer.DrawTrianglesAndEdges = _contourAnalysisOpts.ShowTriangles;
            _debugGlyphVisualizer.DrawEndLineHub = _contourAnalysisOpts.DrawLineHubConn;
            _debugGlyphVisualizer.DrawPerpendicularLine = _contourAnalysisOpts.DrawPerpendicularLine;
            _debugGlyphVisualizer.DrawCentroid = _contourAnalysisOpts.DrawCentroidBone;
            _debugGlyphVisualizer.DrawCentroid = _contourAnalysisOpts.DrawGlyphBone;

            _debugGlyphVisualizer.GlyphEdgeOffset = _contourAnalysisOpts.EdgeOffset;

            _debugGlyphVisualizer.DrawDynamicOutline = _contourAnalysisOpts.DynamicOutline;
            _debugGlyphVisualizer.DrawRegenerateOutline = _contourAnalysisOpts.DrawRegenerationOutline;
            _debugGlyphVisualizer.DrawGlyphPoint = _contourAnalysisOpts.DrawGlyphPoint;

#if DEBUG
            DynamicOutline.dbugTestNewGridFitting = _contourAnalysisOpts.EnableGridFit;
            DynamicOutline.dbugActualPosToConsole = _contourAnalysisOpts.WriteFitOutputToConsole;
            DynamicOutline.dbugUseHorizontalFitValue = _contourAnalysisOpts.UseHorizontalFitAlignment;
#endif


            //------------------------------------------------------

            _debugGlyphVisualizer.RenderChar(testChar, _glyphRenderOptions.HintTechnique);
            //---------------------------------------------------- 

            //--------------------------
            if (_contourAnalysisOpts.ShowGrid)
            {
                //render grid
                RenderGrids(800, 600, _gridSize, _painter);
            }
            _painter.SetOrigin(0, 0);


            //6. use this util to copy image from Agg actual image to System.Drawing.Bitmap
            PixelFarm.CpuBlit.BitmapHelper.CopyToGdiPlusBitmapSameSize(_destImg, _winBmp);
            _g.Clear(System.Drawing.Color.White);
            //7. just render our bitmap
            _g.DrawImage(_winBmp, new System.Drawing.Point(10, 0));
        }

        void RenderWithMsdfImg(Typeface typeface, char testChar, float sizeInPoint)
        {
            _painter.FillColor = PixelFarm.Drawing.Color.Black;
            //p.UseSubPixelRendering = chkLcdTechnique.Checked;
            _painter.Clear(PixelFarm.Drawing.Color.White);
            //----------------------------------------------------
            var builder = new GlyphOutlineBuilder(typeface);
            builder.SetHintTechnique(_glyphRenderOptions.HintTechnique);

            //----------------------------------------------------
            builder.Build(testChar, sizeInPoint);
            //----------------------------------------------------
            var glyphToContour = new ContourBuilder();


            builder.ReadShapes(new GlyphTranslatorToContourBuilder(glyphToContour));
            //glyphToContour.Read(builder.GetOutputPoints(), builder.GetOutputContours());
            Msdfgen.MsdfGenParams genParams = new Msdfgen.MsdfGenParams();
            BitmapAtlasItemSource glyphImg = MsdfImageGen.CreateMsdfImageV1(glyphToContour, genParams);

            MemBitmap actualImg = MemBitmap.CreateFromCopy(glyphImg.Width, glyphImg.Height, glyphImg.GetImageBuffer());
            _painter.DrawImage(actualImg, 0, 0);

            //using (Bitmap bmp = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            //{
            //    var bmpdata = bmp.LockBits(new Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
            //    System.Runtime.InteropServices.Marshal.Copy(buffer, 0, bmpdata.Scan0, buffer.Length);
            //    bmp.UnlockBits(bmpdata);
            //    bmp.Save("a001_xn2_" + n + ".png");
            //}

            if (_contourAnalysisOpts.ShowGrid)
            {
                //render grid
                RenderGrids(800, 600, _gridSize, _painter);
            }

            //6. use this util to copy image from Agg actual image to System.Drawing.Bitmap
            PixelFarm.CpuBlit.BitmapHelper.CopyToGdiPlusBitmapSameSize(_destImg, _winBmp);
            //--------------- 
            //7. just render our bitmap
            _g.Clear(Color.White);
            _g.DrawImage(_winBmp, new Point(30, 20));
        }

        void RenderGrids(int width, int height, int sqSize, AggPainter p)
        {
            //render grid 
            p.FillColor = new PixelFarm.Drawing.Color(0xFF, 0x80, 0x80, 0x80);//gray

            float pointW = (sqSize >= 100) ? 2 : 1;

            for (int y = 0; y < height;)
            {
                for (int x = 0; x < width;)
                {
                    p.FillRect(x, y, pointW, pointW);
                    x += sqSize;
                }
                y += sqSize;
            }
        }




        int _gridSize = 5;//default 

        private void cmdBuildMsdfTexture_Click(object sender, EventArgs e)
        {
            FormMsdfTest2 test2 = new FormMsdfTest2();
            test2.Show();
        }


        private void Form1_Load(object sender, EventArgs e1)
        {
            this.Text = "Render with PixelFarm";
            //
            _basicOptions = openFontOptions1.Options;
            _basicOptions.TypefaceChanged += (s, e) =>
            {
                if (e.SelectedTypeface == null) return;
                //
                if (_devVxsTextPrinter != null)
                {
                    PixelFarm.Drawing.FontStyle fontstyle = PixelFarm.Drawing.FontStyle.Regular;
                    switch (_basicOptions.SelectedTypefaceStyle)
                    {
                        case Typography.FontManagement.TypefaceStyle.Regular:
                            fontstyle = PixelFarm.Drawing.FontStyle.Regular;
                            break;
                        case Typography.FontManagement.TypefaceStyle.Bold:
                            fontstyle = PixelFarm.Drawing.FontStyle.Bold;
                            break;
                        case Typography.FontManagement.TypefaceStyle.Italic:
                            fontstyle = PixelFarm.Drawing.FontStyle.Italic;
                            break;
                        case Typography.FontManagement.TypefaceStyle.Others:
                            fontstyle = PixelFarm.Drawing.FontStyle.Others;
                            break;
                    }
                    _devVxsTextPrinter.Typeface = e.SelectedTypeface;
                    var reqFont = new PixelFarm.Drawing.RequestFont(
                        e.SelectedTypeface.Name,
                        _basicOptions.FontSizeInPoints,
                        fontstyle);
                    _devVxsTextPrinter.ChangeFont(reqFont);
                    _painter.CurrentFont = reqFont;
                }


                this.glyphNameListUserControl1.Typeface = e.SelectedTypeface;
            };

            _basicOptions.UpdateRenderOutput += (s, e) => UpdateRenderOutput();
            //
            _glyphRenderOptions = glyphRenderOptionsUserControl1.Options;
            _glyphRenderOptions.UpdateRenderOutput += (s, e) => UpdateRenderOutput();
            //
            _contourAnalysisOpts = glyphContourAnalysisOptionsUserControl1.Options;
            _contourAnalysisOpts.UpdateRenderOutput += (s, e) => UpdateRenderOutput();



            txtInputChar.TextChanged += (s, e) => UpdateRenderOutput();
            button1.Click += (s, e) => UpdateRenderOutput();

            //
            this.glyphNameListUserControl1.GlyphChanged += (s, e) =>
            {
                //test render 
                //just our convention by add & and ;

                if (this.glyphNameListUserControl1.RenderByGlyphName)
                {
                    RenderByGlyphName(glyphNameListUserControl1.SelectedGlyphName);
                }
                else
                {
                    //render by glyph index
                    RenderByGlyphIndex(glyphNameListUserControl1.SelectedGlyphIndex);
                }
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
            string inputstr = "0";
            //string inputstr = "e";
            //string inputstr = "l";
            //string inputstr = "t";
            //string inputstr = "i";
            //string inputstr = "ma"; 
            //string inputstr = "po";
            //string inputstr = "Å";
            //string inputstr = "fi";
            //string inputstr = "ก่นกิ่น";
            //string inputstr = "ญญู";
            //string inputstr = "ป่า"; //for gpos test 
            //string inputstr = "快速上手";
            //string inputstr = "啊";
            this.txtInputChar.Text = inputstr;
            _readyToRender = true;

            if (_readyToRender)
            {
                UpdateRenderOutput();
            }
        }

        private void cmdMeasureString_Click(object sender, EventArgs e)
        {

            //How to measure user's string...
            //this demostrate step-by-step

            //similar to ...  selectedTextPrinter.DrawString(printTextBuffer, x_pos, y_pos); 
            string str = txtInputChar.Text;
            //
            Typeface typeface = _basicOptions.Typeface;
            float fontSizeInPoints = _basicOptions.FontSizeInPoints;

            var layout = new Typography.TextLayout.GlyphLayout();
            layout.Typeface = typeface;
            layout.ScriptLang = _basicOptions.ScriptLang;
            layout.PositionTechnique = _basicOptions.PositionTech;
            layout.EnableLigature = false;// true
            layout.EnableComposition = true;

            //3.
            //3.1 : if you want GlyphPlanList too.
            //var resultGlyphPlanList = new Typography.TextLayout.GlyphPlanList();
            //Typography.TextLayout.MeasuredStringBox box = layout.LayoutAndMeasureString(str.ToCharArray(), 0, str.Length, _basicOptions.FontSizeInPoints, resultGlyphPlanList);

            //or
            //3.2 : only MeasuredStringBox
            Typography.TextLayout.MeasuredStringBox box =
                layout.LayoutAndMeasureString(
                    str.ToCharArray(), 0,
                    str.Length,
                    fontSizeInPoints);

            this.lblStringSize.Text = "measure (W,H)= (" + box.width.ToString() + "," + (box.AscendingInPx - box.DescendingInPx) + ") px";
        }

        private void cmdTestFontAtlas_Click(object sender, EventArgs e)
        {

            FormFontAtlas fontAtlas = new FormFontAtlas();

            fontAtlas.SetFont(_basicOptions.Typeface, _basicOptions.FontSizeInPoints);

            fontAtlas.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {

            OpenFontReader openFontReader = new OpenFontReader();
            string filename = "Test/Sarabun-Regular.woff";
            //using (FileStream fs = new FileStream(filename, FileMode.Open))
            //{
            //    PreviewFontInfo previewFont = openFontReader.ReadPreview(fs);
            //} 
            //assign woff decompressor here   
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                openFontReader.Read(fs);
            }
        }



        private void button3_Click(object sender, EventArgs e)
        {
            string filename = "Test/Sarabun-Regular.woff2";
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                OpenFontReader openFontReader = new OpenFontReader();
                PreviewFontInfo previewFontInfo = openFontReader.ReadPreview(fs);
            }
        }


        private void cmdTestReloadGlyphs_Click(object sender, EventArgs e)
        {
            (new FormTestTrimmableFeature()).Show();
        }

        private void cmdTestFontReq_Click(object sender, EventArgs e)
        {
            (new FormTestFontRequest()).Show();
        }
    }
}