//MIT, 2016-2017, WinterDev
 
using PixelFarm.Agg;
using PixelFarm.Agg.VertexSource;
using Typography.OpenFont;


namespace PixelFarm.Drawing.Fonts
{
     
    //this is PixelFarm version ***
    //render with MiniAgg 
    class GlyphPathBuilderVxs : IGlyphPathBuilder
    {
        CurveFlattener curveFlattener = new CurveFlattener();
        PathWriter ps = new PathWriter();
        public GlyphPathBuilderVxs()
        {
        }
        public void BeginRead(int countourCount)
        {
            ps.Clear();
        }
        public void EndRead()
        {

        }
        public void CloseFigure()
        {

            ps.CloseFigure();
        }
        public void Curve3(float p2x, float p2y, float x, float y)
        {

            ps.Curve3(p2x, p2y, x, y);
        }
        public void Curve4(float p2x, float p2y, float p3x, float p3y, float x, float y)
        {
            ps.Curve4(p2x, p2y, p3x, p3y, x, y);
        }
        public void LineTo(float x, float y)
        {

            ps.LineTo(x, y);
        }
        public void MoveTo(float x, float y)
        {

            ps.MoveTo(x, y);
        }

        /// <summary>
        /// get processed/scaled vxs
        /// </summary>
        /// <returns></returns>
        public VertexStore GetVxs(float scale = 1)
        {
            //TODO: review here again
            VertexStore vxs1 = new VertexStore();
            if (scale == 1)
            {
                return curveFlattener.MakeVxs(ps.Vxs, vxs1);
            }
            else
            {

                VertexStore vxs2 = new VertexStore();
                //float scale = TypeFace.CalculateFromPointToPixelScale(SizeInPoints);
                var mat = PixelFarm.Agg.Transform.Affine.NewMatix(
                    new PixelFarm.Agg.Transform.AffinePlan(
                        PixelFarm.Agg.Transform.AffineMatrixCommand.Scale, scale, scale));
                //transform -> flatten ->output
                return curveFlattener.MakeVxs(mat.TransformToVxs(ps.Vxs, vxs1), vxs2);
            }
            //
            //if (PassHintInterpreterModule)
            //{
            //    return curveFlattener.MakeVxs(ps.Vxs, vxs1);
            //}
            //else
            //{
            // 
            //}
        }
        public void GetVxs(VertexStore output, VertexStorePool vxsPool, float scale = 1)
        {

            if (scale == 1)
            {
                curveFlattener.MakeVxs(ps.Vxs, output);
            }
            else
            {
                //float scale = TypeFace.CalculateFromPointToPixelScale(SizeInPoints);
                var mat = PixelFarm.Agg.Transform.Affine.NewMatix(
                    new PixelFarm.Agg.Transform.AffinePlan(
                        PixelFarm.Agg.Transform.AffineMatrixCommand.Scale, scale, scale));
                //transform -> flatten ->output
                VertexStore tmpVxs = vxsPool.GetFreeVxs();
                curveFlattener.MakeVxs(mat.TransformToVxs(ps.Vxs, tmpVxs), output);
                vxsPool.Release(ref tmpVxs);
            }
        }
        public VertexStore GetUnscaledVxs()
        {
            return VertexStore.CreateCopy(ps.Vxs);
        }
    }


}