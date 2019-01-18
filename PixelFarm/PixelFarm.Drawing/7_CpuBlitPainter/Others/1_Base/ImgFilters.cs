//MIT, 2014-present, WinterDev
using PixelFarm.Drawing;
using PixelFarm.CpuBlit.Imaging;
using System;

namespace PaintFx.Effects
{


    public interface ICpuBlitImgFilter
    {
        void SetTarget(PixelFarm.CpuBlit.PixelProcessing.BitmapBlenderBase targt);
    }
    public abstract class CpuBlitImgFilter : PixelFarm.Drawing.IImageFilter, ICpuBlitImgFilter
    {
        protected PixelFarm.CpuBlit.PixelProcessing.BitmapBlenderBase _target;
        public abstract void Apply();
        public void SetTarget(PixelFarm.CpuBlit.PixelProcessing.BitmapBlenderBase target)
        {
            _target = target;
        }
    }
    public class ImgFilterStackBlur : CpuBlitImgFilter
    {
        StackBlur _stackBlur = new StackBlur();
        public int RadiusX { get; set; } = 1;
        public int RadiusY { get; set; } = 1;
        public override void Apply()
        {
            _stackBlur.Blur(_target, RadiusX, RadiusY);
        }

    }




    public class ImgFilterSharpen : CpuBlitImgFilter
    {
        SharpenRenderer _shRenderer1 = new SharpenRenderer();
        /// <summary>
        /// pixels
        /// </summary>
        public int Radius { get; set; } = 1;
        public override void Apply()
        {
            unsafe
            {
                using (TempMemPtr bufferPtr = _target.GetBufferPtr())
                {
                    int[] output = new int[bufferPtr.LengthInBytes / 4]; //TODO: review here again

                    fixed (int* outputPtr = &output[0])
                    {
                        byte* srcBuffer = (byte*)bufferPtr.Ptr;
                        int* srcBuffer1 = (int*)srcBuffer;
                        int* outputBuffer1 = (int*)outputPtr;
                        int stride = _target.Stride;
                        int w = _target.Width;
                        int h = _target.Height;

                        MemHolder srcMemHolder = new MemHolder((IntPtr)srcBuffer1, bufferPtr.LengthInBytes / 4);//
                        Surface srcSurface = new Surface(stride, w, h, srcMemHolder);
                        //
                        MemHolder destMemHolder = new MemHolder((IntPtr)outputPtr, bufferPtr.LengthInBytes / 4);
                        Surface destSurface = new Surface(stride, w, h, destMemHolder);
                        //

                        _shRenderer1.Amount = Radius;
                        _shRenderer1.Render(srcSurface, destSurface, new PixelFarm.Drawing.Rectangle[]{
                            new PixelFarm.Drawing.Rectangle(0,0,w,h)
                        }, 0, 1);
                    }


                    _target.WriteBuffer(output);
                }
            }
        }
    }

    public class ImgFilterEmboss : CpuBlitImgFilter
    {
        EmbossRenderer _emboss = new EmbossRenderer();

        public ImgFilterEmboss()
        {
            Angle = 30;
        }
        /// <summary>
        /// angle in degree
        /// </summary>
        public float Angle
        {
            get => (float)_emboss.Angle;
            set => _emboss.SetParameters(value);
        }
        public override void Apply()
        {
            unsafe
            {

                using (TempMemPtr bufferPtr = _target.GetBufferPtr())
                {
                    int[] output = new int[bufferPtr.LengthInBytes / 4]; //TODO: review here again

                    fixed (int* outputPtr = &output[0])
                    {
                        byte* srcBuffer = (byte*)bufferPtr.Ptr;
                        int* srcBuffer1 = (int*)srcBuffer;
                        int* outputBuffer1 = (int*)outputPtr;
                        int stride = _target.Stride;
                        int w = _target.Width;
                        int h = _target.Height;

                        MemHolder srcMemHolder = new MemHolder((IntPtr)srcBuffer1, bufferPtr.LengthInBytes / 4);//
                        Surface srcSurface = new Surface(stride, w, h, srcMemHolder);
                        //
                        MemHolder destMemHolder = new MemHolder((IntPtr)outputPtr, bufferPtr.LengthInBytes / 4);
                        Surface destSurface = new Surface(stride, w, h, destMemHolder);
                        //

                        _emboss.Render(srcSurface, destSurface, new PixelFarm.Drawing.Rectangle[]{
                            new PixelFarm.Drawing.Rectangle(0,0,w,h)
                        }, 0, 1);
                    }

                    _target.WriteBuffer(output);
                }
            }
        }
    }

