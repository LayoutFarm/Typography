//MIT, 2016-present, WinterDev

using PixelFarm.Drawing;
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
    class AggRenderVxFormattedString : RenderVxFormattedString
    {
        public AggRenderVxFormattedString()
        {
        }
#if DEBUG
        public string OriginalString { get; set; }
        public override string dbugName => "Agg";
#endif
    }
}