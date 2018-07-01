//BSD, 2014-present, WinterDev
//----------------------------------------------------------------------------
// Anti-Grain Geometry - Version 2.4
// Copyright (C) 2002-2005 Maxim Shemanarev (http://www.antigrain.com)
//
// C# Port port by: Lars Brubaker
//                  larsbrubaker@gmail.com
// Copyright (C) 2007
//
// Permission to copy, use, modify, sell and distribute this software 
// is granted provided this copyright notice appears in all copies. 
// This software is provided "as is" without express or implied
// warranty, and with no claim as to its suitability for any purpose.
//
//----------------------------------------------------------------------------
// Contact: mcseem@antigrain.com
//          mcseemagg@yahoo.com
//          http://www.antigrain.com
//----------------------------------------------------------------------------

using System;
namespace PixelFarm.CpuBlit.PixelProcessing
{
    /// <summary>
    /// sub-image reader /writer/blend part of org bitmap
    /// </summary>
    public class SubBitmapBlender : BitmapBlenderBase
    {
        public SubBitmapBlender(IBitmapBlender image,
            int arrayOffset32,
            int width,
            int height)
        {
            this.OutputPixelBlender = image.OutputPixelBlender;
            AttachBuffer(image.GetOrgInt32Buffer(),
                arrayOffset32,
                width,
                height,
                image.Stride,
                image.BitDepth,
                image.BytesBetweenPixelsInclusive);
        }

        public SubBitmapBlender(int[] buffer,
            int arrayOffset32,
            int width,
            int height,
            int strideInBytes,
            int bitDepth,
            int distanceInBytesBetweenPixelsInclusive)
        {
            AttachBuffer(buffer,
                arrayOffset32,
                width,
                height,
                strideInBytes, bitDepth,
                distanceInBytesBetweenPixelsInclusive);
        }
        public SubBitmapBlender(IBitmapSrc image,
            PixelBlender32 blender,
            int distanceBetweenPixelsInclusive,
            int arrayOffset32,
            int bitsPerPixel)
        {

            this.OutputPixelBlender = blender;
            Attach(image, blender, distanceBetweenPixelsInclusive, arrayOffset32, bitsPerPixel);
        }
        public SubBitmapBlender(IBitmapSrc image, PixelBlender32 blender)
        {
            Attach(image, blender, image.BytesBetweenPixelsInclusive, 0, image.BitDepth);
        }

        public override void ReplaceBuffer(int[] newbuffer)
        {
            if (_sourceImage != null)
            {
                _sourceImage.ReplaceBuffer(newbuffer);
            }

        }

        void AttachBuffer(int[] buffer,
          int elemOffset,
          int width,
          int height,
          int strideInBytes,
          int bitDepth,
          int distanceInBytesBetweenPixelsInclusive)
        {

            SetBufferToNull();
            SetDimmensionAndFormat(width, height, strideInBytes, bitDepth,
                distanceInBytesBetweenPixelsInclusive);
            SetBuffer(buffer, elemOffset);

        }

        IBitmapSrc _sourceImage;
        void Attach(IBitmapSrc sourceImage,
          PixelBlender32 outputPxBlender,
          int distanceBetweenPixelsInclusive,
          int arrayElemOffset,
          int bitsPerPixel)
        {
            _sourceImage = sourceImage;
            SetDimmensionAndFormat(sourceImage.Width,
                sourceImage.Height,
                sourceImage.Stride,
                bitsPerPixel,
                distanceBetweenPixelsInclusive);

            int srcOffset32 = sourceImage.GetBufferOffsetXY32(0, 0);
            int[] buffer = sourceImage.GetOrgInt32Buffer();
            SetBuffer(buffer, srcOffset32 + arrayElemOffset);

            this.OutputPixelBlender = outputPxBlender;
        }
        //bool Attach(IBitmapBlender sourceImage, int x1, int y1, int x2, int y2)
        //{
        //    _sourceImage = sourceImage;
        //    SetBufferToNull();
        //    if (x1 > x2 || y1 > y2)
        //    {
        //        throw new Exception("You need to have your x1 and y1 be the lower left corner of your sub image.");
        //    }
        //    RectInt boundsRect = new RectInt(x1, y1, x2, y2);
        //    if (boundsRect.Clip(new RectInt(0, 0, sourceImage.Width - 1, sourceImage.Height - 1)))
        //    {
        //        SetDimmensionAndFormat(boundsRect.Width, boundsRect.Height, sourceImage.Stride, sourceImage.BitDepth, sourceImage.BytesBetweenPixelsInclusive);
        //        int bufferOffset = sourceImage.GetByteBufferOffsetXY(boundsRect.Left, boundsRect.Bottom) / 4;
        //        int[] buffer = sourceImage.GetInt32Buffer();
        //        SetBuffer(buffer, bufferOffset);
        //        return true;
        //    }

        //    return false;
        //}

        void SetBuffer(int[] int32Buffer, int arrayElemOffset)
        {
            int height = this.Height;

            if (int32Buffer.Length < height * Width)
            {
                throw new Exception("Your buffer does not have enough room it it for your height and strideInBytes.");
            }

            SetBuffer(int32Buffer);
            int32ArrayStartPixelAt = arrayElemOffset;

            if (this.Stride < 0) //stride in bytes
            {
                //TODO: review here 
                int addAmount = -((height - 1) * Width);
                int32ArrayStartPixelAt = addAmount + arrayElemOffset;
            }
            SetUpLookupTables();
        }
    }





    public static class BitmapBlenderExtension
    {
        /// <summary>
        /// This will create a new ImageBuffer that references the same memory as the image that you took the sub image from.
        /// It will modify the original main image when you draw to it.
        /// </summary>
        /// <param name="parentImage"></param>
        /// <param name="subImgBounds"></param>
        /// <returns></returns>
        public static SubBitmapBlender CreateSubBitmapBlender(IBitmapBlender parentImage, RectInt subImgBounds)
        {
            if (subImgBounds.Left < 0 || subImgBounds.Bottom < 0 || subImgBounds.Right > parentImage.Width || subImgBounds.Top > parentImage.Height
                || subImgBounds.Left >= subImgBounds.Right || subImgBounds.Bottom >= subImgBounds.Top)
            {
                throw new ArgumentException("The subImageBounds must be on the image and valid.");
            }

            int left = Math.Max(0, subImgBounds.Left);
            int bottom = Math.Max(0, subImgBounds.Bottom);
            int width = Math.Min(parentImage.Width - left, subImgBounds.Width);
            int height = Math.Min(parentImage.Height - bottom, subImgBounds.Height);
            return new SubBitmapBlender(parentImage, parentImage.GetBufferOffsetXY32(left, bottom), width, height);
        }
    }

}