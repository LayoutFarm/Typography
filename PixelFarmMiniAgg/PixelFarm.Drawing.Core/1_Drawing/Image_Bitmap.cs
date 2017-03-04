//MIT, 2014-2017, WinterDev

using System;
namespace PixelFarm.Drawing
{
    public abstract class Image : System.IDisposable
    {
        public abstract void Dispose();
        public abstract int Width { get; }
        public abstract int Height { get; }


        public Size Size
        {
            get { return new Size(this.Width, this.Height); }
        }
        public abstract bool IsReferenceImage { get; }
        public abstract int ReferenceX { get; }
        public abstract int ReferenceY { get; }

        //--------
        System.IDisposable innerImage;
        public static System.IDisposable GetCacheInnerImage(Image img)
        {
            return img.innerImage;
        }
        public static void SetCacheInnerImage(Image img, System.IDisposable innerImage)
        {
            img.innerImage = innerImage;
        }
        //public abstract BitmapData LockBits();
        //public abstract void UnlockBits(BitmapData bmpdata);

        //public BitmapData LockBits()
        //{
        //    BitmapData bmpData = new BitmapData();
        //    return bmpData;
        //}
        //public void UnlockBits(BitmapData bmpdata)
        //{

        //}
    }

    //public abstract class BitmapData
    //{
    //    public abstract IntPtr Scan0
    //    {
    //        get;
    //    }
    //}
}