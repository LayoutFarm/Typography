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
namespace PaintFx.Effects
{

    public class ReduceNoiseRenderer : HistogramRenderer
    {

        public int Radius { get; set; }
        public double Strength { get; set; }
        public override unsafe ColorBgra Apply(ColorBgra color, int area, int* hb, int* hg, int* hr, int* ha)
        {
            ColorBgra normalized = GetPercentileOfColor(color, area, hb, hg, hr, ha);
            double lerp = Strength * (1 - 0.75 * color.GetIntensity());

            return ColorBgra.Lerp(color, normalized, lerp);
        }

        static unsafe ColorBgra GetPercentileOfColor(ColorBgra color, int area, int* hb, int* hg, int* hr, int* ha)
        {
            int rc = 0;
            int gc = 0;
            int bc = 0;

            for (int i = 0; i < color.R; ++i)
            {
                rc += hr[i];
            }

            for (int i = 0; i < color.G; ++i)
            {
                gc += hg[i];
            }

            for (int i = 0; i < color.B; ++i)
            {
                bc += hb[i];
            }

            rc = (rc * 255) / area;
            gc = (gc * 255) / area;
            bc = (bc * 255) / area;

            return ColorBgra.FromBgr((byte)bc, (byte)gc, (byte)rc);
        }

        public override void Render(Surface src, Surface dest, Rectangle[] renderRects, int startIndex, int length)
        {
            for (int i = startIndex; i < startIndex + length; ++i)
            {
                RenderRect(Radius, src, dest, renderRects[i]);
            }
        }
    }
}