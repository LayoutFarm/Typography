//MIT, 2016-present, WinterDev

using PixelFarm.Drawing;
namespace PixelFarm.CpuBlit
{
    class AggRenderVx : PixelFarm.Drawing.RenderVx
    {
        internal VertexStore _vxs;
        public AggRenderVx(VertexStore vxs)
        {
            _vxs = vxs;
        }
    }
    class AggRenderVxFormattedString : PixelFarm.Drawing.RenderVxFormattedString
    {
        public AggRenderVxFormattedString()
        {
        }
#if DEBUG
        public string OriginalString { get; set; }
#endif
    }
}