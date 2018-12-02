//MIT, 2017-present, WinterDev
using System;
namespace PixelFarm.CpuBlit.Imaging
{
    public delegate void SaveImageBufferToFileDel(IntPtr imgBuffer, int stride, int width, int height, string filename);

    //---------------------------------
    //png
    public static class PngImageReader
    {

    }
    public static class PngImageWriter
    {
        public static void SaveImgBufferToPngFile(TempMemPtr imgBuffer, int stride, int width, int height, string filename)
        {
            if (s_saveToFile != null)
            {
                unsafe
                {
                    //fixed (int* head = imgBuffer.Ptr)

                    int* head = (int*)imgBuffer.Ptr;
                    {
                        s_saveToFile((IntPtr)head, stride, width, height, filename);
                    }
                }
            }
        }
        static SaveImageBufferToFileDel s_saveToFile;

        public static bool HasDefaultSaveToFileDelegate()
        {
            return s_saveToFile != null;
        }
        public static void InstallImageSaveToFileService(SaveImageBufferToFileDel saveToFileDelegate)
        {
            s_saveToFile = saveToFileDelegate;
        }


#if DEBUG
        public static void dbugSaveToPngFile(this MemBitmap bmp, string filename)
        {

            SaveImgBufferToPngFile(MemBitmap.GetBufferPtr(bmp),
                bmp.Stride,
                bmp.Width,
                bmp.Height,
                filename);
        }
#endif
    }

    //---------------------------------
    //jpg

    public static class JpgImageReader
    {


    }
    public static class JpgImageWriter
    {
        public static void SaveImgBufferToJpgFile(
            int[] imgBuffer,
            int stride,
            int width,
            int height,
            string filename)
        {
            if (s_saveToFile != null)
            {
                unsafe
                {
                    fixed (int* head = &imgBuffer[0])
                    {
                        s_saveToFile((IntPtr)head, stride, width, height, filename);
                    }
                }
            }
        }
        public static unsafe void SaveImgBufferToJpgFileUnsafe(
           TempMemPtr tmpMem,
           int stride,
           int width,
           int height,
           string filename)
        {
            if (s_saveToFile != null)
            {
                unsafe
                {
                    //fixed (int* head = &imgBuffer[0])
                    int* head = (int*)tmpMem.Ptr;
                    {
                        s_saveToFile((IntPtr)head, stride, width, height, filename);
                    }
                }
            }
        }
        static SaveImageBufferToFileDel s_saveToFile;

        public static bool HasDefaultSaveToFileDelegate()
        {
            return s_saveToFile != null;
        }
        public static void InstallImageSaveToFileService(SaveImageBufferToFileDel saveToFileDelegate)
        {
            s_saveToFile = saveToFileDelegate;
        }


#if DEBUG
        public static void dbugSaveToJpgFile(this MemBitmap bmp, string filename)
        {
            TempMemPtr tmpMem = MemBitmap.GetBufferPtr(bmp);
            SaveImgBufferToJpgFileUnsafe(tmpMem,
                bmp.Stride,
                bmp.Width,
                bmp.Height,
                filename);
            tmpMem.Dispose();
        }
#endif
    }

}