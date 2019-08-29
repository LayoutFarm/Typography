//MIT, 2014-present, WinterDev
using System;
using PixelFarm.Drawing;

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



#if DEBUG

    //we will visit this again



    class AggBezierArc
    {
        //------------------------------------------------------------------------
        public double[] m_vertices = new double[26];
        VertexCmd m_cmd;
        public int m_num_vertices;
        int m_verticx;
        const double bezier_arc_angle_epsilon = 0.01;


        static double fabs(double a)
        {
            return (a < 0) ? -a : a;
        }
        static double fmod(double x, double y)
        {
            double result1 = x / y;
            return result1 - Math.Floor(result1);
        }
        public void init(double x, double y,
                                 double rx, double ry,
                                 double start_angle,
                                 double sweep_angle)
        {

            start_angle = fmod(start_angle, 2.0 * Math.PI);
            if (sweep_angle >= 2.0 * Math.PI) sweep_angle = 2.0 * Math.PI;
            if (sweep_angle <= -2.0 * Math.PI) sweep_angle = -2.0 * Math.PI;

            if (fabs(sweep_angle) < 1e-10)
            {
                m_num_vertices = 4;
                m_cmd = VertexCmd.LineTo;
                m_vertices[0] = x + rx * Math.Cos(start_angle);
                m_vertices[1] = y + ry * Math.Sin(start_angle);
                m_vertices[2] = x + rx * Math.Cos(start_angle + sweep_angle);
                m_vertices[3] = y + ry * Math.Sin(start_angle + sweep_angle);
                return;
            }

            double total_sweep = 0.0;
            double local_sweep = 0.0;
            double prev_sweep;
            m_num_vertices = 2;
            m_cmd = VertexCmd.C4;

            bool done = false;
            do
            {
                if (sweep_angle < 0.0)
                {
                    prev_sweep = total_sweep;
                    local_sweep = -Math.PI * 0.5;
                    total_sweep -= Math.PI * 0.5;
                    if (total_sweep <= sweep_angle + bezier_arc_angle_epsilon)
                    {
                        local_sweep = sweep_angle - prev_sweep;
                        done = true;
                    }
                }
                else
                {
                    prev_sweep = total_sweep;
                    local_sweep = Math.PI * 0.5;
                    total_sweep += Math.PI * 0.5;
                    if (total_sweep >= sweep_angle - bezier_arc_angle_epsilon)
                    {
                        local_sweep = sweep_angle - prev_sweep;
                        done = true;
                    }
                }

                unsafe
                {
                    fixed (double* m_head = m_vertices)
                    {
                        arc_to_bezier(x, y, rx, ry,
                            start_angle,
                            local_sweep,
                            m_head + m_num_vertices - 2);
                    }

                }


                m_num_vertices += 6;
                start_angle += local_sweep;
            }
            while (!done && m_num_vertices < 26);
        }
        ////------------------------------------------------------------arc_to_bezier
        unsafe void arc_to_bezier(double cx, double cy, double rx, double ry,
                            double start_angle, double sweep_angle,
                            double* curve)
        {
            double x0 = Math.Cos(sweep_angle / 2.0);
            double y0 = Math.Sin(sweep_angle / 2.0);
            double tx = (1.0 - x0) * 4.0 / 3.0;
            double ty = y0 - tx * x0 / y0;
            double[] px = new double[4];
            double[] py = new double[4];
            px[0] = x0;
            py[0] = -y0;
            px[1] = x0 + tx;
            py[1] = -ty;
            px[2] = x0 + tx;
            py[2] = ty;
            px[3] = x0;
            py[3] = y0;

            double sn = Math.Sin(start_angle + sweep_angle / 2.0);
            double cs = Math.Cos(start_angle + sweep_angle / 2.0);

            for (int i = 0; i < 4; i++)
            {
                curve[i * 2] = cx + rx * (px[i] * cs - py[i] * sn);
                curve[i * 2 + 1] = cy + ry * (px[i] * sn + py[i] * cs);
            }
        }
    }

    class AggBezierArcSvg
    {

        //==========================================================bezier_arc_svg
        // Compute an SVG-style bezier arc. 
        //
        // Computes an elliptical arc from (x1, y1) to (x2, y2). The size and 
        // orientation of the ellipse are defined by two radii (rx, ry) 
        // and an x-axis-rotation, which indicates how the ellipse as a whole 
        // is rotated relative to the current coordinate system. The center 
        // (cx, cy) of the ellipse is calculated automatically to satisfy the 
        // constraints imposed by the other parameters. 
        // large-arc-flag and sweep-flag contribute to the automatic calculations 
        // and help determine how the arc is drawn.

        //from agg_bezier_arc.cpp
        // This epsilon is used to prevent us from adding degenerate curves 
        // (converging to a single point).
        // The value isn't very critical. Function arc_to_bezier() has a limit 
        // of the sweep_angle. If fabs(sweep_angle) exceeds pi/2 the curve 
        // becomes inaccurate. But slight exceeding is quite appropriate.
        //-------------------------------------------------bezier_arc_angle_epsilon



        AggBezierArc m_arc = new AggBezierArc();
        bool m_radii_ok;
        //--------------------------------------------------------------------
        public void init(double x0, double y0,
                                  double rx, double ry,
                                  double angle,
                                  bool large_arc_flag,
                                  bool sweep_flag,
                                  double x2, double y2)
        {
            m_radii_ok = true;

            if (rx < 0.0) rx = -rx;
            if (ry < 0.0) ry = -rx;

            // Calculate the middle point between 
            // the current and the final points
            //------------------------
            double dx2 = (x0 - x2) / 2.0;
            double dy2 = (y0 - y2) / 2.0;

            double cos_a = Math.Cos(angle);
            double sin_a = Math.Sin(angle);

            // Calculate (x1, y1)
            //------------------------
            double x1 = cos_a * dx2 + sin_a * dy2;
            double y1 = -sin_a * dx2 + cos_a * dy2;

            // Ensure radii are large enough
            //------------------------
            double prx = rx * rx;
            double pry = ry * ry;
            double px1 = x1 * x1;
            double py1 = y1 * y1;

            // Check that radii are large enough
            //------------------------
            double radii_check = px1 / prx + py1 / pry;
            if (radii_check > 1.0)
            {
                rx = Math.Sqrt(radii_check) * rx;
                ry = Math.Sqrt(radii_check) * ry;
                prx = rx * rx;
                pry = ry * ry;
                if (radii_check > 10.0) m_radii_ok = false;
            }

            // Calculate (cx1, cy1)
            //------------------------
            double sign = (large_arc_flag == sweep_flag) ? -1.0 : 1.0;
            double sq = (prx * pry - prx * py1 - pry * px1) / (prx * py1 + pry * px1);
            double coef = sign * Math.Sqrt((sq < 0) ? 0 : sq);
            double cx1 = coef * ((rx * y1) / ry);
            double cy1 = coef * -((ry * x1) / rx);

            //
            // Calculate (cx, cy) from (cx1, cy1)
            //------------------------
            double sx2 = (x0 + x2) / 2.0;
            double sy2 = (y0 + y2) / 2.0;
            double cx = sx2 + (cos_a * cx1 - sin_a * cy1);
            double cy = sy2 + (sin_a * cx1 + cos_a * cy1);

            // Calculate the start_angle (angle1) and the sweep_angle (dangle)
            //------------------------
            double ux = (x1 - cx1) / rx;
            double uy = (y1 - cy1) / ry;
            double vx = (-x1 - cx1) / rx;
            double vy = (-y1 - cy1) / ry;
            double p, n;

            // Calculate the angle start
            //------------------------
            n = Math.Sqrt(ux * ux + uy * uy);
            p = ux; // (1 * ux) + (0 * uy)
            sign = (uy < 0) ? -1.0 : 1.0;
            double v = p / n;
            if (v < -1.0) v = -1.0;
            if (v > 1.0) v = 1.0;
            double start_angle = sign * Math.Acos(v);

            // Calculate the sweep angle
            //------------------------
            n = Math.Sqrt((ux * ux + uy * uy) * (vx * vx + vy * vy));
            p = ux * vx + uy * vy;
            sign = (ux * vy - uy * vx < 0) ? -1.0 : 1.0;
            v = p / n;
            if (v < -1.0) v = -1.0;
            if (v > 1.0) v = 1.0;
            double sweep_angle = sign * Math.Acos(v);
            if (!sweep_flag && sweep_angle > 0)
            {
                sweep_angle -= Math.PI * 2.0;
            }
            else
            if (sweep_flag && sweep_angle < 0)
            {
                sweep_angle += Math.PI * 2.0;
            }

            // We can now build and transform the resulting arc
            //------------------------
            m_arc.init(0.0, 0.0, rx, ry, start_angle, sweep_angle);

            Affine mtx = Affine.New(
                AffinePlan.Rotate(angle),
                AffinePlan.Translate(cx, cy)
                );

            for (int i = 2; i < m_arc.m_num_vertices - 2; i += 2)
            {
                mtx.Transform(ref m_arc.m_vertices[i], ref m_arc.m_vertices[i + 1]);
            }

            // We must make sure that the starting and ending points
            // exactly coincide with the initial (x0,y0) and (x2,y2)
            m_arc.m_vertices[0] = x0;
            m_arc.m_vertices[1] = y0;
            if (m_arc.m_num_vertices > 2)
            {
                m_arc.m_vertices[m_arc.m_num_vertices - 2] = x2;
                m_arc.m_vertices[m_arc.m_num_vertices - 1] = y2;
            }
        }


        struct CenterFormArc
        {
            public double cx;
            public double cy;
            public double radStartAngle;
            public double radSweepDiff;
            public bool scaleUp;
        }


        //---------------------------------------------------------------------
        public void DrawArc(
            float fromX, float fromY, float endX, float endY,
            float xaxisRotationAngleDec, float rx, float ry,
            SvgArcSize arcSize, SvgArcSweep arcSweep)
        {
            //------------------
            //SVG Elliptical arc ...
            //from Apache Batik
            //-----------------

            CenterFormArc centerFormArc = new CenterFormArc();
            ComputeArc2(fromX, fromY, rx, ry,
                 AggMath.deg2rad(xaxisRotationAngleDec),
                 arcSize == SvgArcSize.Large,
                 arcSweep == SvgArcSweep.Negative,
                 endX, endY, ref centerFormArc);

            //
            using (VectorToolBox.Borrow(out Arc arcTool))
            using (VxsTemp.Borrow(out var v1, out var v2, out var v3))
            {
                arcTool.Init(centerFormArc.cx, centerFormArc.cy, rx, ry,
                  centerFormArc.radStartAngle,
                  (centerFormArc.radStartAngle + centerFormArc.radSweepDiff));
                bool stopLoop = false;
                foreach (VertexData vertexData in arcTool.GetVertexIter())
                {
                    switch (vertexData.command)
                    {
                        case VertexCmd.NoMore:
                            stopLoop = true;
                            break;
                        default:
                            v1.AddVertex(vertexData.x, vertexData.y, vertexData.command);
                            //yield return vertexData;
                            break;
                    }
                    //------------------------------
                    if (stopLoop) { break; }
                }

                double scaleRatio = 1;
                if (centerFormArc.scaleUp)
                {
                    int vxs_count = v1.Count;
                    double px0, py0, px_last, py_last;
                    v1.GetVertex(0, out px0, out py0);
                    v1.GetVertex(vxs_count - 1, out px_last, out py_last);
                    double distance1 = Math.Sqrt((px_last - px0) * (px_last - px0) + (py_last - py0) * (py_last - py0));
                    double distance2 = Math.Sqrt((endX - fromX) * (endX - fromX) + (endY - fromY) * (endY - fromY));
                    if (distance1 < distance2)
                    {
                        scaleRatio = distance2 / distance1;
                    }
                    else
                    {
                    }
                }

                if (xaxisRotationAngleDec != 0)
                {
                    //also  rotate 
                    if (centerFormArc.scaleUp)
                    {

                        //var mat = Affine.NewMatix(
                        //        new AffinePlan(AffineMatrixCommand.Translate, -centerFormArc.cx, -centerFormArc.cy),
                        //        new AffinePlan(AffineMatrixCommand.Scale, scaleRatio, scaleRatio),
                        //        new AffinePlan(AffineMatrixCommand.Rotate, DegToRad(xaxisRotationAngleDec)),
                        //        new AffinePlan(AffineMatrixCommand.Translate, centerFormArc.cx, centerFormArc.cy));
                        //mat1.TransformToVxs(v1, v2);
                        //v1 = v2;

                        AffineMat mat = AffineMat.Iden;
                        mat.Translate(-centerFormArc.cx, -centerFormArc.cy);
                        mat.Scale(scaleRatio);
                        mat.RotateDeg(xaxisRotationAngleDec);
                        mat.Translate(centerFormArc.cx, centerFormArc.cy);
                        VertexStoreTransformExtensions.TransformToVxs(ref mat, v1, v2);
                        v1 = v2;
                    }
                    else
                    {
                        //not scale
                        //var mat = Affine.NewMatix(
                        //        AffinePlan.Translate(-centerFormArc.cx, -centerFormArc.cy),
                        //        AffinePlan.RotateDeg(xaxisRotationAngleDec),
                        //        AffinePlan.Translate(centerFormArc.cx, centerFormArc.cy)); 
                        //mat.TransformToVxs(v1, v2);
                        //v1 = v2;

                        AffineMat mat = AffineMat.Iden;
                        mat.Translate(-centerFormArc.cx, -centerFormArc.cy);
                        mat.RotateDeg(xaxisRotationAngleDec);
                        mat.Translate(centerFormArc.cx, centerFormArc.cy);
                        VertexStoreTransformExtensions.TransformToVxs(ref mat, v1, v2);
                        v1 = v2;
                    }
                }
                else
                {
                    //no rotate
                    if (centerFormArc.scaleUp)
                    {
                        //var mat = Affine.NewMatix(
                        //        new AffinePlan(AffineMatrixCommand.Translate, -centerFormArc.cx, -centerFormArc.cy),
                        //        new AffinePlan(AffineMatrixCommand.Scale, scaleRatio, scaleRatio),
                        //        new AffinePlan(AffineMatrixCommand.Translate, centerFormArc.cx, centerFormArc.cy));

                        //mat.TransformToVxs(v1, v2);
                        //v1 = v2; 
                        AffineMat mat = AffineMat.Iden;
                        mat.Translate(-centerFormArc.cx, -centerFormArc.cy);
                        mat.RotateDeg(scaleRatio);
                        mat.Translate(centerFormArc.cx, centerFormArc.cy);
                        //
                        VertexStoreTransformExtensions.TransformToVxs(ref mat, v1, v2);
                        v1 = v2;

                    }
                }

                //_stroke.Width = this.StrokeWidth;
                //_stroke.MakeVxs(v1, v3);
                //_pcx.DrawGfxPath(_pcx.StrokeColor, _pathRenderVxBuilder.CreatePathRenderVx(v3));

            }
        }



        static void ComputeArc2(double x0, double y0,
                             double rx, double ry,
                             double xAngleRad,
                             bool largeArcFlag,
                             bool sweepFlag,
                             double x, double y, ref CenterFormArc result)
        {
            //from  SVG1.1 spec
            //----------------------------------
            //step1: Compute (x1dash,y1dash)
            //----------------------------------

            double dx2 = (x0 - x) / 2.0;
            double dy2 = (y0 - y) / 2.0;
            double cosAngle = Math.Cos(xAngleRad);
            double sinAngle = Math.Sin(xAngleRad);
            double x1 = (cosAngle * dx2 + sinAngle * dy2);
            double y1 = (-sinAngle * dx2 + cosAngle * dy2);
            // Ensure radii are large enough
            rx = Math.Abs(rx);
            ry = Math.Abs(ry);
            double prx = rx * rx;
            double pry = ry * ry;
            double px1 = x1 * x1;
            double py1 = y1 * y1;
            // check that radii are large enough


            double radiiCheck = px1 / prx + py1 / pry;
            if (radiiCheck > 1)
            {
                rx = Math.Sqrt(radiiCheck) * rx;
                ry = Math.Sqrt(radiiCheck) * ry;
                prx = rx * rx;
                pry = ry * ry;
                result.scaleUp = true;
            }

            //----------------------------------
            //step2: Compute (cx1,cy1)
            //----------------------------------
            double sign = (largeArcFlag == sweepFlag) ? -1 : 1;
            double sq = ((prx * pry) - (prx * py1) - (pry * px1)) / ((prx * py1) + (pry * px1));
            sq = (sq < 0) ? 0 : sq;
            double coef = (sign * Math.Sqrt(sq));
            double cx1 = coef * ((rx * y1) / ry);
            double cy1 = coef * -((ry * x1) / rx);
            //----------------------------------
            //step3:  Compute (cx, cy) from (cx1, cy1)
            //----------------------------------
            double sx2 = (x0 + x) / 2.0;
            double sy2 = (y0 + y) / 2.0;
            double cx = sx2 + (cosAngle * cx1 - sinAngle * cy1);
            double cy = sy2 + (sinAngle * cx1 + cosAngle * cy1);
            //----------------------------------
            //step4: Compute theta and anfkediff
            double ux = (x1 - cx1) / rx;
            double uy = (y1 - cy1) / ry;
            double vx = (-x1 - cx1) / rx;
            double vy = (-y1 - cy1) / ry;
            double p, n;
            // Compute the angle start
            n = Math.Sqrt((ux * ux) + (uy * uy));
            p = ux; // (1 * ux) + (0 * uy)
            sign = (uy < 0) ? -1d : 1d;
            double angleStart = (sign * Math.Acos(p / n));  // Math.toDegrees(sign * Math.Acos(p / n));
            // Compute the angle extent
            n = Math.Sqrt((ux * ux + uy * uy) * (vx * vx + vy * vy));
            p = ux * vx + uy * vy;
            sign = (ux * vy - uy * vx < 0) ? -1d : 1d;
            double angleExtent = (sign * Math.Acos(p / n));// Math.toDegrees(sign * Math.Acos(p / n));
            //if (!sweepFlag && angleExtent > 0)
            //{
            //    angleExtent -= 360f;
            //}
            //else if (sweepFlag && angleExtent < 0)
            //{
            //    angleExtent += 360f;
            //}

            result.cx = cx;
            result.cy = cy;
            result.radStartAngle = angleStart;
            result.radSweepDiff = angleExtent;
        }
        static Arc ComputeArc(double x0, double y0,
                              double rx, double ry,
                              double angle,
                              bool largeArcFlag,
                              bool sweepFlag,
                               double x, double y)
        {

            //from Apache2, https://xmlgraphics.apache.org/
            /** 
         * This constructs an unrotated Arc2D from the SVG specification of an 
         * Elliptical arc.  To get the final arc you need to apply a rotation
         * transform such as:
         * 
         * AffineTransform.getRotateInstance
         *     (angle, arc.getX()+arc.getWidth()/2, arc.getY()+arc.getHeight()/2);
         */
            //
            // Elliptical arc implementation based on the SVG specification notes
            //

            // Compute the half distance between the current and the final point
            double dx2 = (x0 - x) / 2.0;
            double dy2 = (y0 - y) / 2.0;
            // Convert angle from degrees to radians
            angle = ((angle % 360.0) * Math.PI / 180f);
            double cosAngle = Math.Cos(angle);
            double sinAngle = Math.Sin(angle);
            //
            // Step 1 : Compute (x1, y1)
            //
            double x1 = (cosAngle * dx2 + sinAngle * dy2);
            double y1 = (-sinAngle * dx2 + cosAngle * dy2);
            // Ensure radii are large enough
            rx = Math.Abs(rx);
            ry = Math.Abs(ry);
            double Prx = rx * rx;
            double Pry = ry * ry;
            double Px1 = x1 * x1;
            double Py1 = y1 * y1;
            // check that radii are large enough
            double radiiCheck = Px1 / Prx + Py1 / Pry;
            if (radiiCheck > 1)
            {
                rx = Math.Sqrt(radiiCheck) * rx;
                ry = Math.Sqrt(radiiCheck) * ry;
                Prx = rx * rx;
                Pry = ry * ry;
            }

            //
            // Step 2 : Compute (cx1, cy1)
            //
            double sign = (largeArcFlag == sweepFlag) ? -1 : 1;
            double sq = ((Prx * Pry) - (Prx * Py1) - (Pry * Px1)) / ((Prx * Py1) + (Pry * Px1));
            sq = (sq < 0) ? 0 : sq;
            double coef = (sign * Math.Sqrt(sq));
            double cx1 = coef * ((rx * y1) / ry);
            double cy1 = coef * -((ry * x1) / rx);
            //
            // Step 3 : Compute (cx, cy) from (cx1, cy1)
            //
            double sx2 = (x0 + x) / 2.0;
            double sy2 = (y0 + y) / 2.0;
            double cx = sx2 + (cosAngle * cx1 - sinAngle * cy1);
            double cy = sy2 + (sinAngle * cx1 + cosAngle * cy1);
            //
            // Step 4 : Compute the angleStart (angle1) and the angleExtent (dangle)
            //
            double ux = (x1 - cx1) / rx;
            double uy = (y1 - cy1) / ry;
            double vx = (-x1 - cx1) / rx;
            double vy = (-y1 - cy1) / ry;
            double p, n;
            // Compute the angle start
            n = Math.Sqrt((ux * ux) + (uy * uy));
            p = ux; // (1 * ux) + (0 * uy)
            sign = (uy < 0) ? -1d : 1d;
            double angleStart = (sign * Math.Acos(p / n));  // Math.toDegrees(sign * Math.Acos(p / n));
            // Compute the angle extent
            n = Math.Sqrt((ux * ux + uy * uy) * (vx * vx + vy * vy));
            p = ux * vx + uy * vy;
            sign = (ux * vy - uy * vx < 0) ? -1d : 1d;
            double angleExtent = (sign * Math.Acos(p / n));// Math.toDegrees(sign * Math.Acos(p / n));
            if (!sweepFlag && angleExtent > 0)
            {
                angleExtent -= 360f;
            }
            else if (sweepFlag && angleExtent < 0)
            {
                angleExtent += 360f;
            }
            //angleExtent %= 360f;
            //angleStart %= 360f;

            //
            // We can now build the resulting Arc2D in double precision
            //
            //Arc2D.Double arc = new Arc2D.Double();
            //arc.x = cx - rx;
            //arc.y = cy - ry;
            //arc.width = rx * 2.0;
            //arc.height = ry * 2.0;
            //arc.start = -angleStart;
            //arc.extent = -angleExtent;
            Arc arc = new Arc();
            arc.Init(x, y, rx, ry, -(angleStart), -(angleExtent));
            return arc;
        }
        //================
    }

#endif

}