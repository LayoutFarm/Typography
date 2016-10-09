//MIT, 2014-2016, WinterDev

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
using PixelFarm.Drawing;
using PixelFarm.Agg.Image;
using PixelFarm.Agg.Transform;
namespace PixelFarm.Agg
{
    public sealed partial class ImageGraphics2D : Graphics2D
    {
        ImageReaderWriterBase destImageReaderWriter;
        ScanlinePacked8 sclinePack8;
        VertexStore myTmpImgRectVxs = new VertexStore();
        ScanlineRasToDestBitmapRenderer sclineRasToBmp;
        PixelBlenderBGRA pixBlenderRGBA32;
        IPixelBlender currentBlender;
        double ox; //canvas origin x
        double oy; //canvas origin y
        int destWidth;
        int destHeight;
        RectInt clipBox;
        ImageInterpolationQuality imgInterpolationQuality = ImageInterpolationQuality.Bilinear;
        GraphicsPlatform gfxPlatform;
        public ImageGraphics2D(ActualImage destImage, GraphicsPlatform gfxPlatform)
        {
            //create from actual image
            this.gfxPlatform = gfxPlatform;
            this.destActualImage = destImage;
            this.destImageReaderWriter = new MyImageReaderWriter(destImage);
            this.sclineRas = new ScanlineRasterizer();
            this.sclineRasToBmp = new ScanlineRasToDestBitmapRenderer();
            this.destWidth = destImage.Width;
            this.destHeight = destImage.Height;
            this.clipBox = new RectInt(0, 0, destImage.Width, destImage.Height);
            this.sclineRas.SetClipBox(this.clipBox);
            this.sclinePack8 = new ScanlinePacked8();
            this.currentBlender = this.pixBlenderRGBA32 = new PixelBlenderBGRA();
        }
        public GraphicsPlatform GfxPlatform
        {
            get { return this.gfxPlatform; }
        }
        public override ScanlinePacked8 ScanlinePacked8
        {
            get { return this.sclinePack8; }
        }
        public override IPixelBlender PixelBlender
        {
            get
            {
                return this.currentBlender;
            }
            set
            {
                this.currentBlender = value;
            }
        }
        public override ImageReaderWriterBase DestImage
        {
            get { return this.destImageReaderWriter; }
        }
        public override ScanlineRasToDestBitmapRenderer ScanlineRasToDestBitmap
        {
            get { return this.sclineRasToBmp; }
        }
        public override void SetClippingRect(RectInt rect)
        {
            ScanlineRasterizer.SetClipBox(rect);
        }
        public override RectInt GetClippingRect()
        {
            return ScanlineRasterizer.GetVectorClipBox();
        }
        public ImageInterpolationQuality ImageInterpolationQuality
        {
            get { return this.ImageInterpolationQuality; }
            set { this.imgInterpolationQuality = value; }
        }
        VertexStore GetFreeVxs()
        {
            if (myTmpImgRectVxs != null)
            {
                VertexStore tmp = this.myTmpImgRectVxs;
                this.myTmpImgRectVxs = null;
                return tmp;
            }
            else
            {
                return new VertexStore(4);
            }
        }
        void ReleaseVxs(VertexStore vxs)
        {
            this.myTmpImgRectVxs = vxs;
            vxs.Clear();
        }
        public override void Clear(Color color)
        {
            RectInt clippingRectInt = GetClippingRect();
            var destImage = this.DestImage;
            int width = destImage.Width;
            int height = destImage.Height;
            byte[] buffer = destImage.GetBuffer();
            switch (destImage.BitDepth)
            {
                case 8:
                    {
                        //int bytesBetweenPixels = destImage.BytesBetweenPixelsInclusive;
                        //byte byteColor = color.Red0To255;
                        //int clipRectLeft = clippingRectInt.Left;

                        //for (int y = clippingRectInt.Bottom; y < clippingRectInt.Top; ++y)
                        //{
                        //    int bufferOffset = destImage.GetBufferOffsetXY(clipRectLeft, y);
                        //    for (int x = 0; x < clippingRectInt.Width; ++x)
                        //    {
                        //        buffer[bufferOffset] = color.blue;
                        //        bufferOffset += bytesBetweenPixels;
                        //    }
                        //}
                        throw new NotSupportedException("temp");
                    }
                case 24:
                    {
                        //int bytesBetweenPixels = destImage.BytesBetweenPixelsInclusive;
                        //int clipRectLeft = clippingRectInt.Left;
                        //for (int y = clippingRectInt.Bottom; y < clippingRectInt.Top; y++)
                        //{
                        //    int bufferOffset = destImage.GetBufferOffsetXY(clipRectLeft, y);
                        //    for (int x = 0; x < clippingRectInt.Width; ++x)
                        //    {
                        //        buffer[bufferOffset + 0] = color.blue;
                        //        buffer[bufferOffset + 1] = color.green;
                        //        buffer[bufferOffset + 2] = color.red;
                        //        bufferOffset += bytesBetweenPixels;
                        //    }
                        //}
                        throw new NotSupportedException("temp");
                    }
                    break;
                case 32:
                    {
                        //------------------------------
                        //fast clear buffer
                        //skip clipping ****
                        //TODO: reimplement clipping***
                        //------------------------------ 
                        if (color == Color.White)
                        {
                            //fast cleat with white color
                            int n = buffer.Length / 4;
                            unsafe
                            {
                                fixed (void* head = &buffer[0])
                                {
                                    uint* head_i32 = (uint*)head;
                                    for (int i = n - 1; i >= 0; --i)
                                    {
                                        *head_i32 = 0xffffffff; //white (ARGB)
                                        head_i32++;
                                    }
                                }
                            }
                        }
                        else if (color == Color.Black)
                        {
                            //fast cleat with black color
                            int n = buffer.Length / 4;
                            unsafe
                            {
                                fixed (void* head = &buffer[0])
                                {
                                    uint* head_i32 = (uint*)head;
                                    for (int i = n - 1; i >= 0; --i)
                                    {
                                        *head_i32 = 0xff000000; //black (ARGB)
                                        head_i32++;
                                    }
                                }
                            }
                        }
                        else
                        {
                            //other color
                            uint colorARGB = (uint)((color.alpha << 24) | ((color.red << 16) | (color.green << 8) | color.blue));
                            int n = buffer.Length / 4;
                            unsafe
                            {
                                fixed (void* head = &buffer[0])
                                {
                                    uint* head_i32 = (uint*)head;
                                    for (int i = n - 1; i >= 0; --i)
                                    {
                                        *head_i32 = colorARGB;
                                        head_i32++;
                                    }
                                }
                            }
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public override void Render(VertexStoreSnap vxsSnap, Drawing.Color color)
        {
            //reset rasterizer before render each vertextSnap 
            //-----------------------------
            sclineRas.Reset();
            Affine transform = this.CurrentTransformMatrix;
            if (!transform.IsIdentity())
            {
                sclineRas.AddPath(transform.TransformToVxs(vxsSnap));
            }
            else
            {
                sclineRas.AddPath(vxsSnap);
            }
            sclineRasToBmp.RenderWithColor(destImageReaderWriter, sclineRas, sclinePack8, color);
            unchecked { destImageChanged++; };
            //-----------------------------
        }
    }
}
