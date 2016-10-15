//MIT, 2016,  WinterDev
using System;
using System.Collections.Generic;
using NRasterizer; 

namespace PixelFarm.Agg
{
    //this is PixelFarm version ***
    //render with MiniAgg

    public class GlyphPathBuilderVxs : NRasterizer.GlyphPathBuilderBase
    {
        PixelFarm.Agg.VertexSource.PathWriter ps = new PixelFarm.Agg.VertexSource.PathWriter();
        public GlyphPathBuilderVxs(Typeface typeface)
            : base(typeface)
        {

        }
        protected override void OnBeginRead(int countourCount)
        {
            ps.Clear();
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
            ps.Curve3(p2x, p2y, x, y);
        }
        protected override void OnCurve4(double p2x, double p2y, double p3x, double p3y, double x, double y)
        {
            ps.Curve4(p2x, p2y, p3x, p3y, x, y);
        }
        protected override void OnLineTo(double x, double y)
        {
            ps.LineTo(x, y);
        }
        protected override void OnMoveTo(double x, double y)
        {
            ps.MoveTo(x, y);
        }
        public VertexStore GetVxs()
        {
            return ps.Vxs;
        }
    }


}