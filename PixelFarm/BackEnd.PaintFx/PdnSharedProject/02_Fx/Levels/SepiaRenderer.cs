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
    public class SepiaRenderer : EffectRendererBase
    {
        private UnaryPixelOp levels;
        private UnaryPixelOp desaturate;
        public SepiaRenderer()
        {
            this.desaturate = new UnaryPixelOps.Desaturate();

            this.levels = new UnaryPixelOps.Level(
                ColorBgra.Black,
                ColorBgra.White,
                new float[] { 1.2f, 1.0f, 0.8f },
                ColorBgra.Black,
                ColorBgra.White);
        }
        public override void Render(Surface src, Surface dest, Rectangle[] rois, int startIndex, int length)
        {
            this.desaturate.Apply(dest, src, rois, startIndex, length);
            this.levels.Apply(dest, dest, rois, startIndex, length);
        }
    }
}