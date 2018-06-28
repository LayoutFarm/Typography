//BSD, 2014-present, WinterDev
//----------------------------------------------------------------------------
// Anti-Grain Geometry - Version 2.4
// Copyright (C) 2002-2005 Maxim Shemanarev (http://www.antigrain.com)
// Copyright (C) 2005 Tony Juricic (tonygeek@yahoo.com)
//
// Permission to copy, use, modify, sell and distribute this software 
// is granted provided this copyright notice appears in all copies. 
// This software is provided "as is" without express or implied
// warranty, and with no claim as to its suitability for any purpose.
//
//----------------------------------------------------------------------------
// Contact: mcseem@antigrain.com
//          mcseemagg@yahoo.com
//          http://www.antigrain.com
//----------------------------------------------------------------------------

using System;
using PixelFarm.VectorMath;
namespace PixelFarm.CpuBlit.VertexProcessing
{
    public static class Curves
    {
        //--------------------------------------------curve_approximation_method_e
        public enum CurveApproximationMethod
        {
            Inc,
            Div
        }

        //static readonly double CURVE_DISTANCE_EPSILON = 1e-30;
        internal const double CURVE_COLLINEARITY_EPSILON = 1e-30;
        internal const double CURVE_ANGLE_TOLERANCE_EPSILON = 0.01;
        internal const int CURVE_RECURSION_LIMIT = 32;
        //-------------------------------------------------------catrom_to_bezier
        public static Curve4Points CatromToBezier(double x1, double y1,
                                              double x2, double y2,
                                              double x3, double y3,
                                              double x4, double y4)
        {
            // Trans. matrix Catmull-Rom to Bezier
            //
            //  0       1       0       0
            //  -1/6    1       1/6     0
            //  0       1/6     1       -1/6
            //  0       0       1       0
            //
            return new Curve4Points(
                x2,
                y2,
                (-x1 + 6 * x2 + x3) / 6,
                (-y1 + 6 * y2 + y3) / 6,
                (x2 + 6 * x3 - x4) / 6,
                (y2 + 6 * y3 - y4) / 6,
                x3,
                y3);
        }

        //-----------------------------------------------------------------------
        public static Curve4Points CatromToBezier(Curve4Points cp)
        {
            return CatromToBezier(
                    cp.c0, cp.c1,
                    cp.c2, cp.c3,
                    cp.c4, cp.c5,
                    cp.c6, cp.c7);
        }

        //-----------------------------------------------------ubspline_to_bezier
        public static Curve4Points UbSplineToBezier(double x1, double y1,
                                                double x2, double y2,
                                                double x3, double y3,
                                                double x4, double y4)
        {
            // Trans. matrix Uniform BSpline to Bezier
            //
            //  1/6     4/6     1/6     0
            //  0       4/6     2/6     0
            //  0       2/6     4/6     0
            //  0       1/6     4/6     1/6
            //
            return new Curve4Points(
                (x1 + 4 * x2 + x3) / 6,
                (y1 + 4 * y2 + y3) / 6,
                (4 * x2 + 2 * x3) / 6,
                (4 * y2 + 2 * y3) / 6,
                (2 * x2 + 4 * x3) / 6,
                (2 * y2 + 4 * y3) / 6,
                (x2 + 4 * x3 + x4) / 6,
                (y2 + 4 * y3 + y4) / 6);
        }

        //-----------------------------------------------------------------------
        public static Curve4Points UbSplineToBezier(Curve4Points cp)
        {
            return UbSplineToBezier(
                    cp.c0, cp.c1,
                    cp.c2, cp.c3,
                    cp.c4, cp.c5,
                    cp.c6, cp.c7);
        }

        //------------------------------------------------------hermite_to_bezier
        public static Curve4Points HermiteToBezier(double x1, double y1,
                                               double x2, double y2,
                                               double x3, double y3,
                                               double x4, double y4)
        {
            // Trans. matrix Hermite to Bezier
            //
            //  1       0       0       0
            //  1       0       1/3     0
            //  0       1       0       -1/3
            //  0       1       0       0
            //
            return new Curve4Points(
                x1,
                y1,
                (3 * x1 + x3) / 3,
                (3 * y1 + y3) / 3,
                (3 * x2 - x4) / 3,
                (3 * y2 - y4) / 3,
                x2,
                y2);
        }

