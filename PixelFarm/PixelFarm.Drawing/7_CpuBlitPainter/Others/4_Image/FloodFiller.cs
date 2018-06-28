//BSD, 2014-present, WinterDev

using PixelFarm.Drawing; 
namespace PixelFarm.CpuBlit.Imaging
{
    public class FloodFill
    {
        abstract class FillingRule
        {
            protected Color startColor;
            protected Color fillColor;
            protected FillingRule(Color fillColor)
            {
                this.fillColor = fillColor;
            }

            public void SetStartColor(Color startColor)
            {
                this.startColor = startColor;
            }

            public unsafe void SetPixel(int* dest)
            {
                //*dest = (fillColor.red << 16) | (fillColor.green << 8) | (fillColor.blue);
                *dest = (fillColor.blue << 16) | (fillColor.green << 8) | (fillColor.red);
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

                return r == startColor.red &&
                       g == startColor.green &&
                       b == startColor.blue;

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


                return (r >= (startColor.red - tolerance0To255)) && (r <= (startColor.red + tolerance0To255)) &&
                       (g >= (startColor.green - tolerance0To255)) && (r <= (startColor.green + tolerance0To255)) &&
                       (b >= (startColor.blue - tolerance0To255)) && (r <= (startColor.blue + tolerance0To255));


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


        int imageWidth;
        int imageHeight;


        protected bool[] pixelsChecked;
        FillingRule fillRule;
        Queue<Range> ranges = new Queue<Range>(9);


        public FloodFill(Color fillColor)
        {
            fillRule = new ExactMatch(fillColor);
        }

        public FloodFill(Color fillColor, int tolerance0To255)
        {
            if (tolerance0To255 > 0)
            {
                fillRule = new ToleranceMatch(fillColor, tolerance0To255);
            }
            else
            {
                fillRule = new ExactMatch(fillColor);
            }
        }
        public void Fill(ActualBitmap img, int x, int y)
        {
            Fill((IBitmapSrc)img, x, y);
        }


        IBitmapSrc _destImgRW;

        public void Fill(IBitmapSrc bufferToFillOn, int x, int y)
        {
            y -= imageHeight;
            unchecked // this way we can overflow the uint on negative and get a big number
            {
                if ((uint)x >= bufferToFillOn.Width || (uint)y >= bufferToFillOn.Height)
                {
                    return;
                }
            }
            _destImgRW = bufferToFillOn;
            TempMemPtr destBufferPtr = bufferToFillOn.GetBufferPtr();
            unsafe
            {
                imageWidth = bufferToFillOn.Width;
                imageHeight = bufferToFillOn.Height;
                //reset new buffer, clear mem?
                pixelsChecked = new bool[imageWidth * imageHeight];

                int* destBuffer = (int*)destBufferPtr.Ptr;
                int startColorBufferOffset = bufferToFillOn.GetBufferOffsetXY32(x, y);

                int start_color = *(destBuffer + startColorBufferOffset);

                fillRule.SetStartColor(Drawing.Color.FromArgb(
                    (start_color >> 16) & 0xff,
                    (start_color >> 8) & 0xff,
                    (start_color) & 0xff));


                LinearFill(destBuffer, x, y);

                while (ranges.Count > 0)
                {
                    Range range = ranges.Dequeue();
                    int downY = range.y - 1;
                    int upY = range.y + 1;
                    int downPixelOffset = (imageWidth * (range.y - 1)) + range.startX;
                    int upPixelOffset = (imageWidth * (range.y + 1)) + range.startX;
                    for (int rangeX = range.startX; rangeX <= range.endX; rangeX++)
                    {
                        if (range.y > 0)
                        {
                            if (!pixelsChecked[downPixelOffset])
                            {
                                int bufferOffset = bufferToFillOn.GetBufferOffsetXY32(rangeX, downY);

                                if (fillRule.CheckPixel(*(destBuffer + bufferOffset)))
                                {
                                    LinearFill(destBuffer, rangeX, downY);
                                }
                            }
                        }

                        if (range.y < (imageHeight - 1))
                        {
                            if (!pixelsChecked[upPixelOffset])
                            {
                                int bufferOffset = bufferToFillOn.GetBufferOffsetXY32(rangeX, upY);
                                if (fillRule.CheckPixel(*(destBuffer + bufferOffset)))
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
            destBufferPtr.Release();
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
            int pixelOffset = (imageWidth * y) + x;
            while (true)
            {
                fillRule.SetPixel(destBuffer + bufferOffset);
                pixelsChecked[pixelOffset] = true;
                leftFillX--;
                pixelOffset--;
                bufferOffset--;
                if (leftFillX <= 0 || (pixelsChecked[pixelOffset]) || !fillRule.CheckPixel(*(destBuffer + bufferOffset)))
                {
                    break;
                }
            }
            leftFillX++;
            //
            int rightFillX = x;
            bufferOffset = _destImgRW.GetBufferOffsetXY32(x, y);
            pixelOffset = (imageWidth * y) + x;
            while (true)
            {
                fillRule.SetPixel(destBuffer + bufferOffset);
                pixelsChecked[pixelOffset] = true;
                rightFillX++;
                pixelOffset++;
                bufferOffset++;
                if (rightFillX >= imageWidth || pixelsChecked[pixelOffset] || !fillRule.CheckPixel(*(destBuffer + bufferOffset)))
                {
                    break;
                }
            }
            rightFillX--;
            ranges.Enqueue(new Range(leftFillX, rightFillX, y));
        }
    }
}
