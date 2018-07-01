//BSD, 2014-present, WinterDev
//MIT, 2018-present, WinterDev
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
        public abstract PixelFarm.CpuBlit.ActualBitmap ReadPngBitmap(string filename);
        public abstract void SavePngBitmap(PixelFarm.CpuBlit.ActualBitmap bmp, string filename);

    }

    public static class StorageService
    {
        static StorageServiceProvider s_provider;
        public static void RegisterProvider(StorageServiceProvider provider)
        {
            s_provider = provider;
        }
        public static StorageServiceProvider Provider
        {
            get { return s_provider; }
        }
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

    public abstract class ImageBinder
    {
        PixelFarm.Drawing.Image _image;
        string _imageSource;
        LazyLoadImageFunc lazyLoadImgFunc;
        public event System.EventHandler ImageChanged;

#if DEBUG
        static int dbugTotalId;
        public int dbugId = dbugTotalId++;
#endif

        public ImageBinder()
        {
        }
        public ImageBinder(string imgSource)
        {
            this._imageSource = imgSource;
        }
        public string ImageSource
        {
            get { return this._imageSource; }
        }
        public ImageBinderState State
        {
            get;
            set;
        }
        public PixelFarm.Drawing.Image Image
        {
            get { return this._image; }
        }

        public int ImageWidth
        {
            get
            {
                if (this._image != null)
                {
                    return this._image.Width;
                }
                else
                {
                    //default?
                    return 16;
                }
            }
        }
        public int ImageHeight
        {
            get
            {
                if (this._image != null)
                {
                    return this._image.Height;
                }
                else
                {   //default?
                    return 16;
                }
            }
        }

        public void SetImage(PixelFarm.Drawing.Image image)
        {
            //set image to this binder
            if (image != null)
            {
                this._image = image;
                this.State = ImageBinderState.Loaded;
                this.OnImageChanged();
            }
        }
        protected virtual void OnImageChanged()
        {
            ImageChanged?.Invoke(this, System.EventArgs.Empty);
        }
        public bool HasLazyFunc
        {
            get { return this.lazyLoadImgFunc != null; }
        }

        public void SetLazyLoaderFunc(LazyLoadImageFunc lazyLoadFunc)
        {
            this.lazyLoadImgFunc = lazyLoadFunc;
        }
        public void LazyLoadImage()
        {
            if (this.lazyLoadImgFunc != null)
            {
                this.lazyLoadImgFunc(this);
            }
        }

        //
        public static readonly ImageBinder NoImage = new NoImageImageBinder();
        class NoImageImageBinder : ImageBinder
        {
            public NoImageImageBinder()
            {
                this.State = ImageBinderState.NoImage;
            }
        }

    }


    public delegate void LazyLoadImageFunc(ImageBinder binder);
    public enum ImageBinderState
    {
        Unload,
        Loaded,
        Loading,
        Error,
        NoImage
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
            public int RightIndex { get { return startIndex + length; } }
            public static readonly TextSplitBound Empty = new TextSplitBound();

        }
        //TODO: review here
        public static class Default
        {
            public static ITextBreaker TextBreaker { get; set; }
        }

    }
}