        //-----------------------------------------------------------------------
        public static Curve4Points HermiteToBezier(Curve4Points cp)
        {
            return HermiteToBezier(
                    cp.c0, cp.c1,
                    cp.c2, cp.c3,
                    cp.c4, cp.c5,
                    cp.c6, cp.c7);
        }
    }

    //--------------------------------------------------------------curve3_inc
    public sealed class Curve3Inc
    {
        int m_num_steps;
        int m_step;
        double m_scale;
        double m_start_x;
        double m_start_y;
        double m_end_x;
        double m_end_y;
        double m_fx;
        double m_fy;
        double m_dfx;
        double m_dfy;
        double m_ddfx;
        double m_ddfy;
        double m_saved_fx;
        double m_saved_fy;
        double m_saved_dfx;
        double m_saved_dfy;
        public Curve3Inc()
        {
            m_num_steps = (0);
            m_step = (0);
            m_scale = (1.0);
        }

        public Curve3Inc(double x1, double y1,
                   double x2, double y2,
                   double x3, double y3)
        {
            m_num_steps = (0);
            m_step = (0);
            m_scale = (1.0);
            Init(x1, y1, x2, y2, x3, y3);
        }

        public void Reset() { m_num_steps = 0; m_step = -1; }

        public void Init(double x1, double y1,
                  double cx, double cy,
                  double x2, double y2)
        {
            m_start_x = x1;
            m_start_y = y1;
            m_end_x = x2;
            m_end_y = y2;
            double dx1 = cx - x1;
            double dy1 = cy - y1;
            double dx2 = x2 - cx;
            double dy2 = y2 - cy;
            double len = Math.Sqrt(dx1 * dx1 + dy1 * dy1) + Math.Sqrt(dx2 * dx2 + dy2 * dy2);
            m_num_steps = (int)AggMath.uround(len * 0.25 * m_scale);
            if (m_num_steps < 4)
            {
                m_num_steps = 4;
            }

            double eachIncStep = 1.0 / m_num_steps;
            double eachIncStep2 = eachIncStep * eachIncStep;
            double tmpx = (x1 - cx * 2.0 + x2) * eachIncStep2;
            double tmpy = (y1 - cy * 2.0 + y2) * eachIncStep2;
            m_saved_fx = m_fx = x1;
            m_saved_fy = m_fy = y1;
            m_saved_dfx = m_dfx = tmpx + (cx - x1) * (2.0 * eachIncStep);
            m_saved_dfy = m_dfy = tmpy + (cy - y1) * (2.0 * eachIncStep);
            m_ddfx = tmpx * 2.0;
            m_ddfy = tmpy * 2.0;
            m_step = m_num_steps;
        }

        public Curves.CurveApproximationMethod ApproximationMethod
        {
            get { return Curves.CurveApproximationMethod.Inc; }
            set { }
        }
        public double ApproximationScale
        {
            get { return this.m_scale; }
            set { this.m_scale = value; }
        }
        public double AngleTolerance
        {
            get { return 0; }
            set { }
        }
        public double CuspLimit
        {
            get { return 0; }
            set { }
        }
    }

    //-------------------------------------------------------------curve3_div
    public sealed class Curve3Div
    {
        double m_approximation_scale;
        double m_distance_tolerance_square;
        double m_angle_tolerance;
        int m_count;
        ArrayList<Vector2> m_points;
        public Curve3Div()
        {
            m_points = new ArrayList<Vector2>();
            m_approximation_scale = (1.0);
            m_angle_tolerance = (0.0);
            m_count = 0;
        }

        public Curve3Div(double x1, double y1,
                   double cx, double cy,
                   double x2, double y2)
        {
            m_approximation_scale = (1.0);
            m_angle_tolerance = (0.0);
            m_count = 0;
            Init(x1, y1, cx, cy, x2, y2);
        }

        public void Reset() { m_points.Clear(); m_count = 0; }
        public void Init(double x1, double y1,
                double cx, double cy,
                double x2, double y2)
        {
            m_points.Clear();
            m_distance_tolerance_square = 0.5 / m_approximation_scale;
            m_distance_tolerance_square *= m_distance_tolerance_square;
            AddBezier(x1, y1, cx, cy, x2, y2);
            m_count = 0;
        }

