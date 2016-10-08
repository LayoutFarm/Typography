using System;
using System.Collections.Generic;
using System.Windows.Forms;
using NRasterizer;

using System.Drawing;
using System.Drawing.Drawing2D;

namespace Sample2
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
    public class Rasterizer2
    {
        private readonly Typeface _typeface;
        private const int pointsPerInch = 72;
        GraphicsPath gfxPath;
        public Rasterizer2(Typeface typeface)
        {
            _typeface = typeface;
        }

        void RenderGlyph(Glyph glyph, int resolution, int fx, int fy, int size, int x, int y)
        {
            float scale = (float)(size * resolution) / (pointsPerInch * _typeface.UnitsPerEm);

            for (int contour = 0; contour < glyph.ContourCount; contour++)
            {
                var aerg = new List<Segment>(glyph.GetContourIterator(contour, fx, fy, x, y, scale, -scale));
                foreach (Segment s in aerg)
                {
                    switch (s.Kind)
                    {
                        case SegmentKind.Line:
                            {
                                Line line = (Line)s;
                                gfxPath.AddLine(line.x0, line.y0, line.x1, line.y1);
                            }
                            break;
                        case SegmentKind.Bezier:
                            {
                                Bezier b = (Bezier)s;
                                gfxPath.AddCurve(
                                    new PointF[]
                                    {
                                        new PointF(b.x0,b.y0),
                                        new PointF(b.x1,b.y1),
                                        new PointF(b.x2,b.y2), 
                                    }
                                    );

                            }
                            break;
                    }

                }
            }
        }

        public void Rasterize(GraphicsPath gfxPath, string text, int size, int resolution, bool toFlags = false)
        {
            this.gfxPath = gfxPath;
            int fx = 64;
            int fy = 0;
            foreach (var character in text)
            {
                var glyph = _typeface.Lookup(character);
                RenderGlyph(glyph, resolution, fx, fy, size, 0, 70);
                fx += _typeface.GetAdvanceWidth(character);
            }

            //if (toFlags)
            //{
            //    RenderFlags(flags, raster);
            //}
            //else
            //{
            //    RenderScanlines(flags, raster);
            //}
        }

        // TODO: Duplicated code from Rasterize & SetScanFlags
        //public IEnumerable<Segment> GetAllSegments(string text, int size, int resolution)
        //{
        //    int x = 0;
        //    int y = 70;

        //    // 
        //    int fx = 64;
        //    int fy = 0;
        //    foreach (var character in text)
        //    {
        //        var glyph = _typeface.Lookup(character);

        //        float scale = (float)(size * resolution) / (pointsPerInch * _typeface.UnitsPerEm);
        //        for (int contour = 0; contour < glyph.ContourCount; contour++)
        //        {
        //            var aerg = new List<Segment>(glyph.GetContourIterator(contour, fx, fy, x, y, scale, -scale));
        //            foreach (var segment in glyph.GetContourIterator(contour, fx, fy, x, y, scale, -scale))
        //            {
        //                yield return segment;
        //            }
        //        }

        //        fx += _typeface.GetAdvanceWidth(character);
        //    }
        //}
    }
}
