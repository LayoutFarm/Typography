//MIT, 2009-2015, Rene Schulte and WriteableBitmapEx Contributors, https://github.com/teichgraf/WriteableBitmapEx
//
//   Project:           WriteableBitmapEx - WriteableBitmap extensions
//   Description:       Collection of extension methods for the WriteableBitmap class.
//
//   Changed by:        $Author: unknown $
//   Changed on:        $Date: 2015-04-17 19:54:47 +0200 (Fr, 17 Apr 2015) $
//   Changed in:        $Revision: 113740 $
//   Project:           $URL: https://writeablebitmapex.svn.codeplex.com/svn/trunk/Source/WriteableBitmapEx/BitmapContext.cs $
//   Id:                $Id: BitmapContext.cs 113740 2015-04-17 17:54:47Z unknown $
//
//
//   Copyright © 2009-2015 Rene Schulte and WriteableBitmapEx Contributors
//
//   This code is open source. Please read the License.txt for details. No worries, we won't sue you! ;)
// 

using System;
namespace BitmapBufferEx
{
    /// <summary>
    /// Read Write Mode for the BitmapContext.
    /// </summary>
    public enum ReadWriteMode
    {
        /// <summary>
        /// On Dispose of a BitmapContext, do not Invalidate
        /// </summary>
        ReadOnly,

        /// <summary>
        /// On Dispose of a BitmapContext, invalidate the bitmap
        /// </summary>
        ReadWrite
    }

    public unsafe struct NativeInt32Arr
    {
        public readonly unsafe int* _inf32Buffer;
        public readonly int _len;

        public NativeInt32Arr(int* int32Buffer, int len)
        {
            _inf32Buffer = int32Buffer;
            _len = len;
        }
        //public int this[int index]
        //{
        //    get
        //    {
        //        return _inf32Buffer[index];
        //    }
        //    set
        //    {
        //        _inf32Buffer[index] = value;
        //    }
        //}


    }
    /// <summary>
    /// A disposable cross-platform wrapper around a WriteableBitmap, allowing a common API for Silverlight + WPF with locking + unlocking if necessary
    /// </summary>
    /// <remarks>Attempting to put as many preprocessor hacks in this file, to keep the rest of the codebase relatively clean</remarks>
    public struct BitmapContext : IDisposable
    {
        private readonly BitmapBuffer _writeableBitmap;
        private readonly ReadWriteMode _mode;

        private readonly int _pixelWidth;
        private readonly int _pixelHeight;

        /// <summary>
        /// The Bitmap
        /// </summary>
        public BitmapBuffer WriteableBitmap { get { return _writeableBitmap; } }

        /// <summary>
        /// Width of the bitmap
        /// </summary>
        public int Width { get { return _writeableBitmap.PixelWidth; } }

        /// <summary>
        /// Height of the bitmap
        /// </summary>
        public int Height { get { return _writeableBitmap.PixelHeight; } }

        /// <summary>
        /// Creates an instance of a BitmapContext, with default mode = ReadWrite
        /// </summary>
        /// <param name="writeableBitmap"></param>
        public BitmapContext(BitmapBuffer writeableBitmap)
            : this(writeableBitmap, ReadWriteMode.ReadWrite)
        {
        }

        /// <summary>
        /// Creates an instance of a BitmapContext, with specified ReadWriteMode
        /// </summary>
        /// <param name="writeableBitmap"></param>
        /// <param name="mode"></param>
        public BitmapContext(BitmapBuffer writeableBitmap, ReadWriteMode mode)
        {
            _writeableBitmap = writeableBitmap;
            _mode = mode;

            _pixelWidth = _writeableBitmap.PixelWidth;
            _pixelHeight = _writeableBitmap.PixelHeight;
        }

        public NativeInt32Arr Pixels
        {
            get
            {
                unsafe
                {
                    return new NativeInt32Arr((int*)_writeableBitmap.Pixels, _writeableBitmap.LenInBytes);
                }
            }
        }
        public void Dispose() { }

        /// <summary>
        /// Gets the length of the Pixels array 
        /// </summary>
        public int Length { get { return _writeableBitmap.LenInBytes / 4; } }

