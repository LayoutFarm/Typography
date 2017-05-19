//MIT, 2017, WinterDev
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

//
using Typography.OpenFont;
//
using DrawingGL;
using DrawingGL.Text;
//


namespace Test_WinForm_TessGlyph
{
    public partial class FormTess : Form
    {
        Graphics g;
        float[] glyphPoints2;
        int[] contourEnds;

        TessTool tessTool = new TessTool();
        public FormTess()
        {
            InitializeComponent();
        }
        private void FormTess_Load(object sender, EventArgs e)
        {
            g = this.pnlGlyph.CreateGraphics();

            //string testFont = "d:\\WImageTest\\DroidSans.ttf";
            string testFont = "c:\\Windows\\Fonts\\Tahoma.ttf";
            using (FileStream fs = new FileStream(testFont, FileMode.Open, FileAccess.Read))
            {
                OpenFontReader reader = new OpenFontReader();
                Typeface typeface = reader.Read(fs);

                //--
                var builder = new Typography.Contours.GlyphPathBuilder(typeface);
                builder.BuildFromGlyphIndex(typeface.LookupIndex('a'), 256);

                var txToPath = new GlyphTranslatorToPath();
                var writablePath = new WritablePath();
                txToPath.SetOutput(writablePath);
                builder.ReadShapes(txToPath);
                //from contour to  
                var curveFlattener = new SimpleCurveFlattener();
                float[] flattenPoints = curveFlattener.Flatten(writablePath._points, out contourEnds);
                glyphPoints2 = flattenPoints;
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
        }

        float[] GetPolygonData(out int[] endContours)
        {
            endContours = this.contourEnds;
            return glyphPoints2;

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
        void DrawOutput()
        {
            //-----------
            //for GDI+ only
            bool drawInvert = chkInvert.Checked;
            int viewHeight = this.pnlGlyph.Height;
            if (drawInvert)
            {
                g.ScaleTransform(1, -1);
                g.TranslateTransform(0, -viewHeight);
            }
            //----------- 
            //show tess
            g.Clear(Color.White);
            int[] contourEndIndices;
            float[] polygon1 = GetPolygonData(out contourEndIndices);


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
                        g.DrawLine(pen1, p0, p1);
                        g.DrawString(a.ToString(), this.Font, Brushes.Black, p0);
                        m += 2;
                        a++;
                    }
                    //close coutour 

                    p0 = new PointF(polygon1[endAt - 1], polygon1[endAt]);
                    p1 = new PointF(polygon1[startAt - 3], polygon1[startAt - 2]);
                    g.DrawLine(pen1, p0, p1);
                    g.DrawString(a.ToString(), this.Font, Brushes.Black, p0);
                    //
                    startAt = (endAt + 1) + 3;
                }
            }
            int areaCount;
            float[] tessData = tessTool.TessPolygon(polygon1, contourEnds, out areaCount);
            //draw tess 
            int j = tessData.Length;
            for (int i = 0; i < j;)
            {
                var p0 = new PointF(tessData[i], tessData[i + 1]);
                var p1 = new PointF(tessData[i + 2], tessData[i + 3]);
                var p2 = new PointF(tessData[i + 4], tessData[i + 5]);

                g.DrawLine(Pens.Red, p0, p1);
                g.DrawLine(Pens.Red, p1, p2);
                g.DrawLine(Pens.Red, p2, p0);

                i += 6;
            }

            //-----------
            //for GDI+ only
            g.ResetTransform();
            //-----------
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
