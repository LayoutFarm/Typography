//MIT, 2016-present, WinterDev

using PixelFarm.Drawing;
using PixelFarm.Drawing.Internal;

namespace PixelFarm.CpuBlit
{
    class AggRenderVx : RenderVx
    {
        internal VertexStore _vxs;
        public AggRenderVx(VertexStore vxs)
        {
            _vxs = vxs;
        }
    }


    public class AggRenderVxFormattedString : RenderVxFormattedString
    {
        internal AggRenderVxFormattedString()
        {
        }
        public object Seq { get; set; }
        public override int StripCount => throw new System.NotImplementedException();
        public string DelayString { get; internal set; }
        public bool IsDelay { get; internal set; }
#if DEBUG
        public string OriginalString { get; set; }
        public override string dbugName => "Agg";
#endif
    }
}