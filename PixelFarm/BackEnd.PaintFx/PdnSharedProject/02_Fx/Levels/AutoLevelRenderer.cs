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
    public class AutoLevelRenderer : EffectRendererBase
    {
        private UnaryPixelOps.Level levels = null;
        public void SetParameters(Surface src, Rectangle rgn)
        {
            HistogramRgb histogram = new HistogramRgb();
            histogram.UpdateHistogram(src, rgn);
            this.levels = histogram.MakeLevelsAuto();
        }
        public override void Render(Surface src, Surface dst, Rectangle[] rois, int startIndex, int length)
        {
            if (this.levels.isValid)
            {
                this.levels.Apply(dst, src, rois, startIndex, length);
            }
        }

    }
}