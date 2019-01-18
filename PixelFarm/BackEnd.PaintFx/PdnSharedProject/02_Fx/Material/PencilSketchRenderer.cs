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
    public class PencilSketchRenderer
    {
        int _pencilTipSize;
        int _colorRange;


        GaussainBlurRenderer _gaussainBlueRenderer = new GaussainBlurRenderer();
        UnaryPixelOps.Desaturate _desaturateOp = new UnaryPixelOps.Desaturate();
        InvertColorRenderer _invertColorRenderer = new InvertColorRenderer();
        BrightnessAndContrastRenderer _bcRenderer = new BrightnessAndContrastRenderer();
        UserBlendOps.ColorDodgeBlendOp _colorDodgeOp = new UserBlendOps.ColorDodgeBlendOp();

        public PencilSketchRenderer()
        {
            _pencilTipSize = 2;
            _colorRange = 0;

            _gaussainBlueRenderer.Radius = _pencilTipSize;
            _bcRenderer.SetParameters(_colorRange, -_colorRange);


        }
        public unsafe void Render(Surface src, Surface dst, Rectangle[] rois, int start, int len)
        {

            _gaussainBlueRenderer.Render(src, dst, rois, start, len);// src->dst
            _bcRenderer.Render(dst, dst, rois, start, len);//dst->dst ***
            _invertColorRenderer.Render(dst, dst, rois, start, len);//dst->dst ***
            _desaturateOp.Apply(dst, dst, rois, start, len);//dst->dst ***

            int lim = start + len;
            for (int i = start; i < lim; ++i)
            {
                Rectangle roi = rois[i];

                for (int y = roi.Top; y < roi.Bottom; ++y)
                {
                    ColorBgra* srcPtr = src.GetPointAddressUnchecked(roi.X, y);
                    ColorBgra* dstPtr = dst.GetPointAddressUnchecked(roi.X, y);

                    for (int x = roi.Left; x < roi.Right; ++x)
                    {
                        ColorBgra srcGrey = _desaturateOp.Apply(*srcPtr);
                        ColorBgra sketched = _colorDodgeOp.Apply(srcGrey, *dstPtr);
                        *dstPtr = sketched;

                        ++srcPtr;
                        ++dstPtr;
                    }
                }
            }

        }

    }
}