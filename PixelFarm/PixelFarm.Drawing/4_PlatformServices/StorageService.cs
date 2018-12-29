//BSD, 2014-present, WinterDev
//MIT, 2018-present, WinterDev
using System;
using System.Collections.Generic;
using System.IO;
namespace PixelFarm.Platforms
{
    public abstract class StorageServiceProvider
    {
        public abstract bool DataExists(string dataName);
        public abstract void SaveData(string dataName, byte[] content);
        public abstract byte[] ReadData(string dataName);
        public Stream ReadDataStream(string dataName)
        {
            byte[] data = ReadData(dataName);
            return new MemoryStream(data);
        }
        public abstract PixelFarm.CpuBlit.MemBitmap ReadPngBitmap(string filename);
        public abstract void SavePngBitmap(PixelFarm.CpuBlit.MemBitmap bmp, string filename);

    }

    public static class StorageService
    {
        static StorageServiceProvider s_provider;
        public static void RegisterProvider(StorageServiceProvider provider)
        {
            s_provider = provider;
        }
        public static StorageServiceProvider Provider => s_provider;
    }
}




namespace LayoutFarm
{

    //temp here, 
    //these will be moved later


    public struct WordBreakInfo
    {
        public int breakAt;
        public byte wordKind;
    }

    public interface ITextBreaker
    {
        void DoBreak(char[] inputBuffer, int startIndex, int len, List<WordBreakInfo> breakAtList);
        void DoBreak(char[] inputBuffer, int startIndex, int len, List<int> breakAtList);
    }


    public static class UIMsgQueue
    {
        public delegate void RunOnceDelegate();
        static Action<RunOnceDelegate> s_runOnceRegisterImpl;
        public static void RegisterRunOnce(RunOnceDelegate runOnce)
        {
            if (s_runOnceRegisterImpl == null)
            {
                throw new NotSupportedException();
            }
            s_runOnceRegisterImpl(runOnce);
        }
        public static void RegisterRunOnceImpl(Action<RunOnceDelegate> runOnceRegisterImpl)
        {
            s_runOnceRegisterImpl = runOnceRegisterImpl;
        }
    }
    public class ImageBinder : PixelFarm.Drawing.BitmapBufferProvider
    {

        /// <summary>
        /// local img cached
        /// </summary>
        PixelFarm.Drawing.Image _localImg;
        bool _isLocalImgOwner;

        LoadImageFunc _lazyLoadImgFunc;


        int _previewImgWidth = 16; //default ?
        int _previewImgHeight = 16;

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

        public override void NotifyUsage()
        {
        }
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
        public virtual void SetLocalImage(PixelFarm.Drawing.Image image, bool fromAnotherThread = false)
        {
            //set image to this binder
            if (image != null)
            {
                _localImg = image;
                this.State = BinderState.Loaded;
                if (!fromAnotherThread)
                {
                    this.RaiseImageChanged();
                }
                else
                {
                    UIMsgQueue.RegisterRunOnce(() => this.RaiseImageChanged());
                }
            }
            else
            {
                //if set to null

            }
        }

        public virtual void RaiseImageChanged()
        {
            ImageChanged?.Invoke(this, System.EventArgs.Empty);
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

    public delegate void LoadImageFunc(ImageBinder binder);

    public enum BinderState : byte
    {
        Unload,
        Loaded,
        Loading,
        Unloading,
        Error,
        Blank
    }


    namespace Composers
    {
        //TODO: review here
        public struct TextSplitBound
        {
            public readonly int startIndex;
            public readonly int length;
            public TextSplitBound(int startIndex, int length)
            {
                this.startIndex = startIndex;
                this.length = length;
            }

            public int RightIndex => startIndex + length;

            public static readonly TextSplitBound Empty = new TextSplitBound();

#if DEBUG
            public override string ToString()
            {
                return startIndex + ":+" + length;
            }
#endif

        }
        //TODO: review here
        public static class Default
        {
            public static ITextBreaker TextBreaker { get; set; }
        }

    }
}