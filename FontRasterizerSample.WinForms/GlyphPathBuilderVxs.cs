//MIT, 2016,  WinterDev
using System;
using System.Collections.Generic;
using NOpenType;
using PixelFarm.Agg.VertexSource;

namespace PixelFarm.Agg
{
    //this is PixelFarm version ***
    //render with MiniAgg

    public class GlyphPathBuilderVxs : NOpenType.GlyphPathBuilderBase
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
        protected override void OnCurve3(float p2x, float p2y, float x, float y)
        {
            ps.Curve3(p2x, p2y, x, y);
        }
        protected override void OnCurve4(float p2x, float p2y, float p3x, float p3y, float x, float y)
        {
            ps.Curve4(p2x, p2y, p3x, p3y, x, y);
        }
        protected override void OnLineTo(float x, float y)
        {
            ps.LineTo(x, y);
        }
        protected override void OnMoveTo(float x, float y)
        {
            ps.MoveTo(x, y);
        }


        VertexStore _reuseVxs1 = new VertexStore();
        VertexStore _reuseVxs2 = new VertexStore();

        /// <summary>
        /// get processed/scaled vxs
        /// </summary>
        /// <returns></returns>
        public VertexStore GetVxs()
        {

            //1. calculate scale 
            float scale = UseTrueTypeInterpreter ? 1 :
                TypeFace.CalculateScale(SizeInPoints);
            var mat = PixelFarm.Agg.Transform.Affine.NewMatix(
                new PixelFarm.Agg.Transform.AffinePlan(
                    PixelFarm.Agg.Transform.AffineMatrixCommand.Scale, scale, scale));

            _reuseVxs1.Clear();
            _reuseVxs2.Clear();

            return curveFlattener.MakeVxs(mat.TransformToVxs(ps.Vxs, _reuseVxs1), _reuseVxs2);
        }
        public VertexStore GetUnscaledVxs()
        {
            return VertexStore.CreateCopy(ps.Vxs);
        }
        static CurveFlattener curveFlattener = new CurveFlattener();
    }


}