        /// <summary>
        /// Performs a Copy operation from source BitmapContext to destination BitmapContext
        /// </summary>
        /// <remarks>Equivalent to calling Buffer.BlockCopy in Silverlight, or native memcpy in WPF</remarks>
        public static void BlockCopy(BitmapContext src, int srcOffset, BitmapContext dest, int destOffset, int count)
        {
            unsafe
            {
                byte* srcStartAt = (byte*)src.Pixels._inf32Buffer + srcOffset;
                byte* destStartAt = (byte*)dest.Pixels._inf32Buffer + destOffset;

                PixelFarm.CpuBlit.NativeMemMx.memcpy((byte*)destStartAt, (byte*)srcStartAt, count);
            }
            //Buffer.BlockCopy(src.Pixels, srcOffset, dest.Pixels, destOffset, count);
        }

        /// <summary>
        /// Performs a Copy operation from source Array to destination BitmapContext
        /// </summary>
        /// <remarks>Equivalent to calling Buffer.BlockCopy in Silverlight, or native memcpy in WPF</remarks>
        public static unsafe void BlockCopy(int* src, int srcOffset, BitmapContext dest, int destOffset, int count)
        {
            //Buffer.BlockCopy(src, srcOffset, dest.Pixels, destOffset, count);

            byte* srcStartAt = (byte*)src + srcOffset;
            byte* destStartAt = (byte*)dest.Pixels._inf32Buffer + destOffset;
            PixelFarm.CpuBlit.NativeMemMx.memcpy((byte*)destStartAt, (byte*)srcStartAt, count);

            //PixelFarm.CpuBlit.NativeMemMx.memcpy((byte*)destStartAt, (byte*)srcStartAt, count);
        }

        ///// <summary>
        ///// Performs a Copy operation from source BitmapContext to destination Array
        ///// </summary>
        ///// <remarks>Equivalent to calling Buffer.BlockCopy in Silverlight, or native memcpy in WPF</remarks>
        //public static unsafe void BlockCopy(BitmapContext src, int srcOffset, int* dest, int destOffset, int count)
        //{
        //    //Buffer.BlockCopy(src.Pixels, srcOffset, dest, destOffset, count);

        //    byte* srcStartAt = (byte*)src.Pixels._inf32Buffer + srcOffset;
        //    byte* destStartAt = (byte*)dest + destOffset;
        //    PixelFarm.CpuBlit.NativeMemMx.memcpy((byte*)destStartAt, (byte*)srcStartAt, count);

        //}

        /// <summary>
        /// Clears the BitmapContext, filling the underlying bitmap with zeros
        /// </summary>
        public void Clear()
        {
            unsafe
            {
                byte* px = (byte*)_writeableBitmap.Pixels;
                PixelFarm.CpuBlit.NativeMemMx.memset((byte*)px, 0, _writeableBitmap.LenInBytes);
            }
            ////int[] pixels = _writeableBitmap.Pixels;
            //Array.Clear(pixels, 0, pixels.Length);
        }
    }


    /// <summary>
    /// Provides the WriteableBitmap context pixel data
    /// </summary>
    public static partial class WriteableBitmapContextExtensions
    {
        /// <summary>
        /// Gets a BitmapContext within which to perform nested IO operations on the bitmap
        /// </summary>
        /// <remarks>For WPF the BitmapContext will lock the bitmap. Call Dispose on the context to unlock</remarks>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public static BitmapContext GetBitmapContext(this BitmapBuffer bmp)
        {
            return new BitmapContext(bmp);
        }

        /// <summary>
        /// Gets a BitmapContext within which to perform nested IO operations on the bitmap
        /// </summary>
        /// <remarks>For WPF the BitmapContext will lock the bitmap. Call Dispose on the context to unlock</remarks>
        /// <param name="bmp">The bitmap.</param>
        /// <param name="mode">The ReadWriteMode. If set to ReadOnly, the bitmap will not be invalidated on dispose of the context, else it will</param>
        /// <returns></returns>
        public static BitmapContext GetBitmapContext(this BitmapBuffer bmp, ReadWriteMode mode)
        {
            return new BitmapContext(bmp, mode);
        }
    }
}