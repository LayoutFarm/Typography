/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////
//Apache2, 2017-present, WinterDev
using System;
using PixelFarm.Drawing;

namespace PaintFx.Effects
{
    public class ReliefEffectRenderer : ColorDiffEffectRenderer
    {


        private double angle;
        public double Angle
        {
            get { return angle; }
        }
        public void SetAngle(double a)
        {
            this.angle = a;
            this.weights = new double[3][];
            for (int i = 0; i < this.weights.Length; ++i)
            {
                this.weights[i] = new double[3];
            }

            // adjust and convert angle to radians
            double r = (double)this.angle * 2.0 * Math.PI / 360.0;

            // angle delta for each weight
            double dr = Math.PI / 4.0;

            // for r = 0 this builds an Relief filter pointing straight left
            this.weights[0][0] = Math.Cos(r + dr);
            this.weights[0][1] = Math.Cos(r + 2.0 * dr);
            this.weights[0][2] = Math.Cos(r + 3.0 * dr);

            this.weights[1][0] = Math.Cos(r);
            this.weights[1][1] = 1;
            this.weights[1][2] = Math.Cos(r + 4.0 * dr);

            this.weights[2][0] = Math.Cos(r - dr);
            this.weights[2][1] = Math.Cos(r - 2.0 * dr);
            this.weights[2][2] = Math.Cos(r - 3.0 * dr);

        }
    }
}