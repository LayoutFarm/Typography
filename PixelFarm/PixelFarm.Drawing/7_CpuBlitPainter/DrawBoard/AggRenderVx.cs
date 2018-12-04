//MIT, 2016-present, WinterDev

using PixelFarm.Drawing;
namespace PixelFarm.CpuBlit
{
    class AggRenderVx : PixelFarm.Drawing.RenderVx
    {
        internal VertexStore vxs;
        public AggRenderVx(VertexStore vxs)
        {
            this.vxs = vxs;
        }
    }
    class AggRenderVxFormattedString : PixelFarm.Drawing.RenderVxFormattedString
    {
        string str;
        public AggRenderVxFormattedString(string str)
        {
            this.str = str;

        }
        public override string OriginalString
        {
            get { return this.str; }
        }
    }
}