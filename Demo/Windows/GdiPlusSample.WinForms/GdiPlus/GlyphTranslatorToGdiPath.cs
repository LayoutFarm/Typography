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
    /// <summary>
    /// read result as Gdi+ GraphicsPath
    /// </summary>
    public class GlyphTranslatorToGdiPath : IGlyphTranslator
    {
        //this gdi+ version
        GraphicsPath ps;
        float lastMoveX;
        float lastMoveY;
        float lastX;
        float lastY;

        public GlyphTranslatorToGdiPath()
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
        public void MoveTo(float x0, float y0)
        {
            lastX = lastMoveX = (float)x0;
            lastY = lastMoveY = (float)y0;
        }
        public void CloseContour()
        {
            ps.CloseFigure();
        }
        public void Curve3(float x1, float y1, float x2, float y2)
        {
            //from http://stackoverflow.com/questions/9485788/convert-quadratic-curve-to-cubic-curve
            //Control1X = StartX + (.66 * (ControlX - StartX))
            //Control2X = EndX + (.66 * (ControlX - EndX)) 

            float c1x = lastX + (float)((2f / 3f) * (x1 - lastX));
            float c1y = lastY + (float)((2f / 3f) * (y1 - lastY));
            //---------------------------------------------------------------------
            float c2x = (float)(x2 + ((2f / 3f) * (x1 - x2)));
            float c2y = (float)(y2 + ((2f / 3f) * (y1 - y2)));
            //---------------------------------------------------------------------
            ps.AddBezier(
                new PointF(lastX, lastY),
                new PointF(c1x, c1y),
                new PointF(c2x, c2y),
                new PointF(lastX = (float)x2, lastY = (float)y2));

        }
        public void Curve4(float x1, float y1, float x2, float y2, float x3, float y3)
        {

            ps.AddBezier(
                new PointF(lastX, lastY),
                new PointF((float)x1, (float)y1),
                new PointF((float)x2, (float)y2),
                new PointF(lastX = (float)x3, lastY = (float)y3));
        }

        public void LineTo(float x1, float y1)
        {
            ps.AddLine(
                 new PointF(lastX, lastY),
                 new PointF(lastX = (float)x1, lastY = (float)y1));
        }

        public void Reset()
        {
            ps = null;
            lastMoveX = lastMoveY = lastX = lastY;
        }
        public GraphicsPath ResultGraphicsPath { get { return this.ps; } }

    }
}

