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
        CurveFlattener _curveFlattener = new CurveFlattener();
        PathWriter _pw = new PathWriter();
        VertexStore _vxs = new VertexStore();
        public GlyphTranslatorToVxs()
        {
            _pw.BindVxs(_vxs);
        }

#if DEBUG
        public VertexStore dbugVxs => _vxs;
        public PathWriter dbugGetPathWriter()
        {
            return _pw;
        }
#endif
        public void BeginRead(int countourCount)
        {
            _pw.Clear();
        }
        public void EndRead()
        {

        }
        public void CloseContour()
        {
            _pw.CloseFigure();
        }
        public void Curve3(float x1, float y1, float x2, float y2)
        {
            _pw.Curve3(x1, y1, x2, y2);
        }
        public void Curve4(float x1, float y1, float x2, float y2, float x3, float y3)
        {
            _pw.Curve4(x1, y1, x2, y2, x3, y3);
        }
        public void LineTo(float x1, float y1)
        {
            _pw.LineTo(x1, y1);
        }
        public void MoveTo(float x0, float y0)
        {

            _pw.MoveTo(x0, y0);
        }

        public void Reset()
        {
            _pw.Clear();
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
                _curveFlattener.MakeVxs(_vxs, output);
            }
            else
            {
                var mat = PixelFarm.CpuBlit.VertexProcessing.Affine.NewMatix(
                    new PixelFarm.CpuBlit.VertexProcessing.AffinePlan(
                        PixelFarm.CpuBlit.VertexProcessing.AffineMatrixCommand.Scale, scale, scale));
                //transform -> flatten ->output
                //TODO: review here again***
                using (VxsTemp.Borrow(out var v1))
                using (VectorToolBox.Borrow(out CurveFlattener f))
                {
                    _curveFlattener.MakeVxs(_vxs, mat, output);
                }
            }
        }
    }
}