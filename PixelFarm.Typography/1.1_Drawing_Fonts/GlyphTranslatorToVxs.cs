//MIT, 2016-present, WinterDev

using PixelFarm.CpuBlit;
using PixelFarm.CpuBlit.VertexProcessing;
using Typography.OpenFont;


namespace PixelFarm.Drawing.Fonts
{

    //this is PixelFarm version ***

    /// <summary>
    /// read glyph and write the result to target vxs
    /// </summary>
    public class GlyphTranslatorToVxs : IGlyphTranslator
    {
        CurveFlattener curveFlattener = new CurveFlattener();
        PathWriter ps = new PathWriter();
        public GlyphTranslatorToVxs()
        {
        }
#if DEBUG
        public PathWriter dbugGetPathWriter()
        {
            return ps;
        }
#endif
        public void BeginRead(int countourCount)
        {
            ps.Clear();
        }
        public void EndRead()
        {

        }
        public void CloseContour()
        {
            ps.CloseFigure();
        }
        public void Curve3(float x1, float y1, float x2, float y2)
        {
            ps.Curve3(x1, y1, x2, y2);
        }
        public void Curve4(float x1, float y1, float x2, float y2, float x3, float y3)
        {
            ps.Curve4(x1, y1, x2, y2, x3, y3);
        }
        public void LineTo(float x1, float y1)
        {
            ps.LineTo(x1, y1);
        }
        public void MoveTo(float x0, float y0)
        {

            ps.MoveTo(x0, y0);
        }

        public void Reset()
        {
            ps.Clear();
        }
        /// <summary>
        /// write output to vxs
        /// </summary>
        /// <param name="output"></param> 
        /// <param name="scale"></param>
        public void WriteOutput(VertexStore output, float scale = 1)
        {
            if (scale == 1)
            {
                curveFlattener.MakeVxs(ps.Vxs, output);
            }
            else
            {
                var mat = PixelFarm.CpuBlit.VertexProcessing.Affine.NewMatix(
                    new PixelFarm.CpuBlit.VertexProcessing.AffinePlan(
                        PixelFarm.CpuBlit.VertexProcessing.AffineMatrixCommand.Scale, scale, scale));
                //transform -> flatten ->output
                //TODO: review here again***
                VertexStore tmpVxs = new VertexStore();
                curveFlattener.MakeVxs(ps.Vxs, tmpVxs);
                mat.TransformToVxs(tmpVxs, output);
            }
        }
    }
}