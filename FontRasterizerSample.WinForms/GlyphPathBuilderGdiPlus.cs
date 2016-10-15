//MIT, 2016,  WinterDev
using System;
using System.Collections.Generic;
//
using System.Drawing; //*** 
using NRasterizer;

namespace PixelFarm.Agg
{
    //this is Gdi+ version
    //render with System.Drawing.Drawing2d.GraphicsPath 

    public class GlyphPathBuilderGdiPlus : GlyphPathBuilderBase
    {
        System.Drawing.Drawing2D.GraphicsPath ps = new System.Drawing.Drawing2D.GraphicsPath();
        float lastMoveX;
        float lastMoveY;
        float lastX;
        float lastY;

        public GlyphPathBuilderGdiPlus(Typeface typeface)
            : base(typeface)
        {

        }
        protected override void OnBeginRead(int countourCount)
        {
            ps.Reset();
        }
        protected override void OnEndRead()
        {

        }
        protected override void OnCloseFigure()
        {
            ps.CloseFigure();
        }
        protected override void OnCurve3(double p2x, double p2y, double x, double y)
        {

            //from http://stackoverflow.com/questions/9485788/convert-quadratic-curve-to-cubic-curve
            //Control1X = StartX + (.66 * (ControlX - StartX))
            //Control2X = EndX + (.66 * (ControlX - EndX))


            float c1x = lastX + (float)((2f / 3f) * (p2x - lastX));
            float c1y = lastY + (float)((2f / 3f) * (p2y - lastY));
            //---------------------------------------------------------------------
            float c2x = (float)(x + ((2f / 3f) * (p2x - x)));
            float c2y = (float)(y + ((2f / 3f) * (p2y - y)));
            //---------------------------------------------------------------------
            ps.AddBezier(
                new PointF(lastX, lastY),
                new PointF(c1x, c1y),
                new PointF(c2x, c2y),
                new PointF(lastX = (float)x, lastY = (float)y));

        }
        protected override void OnCurve4(double p2x, double p2y, double p3x, double p3y, double x, double y)
        {
            // ps.Curve4(p2x, p2y, p3x, p3y, x, y);

            ps.AddBezier(
                new PointF(lastX, lastY),
                new PointF((float)p2x, (float)p2y),
                new PointF((float)p3x, (float)p3y),
                new PointF(lastX = (float)x, lastY = (float)y));

        }
        protected override void OnLineTo(double x, double y)
        {
            ps.AddLine(
               new PointF(lastX, lastY),
               new PointF(lastX = (float)x, lastY = (float)y));
        }
        protected override void OnMoveTo(double x, double y)
        {
            lastX = lastMoveX = (float)x;
            lastY = lastMoveY = (float)y;
        }
        public System.Drawing.Drawing2D.GraphicsPath GetGraphicsPath()
        {
            return ps;
        }
    }


}