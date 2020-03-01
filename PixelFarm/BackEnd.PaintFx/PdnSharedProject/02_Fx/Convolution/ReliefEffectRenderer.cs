/////////////////////////////////////////////////////////////////////////////////
// Paint.NET (MIT,from version 3.36.7, see=> https://github.com/rivy/OpenPDN   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////
//MIT, 2017-present, WinterDev
using System;
using PixelFarm.Drawing;

namespace PaintFx.Effects
{
    public class ReliefEffectRenderer : ColorDiffEffectRenderer
    {
        public double Angle { get; private set; }
        public void SetAngle(double a)
        {
            this.Angle = a;
            _weights = new double[3][];
            for (int i = 0; i < _weights.Length; ++i)
            {
                _weights[i] = new double[3];
            }

            // adjust and convert angle to radians
            double r = (double)this.Angle * 2.0 * Math.PI / 360.0;

            // angle delta for each weight
            double dr = Math.PI / 4.0;

            // for r = 0 this builds an Relief filter pointing straight left
            _weights[0][0] = Math.Cos(r + dr);
            _weights[0][1] = Math.Cos(r + 2.0 * dr);
            _weights[0][2] = Math.Cos(r + 3.0 * dr);

            _weights[1][0] = Math.Cos(r);
            _weights[1][1] = 1;
            _weights[1][2] = Math.Cos(r + 4.0 * dr);

            _weights[2][0] = Math.Cos(r - dr);
            _weights[2][1] = Math.Cos(r - 2.0 * dr);
            _weights[2][2] = Math.Cos(r - 3.0 * dr);

        }
    }
}