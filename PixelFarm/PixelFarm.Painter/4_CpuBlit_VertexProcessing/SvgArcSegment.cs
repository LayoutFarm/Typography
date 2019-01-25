//MIT, 2014-present, WinterDev
using System;
namespace PixelFarm.CpuBlit.VertexProcessing
{
    public enum SvgArcSweep
    {
        Negative = 0,
        Positive = 1
    }

    public enum SvgArcSize
    {
        Small = 0,
        Large = 1
    }
    public sealed class SvgArcSegment
    {
        //from SVG.NET (https://github.com/vvvv/SVG)

        private const double RadiansPerDegree = Math.PI / 180.0;
        private const double DoublePI = Math.PI * 2;

        public float RadiusX
        {
            get;
            set;
        }

        public float RadiusY
        {
            get;
            set;
        }

        public float Angle
        {
            get;
            set;
        }

        public SvgArcSweep Sweep
        {
            get;
            set;
        }

        public SvgArcSize Size
        {
            get;
            set;
        }
        public float StartX { get; set; }
        public float StartY { get; set; }
        public float EndX { get; set; }
        public float EndY { get; set; }

        internal SvgArcSegment() { }
        public void Set(float startX, float startY,
            float radiusX, float radiusY,
            float angle, SvgArcSize size,
            SvgArcSweep sweep, float endX, float endY)
        {
            this.StartX = startX;
            this.StartY = startY;
            this.EndX = endX;
            this.EndY = endY;

            this.RadiusX = Math.Abs(radiusX);
            this.RadiusY = Math.Abs(radiusY);
            this.Angle = angle;
            this.Sweep = sweep;
            this.Size = size;
        }

        static double CalculateVectorAngle(double ux, double uy, double vx, double vy)
        {
            double ta = Math.Atan2(uy, ux);
            double tb = Math.Atan2(vy, vx);

            if (tb >= ta)
            {
                return tb - ta;
            }

            return SvgArcSegment.DoublePI - (ta - tb);
        }

        public void AddToPath(PathWriter graphicsPath)
        {
            if (StartX == EndX && StartY == EndY)
            {
                return;
            }

            if (this.RadiusX == 0.0f && this.RadiusY == 0.0f)
            {
                //graphicsPath.AddLine(this.Start, this.End);
                graphicsPath.LineTo(this.StartX, this.StartY);
                graphicsPath.LineTo(this.EndX, this.EndY);
                return;
            }

            double sinPhi = Math.Sin(this.Angle * SvgArcSegment.RadiansPerDegree);
            double cosPhi = Math.Cos(this.Angle * SvgArcSegment.RadiansPerDegree);

            double x1dash = cosPhi * (this.StartX - this.EndX) / 2.0 + sinPhi * (this.StartY - this.EndY) / 2.0;
            double y1dash = -sinPhi * (this.StartX - this.EndX) / 2.0 + cosPhi * (this.StartY - this.EndY) / 2.0;

            double root;
            double numerator = this.RadiusX * this.RadiusX * this.RadiusY * this.RadiusY - this.RadiusX * this.RadiusX * y1dash * y1dash - this.RadiusY * this.RadiusY * x1dash * x1dash;

            float rx = this.RadiusX;
            float ry = this.RadiusY;

            if (numerator < 0.0)
            {
                float s = (float)Math.Sqrt(1.0 - numerator / (this.RadiusX * this.RadiusX * this.RadiusY * this.RadiusY));

                rx *= s;
                ry *= s;
                root = 0.0;
            }
            else
            {
                root = ((this.Size == SvgArcSize.Large && this.Sweep == SvgArcSweep.Positive) || (this.Size == SvgArcSize.Small && this.Sweep == SvgArcSweep.Negative) ? -1.0 : 1.0) * Math.Sqrt(numerator / (this.RadiusX * this.RadiusX * y1dash * y1dash + this.RadiusY * this.RadiusY * x1dash * x1dash));
            }

            double cxdash = root * rx * y1dash / ry;
            double cydash = -root * ry * x1dash / rx;

            double cx = cosPhi * cxdash - sinPhi * cydash + (this.StartX + this.EndX) / 2.0;
            double cy = sinPhi * cxdash + cosPhi * cydash + (this.StartY + this.EndY) / 2.0;

            double theta1 = SvgArcSegment.CalculateVectorAngle(1.0, 0.0, (x1dash - cxdash) / rx, (y1dash - cydash) / ry);
            double dtheta = SvgArcSegment.CalculateVectorAngle((x1dash - cxdash) / rx, (y1dash - cydash) / ry, (-x1dash - cxdash) / rx, (-y1dash - cydash) / ry);

            if (this.Sweep == SvgArcSweep.Negative && dtheta > 0)
            {
                dtheta -= 2.0 * Math.PI;
            }
            else if (this.Sweep == SvgArcSweep.Positive && dtheta < 0)
            {
                dtheta += 2.0 * Math.PI;
            }

            int segments = (int)Math.Ceiling((double)Math.Abs(dtheta / (Math.PI / 2.0)));
            double delta = dtheta / segments;
            double t = 8.0 / 3.0 * Math.Sin(delta / 4.0) * Math.Sin(delta / 4.0) / Math.Sin(delta / 2.0);

            double startX = this.StartX;
            double startY = this.StartY;

            for (int i = 0; i < segments; ++i)
            {
                double cosTheta1 = Math.Cos(theta1);
                double sinTheta1 = Math.Sin(theta1);
                double theta2 = theta1 + delta;
                double cosTheta2 = Math.Cos(theta2);
                double sinTheta2 = Math.Sin(theta2);

                double endpointX = cosPhi * rx * cosTheta2 - sinPhi * ry * sinTheta2 + cx;
                double endpointY = sinPhi * rx * cosTheta2 + cosPhi * ry * sinTheta2 + cy;

                double dx1 = t * (-cosPhi * rx * sinTheta1 - sinPhi * ry * cosTheta1);
                double dy1 = t * (-sinPhi * rx * sinTheta1 + cosPhi * ry * cosTheta1);

                double dxe = t * (cosPhi * rx * sinTheta2 + sinPhi * ry * cosTheta2);
                double dye = t * (sinPhi * rx * sinTheta2 - cosPhi * ry * cosTheta2);

                //graphicsPath.AddBezier((float)startX, (float)startY, (float)(startX + dx1), (float)(startY + dy1),
                //    (float)(endpointX + dxe), (float)(endpointY + dye), (float)endpointX, (float)endpointY);
                graphicsPath.Curve4((float)(startX + dx1), (float)(startY + dy1),
                 (float)(endpointX + dxe), (float)(endpointY + dye), (float)endpointX, (float)endpointY);
                theta1 = theta2;
                startX = (float)endpointX;
                startY = (float)endpointY;
            }
        }

    }

}