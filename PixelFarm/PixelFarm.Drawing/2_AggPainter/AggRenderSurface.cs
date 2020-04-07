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
//
namespace PixelFarm.CpuBlit
{
    public sealed partial class AggRenderSurface
    {
        MemBitmap _destBmp;
      
        readonly ScanlinePacked8 _sclinePack8;
        readonly ScanlineRasterizer _sclineRas;

        MyBitmapBlender _destBitmapBlender; 
        PixelBlenderBGRA _pixelBlenderBGRA;
        DestBitmapRasterizer _bmpRasterizer;

        double _ox; //canvas origin x
        double _oy; //canvas origin y
        int _destWidth;
        int _destHeight;

        internal event EventHandler DstBitmapAttached;
        internal event EventHandler DstBitmapDetached;

        public AggRenderSurface()
        {
            //1. attach dst bmp before use this
            //2. you can detach this surface and attach to another bmp surface

            _pixelBlenderBGRA = new PixelBlenderBGRA();
            _destBitmapBlender = new MyBitmapBlender();

            _bmpRasterizer = new DestBitmapRasterizer();
            _sclinePack8 = new ScanlinePacked8();
            _sclineRas = new ScanlineRasterizer();

            _currentImgSpanGen = _imgSpanGenBilinearClip;
            CurrentTransformMatrix = Affine.IdentityMatrix;
        }
        public void SetCustomPixelBlender(PixelBlender32 customPixelBlender)
        {
            _destBitmapBlender.OutputPixelBlender = (customPixelBlender != null) ? customPixelBlender : _pixelBlenderBGRA;
        }
        public void SetGamma(PrebuiltGammaTable prebuiltGammaTable)
        {
            _sclineRas.SetGammaLut(prebuiltGammaTable);
        }

        public void AttachDstBitmap(MemBitmap dstBmp)
        {
            _destBmp = dstBmp;
            _destBitmapBlender.Attach(dstBmp, _pixelBlenderBGRA);
            _sclineRas.SetClipBox(
               new Q1Rect(0, 0,
               _destWidth = dstBmp.Width, //**
               _destHeight = dstBmp.Height) //**
            );

            DstBitmapAttached?.Invoke(this, EventArgs.Empty);
        }
        public void DetachDstBitmap()
        {
            _destBmp = null;
            _destBitmapBlender.Detach();
            _destWidth = _destHeight = 0;

            DstBitmapDetached?.Invoke(this, EventArgs.Empty);
        }
        //
        public int Width => _destWidth;
        public int Height => _destHeight;
        public MemBitmap DestBitmap => _destBmp;

        //low-level Agg infrastructures
        public BitmapBlenderBase DestBitmapBlender => _destBitmapBlender;
        public ScanlineRasterizer ScanlineRasterizer => _sclineRas;
        public ScanlinePacked8 ScanlinePacked8 => _sclinePack8;
        public DestBitmapRasterizer BitmapRasterizer => _bmpRasterizer;
        public FillingRule FillingRule
        {
            get => _sclineRas.ScanlineFillingRule;
            set => _sclineRas.ScanlineFillingRule = value;
        }

        public float ScanlineRasOriginX => _sclineRas.OffsetOriginX;
        public float ScanlineRasOriginY => _sclineRas.OffsetOriginY;
        //
        // 
        public PixelBlender32 PixelBlender
        {
            get => _destBitmapBlender.OutputPixelBlender;
            set => _destBitmapBlender.OutputPixelBlender = value;
        }
        public ICoordTransformer CurrentTransformMatrix { get; set; }
        //
        public Q1Rect GetClippingRect() => ScanlineRasterizer.GetVectorClipBox();
        public void SetClippingRect(Q1Rect rect)
        {
            rect.IntersectWithRectangle(new Q1Rect(0, 0, this.Width, this.Height));
            ScanlineRasterizer.SetClipBox(rect);
        }
        public PixelFarm.CpuBlit.FragmentProcessing.ImgSpanGen CustomImgSpanGen
        {
            get => _imgSpanGenCustom;
            set
            {
                _imgSpanGenCustom = value;
                if (value == null)
                {
                    //switch to default
                    _currentImgSpanGen = _imgSpanGenBilinearClip;
                }
                else
                {
                    _currentImgSpanGen = value;
                }
            }
        }

        public ImageInterpolationQuality ImageInterpolationQuality { get; set; }

