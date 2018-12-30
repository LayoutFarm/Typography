//MIT, 2014-present, WinterDev

using System;
namespace PixelFarm.Drawing
{

    public abstract class Image : IDisposable
    {
        IDisposable _innerImg;
        bool _handleInnerImgAsOwner;

        public abstract void Dispose();
        public abstract int Width { get; }
        public abstract int Height { get; }
        public Size Size => new Size(this.Width, this.Height);
        public abstract bool IsReferenceImage { get; }
        public abstract int ReferenceX { get; }
        public abstract int ReferenceY { get; } 

        public static object GetCacheInnerImage(Image img)
        {
            return img._innerImg;
        }
        public static void ClearCache(Image img)
        {
            if (img != null)
            {
                if (img._innerImg != null && img._handleInnerImgAsOwner)
                {
                    img._innerImg.Dispose();
                }
                img._innerImg = null;
            }
        }
        public static void SetCacheInnerImage(Image img, IDisposable o, bool handleInnerImgAsOwner)
        {
            img._innerImg = o;
            img._handleInnerImgAsOwner = handleInnerImgAsOwner;
        }

    }

}