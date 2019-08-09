//MIT, 2017-present, WinterDev
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

//
using Typography.OpenFont;
//
using DrawingGL;
using DrawingGL.Text;
//
using Tesselate;

namespace Test_WinForm_TessGlyph
{
    public partial class FormTess : Form
    {
        Graphics _g;
        float[] _glyphPoints2;
        int[] _contourEnds;

        TessTool _tessTool = new TessTool();
        public FormTess()
        {
            InitializeComponent();

            rdoSimpleIncCurveFlattener.Checked = true;
            rdoSimpleIncCurveFlattener.CheckedChanged += (s, e) => UpdateOutput();
            //
            rdoSubdivCureveFlattener.CheckedChanged += (s, e) => UpdateOutput();

            textBox1.KeyUp += (s, e) => UpdateOutput();


            rdoTessPoly2Tri.CheckedChanged += (s, e) => UpdateOutput();
            rdoTessSGI.CheckedChanged += (s, e) => UpdateOutput();


            txtIncrementalTessStep.KeyUp += (s, e) => UpdateOutput();
            txtDivCurveRecursiveLimit.KeyUp += (s, e) => UpdateOutput();
            txtDivAngleTolerenceEpsilon.KeyUp += (s, e) => UpdateOutput();
        }

        void UpdateOutput()
        {
            string oneChar = this.textBox1.Text.Trim();
            if (string.IsNullOrEmpty(oneChar)) return;
            //
            char selectedChar = oneChar[0];
            //
            //
            //selectedChar = 'e'; 
            if (_g == null)
            {
                _g = this.panel1.CreateGraphics();
            }
            _g.Clear(Color.White);

            //-------
            //string testFont = "c:\\Windows\\Fonts\\Tahoma.ttf";
            string testFont = @"D:\projects\Typography\Demo\Windows\TestFonts\SourceSerifPro-Regular.otf";
            using (FileStream fs = new FileStream(testFont, FileMode.Open, FileAccess.Read))
            {
                OpenFontReader reader = new OpenFontReader();
                Typeface typeface = reader.Read(fs);

                //--
                var builder = new Typography.Contours.GlyphPathBuilder(typeface);
                builder.BuildFromGlyphIndex(typeface.LookupIndex(selectedChar), 300);

                var txToPath = new GlyphTranslatorToPath();
                var writablePath = new WritablePath();
                txToPath.SetOutput(writablePath);
                builder.ReadShapes(txToPath);

                //------
                //
                //**flatten contour before send to Tess***
                var curveFlattener = new SimpleCurveFlattener();

                if (rdoSimpleIncCurveFlattener.Checked)
                {
                    if (int.TryParse(txtIncrementalTessStep.Text, out int incSteps))
                    {
                        curveFlattener.IncrementalStep = incSteps;
                    }
                    curveFlattener.FlattenMethod = CurveFlattenMethod.Inc;
                }
                else
                {
                    if (double.TryParse(txtDivAngleTolerenceEpsilon.Text, out double angleTolerenceEpsilon))
                    {
                        //convert degree to rad
                        curveFlattener.DivCurveAngleTolerenceEpsilon = DegToRad(angleTolerenceEpsilon);
                    }
                    if (int.TryParse(txtDivCurveRecursiveLimit.Text, out int recuvesiveLim))
                    {
                        curveFlattener.DivCurveRecursiveLimit = recuvesiveLim;
                    }
                    curveFlattener.FlattenMethod = CurveFlattenMethod.Div;
                }
                _glyphPoints2 = curveFlattener.Flatten(writablePath._points, out _contourEnds);

                ////--------------------------------------
                ////raw glyph points
                //int j = glyphPoints.Length;
                //float scale = typeface.CalculateToPixelScaleFromPointSize(256);
                //glyphPoints2 = new float[j * 2];
                //int n = 0;
                //for (int i = 0; i < j; ++i)
                //{
                //    GlyphPointF pp = glyphPoints[i];
                //    glyphPoints2[n] = pp.X * scale;
                //    n++;
                //    glyphPoints2[n] = pp.Y * scale;
                //    n++;
                //}
                ////--------------------------------------
            }
            DrawOutput();
        }

        static double DegToRad(double degree)
        {
            return degree * (Math.PI / 180d);
        }
        static double RadToDeg(double degree)
        {
            return degree * (180d / Math.PI);
        }

        private void FormTess_Load(object sender, EventArgs e)
        {
            if (_g == null)
            {
                _g = this.panel1.CreateGraphics();
            }
            _g.Clear(Color.White);

        }

        float[] GetPolygonData(out int[] endContours)
        {
            endContours = _contourEnds;
            return _glyphPoints2;

            ////--
            ////for test

            //return new float[]
            //{
            //        10,10,
            //        200,10,
            //        100,100,
            //        150,200,
            //        20,200,
            //        50,100
            //};
        }


        float[] TransformPoints(float[] polygonXYs, System.Drawing.Drawing2D.Matrix transformMat)
        {
            //for example only

            PointF[] points = new PointF[1];
            float[] transformXYs = new float[polygonXYs.Length];

            for (int i = 0; i < polygonXYs.Length;)
            {
                points[0] = new PointF(polygonXYs[i], polygonXYs[i + 1]);
                transformMat.TransformPoints(points);

                transformXYs[i] = points[0].X;
                transformXYs[i + 1] = points[0].Y;
                i += 2;
            }
            return transformXYs;
        }





