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




    public class ImgFilterSvgFeColorMatrix : CpuBlitImgFilter
    {
        //https://developer.mozilla.org/en-US/docs/Web/SVG/Element/feColorMatrix
        //| R' |     | a00 a01 a02 a03 a04 |   | R |
        //| G' |     | a10 a11 a12 a13 a14 |   | G |
        //| B' |  =  | a20 a21 a22 a23 a24 | * | B |
        //| A' |     | a30 a31 a32 a33 a34 |   | A |
        //| 1  |     |  0   0   0   0   1  |   | 1 |
        //https://alistapart.com/article/finessing-fecolormatrix
        //apply filter 
        public float[] Elements { get; set; }
        public override void Apply()
        {
            unsafe
            {
                using (TempMemPtr bufferPtr = _target.GetBufferPtr())
                {
                     
                    byte* srcBuffer = (byte*)bufferPtr.Ptr;
                    int* srcBuffer1 = (int*)srcBuffer;
                    
                    int stride = _target.Stride;
                    int w = _target.Width;
                    int h = _target.Height;

                    float[] elems = Elements;

                    for (int y = 0; y < h; ++y)
                    {
                        for (int x = 0; x < w; ++x)
                        {
                            int src = *srcBuffer1;

                            float r = ((src >> CO.R_SHIFT) & 0xFF) / 255f;
                            float g = ((src >> CO.G_SHIFT) & 0xFF) / 255f;
                            float b = ((src >> CO.B_SHIFT) & 0xFF) / 255f;
                            float a = ((src >> CO.A_SHIFT) & 0xFF) / 255f;

                            float newR = r * elems[0] + g * elems[1] + b * elems[2] + a * elems[3] + 1 * elems[4];
                            float newG = r * elems[5] + g * elems[6] + b * elems[7] + a * elems[8] + 1 * elems[9];
                            float newB = r * elems[10] + g * elems[11] + b * elems[12] + a * elems[13] + 1 * elems[14];
                            float newA = r * elems[15] + g * elems[16] + b * elems[17] + a * elems[18] + 1 * elems[19];

                            *srcBuffer1 = ((byte)(newR * 255) << CO.R_SHIFT) |
                                   ((byte)(newG * 255) << CO.G_SHIFT) |
                                   ((byte)(newB * 255) << CO.B_SHIFT) |
                                   ((byte)(newA * 255) << CO.A_SHIFT);
                            srcBuffer1++;
                        }
                    }
                   
                }
            }
        }
    }
}