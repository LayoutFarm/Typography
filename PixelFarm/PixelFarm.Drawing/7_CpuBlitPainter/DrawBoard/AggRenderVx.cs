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
        string _str;
        public AggRenderVxFormattedString(string str)
        {
            _str = str;

        }
        public override string OriginalString => _str;
    }
}