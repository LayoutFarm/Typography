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
using Typography.WebFont;

using BrotliSharpLib;
using PaintLab.Svg;
using LayoutFarm.WebLexer;

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

        public Form1()
        {
            InitializeComponent();

            var dicProvider = new Typography.TextBreak.IcuSimpleTextFileDictionaryProvider() { DataDir = "../../../../../Typography.TextBreak/icu62/brkitr" };
            Typography.TextBreak.CustomBreakerBuilder.Setup(dicProvider);
            this.Load += new System.EventHandler(this.Form1_Load);
            SetupWoffDecompressFunctions();


            MemBitmapExtensions.DefaultMemBitmapIO = new PixelFarm.Drawing.WinGdi.GdiBitmapIO();


            lstTextBaseline.Items.AddRange(
               new object[] {
                   PixelFarm.Drawing.TextBaseline.Alphabetic,
                   PixelFarm.Drawing.TextBaseline.Bottom,
                   PixelFarm.Drawing.TextBaseline.Top,
                   //TODO: implement other types
               });
            lstTextBaseline.SelectedIndex = 0;//default
            lstTextBaseline.SelectedIndexChanged += (s, e) => UpdateRenderOutput();
        }

        void SetupWoffDecompressFunctions()
        {
            //
            //Woff
            WoffDefaultZlibDecompressFunc.DecompressHandler = (byte[] compressedBytes, byte[] decompressedResult) =>
            {
                //ZLIB
                //****
                //YOU can change to  your prefer decode libs***
                //****

                bool result = false;
                try
                {
                    var inflater = new ICSharpCode.SharpZipLib.Zip.Compression.Inflater();
                    inflater.SetInput(compressedBytes);
                    inflater.Inflate(decompressedResult);
#if DEBUG
                    long outputLen = inflater.TotalOut;
                    if (outputLen != decompressedResult.Length)
                    {

                    }
#endif

                    result = true;
                }
                catch (Exception ex)
                {

                }
                return result;
            };
            //Woff2

            Woff2DefaultBrotliDecompressFunc.DecompressHandler = (byte[] compressedBytes, Stream output) =>
            {
                //BROTLI
                //****
                //YOU can change to  your prefer decode libs***
                //****

                bool result = false;
                try
                {
                    using (MemoryStream ms = new MemoryStream(compressedBytes))
                    {

                        ms.Position = 0;//set to start pos
                        DecompressAndCalculateCrc1(ms, output);
                        //
                        //  

                        //Decompress(ms, output);
                    }
                    //DecompressBrotli(compressedBytes, output);
                    result = true;
                }
                catch (Exception ex)
                {

                }
                return result;
            };
        }


        void RenderByGlyphName(string selectedGlyphName)
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
            ushort selectedGlyphIndex =
                glyphNameListUserControl1.Typeface.GetGlyphIndexByName(selectedGlyphName);

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

        bool _readyToRender;

        PixelFarm.Drawing.OpenFontTextService _textService;

        VgVisualDocHost _vgDocHost = new VgVisualDocHost();
        MemBitmap ParseAndRenderSvg(System.Text.StringBuilder svgContent)
        {
            //----------
            //copy from HtmlRenderer's SvgViewer demo
            //----------  
            var docBuilder = new VgDocBuilder();
            var parser = new SvgParser(docBuilder);
            TextSnapshot textSnapshot = new TextSnapshot(svgContent.ToString());
            parser.ParseDocument(textSnapshot);

            VgVisualDocBuilder builder = new VgVisualDocBuilder();
            VgVisualElement vgVisElem = builder.CreateVgVisualDoc(docBuilder.ResultDocument, _vgDocHost).VgRootElem;
            PixelFarm.CpuBlit.VertexProcessing.Q1RectD bounds = vgVisElem.GetRectBounds();
            float actualXOffset = (float)-bounds.Left;
            float actualYOffset = (float)-bounds.Bottom;

            int bmpW = (int)Math.Round(bounds.Width);
            int bmpH = (int)Math.Round(bounds.Height);

            if (bmpW == 0 || bmpH == 0)
            {
                return null;
            }
            MemBitmap memBitmap = new MemBitmap(bmpW, bmpH);
            using (Tools.BorrowAggPainter(memBitmap, out var p))
            using (Tools.More.BorrowVgPaintArgs(p, out var paintArgs))
            {
                float orgX = p.OriginX;
                float orgY = p.OriginY;
                p.SetOrigin(actualXOffset, actualYOffset);

                p.Clear(PixelFarm.Drawing.Color.White);

                p.FillColor = PixelFarm.Drawing.Color.Black;

                double prevStrokeW = p.StrokeWidth;

                vgVisElem.Paint(paintArgs);

                p.StrokeWidth = prevStrokeW;//restore 

                p.SetOrigin(orgX, orgY);//restore
            }

            return memBitmap;
        }

        PixelFarm.Drawing.Color _grayColor = new PixelFarm.Drawing.Color(0xFF, 0x80, 0x80, 0x80);
        void UpdateRenderOutput()
        {
            if (!_readyToRender) return;
            //
            if (_g == null)
            {
                _destImg = new MemBitmap(800, 600);
                _painter = AggPainter.Create(_destImg);
                _winBmp = new Bitmap(_destImg.Width, _destImg.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                _g = this.CreateGraphics();

                _painter.CurrentFont = new PixelFarm.Drawing.RequestFont("tahoma", 14); 

                _textService = new PixelFarm.Drawing.OpenFontTextService();
                _textService.LoadFontsFromFolder("../../../TestFonts");

                _devVxsTextPrinter = new PixelFarm.Drawing.VxsTextPrinter(_painter, _textService);
                _devVxsTextPrinter.SetSvgBmpBuilderFunc(ParseAndRenderSvg);

                _devVxsTextPrinter.ScriptLang = _basicOptions.ScriptLang;
                _devVxsTextPrinter.PositionTechnique = Typography.TextLayout.PositionTechnique.OpenFont;

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
                        case Typography.FontManagement.TypefaceStyle.Bold:
                            fontstyle = PixelFarm.Drawing.FontStyle.Bold;
                            break;
                        case Typography.FontManagement.TypefaceStyle.Italic:
                            fontstyle = PixelFarm.Drawing.FontStyle.Italic;
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
            this.glyphNameListUserControl1.GlyphNameChanged += (s, e) =>
            {
                //test render 
                //just our convention by add & and ;
                RenderByGlyphName(glyphNameListUserControl1.SelectedGlyphName);
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

            var layout = new Typography.TextLayout.GlyphLayout(typeface);
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



        /// <summary>
        /// ECMA CRC64 polynomial.
        /// </summary>
        static readonly long CRC_64_POLY = Convert.ToInt64("0xC96C5795D7870F42", 16);
        static long UpdateCrc64(long crc, byte[] data, int offset, int length)
        {
            for (int i = offset; i < offset + length; ++i)
            {
                long c = (crc ^ (long)(data[i] & 0xFF)) & 0xFF;
                for (int k = 0; k < 8; k++)
                {
                    c = ((c & 1) == 1) ? CRC_64_POLY ^ (long)((ulong)c >> 1) : (long)((ulong)c >> 1);
                }
                crc = c ^ (long)((ulong)crc >> 8);
            }
            return crc;
        }
        static long DecompressAndCalculateCrc1(Stream input, Stream output)
        {
            try
            {
                long crc = -1;
                byte[] buffer = new byte[65536];
                CSharpBrotli.Decode.BrotliInputStream decompressedStream = new CSharpBrotli.Decode.BrotliInputStream(input);
                using (MemoryStream ms = new MemoryStream())
                using (BinaryWriter writer = new BinaryWriter(ms))
                {
                    while (true)
                    {
                        int len = decompressedStream.Read(buffer);
                        if (len <= 0)
                        {
                            break;
                        }
                        else
                        {

                        }

                        writer.Write(buffer, 0, len);

                        crc = UpdateCrc64(crc, buffer, 0, len);
                    }

                    decompressedStream.Close();
                    writer.Flush();

                    byte[] outputBuffer = ms.ToArray();

                    output.Write(outputBuffer, 0, outputBuffer.Length);

                    writer.Close();
                }
                return crc ^ -1;
            }
            catch (IOException ex)
            {
                throw ex;
            }
        }
        static void Decompress(Stream input, Stream output)
        {
            /// <exception cref="System.IO.IOException"/>

            byte[] buffer = new byte[65536];
            bool byByte = false;

            Org.Brotli.Dec.BrotliInputStream brotliInput = new Org.Brotli.Dec.BrotliInputStream(input);
            if (byByte)
            {
                byte[] oneByte = new byte[1];
                while (true)
                {
                    int next = brotliInput.ReadByte();
                    if (next == -1)
                    {
                        break;
                    }
                    oneByte[0] = unchecked((byte)next);
                    output.Write(oneByte, 0, 1);
                }
            }
            else
            {
                while (true)
                {
                    int len = brotliInput.Read(buffer, 0, buffer.Length);
                    if (len <= 0)
                    {
                        break;
                    }
                    output.Write(buffer, 0, len);
                }
            }
            brotliInput.Close();
        }
        void DecompressBrotli(byte[] compressed, Stream output)
        {
            var decompressed = Brotli.DecompressBuffer(compressed, 0, compressed.Length);

        }
        private void button3_Click(object sender, EventArgs e)
        {
            string filename = "Sarabun-Regular.woff2";

            OpenFontReader openFontReader = new OpenFontReader();
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                IPreviewFontInfo previewFontInfo = openFontReader.ReadPreview(fs);
            }
        }

    }
}