        public Curves.CurveApproximationMethod ApproximationMethod
        {
            get { return Curves.CurveApproximationMethod.Div; }
        }
        public double ApproximationScale
        {
            get { return this.m_approximation_scale; }
            set { this.m_approximation_scale = value; }
        }
        public double AngleTolerance
        {
            get { return this.m_angle_tolerance; }
            set { this.m_angle_tolerance = value; }
        }
        public double CuspLimit
        {
            get { return 0; }
            set { }
        }

        public ArrayList<Vector2> GetInternalPoints() { return this.m_points; }
        void AddBezier(double x1, double y1,
                    double x2, double y2,
                    double x3, double y3)
        {
            m_points.AddVertex(new Vector2(x1, y1));
            AddRecursiveBezier(x1, y1, x2, y2, x3, y3, 0);
            m_points.AddVertex(new Vector2(x3, y3));
        }

        private void AddRecursiveBezier(double x1, double y1,
                              double x2, double y2,
                              double x3, double y3,
                              int level)
        {
            if (level > Curves.CURVE_RECURSION_LIMIT)
            {
                return;
            }

            // Calculate all the mid-points of the line segments
            //----------------------
            double x12 = (x1 + x2) / 2;
            double y12 = (y1 + y2) / 2;
            double x23 = (x2 + x3) / 2;
            double y23 = (y2 + y3) / 2;
            double x123 = (x12 + x23) / 2;
            double y123 = (y12 + y23) / 2;
            double dx = x3 - x1;
            double dy = y3 - y1;
            double d = Math.Abs(((x2 - x3) * dy - (y2 - y3) * dx));
            double da;
            if (d > Curves.CURVE_COLLINEARITY_EPSILON)
            {
                // Regular case
                //-----------------
                if (d * d <= m_distance_tolerance_square * (dx * dx + dy * dy))
                {
                    // If the curvature doesn't exceed the distance_tolerance value
                    // we tend to finish subdivisions.
                    //----------------------
                    if (m_angle_tolerance < Curves.CURVE_ANGLE_TOLERANCE_EPSILON)
                    {
                        m_points.AddVertex(new Vector2(x123, y123));
                        return;
                    }

                    // Angle & Cusp Condition
                    //----------------------
                    da = Math.Abs(Math.Atan2(y3 - y2, x3 - x2) - Math.Atan2(y2 - y1, x2 - x1));
                    if (da >= Math.PI) da = 2 * Math.PI - da;
                    if (da < m_angle_tolerance)
                    {
                        // Finally we can stop the recursion
                        //----------------------
                        m_points.AddVertex(new Vector2(x123, y123));
                        return;
                    }
                }
            }
            else
            {
                // Collinear case
                //------------------
                da = dx * dx + dy * dy;
                if (da == 0)
                {
                    d = AggMath.calc_sq_distance(x1, y1, x2, y2);
                }
                else
                {
                    d = ((x2 - x1) * dx + (y2 - y1) * dy) / da;
                    if (d > 0 && d < 1)
                    {
                        // Simple collinear case, 1---2---3
                        // We can leave just two endpoints
                        return;
                    }
                    if (d <= 0) d = AggMath.calc_sq_distance(x2, y2, x1, y1);
                    else if (d >= 1) d = AggMath.calc_sq_distance(x2, y2, x3, y3);
                    else d = AggMath.calc_sq_distance(x2, y2, x1 + d * dx, y1 + d * dy);
                }
                if (d < m_distance_tolerance_square)
                {
                    m_points.AddVertex(new Vector2(x2, y2));
                    return;
                }
            }

            // Continue subdivision
            //----------------------
            AddRecursiveBezier(x1, y1, x12, y12, x123, y123, level + 1);
            AddRecursiveBezier(x123, y123, x23, y23, x3, y3, level + 1);
        }
    }

    //-------------------------------------------------------------curve4_points
    public sealed class Curve4Points
    {
        public readonly double c0, c1, c2, c3, c4, c5, c6, c7;
        public Curve4Points() { }
        public Curve4Points(double x1, double y1,
                      double x2, double y2,
                      double x3, double y3,
                      double x4, double y4)
        {
            c0 = x1; c1 = y1;
            c2 = x2; c3 = y2;
            c4 = x3; c5 = y3;
            c6 = x4; c7 = y4;
        }
    }

