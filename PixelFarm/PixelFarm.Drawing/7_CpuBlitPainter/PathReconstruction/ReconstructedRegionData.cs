//MIT, 2019-present, WinterDev
using PixelFarm.CpuBlit;
using CO = PixelFarm.Drawing.CO;

namespace PixelFarm.PathReconstruction
{
    /// <summary>
    /// region that is created from path reconstruction
    /// </summary>
    public class ReconstructedRegionData
    {
        bool _evalBounds;
        PixelFarm.Drawing.Rectangle _cacheBounds;
        public ReconstructedRegionData(bool copyHSpans = true)
        {   
            WithHSpansTable = copyHSpans;
        }

        internal bool WithHSpansTable { get; }

        /// <summary>
        /// (must be) sorted hSpans, from reconstruction
        /// </summary>
        public HSpan[] HSpans { get; internal set; }
        /// <summary>
        /// reconstructed outline
        /// </summary>
        public RawOutline Outline { get; internal set; }

        PixelFarm.Drawing.Rectangle FindTotalRectBoundsFromHSpans()
        {
            HSpan[] hspans = HSpans;
            if (hspans == null) throw new System.NotSupportedException();


            int left = int.MaxValue;
            int right = int.MinValue;
            int top = int.MaxValue;
            int bottom = int.MinValue;

            for (int i = 0; i < hspans.Length; ++i)
            {
                HSpan span = hspans[i];
                int sp_bottom = span.y + 1;

                if (sp_bottom > bottom)
                {
                    bottom = sp_bottom;
                }

                if (span.y < top)
                {
                    top = span.y;
                }
                //

                if (span.startX < left)
                {
                    left = span.startX;
                }

                if (span.endX > right)
                {
                    right = span.endX;
                }
            }

            return new Drawing.Rectangle(left, top, right - left, bottom - top);
        }
        /// <summary>
        /// find bounds
        /// </summary>
        public PixelFarm.Drawing.Rectangle GetBounds(bool exactBoundsFromSpan = false)
        {
            if (_evalBounds) return _cacheBounds;

            if (exactBoundsFromSpan || Outline == null)
            {
                //calculate all bounds from HSpans again
                _evalBounds = true;
                return _cacheBounds = FindTotalRectBoundsFromHSpans();
            }
            //
            //find bounds...
            if (Outline != null)
            {
                //get bounds from raw-outline (near
            }
            else
            {

            }
            return new Drawing.Rectangle();
        }
        public void InvalidateOutline()
        {
            _evalBounds = false;
        }

    }


    public static class ReconstructedRegionDataExtensions
    {
        /// <summary>
        /// reconstruct regionOutline from internal region data
        /// </summary>
        /// <param name="rgnOutline"></param>
        public static void ReconstructOutline(this ReconstructedRegionData rgnData, RawOutline rgnOutline)
        {
            var outlineTracer = new OutlineTracer();
            outlineTracer.TraceOutline(rgnData, rgnOutline);
        }

        public static BitmapBasedRegion CreateBitmapBasedRegion(this ReconstructedRegionData rgnData)
        {
            return new BitmapBasedRegion(rgnData);
        }
        public static CpuBlit.MemBitmap CreateMaskBitmap(this ReconstructedRegionData rgnData,
           bool useFitBounds = true)
        {
            return CreateMaskBitmap(rgnData, Drawing.Color.Black, Drawing.Color.White, useFitBounds);
        }
        public static CpuBlit.MemBitmap CreateMaskBitmap(this ReconstructedRegionData rgnData,
            Drawing.Color solidPartColor,
            Drawing.Color holeColor,
            bool useFitBounds = true)
        {
            //1. find size of membitmap
            Drawing.Rectangle fitBounds = rgnData.GetBounds();
            //2. fit bounds or not
            int bmpW, bmpH, offsetX, offsetY;

            if (useFitBounds)
            {
                bmpW = fitBounds.Width;
                bmpH = fitBounds.Height;
                offsetX = -fitBounds.X;
                offsetY = -fitBounds.Y;
            }
            else
            {
                bmpW = fitBounds.Left + fitBounds.Right;
                bmpH = fitBounds.Top + fitBounds.Height;
                offsetX = 0;
                offsetY = 0;
            }

            //3. create mask bmp
            MemBitmap maskBmp = new MemBitmap(bmpW, bmpH);
            //4. fill mask data
            maskBmp.Clear(solidPartColor);

            int holdColorInt32 =
                   (holeColor.A << CO.A_SHIFT) |
                   (holeColor.B << CO.B_SHIFT) |
                   (holeColor.G << CO.G_SHIFT) |
                   (holeColor.R << CO.R_SHIFT);

            var memPtr = MemBitmap.GetBufferPtr(maskBmp);
            unsafe
            {
                int* buffer = (int*)memPtr.Ptr;
                //fill
                HSpan[] hspans = rgnData.HSpans;
                if (useFitBounds)
                {
                    int totalBufferLen = bmpW * bmpH;

                    for (int i = 0; i < hspans.Length; ++i)
                    {
                        HSpan span = hspans[i];
                        int len = span.endX - span.startX;

#if DEBUG
                        int offset = ((span.y + offsetY) * bmpW + (span.startX + offsetX));
                        if (offset >= totalBufferLen || offset + len > totalBufferLen)
                        {
                            throw new System.Exception("out-of-range");
                            break;
                        }
                        else if (offset < 0 || offset + len < 0)
                        {

                        }
#endif

                        int* pixAddr = buffer + ((span.y + offsetY) * bmpW + (span.startX + offsetX)); //with offsetX,offsetY

                        for (int n = len - 1; n >= 0; --n)
                        {
                            *pixAddr = holdColorInt32;
                            pixAddr++;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < hspans.Length; ++i)
                    {
                        HSpan span = hspans[i];
                        int len = span.endX - span.startX;

                        int* pixAddr = buffer + ((span.y) * bmpW + (span.startX)); //no offsetX,offsetY

                        for (int n = len - 1; n >= 0; --n)
                        {
                            *pixAddr = holdColorInt32;
                            pixAddr++;
                        }
                    }
                }
            }

            //return null;
            return maskBmp;
        }
    }

}