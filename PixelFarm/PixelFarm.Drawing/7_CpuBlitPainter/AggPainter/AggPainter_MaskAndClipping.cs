//MIT, 2016-present, WinterDev

using System;
using PixelFarm.Drawing;
using PixelFarm.CpuBlit.Imaging;
using PixelFarm.CpuBlit.PixelProcessing;

using BitmapBufferEx;
namespace PixelFarm.CpuBlit
{

    partial class AggPainter
    {
        AggRenderSurface _aggsx_mask;

        TargetBufferName _targetBufferName;
        bool _enableBuiltInMaskComposite;
        MemBitmap _alphaBitmap;

        PixelBlender32 _defaultPixelBlender;
        PixelBlenderWithMask _maskPixelBlender;
        PixelBlenderPerColorComponentWithMask _maskPixelBlenderPerCompo;
        ClipingTechnique _currentClipTech;

        /// <summary>
        /// we DO NOT store vxs
        /// </summary>
        /// <param name="vxs"></param>
        public override void SetClipRgn(VertexStore vxs)
        {
            //clip rgn implementation
            //this version replace only
            //TODO: add append clip rgn 
            if (vxs != null)
            {
                if (SimpleRectClipEvaluator.EvaluateRectClip(vxs, out RectangleF clipRect))
                {

                    this.SetClipBox(
                        (int)Math.Floor(clipRect.X), (int)Math.Floor(clipRect.Y),
                        (int)Math.Ceiling(clipRect.Right), (int)Math.Ceiling(clipRect.Bottom));

                    _currentClipTech = ClipingTechnique.ClipSimpleRect;
                }
                else
                {
                    //not simple rect => 
                    //use mask technique

                    _currentClipTech = ClipingTechnique.ClipMask;
                    //1. switch to mask buffer
                    this.TargetBufferName = TargetBufferName.AlphaMask;
                    //2.
                    Color prevColor = this.FillColor; //save

                    this.FillColor = Color.White; 
                    _aggsx.Render(vxs, FillColor);

                    //this.Fill(vxs); //fill vxs with white color (on black bg)

                    this.FillColor = prevColor; //restore
                    //3. switch back to default layer
                    this.TargetBufferName = TargetBufferName.Default;//swicth to default buffer
                    this.EnableBuiltInMaskComposite = true;
                }
            }
            else
            {
                //remove clip rgn if exists**
                switch (_currentClipTech)
                {
                    case ClipingTechnique.ClipMask:
                        this.EnableBuiltInMaskComposite = false;
                        this.TargetBufferName = TargetBufferName.AlphaMask;//swicth to mask buffer
                        this.Clear(Color.Black);
                        this.TargetBufferName = TargetBufferName.Default;

                        break;
                    case ClipingTechnique.ClipSimpleRect:

                        this.SetClipBox(0, 0, this.Width, this.Height);
                        break;
                }

                _currentClipTech = ClipingTechnique.None;
            }
        }

        public override RectInt ClipBox
        {
            get => _aggsx.GetClippingRect();
            set => _aggsx.SetClippingRect(value);
        }
        public override void SetClipBox(int x1, int y1, int x2, int y2)
        {
            _aggsx.SetClippingRect(new RectInt(x1, y1, x2, y2));
        }
        //---------------------------------------------------------------

        void SetupMaskPixelBlender()
        {
            if (_aggsx_mask != null)
            {
                //also set the canvas origin for the aggsx_mask
                _aggsx_mask.SetScanlineRasOrigin(this.OriginX, this.OriginY);
                return;
            }
            //----------
            //same size                  

            _alphaBitmap = new MemBitmap(_aggsx_0.Width, _aggsx_0.Height);
#if DEBUG
            _alphaBitmap._dbugNote = "AggPrinter.SetupMaskPixelBlender";
#endif

            _aggsx_mask = new AggRenderSurface(_alphaBitmap) { PixelBlender = new PixelBlenderBGRA() };
            _aggsx_mask.SetScanlineRasOrigin(this.OriginX, this.OriginY); //also set the canvas origin for the aggsx_mask

            _maskPixelBlender = new PixelBlenderWithMask();
            _maskPixelBlenderPerCompo = new PixelBlenderPerColorComponentWithMask();

            _maskPixelBlender.SetMaskBitmap(_alphaBitmap); //same alpha bitmap
            _maskPixelBlenderPerCompo.SetMaskBitmap(_alphaBitmap); //same alpha bitmap
        }
        public TargetBufferName TargetBufferName
        {
            get => _targetBufferName;
            set
            {
                //change or not
                if (_targetBufferName != value)
                {
                    switch (value)
                    {
                        default: throw new NotSupportedException();
                        case TargetBufferName.Default:
                            //default 
                            _aggsx = _aggsx_0; //*** 
                            break;
                        case TargetBufferName.AlphaMask:
                            SetupMaskPixelBlender();
                            _aggsx = _aggsx_mask;//*** 
                            break;
                    }


                    TempMemPtr tmp = MemBitmap.GetBufferPtr(_aggsx.DestBitmap);
                    unsafe
                    {
                        _bxt = new BitmapBuffer(
                       _aggsx.Width,
                       _aggsx.Height,
                        tmp.Ptr,
                        tmp.LengthInBytes);
                    }


                    _targetBufferName = value;
                }

            }
        }
        public bool EnableBuiltInMaskComposite
        {
            get => _enableBuiltInMaskComposite;
            set
            {
                if (_enableBuiltInMaskComposite != value)
                {
                    _enableBuiltInMaskComposite = value;
                    if (value)
                    {
                        //use mask composite
                        this.DestBitmapBlender.OutputPixelBlender = _maskPixelBlender;
                    }
                    else
                    {
                        //use default composite
                        this.DestBitmapBlender.OutputPixelBlender = _defaultPixelBlender;
                    }
                }
            }
        }

    }

    public enum TargetBufferName
    {
        Unknown,
        Default,
        AlphaMask
    }
}