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

            this.txtInputChar.Text = "x";

            lstFontSizes.Items.AddRange(
                new object[]{
                    8, 9,
                    10,11,
                    12,
                    14,
                    16,
                    18,20,22,24,26,28,36,48,72,240,300
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
            this.lstFontSizes.SelectedIndex = lstFontSizes.Items.Count - 1;//select last one  
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
            //ReadAndRender(@"..\..\cambriaz.ttf");
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

            //5. use PixelFarm's Agg to render to bitmap...
            //5.1 clear background
            p.Clear(PixelFarm.Drawing.Color.White);

            if (chkFillBackground.Checked)
            {
                //5.2 
                p.FillColor = PixelFarm.Drawing.Color.Black;
                //5.3
                p.Fill(vxs);
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

            if (chkMasterOutlineAnalysis.Checked)
            {
                List<GlyphContour> contours = builder.GetContours();
                int j = contours.Count;
                float pixelScale = builder.GetPixelScale();
                for (int i = 0; i < j; ++i)
                {
                    DrawGlyphControlPoints3(contours[i], p, pixelScale);
                }
            }

            if (chkShowTess.Checked)
            {
                //draw for debug ...
                //draw control point
                List<GlyphContour> contours = builder.GetContours();
                float scale = builder.GetPixelScale();
                TessWithPolyTriAndDraw(contours, p, scale);
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
                        p.StrokeColor = PixelFarm.Drawing.Color.Magenta;
                        break;
                    case PixelFarm.Agg.Typography.LineSlopeKind.Horizontal:
                        p.StrokeColor = PixelFarm.Drawing.Color.Red;
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
        void TessWithPolyTriAndDraw(List<GlyphContour> contours, AggCanvasPainter p, float scale)
        {


            List<Poly2Tri.PolygonPoint> points = new List<Poly2Tri.PolygonPoint>();
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

            Poly2Tri.P2T.Triangulate(polygon); //that poly is triangulated

            PixelFarm.Agg.Typography.GlyphFitOutline glyphFitOutline = new PixelFarm.Agg.Typography.GlyphFitOutline(polygon);
            glyphFitOutline.Analyze();

            p.StrokeColor = PixelFarm.Drawing.Color.Magenta;
#if DEBUG
            List<PixelFarm.Agg.Typography.GlyphTriangle> triAngles = glyphFitOutline.dbugGetTriangles();

            double prev_cenX = 0;
            double prev_cenY = 0;

            int j = triAngles.Count;
            for (int i = 0; i < j; ++i)
            {
                PixelFarm.Agg.Typography.GlyphTriangle tri = triAngles[i];
                PixelFarm.Agg.Typography.EdgeLine e0 = tri.e0;
                PixelFarm.Agg.Typography.EdgeLine e1 = tri.e1;
                PixelFarm.Agg.Typography.EdgeLine e2 = tri.e2;

                //draw each triangles
                DrawEdge(p, e0, scale);
                DrawEdge(p, e1, scale);
                DrawEdge(p, e2, scale);

                //draw centroid
                double cen_x = tri.CentroidX;
                double cen_y = tri.CentroidY;
                p.FillColor = PixelFarm.Drawing.Color.Yellow;
                p.FillRectLBWH(cen_x * scale, cen_y * scale, 2, 2);
                if (i == 0)
                {
                    //start mark
                    p.FillColor = PixelFarm.Drawing.Color.Yellow;
                    p.FillRectLBWH(cen_x * scale, cen_y * scale, 7, 7);
                }
                else
                {
                    //draw line from prev centroid to this centroid
                    p.StrokeColor = PixelFarm.Drawing.Color.Red;
                    p.Line(prev_cenX * scale, prev_cenY * scale, cen_x * scale, cen_y * scale);
                }
                prev_cenX = cen_x;
                prev_cenY = cen_y;
            }


#endif

            //---------------
            //List<EdgeLine> edges = new List<EdgeLine>();
            //foreach (var tri in polygon.Triangles)
            //{
            //    //draw each triangles
            //    p.Line(tri.P0.X * scale, tri.P0.Y * scale, tri.P1.X * scale, tri.P1.Y * scale);
            //    p.Line(tri.P1.X * scale, tri.P1.Y * scale, tri.P2.X * scale, tri.P2.Y * scale);
            //    p.Line(tri.P2.X * scale, tri.P2.Y * scale, tri.P0.X * scale, tri.P0.Y * scale);

            //    edges.Add(new EdgeLine(tri.P0, tri.P1));
            //    edges.Add(new EdgeLine(tri.P1, tri.P2));
            //    edges.Add(new EdgeLine(tri.P2, tri.P0));

            //    //find center of each triangle
            //    //--------------------------------------------- 
            //    var p_centerx = (tri.P0.X + tri.P1.X + tri.P2.X) * scale;
            //    var p_centery = (tri.P0.Y + tri.P1.Y + tri.P2.Y) * scale;

            //    p.FillRectLBWH(p_centerx / 3, p_centery / 3, 2, 2);
            //}
            //-------------------

            //sort
            //remove duplicated edge?

            //edges.Sort((e1, e2) =>
            //{
            //    if (e1.y1 == e2.y1)
            //    {
            //        return e1.x0.CompareTo(e2.x0);
            //    }
            //    else
            //    {
            //        return e1.y1.CompareTo(e2.y1);
            //    }
            //});
            ////remove shared edge


            //for (int i = edges.Count - 1; i > 0; --i)
            //{
            //    EdgeLine e_now = edges[i];
            //    EdgeLine e_prev = edges[i - 1];

            //    if (e_now.SameCoordinateWidth(e_prev))
            //    {
            //        //remove the two
            //        //TODO: remove if we can have more than duplicate 2 edges
            //        edges.RemoveAt(i);
            //        edges.RemoveAt(i - 1);
            //        --i;
            //    }
            //}

            //for (int i = edges.Count - 1; i > 0; --i)
            //{
            //    //draw only unique edge
            //    EdgeLine e = edges[i];
            //    p.Line(e.x0, e.y0, e.x1, e.y1);
            //}

        }

        class EdgeLine
        {
            public double x0;
            public double y0;
            public double x1;
            public double y1;
            public EdgeLine(Poly2Tri.TriangulationPoint p, Poly2Tri.TriangulationPoint q)
            {
                x0 = p.X;
                y0 = p.Y;
                x1 = q.X;
                y1 = q.Y;
                //
                Arrange();

            }
            void Arrange()
            {
                if (y1 < y0)
                {
                    //swap
                    double tmp_y = y1;
                    y1 = y0;
                    y0 = tmp_y;
                    //swap x 
                    double tmp_x = x1;
                    x1 = x0;
                    x0 = tmp_x;
                }
                else if (y1 == y0)
                {
                    if (x1 < x0)
                    {
                        //swap
                        //swap
                        double tmp_y = y1;
                        y1 = y0;
                        y0 = tmp_y;
                        //swap x 
                        double tmp_x = x1;
                        x1 = x0;
                        x0 = tmp_x;
                    }
                }
            }
            public override string ToString()
            {
                return x0 + "," + y0 + "," + x1 + "," + y1;
            }
            public bool SameCoordinateWidth(EdgeLine another)
            {
                return this.x0 == another.x0 &&
                    this.x1 == another.x1 &&
                    this.y0 == another.y0 &&
                    this.y1 == another.y1;
            }


        }

        struct TmpPoint
        {
            public double x;
            public double y;
#if DEBUG
            public override string ToString()
            {
                return x + "," + y;
            }
#endif
        }
        /// <summary>
        /// create polygon from original master outline point,
        /// fix duplicated point
        /// </summary>
        /// <param name="cnt"></param>
        /// <returns></returns>
        static Poly2Tri.Polygon CreatePolygon1(GlyphContour cnt)
        {
            List<Poly2Tri.PolygonPoint> points = new List<Poly2Tri.PolygonPoint>();
            List<float> allPoints = cnt.allPoints;
            int lim = allPoints.Count - 1;

            //limitation: poly tri not accept duplicated points!
            double prevX = 0;
            double prevY = 0;

            Dictionary<TmpPoint, bool> tmpPoints = new Dictionary<TmpPoint, bool>();
            for (int i = 0; i < lim; )
            {
                var x = allPoints[i];
                var y = allPoints[i + 1];
                //
                if (x != prevX && y != prevY)
                {
                    TmpPoint tmp_point = new TmpPoint();
                    tmp_point.x = x;
                    tmp_point.y = y;
                    if (!tmpPoints.ContainsKey(tmp_point))
                    {
                        tmpPoints.Add(tmp_point, true);
                        points.Add(new Poly2Tri.PolygonPoint(
                            x,
                            y));
                    }
                    else
                    {
                        //temp fixed***
                        while (true)
                        {
                            x += 0.1f;
                            y += 0.1f;

                            tmp_point.x = x;
                            tmp_point.y = y;
                            if (!tmpPoints.ContainsKey(tmp_point))
                            {
                                tmpPoints.Add(tmp_point, true);
                                points.Add(new Poly2Tri.PolygonPoint(
                                    x,
                                    y));
                                break;
                            }
                            else
                            {

                            }
                        }
                    }

                    prevX = x;
                    prevY = y;

                }
                else
                {
                    //a duplicate point
                    //temp fix***
                    //minor shift x and y
                    x += 0.5f;
                    y += 0.5f;


                    TmpPoint tmp_point = new TmpPoint();
                    tmp_point.x = x;
                    tmp_point.y = y;
                    if (!tmpPoints.ContainsKey(tmp_point))
                    {
                        tmpPoints.Add(tmp_point, true);
                        points.Add(new Poly2Tri.PolygonPoint(
                            x,
                            y));
                    }
                    else
                    {
                    }

                    prevX = x;
                    prevY = y;
                }
                i += 2;
            }

            Poly2Tri.Polygon polygon = new Poly2Tri.Polygon(points.ToArray());
            return polygon;
        }

        /// <summary>
        /// create polygon from flatten curve outline point
        /// </summary>
        /// <param name="cnt"></param>
        /// <returns></returns>
        static Poly2Tri.Polygon CreatePolygon2(GlyphContour cnt)
        {
            List<Poly2Tri.PolygonPoint> points = new List<Poly2Tri.PolygonPoint>();
            List<GlyphPart> allParts = cnt.parts;
            //---------------------------------------
            //merge all generated points
            //also remove duplicated point too!
            List<float> allPoints = new List<float>();

            {
                int tt = 0;
                int j = allParts.Count;

                for (int i = 0; i < j; ++i)
                {
                    GlyphPart p = allParts[i];
#if DEBUG
                    if (allPoints.Count >= 30)
                    {

                    }
#endif
                    List<GlyphPoint2D> fpoints = p.GetFlattenPoints();
                    if (tt == 0)
                    {
                        int n = fpoints.Count;
                        for (int m = 0; m < n; ++m)
                        {
                            GlyphPoint2D fp = fpoints[m];
                            allPoints.Add((float)fp.x);
                            allPoints.Add((float)fp.y);
                        }
                        tt++;
                    }
                    else
                    {
                        //except first point
                        int n = fpoints.Count;
                        for (int m = 1; m < n; ++m)
                        {
                            GlyphPoint2D fp = fpoints[m];
                            allPoints.Add((float)fp.x);
                            allPoints.Add((float)fp.y);
                        }
                    }

                }

            }
            //---------------------------------------
            {
                //check last (x,y) and first (x,y)
                int lim = allPoints.Count - 1;
                {
                    if (allPoints[lim] == allPoints[1]
                        && allPoints[lim - 1] == allPoints[0])
                    {
                        //remove last (x,y)
                        allPoints.RemoveAt(lim);
                        allPoints.RemoveAt(lim - 1);
                        lim -= 2;
                    }
                }




                //limitation: poly tri not accept duplicated points!
                double prevX = 0;
                double prevY = 0;
                Dictionary<TmpPoint, bool> tmpPoints = new Dictionary<TmpPoint, bool>();
                for (int i = 0; i < lim; )
                {
                    var x = allPoints[i];
                    var y = allPoints[i + 1];

                    if (x == prevX && y == prevY)
                    {
                        throw new NotSupportedException();
                    }
                    else
                    {
                        TmpPoint tmp_point = new TmpPoint();
                        tmp_point.x = x;
                        tmp_point.y = y;
                        if (!tmpPoints.ContainsKey(tmp_point))
                        {
                            tmpPoints.Add(tmp_point, true);
                            points.Add(new Poly2Tri.PolygonPoint(
                                x,
                                y));
                        }
                        else
                        {

                            throw new NotSupportedException();
                            //temp fixed***

                            //while (true)
                            //{
                            //    x += 0.1f;
                            //    y += 0.1f;

                            //    tmp_point.x = x;
                            //    tmp_point.y = y;
                            //    if (!tmpPoints.ContainsKey(tmp_point))
                            //    {
                            //        tmpPoints.Add(tmp_point, true);
                            //        points.Add(new Poly2Tri.PolygonPoint(
                            //            x,
                            //            y));
                            //        break;
                            //    }
                            //    else
                            //    {

                            //    }
                            //}
                        }

                        prevX = x;
                        prevY = y;

                    }
                    //if (x != prevX && y != prevY)
                    //{


                    //}
                    //else
                    //{
                    //    //a duplicate point
                    //    //temp fix***
                    //    //minor shift x and y
                    //    x += 0.5f;
                    //    y += 0.5f;
                    //    TmpPoint tmp_point = new TmpPoint();
                    //    tmp_point.x = x;
                    //    tmp_point.y = y;
                    //    if (!tmpPoints.ContainsKey(tmp_point))
                    //    {
                    //        tmpPoints.Add(tmp_point, true);
                    //        points.Add(new Poly2Tri.PolygonPoint(
                    //            x,
                    //            y));
                    //    }
                    //    else
                    //    {
                    //    }

                    //    prevX = x;
                    //    prevY = y;
                    //}
                    i += 2;
                }

                Poly2Tri.Polygon polygon = new Poly2Tri.Polygon(points.ToArray());
                return polygon;
            }
        }

        void DrawGlyphControlPoints3(GlyphContour cnt, AggCanvasPainter p, float pixelScale)
        {

        }
        void DrawGlyphControlPoints2(GlyphContour cnt, AggCanvasPainter p, float pixelScale)
        {
            //for debug
            List<GlyphPart> parts = cnt.parts;
            int j = parts.Count;

            int gridSize = this._gridSize;
            int halfGrid = gridSize / 2;

            //------------------------------------------------
            p.FillColor = PixelFarm.Drawing.Color.Black;
            VertexStore vxs = new VertexStore();

            int totalCount = 0;

            for (int i = 0; i < j; ++i)
            {
                GlyphPart part = parts[i];
                List<GlyphPoint2D> points = part.GetFlattenPoints();
                int n = points.Count;

                for (int m = 0; m < n; ++m)
                {
                    GlyphPoint2D point = points[m];
                    int proper = (int)Math.Round(point.y, 0);

                    int lower = (int)(point.y);
                    int snap1_diff = lower % 5;
                    float f_y = proper;
                    float f_x = (float)point.x;

                    if (point.kind == PointKind.CurveInbetween)
                    {

                    }
                    else
                    {
                        if (proper - lower != 0)
                        {
                            if (snap1_diff > halfGrid)
                            {
                                f_y = ((lower / gridSize) + 1);
                                f_y *= gridSize;
                            }
                            else
                            {
                                f_y = (lower / gridSize);
                                f_y *= gridSize;
                            }
                        }
                        else
                        {
                            f_y = (lower / gridSize);
                            f_y *= gridSize;
                        }
                    }

                    f_x = f_x * pixelScale;
                    f_y = f_y * pixelScale;

                    if (totalCount == 0)
                    {
                        vxs.AddMoveTo(f_x, f_y);
                    }
                    else
                    {
                        vxs.AddLineTo(f_x, f_y);
                    }

                    totalCount++;
                }

            }

            vxs.AddCloseFigure();
            p.Fill(vxs);

            //------------------
            for (int i = 0; i < j; ++i)
            {
                GlyphPart part = parts[i];
                List<GlyphPoint2D> points = part.GetFlattenPoints();
                int n = points.Count;
                for (int m = 0; m < n; ++m)
                {
                    GlyphPoint2D point = points[m];
                    switch (point.kind)
                    {
                        default:
                            p.FillColor = PixelFarm.Drawing.Color.Green;
                            break;
                        case PointKind.CurveInbetween:
                            p.FillColor = PixelFarm.Drawing.Color.Red;
                            break;
                    }

                    int proper = (int)Math.Round(point.y, 0);
                    int lower = (int)(point.y);
                    int snap1_diff = lower % 5;
                    float f_y = proper;
                    float f_x = (float)point.x;

                    if (point.kind == PointKind.CurveInbetween)
                    {

                    }
                    else
                    {

                        if (proper - lower != 0)
                        {
                            if (snap1_diff > halfGrid)
                            {
                                f_y = ((lower / gridSize) + 1);
                                f_y *= gridSize;
                            }
                            else
                            {
                                f_y = (lower / gridSize);
                                f_y *= gridSize;
                            }
                        }
                        else
                        {
                            f_y = (lower / gridSize);
                            f_y *= gridSize;
                        }
                    }

                    f_x = f_x * pixelScale;
                    f_y = f_y * pixelScale;


                    p.FillRectLBWH(f_x, f_y, 2, 2);
                }
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
            //p.UseSubPixelRendering = true;
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
    }
}
