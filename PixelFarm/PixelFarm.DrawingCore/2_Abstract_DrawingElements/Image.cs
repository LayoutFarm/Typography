//MIT, 2014-present, WinterDev

using System;
namespace PixelFarm.Drawing
{

    public abstract class Image : IDisposable
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

        public abstract void RequestInternalBuffer(ref ImgBufferRequestArgs buffRequest);

        //--------
        WeakReference innerImage;
        public static object GetCacheInnerImage(Image img)
        {
            if (img.innerImage != null && img.innerImage.IsAlive)
            {
                return img.innerImage.Target;
            }
            return null;
        }
        public static void ClearCache(Image img)
        {
            if (img != null)
            {
                img.innerImage = null;
            }
        }
        public static void SetCacheInnerImage(Image img, object o)
        {
            img.innerImage = new WeakReference(o);
        }




        //----------------------------
        public enum RequestType
        {
            Rent,
            Copy
        }
        public struct ImgBufferRequestArgs
        {
            public ImgBufferRequestArgs(int requestPixelFormat, RequestType reqType)
            {
                this.RequestType = reqType;
                this.RequestPixelFormat = requestPixelFormat; 
                this.IsInvertedImage = true;
                this.OutputBuffer32 = null;
            }
            public bool IsInvertedImage { get; set; }
            public int RequestPixelFormat { get; private set; }
            public RequestType RequestType { get; private set; } 
            public int[] OutputBuffer32 { get; set; }
        }
    }

}