//MIT, 2014-2016, WinterDev

namespace PixelFarm.Drawing
{
    public abstract class Region : System.IDisposable
    {
        public abstract void Dispose();
        public abstract object InnerRegion { get; }
    }
}