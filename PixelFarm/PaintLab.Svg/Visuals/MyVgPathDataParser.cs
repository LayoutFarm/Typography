//MIT, 2014-present, WinterDev

using System;
using PixelFarm.CpuBlit;

namespace PaintLab.Svg
{
    class MyVgPathDataParser : VgPathDataParser
    {
        PathWriter _writer;
        public void SetPathWriter(PathWriter writer)
        {
            _writer = writer;
            _writer.StartFigure();
        }
        protected override void OnArc(float r1, float r2, float xAxisRotation, int largeArcFlag, int sweepFlags, float x, float y, bool isRelative)
        {
            _writer.SvgArcToCurve4(r1, r2, xAxisRotation, largeArcFlag, sweepFlags, x, y, isRelative);
        }
        protected override void OnCloseFigure()
        {
            _writer.CloseFigure();
        }
        protected override void OnCurveToCubic(
            float x1, float y1,
            float x2, float y2,
            float x, float y, bool isRelative)
        {
            if (isRelative)
            {
                _writer.Curve4Rel(x1, y1, x2, y2, x, y);
            }
            else
            {
                _writer.Curve4(x1, y1, x2, y2, x, y);
            }
        }
        protected override void OnCurveToCubicSmooth(float x2, float y2, float x, float y, bool isRelative)
        {
            if (isRelative)
            {
                _writer.SmoothCurve4Rel(x2, y2, x, y);
            }
            else
            {
                _writer.SmoothCurve4(x2, y2, x, y);
            }

        }
        protected override void OnCurveToQuadratic(float x1, float y1, float x, float y, bool isRelative)
        {
            if (isRelative)
            {
                _writer.Curve3Rel(x1, y1, x, y);
            }
            else
            {
                _writer.Curve3(x1, y1, x, y);
            }
        }
        protected override void OnCurveToQuadraticSmooth(float x, float y, bool isRelative)
        {
            if (isRelative)
            {
                _writer.SmoothCurve3Rel(x, y);
            }
            else
            {
                _writer.SmoothCurve3(x, y);
            }
        }
        protected override void OnHLineTo(float x, bool isRelative)
        {
            if (isRelative)
            {
                _writer.HorizontalLineToRel(x);
            }
            else
            {
                _writer.HorizontalLineTo(x);
            }
        }

        protected override void OnLineTo(float x, float y, bool isRelative)
        {
            if (isRelative)
            {
                _writer.LineToRel(x, y);
            }
            else
            {
                _writer.LineTo(x, y);
            }
        }
        protected override void OnMoveTo(float x, float y, bool isRelative)
        {
            if (isRelative)
            {
                _writer.MoveToRel(x, y);
            }
            else
            {
                _writer.MoveTo(x, y);
            }
        }
        protected override void OnVLineTo(float y, bool isRelative)
        {
            if (isRelative)
            {
                _writer.VerticalLineToRel(y);
            }
            else
            {
                _writer.VerticalLineTo(y);
            }
        }
    }

}