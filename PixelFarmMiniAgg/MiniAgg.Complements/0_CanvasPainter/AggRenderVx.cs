//MIT, 2016, WinterDev

namespace PixelFarm.Agg
{
    class AggRenderVx : PixelFarm.Drawing.RenderVx
    {
        internal VertexStoreSnap snap;
        public AggRenderVx(VertexStoreSnap snap)
        {
            this.snap = snap;
        }
    }
}