        void DrawOutput()
        {
            if (_g == null)
            {
                return;
            }

            //-----------
            //for GDI+ only
            bool flipYAxis = chkFlipY.Checked;
            int viewHeight = this.panel1.Height;

            //----------- 
            //show tess
            _g.Clear(Color.White);
            int[] contourEndIndices;
            float[] polygon1 = GetPolygonData(out contourEndIndices);
            if (polygon1 == null) return;
            //
            if (flipYAxis)
            {
                var transformMat = new System.Drawing.Drawing2D.Matrix();
                transformMat.Scale(1, -1);
                transformMat.Translate(0, -viewHeight);
                polygon1 = TransformPoints(polygon1, transformMat);

                //when we flipY, meaning of clockwise-counter clockwise is changed.
                //    
                //see https://stackoverflow.com/questions/1165647/how-to-determine-if-a-list-of-polygon-points-are-in-clockwise-order
                //...(comment)                     
                //...A minor caveat: this answer assumes a normal Cartesian coordinate system.
                //The reason that's worth mentioning is that some common contexts, like HTML5 canvas, use an inverted Y-axis.
                //Then the rule has to be flipped: if the area is negative, the curve is clockwise. – LarsH Oct 11 '13 at 20:49
            }


            using (Pen pen1 = new Pen(Color.LightGray, 6))
            {
                int nn = polygon1.Length;
                int a = 0;
                PointF p0;
                PointF p1;

                int contourCount = contourEndIndices.Length;
                int startAt = 3;
                for (int cnt_index = 0; cnt_index < contourCount; ++cnt_index)
                {
                    int endAt = contourEndIndices[cnt_index];
                    for (int m = startAt; m <= endAt;)
                    {
                        p0 = new PointF(polygon1[m - 3], polygon1[m - 2]);
                        p1 = new PointF(polygon1[m - 1], polygon1[m]);
                        _g.DrawLine(pen1, p0, p1);
                        _g.DrawString(a.ToString(), this.Font, Brushes.Black, p0);
                        m += 2;
                        a++;
                    }
                    //close contour 

                    p0 = new PointF(polygon1[endAt - 1], polygon1[endAt]);
                    p1 = new PointF(polygon1[startAt - 3], polygon1[startAt - 2]);
                    _g.DrawLine(pen1, p0, p1);
                    _g.DrawString(a.ToString(), this.Font, Brushes.Black, p0);
                    //
                    startAt = (endAt + 1) + 3;
                }
            }
            //----------------------------------------------------------------------------

            //tess
            if (rdoTessSGI.Checked)
            {

                //SGI Tess Lib

                if (!_tessTool.TessPolygon(polygon1, _contourEnds))
                {
                    return;
                }

                //1.
                List<ushort> indexList = _tessTool.TessIndexList;
                //2.
                List<TessVertex2d> tempVertexList = _tessTool.TempVertexList;
                //3.
                int vertexCount = indexList.Count;
                //-----------------------------    
                int orgVertexCount = polygon1.Length / 2;
                float[] vtx = new float[vertexCount * 2];//***
                int n = 0;

                for (int p = 0; p < vertexCount; ++p)
                {
                    ushort index = indexList[p];
                    if (index >= orgVertexCount)
                    {
                        //extra coord (newly created)
                        TessVertex2d extraVertex = tempVertexList[index - orgVertexCount];
                        vtx[n] = (float)extraVertex.x;
                        vtx[n + 1] = (float)extraVertex.y;
                    }
                    else
                    {
                        //original corrd
                        vtx[n] = (float)polygon1[index * 2];
                        vtx[n + 1] = (float)polygon1[(index * 2) + 1];
                    }
                    n += 2;
                }
                //-----------------------------    
                //draw tess result
                int j = vtx.Length;
                for (int i = 0; i < j;)
                {
                    var p0 = new PointF(vtx[i], vtx[i + 1]);
                    var p1 = new PointF(vtx[i + 2], vtx[i + 3]);
                    var p2 = new PointF(vtx[i + 4], vtx[i + 5]);

                    _g.DrawLine(Pens.Red, p0, p1);
                    _g.DrawLine(Pens.Red, p1, p2);
                    _g.DrawLine(Pens.Red, p2, p0);

                    i += 6;
                }
            }
            else
            {

                List<Poly2Tri.Polygon> outputPolygons = new List<Poly2Tri.Polygon>();
                Poly2TriExampleHelper.Triangulate(polygon1, contourEndIndices, flipYAxis, outputPolygons);
                foreach (Poly2Tri.Polygon polygon in outputPolygons)
                {
                    foreach (Poly2Tri.DelaunayTriangle tri in polygon.Triangles)
                    {
                        Poly2Tri.TriangulationPoint p0 = tri.P0;
                        Poly2Tri.TriangulationPoint p1 = tri.P1;
                        Poly2Tri.TriangulationPoint p2 = tri.P2;

                        _g.DrawLine(Pens.Red, (float)p0.X, (float)p0.Y, (float)p1.X, (float)p1.Y);
                        _g.DrawLine(Pens.Red, (float)p1.X, (float)p1.Y, (float)p2.X, (float)p2.Y);
                        _g.DrawLine(Pens.Red, (float)p2.X, (float)p2.Y, (float)p0.X, (float)p0.Y);
                    }
                }
            }
        }
        private void cmdDrawGlyph_Click(object sender, EventArgs e)
        {
            DrawOutput();
        }

        private void chkInvert_CheckedChanged(object sender, EventArgs e)
        {
            DrawOutput();
        }

    }
}
