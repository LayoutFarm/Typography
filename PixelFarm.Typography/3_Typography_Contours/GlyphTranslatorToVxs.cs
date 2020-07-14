//MIT, 2016-present, WinterDev

using PixelFarm.CpuBlit;
using PixelFarm.CpuBlit.VertexProcessing;
using PixelFarm.Drawing;



namespace Typography.OpenFont.Contours
{

    //this is PixelFarm version ***

    /// <summary>
    /// read glyph and write the result to target vxs
    /// </summary>
    public class GlyphTranslatorToVxs : IGlyphTranslator
    {
        readonly PathWriter _pw = new PathWriter();
        readonly VertexStore _vxs = new VertexStore();
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
        public void BeginRead(int contourCount)
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

        public void WriteUnFlattenOutput(VertexStore output, float scale)
        {
            if (scale == 1)
            {
                output.AppendVertexStore(_vxs);
            }
            else
            {
                AffineMat.GetScaleMat(scale).TransformToVxs(_vxs, output);
            }
        }
        /// <summary>
        /// write output to vxs, use default curve flattener
        /// </summary>
        /// <param name="output"></param> 
        /// <param name="scale"></param>
        public void WriteOutput(VertexStore output, float scale = 1)
        {
            using (Tools.BorrowCurveFlattener(out var f))
            {
                WriteOutput(output, f, scale);
            }
        }
        /// <summary>
        /// write output to vxs, use user's curve flattener
        /// </summary>
        /// <param name="output"></param> 
        /// <param name="scale"></param>
        public void WriteOutput(VertexStore output, CurveFlattener curveFlattener, float scale = 1)
        {
            if (scale == 1)
            {
                curveFlattener.MakeVxs(_vxs, output);
            }
            else
            {

                AffineMat mat = AffineMat.GetScaleMat(scale);
                curveFlattener.MakeVxs(_vxs, mat, output);
            }
        }
    }
}