    //-------------------------------------------------------------curve4_inc
    public sealed class Curve4Inc
    {
        int m_num_steps;
        int m_step;
        double m_scale;
        double m_start_x;
        double m_start_y;
        double m_end_x;
        double m_end_y;
        double m_fx;
        double m_fy;
        double m_dfx;
        double m_dfy;
        double m_ddfx;
        double m_ddfy;
        double m_dddfx;
        double m_dddfy;
        double m_saved_fx;
        double m_saved_fy;
        double m_saved_dfx;
        double m_saved_dfy;
        double m_saved_ddfx;
        double m_saved_ddfy;
        public Curve4Inc()
        {
            m_num_steps = (0);
            m_step = (0);
            m_scale = (1.0);
        }

        public Curve4Inc(double x1, double y1,
                  double cx1, double cy1,
                  double cx2, double cy2,
                  double x2, double y2)
        {
            m_num_steps = (0);
            m_step = (0);
            m_scale = (1.0);
            Init(x1, y1, cx1, cy1, cx2, cy2, x2, y2);
        }

        public Curve4Inc(Curve4Points cp)
        {
            m_num_steps = (0);
            m_step = (0);
            m_scale = (1.0);
            Init(
                    cp.c0, cp.c1,
                    cp.c2, cp.c3,
                    cp.c4, cp.c5,
                    cp.c6, cp.c7);
        }

        public void Reset() { m_num_steps = 0; m_step = -1; }
        public void Init(double x1, double y1,
                  double cx1, double cy1,
                  double cx2, double cy2,
                  double x2, double y2)
        {
            m_start_x = x1;
            m_start_y = y1;
            m_end_x = x2;
            m_end_y = y2;
            double dx1 = cx1 - x1;
            double dy1 = cy1 - y1;
            double dx2 = cx2 - cx1;
            double dy2 = cy2 - cy1;
            double dx3 = x2 - cx2;
            double dy3 = y2 - cy2;
            double len = (Math.Sqrt(dx1 * dx1 + dy1 * dy1) +
                          Math.Sqrt(dx2 * dx2 + dy2 * dy2) +
                          Math.Sqrt(dx3 * dx3 + dy3 * dy3)) * 0.25 * m_scale;
            m_num_steps = (int)AggMath.uround(len);
            if (m_num_steps < 4)
            {
                m_num_steps = 4;
            }

            double eachIncStep = 1.0 / m_num_steps;
            double eachIncStep2 = eachIncStep * eachIncStep;
            double eachIncStep3 = eachIncStep * eachIncStep * eachIncStep;
            double pre1 = 3.0 * eachIncStep;
            double pre2 = 3.0 * eachIncStep2;
            double pre4 = 6.0 * eachIncStep2;
            double pre5 = 6.0 * eachIncStep3;
            double tmp1x = x1 - cx1 * 2.0 + cx2;
            double tmp1y = y1 - cy1 * 2.0 + cy2;
            double tmp2x = (cx1 - cx2) * 3.0 - x1 + x2;
            double tmp2y = (cy1 - cy2) * 3.0 - y1 + y2;
            m_saved_fx = m_fx = x1;
            m_saved_fy = m_fy = y1;
            m_saved_dfx = m_dfx = (cx1 - x1) * pre1 + tmp1x * pre2 + tmp2x * eachIncStep3;
            m_saved_dfy = m_dfy = (cy1 - y1) * pre1 + tmp1y * pre2 + tmp2y * eachIncStep3;
            m_saved_ddfx = m_ddfx = tmp1x * pre4 + tmp2x * pre5;
            m_saved_ddfy = m_ddfy = tmp1y * pre4 + tmp2y * pre5;
            m_dddfx = tmp2x * pre5;
            m_dddfy = tmp2y * pre5;
            m_step = m_num_steps;
        }

        public void Init(Curve4Points cp)
        {
            Init(cp.c0, cp.c1,
                    cp.c2, cp.c3,
                    cp.c4, cp.c5,
                    cp.c6, cp.c7);
        }
        public Curves.CurveApproximationMethod ApproximationMethod
        {
            get { return Curves.CurveApproximationMethod.Inc; }
            set { }
        }
        public double ApproximationScale
        {
            get { return this.m_scale; }
            set { this.m_scale = value; }
        }
        public double AngleTolerance
        {
            get { return 0; }
            set { }
        }
        public double CuspLmit
        {
            get { return 0; }
            set { }
        }


