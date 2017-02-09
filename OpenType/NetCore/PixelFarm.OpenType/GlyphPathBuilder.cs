 //Apache2, 2014-2017, WinterDev
 
namespace NOpenType
{
    /// <summary>
    /// gerneral glyph path builder
    /// </summary>
    public class GlyphPathBuilder : GlyphPathBuilderBase
    {
        IGlyphRasterizer _rasterizer;
        float scale;
        public GlyphPathBuilder(Typeface typeface, IGlyphRasterizer ras)
            : base(typeface)
        {
            this._rasterizer = ras;
        }
        protected override void OnBeginRead(int countourCount)
        {
            if (this.PassHintInterpreterModule)
            {
                scale = 1;
            }
            else
            {
                scale = TypeFace.CalculateScale(SizeInPoints);
            }
            _rasterizer.BeginRead(countourCount);
        }
        protected override void OnCloseFigure()
        {
            _rasterizer.CloseFigure();
        }
        protected override void OnCurve3(float p2x, float p2y, float x, float y)
        {
            _rasterizer.Curve3(p2x * scale, p2y * scale, x * scale, y * scale);
        }
        protected override void OnCurve4(float p2x, float p2y, float p3x, float p3y, float x, float y)
        {
            _rasterizer.Curve4(p2x * scale, p2y * scale, p3x * scale, p3y * scale, x * scale, y * scale);
        }
        protected override void OnLineTo(float x, float y)
        {
            _rasterizer.LineTo(x * scale, y * scale);
        }
        protected override void OnMoveTo(float x, float y)
        {
            _rasterizer.MoveTo(x * scale, y * scale);
        }
        protected override void OnEndRead()
        {
            _rasterizer.EndRead();
        }

    }

}