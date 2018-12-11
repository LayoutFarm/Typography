//MIT, 2017, Zou Wei(github/zwcloud), WinterDev 
//Apache2, 2014-2016, Samuel Carlsson, WinterDev

using System;
using Typography.OpenFont;
namespace DrawingGL.Text
{

    /// <summary>
    /// for translate raw glyph data to target path
    /// </summary>
    class GlyphTranslatorToPath : IGlyphTranslator
    {
        IWritablePath _ps;
        float _lastMoveX;
        float _lastMoveY;
        float _lastX;
        float _lastY;
        System.Text.StringBuilder _stbuilder = null;

        public GlyphTranslatorToPath()
        {
        }
        public void SetOutput(IWritablePath ps, System.Text.StringBuilder stbuilder = null)
        {
            _ps = ps;
            _stbuilder = stbuilder;
        }
        public void BeginRead(int contourCount)
        {

        }
        public void EndRead()
        {

        }
        public void MoveTo(float x0, float y0)
        {

            _lastX = _lastMoveX = (float)x0;
            _lastY = _lastMoveY = (float)y0;
            _ps.MoveTo(x0, y0);
            if (_stbuilder != null)
            {
                _stbuilder.AppendLine(string.Format("move_to ({0:0.00}, {1:0.00})", x0, y0));
            }
        }
        public void CloseContour()
        {
            _ps.CloseFigure();

            if (_stbuilder != null)
            {
                _stbuilder.AppendLine("close");
            }
        }
        public void Curve3(float x1, float y1, float x2, float y2)
        {
            //convert curve3 to curve4 
            //from http://stackoverflow.com/questions/9485788/convert-quadratic-curve-to-cubic-curve
            //Control1X = StartX + (.66 * (ControlX - StartX))
            //Control2X = EndX + (.66 * (ControlX - EndX)) 

            float c1x = _lastX + (float)((2f / 3f) * (x1 - _lastX));
            float c1y = _lastY + (float)((2f / 3f) * (y1 - _lastY));
            //---------------------------------------------------------------------
            float c2x = (float)(x2 + ((2f / 3f) * (x1 - x2)));
            float c2y = (float)(y2 + ((2f / 3f) * (y1 - y2)));
            //---------------------------------------------------------------------
            _ps.BezireTo(
                 c1x, c1y,
                 c2x, c2y,
                 _lastX = x2, _lastY = y2);
            //---------------------------------------------------------------------
            if (_stbuilder != null)
            {
                _stbuilder.AppendLine(
                    string.Format("quad_bezier_to c1({0:0.00}, {1:0.00}) end ({2:0.00}, {3:0.00})",
                    x1, y1, x2, y2));
            }
        }
        public void Curve4(float x1, float y1, float x2, float y2, float x3, float y3)
        {
            _ps.BezireTo(
              x1, y1,
              x2, y2,
              _lastX = x3, _lastY = y3);

            if (_stbuilder != null)
            {
                _stbuilder.AppendLine(
                    string.Format("cubic_bezier_to c0 ({0:0.00}, {1:0.00}) c1 ({2:0.00}, {3:0.00}) end ({4:0.00}, {5:0.00})",
                    x1, y1, x2, y2, x3, y3));
            }
        }
        public void LineTo(float x1, float y1)
        {
            _ps.LineTo(_lastX = x1, _lastY = y1);
        }
        public void Reset()
        {
            _ps = null;
            _lastMoveX = _lastMoveY = _lastX = _lastY;
        }
    }
}

