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
    public class GlowRenderer : EffectRendererBase
    {
        UserBlendOps.ScreenBlendOp screenBlendOp = new UserBlendOps.ScreenBlendOp();

        public void SetParameters(int radius, int brightness, int contrast)
        {
            BlurRenderer.Radius = radius;
            BrightnessAndContrastRenderer.SetParameters(brightness, contrast);
        }
        public GaussainBlurRenderer BlurRenderer
        {
            get;
            set;
        }
        public BrightnessAndContrastRenderer BrightnessAndContrastRenderer
        {
            get;
            set;
        }
        public override void Render(
            Surface src,
            Surface dest,
            Rectangle[] rois,
            int startIndex,
            int length)
        {

            unsafe
            {
                BlurRenderer.Render(src, dest, rois, startIndex, length);
                //****  // have to do adjustment in place, hence dstArgs for both 'args' parameters
                BrightnessAndContrastRenderer.Render(dest, dest, rois, startIndex, length);

                for (int i = startIndex; i < startIndex + length; ++i)
                {
                    Rectangle roi = rois[i];

                    for (int y = roi.Top; y < roi.Bottom; ++y)
                    {
                        ColorBgra* dstPtr = dest.GetPointAddressUnchecked(roi.Left, y);
                        ColorBgra* srcPtr = src.GetPointAddressUnchecked(roi.Left, y);


                        screenBlendOp.Apply(dstPtr, srcPtr, dstPtr, roi.Width);
                    }
                }
            }
        }

    }

}