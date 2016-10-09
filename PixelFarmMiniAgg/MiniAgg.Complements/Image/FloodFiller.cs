//BSD, 2014-2016, WinterDev

using PixelFarm.Drawing;
namespace PixelFarm.Agg
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

            public virtual void SetPixel(byte[] destBuffer, int bufferOffset)
            {
                destBuffer[bufferOffset] = fillColor.blue;
                destBuffer[bufferOffset + 1] = fillColor.green;
                destBuffer[bufferOffset + 2] = fillColor.red;
            }

            public abstract bool CheckPixel(byte[] destBuffer, int bufferOffset);
        }

        class ExactMatch : FillingRule
        {
            public ExactMatch(Color fillColor)
                : base(fillColor)
            {
            }

            public override bool CheckPixel(byte[] destBuffer, int bufferOffset)
            {
                return (destBuffer[bufferOffset] == startColor.red) &&
                    (destBuffer[bufferOffset + 1] == startColor.green) &&
                    (destBuffer[bufferOffset + 2] == startColor.blue);
            }
        }

        class ToleranceMatch : FillingRule
        {
            int tolerance0To255;
            public ToleranceMatch(Color fillColor, int tolerance0To255)
                : base(fillColor)
            {
                this.tolerance0To255 = tolerance0To255;
            }

            public override bool CheckPixel(byte[] destBuffer, int bufferOffset)
            {
                return (destBuffer[bufferOffset] >= (startColor.red - tolerance0To255)) && destBuffer[bufferOffset] <= (startColor.red + tolerance0To255) &&
                    (destBuffer[bufferOffset + 1] >= (startColor.green - tolerance0To255)) && destBuffer[bufferOffset + 1] <= (startColor.green + tolerance0To255) &&
                    (destBuffer[bufferOffset + 2] >= (startColor.blue - tolerance0To255)) && destBuffer[bufferOffset + 2] <= (startColor.blue + tolerance0To255);
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

        ImageReaderWriterBase destImage;
        protected int imageStride = 0;
        protected byte[] destBuffer = null;
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
        public void Fill(ActualImage img, int x, int y)
        {
            Fill(new MyImageReaderWriter(img), x, y);
        }
        public void Fill(ImageReaderWriterBase bufferToFillOn, int x, int y)
        {
            unchecked // this way we can overflow the uint on negative and get a big number
            {
                if ((uint)x > bufferToFillOn.Width || (uint)y > bufferToFillOn.Height)
                {
                    return;
                }
            }

            destImage = bufferToFillOn;
            imageStride = destImage.Stride;
            destBuffer = destImage.GetBuffer();
            int imageWidth = destImage.Width;
            int imageHeight = destImage.Height;
            pixelsChecked = new bool[destImage.Width * destImage.Height];
            int startColorBufferOffset = destImage.GetBufferOffsetXY(x, y);
            fillRule.SetStartColor(Drawing.Color.FromArgb(destImage.GetBuffer()[startColorBufferOffset + 2], destImage.GetBuffer()[startColorBufferOffset + 1], destImage.GetBuffer()[startColorBufferOffset]));
            LinearFill(x, y);
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
                            int bufferOffset = destImage.GetBufferOffsetXY(rangeX, downY);
                            if (fillRule.CheckPixel(destBuffer, bufferOffset))
                            {
                                LinearFill(rangeX, downY);
                            }
                        }
                    }

                    if (range.y < (imageHeight - 1))
                    {
                        if (!pixelsChecked[upPixelOffset])
                        {
                            int bufferOffset = destImage.GetBufferOffsetXY(rangeX, upY);
                            if (fillRule.CheckPixel(destBuffer, bufferOffset))
                            {
                                LinearFill(rangeX, upY);
                            }
                        }
                    }
                    upPixelOffset++;
                    downPixelOffset++;
                }
            }
        }

        void LinearFill(int x, int y)
        {
            int bytesPerPixel = destImage.BytesBetweenPixelsInclusive;
            int imageWidth = destImage.Width;
            int leftFillX = x;
            int bufferOffset = destImage.GetBufferOffsetXY(x, y);
            int pixelOffset = (imageWidth * y) + x;
            while (true)
            {
                fillRule.SetPixel(destBuffer, bufferOffset);
                pixelsChecked[pixelOffset] = true;
                leftFillX--;
                pixelOffset--;
                bufferOffset -= bytesPerPixel;
                if (leftFillX <= 0 || (pixelsChecked[pixelOffset]) || !fillRule.CheckPixel(destBuffer, bufferOffset))
                {
                    break;
                }
            }
            leftFillX++;
            int rightFillX = x;
            bufferOffset = destImage.GetBufferOffsetXY(x, y);
            pixelOffset = (imageWidth * y) + x;
            while (true)
            {
                fillRule.SetPixel(destBuffer, bufferOffset);
                pixelsChecked[pixelOffset] = true;
                rightFillX++;
                pixelOffset++;
                bufferOffset += bytesPerPixel;
                if (rightFillX >= imageWidth || pixelsChecked[pixelOffset] || !fillRule.CheckPixel(destBuffer, bufferOffset))
                {
                    break;
                }
            }
            rightFillX--;
            ranges.Enqueue(new Range(leftFillX, rightFillX, y));
        }
    }
}
