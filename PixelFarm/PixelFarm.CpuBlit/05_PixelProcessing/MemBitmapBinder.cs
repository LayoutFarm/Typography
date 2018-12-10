//MIT, 2014-present, WinterDev

using System;
namespace PixelFarm.Drawing
{

    public sealed class MemBitmapBinder : BitmapBufferProvider
    {
        PixelFarm.CpuBlit.MemBitmap _memBmp;
        bool _isMemBmpOwner;
        bool _releaseLocalBmpIfRequired;


        public MemBitmapBinder(PixelFarm.CpuBlit.MemBitmap memBmp, bool isMemBmpOwner)
        {
            _memBmp = memBmp;
            _isMemBmpOwner = isMemBmpOwner;
        }
        public override void ReleaseLocalBitmapIfRequired()
        {
            _releaseLocalBmpIfRequired = true;
        }
        public override void NotifyUsage()
        {
        }
        //
        public override int Width => _memBmp.Width;
        public override int Height => _memBmp.Height;
        //
        public override bool IsYFlipped => false;
        //
        public override IntPtr GetRawBufferHead()
        {
            if (_memBmp == null)
            {
                return IntPtr.Zero;
            }
            return PixelFarm.CpuBlit.MemBitmap.GetBufferPtr(_memBmp).Ptr;
        }
        public override void Dispose()
        {
            if (_memBmp != null)
            {
                if (_isMemBmpOwner)
                {
                    _memBmp.Dispose();
                }
                _memBmp = null;
            }
        }
        public override void ReleaseBufferHead()
        {

        }

    }
}