    public class ImgFilterEdgeDetection : CpuBlitImgFilter
    {
        EdgeDetectRenderer _edge = new EdgeDetectRenderer();
        public float Angle
        {
            get => (float)_edge.Angle;
            set
            {
                _edge.SetAngle(value);
            }
        }
        public override void Apply()
        {
            unsafe
            {

                _edge.SetAngle(Angle);

                using (TempMemPtr bufferPtr = _target.GetBufferPtr())
                {
                    int[] output = new int[bufferPtr.LengthInBytes / 4]; //TODO: review here again

                    fixed (int* outputPtr = &output[0])
                    {
                        byte* srcBuffer = (byte*)bufferPtr.Ptr;
                        int* srcBuffer1 = (int*)srcBuffer;
                        int* outputBuffer1 = (int*)outputPtr;
                        int stride = _target.Stride;
                        int w = _target.Width;
                        int h = _target.Height;

                        MemHolder srcMemHolder = new MemHolder((IntPtr)srcBuffer1, bufferPtr.LengthInBytes / 4);//
                        Surface srcSurface = new Surface(stride, w, h, srcMemHolder);
                        //
                        MemHolder destMemHolder = new MemHolder((IntPtr)outputPtr, bufferPtr.LengthInBytes / 4);
                        Surface destSurface = new Surface(stride, w, h, destMemHolder);
                        //
                        _edge.Render(
                            new RenderArgs(destSurface), //*** dest
                            new RenderArgs(srcSurface), //** src
                            new PixelFarm.Drawing.Rectangle[]{
                            new PixelFarm.Drawing.Rectangle(0,0,w,h)
                        }, 0, 1);
                    }

                    //ActualImage.SaveImgBufferToPngFile(output, img.Stride, img.Width + 1, img.Height + 1, "d:\\WImageTest\\test_1.png");
                    _target.WriteBuffer(output);
                }
            }
        }

    }

    public class ImgFilterOilPaint : CpuBlitImgFilter
    {
        OilPaintRenderer _oilPaint = new OilPaintRenderer();
        public ImgFilterOilPaint()
        {
            _oilPaint.SetParameters(3, 50);//example
        }
        public override void Apply()
        {
            unsafe
            {
                using (TempMemPtr bufferPtr = _target.GetBufferPtr())
                {
                    int[] output = new int[bufferPtr.LengthInBytes / 4]; //TODO: review here again

                    fixed (int* outputPtr = &output[0])
                    {
                        byte* srcBuffer = (byte*)bufferPtr.Ptr;
                        int* srcBuffer1 = (int*)srcBuffer;
                        int* outputBuffer1 = (int*)outputPtr;
                        int stride = _target.Stride;
                        int w = _target.Width;
                        int h = _target.Height;

                        MemHolder srcMemHolder = new MemHolder((IntPtr)srcBuffer1, bufferPtr.LengthInBytes / 4);//
                        Surface srcSurface = new Surface(stride, w, h, srcMemHolder);
                        //
                        MemHolder destMemHolder = new MemHolder((IntPtr)outputPtr, bufferPtr.LengthInBytes / 4);
                        Surface destSurface = new Surface(stride, w, h, destMemHolder);
                        // 

                        _oilPaint.Render(srcSurface, destSurface, new PixelFarm.Drawing.Rectangle[]{
                            new PixelFarm.Drawing.Rectangle(0,0,w,h)
                        }, 0, 1);
                    }


                    _target.WriteBuffer(output);
                }
            }
        }
    }



