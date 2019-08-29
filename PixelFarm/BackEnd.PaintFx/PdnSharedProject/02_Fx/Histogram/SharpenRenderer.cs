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
    public class SharpenRenderer : HistogramRenderer
    {

        public int Amount { get; set; }
        public override void Render(Surface src, Surface dest, Rectangle[] rois, int startIndex, int length)
        {
            foreach (Rectangle rect in rois)
            {
                RenderRect(Amount, src, dest, rect);
            }
        }
        public unsafe override ColorBgra Apply(ColorBgra src, int area, int* hb, int* hg, int* hr, int* ha)
        {
            ColorBgra median = GetPercentile(50, area, hb, hg, hr, ha);
            return ColorBgra.Lerp(src, median, -0.5f);
        }
    }
}