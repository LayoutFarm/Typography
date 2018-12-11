//BSD, 2014-present, WinterDev

using PixelFarm.Drawing;
namespace PixelFarm.CpuBlit.Imaging
{
    public class FloodFill
    {
        int _imageWidth;
        int _imageHeight;


        protected bool[] _pixelsChecked;
        FillingRule _fillRule;
        Queue<Range> _ranges = new Queue<Range>(9);
        IBitmapSrc _destImgRW;

        abstract class FillingRule
        {
            protected Color _startColor;
            protected Color _fillColor;
            protected FillingRule(Color fillColor)
            {
                _fillColor = fillColor;
            }

            public void SetStartColor(Color startColor)
            {
                _startColor = startColor;
            }

            public unsafe void SetPixel(int* dest)
            {
                //*dest = (fillColor.red << 16) | (fillColor.green << 8) | (fillColor.blue);
                *dest = (_fillColor.blue << 16) | (_fillColor.green << 8) | (_fillColor.red);
            }

            public abstract bool CheckPixel(int pixelValue32);
        }

        sealed class ExactMatch : FillingRule
        {
            public ExactMatch(Color fillColor)
                : base(fillColor)
            {
            }

            public override bool CheckPixel(int pixelValue32)
            {
                //ARGB
                int r = ((pixelValue32 >> (PixelFarm.CpuBlit.PixelProcessing.CO.R * 8)) & 0xff);//16
                int g = ((pixelValue32 >> (PixelFarm.CpuBlit.PixelProcessing.CO.G * 8)) & 0xff);//8
                int b = ((pixelValue32 >> (PixelFarm.CpuBlit.PixelProcessing.CO.B * 8)) & 0xff);//0

                return r == _startColor.red &&
                       g == _startColor.green &&
                       b == _startColor.blue;

                //return (destBuffer[bufferOffset] == startColor.red) &&
                //    (destBuffer[bufferOffset + 1] == startColor.green) &&
                //    (destBuffer[bufferOffset + 2] == startColor.blue);
            }
        }

        sealed class ToleranceMatch : FillingRule
        {
            int tolerance0To255;
            public ToleranceMatch(Color fillColor, int tolerance0To255)
                : base(fillColor)
            {
                this.tolerance0To255 = tolerance0To255;
            }

            public override bool CheckPixel(int pixelValue32)
            {
                //ARGB
                int r = ((pixelValue32 >> (PixelFarm.CpuBlit.PixelProcessing.CO.R * 8)) & 0xff);
                int g = ((pixelValue32 >> (PixelFarm.CpuBlit.PixelProcessing.CO.G * 8)) & 0xff);
                int b = ((pixelValue32 >> (PixelFarm.CpuBlit.PixelProcessing.CO.B * 8)) & 0xff);


                return (r >= (_startColor.red - tolerance0To255)) && (r <= (_startColor.red + tolerance0To255)) &&
                       (g >= (_startColor.green - tolerance0To255)) && (r <= (_startColor.green + tolerance0To255)) &&
                       (b >= (_startColor.blue - tolerance0To255)) && (r <= (_startColor.blue + tolerance0To255));


                //return (destBuffer[bufferOffset] >= (startColor.red - tolerance0To255)) && destBuffer[bufferOffset] <= (startColor.red + tolerance0To255) &&
                //    (destBuffer[bufferOffset + 1] >= (startColor.green - tolerance0To255)) && destBuffer[bufferOffset + 1] <= (startColor.green + tolerance0To255) &&
                //    (destBuffer[bufferOffset + 2] >= (startColor.blue - tolerance0To255)) && destBuffer[bufferOffset + 2] <= (startColor.blue + tolerance0To255);
            }
        }

        struct Range
        {
            public int startX;
            public int endX;
            public int y;
            public Range(int startX, int endX, int y)
            {
                this.startX = startX;
                this.endX = endX;
                this.y = y;
            }
        }


       

        public FloodFill(Color fillColor)
        {
            _fillRule = new ExactMatch(fillColor);
        }

