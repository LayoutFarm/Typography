//MIT, 2016-2017, WinterDev
using System;
using System.Collections.Generic;

using System.Drawing;
using System.IO;
using System.Windows.Forms;

using Typography.OpenFont;
using Typography.Rendering;

using PixelFarm.Agg;
using Typography.TextLayout;
using PixelFarm.Drawing.Fonts;

namespace SampleWinForms
{
    public partial class Form1 : Form
    {
        Graphics g;
        AggCanvasPainter p;
        ImageGraphics2D imgGfx2d;
        ActualImage destImg;
        Bitmap winBmp;


        public Form1()
        {
            InitializeComponent();
            this.Load += new EventHandler(Form1_Load);

            //----------
            cmbRenderChoices.Items.Add(RenderChoice.RenderWithMiniAgg);
            cmbRenderChoices.Items.Add(RenderChoice.RenderWithPlugableGlyphRasterizer);
            cmbRenderChoices.Items.Add(RenderChoice.RenderWithTextPrinterAndMiniAgg);
            cmbRenderChoices.Items.Add(RenderChoice.RenderWithMsdfGen);
            cmbRenderChoices.SelectedIndex = 2;
            cmbRenderChoices.SelectedIndexChanged += (s, e) => UpdateRenderOutput();
            //----------
            cmbPositionTech.Items.Add(PositionTecnhique.OpenType);
            cmbPositionTech.Items.Add(PositionTecnhique.Kerning);
            cmbPositionTech.Items.Add(PositionTecnhique.None);
            cmbPositionTech.SelectedIndex = 0;
            cmbPositionTech.SelectedIndexChanged += (s, e) => UpdateRenderOutput();
            //----------
            cmbHintTechnique.Items.Add(HintTechnique.None);
            cmbHintTechnique.Items.Add(HintTechnique.TrueTypeInstruction);
            cmbHintTechnique.Items.Add(HintTechnique.TrueTypeInstruction_VerticalOnly);
            cmbHintTechnique.Items.Add(HintTechnique.CustomAutoFit);
            cmbHintTechnique.SelectedIndex = 0;
            cmbHintTechnique.SelectedIndexChanged += (s, e) => UpdateRenderOutput();
            //----------
            lstFontSizes.Items.AddRange(
                new object[]{
                    8, 9,
                    10,11,
                    12,
                    14,
                    16,
                    18,20,22,24,26,28,36,48,72,240,300,360
                });
            this.txtGridSize.KeyDown += TxtGridSize_KeyDown;

            //----------
            //simple load test fonts from local test dir
            //and send it into test list

            int selectedFileIndex = -1;
            //string selectedFontFileName = "pala.ttf";
            string selectedFontFileName = "tahoma.ttf";
            //string selectedFontFileName="cambriaz.ttf";
            //string selectedFontFileName="CompositeMS2.ttf"; 

            int fileIndexCount = 0;
            foreach (string file in Directory.GetFiles("..\\..", "*.ttf"))
            {
                var tmpLocalFile = new TempLocalFontFile(file);
                lstFontList.Items.Add(tmpLocalFile);
                if (selectedFileIndex < 0 && tmpLocalFile.OnlyFileName == selectedFontFileName)
                {
                    selectedFileIndex = fileIndexCount;
                    _currentSelectedFontFile = file;
                }
                fileIndexCount++;
            }
            if (selectedFileIndex < 0) { selectedFileIndex = 0; }
            lstFontList.SelectedIndex = selectedFileIndex;
            lstFontList.SelectedIndexChanged += (s, e) =>
            {
                _currentSelectedFontFile = ((TempLocalFontFile)lstFontList.SelectedItem).actualFileName;
                UpdateRenderOutput();
            };
            //----------------
            //string inputstr = "ก้า";
            string inputstr = "น้ำ";
            //string inputstr = "fi";
            //string inputstr = "ก่นกิ่น";
            //string inputstr = "ญญู";
            //string inputstr = "ป่า"; //for gpos test 
            //string inputstr = "快速上手";
            //----------------
            this.txtInputChar.Text = inputstr;
            this.chkFillBackground.Checked = true;
        }
        string _currentSelectedFontFile = "";
        private void CmbPositionTech_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateRenderOutput();
        }
        class TempLocalFontFile
        {
            //temp only
            public readonly string actualFileName;
            public TempLocalFontFile(string actualFileName)
            {
                this.actualFileName = actualFileName;
            }
            public string OnlyFileName
            {
                get { return Path.GetFileName(actualFileName); }
            }
#if DEBUG
            public override string ToString()
            {
                return this.OnlyFileName;
            }
#endif            
        }
        enum RenderChoice
        {
            RenderWithMiniAgg,
            RenderWithPlugableGlyphRasterizer,
            RenderWithTextPrinterAndMiniAgg,
            RenderWithMsdfGen, //rendering with multi-channel signed distance field img
            RenderWithSdfGen//not support sdfgen
        }

        void Form1_Load(object sender, EventArgs e)
        {
            this.Text = "Render with PixelFarm";
            //this.lstFontSizes.SelectedIndex = lstFontSizes.Items.Count - 1;//select last one  
            this.lstFontSizes.SelectedIndex = 0;//select last one  
        }

        private void button1_Click(object sender, EventArgs e)
        {
            UpdateRenderOutput();
        }