        public void RewindZero()
        {
            if (m_num_steps == 0)
            {
                m_step = -1;
                return;
            }
            m_step = m_num_steps;
            m_fx = m_saved_fx;
            m_fy = m_saved_fy;
            m_dfx = m_saved_dfx;
            m_dfy = m_saved_dfy;
            m_ddfx = m_saved_ddfx;
            m_ddfy = m_saved_ddfy;
        }

        //public VertexCmd GetNextVertex(out double x, out double y)
        //{
        //    if (m_step < 0)
        //    {
        //        x = 0;
        //        y = 0;
        //        return VertexCmd.NoMore;
        //    }

        //    if (m_step == m_num_steps)
        //    {
        //        x = m_start_x;
        //        y = m_start_y;
        //        --m_step;
        //        return VertexCmd.MoveTo;
        //    }

        //    if (m_step == 0)
        //    {
        //        x = m_end_x;
        //        y = m_end_y;
        //        --m_step;
        //        return VertexCmd.LineTo;
        //    }

        //    m_fx += m_dfx;
        //    m_fy += m_dfy;
        //    m_dfx += m_ddfx;
        //    m_dfy += m_ddfy;
        //    m_ddfx += m_dddfx;
        //    m_ddfy += m_dddfy;
        //    x = m_fx;
        //    y = m_fy;
        //    --m_step;
        //    return VertexCmd.LineTo;
        //}
    }

    //-------------------------------------------------------------curve4_div
    public sealed class Curve4Div
    {
        double m_approximation_scale;
        double m_distance_tolerance_square;
        double m_angle_tolerance;
        double m_cusp_limit;
        int m_count;
        ArrayList<Vector2> m_points;
        public Curve4Div()
        {
            m_points = new ArrayList<Vector2>();
            m_approximation_scale = (1.0);
            m_angle_tolerance = (0.0);
            m_cusp_limit = (0.0);
            m_count = (0);
        }

        public Curve4Div(double x1, double y1,
                   double x2, double y2,
                   double x3, double y3,
                   double x4, double y4)
        {
            m_approximation_scale = (1.0);
            m_angle_tolerance = (0.0);
            m_cusp_limit = (0.0);
            m_count = (0);
            Init(x1, y1, x2, y2, x3, y3, x4, y4);
        }

        public Curve4Div(Curve4Points cp)
        {
            m_approximation_scale = (1.0);
            m_angle_tolerance = (0.0);
            m_count = (0);
            Init(
                    cp.c0, cp.c1,
                    cp.c2, cp.c3,
                    cp.c4, cp.c5,
                    cp.c6, cp.c7);
        }
        public ArrayList<Vector2> GetInternalPoints() { return this.m_points; }
        public void Reset() { m_points.Clear(); m_count = 0; }
        public void Init(double x1, double y1,
                  double x2, double y2,
                  double x3, double y3,
                  double x4, double y4)
        {
            m_points.Clear();
            m_distance_tolerance_square = 0.5 / m_approximation_scale;
            m_distance_tolerance_square *= m_distance_tolerance_square;
            AddBezier(x1, y1, x2, y2, x3, y3, x4, y4);
            m_count = 0;
        }


        public void Init(Curve4Points cp)
        {
            Init(
                    cp.c0, cp.c1,
                    cp.c2, cp.c3,
                    cp.c4, cp.c5,
                    cp.c6, cp.c7);
        }
        public double ApproximationScale
        {
            get { return this.m_approximation_scale; }
            set { this.m_approximation_scale = value; }
        }
        public double AngleTolerance
        {
            get { return this.m_angle_tolerance; }
            set { this.m_angle_tolerance = value; }
        }

        public double CuspLimit
        {
            get { return (m_cusp_limit == 0.0) ? 0.0 : Math.PI - m_cusp_limit; }
            set { m_cusp_limit = (value == 0.0) ? 0.0 : Math.PI - value; }
        }


        void AddBezier(double x1, double y1,
                  double x2, double y2,
                  double x3, double y3,
                  double x4, double y4)
        {
            m_points.AddVertex(new Vector2(x1, y1));
            AddRecursiveBezier(x1, y1, x2, y2, x3, y3, x4, y4, 0);
            m_points.AddVertex(new Vector2(x4, y4));
        }