        public FloodFill(Color fillColor, int tolerance0To255)
        {
            if (tolerance0To255 > 0)
            {
                _fillRule = new ToleranceMatch(fillColor, tolerance0To255);
            }
            else
            {
                _fillRule = new ExactMatch(fillColor);
            }
        }
        public void Fill(MemBitmap memBmp, int x, int y)
        {
            Fill((IBitmapSrc)memBmp, x, y);
        } 
        public void Fill(IBitmapSrc bufferToFillOn, int x, int y)
        {
            y -= _imageHeight;
            unchecked // this way we can overflow the uint on negative and get a big number
            {
                if ((uint)x >= bufferToFillOn.Width || (uint)y >= bufferToFillOn.Height)
                {
                    return;
                }
            }
            _destImgRW = bufferToFillOn;

            unsafe
            {
                using (TempMemPtr destBufferPtr = bufferToFillOn.GetBufferPtr())
                {

                    _imageWidth = bufferToFillOn.Width;
                    _imageHeight = bufferToFillOn.Height;
                    //reset new buffer, clear mem?
                    _pixelsChecked = new bool[_imageWidth * _imageHeight];

                    int* destBuffer = (int*)destBufferPtr.Ptr;
                    int startColorBufferOffset = bufferToFillOn.GetBufferOffsetXY32(x, y);

                    int start_color = *(destBuffer + startColorBufferOffset);

                    _fillRule.SetStartColor(Drawing.Color.FromArgb(
                        (start_color >> 16) & 0xff,
                        (start_color >> 8) & 0xff,
                        (start_color) & 0xff));


                    LinearFill(destBuffer, x, y);

                    while (_ranges.Count > 0)
                    {
                        Range range = _ranges.Dequeue();
                        int downY = range.y - 1;
                        int upY = range.y + 1;
                        int downPixelOffset = (_imageWidth * (range.y - 1)) + range.startX;
                        int upPixelOffset = (_imageWidth * (range.y + 1)) + range.startX;
                        for (int rangeX = range.startX; rangeX <= range.endX; rangeX++)
                        {
                            if (range.y > 0)
                            {
                                if (!_pixelsChecked[downPixelOffset])
                                {
                                    int bufferOffset = bufferToFillOn.GetBufferOffsetXY32(rangeX, downY);

                                    if (_fillRule.CheckPixel(*(destBuffer + bufferOffset)))
                                    {
                                        LinearFill(destBuffer, rangeX, downY);
                                    }
                                }
                            }

                            if (range.y < (_imageHeight - 1))
                            {
                                if (!_pixelsChecked[upPixelOffset])
                                {
                                    int bufferOffset = bufferToFillOn.GetBufferOffsetXY32(rangeX, upY);
                                    if (_fillRule.CheckPixel(*(destBuffer + bufferOffset)))
                                    {
                                        LinearFill(destBuffer, rangeX, upY);
                                    }
                                }
                            }
                            upPixelOffset++;
                            downPixelOffset++;
                        }
                    }
                }
            }

        }

        /// <summary>
        /// fill to left side and right side of the line
        /// </summary>
        /// <param name="destBuffer"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        unsafe void LinearFill(int* destBuffer, int x, int y)
        {

            int leftFillX = x;
            int bufferOffset = _destImgRW.GetBufferOffsetXY32(x, y);
            int pixelOffset = (_imageWidth * y) + x;
            while (true)
            {
                _fillRule.SetPixel(destBuffer + bufferOffset);
                _pixelsChecked[pixelOffset] = true;
                leftFillX--;
                pixelOffset--;
                bufferOffset--;
                if (leftFillX <= 0 || (_pixelsChecked[pixelOffset]) || !_fillRule.CheckPixel(*(destBuffer + bufferOffset)))
                {
                    break;
                }
            }
            leftFillX++;
            //
            int rightFillX = x;
            bufferOffset = _destImgRW.GetBufferOffsetXY32(x, y);
            pixelOffset = (_imageWidth * y) + x;
            while (true)
            {
                _fillRule.SetPixel(destBuffer + bufferOffset);
                _pixelsChecked[pixelOffset] = true;
                rightFillX++;
                pixelOffset++;
                bufferOffset++;
                if (rightFillX >= _imageWidth || _pixelsChecked[pixelOffset] || !_fillRule.CheckPixel(*(destBuffer + bufferOffset)))
                {
                    break;
                }
            }
            rightFillX--;
            _ranges.Enqueue(new Range(leftFillX, rightFillX, y));
        }
    }
}
