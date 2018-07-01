/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////
//Apache2, 2017-present, WinterDev
using PixelFarm.Drawing;

namespace PaintFx
{
    /// <summary>
    /// Histogram is used to calculate a histogram for a surface (in a selection,
    /// if desired). This can then be used to retrieve percentile, average, peak,
    /// and distribution information.
    /// </summary>
    public sealed class HistogramLuminosity
        : Histogram
    {
        public HistogramLuminosity()
            : base(1, 256)
        {
            this.visualColors = new ColorBgra[] { ColorBgra.Black };
        }

        public override ColorBgra GetMeanColor()
        {
            float[] mean = GetMean();
            return ColorBgra.FromBgr((byte)(mean[0] + 0.5f), (byte)(mean[0] + 0.5f), (byte)(mean[0] + 0.5f));
        }

        public override ColorBgra GetPercentileColor(float fraction)
        {
            int[] perc = GetPercentile(fraction);
            return ColorBgra.FromBgr((byte)(perc[0]), (byte)(perc[0]), (byte)(perc[0]));
        }

        protected override unsafe void AddSurfaceRectangleToHistogram(Surface surface, Rectangle rect)
        {
            long[] histogramLuminosity = histogram[0];

            for (int y = rect.Top; y < rect.Bottom; ++y)
            {

                ColorBgra* ptr = surface.GetPointAddressUnchecked(rect.Left, y);
                for (int x = rect.Left; x < rect.Right; ++x)
                {
                    ++histogramLuminosity[ptr->GetIntensityByte()];
                    ++ptr;
                }
            }
        }

        public UnaryPixelOps.Level MakeLevelsAuto()
        {
            ColorBgra lo, md, hi;

            lo = GetPercentileColor(0.005f);
            md = GetMeanColor();
            hi = GetPercentileColor(0.995f);

            return UnaryPixelOps.Level.AutoFromLoMdHi(lo, md, hi);
        }
    }
}
