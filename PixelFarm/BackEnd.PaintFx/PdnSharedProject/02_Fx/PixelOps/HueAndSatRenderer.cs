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
    public class HueAndSatRenderer : EffectRendererBase
    {
        int _hue;
        int _saturation;
        int _lightness;
        UnaryPixelOp _pixelOp;
        public void SetParameters(int hue, int sat, int lightness)
        {
            _hue = hue;
            _saturation = sat;
            _lightness = lightness;

            // map the range [0,100] -> [0,100] and the range [101,200] -> [103,400]
            if (_saturation > 100)
            {
                _saturation = ((_saturation - 100) * 3) + 100;
            }

            if (_hue == 0 && _saturation == 100 && _lightness == 0)
            {
                _pixelOp = new UnaryPixelOps.Identity();
            }
            else
            {
                _pixelOp = new UnaryPixelOps.HueSaturationLightness(_hue, _saturation, _lightness);
            }
        }
        public override void Render(Surface src, Surface dst, Rectangle[] rois, int start, int len)
        {
            _pixelOp.Apply(dst, src, rois, start, len);
        }
    }
}