    public class ImgFilterPencilSketch : CpuBlitImgFilter
    {
        PencilSketchRenderer _pencilSketch = new PencilSketchRenderer();
        public ImgFilterPencilSketch()
        {

        }
        public override void Apply()
        {
            unsafe
            {
                using (TempMemPtr bufferPtr = _target.GetBufferPtr())
                {
                    int[] output = new int[bufferPtr.LengthInBytes / 4]; //TODO: review here again

                    fixed (int* outputPtr = &output[0])
                    {
                        byte* srcBuffer = (byte*)bufferPtr.Ptr;
                        int* srcBuffer1 = (int*)srcBuffer;
                        int* outputBuffer1 = (int*)outputPtr;
                        int stride = _target.Stride;
                        int w = _target.Width;
                        int h = _target.Height;

                        MemHolder srcMemHolder = new MemHolder((IntPtr)srcBuffer1, bufferPtr.LengthInBytes / 4);//
                        Surface srcSurface = new Surface(stride, w, h, srcMemHolder);
                        //
                        MemHolder destMemHolder = new MemHolder((IntPtr)outputPtr, bufferPtr.LengthInBytes / 4);
                        Surface destSurface = new Surface(stride, w, h, destMemHolder);
                        // 

                        _pencilSketch.Render(srcSurface, destSurface, new PixelFarm.Drawing.Rectangle[]{
                            new PixelFarm.Drawing.Rectangle(0,0,w,h)
                        }, 0, 1);
                    }
                    _target.WriteBuffer(output);
                }
            }
        }
    }



    public class ImgFilterAutoLevel : CpuBlitImgFilter
    {
        AutoLevelRenderer _autoLevelRenderer = new AutoLevelRenderer();
        public ImgFilterAutoLevel()
        {

        } 
        public override void Apply()
        {
            unsafe
            {
                using (TempMemPtr bufferPtr = _target.GetBufferPtr())
                {
                    int[] output = new int[bufferPtr.LengthInBytes / 4]; //TODO: review here again

                    fixed (int* outputPtr = &output[0])
                    {
                        byte* srcBuffer = (byte*)bufferPtr.Ptr;
                        int* srcBuffer1 = (int*)srcBuffer;
                        int* outputBuffer1 = (int*)outputPtr;
                        int stride = _target.Stride;
                        int w = _target.Width;
                        int h = _target.Height;

                        MemHolder srcMemHolder = new MemHolder((IntPtr)srcBuffer1, bufferPtr.LengthInBytes / 4);//
                        Surface srcSurface = new Surface(stride, w, h, srcMemHolder);
                        //
                        MemHolder destMemHolder = new MemHolder((IntPtr)outputPtr, bufferPtr.LengthInBytes / 4);
                        Surface destSurface = new Surface(stride, w, h, destMemHolder);
                        // 
                        _autoLevelRenderer.SetParameters(srcSurface, new PixelFarm.Drawing.Rectangle(0, 0, w, h));
                        _autoLevelRenderer.Render(srcSurface, destSurface, new PixelFarm.Drawing.Rectangle[]{
                            new PixelFarm.Drawing.Rectangle(0,0,w,h)
                        }, 0, 1);
                    }
                    _target.WriteBuffer(output);
                }
            }
        }
    }




    //public class ImgFilterRecursiveBlur : CpuBlitImgFilter
    //{
    //    RecursiveBlur m_recursive_blur;
    //    /// <summary>
    //    /// pixels
    //    /// </summary>
    //    public double Radius { get; set; } = 1;
    //    public override void Apply()
    //    {
    //        if (m_recursive_blur == null) m_recursive_blur = new RecursiveBlur(new RecursiveBlurCalcRGB());
    //        //----------
    //        m_recursive_blur.Blur(_target, Radius);
    //    }

    //}
}