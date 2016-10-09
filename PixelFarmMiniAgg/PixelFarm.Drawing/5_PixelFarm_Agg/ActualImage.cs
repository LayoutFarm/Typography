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
using PixelFarm.Agg.Image;
namespace PixelFarm.Agg
{
    public sealed class ActualImage
    {
        int width;
        int height;
        int stride;
        int bitDepth;
        PixelFormat pixelFormat;
        byte[] pixelBuffer;
        public ActualImage(int width, int height, PixelFormat format)
        {
            //width and height must >0 
            this.width = width;
            this.height = height;
            switch (this.pixelFormat = format)
            {
                case PixelFormat.ARGB32:
                    {
                        this.bitDepth = 32;
                        this.stride = width * (32 / 8);
                        this.pixelBuffer = new byte[stride * height];
                    }
                    break;
                case PixelFormat.GrayScale8:
                    {
                        this.bitDepth = 8; //bit per pixel
                        int bytesPerPixel = (bitDepth + 7) / 8;
                        this.stride = 4 * ((width * bytesPerPixel + 3) / 4);
                        this.pixelBuffer = new byte[stride * height];
                    }
                    break;
                case PixelFormat.RGB24:
                    {
                        this.bitDepth = 24; //bit per pixel
                        int bytesPerPixel = (bitDepth + 7) / 8;
                        this.stride = 4 * ((width * bytesPerPixel + 3) / 4);
                        this.pixelBuffer = new byte[stride * height];
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
        public int Width
        {
            get { return this.width; }
        }
        public int Height
        {
            get { return this.height; }
        }
        public RectInt Bounds
        {
            get { return new RectInt(0, 0, this.width, this.height); }
        }

        public PixelFormat PixelFormat { get { return this.pixelFormat; } }
        public int Stride { get { return this.stride; } }
        public int BitDepth { get { return this.bitDepth; } }
        public byte[] GetBuffer()
        {
            return this.pixelBuffer;
        }
    }
}