        public void Clear(Color color)
        {

            BitmapBlenderBase destImage = this.DestBitmapBlender;
#if DEBUG
            if (destImage.BitDepth != 32) throw new NotSupportedException();
#endif

            MemBitmapExtensions.Clear(destImage.GetBufferPtr(), color);

            //unsafe
            //{                
            //    int* buffer = (int*)tmp.Ptr;
            //    int len32 = tmp.LengthInBytes / 4;

            //    switch (destImage.BitDepth)
            //    {
            //        default: throw new NotSupportedException();
            //        case 32:
            //            {
            //                //------------------------------
            //                //fast clear buffer
            //                //skip clipping ****
            //                //TODO: reimplement clipping***
            //                //------------------------------ 
            //                if (color == Color.White)
            //                {
            //                    //fast cleat with white color
            //                    int n = len32;
            //                    unsafe
            //                    {
            //                        //fixed (void* head = &buffer[0])
            //                        {
            //                            uint* head_i32 = (uint*)buffer;
            //                            for (int i = n - 1; i >= 0; --i)
            //                            {
            //                                *head_i32 = 0xffffffff; //white (ARGB)
            //                                head_i32++;
            //                            }
            //                        }
            //                    }
            //                }
            //                else if (color == Color.Black)
            //                {
            //                    //fast clear with black color
            //                    int n = len32;
            //                    unsafe
            //                    {
            //                        //fixed (void* head = &buffer[0])
            //                        {
            //                            uint* head_i32 = (uint*)buffer;
            //                            for (int i = n - 1; i >= 0; --i)
            //                            {
            //                                *head_i32 = 0xff000000; //black (ARGB)
            //                                head_i32++;
            //                            }
            //                        }
            //                    }
            //                }
            //                else if (color == Color.Empty)
            //                {
            //                    int n = len32;
            //                    unsafe
            //                    {
            //                        //fixed (void* head = &buffer[0])
            //                        {
            //                            uint* head_i32 = (uint*)buffer;
            //                            for (int i = n - 1; i >= 0; --i)
            //                            {
            //                                *head_i32 = 0x00000000; //empty
            //                                head_i32++;
            //                            }
            //                        }
            //                    }
            //                }
            //                else
            //                {
            //                    //other color
            //                    //#if WIN32
            //                    //                            uint colorARGB = (uint)((color.alpha << 24) | ((color.red << 16) | (color.green << 8) | color.blue));
            //                    //#else
            //                    //                            uint colorARGB = (uint)((color.alpha << 24) | ((color.blue << 16) | (color.green << 8) | color.red));
            //                    //#endif

            //                    //ARGB
            //                    uint colorARGB = (uint)((color.alpha << CO.A_SHIFT) | ((color.red << CO.R_SHIFT) | (color.green << CO.G_SHIFT) | color.blue << CO.B_SHIFT));
            //                    int n = len32;
            //                    unsafe
            //                    {
            //                        //fixed (void* head = &buffer[0])
            //                        {
            //                            uint* head_i32 = (uint*)buffer;
            //                            for (int i = n - 1; i >= 0; --i)
            //                            {
            //                                *head_i32 = colorARGB;
            //                                head_i32++;
            //                            }
            //                        }
            //                    }
            //                }
            //            }
            //            break;
            //    }
            //}

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
            ICoordTransformer transform = this.CurrentTransformMatrix;
            if (!transform.IsIdentity)
            {
                _sclineRas.AddPath(vxs, transform);
            }
            else
            {
                _sclineRas.AddPath(vxs);
            }
            _bmpRasterizer.RenderWithColor(_destBitmapBlender, _sclineRas, _sclinePack8, c);
            unchecked { _destImageChanged++; };
            //-----------------------------
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
                return _sclineRas.ExtendWidthX3ForSubPixelLcdEffect;
            }
            set
            {
                if (value)
                {
                    //TODO: review here again             
                    _sclineRas.ExtendWidthX3ForSubPixelLcdEffect = true;
                    _bmpRasterizer.ScanlineRenderMode = ScanlineRenderMode.SubPixelLcdEffect;
                }
                else
                {
                    _sclineRas.ExtendWidthX3ForSubPixelLcdEffect = false;
                    _bmpRasterizer.ScanlineRenderMode = ScanlineRenderMode.Default;
                }
            }
        }
#if DEBUG
        VertexStore dbug_v1 = new VertexStore();
        VertexStore dbug_v2 = new VertexStore();
        Stroke dbugStroke = new Stroke(1);
        public string dbugName;
        public void dbugLine(double x1, double y1, double x2, double y2, Drawing.Color color)
        {

            dbugStroke.Width = 1;
            dbug_v1.AddMoveTo(x1, y1);
            dbug_v1.AddLineTo(x2, y2);
            //dbug_v1.AddStop();

            dbugStroke.MakeVxs(dbug_v1, dbug_v2);
            Render(dbug_v2, color);
            dbug_v1.Clear();
            dbug_v2.Clear();
        }
        public override string ToString()
        {
            return dbugName ?? "";
        }
#endif

    }



    partial class AggRenderSurface
    {



        class MyBitmapBlender : BitmapBlenderBase
        {
            MemBitmap _bmp;
            public MyBitmapBlender()
            {
            }
            protected override void OnAttachingDstBitmap(MemBitmap bmp)
            {
                _bmp = bmp;
            }
            public override void WriteBuffer(int[] newbuffer)
            {
                MemBitmap.ReplaceBuffer(_bmp, newbuffer);
            }
        }
    }
}