        void AddRecursiveBezier(double x1, double y1,
                            double x2, double y2,
                            double x3, double y3,
                            double x4, double y4,
                            int level)
        {
            //recursive
            if (level > Curves.CURVE_RECURSION_LIMIT)
            {
                return;
            }

            // Calculate all the mid-points of the line segments
            //----------------------
            double x12 = (x1 + x2) / 2;
            double y12 = (y1 + y2) / 2;
            double x23 = (x2 + x3) / 2;
            double y23 = (y2 + y3) / 2;
            double x34 = (x3 + x4) / 2;
            double y34 = (y3 + y4) / 2;
            double x123 = (x12 + x23) / 2;
            double y123 = (y12 + y23) / 2;
            double x234 = (x23 + x34) / 2;
            double y234 = (y23 + y34) / 2;
            double x1234 = (x123 + x234) / 2;
            double y1234 = (y123 + y234) / 2;
            // Try to approximate the full cubic curve by a single straight line
            //------------------
            double dx = x4 - x1;
            double dy = y4 - y1;
            double d2 = Math.Abs(((x2 - x4) * dy - (y2 - y4) * dx));
            double d3 = Math.Abs(((x3 - x4) * dy - (y3 - y4) * dx));
            double da1, da2, k;
            int SwitchCase = 0;
            if (d2 > Curves.CURVE_COLLINEARITY_EPSILON)
            {
                SwitchCase = 2;
            }
            if (d3 > Curves.CURVE_COLLINEARITY_EPSILON)
            {
                SwitchCase++;
            }

            switch (SwitchCase)
            {
                case 0:
                    // All collinear OR p1==p4
                    //----------------------
                    k = dx * dx + dy * dy;
                    if (k == 0)
                    {
                        d2 = AggMath.calc_sq_distance(x1, y1, x2, y2);
                        d3 = AggMath.calc_sq_distance(x4, y4, x3, y3);
                    }
                    else
                    {
                        k = 1 / k;
                        da1 = x2 - x1;
                        da2 = y2 - y1;
                        d2 = k * (da1 * dx + da2 * dy);
                        da1 = x3 - x1;
                        da2 = y3 - y1;
                        d3 = k * (da1 * dx + da2 * dy);
                        if (d2 > 0 && d2 < 1 && d3 > 0 && d3 < 1)
                        {
                            // Simple collinear case, 1---2---3---4
                            // We can leave just two endpoints
                            return;
                        }
                        if (d2 <= 0) d2 = AggMath.calc_sq_distance(x2, y2, x1, y1);
                        else if (d2 >= 1) d2 = AggMath.calc_sq_distance(x2, y2, x4, y4);
                        else d2 = AggMath.calc_sq_distance(x2, y2, x1 + d2 * dx, y1 + d2 * dy);
                        if (d3 <= 0) d3 = AggMath.calc_sq_distance(x3, y3, x1, y1);
                        else if (d3 >= 1) d3 = AggMath.calc_sq_distance(x3, y3, x4, y4);
                        else d3 = AggMath.calc_sq_distance(x3, y3, x1 + d3 * dx, y1 + d3 * dy);
                    }
                    if (d2 > d3)
                    {
                        if (d2 < m_distance_tolerance_square)
                        {
                            m_points.AddVertex(new Vector2(x2, y2));
                            return;
                        }
                    }
                    else
                    {
                        if (d3 < m_distance_tolerance_square)
                        {
                            m_points.AddVertex(new Vector2(x3, y3));
                            return;
                        }
                    }
                    break;
                case 1:
                    // p1,p2,p4 are collinear, p3 is significant
                    //----------------------
                    if (d3 * d3 <= m_distance_tolerance_square * (dx * dx + dy * dy))
                    {
                        if (m_angle_tolerance < Curves.CURVE_ANGLE_TOLERANCE_EPSILON)
                        {
                            m_points.AddVertex(new Vector2(x23, y23));
                            return;
                        }

                        // Angle Condition
                        //----------------------
                        da1 = Math.Abs(Math.Atan2(y4 - y3, x4 - x3) - Math.Atan2(y3 - y2, x3 - x2));
                        if (da1 >= Math.PI) da1 = 2 * Math.PI - da1;
                        if (da1 < m_angle_tolerance)
                        {
                            m_points.AddVertex(new Vector2(x2, y2));
                            m_points.AddVertex(new Vector2(x3, y3));
                            return;
                        }

                        if (m_cusp_limit != 0.0)
                        {
                            if (da1 > m_cusp_limit)
                            {
                                m_points.AddVertex(new Vector2(x3, y3));
                                return;
                            }
                        }
                    }
                    break;
                case 2:
                    // p1,p3,p4 are collinear, p2 is significant
                    //----------------------
                    if (d2 * d2 <= m_distance_tolerance_square * (dx * dx + dy * dy))
                    {
                        if (m_angle_tolerance < Curves.CURVE_ANGLE_TOLERANCE_EPSILON)
                        {
                            m_points.AddVertex(new Vector2(x23, y23));
                            return;
                        }

                        // Angle Condition
                        //----------------------
                        da1 = Math.Abs(Math.Atan2(y3 - y2, x3 - x2) - Math.Atan2(y2 - y1, x2 - x1));
                        if (da1 >= Math.PI) da1 = 2 * Math.PI - da1;
                        if (da1 < m_angle_tolerance)
                        {
                            m_points.AddVertex(new Vector2(x2, y2));
                            m_points.AddVertex(new Vector2(x3, y3));
                            return;
                        }

                        if (m_cusp_limit != 0.0)
                        {
                            if (da1 > m_cusp_limit)
                            {
                                m_points.AddVertex(new Vector2(x2, y2));
                                return;
                            }
                        }
                    }
                    break;
                case 3:
                    // Regular case
                    //-----------------
                    if ((d2 + d3) * (d2 + d3) <= m_distance_tolerance_square * (dx * dx + dy * dy))
                    {
                        // If the curvature doesn't exceed the distance_tolerance value
                        // we tend to finish subdivisions.
                        //----------------------
                        if (m_angle_tolerance < Curves.CURVE_ANGLE_TOLERANCE_EPSILON)
                        {
                            m_points.AddVertex(new Vector2(x23, y23));
                            return;
                        }

                        // Angle & Cusp Condition
                        //----------------------
                        k = Math.Atan2(y3 - y2, x3 - x2);
                        da1 = Math.Abs(k - Math.Atan2(y2 - y1, x2 - x1));
                        da2 = Math.Abs(Math.Atan2(y4 - y3, x4 - x3) - k);
                        if (da1 >= Math.PI) da1 = 2 * Math.PI - da1;
                        if (da2 >= Math.PI) da2 = 2 * Math.PI - da2;
                        if (da1 + da2 < m_angle_tolerance)
                        {
                            // Finally we can stop the recursion
                            //----------------------
                            m_points.AddVertex(new Vector2(x23, y23));
                            return;
                        }

                        if (m_cusp_limit != 0.0)
                        {
                            if (da1 > m_cusp_limit)
                            {
                                m_points.AddVertex(new Vector2(x2, y2));
                                return;
                            }

                            if (da2 > m_cusp_limit)
                            {
                                m_points.AddVertex(new Vector2(x3, y3));
                                return;
                            }
                        }
                    }
                    break;
            }

            // Continue subdivision
            //----------------------
            AddRecursiveBezier(x1, y1, x12, y12, x123, y123, x1234, y1234, level + 1);
            AddRecursiveBezier(x1234, y1234, x234, y234, x34, y34, x4, y4, level + 1);
        }
    }

