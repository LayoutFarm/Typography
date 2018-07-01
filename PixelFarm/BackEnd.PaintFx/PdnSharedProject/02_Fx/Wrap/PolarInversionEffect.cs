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

    public class PolarInversionEffRenderer : WrapBasedRenderer
    {
        private double amount;
        public PolarInversionEffRenderer()
        {

        }
        public void SetParameters(double amount)
        {
            this.amount = amount;
        }
        protected override void InverseTransform(ref TransformData data)
        {
            double x = data.X;
            double y = data.Y;

            // NOTE: when x and y are zero, this will divide by zero and return NaN
            double invertDistance = PixelUtils.Lerp(1d, DefaultRadius2 / ((x * x) + (y * y)), amount);

            data.X = x * invertDistance;
            data.Y = y * invertDistance;
        }
    }
}