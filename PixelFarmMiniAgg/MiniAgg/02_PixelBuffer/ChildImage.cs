//BSD, 2014-2016, WinterDev
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
namespace PixelFarm.Agg.Imaging
{
    public class ChildImage : ImageReaderWriterBase
    {
        int bufferOffset; // the beggining of the image in this buffer 
        public ChildImage(IImageReaderWriter image,
            int bufferOffsetToFirstPixel,
            int width,
            int height)
        {
            SetRecieveBlender(image.GetRecieveBlender());
            AttachBuffer(image.GetBuffer(),
               bufferOffsetToFirstPixel,
                width,
                height,
                image.Stride,
                image.BitDepth,
                image.BytesBetweenPixelsInclusive);
        }
        public ChildImage(byte[] buffer,
            int bufferOffsetToFirstPixel,
            int width,
            int height,
            int strideInBytes,
            int bitDepth,
            int distanceInBytesBetweenPixelsInclusive)
        {
            AttachBuffer(buffer,
                bufferOffsetToFirstPixel,
                width,
                height,
                strideInBytes, bitDepth,
                distanceInBytesBetweenPixelsInclusive);
        }
        public ChildImage(IImageReaderWriter image,
            IPixelBlender blender,
            int distanceBetweenPixelsInclusive,
            int bufferOffset,
            int bitsPerPixel)
        {
            SetRecieveBlender(blender);
            Attach(image, blender, distanceBetweenPixelsInclusive, bufferOffset, bitsPerPixel);
        }
        public ChildImage(IImageReaderWriter image, IPixelBlender blender)
        {
            Attach(image, blender, image.BytesBetweenPixelsInclusive, 0, image.BitDepth);
        }
        public ChildImage(IImageReaderWriter image, IPixelBlender blender, int x1, int y1, int x2, int y2)
        {
            SetRecieveBlender(blender);
            Attach(image, x1, y1, x2, y2);
        }


        void AttachBuffer(byte[] buffer,
          int bufferOffset,
          int width,
          int height,
          int strideInBytes,
          int bitDepth,
          int distanceInBytesBetweenPixelsInclusive)
        {
            m_ByteBuffer = null;
            SetDimmensionAndFormat(width, height, strideInBytes, bitDepth,
                distanceInBytesBetweenPixelsInclusive);
            SetBuffer(buffer, bufferOffset);
        }

        void Attach(IImageReaderWriter sourceImage,
          IPixelBlender recieveBlender,
          int distanceBetweenPixelsInclusive,
          int bufferOffset,
          int bitsPerPixel)
        {
            SetDimmensionAndFormat(sourceImage.Width,
                sourceImage.Height,
                sourceImage.Stride,
                bitsPerPixel,
                distanceBetweenPixelsInclusive);
            int offset = sourceImage.GetBufferOffsetXY(0, 0);
            byte[] buffer = sourceImage.GetBuffer();
            SetBuffer(buffer, offset + bufferOffset);
            SetRecieveBlender(recieveBlender);
        }
        bool Attach(IImageReaderWriter sourceImage, int x1, int y1, int x2, int y2)
        {
            m_ByteBuffer = null;
            if (x1 > x2 || y1 > y2)
            {
                throw new Exception("You need to have your x1 and y1 be the lower left corner of your sub image.");
            }
            RectInt boundsRect = new RectInt(x1, y1, x2, y2);
            if (boundsRect.Clip(new RectInt(0, 0, (int)sourceImage.Width - 1, (int)sourceImage.Height - 1)))
            {
                SetDimmensionAndFormat(boundsRect.Width, boundsRect.Height, sourceImage.Stride, sourceImage.BitDepth, sourceImage.BytesBetweenPixelsInclusive);
                int bufferOffset = sourceImage.GetBufferOffsetXY(boundsRect.Left, boundsRect.Bottom);
                byte[] buffer = sourceImage.GetBuffer();
                SetBuffer(buffer, bufferOffset);
                return true;
            }

            return false;
        }

        void SetBuffer(byte[] byteBuffer, int bufferOffset)
        {
            int height = this.Height;
            int strideInBytes = this.Stride;
            if (byteBuffer.Length < height * strideInBytes)
            {
                throw new Exception("Your buffer does not have enough room it it for your height and strideInBytes.");
            }
            this.m_ByteBuffer = byteBuffer;
            this.bufferOffset = bufferFirstPixel = bufferOffset;
            if (strideInBytes < 0)
            {
                int addAmount = -((int)((int)height - 1) * strideInBytes);
                bufferFirstPixel = addAmount + bufferOffset;
            }
            SetUpLookupTables();
        }
    }
}