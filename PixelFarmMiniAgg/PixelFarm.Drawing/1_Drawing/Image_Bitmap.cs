//MIT, 2014-2016, WinterDev

using System;
namespace PixelFarm.Drawing
{
    public abstract class Image : System.IDisposable
    {
        public abstract void Dispose();
        public abstract int Width { get; }
        public abstract int Height { get; }
        public abstract System.IDisposable InnerImage { get; set; }
        public Size Size
        {
            get { return new Size(this.Width, this.Height); }
        }
        public abstract bool IsReferenceImage { get; }
        public abstract int ReferenceX { get; }
        public abstract int ReferenceY { get; }
    }

    public sealed class Bitmap : Image
    {
        int width;
        int height;
        System.IDisposable innerImage; 
        public Bitmap(int w, int h, System.IDisposable innerImage)
        {
            this.width = w;
            this.height = h;
            this.innerImage = innerImage;
        } 
        public override int Width
        {
            get { return this.width; }
        }
        public override int Height
        {
            get { return this.height; }
        }
        public override System.IDisposable InnerImage
        {
            get { return this.innerImage; }
            set { this.innerImage = value; }
        }
        public override void Dispose()
        {
        }
        public override bool IsReferenceImage
        {
            get { return false; }
        }
        public override int ReferenceX
        {
            get { return 0; }
        }
        public override int ReferenceY
        {
            get { return 0; }
        } 
    }

    public sealed class ReferenceBitmap : Image
    {
        int width;
        int height;
        int referenceX;
        int referenceY;
        Bitmap originalBmp;
        public ReferenceBitmap(Bitmap originalBmp, int x, int y, int w, int h)
        {
            this.originalBmp = originalBmp;
            this.referenceX = x;
            this.referenceY = y;
            this.width = w;
            this.height = h;
        }
        public override int Width
        {
            get { return this.width; }
        }
        public override System.IDisposable InnerImage
        {
            get
            {
                return this.originalBmp.InnerImage;
            }
            set
            {
                //can't set
            }
        }
        public override int Height
        {
            get { return this.height; }
        }
        public override void Dispose()
        {
            this.originalBmp = null;
        }
        public override int ReferenceX
        {
            get { return this.referenceX; }
        }
        public override int ReferenceY
        {
            get { return this.referenceY; }
        }
        public override bool IsReferenceImage
        {
            get { return true; }
        }
    }
}