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
    public class SepiaRenderer : EffectRendererBase
    {
        readonly UnaryPixelOp _levels;
        readonly UnaryPixelOp _desaturate;
        public SepiaRenderer()
        {
            _desaturate = new UnaryPixelOps.Desaturate();

            _levels = new UnaryPixelOps.Level(
                ColorBgra.Black,
                ColorBgra.White,
                new float[] { 1.2f, 1.0f, 0.8f },
                ColorBgra.Black,
                ColorBgra.White);
        }
        public override void Render(Surface src, Surface dest, Rectangle[] rois, int startIndex, int length)
        {
            _desaturate.Apply(dest, src, rois, startIndex, length);
            _levels.Apply(dest, dest, rois, startIndex, length);
        }
    }
}