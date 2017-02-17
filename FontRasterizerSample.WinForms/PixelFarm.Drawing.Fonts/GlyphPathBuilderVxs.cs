//MIT, 2016-2017, WinterDev
using System;
using System.Collections.Generic;


using PixelFarm.Agg;
using PixelFarm.Agg.VertexSource;


using Typography.OpenType;
using Typography.Rendering;

namespace PixelFarm.Drawing.Fonts
{
    //this is PixelFarm version ***
    //render with MiniAgg 
    class GlyphPathBuilderVxs : GlyphPathBuilderBase
    {
        PathWriter ps = new PathWriter();
        List<GlyphContour> contours;
        GlyphContourBuilder cntBuilder;

        public GlyphPathBuilderVxs(Typeface typeface)
            : base(typeface)
        {

        }

        protected override void OnBeginRead(int countourCount)
        {
            ps.Clear();
            //-----------------------------------
            contours = new List<GlyphContour>();
            //start with blank contour
            cntBuilder = new GlyphContourBuilder();
        }
        protected override void OnEndRead()
        {

        }
        protected override void OnCloseFigure()
        {
            cntBuilder.CloseFigure();
            GlyphContour cntContour = cntBuilder.CurrentContour;
            cntContour.allPoints = cntBuilder.GetAllPoints();
            cntBuilder.Reset();
            contours.Add(cntContour);
            ps.CloseFigure();
        }
        protected override void OnCurve3(float p2x, float p2y, float x, float y)
        {
            cntBuilder.Curve3(p2x, p2y, x, y);
            ps.Curve3(p2x, p2y, x, y);
        }
        protected override void OnCurve4(float p2x, float p2y, float p3x, float p3y, float x, float y)
        {
            cntBuilder.Curve4(p2x, p2y, p3x, p3y, x, y);
            ps.Curve4(p2x, p2y, p3x, p3y, x, y);

        }
        protected override void OnLineTo(float x, float y)
        {
            cntBuilder.LineTo(x, y);
            ps.LineTo(x, y);
        }
        protected override void OnMoveTo(float x, float y)
        {
            cntBuilder.MoveTo(x, y);
            ps.MoveTo(x, y);
        }

        /// <summary>
        /// get processed/scaled vxs
        /// </summary>
        /// <returns></returns>
        public VertexStore GetVxs()
        {
            VertexStore vxs1 = new VertexStore();
            if (PassHintInterpreterModule)
            {
                return curveFlattener.MakeVxs(ps.Vxs, vxs1);
            }
            else
            {
                VertexStore vxs2 = new VertexStore();
                float scale = TypeFace.CalculateScale(SizeInPoints);
                var mat = PixelFarm.Agg.Transform.Affine.NewMatix(
                    new PixelFarm.Agg.Transform.AffinePlan(
                        PixelFarm.Agg.Transform.AffineMatrixCommand.Scale, scale, scale));
                //transform -> flatten ->output
                return curveFlattener.MakeVxs(mat.TransformToVxs(ps.Vxs, vxs1), vxs2);
            }
        }
        public void GetVxs(VertexStore output, VertexStorePool vxsPool)
        {

            if (PassHintInterpreterModule)
            {
                curveFlattener.MakeVxs(ps.Vxs, output);
            }
            else
            {
                float scale = TypeFace.CalculateScale(SizeInPoints);
                var mat = PixelFarm.Agg.Transform.Affine.NewMatix(
                    new PixelFarm.Agg.Transform.AffinePlan(
                        PixelFarm.Agg.Transform.AffineMatrixCommand.Scale, scale, scale));
                //transform -> flatten ->output
                VertexStore tmpVxs = vxsPool.GetFreeVxs();
                curveFlattener.MakeVxs(mat.TransformToVxs(ps.Vxs, tmpVxs), output);
                vxsPool.Release(ref tmpVxs);
            }
        }
        public float GetPixelScale()
        {
            return TypeFace.CalculateScale(SizeInPoints);
        }
        public VertexStore GetUnscaledVxs()
        {
            return VertexStore.CreateCopy(ps.Vxs);
        }

        public List<GlyphContour> GetContours()
        {
            return contours;
        }
        CurveFlattener curveFlattener = new CurveFlattener();
    }


}