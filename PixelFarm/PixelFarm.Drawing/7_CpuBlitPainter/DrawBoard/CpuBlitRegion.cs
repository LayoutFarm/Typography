//MIT, 2019-present, WinterDev

using PixelFarm.Drawing;

namespace PixelFarm.CpuBlit
{

    public abstract class CpuBlitRegion : Region
    {
        public enum CpuBlitRegionKind
        {
            VxsRegion,
            BitmapBasedRegion,
            MixedRegion,
        }
        object _innerObj;
        public override object InnerRegion => _innerObj;
        internal void SetInnerObject(object value) => _innerObj = value;

        public override void Dispose()
        {
        } 
        public abstract CpuBlitRegionKind Kind { get; }
    }



}