/////////////////////////////////////////////////////////////////////////////////
// Paint.NET (MIT,from version 3.36.7, see=> https://github.com/rivy/OpenPDN   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////
//MIT, 2017-present, WinterDev

using PixelFarm.Drawing;

namespace PaintFx.Effects
{

    public class UnfocusRenderer : HistogramRenderer
    {
        public int Radius { get; set; }
        public unsafe override ColorBgra ApplyWithAlpha(ColorBgra src, int area, int sum, int* hb, int* hg, int* hr)
        {
            //each slot of the histgram can contain up to area * 255. This will overflow an int when area > 32k
            if (area < 32768)
            {
                int b = 0;
                int g = 0;
                int r = 0;

                for (int i = 1; i < 256; ++i)
                {
                    b += i * hb[i];
                    g += i * hg[i];
                    r += i * hr[i];
                }

                int alpha = sum / area;
                int div = area * 255;

                return ColorBgra.FromBgraClamped(b / div, g / div, r / div, alpha);
            }
            else //use a long if an int will overflow.
            {
                long b = 0;
                long g = 0;
                long r = 0;

                for (long i = 1; i < 256; ++i)
                {
                    b += i * hb[i];
                    g += i * hg[i];
                    r += i * hr[i];
                }

                int alpha = sum / area;
                int div = area * 255;

                return ColorBgra.FromBgraClamped(b / div, g / div, r / div, alpha);
            }
        }

        public override void Render(Surface src, Surface dest, Rectangle[] rois, int startIndex, int length)
        {
            foreach (Rectangle rect in rois)
            {
                RenderRectWithAlpha(this.Radius, src, dest, rect);
            }
        }
    }
}