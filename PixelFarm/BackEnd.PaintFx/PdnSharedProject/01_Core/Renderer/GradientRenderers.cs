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
namespace PaintFx
{
    public static class GradientRenderers
    {
        public abstract class LinearBase : GradientRenderer
        {
            protected float _dtdx;
            protected float _dtdy;

            public override void BeforeRender()
            {
                PointF vec = new PointF(EndPoint.X - StartPoint.X, EndPoint.Y - StartPoint.Y);
                float mag = PixelUtils.Magnitude(vec);

                if (EndPoint.X == StartPoint.X)
                {
                    _dtdx = 0;
                }
                else
                {
                    _dtdx = vec.X / (mag * mag);
                }

                if (EndPoint.Y == StartPoint.Y)
                {
                    _dtdy = 0;
                }
                else
                {
                    _dtdy = vec.Y / (mag * mag);
                }

                base.BeforeRender();
            }

            protected internal LinearBase(bool alphaOnly, BinaryPixelOp normalBlendOp)
                : base(alphaOnly, normalBlendOp)
            {
            }
        }

        public abstract class LinearStraight : LinearBase
        {
            public override float ComputeUnboundedLerp(int x, int y)
            {
                float dx = x - StartPoint.X;
                float dy = y - StartPoint.Y;

                float lerp = (dx * _dtdx) + (dy * _dtdy);

                return lerp;
            }

            protected internal LinearStraight(bool alphaOnly, BinaryPixelOp normalBlendOp)
                : base(alphaOnly, normalBlendOp)
            {
            }
        }

        public sealed class LinearReflected : LinearStraight
        {
            public override float BoundLerp(float t)
            {
                return PixelUtils.Clamp(Math.Abs(t), 0, 1);
            }

            public LinearReflected(bool alphaOnly, BinaryPixelOp normalBlendOp)
                : base(alphaOnly, normalBlendOp)
            {
            }
        }

        public sealed class LinearClamped : LinearStraight
        {
            public override float BoundLerp(float t)
            {
                return PixelUtils.Clamp(t, 0, 1);
            }

            public LinearClamped(bool alphaOnly, BinaryPixelOp normalBlendOp)
                : base(alphaOnly, normalBlendOp)
            {
            }
        }

        public sealed class LinearDiamond : LinearStraight
        {
            public override float ComputeUnboundedLerp(int x, int y)
            {
                float dx = x - StartPoint.X;
                float dy = y - StartPoint.Y;

                float lerp1 = (dx * _dtdx) + (dy * _dtdy);
                float lerp2 = (dx * _dtdy) - (dy * _dtdx);

                float absLerp1 = Math.Abs(lerp1);
                float absLerp2 = Math.Abs(lerp2);

                return absLerp1 + absLerp2;
            }

            public override float BoundLerp(float t)
            {
                return PixelUtils.Clamp(t, 0, 1);
            }

            public LinearDiamond(bool alphaOnly, BinaryPixelOp normalBlendOp)
                : base(alphaOnly, normalBlendOp)
            {
            }
        }

        public sealed class Radial : GradientRenderer
        {
            float _invDistanceScale;

            public override void BeforeRender()
            {
                float distanceScale = PixelUtils.Distance(this.StartPoint, this.EndPoint);

                if (distanceScale == 0)
                {
                    _invDistanceScale = 0;
                }
                else
                {
                    _invDistanceScale = 1.0f / distanceScale;
                }

                base.BeforeRender();
            }

            public override float ComputeUnboundedLerp(int x, int y)
            {
                float dx = x - StartPoint.X;
                float dy = y - StartPoint.Y;

                float distance = (float)Math.Sqrt(dx * dx + dy * dy);

                return distance * _invDistanceScale;
            }

            public override float BoundLerp(float t)
            {
                return PixelUtils.Clamp(t, 0, 1);
            }

            public Radial(bool alphaOnly, BinaryPixelOp normalBlendOp)
                : base(alphaOnly, normalBlendOp)
            {
            }
        }

        public sealed class Conical : GradientRenderer
        {
            float _tOffset;
            const float INV_PI = (float)(1.0 / Math.PI);

            public override void BeforeRender()
            {
                _tOffset = -ComputeUnboundedLerp((int)EndPoint.X, (int)EndPoint.Y);
                base.BeforeRender();
            }

            public override float ComputeUnboundedLerp(int x, int y)
            {
                float ax = x - StartPoint.X;
                float ay = y - StartPoint.Y;

                float theta = (float)Math.Atan2(ay, ax);

                float t = theta * INV_PI;

                return t + _tOffset;
            }

            public override float BoundLerp(float t)
            {
                if (t > 1)
                {
                    t -= 2;
                }
                else if (t < -1)
                {
                    t += 2;
                }

                return PixelUtils.Clamp(Math.Abs(t), 0, 1);
            }

            public Conical(bool alphaOnly, BinaryPixelOp normalBlendOp)
                : base(alphaOnly, normalBlendOp)
            {
            }
        }
    }
}
