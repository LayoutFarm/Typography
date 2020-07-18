//BSD, 2014-present, WinterDev
//MIT, 2018-present, WinterDev
using System;
namespace PixelFarm.Drawing
{
    public delegate void LoadImageFunc(ImageBinder binder); 

    public class ImageBinder : Image
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


        public ImageBinder(PixelFarm.Drawing.Image img, bool isMemBmpOwner = false)
        {
#if DEBUG
            if (img == null)
            {
                throw new NotSupportedException();
            }
#endif
            //binder to image
            _localImg = img;
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
        public virtual void ReleaseLocalBitmapIfRequired()
        {

        }
        public BitmapBufferFormat BitmapFormat { get; set; }

        public virtual bool IsYFlipped { get; set; }
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
        public override void ReleaseRawBufferHead(IntPtr ptr)
        {
            if (_localImg != null)
            {
                _localImg.ReleaseRawBufferHead(ptr);
            }
        }
        public override IntPtr GetRawBufferHead()
        {
            if (_localImg != null)
            {
                return _localImg.GetRawBufferHead();
            }
            return IntPtr.Zero;
        }
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


#if DEBUG
        public void dbugNotifyUsage() { }
#endif

        //
        public static readonly ImageBinder NoImage = new NoImageImageBinder();
        public virtual bool IsAtlasImage => false;

        public override bool IsReferenceImage => true;

        public override int ReferenceX => 0;

        public override int ReferenceY => 0;

        class NoImageImageBinder : ImageBinder
        {
            public NoImageImageBinder()
            {
                this.State = BinderState.Blank;
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
