//BSD, 2014-present, WinterDev
//MIT, 2018-present, WinterDev
using System;

namespace PixelFarm.Drawing
{
    public delegate void LoadImageFunc(ImageBinder binder);

    public class ImageBinder : BitmapBufferProvider
    {

        /// <summary>
        /// local img cached
        /// </summary>
        PixelFarm.Drawing.Image _localImg;
        bool _isLocalImgOwner;
        LoadImageFunc _lazyLoadImgFunc;
        int _previewImgWidth = 16; //default ?
        int _previewImgHeight = 16;
        bool _isAtlasImg;
#if DEBUG
        static int dbugTotalId;
        public int dbugId = dbugTotalId++;
#endif

        protected ImageBinder() { }

        public ImageBinder(string imgSource, bool isMemBmpOwner = false)
        {
            ImageSource = imgSource;
            _isLocalImgOwner = isMemBmpOwner; //if true=> this binder will release a local cahed img
        }

        public ImageBinder(PixelFarm.CpuBlit.MemBitmap memBmp, bool isMemBmpOwner = false)
        {
#if DEBUG
            if (memBmp == null)
            {
                throw new NotSupportedException();
            }
#endif
            //binder to image
            _localImg = memBmp;
            _isLocalImgOwner = isMemBmpOwner; //if true=> this binder will release a local cahed img
            this.State = BinderState.Loaded;
        }
        public ImageBinder(PixelFarm.Drawing.Image otherImg, bool isMemBmpOwner = false)
        {
#if DEBUG
            if (otherImg == null)
            {
                throw new NotSupportedException();
            }
#endif
            //binder to image
            _localImg = otherImg;
            _isLocalImgOwner = isMemBmpOwner; //if true=> this binder will release a local cahed img
            this.State = BinderState.Loaded;
        }

        public event System.EventHandler ImageChanged;

        public virtual void RaiseImageChanged()
        {
            try
            {
                ImageChanged?.Invoke(this, System.EventArgs.Empty);
            }
            catch (Exception ex)
            {

            }
        }

#if DEBUG
        public override void dbugNotifyUsage()
        {
        }
#endif
        public override void ReleaseLocalBitmapIfRequired()
        {

        }
        /// <summary>
        /// preview img size is an expected(assume) img of original img, 
        /// but it may not equal to the actual size after img is loaded.
        /// </summary>
        /// <param name="w"></param>
        /// <param name="h"></param>
        public void SetPreviewImageSize(int w, int h)
        {
            _previewImgWidth = w;
            _previewImgHeight = h;
        }

        /// <summary>
        /// reference to original 
        /// </summary>
        public string ImageSource { get; set; }

        /// <summary>
        /// current loading/binding state
        /// </summary>
        public BinderState State { get; set; }

        /// <summary>
        /// read already loaded img
        /// </summary>
        public PixelFarm.Drawing.Image LocalImage => _localImg;

        public void ClearLocalImage()
        {
            this.State = BinderState.Unloading;//reset this to unload?

            if (_localImg != null)
            {
                if (_isLocalImgOwner)
                {
                    _localImg.Dispose();
                }
                _localImg = null;
            }

            //TODO: review here
            this.State = BinderState.Unload;//reset this to unload?
        }
        public override void Dispose()
        {
            if (this.State == BinderState.Loaded)
            {
                ClearLocalImage();
            }
        }

        public override int Width => (_localImg != null) ? _localImg.Width : _previewImgWidth; //default?

        public override int Height => (_localImg != null) ? _localImg.Height : _previewImgHeight;

        /// <summary>
        /// set local loaded image
        /// </summary>
        /// <param name="image"></param>
        public virtual void SetLocalImage(PixelFarm.Drawing.Image image, bool raiseEvent = true)
        {
            //set image to this binder
            if (image != null)
            {
                _localImg = image;
                this.State = BinderState.Loaded;
               

                if (raiseEvent)
                {
                    RaiseImageChanged();
                }
                else
                {
                    //eg. when we setLocalImage 
                    //from other thread  
                    //don't call raise image changed directly here
                    //please use 'main thread queue' to invoke this
                }
            }
            else
            {
                //if set to null 
            }
        }

        public bool HasLazyFunc => _lazyLoadImgFunc != null;

        /// <summary>
        /// set lazy img loader
        /// </summary>
        /// <param name="lazyLoadFunc"></param>
        public void SetImageLoader(LoadImageFunc lazyLoadFunc)
        {
            _lazyLoadImgFunc = lazyLoadFunc;
        }
        public void LazyLoadImage()
        {
            _lazyLoadImgFunc?.Invoke(this);
        }
        public override IntPtr GetRawBufferHead()
        {

            PixelFarm.CpuBlit.MemBitmap bmp = _localImg as PixelFarm.CpuBlit.MemBitmap;
            if (bmp != null)
            {
                return PixelFarm.CpuBlit.MemBitmap.GetBufferPtr(bmp).Ptr;
            }

            return IntPtr.Zero;
        }
        public override void ReleaseBufferHead()
        {

        }

        public override bool IsYFlipped => false;
        //
        public static readonly ImageBinder NoImage = new NoImageImageBinder();
        public virtual bool IsAtlasImage => false;

        class NoImageImageBinder : ImageBinder
        {
            public NoImageImageBinder()
            {
                this.State = BinderState.Blank;
            }
            public override IntPtr GetRawBufferHead() => IntPtr.Zero;

            public override void ReleaseBufferHead()
            {
            }
        }
    }

    public enum BinderState : byte
    {
        Unload,
        Loaded,
        Loading,
        Unloading,
        Error,
        Blank
    }

}