//MIT, 2014-present, WinterDev

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
using PixelFarm.CpuBlit.Imaging;
using PixelFarm.CpuBlit.VertexProcessing;
using PixelFarm.CpuBlit.Rasterization;
using PixelFarm.CpuBlit.PixelProcessing;
namespace PixelFarm.CpuBlit
{
    public sealed partial class AggRenderSurface
    {
        MyBitmapBlender _destBitmapBlender;
        ScanlinePacked8 sclinePack8;

        DestBitmapRasterizer _bmpRasterizer;

        double ox; //canvas origin x
        double oy; //canvas origin y
        int destWidth;
        int destHeight;
        RectInt clipBox;
        ImageInterpolationQuality imgInterpolationQuality = ImageInterpolationQuality.Bilinear;


        public AggRenderSurface(ActualBitmap destImage)
        {
            //create from actual image 

            this.destActualImage = destImage;

            this._destBitmapBlender = new MyBitmapBlender(destImage, new PixelBlenderBGRA());
            //
            this._sclineRas = new ScanlineRasterizer(destImage.Width, destImage.Height);
            this._bmpRasterizer = new DestBitmapRasterizer();
            //
            this.destWidth = destImage.Width;
            this.destHeight = destImage.Height;
            //
            this.clipBox = new RectInt(0, 0, destImage.Width, destImage.Height);
            this._sclineRas.SetClipBox(this.clipBox);
            this.sclinePack8 = new ScanlinePacked8();
        }


        public int Width { get { return destWidth; } }
        public int Height { get { return destHeight; } }

        public ScanlineRasterizer ScanlineRasterizer
        {
            get { return _sclineRas; }
        }
        public ActualBitmap DestActualImage
        {
            get { return this.destActualImage; }
        }
        public BitmapBlenderBase DestBitmapBlender
        {
            get { return this._destBitmapBlender; }
        }

        public ScanlinePacked8 ScanlinePacked8
        {
            get { return this.sclinePack8; }
        }
        public PixelProcessing.PixelBlender32 PixelBlender
        {
            get
            {
                return this._destBitmapBlender.OutputPixelBlender;
            }
            set
            {
                this._destBitmapBlender.OutputPixelBlender = value;
            }
        }

        public DestBitmapRasterizer BitmapRasterizer
        {
            get { return this._bmpRasterizer; }
        }
        public void SetClippingRect(RectInt rect)
        {
            ScanlineRasterizer.SetClipBox(rect);
        }
        public RectInt GetClippingRect()
        {
            return ScanlineRasterizer.GetVectorClipBox();
        }
        public ImageInterpolationQuality ImageInterpolationQuality
        {
            get { return this.ImageInterpolationQuality; }
            set { this.imgInterpolationQuality = value; }
        }

        public void Clear(Color color)
        {
            RectInt clippingRectInt = GetClippingRect();
            var destImage = this.DestBitmapBlender;
            int width = destImage.Width;
            int height = destImage.Height;


            unsafe
            {
                TempMemPtr tmp = destImage.GetBufferPtr();
                int* buffer = (int*)tmp.Ptr;
                int len32 = tmp.LengthInBytes / 4;

                switch (destImage.BitDepth)
                {
                    default: throw new NotSupportedException();
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
                                int n = len32;
                                unsafe
                                {
                                    //fixed (void* head = &buffer[0])
                                    {
                                        uint* head_i32 = (uint*)buffer;
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
                                int n = len32;
                                unsafe
                                {
                                    //fixed (void* head = &buffer[0])
                                    {
                                        uint* head_i32 = (uint*)buffer;
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
                                //#if WIN
                                //                            uint colorARGB = (uint)((color.alpha << 24) | ((color.red << 16) | (color.green << 8) | color.blue));
                                //#else
                                //                            uint colorARGB = (uint)((color.alpha << 24) | ((color.blue << 16) | (color.green << 8) | color.red));
                                //#endif

                                //ARGB
                                uint colorARGB = (uint)((color.alpha << 24) | ((color.red << 16) | (color.green << 8) | color.blue));
                                int n = len32;
                                unsafe
                                {
                                    //fixed (void* head = &buffer[0])
                                    {
                                        uint* head_i32 = (uint*)buffer;
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
                }
            }

        }





        /// <summary>
        /// we do NOT store vxs
        /// </summary>
        /// <param name="vxs"></param>
        /// <param name="c"></param>
        public void Render(VertexStore vxs, Drawing.Color c)
        {
            //reset rasterizer before render each vertextSnap 
            //-----------------------------
            _sclineRas.Reset();
            Affine transform = this.CurrentTransformMatrix;
            if (!transform.IsIdentity())
            {

                _sclineRas.AddPath(vxs, transform);

                //-------------------------
                //since sclineRas do NOT store vxs
                //then we can reuse the vxs***
                //-------------------------
            }
            else
            {
                _sclineRas.AddPath(vxs);
            }
            _bmpRasterizer.RenderWithColor(_destBitmapBlender, _sclineRas, sclinePack8, c);
            unchecked { destImageChanged++; };
            //-----------------------------
        }
        ActualBitmap destActualImage;
        ScanlineRasterizer _sclineRas;
        Affine currentTxMatrix = Affine.IdentityMatrix;
        public Affine CurrentTransformMatrix
        {
            get { return this.currentTxMatrix; }
            set
            {
                this.currentTxMatrix = value;
            }
        }
        public float ScanlineRasOriginX
        {
            get { return _sclineRas.OffsetOriginX; }
        }
        public float ScanlineRasOriginY
        {
            get { return _sclineRas.OffsetOriginY; }
        }
        public void SetScanlineRasOrigin(float x, float y)
        {
            _sclineRas.OffsetOriginX = x;
            _sclineRas.OffsetOriginY = y;
        }
        public bool UseSubPixelLcdEffect
        {
            get
            {
                return this._sclineRas.ExtendWidthX3ForSubPixelLcdEffect;
            }
            set
            {
                if (value)
                {
                    //TODO: review here again             
                    this._sclineRas.ExtendWidthX3ForSubPixelLcdEffect = true;
                    this._bmpRasterizer.ScanlineRenderMode = ScanlineRenderMode.SubPixelLcdEffect;
                }
                else
                {
                    this._sclineRas.ExtendWidthX3ForSubPixelLcdEffect = false;
                    this._bmpRasterizer.ScanlineRenderMode = ScanlineRenderMode.Default;
                }
            }
        }
#if DEBUG
        VertexStore dbug_v1 = new VertexStore();
        VertexStore dbug_v2 = new VertexStore();
        Stroke dbugStroke = new Stroke(1);
        public void dbugLine(double x1, double y1, double x2, double y2, Drawing.Color color)
        {


            dbug_v1.AddMoveTo(x1, y1);
            dbug_v1.AddLineTo(x2, y2);
            //dbug_v1.AddStop();

            dbugStroke.MakeVxs(dbug_v1, dbug_v2);
            Render(dbug_v2, color);
            dbug_v1.Clear();
            dbug_v2.Clear();
        }
#endif

    }
}
