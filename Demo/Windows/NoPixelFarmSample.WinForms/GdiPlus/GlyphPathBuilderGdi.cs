//Apache2, 2014-2016, Samuel Carlsson, WinterDev


using System.Drawing;
using System.Drawing.Drawing2D;
using Typography.OpenFont;

namespace SampleWinForms
{
    //------------------
    //this is Gdi+ version ***
    //render with System.Drawing.Drawing2D.GraphicsPath
    //------------------

    public class GlyphPathBuilderGdi : IGlyphPathBuilder
    {
        //this gdi+ version
        GraphicsPath ps;
        float lastMoveX;
        float lastMoveY;
        float lastX;
        float lastY;

        public GlyphPathBuilderGdi()
        {

        }
        public void BeginRead(int countourCount)
        {
            ps = new GraphicsPath();
            ps.Reset();
        }
        public void EndRead()
        {

        }
        public void MoveTo(float x, float y)
        {
            lastX = lastMoveX = (float)x;
            lastY = lastMoveY = (float)y;
        }
        public void CloseFigure()
        {
            ps.CloseFigure();
        }
        public void Curve3(float p2x, float p2y, float x, float y)
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
        public void Curve4(float p2x, float p2y, float p3x, float p3y, float x, float y)
        {

            ps.AddBezier(
                new PointF(lastX, lastY),
                new PointF((float)p2x, (float)p2y),
                new PointF((float)p3x, (float)p3y),
                new PointF(lastX = (float)x, lastY = (float)y));
        }

        public void LineTo(float x, float y)
        {
            ps.AddLine(
                 new PointF(lastX, lastY),
                 new PointF(lastX = (float)x, lastY = (float)y));
        }



        public GraphicsPath ResultGraphicPath { get { return this.ps; } }

    }
}

