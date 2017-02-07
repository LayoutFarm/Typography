//MIT, 2016-2017, WinterDev
using System;
using System.Collections.Generic;

using System.Drawing;
using System.IO;
using System.Windows.Forms;

using NOpenType;
using NOpenType.Extensions;

using PixelFarm.Agg;
using PixelFarm.Agg.VertexSource;

namespace SampleWinForms
{
    public partial class Form1 : Form
    {
        Graphics g;
        AggCanvasPainter p;
        ImageGraphics2D imgGfx2d;
        ActualImage destImg;
        Bitmap winBmp;
        static CurveFlattener curveFlattener = new CurveFlattener();

        public Form1()
        {
            InitializeComponent();
            this.Load += new EventHandler(Form1_Load);

            cmbRenderChoices.Items.Add(RenderChoice.RenderWithMiniAgg);
            cmbRenderChoices.Items.Add(RenderChoice.RenderWithPlugableGlyphRasterizer);
            cmbRenderChoices.Items.Add(RenderChoice.RenderWithTextPrinterAndMiniAgg);
            cmbRenderChoices.SelectedIndex = 0;
            cmbRenderChoices.SelectedIndexChanged += new EventHandler(cmbRenderChoices_SelectedIndexChanged);

            this.txtInputChar.Text = "i";

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
        }


        enum RenderChoice
        {
            RenderWithMiniAgg,
            RenderWithPlugableGlyphRasterizer, //new 
            RenderWithTextPrinterAndMiniAgg, //new
        }

