//MIT, 2014-present, WinterDev

namespace PixelFarm.Drawing
{
    public abstract class Region : System.IDisposable
    {
        public abstract void Dispose();
        public abstract object InnerRegion { get; }
    }
}