    //-----------------------------------------------------------------curve3
    /// <summary>
    /// curve to line segment
    /// </summary>
    public sealed class Curve3
    {
        Curve3Inc m_curve_inc = new Curve3Inc();
        Curve3Div m_curve_div = new Curve3Div();
        Curves.CurveApproximationMethod m_approximation_method;
        public Curve3()
        {
            m_approximation_method = Curves.CurveApproximationMethod.Div;
        }

        public void Reset()
        {
            m_curve_inc.Reset();
            m_curve_div.Reset();
        }

        void Init(double x1, double y1,
             double cx, double cy,
             double x2, double y2)
        {
            if (m_approximation_method == Curves.CurveApproximationMethod.Inc)
            {
                m_curve_inc.Init(x1, y1, cx, cy, x2, y2);
            }
            else
            {
                m_curve_div.Init(x1, y1, cx, cy, x2, y2);
            }
        }
        public Curves.CurveApproximationMethod ApproximationMethod
        {
            get { return this.m_approximation_method; }
            set { this.m_approximation_method = value; }
        }
        public double ApproximationScale
        {
            get
            {
                return this.m_curve_inc.ApproximationScale;
            }
            set
            {
                m_curve_inc.ApproximationScale = m_curve_div.ApproximationScale = value;
            }
        }