        void UpdateRenderOutput()
        {
            if (g == null)
            {
                destImg = new ActualImage(400, 300, PixelFormat.ARGB32);
                imgGfx2d = new ImageGraphics2D(destImg); //no platform
                p = new AggCanvasPainter(imgGfx2d);
                winBmp = new Bitmap(400, 300, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                g = this.CreateGraphics();
            }
            ReadAndRender(_currentSelectedFontFile);
        }

        float fontSizeInPoint = 14; //default
        void ReadAndRender(string fontfile)
        {
            if (string.IsNullOrEmpty(this.txtInputChar.Text))
            {
                p.Clear(PixelFarm.Drawing.Color.White);
                return;
            }
            var reader = new OpenTypeReader();
            char testChar = txtInputChar.Text[0];//only 1 char 
            int resolution = 96;

            using (var fs = new FileStream(fontfile, FileMode.Open))
            {
                //1. read typeface from font file
                Typeface typeFace = reader.Read(fs);

                RenderChoice renderChoice = (RenderChoice)this.cmbRenderChoices.SelectedItem;
                switch (renderChoice)
                {
                    case RenderChoice.RenderWithMiniAgg:
                        RenderWithMiniAgg(typeFace, testChar, fontSizeInPoint);
                        break;
                    case RenderChoice.RenderWithPlugableGlyphRasterizer:
                        RenderWithPlugableGlyphRasterizer(typeFace, testChar, fontSizeInPoint, resolution);
                        break;
                    case RenderChoice.RenderWithTextPrinterAndMiniAgg:
                        RenderWithTextPrinterAndMiniAgg(typeFace, this.txtInputChar.Text, fontSizeInPoint, resolution);
                        break;
                    case RenderChoice.RenderWithMsdfGen:
                    case RenderChoice.RenderWithSdfGen:
                        RenderWithMsdfImg(typeFace, testChar, fontSizeInPoint);
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
        }


        static int s_POINTS_PER_INCH = 72; //default value, 
        static int s_PIXELS_PER_INCH = 96; //default value
        public static float ConvEmSizeInPointsToPixels(float emsizeInPoint)
        {
            return (int)(((float)emsizeInPoint / (float)s_POINTS_PER_INCH) * (float)s_PIXELS_PER_INCH);
        }

        //-------------------
        //https://www.microsoft.com/typography/otspec/TTCH01.htm
        //Converting FUnits to pixels
        //Values in the em square are converted to values in the pixel coordinate system by multiplying them by a scale. This scale is:
        //pointSize * resolution / ( 72 points per inch * units_per_em )
        //where pointSize is the size at which the glyph is to be displayed, and resolution is the resolution of the output device.
        //The 72 in the denominator reflects the number of points per inch.
        //For example, assume that a glyph feature is 550 FUnits in length on a 72 dpi screen at 18 point. 
        //There are 2048 units per em. The following calculation reveals that the feature is 4.83 pixels long.
        //550 * 18 * 72 / ( 72 * 2048 ) = 4.83
        //-------------------
        public static float ConvFUnitToPixels(ushort reqFUnit, float fontSizeInPoint, ushort unitPerEm)
        {
            //reqFUnit * scale             
            return reqFUnit * GetFUnitToPixelsScale(fontSizeInPoint, unitPerEm);
        }
        public static float GetFUnitToPixelsScale(float fontSizeInPoint, ushort unitPerEm)
        {
            //reqFUnit * scale             
            return ((fontSizeInPoint * s_PIXELS_PER_INCH) / (s_POINTS_PER_INCH * unitPerEm));
        }

        //from http://www.w3schools.com/tags/ref_pxtoemconversion.asp
        //set default
        // 16px = 1 em
        //-------------------
        //1. conv font design unit to em
        // em = designUnit / unit_per_Em       
        //2. conv font design unit to pixels 
        // float scale = (float)(size * resolution) / (pointsPerInch * _typeface.UnitsPerEm);
        static Msdfgen.Shape CreateMsdfShape(List<GlyphContour> contours)
        {
            var shape = new Msdfgen.Shape();
            int j = contours.Count;
            for (int i = 0; i < j; ++i)
            {
                var cnt = new Msdfgen.Contour();
                shape.contours.Add(cnt);

                GlyphContour contour = contours[i];
                List<GlyphPart> parts = contour.parts;
                int m = parts.Count;
                for (int n = 0; n < m; ++n)
                {
                    GlyphPart p = parts[n];
                    switch (p.Kind)
                    {
                        default: throw new NotSupportedException();
                        case GlyphPartKind.Curve3:
                            {
                                GlyphCurve3 curve3 = (GlyphCurve3)p;
                                cnt.AddQuadraticSegment(
                                    curve3.x0, curve3.y0,
                                    curve3.p2x, curve3.p2y,
                                    curve3.x, curve3.y
                                   );
                            }
                            break;
                        case GlyphPartKind.Curve4:
                            {
                                GlyphCurve4 curve4 = (GlyphCurve4)p;
                                cnt.AddCubicSegment(
                                    curve4.x0, curve4.y0,
                                    curve4.p2x, curve4.p2y,
                                    curve4.p3x, curve4.p3y,
                                    curve4.x, curve4.y);
                            }
                            break;
                        case GlyphPartKind.Line:
                            {
                                GlyphLine line = (GlyphLine)p;
                                cnt.AddLine(
                                    line.x0, line.y0,
                                    line.x1, line.y1);
                            }
                            break;
                    }
                }
            }

            return shape;

        }

        void RenderWithMiniAgg(Typeface typeface, char testChar, float sizeInPoint)
        {
            //----------------------------------------------------
            var builder = new GlyphPathBuilderVxs(typeface);
            var hintTech = (HintTechnique)cmbHintTechnique.SelectedItem;
            builder.UseTrueTypeInterpreter = false;//reset
            builder.UseVerticalHinting = false;//reset
            switch (hintTech)
            {
                case HintTechnique.TrueTypeInstruction:
                    builder.UseTrueTypeInterpreter = true;
                    break;
                case HintTechnique.TrueTypeInstruction_VerticalOnly:
                    builder.UseTrueTypeInterpreter = true;
                    builder.UseVerticalHinting = true;
                    break;
                case HintTechnique.CustomAutoFit:
                    //custom agg autofit 
                    break;
            }
            //----------------------------------------------------

            builder.Build(testChar, sizeInPoint);
            VertexStore vxs = builder.GetVxs();
            p.UseSubPixelRendering = chkLcdTechnique.Checked;

            //5. use PixelFarm's Agg to render to bitmap...
            //5.1 clear background
            p.Clear(PixelFarm.Drawing.Color.White);

            if (chkFillBackground.Checked)
            {
                //5.2 
                p.FillColor = PixelFarm.Drawing.Color.Black;
                //5.3
                if (!chkYGridFitting.Checked)
                {
                    p.Fill(vxs);
                }
            }
            if (chkBorder.Checked)
            {
                //5.4 
                // p.StrokeWidth = 3;
                p.StrokeColor = PixelFarm.Drawing.Color.Green;
                //user can specific border width here...
                //p.StrokeWidth = 2;
                //5.5 
                p.Draw(vxs);
            }

            //master outline analysis
            {
                List<GlyphContour> contours = builder.GetContours();
                int j = contours.Count;
                var analyzer = new GlyphPartAnalyzer();
                analyzer.NSteps = 4;
                analyzer.PixelScale = builder.GetPixelScale();
                for (int i = 0; i < j; ++i)
                {
                    //analyze each contour
                    contours[i].Analyze(analyzer);
                }
                //draw each contour point
            }
            float scale = builder.GetPixelScale();

            GlyphFitOutline glyphOutline = null;
            {
                //draw for debug ...
                //draw control point
                List<GlyphContour> contours = builder.GetContours();
                glyphOutline = TessWithPolyTri(contours, scale);
                if (chkYGridFitting.Checked || chkXGridFitting.Checked)
                {
                    PixelFarm.Agg.VertexStore vxs2 = new VertexStore();
                    int j = contours.Count;
                    for (int i = 0; i < j; ++i)
                    {
                        CreateFitContourVxs(vxs2, contours[i], scale, chkXGridFitting.Checked, chkYGridFitting.Checked);
                    }
                    p.FillColor = PixelFarm.Drawing.Color.Black;
                    p.Fill(vxs2);
                }
            }
            if (chkShowTess.Checked)
            {
#if DEBUG
                debugDrawTriangulatedGlyph(glyphOutline, scale);
#endif
            }

            if (chkShowGrid.Checked)
            {
                //render grid
                RenderGrid(800, 600, _gridSize, p);
            }

            //if (chkShowControlPoints.Checked)
            //{
            //    List<GlyphContour> contours = builder.GetContours();
            //    int j = contours.Count;
            //    for (int i = 0; i < j; ++i)
            //    {
            //        GlyphContour cnt = contours[i];
            //        DrawGlyphContour(cnt, p);
            //    }
            //} 


            //6. use this util to copy image from Agg actual image to System.Drawing.Bitmap
            PixelFarm.Agg.Imaging.BitmapHelper.CopyToGdiPlusBitmapSameSize(destImg, winBmp);
            //--------------- 
            //7. just render our bitmap
            g.Clear(Color.White);
            g.DrawImage(winBmp, new Point(30, 20));
        }

        void RenderWithMsdfImg(Typeface typeface, char testChar, float sizeInPoint)
        {
            //----------------------------------------------------
            var builder = new GlyphPathBuilderVxs(typeface);
            var hintTech = (HintTechnique)cmbHintTechnique.SelectedItem;
            builder.UseTrueTypeInterpreter = false;//reset
            builder.UseVerticalHinting = false;//reset
            switch (hintTech)
            {
                case HintTechnique.TrueTypeInstruction:
                    builder.UseTrueTypeInterpreter = true;
                    break;
                case HintTechnique.TrueTypeInstruction_VerticalOnly:
                    builder.UseTrueTypeInterpreter = true;
                    builder.UseVerticalHinting = true;
                    break;
                case HintTechnique.CustomAutoFit:
                    //custom agg autofit 
                    break;
            }
            //----------------------------------------------------
            builder.Build(testChar, sizeInPoint);
            VertexStore vxs = builder.GetVxs();
            p.UseSubPixelRendering = chkLcdTechnique.Checked;
            //5. use PixelFarm's Agg to render to bitmap...
            //5.1 clear background
            p.Clear(PixelFarm.Drawing.Color.White);

            //master outline analysis
            {
                List<GlyphContour> contours = builder.GetContours();
                int j = contours.Count;
                var analyzer = new GlyphPartAnalyzer();
                analyzer.NSteps = 4;
                analyzer.PixelScale = builder.GetPixelScale();
                for (int i = 0; i < j; ++i)
                {
                    //analyze each contour
                    contours[i].Analyze(analyzer);
                }
                //draw each contour point
            }
            float scale = builder.GetPixelScale();
            GlyphFitOutline glyphOutline = null;
            {
                //draw for debug ...
                //draw control point
                List<GlyphContour> contours = builder.GetContours();
                glyphOutline = TessWithPolyTri(contours, scale);
                int j = contours.Count;
                List<GlyphContour> newFitContours = new List<GlyphContour>();

                for (int i = 0; i < j; ++i)
                {
                    newFitContours.Add(CreateFitContourVxs2(contours[i], scale, chkXGridFitting.Checked, chkYGridFitting.Checked));
                }
                p.FillColor = PixelFarm.Drawing.Color.Black;
                //render with msdf gen 
                //convert vxs to msdf coord and render
                Msdfgen.Shape shape = CreateMsdfShape(newFitContours);
                double left, bottom, right, top;
                shape.findBounds(out left, out bottom, out right, out top);

                Msdfgen.FloatRGBBmp frgbBmp = new Msdfgen.FloatRGBBmp((int)Math.Ceiling((right - left)), (int)Math.Ceiling((top - bottom)));
                Msdfgen.EdgeColoring.edgeColoringSimple(shape, 3);
                Msdfgen.MsdfGenerator.generateMSDF(frgbBmp, shape, 4, new Msdfgen.Vector2(1, 1), new Msdfgen.Vector2(), -1);
                //-----------------------------------
                int[] buffer = Msdfgen.MsdfGenerator.ConvertToIntBmp(frgbBmp);
                //MsdfGen.SwapColorComponentFromBigEndianToWinGdi(buffer);
                ActualImage actualImg = ActualImage.CreateFromBuffer(frgbBmp.Width, frgbBmp.Height, PixelFormat.ARGB32, buffer);
                p.DrawImage(actualImg, 0, 0);
                //-----------------------------------
                //using (Bitmap bmp = new Bitmap(frgbBmp.Width, frgbBmp.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                //{
                //    var bmpdata = bmp.LockBits(new Rectangle(0, 0, frgbBmp.Width, frgbBmp.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
                //    System.Runtime.InteropServices.Marshal.Copy(buffer, 0, bmpdata.Scan0, buffer.Length);
                //    bmp.UnlockBits(bmpdata);
                //    bmp.Save("d:\\WImageTest\\a001_xn2_.png");
                //}
            }


            if (chkShowGrid.Checked)
            {
                //render grid
                RenderGrid(800, 600, _gridSize, p);
            }

            //6. use this util to copy image from Agg actual image to System.Drawing.Bitmap
            PixelFarm.Agg.Imaging.BitmapHelper.CopyToGdiPlusBitmapSameSize(destImg, winBmp);
            //--------------- 
            //7. just render our bitmap
            g.Clear(Color.White);
            g.DrawImage(winBmp, new Point(30, 20));
        }

        static GlyphContour CreateFitContourVxs2(GlyphContour contour, float pixelScale, bool x_axis, bool y_axis)
        {
            GlyphContour newc = new GlyphContour();
            List<GlyphPart> parts = contour.parts;
            int m = parts.Count;
            for (int n = 0; n < m; ++n)
            {
                GlyphPart p = parts[n];
                switch (p.Kind)
                {
                    default: throw new NotSupportedException();
                    case GlyphPartKind.Curve3:
                        {
                            GlyphCurve3 curve3 = (GlyphCurve3)p;
                            newc.AddPart(new GlyphCurve3(
                                curve3.x0 * pixelScale, curve3.y0 * pixelScale,
                                curve3.p2x * pixelScale, curve3.p2y * pixelScale,
                                curve3.x * pixelScale, curve3.y * pixelScale));

                        }
                        break;
                    case GlyphPartKind.Curve4:
                        {
                            GlyphCurve4 curve4 = (GlyphCurve4)p;
                            newc.AddPart(new GlyphCurve4(
                                  curve4.x0 * pixelScale, curve4.y0 * pixelScale,
                                  curve4.p2x * pixelScale, curve4.p2y * pixelScale,
                                  curve4.p3x * pixelScale, curve4.p3y * pixelScale,
                                  curve4.x * pixelScale, curve4.y * pixelScale
                                ));
                        }
                        break;
                    case GlyphPartKind.Line:
                        {
                            GlyphLine line = (GlyphLine)p;
                            newc.AddPart(new GlyphLine(
                                line.x0 * pixelScale, line.y0 * pixelScale,
                                line.x1 * pixelScale, line.y1 * pixelScale
                                ));
                        }
                        break;
                }
            }


            return newc;
        }

        static void CreateFitContourVxs(VertexStore vxs, GlyphContour contour, float pixelScale, bool x_axis, bool y_axis)
        {
            List<GlyphPoint2D> mergePoints = contour.mergedPoints;
            int j = mergePoints.Count;
            //merge 0 = start
            double prev_px = 0;
            double prev_py = 0;
            double p_x = 0;
            double p_y = 0;
            double first_px = 0;
            double first_py = 0;

            {
                GlyphPoint2D p = mergePoints[0];
                p_x = p.x * pixelScale;
                p_y = p.y * pixelScale;

                if (y_axis && p.isPartOfHorizontalEdge && p.isUpperSide && p_y > 3)
                {
                    //vertical fitting
                    //fit p_y to grid
                    p_y = RoundToNearestVerticalSide((float)p_y);
                }

                if (x_axis && p.IsPartOfVerticalEdge && p.IsLeftSide)
                {
                    float new_x = RoundToNearestHorizontalSide((float)p_x);
                    //adjust right-side vertical edge
                    EdgeLine rightside = p.GetMatchingVerticalEdge();
                    if (rightside != null)
                    {

                    }
                    p_x = new_x;
                }
                vxs.AddMoveTo(p_x, p_y);
                //-------------
                first_px = prev_px = p_x;
                first_py = prev_py = p_y;
            }

            for (int i = 1; i < j; ++i)
            {
                //all merge point is polygon point
                GlyphPoint2D p = mergePoints[i];
                p_x = p.x * pixelScale;
                p_y = p.y * pixelScale;

                if (y_axis && p.isPartOfHorizontalEdge && p.isUpperSide && p_y > 3)
                {
                    //vertical fitting
                    //fit p_y to grid
                    p_y = RoundToNearestVerticalSide((float)p_y);
                }

                if (x_axis && p.IsPartOfVerticalEdge && p.IsLeftSide)
                {
                    //horizontal fitting
                    //fix p_x to grid
                    float new_x = RoundToNearestHorizontalSide((float)p_x);
                    ////adjust right-side vertical edge
                    //PixelFarm.Agg.Typography.EdgeLine rightside = p.GetMatchingVerticalEdge();
                    //if (rightside != null && !rightside.IsLeftSide && rightside.IsOutside)
                    //{
                    //    var rightSideP = rightside.p.userData as GlyphPoint2D;
                    //    var rightSideQ = rightside.q.userData as GlyphPoint2D;
                    //    //find move diff
                    //    float movediff = (float)p_x - new_x;
                    //    //adjust right side edge
                    //    rightSideP.x = rightSideP.x + movediff;
                    //    rightSideQ.x = rightSideQ.x + movediff;
                    //}
                    p_x = new_x;
                }
                //
                vxs.AddLineTo(p_x, p_y);
                //
                prev_px = p_x;
                prev_py = p_y;

            }
            vxs.AddLineTo(first_px, first_py);

        }

        const int GRID_SIZE = 1;
        const float GRID_SIZE_25 = 1f / 4f;
        const float GRID_SIZE_50 = 2f / 4f;
        const float GRID_SIZE_75 = 3f / 4f;

        const float GRID_SIZE_33 = 1f / 3f;
        const float GRID_SIZE_66 = 2f / 3f;

        static float RoundToNearestVerticalSide(float org)
        {
            float actual1 = org;
            float integer1 = (int)(actual1);
            float floatModulo = actual1 - integer1;

            if (floatModulo >= (GRID_SIZE_50))
            {
                return (integer1 + 1);
            }
            else
            {
                return integer1;
            }
        }
        static float RoundToNearestHorizontalSide(float org)
        {
            float actual1 = org;
            float integer1 = (int)(actual1);//lower
            float floatModulo = actual1 - integer1;

            if (floatModulo >= (GRID_SIZE_50))
            {
                return (integer1 + 1);
            }
            else
            {
                return integer1;
            }
        }
        void RenderGrid(int width, int height, int sqSize, AggCanvasPainter p)
        {
            //render grid 
            p.FillColor = PixelFarm.Drawing.Color.Gray;
            for (int y = 0; y < height;)
            {
                for (int x = 0; x < width;)
                {
                    p.FillRectLBWH(x, y, 1, 1);
                    x += sqSize;
                }
                y += sqSize;
            }
        }
        static void DrawEdge(AggCanvasPainter p, EdgeLine edge, float scale)
        {
            if (edge.IsOutside)
            {
                //free side                
                switch (edge.SlopKind)
                {
                    default:
                        p.StrokeColor = PixelFarm.Drawing.Color.Green;
                        break;
                    case LineSlopeKind.Vertical:
                        if (edge.IsLeftSide)
                        {
                            p.StrokeColor = PixelFarm.Drawing.Color.Blue;
                        }
                        else
                        {
                            p.StrokeColor = PixelFarm.Drawing.Color.LightGray;
                        }
                        break;
                    case LineSlopeKind.Horizontal:
                        if (edge.IsUpper)
                        {
                            p.StrokeColor = PixelFarm.Drawing.Color.Red;
                        }
                        else
                        {
                            p.StrokeColor = PixelFarm.Drawing.Color.LightGray;
                        }
                        break;
                }
            }
            else
            {
                switch (edge.SlopKind)
                {
                    default:
                        p.StrokeColor = PixelFarm.Drawing.Color.LightGray;
                        break;
                    case LineSlopeKind.Vertical:
                        p.StrokeColor = PixelFarm.Drawing.Color.Blue;
                        break;
                    case LineSlopeKind.Horizontal:
                        p.StrokeColor = PixelFarm.Drawing.Color.Yellow;
                        break;
                }
            }
            p.Line(edge.x0 * scale, edge.y0 * scale, edge.x1 * scale, edge.y1 * scale);
        }

        static void AssignPointEdgeInvolvement(EdgeLine edge)
        {
            if (!edge.IsOutside)
            {
                return;
            }

            switch (edge.SlopKind)
            {

                case LineSlopeKind.Horizontal:
                    {
                        //horiontal edge
                        //must check if this is upper horizontal 
                        //or lower horizontal 
                        //we know after do bone analysis

                        //------------
                        //both p and q of this edge is part of horizontal edge 
                        var p = edge.p.userData as GlyphPoint2D;
                        if (p != null)
                        {
                            //TODO: review here
                            p.isPartOfHorizontalEdge = true;
                            p.isUpperSide = edge.IsUpper;
                            p.horizontalEdge = edge;
                        }

                        var q = edge.q.userData as GlyphPoint2D;
                        if (q != null)
                        {
                            //TODO: review here
                            q.isPartOfHorizontalEdge = true;
                            q.horizontalEdge = edge;
                            q.isUpperSide = edge.IsUpper;
                        }
                    }
                    break;
                case LineSlopeKind.Vertical:
                    {
                        //both p and q of this edge is part of vertical edge 
                        var p = edge.p.userData as GlyphPoint2D;
                        if (p != null)
                        {
                            //TODO: review here 
                            p.AddVerticalEdge(edge);
                        }

                        var q = edge.q.userData as GlyphPoint2D;
                        if (q != null)
                        {   //TODO: review here

                            q.AddVerticalEdge(edge);
                        }
                    }
                    break;
            }

        }
        GlyphFitOutline TessWithPolyTri(List<GlyphContour> contours, float pixelScale)
        {
            List<Poly2Tri.TriangulationPoint> points = new List<Poly2Tri.TriangulationPoint>();
            int cntCount = contours.Count;
            GlyphContour cnt = contours[0];
            Poly2Tri.Polygon polygon = CreatePolygon2(contours[0]);//first contour            
            bool isHoleIf = !cnt.IsClockwise;
            //if (cntCount > 0)
            //{
            //    //debug only
            for (int n = 1; n < cntCount; ++n)
            {
                cnt = contours[n];
                //IsHole is correct after we Analyze() the glyph contour
                polygon.AddHole(CreatePolygon2(cnt));
                //if (cnt.IsClockwise == isHoleIf)
                //{
                //     polygon.AddHole(CreatePolygon2(cnt));
                //}
                //else
                //{
                //    //eg i
                //    //the is a complete separate dot  (i head) over i body 
                //}
            }
            //}

            //------------------------------------------
            Poly2Tri.P2T.Triangulate(polygon); //that poly is triangulated 
            GlyphFitOutline glyphFitOutline = new GlyphFitOutline(polygon);
            glyphFitOutline.Analyze();
            //------------------------------------------

            List<GlyphTriangle> triAngles = glyphFitOutline.dbugGetTriangles();
            int triangleCount = triAngles.Count;

            bool drawBone = this.chkDrawBone.Checked;
            for (int i = 0; i < triangleCount; ++i)
            {
                //---------------
                GlyphTriangle tri = triAngles[i];
                AssignPointEdgeInvolvement(tri.e0);
                AssignPointEdgeInvolvement(tri.e1);
                AssignPointEdgeInvolvement(tri.e2);
            }

            return glyphFitOutline;
        }
        struct TmpPoint
        {
            public readonly double x;
            public readonly double y;
            public TmpPoint(double x, double y)
            {
                this.x = x;
                this.y = y;
            }
#if DEBUG
            public override string ToString()
            {
                return x + "," + y;
            }
#endif
        }

#if DEBUG
        void debugDrawTriangulatedGlyph(GlyphFitOutline glyphFitOutline, float pixelScale)
        {
            p.StrokeColor = PixelFarm.Drawing.Color.Magenta;
            List<GlyphTriangle> triAngles = glyphFitOutline.dbugGetTriangles();
            int j = triAngles.Count;
            //
            double prev_cx = 0, prev_cy = 0;
            // 
            bool drawBone = this.chkDrawBone.Checked;

            for (int i = 0; i < j; ++i)
            {
                //---------------
                GlyphTriangle tri = triAngles[i];
                EdgeLine e0 = tri.e0;
                EdgeLine e1 = tri.e1;
                EdgeLine e2 = tri.e2;
                //---------------
                //draw each triangles
                DrawEdge(p, e0, pixelScale);
                DrawEdge(p, e1, pixelScale);
                DrawEdge(p, e2, pixelScale);
                //---------------
                //draw centroid
                double cen_x = tri.CentroidX;
                double cen_y = tri.CentroidY;
                //---------------
                p.FillColor = PixelFarm.Drawing.Color.Yellow;
                p.FillRectLBWH(cen_x * pixelScale, cen_y * pixelScale, 2, 2);
                if (!drawBone)
                {
                    //if not draw bone then draw connected lines
                    if (i == 0)
                    {
                        //start mark
                        p.FillColor = PixelFarm.Drawing.Color.Yellow;
                        p.FillRectLBWH(cen_x * pixelScale, cen_y * pixelScale, 7, 7);
                    }
                    else
                    {
                        p.StrokeColor = PixelFarm.Drawing.Color.Red;
                        p.Line(
                            prev_cx * pixelScale, prev_cy * pixelScale,
                           cen_x * pixelScale, cen_y * pixelScale);
                    }
                    prev_cx = cen_x;
                    prev_cy = cen_y;
                }
            }
            //---------------
            //draw bone 
            if (drawBone)
            {
                List<GlyphBone> bones = glyphFitOutline.dbugGetBones();
                j = bones.Count;
                for (int i = 0; i < j; ++i)
                {
                    GlyphBone b = bones[i];
                    if (i == 0)
                    {
                        //start mark
                        p.FillColor = PixelFarm.Drawing.Color.Yellow;
                        p.FillRectLBWH(b.p.CentroidX * pixelScale, b.p.CentroidY * pixelScale, 7, 7);
                    }
                    //draw each bone
                    p.StrokeColor = PixelFarm.Drawing.Color.Red;
                    p.Line(
                        b.p.CentroidX * pixelScale, b.p.CentroidY * pixelScale,
                        b.q.CentroidX * pixelScale, b.q.CentroidY * pixelScale);
                }
            }
            //---------------
        }

#endif

        /// <summary>
        /// create polygon from flatten curve outline point
        /// </summary>
        /// <param name="cnt"></param>
        /// <returns></returns>
        static Poly2Tri.Polygon CreatePolygon2(GlyphContour cnt)
        {
            List<Poly2Tri.TriangulationPoint> points = new List<Poly2Tri.TriangulationPoint>();
            List<GlyphPart> allParts = cnt.parts;
            //---------------------------------------
            //merge all generated points
            //also remove duplicated point too! 
            List<GlyphPoint2D> mergedPoints = new List<GlyphPoint2D>();
            cnt.mergedPoints = mergedPoints;
            //---------------------------------------
            {
                int tt = 0;
                int j = allParts.Count;

                for (int i = 0; i < j; ++i)
                {
                    GlyphPart p = allParts[i];

                    List<GlyphPoint2D> fpoints = p.GetFlattenPoints();
                    if (tt == 0)
                    {
                        int n = fpoints.Count;
                        for (int m = 0; m < n; ++m)
                        {
                            //GlyphPoint2D fp = fpoints[m];
                            mergedPoints.Add(fpoints[m]);
                            //allPoints.Add((float)fp.x);
                            //allPoints.Add((float)fp.y);
                        }
                        tt++;
                    }
                    else
                    {
                        //except first point
                        int n = fpoints.Count;
                        for (int m = 1; m < n; ++m)
                        {
                            //GlyphPoint2D fp = fpoints[m];
                            mergedPoints.Add(fpoints[m]);
                            //allPoints.Add((float)fp.x);
                            //allPoints.Add((float)fp.y);
                        }
                    }

                }

            }
            //---------------------------------------
            {
                //check last (x,y) and first (x,y)
                int lim = mergedPoints.Count - 1;
                {
                    if (mergedPoints[lim].IsEqualValues(mergedPoints[0]))
                    {
                        //remove last (x,y)
                        mergedPoints.RemoveAt(lim);
                        lim -= 1;
                    }
                }

                //limitation: poly tri not accept duplicated points!
                double prevX = 0;
                double prevY = 0;
                Dictionary<TmpPoint, bool> tmpPoints = new Dictionary<TmpPoint, bool>();
                lim = mergedPoints.Count;

                for (int i = 0; i < lim; ++i)
                {
                    GlyphPoint2D p = mergedPoints[i];
                    double x = p.x;
                    double y = p.y;

                    if (x == prevX && y == prevY)
                    {
                        throw new NotSupportedException();
                    }
                    else
                    {
                        TmpPoint tmp_point = new TmpPoint(x, y);
                        if (!tmpPoints.ContainsKey(tmp_point))
                        {
                            //ensure no duplicated point
                            tmpPoints.Add(tmp_point, true);
                            var userTriangulationPoint = new Poly2Tri.TriangulationPoint(x, y) { userData = p };
                            p.triangulationPoint = userTriangulationPoint;
                            points.Add(userTriangulationPoint);
                        }
                        else
                        {
                            throw new NotSupportedException();
                        }

                        prevX = x;
                        prevY = y;
                    }
                }

                Poly2Tri.Polygon polygon = new Poly2Tri.Polygon(points.ToArray());
                return polygon;
            }
        }

        void DrawGlyphContour(GlyphContour cnt, AggCanvasPainter p)
        {
            //for debug
            List<GlyphPart> parts = cnt.parts;
            int j = parts.Count;
            for (int i = 0; i < j; ++i)
            {
                GlyphPart part = parts[i];
                switch (part.Kind)
                {
                    default: throw new NotSupportedException();
                    case GlyphPartKind.Line:
                        {
                            GlyphLine line = (GlyphLine)part;
                            p.FillColor = PixelFarm.Drawing.Color.Red;
                            p.FillRectLBWH(line.x0, line.y0, 2, 2);
                            p.FillRectLBWH(line.x1, line.y1, 2, 2);
                        }
                        break;
                    case GlyphPartKind.Curve3:
                        {
                            GlyphCurve3 c = (GlyphCurve3)part;
                            p.FillColor = PixelFarm.Drawing.Color.Red;
                            p.FillRectLBWH(c.x0, c.y0, 2, 2);
                            p.FillColor = PixelFarm.Drawing.Color.Blue;
                            p.FillRectLBWH(c.p2x, c.p2y, 2, 2);
                            p.FillColor = PixelFarm.Drawing.Color.Red;
                            p.FillRectLBWH(c.x, c.y, 2, 2);
                        }
                        break;
                    case GlyphPartKind.Curve4:
                        {
                            GlyphCurve4 c = (GlyphCurve4)part;
                            p.FillColor = PixelFarm.Drawing.Color.Red;
                            p.FillRectLBWH(c.x0, c.y0, 2, 2);
                            p.FillColor = PixelFarm.Drawing.Color.Blue;
                            p.FillRectLBWH(c.p2x, c.p2y, 2, 2);
                            p.FillRectLBWH(c.p3x, c.p3y, 2, 2);
                            p.FillColor = PixelFarm.Drawing.Color.Red;
                            p.FillRectLBWH(c.x, c.y, 2, 2);
                        }
                        break;
                }
            }
        }

        void RenderWithPlugableGlyphRasterizer(Typeface typeface, char testChar, float sizeInPoint, int resolution)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.Clear(Color.White);
            ////credit:
            ////http://stackoverflow.com/questions/1485745/flip-coordinates-when-drawing-to-control
            g.ScaleTransform(1.0F, -1.0F);// Flip the Y-Axis 
            g.TranslateTransform(0.0F, -(float)300);// Translate the drawing area accordingly  

            //2. glyph to gdi path
            var gdiGlyphRasterizer = new GDIGlyphRasterizer();
            var builder = new GlyphPathBuilder(typeface, gdiGlyphRasterizer);

            var hintTech = (HintTechnique)cmbHintTechnique.SelectedItem;
            builder.UseTrueTypeInterpreter = false;//reset
            builder.UseVerticalHinting = false;//reset
            switch (hintTech)
            {
                case HintTechnique.TrueTypeInstruction:
                    builder.UseTrueTypeInterpreter = true;
                    break;
                case HintTechnique.TrueTypeInstruction_VerticalOnly:
                    builder.UseTrueTypeInterpreter = true;
                    builder.UseVerticalHinting = true;
                    break;
                case HintTechnique.CustomAutoFit:
                    //custom agg autofit 
                    break;
            }
            //----------------------------------------------------
            builder.Build(testChar, sizeInPoint);


            if (chkFillBackground.Checked)
            {
                gdiGlyphRasterizer.Fill(g, Brushes.Black);
            }
            if (chkBorder.Checked)
            {
                gdiGlyphRasterizer.Draw(g, Pens.Green);
            }
            //transform back
            g.ScaleTransform(1.0F, -1.0F);// Flip the Y-Axis 
            g.TranslateTransform(0.0F, -(float)300);// Translate the drawing area accordingly            

        }
        void RenderWithTextPrinterAndMiniAgg(Typeface typeface, string str, float sizeInPoint, int resolution)
        {
            //1. 
            TextPrinter printer = new TextPrinter();
            printer.ScriptLang = ScriptLangs.Thai;
            //
            printer.EnableLigature = this.chkGsubEnableLigature.Checked;
            printer.PositionTechnique = (PositionTecnhique)cmbPositionTech.SelectedItem;
            //printer.EnableTrueTypeHint = this.chkTrueTypeHint.Checked;
            //printer.UseAggVerticalHinting = this.chkVerticalHinting.Checked;
            //
            int len = str.Length;
            //
            List<GlyphPlan> glyphPlanList = new List<GlyphPlan>(len);
            printer.Print(typeface, sizeInPoint, str, glyphPlanList);
            //--------------------------

            //5. use PixelFarm's Agg to render to bitmap...
            //5.1 clear background
            p.Clear(PixelFarm.Drawing.Color.White);
            //---------------------------
            //TODO: review here
            //fake subpixel rendering 
            //not correct

            p.UseSubPixelRendering = chkLcdTechnique.Checked;
            //---------------------------
            if (chkFillBackground.Checked)
            {
                //5.2 
                p.FillColor = PixelFarm.Drawing.Color.Black;
                //5.3 
                int glyphListLen = glyphPlanList.Count;

                float ox = p.OriginX;
                float oy = p.OriginY;
                float cx = 0;
                float cy = 10;
                for (int i = 0; i < glyphListLen; ++i)
                {
                    GlyphPlan glyphPlan = glyphPlanList[i];
                    cx = glyphPlan.x;
                    cy = glyphPlan.y;
                    p.SetOrigin(cx, cy);
                    p.Fill((VertexStore)glyphPlan.vxs);
                }
                p.SetOrigin(ox, oy);

            }
            if (chkBorder.Checked)
            {
                //5.4 
                p.StrokeColor = PixelFarm.Drawing.Color.Green;
                //user can specific border width here...
                //p.StrokeWidth = 2;
                //5.5 
                int glyphListLen = glyphPlanList.Count;
                float ox = p.OriginX;
                float oy = p.OriginY;
                float cx = 0;
                float cy = 10;
                for (int i = 0; i < glyphListLen; ++i)
                {
                    GlyphPlan glyphPlan = glyphPlanList[i];
                    cx = glyphPlan.x;
                    p.SetOrigin(cx, cy);
                    p.Draw((VertexStore)glyphPlan.vxs);
                }
                p.SetOrigin(ox, oy);
            }
            //6. use this util to copy image from Agg actual image to System.Drawing.Bitmap
            PixelFarm.Agg.Imaging.BitmapHelper.CopyToGdiPlusBitmapSameSize(destImg, winBmp);
            //--------------- 
            //7. just render our bitmap
            g.Clear(Color.White);
            g.DrawImage(winBmp, new Point(10, 0));
            //--------------------------

        }


        private void txtInputChar_TextChanged(object sender, EventArgs e)
        {
            UpdateRenderOutput();
        }


        private void lstFontSizes_SelectedIndexChanged(object sender, EventArgs e)
        {
            //new font size
            fontSizeInPoint = (int)lstFontSizes.SelectedItem;
            UpdateRenderOutput();
        }

        private void chkKern_CheckedChanged(object sender, EventArgs e)
        {
            UpdateRenderOutput();
        }

        private void chkTrueTypeHint_CheckedChanged(object sender, EventArgs e)
        {
            UpdateRenderOutput();
        }

        private void chkShowTess_CheckedChanged(object sender, EventArgs e)
        {
            UpdateRenderOutput();
        }

        private void chkShowGrid_CheckedChanged(object sender, EventArgs e)
        {
            UpdateRenderOutput();
        }

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
                    else if (result > 200)
                    {
                        _gridSize = 200;
                    }
                }
                this._gridSize = result;
                this.txtGridSize.Text = _gridSize.ToString();
                UpdateRenderOutput();
            }

        }

        private void chkVerticalHinting_CheckedChanged(object sender, EventArgs e)
        {
            UpdateRenderOutput();
        }

        private void chkMasterOutlineAnalysis_CheckedChanged(object sender, EventArgs e)
        {
            UpdateRenderOutput();
        }

        private void chkDrawBone_CheckedChanged(object sender, EventArgs e)
        {
            UpdateRenderOutput();
        }

        private void chkYGridFitting_CheckedChanged(object sender, EventArgs e)
        {
            UpdateRenderOutput();
        }

        private void chkXGridFitting_CheckedChanged(object sender, EventArgs e)
        {
            UpdateRenderOutput();
        }

        private void chkFillBackground_CheckedChanged(object sender, EventArgs e)
        {
            UpdateRenderOutput();
        }

        private void chkLcdTechnique_CheckedChanged(object sender, EventArgs e)
        {
            UpdateRenderOutput();
        }

        private void cmdBuildMsdfTexture_Click(object sender, EventArgs e)
        {
            string sampleFontFile = @"..\..\tahoma.ttf";
            CreateSampleMsdfTextureFont(
                sampleFontFile,
                18,
                0,
                255,
                "d:\\WImageTest\\sample_msdf.png");

        }
        static void CreateSampleMsdfTextureFont(string fontfile, float sizeInPoint, ushort startGlyphIndex, ushort endGlyphIndex, string outputFile)
        {
            //sample
            var reader = new OpenTypeReader();
            using (var fs = new FileStream(fontfile, FileMode.Open))
            {
                //1. read typeface from font file
                Typeface typeface = reader.Read(fs);

                //sample: create sample msdf texture 
                //-------------------------------------------------------------
                var builder = new GlyphPathBuilderVxs(typeface);
                //builder.UseTrueTypeInterpreter = this.chkTrueTypeHint.Checked;
                //builder.UseVerticalHinting = this.chkVerticalHinting.Checked;
                //-------------------------------------------------------------
                var atlasBuilder = new SimpleFontAtlasBuilder2();
                for (ushort n = startGlyphIndex; n <= endGlyphIndex; ++n)
                {
                    builder.BuildFromGlyphIndex(n, sizeInPoint);
                    float scale = builder.GetPixelScale();
                    scale = 1;
                    List<GlyphContour> contours = builder.GetContours();
                    int j = contours.Count;
                    List<GlyphContour> newFitContours = new List<GlyphContour>();
                    for (int i = 0; i < j; ++i)
                    {
                        newFitContours.Add(CreateFitContourVxs2(contours[i], scale, false, false));
                    }

                    Msdfgen.Shape shape = CreateMsdfShape(newFitContours);
                    shape.InverseYAxis = true;
                    double left, bottom, right, top;
                    shape.findBounds(out left, out bottom, out right, out top);

                    Msdfgen.FloatRGBBmp frgbBmp = new Msdfgen.FloatRGBBmp((int)Math.Ceiling((right - left)), (int)Math.Ceiling((top - bottom)));
                    Msdfgen.EdgeColoring.edgeColoringSimple(shape, 3);
                    Msdfgen.MsdfGenerator.generateMSDF(frgbBmp, shape, 4, new Msdfgen.Vector2(1, 1), new Msdfgen.Vector2(), -1);
                    //-----------------------------------
                    int[] buffer = Msdfgen.MsdfGenerator.ConvertToIntBmp(frgbBmp);
                    int w = frgbBmp.Width;
                    int h = frgbBmp.Height;
                    if (w < 5)
                    {
                        w = 5;
                    }
                    if (h < 5)
                    {
                        h = 5;
                    }
                    ActualImage actualImg = ActualImage.CreateFromBuffer(w, h, PixelFormat.ARGB32, buffer);
                    atlasBuilder.AddGlyph((int)n, actualImg);

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
                    var bmpdata = bmp.LockBits(new Rectangle(0, 0, glyphImg2.Width, glyphImg2.Height),
                        System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
                    int[] intBuffer = glyphImg2.GetImageBuffer();

                    System.Runtime.InteropServices.Marshal.Copy(intBuffer, 0, bmpdata.Scan0, intBuffer.Length);
                    bmp.UnlockBits(bmpdata);
                    bmp.Save("d:\\WImageTest\\a_total.png");
                }
                atlasBuilder.SaveFontInfo("d:\\WImageTest\\a_info.xml");
            }
        }

        private void chkGsubEnableLigature_CheckedChanged(object sender, EventArgs e)
        {
            UpdateRenderOutput();
        }
    }
}
