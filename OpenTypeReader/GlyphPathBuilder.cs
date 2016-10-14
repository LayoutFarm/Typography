//Apache2, 2014-2016,   WinterDev
using System;
using System.Collections.Generic;
namespace NRasterizer
{
    /// <summary>
    /// gerneral glyph path builder
    /// </summary>
    public class GlyphPathBuilder : GlyphPathBuilderBase
    {
        IGlyphRasterizer _rasterizer;
        public GlyphPathBuilder(Typeface typeface, IGlyphRasterizer ras)
            : base(typeface)
        {
            this._rasterizer = ras;
        }
        protected override void OnBeginRead(int countourCount)
        {
            _rasterizer.BeginRead(countourCount);
        }
        protected override void OnCloseFigure()
        {
            _rasterizer.CloseFigure();

        }
        protected override void OnCurve3(double p2x, double p2y, double x, double y)
        {
            _rasterizer.Curve3(p2x, p2y, x, y);
        }
        protected override void OnCurve4(double p2x, double p2y, double p3x, double p3y, double x, double y)
        {
            _rasterizer.Curve4(p2x, p2y, p3x, p3y, x, y);
        }
        protected override void OnLineTo(double x, double y)
        {
            _rasterizer.LineTo(x, y);
        }
        protected override void OnMoveTo(double x, double y)
        {
            _rasterizer.MoveTo(x, y);
        }
        protected override void OnEndRead()
        {
            _rasterizer.EndRead();
        }
        public IGlyphRasterizer Rasterizer { get; set; }

    }

}