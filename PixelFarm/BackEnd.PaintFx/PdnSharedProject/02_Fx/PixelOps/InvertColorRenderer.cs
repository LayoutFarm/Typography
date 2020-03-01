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

    public class InvertColorRenderer : EffectRendererBase
    {
        UnaryPixelOps.Invert _invertOp = new UnaryPixelOps.Invert();
        public override void Render(Surface src, Surface dst, Rectangle[] rois, int start, int len)
        {
            _invertOp.Apply(dst, src, rois, start, len);
        }
    }
}