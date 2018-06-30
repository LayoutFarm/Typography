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
    public class HueAndSatRenderer : EffectRendererBase
    {
        private int hue;
        private int saturation;
        private int lightness;
        private UnaryPixelOp pixelOp;
        public void SetParameters(int hue, int sat, int lightness)
        {
            this.hue = hue;
            this.saturation = sat;
            this.lightness = lightness;

            // map the range [0,100] -> [0,100] and the range [101,200] -> [103,400]
            if (this.saturation > 100)
            {
                this.saturation = ((this.saturation - 100) * 3) + 100;
            }

            if (this.hue == 0 && this.saturation == 100 && this.lightness == 0)
            {
                this.pixelOp = new UnaryPixelOps.Identity();
            }
            else
            {
                this.pixelOp = new UnaryPixelOps.HueSaturationLightness(this.hue, this.saturation, this.lightness);
            }
        }
        public override void Render(Surface src, Surface dst, Rectangle[] rois, int start, int len)
        {
            this.pixelOp.Apply(dst, src, rois, start, len);
        }
    }
}