        void Form1_Load(object sender, EventArgs e)
        {
            this.Text = "Render with PixelFarm";
            //this.lstFontSizes.SelectedIndex = lstFontSizes.Items.Count - 1;//select last one  
            this.lstFontSizes.SelectedIndex = 0;//select last one  
        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (g == null)
            {
                destImg = new ActualImage(400, 300, PixelFormat.ARGB32);
                imgGfx2d = new ImageGraphics2D(destImg); //no platform
                p = new AggCanvasPainter(imgGfx2d);
                winBmp = new Bitmap(400, 300, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                g = this.CreateGraphics();
            }
            //ReadAndRender(@"..\..\segoeui.ttf");
            ReadAndRender(@"..\..\tahoma.ttf");
            // ReadAndRender(@"..\..\cambriaz.ttf");
            //ReadAndRender(@"..\..\CompositeMS2.ttf");
        }

        float fontSizeInPoint = 14; //default
        void ReadAndRender(string fontfile)
        {
            if (string.IsNullOrEmpty(this.txtInputChar.Text))
            {
                return;
            }
            var reader = new OpenTypeReader();
            char testChar = txtInputChar.Text[0];//only 1 char 
            int resolution = 96;

            using (var fs = new FileStream(fontfile, FileMode.Open))
            {
                //1. read typeface from font file
                Typeface typeFace = reader.Read(fs);

#if DEBUG
                //-----
                //about typeface 
                //short ascender = typeFace.Ascender;
                //short descender = typeFace.Descender;
                //short lineGap = typeFace.LineGap;

                //NOpenType.Tables.UnicodeLangBits test = NOpenType.Tables.UnicodeLangBits.Thai;
                //NOpenType.Tables.UnicodeRangeInfo rangeInfo = test.ToUnicodeRangeInfo();
                //bool doseSupport = typeFace.DoseSupportUnicode(test); 
                ////-----
                ////string inputstr = "ก่นกิ่น";
                //string inputstr = "ญญู";
                //List<int> outputGlyphIndice = new List<int>();
                //typeFace.Lookup(inputstr.ToCharArray(), outputGlyphIndice);
#endif

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



        void RenderWithMiniAgg(Typeface typeface, char testChar, float sizeInPoint)
        {
            //2. glyph-to-vxs builder
            var builder = new GlyphPathBuilderVxs(typeface);
            builder.UseTrueTypeInterpreter = this.chkTrueTypeHint.Checked;
            builder.UseVerticalHinting = this.chkVerticalHinting.Checked;
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

            PixelFarm.Agg.Typography.GlyphFitOutline glyphOutline = null;
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
                    PixelFarm.Agg.Typography.EdgeLine rightside = p.GetMatchingVerticalEdge();
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
            for (int y = 0; y < height; )
            {
                for (int x = 0; x < width; )
                {
                    p.FillRectLBWH(x, y, 1, 1);
                    x += sqSize;
                }
                y += sqSize;
            }
        }
        static void DrawEdge(AggCanvasPainter p, PixelFarm.Agg.Typography.EdgeLine edge, float scale)
        {
            if (edge.IsOutside)
            {
                //free side                
                switch (edge.SlopKind)
                {
                    default:
                        p.StrokeColor = PixelFarm.Drawing.Color.Green;
                        break;
                    case PixelFarm.Agg.Typography.LineSlopeKind.Vertical:
                        if (edge.IsLeftSide)
                        {
                            p.StrokeColor = PixelFarm.Drawing.Color.Blue;
                        }
                        else
                        {
                            p.StrokeColor = PixelFarm.Drawing.Color.LightGray;
                        }
                        break;
                    case PixelFarm.Agg.Typography.LineSlopeKind.Horizontal:
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
                    case PixelFarm.Agg.Typography.LineSlopeKind.Vertical:
                        p.StrokeColor = PixelFarm.Drawing.Color.Blue;
                        break;
                    case PixelFarm.Agg.Typography.LineSlopeKind.Horizontal:
                        p.StrokeColor = PixelFarm.Drawing.Color.Yellow;
                        break;
                }
            }
            p.Line(edge.x0 * scale, edge.y0 * scale, edge.x1 * scale, edge.y1 * scale);
        }

        static void AssignPointEdgeInvolvement(PixelFarm.Agg.Typography.EdgeLine edge)
        {
            if (!edge.IsOutside)
            {
                return;
            }

            switch (edge.SlopKind)
            {

                case PixelFarm.Agg.Typography.LineSlopeKind.Horizontal:
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
                    } break;
                case PixelFarm.Agg.Typography.LineSlopeKind.Vertical:
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
                    } break;
            }

        }
        PixelFarm.Agg.Typography.GlyphFitOutline TessWithPolyTri(List<GlyphContour> contours, float pixelScale)
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
            PixelFarm.Agg.Typography.GlyphFitOutline glyphFitOutline = new PixelFarm.Agg.Typography.GlyphFitOutline(polygon);
            glyphFitOutline.Analyze();
            //------------------------------------------

            List<PixelFarm.Agg.Typography.GlyphTriangle> triAngles = glyphFitOutline.dbugGetTriangles();
            int triangleCount = triAngles.Count;

            bool drawBone = this.chkDrawBone.Checked;
            for (int i = 0; i < triangleCount; ++i)
            {
                //---------------
                PixelFarm.Agg.Typography.GlyphTriangle tri = triAngles[i];
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
        void debugDrawTriangulatedGlyph(PixelFarm.Agg.Typography.GlyphFitOutline glyphFitOutline, float pixelScale)
        {
            p.StrokeColor = PixelFarm.Drawing.Color.Magenta;
            List<PixelFarm.Agg.Typography.GlyphTriangle> triAngles = glyphFitOutline.dbugGetTriangles();
            int j = triAngles.Count;
            //
            double prev_cx = 0, prev_cy = 0;
            // 
            bool drawBone = this.chkDrawBone.Checked;

            for (int i = 0; i < j; ++i)
            {
                //---------------
                PixelFarm.Agg.Typography.GlyphTriangle tri = triAngles[i];
                PixelFarm.Agg.Typography.EdgeLine e0 = tri.e0;
                PixelFarm.Agg.Typography.EdgeLine e1 = tri.e1;
                PixelFarm.Agg.Typography.EdgeLine e2 = tri.e2;
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
                List<PixelFarm.Agg.Typography.GlyphBone> bones = glyphFitOutline.dbugGetBones();
                j = bones.Count;
                for (int i = 0; i < j; ++i)
                {
                    PixelFarm.Agg.Typography.GlyphBone b = bones[i];
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
            var gdiGlyphRasterizer = new NOpenType.CLI.GDIGlyphRasterizer();
            var builder = new GlyphPathBuilder(typeface, gdiGlyphRasterizer);
            builder.UseTrueTypeInterpreter = this.chkTrueTypeHint.Checked;
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
            printer.EnableKerning = this.chkKern.Checked;
            printer.EnableTrueTypeHint = this.chkTrueTypeHint.Checked;
            printer.UseAggVerticalHinting = this.chkVerticalHinting.Checked;

            int len = str.Length;

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
                    p.SetOrigin(cx, cy);
                    p.Fill(glyphPlan.vxs);
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
                    p.Draw(glyphPlan.vxs);
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
            button1_Click(this, EventArgs.Empty);
        }
        void cmbRenderChoices_SelectedIndexChanged(object sender, EventArgs e)
        {
            button1_Click(this, EventArgs.Empty);
        }

        private void lstFontSizes_SelectedIndexChanged(object sender, EventArgs e)
        {
            //new font size
            fontSizeInPoint = (int)lstFontSizes.SelectedItem;
            button1_Click(this, EventArgs.Empty);
        }

        private void chkKern_CheckedChanged(object sender, EventArgs e)
        {
            button1_Click(this, EventArgs.Empty);
        }

        private void chkTrueTypeHint_CheckedChanged(object sender, EventArgs e)
        {
            button1_Click(this, EventArgs.Empty);
        }

        private void chkShowTess_CheckedChanged(object sender, EventArgs e)
        {
            button1_Click(this, EventArgs.Empty);
        }

        private void chkShowGrid_CheckedChanged(object sender, EventArgs e)
        {
            button1_Click(this, EventArgs.Empty);
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
                button1_Click(this, EventArgs.Empty);
            }

        }

        private void chkVerticalHinting_CheckedChanged(object sender, EventArgs e)
        {
            button1_Click(this, EventArgs.Empty);
        }

        private void chkMasterOutlineAnalysis_CheckedChanged(object sender, EventArgs e)
        {
            button1_Click(this, EventArgs.Empty);
        }

        private void chkDrawBone_CheckedChanged(object sender, EventArgs e)
        {
            button1_Click(this, EventArgs.Empty);
        }

        private void chkYGridFitting_CheckedChanged(object sender, EventArgs e)
        {
            button1_Click(this, EventArgs.Empty);
        }

        private void chkXGridFitting_CheckedChanged(object sender, EventArgs e)
        {
            button1_Click(this, EventArgs.Empty);
        }

        private void chkFillBackground_CheckedChanged(object sender, EventArgs e)
        {
            button1_Click(this, EventArgs.Empty);
        }

        private void chkLcdTechnique_CheckedChanged(object sender, EventArgs e)
        {
            button1_Click(this, EventArgs.Empty);
        }

        private void cmdAggLcd1_Click(object sender, EventArgs e)
        {


            //1. create simple vertical line to test agg's lcd rendernig technique
            //create gray-scale actual image
            ActualImage glyphImg = new ActualImage(100, 100, PixelFormat.ARGB32);
            ImageGraphics2D glyph2d = new ImageGraphics2D(glyphImg);
            AggCanvasPainter painter = new AggCanvasPainter(glyph2d);

            painter.StrokeColor = PixelFarm.Drawing.Color.Black;
            painter.StrokeWidth = 2.0f;
            painter.Line(0, 0, 15 * 3, 15); //scale horizontal 3 times, 

            //painter.Line(2, 0, 2, 15);
            //painter.Line(2, 0, 20, 20);
            //painter.Line(2, 0, 30, 15);
            //painter.Line(2, 0, 30, 5);
            //clear surface bg
            p.Clear(PixelFarm.Drawing.Color.White);
            //draw img into that bg
            //--------------- 
            //convert glyphImg from RGBA to grey Scale buffer
            //---------------
            //lcd process ...
            byte[] glyphGreyScale = CreateGreyScaleBuffer(glyphImg);
            //
            int newGreyBuffWidth;
            byte[] greyBuff = CreateNewExpandedLcdGrayScale(glyphGreyScale, glyphImg.Width, glyphImg.Height, out newGreyBuffWidth);
            //blend lcd
            BlendWithLcdSpans(destImg, greyBuff, newGreyBuffWidth, glyphImg.Height);
            //--------------- 
            p.DrawImage(glyphImg, 0, 50);

            PixelFarm.Agg.Imaging.BitmapHelper.CopyToGdiPlusBitmapSameSize(destImg, winBmp);
            //--------------- 
            //7. just render our bitmap
            g.Clear(Color.White);
            g.DrawImage(winBmp, new Point(30, 20));
        }
        static byte[] CreateGreyScaleBuffer(ActualImage img)
        {
            //assume img is 32 rgba img
            int imgW = img.Width;
            int height = img.Height;
            //56 level grey scale buffer

            byte[] srcImgBuffer = ActualImage.GetBuffer(img);
            int greyScaleBufferLen = imgW * height;
            byte[] greyScaleBuffer = new byte[greyScaleBufferLen];

            //for (int i = greyScaleBufferLen - 1; i >= 0; --i)
            //{
            //    greyScaleBuffer[i] = 64;
            //}


            int destIndex = 0;
            int srcImgIndex = 0;
            int srcImgStride = img.Stride;

            for (int y = 0; y < height; ++y)
            {
                srcImgIndex = srcImgStride * y;
                destIndex = imgW * y;
                for (int x = 0; x < imgW; ++x)
                {
                    byte r = srcImgBuffer[srcImgIndex];
                    byte g = srcImgBuffer[srcImgIndex + 1];
                    byte b = srcImgBuffer[srcImgIndex + 2];
                    byte a = srcImgBuffer[srcImgIndex + 3];
                    if (r != 0 || g != 0 || b != 0)
                    {
                    }
                    if (a != 255)
                    {

                    }
                    //skip alpha
                    //byte greyScaleValue =
                    //    (byte)((0.333f * (float)r) + (0.5f * (float)g) + (0.1666f * (float)b));

                    greyScaleBuffer[destIndex] = (byte)(((a + 1) / 256f) * 64f);

                    destIndex++;
                    srcImgIndex += 4;
                }
            }
            return greyScaleBuffer;
        }

        void BlendWithLcdSpans(ActualImage destImg, byte[] greyBuff, int greyBufferWidth, int greyBufferHeight)
        {

            PixelFarm.Drawing.Color color = PixelFarm.Drawing.Color.Black;
            for (int y = 0; y < greyBufferHeight; ++y)
            {
                BlendLcdSpan(destImg, greyBuff, color, 0, y, greyBufferWidth);
            }
            //SwapRB(destImg);
        }




        LcdDistributionLut g8_1_2lcd = new LcdDistributionLut(LcdDistributionLut.GrayLevels.Gray8, 0.5, 0.25, 0.125);
        void BlendWithLcdTechnique(ActualImage destImg, ActualImage glyphImg, PixelFarm.Drawing.Color color)
        {
            var g8Lut = g8_1_2lcd;
            var forwardBuffer = new ScanlineSubPixelRasterizer.ForwardTemporaryBuffer();
            int glyphH = glyphImg.Height;
            int glyphW = glyphImg.Width;
            byte[] glyphBuffer = ActualImage.GetBuffer(glyphImg);
            int srcIndex = 0;
            int srcStride = glyphImg.Stride;
            byte[] destImgBuffer = ActualImage.GetBuffer(destImg);
            //start pixel
            int destImgIndex = 0;
            int destX = 0;
            byte[] rgb = new byte[]{
                color.R,
                color.G,
                color.B
            };

            byte color_a = color.alpha;

            for (int y = 0; y < glyphH; ++y)
            {
                srcIndex = srcStride * y;
                destImgIndex = (destImg.Stride * y) + (destX * 4); //4 color component
                int i = 0;
                int round = 0;
                forwardBuffer.Reset();
                byte e0 = 0;
                for (int x = 0; x < glyphW; ++x)
                {
                    //1.
                    //read 1 pixel (4 bytes, 4 color components)
                    byte r = glyphBuffer[srcIndex];
                    byte g = glyphBuffer[srcIndex + 1];
                    byte b = glyphBuffer[srcIndex + 2];
                    byte a = glyphBuffer[srcIndex + 3];
                    //2.
                    //convert to grey scale and convert to 65 level grey scale value
                    byte greyScaleValue = g8Lut.Convert255ToLevel(a);
                    //3.
                    //from single grey scale value it is expanded into 5 color component
                    for (int n = 0; n < 3; ++n)
                    {
                        forwardBuffer.WriteAccum(
                            g8Lut.Tertiary(greyScaleValue),
                            g8Lut.Secondary(greyScaleValue),
                            g8Lut.Primary(greyScaleValue));
                        //4. read accumulate 'energy' back 
                        forwardBuffer.ReadNext(out e0);
                        //5. blend this pixel to dest image (expand to 5 (sub)pixel) 
                        //------------------------------------------------------------
                        ScanlineSubPixelRasterizer.BlendSpan(e0 * color_a, rgb, ref i, destImgBuffer, ref destImgIndex, ref round);
                        //------------------------------------------------------------
                    }
                    srcIndex += 4;
                }
                //---------
                //when finish each line
                //we must draw extened 4 pixels
                //---------
                {
                    byte e1, e2, e3, e4;
                    forwardBuffer.ReadRemaining4(out e1, out e2, out e3, out e4);
                    int remainingEnergy = Math.Min(srcStride, 4);
                    switch (remainingEnergy)
                    {
                        default: throw new NotSupportedException();
                        case 4:
                            ScanlineSubPixelRasterizer.BlendSpan(e1 * color_a, rgb, ref i, destImgBuffer, ref destImgIndex, ref round);
                            ScanlineSubPixelRasterizer.BlendSpan(e2 * color_a, rgb, ref i, destImgBuffer, ref destImgIndex, ref round);
                            ScanlineSubPixelRasterizer.BlendSpan(e3 * color_a, rgb, ref i, destImgBuffer, ref destImgIndex, ref round);
                            ScanlineSubPixelRasterizer.BlendSpan(e4 * color_a, rgb, ref i, destImgBuffer, ref destImgIndex, ref round);
                            break;
                        case 3:
                            ScanlineSubPixelRasterizer.BlendSpan(e1 * color_a, rgb, ref i, destImgBuffer, ref destImgIndex, ref round);
                            ScanlineSubPixelRasterizer.BlendSpan(e2 * color_a, rgb, ref i, destImgBuffer, ref destImgIndex, ref round);
                            ScanlineSubPixelRasterizer.BlendSpan(e3 * color_a, rgb, ref i, destImgBuffer, ref destImgIndex, ref round);
                            break;
                        case 2:
                            ScanlineSubPixelRasterizer.BlendSpan(e1 * color_a, rgb, ref i, destImgBuffer, ref destImgIndex, ref round);
                            ScanlineSubPixelRasterizer.BlendSpan(e2 * color_a, rgb, ref i, destImgBuffer, ref destImgIndex, ref round);
                            break;
                        case 1:
                            ScanlineSubPixelRasterizer.BlendSpan(e1 * color_a, rgb, ref i, destImgBuffer, ref destImgIndex, ref round);
                            break;
                        case 0:
                            //nothing
                            break;
                    }
                }
            }
        }
        void BlendLcdSpan(ActualImage destImg, byte[] expandGreyBuffer,
            PixelFarm.Drawing.Color color, int x, int y, int width)
        {
            byte[] rgb = new byte[3]{
                color.R,
                color.G,
                color.B
            };
            //-------------------------
            //destination
            byte[] destImgBuffer = ActualImage.GetBuffer(destImg);
            //start pixel
            int destImgIndex = (x * 4) + (destImg.Stride * y);
            //start img src
            int srcImgIndex = x + (width * y);
            int spanIndex = srcImgIndex;
            int i = x % 3;

            int round = 0;

            do
            {
                int a0 = expandGreyBuffer[spanIndex] * color.alpha;
                byte existingColor = destImgBuffer[destImgIndex];
                byte newValue = (byte)((((rgb[i] - existingColor) * a0) + (existingColor << 16)) >> 16);
                destImgBuffer[destImgIndex] = newValue;
                //move to next dest
                destImgIndex++;


                i++;
                if (i > 2)
                {
                    i = 0;//reset
                }
                round++;
                if (round > 2)
                {
                    //this is alpha chanel
                    //so we skip alpha byte to next

                    //and swap rgb of latest write pixel
                    //--------------------------
                    //in-place swap
                    byte r = destImgBuffer[destImgIndex - 1];
                    byte b = destImgBuffer[destImgIndex - 3];
                    destImgBuffer[destImgIndex - 3] = r;
                    destImgBuffer[destImgIndex - 1] = b;
                    //--------------------------

                    destImgIndex++;
                    round = 0;
                }
                spanIndex++;
                srcImgIndex++;


            } while (--width > 0);


        }
        static void SwapRB(ActualImage destImg)
        {
            byte[] destImgBuffer = ActualImage.GetBuffer(destImg);
            int width = destImg.Width;
            int height = destImg.Height;
            int destIndex = 0;
            for (int y = 0; y < height; ++y)
            {
                destIndex = (y * destImg.Stride);
                for (int x = 0; x < width; ++x)
                {
                    byte r = destImgBuffer[destIndex];
                    byte g = destImgBuffer[destIndex + 1];
                    byte b = destImgBuffer[destIndex + 2];
                    byte a = destImgBuffer[destIndex + 3];
                    //swap
                    destImgBuffer[destIndex + 2] = r;
                    destImgBuffer[destIndex] = b;
                    destIndex += 4;
                }
            }
        }
        // Swap Blue and Red, that is convert RGB->BGR or BGR->RGB
        ////---------------------------------
        //void swap_rb(unsigned char* buf, unsigned width, unsigned height, unsigned stride)
        //{
        //    unsigned x, y;
        //    for(y = 0; y < height; ++y)
        //    {
        //        unsigned char* p = buf + stride * y;
        //        for(x = 0; x < width; ++x)
        //        {
        //            unsigned char v = p[0];
        //            p[0] = p[2];
        //            p[2] = v;
        //            p += 3;
        //        }
        //    }
        //}
        //// Blend one span into the R-G-B 24 bit frame buffer
        //// For the B-G-R byte order or for 32-bit buffers modify
        //// this function accordingly. The general idea is 'span' 
        //// contains alpha values for individual color channels in the 
        //// R-G-B order, so, for the B-G-R order you will have to 
        //// choose values from the 'span' array differently
        ////---------------------------------
        //void blend_lcd_span(int x, 
        //                    int y, 
        //                    const unsigned char* span, 
        //                    int width, 
        //                    const rgba& color, 
        //                    unsigned char* rgb24_buf, 
        //                    unsigned rgb24_stride)
        //{
        //    unsigned char* p = rgb24_buf + rgb24_stride * y + x;
        //    unsigned char rgb[3] = { color.r, color.g, color.b };
        //    int i = x % 3;
        //    do
        //    {
        //        int a0 = int(*span++) * color.a;
        //        *p++ = (unsigned char)((((rgb[i++] - *p) * a0) + (*p << 16)) >> 16);
        //        if(i > 2) i = 0;
        //    }
        //    while(--width);
        //}

        /// <summary>
        /// convert from original grey scale to expand lcd-ready grey scale ***
        /// </summary>
        /// <param name="src"></param>
        /// <param name="srcW"></param>
        /// <param name="srcH"></param>
        /// <param name="newImageStride"></param>
        /// <returns></returns>
        static byte[] CreateNewExpandedLcdGrayScale(byte[] src, int srcW, int srcH, out int newImageStride)
        {
            //version 1:
            //agg lcd test
            //lcd_distribution_lut<ggo_gray8> lut(1.0/3.0, 2.0/9.0, 1.0/9.0);
            //lcd_distribution_lut<ggo_gray8> lut(0.5, 0.25, 0.125);
            LcdDistributionLut lut = new LcdDistributionLut(LcdDistributionLut.GrayLevels.Gray8, 0.5, 0.25, 0.125);
            int destImgStride = srcW + 4; //expand the original gray scale 
            newImageStride = destImgStride;

            byte[] destBuffer = new byte[destImgStride * srcH];


            int destImgIndex = 0;
            int srcImgIndex = 0;
            for (int y = 0; y < srcH; ++y)
            {

                //find destination img
                srcImgIndex = y * srcW;
                destImgIndex = y * destImgStride; //start at new line  
                for (int x = 0; x < srcW; ++x)
                {
                    //convert to grey scale  
                    int v = src[srcImgIndex];// (int)((greyScaleValue / 255f) * 65f);
                    //----------------------------------
                    destBuffer[destImgIndex] += lut.Tertiary(v);
                    destBuffer[destImgIndex + 1] += lut.Secondary(v);
                    destBuffer[destImgIndex + 2] += lut.Primary(v);
                    destBuffer[destImgIndex + 3] += lut.Secondary(v);
                    destBuffer[destImgIndex + 4] += lut.Tertiary(v);
                    destImgIndex++;
                    srcImgIndex++;
                }
            }
            return destBuffer;
        }

        private void cmdAggLcd2_Click(object sender, EventArgs e)
        {
            //version 2:
            //1. create simple vertical line to test agg's lcd rendernig technique
            //create gray-scale actual image
            ActualImage glyphImg = new ActualImage(100, 100, PixelFormat.ARGB32);
            ImageGraphics2D glyph2d = new ImageGraphics2D(glyphImg);
            AggCanvasPainter painter = new AggCanvasPainter(glyph2d);

            painter.StrokeColor = PixelFarm.Drawing.Color.Black;
            painter.StrokeWidth = 1.0f;
            painter.Line(0, 0, 15, 20);//not need to scale3

            //clear surface bg
            p.Clear(PixelFarm.Drawing.Color.White);
            //--------------------------
            BlendWithLcdTechnique(destImg, glyphImg, PixelFarm.Drawing.Color.Black);


            //--------------- 
            p.DrawImage(glyphImg, 0, 50);

            PixelFarm.Agg.Imaging.BitmapHelper.CopyToGdiPlusBitmapSameSize(destImg, winBmp);
            //--------------- 
            //7. just render our bitmap
            g.Clear(Color.White);
            g.DrawImage(winBmp, new Point(30, 20));
        }
        private void cmdAggLcd3_Click(object sender, EventArgs e)
        {
            //version 3: 
            //
            p.Clear(PixelFarm.Drawing.Color.White);
            //--------------------------
            p.StrokeColor = PixelFarm.Drawing.Color.Black;
            p.StrokeWidth = 1.0f;
            p.UseSubPixelRendering = true;
            p.Line(0, 0, 15, 20);
            //p.UseSubPixelRendering = false;
            //p.Line(30, 0, 45, 20);
            //--------------------------
            PixelFarm.Agg.Imaging.BitmapHelper.CopyToGdiPlusBitmapSameSize(destImg, winBmp);
            //--------------- 
            //7. just render our bitmap
            g.Clear(Color.White);
            g.DrawImage(winBmp, new Point(30, 20));
        }

    }
}
