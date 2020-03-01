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
    public class AutoLevelRenderer : EffectRendererBase
    {
        UnaryPixelOps.Level _levels = null;
        public void SetParameters(Surface src, Rectangle rgn)
        {
            HistogramRgb histogram = new HistogramRgb();
            histogram.UpdateHistogram(src, rgn);
            _levels = histogram.MakeLevelsAuto();
        }
        public override void Render(Surface src, Surface dst, Rectangle[] rois, int startIndex, int length)
        {
            if (_levels.isValid)
            {
                _levels.Apply(dst, src, rois, startIndex, length);
            }
        }

    }
}