        public double AngleTolerance
        {
            get { return this.m_curve_div.AngleTolerance; }
            set { this.m_curve_div.AngleTolerance = value; }
        }

        public double CuspLimit
        {
            get { return this.m_curve_div.CuspLimit; }
            set { this.m_curve_div.CuspLimit = value; }
        }



        //IEnumerable<VertexData> GetVertexIter()
        //{
        //    if (m_approximation_method == Curves.CurveApproximationMethod.Inc)
        //    {
        //        foreach (VertexData vertexData in m_curve_inc.GetVertexIter())
        //        {
        //            yield return vertexData;
        //        }
        //    }
        //    else
        //    {
        //        foreach (VertexData vertexData in m_curve_div.GetVertexIter())
        //        {
        //            yield return vertexData;
        //        }
        //    }
        //}
    }

    //-----------------------------------------------------------------curve4
    public sealed class Curve4
    {
        Curve4Inc m_curve_inc = new Curve4Inc();
        Curve4Div m_curve_div = new Curve4Div();
        Curves.CurveApproximationMethod m_approximation_method;
        public Curve4()
        {
            m_approximation_method = Curves.CurveApproximationMethod.Div;
        }

        public Curve4(double x1, double y1,
               double cx1, double cy1,
               double cx2, double cy2,
               double x2, double y2)
            : base()
        {
            m_approximation_method = Curves.CurveApproximationMethod.Div;
            Init(x1, y1, cx1, cy1, cx2, cy2, x2, y2);
        }

        public Curve4(Curve4Points cp)
        {
            m_approximation_method = Curves.CurveApproximationMethod.Div;
            Init(
                    cp.c0, cp.c1,
                    cp.c2, cp.c3,
                    cp.c4, cp.c5,
                    cp.c6, cp.c7);
        }


        public void Reset()
        {
            m_curve_inc.Reset();
            m_curve_div.Reset();
        }

        public void Init(double x1, double y1,
               double cx1, double cy1,
               double cx2, double cy2,
               double x2, double y2)
        {
            if (m_approximation_method == Curves.CurveApproximationMethod.Inc)
            {
                m_curve_inc.Init(x1, y1, cx1, cy1, cx2, cy2, x2, y2);
            }
            else
            {
                m_curve_div.Init(x1, y1, cx1, cy1, cx2, cy2, x2, y2);
            }
        }

        public void Init(Curve4Points cp)
        {
            Init(
                    cp.c0, cp.c1,
                    cp.c2, cp.c3,
                    cp.c4, cp.c5,
                    cp.c6, cp.c7);
        }


        public Curves.CurveApproximationMethod ApproximationMethod
        {
            get { return m_approximation_method; }
            set { m_approximation_method = value; }
        }


        public double ApproximationScale
        {
            get { return m_curve_inc.ApproximationScale; }
            set
            {
                this.m_curve_inc.ApproximationScale = value;
                this.m_curve_div.ApproximationScale = value;
            }
        }

        public double AngleTolerance
        {
            get { return m_curve_div.AngleTolerance; }
            set
            {
                m_curve_div.AngleTolerance = value;
                m_curve_inc.AngleTolerance = value;
            }
        }

        public double CuspLimit
        {
            get { return this.m_curve_div.CuspLimit; }
            set { this.m_curve_div.CuspLimit = value; }
        }



        //public IEnumerable<VertexData> GetVertexIter()
        //{
        //    if (m_approximation_method == Curves.CurveApproximationMethod.Inc)
        //    {
        //        return m_curve_inc.GetVertexIter();
        //    }
        //    else
        //    {
        //        return m_curve_div.GetVertexIter();
        //    }
        //}

        //public void RewindZero()
        //{
        //    if (m_approximation_method == Curves.CurveApproximationMethod.Inc)
        //    {
        //        m_curve_inc.RewindZero();
        //    }
        //    else
        //    {
        //        m_curve_div.RewindZero();
        //    }
        //}
        //public ShapePath.FlagsAndCommand GetNextVertex(out double x, out double y)
        //{
        //    if (m_approximation_method == Curves.CurveApproximationMethod.Inc)
        //    {
        //        return m_curve_inc.GetNextVertex(out x, out y);
        //    }
        //    return m_curve_div.vertex(out x, out y);
        //}
    }
}