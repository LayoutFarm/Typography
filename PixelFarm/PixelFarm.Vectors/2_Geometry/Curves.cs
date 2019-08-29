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
namespace PixelFarm.CpuBlit.VertexProcessing
{
    public static class Curves
    {


        //static readonly double CURVE_DISTANCE_EPSILON = 1e-30;
        internal const double CURVE_COLLINEARITY_EPSILON = 1e-30;
        internal const double CURVE_ANGLE_TOLERANCE_EPSILON = 0.01;
        internal const int CURVE_RECURSION_LIMIT = 32;
        //-------------------------------------------------------catrom_to_bezier
        public static void CatromToBezier(double x1, double y1,
                                              double x2, double y2,
                                              double x3, double y3,
                                              double x4, double y4,
                                              Curve4Points output)
        {
            // Trans. matrix Catmull-Rom to Bezier
            //
            //  0       1       0       0
            //  -1/6    1       1/6     0
            //  0       1/6     1       -1/6
            //  0       0       1       0
            //
            output.Set(
                 x2,
                 y2,
                (-x1 + 6 * x2 + x3) / 6,
                (-y1 + 6 * y2 + y3) / 6,
                (x2 + 6 * x3 - x4) / 6,
                (y2 + 6 * y3 - y4) / 6,
                x3,
                y3);
        }

        //-----------------------------------------------------ubspline_to_bezier
        public static void UbSplineToBezier(double x1, double y1,
                                                double x2, double y2,
                                                double x3, double y3,
                                                double x4, double y4,
                                                Curve4Points output)
        {
            // Trans. matrix Uniform BSpline to Bezier
            //
            //  1/6     4/6     1/6     0
            //  0       4/6     2/6     0
            //  0       2/6     4/6     0
            //  0       1/6     4/6     1/6
            //
            output.Set(
                (x1 + 4 * x2 + x3) / 6,
                (y1 + 4 * y2 + y3) / 6,
                (4 * x2 + 2 * x3) / 6,
                (4 * y2 + 2 * y3) / 6,
                (2 * x2 + 4 * x3) / 6,
                (2 * y2 + 4 * y3) / 6,
                (x2 + 4 * x3 + x4) / 6,
                (y2 + 4 * y3 + y4) / 6);
        }
        //------------------------------------------------------hermite_to_bezier
        public static void HermiteToBezier(double x1, double y1,
                                               double x2, double y2,
                                               double x3, double y3,
                                               double x4, double y4,
                                               Curve4Points output)
        {
            // Trans. matrix Hermite to Bezier
            //
            //  1       0       0       0
            //  1       0       1/3     0
            //  0       1       0       -1/3
            //  0       1       0       0
            //
            output.Set(
                x1,
                y1,
                (3 * x1 + x3) / 3,
                (3 * y1 + y3) / 3,
                (3 * x2 - x4) / 3,
                (3 * y2 - y4) / 3,
                x2,
                y2);
        }
    }
    //-------------------------------------------------------------curve4_points
    public sealed class Curve4Points
    {
        public double x0, y0, x1, y1, x2, y2, x3, y3;
        public void Set(double x0, double y0,
                      double x1, double y1,
                      double x2, double y2,
                      double x3, double y3)
        {
            this.x0 = x0; this.y0 = y0;
            this.x1 = x1; this.y1 = y1;
            this.x2 = x2; this.y2 = y2;
            this.x3 = x3; this.y3 = y3;
        }
    }
 

    public interface ICurveFlattenerOutput
    {
        void Append(double x, double y);
    }

    /// <summary>
    /// incremental curve flattener
    /// </summary>
    public sealed class CurveIncFlattener
    {
        double _scale;
        public CurveIncFlattener()
        {
            Reset();
        }

        public double ApproximationScale
        {
            get => _scale;
            set => _scale = value;
        }

        public bool UseFixedStepCount { get; set; }
        public int FixedStepCount { get; set; }

        public void Reset()
        {
            _scale = 1;
            UseFixedStepCount = false;
            FixedStepCount = 3;
        }

        //curve3_inc
        public void Flatten(double x0, double y0,
                  double x1, double y1,
                  double x2, double y2,
                  ICurveFlattenerOutput output, bool skipFirstPoint)
        {

            int _num_steps;

            if (UseFixedStepCount)
            {
                _num_steps = FixedStepCount;
            }
            else
            {
                //calculate 
                double dx1 = x1 - x0;
                double dy1 = y1 - y0;
                double dx2 = x2 - x1;
                double dy2 = y2 - y1;
                double len = Math.Sqrt(dx1 * dx1 + dy1 * dy1) + Math.Sqrt(dx2 * dx2 + dy2 * dy2);
                _num_steps = (int)AggMath.uround(len * 0.25 * _scale);

                if (_num_steps < 4)
                {
                    _num_steps = 4;
                }
            }


            double eachIncStep = 1.0 / _num_steps;
            double eachIncStep2 = eachIncStep * eachIncStep;
            double tmpx = (x0 - x1 * 2.0 + x2) * eachIncStep2;
            double tmpy = (y0 - y1 * 2.0 + y2) * eachIncStep2;
            double _fx = x0;
            double _fy = y0;
            double _dfx = tmpx + (x1 - x0) * (2.0 * eachIncStep);
            double _dfy = tmpy + (y1 - y0) * (2.0 * eachIncStep);
            double _ddfx = tmpx * 2.0;
            double _ddfy = tmpy * 2.0;


            //---------------
            //skip first step?
            //---------------
            if (!skipFirstPoint)
            {
                output.Append(x0, y0);
            }
            for (int i = 0; i < _num_steps; ++i)
            {
                _fx += _dfx;
                _fy += _dfy;
                _dfx += _ddfx;
                _dfy += _ddfy;

                output.Append(_fx, _fy);
            }

            //last point
            output.Append(x2, y2);
        }

        //curve4_inc
        public void Flatten(double x0, double y0,
                 double x1, double y1,
                 double x2, double y2,
                 double x3, double y3,
                 ICurveFlattenerOutput output,
                 bool skipFirstPoint)
        {
            int _num_steps = 0;

            if (UseFixedStepCount)
            {
                //use fixed step
                _num_steps = FixedStepCount;
            }
            else
            {
                //calculate step
                double dx1 = x1 - x0;
                double dy1 = y1 - y0;
                double dx2 = x2 - x1;
                double dy2 = y2 - y1;
                double dx3 = x3 - x2;
                double dy3 = y3 - y2;
                double len = (Math.Sqrt(dx1 * dx1 + dy1 * dy1) +
                              Math.Sqrt(dx2 * dx2 + dy2 * dy2) +
                              Math.Sqrt(dx3 * dx3 + dy3 * dy3)) * 0.25 * _scale;

                _num_steps = (int)AggMath.uround(len);
                if (_num_steps < 4)
                {
                    _num_steps = 4;
                }
            }


            double eachIncStep = 1.0 / _num_steps;
            double eachIncStep2 = eachIncStep * eachIncStep;
            double eachIncStep3 = eachIncStep * eachIncStep * eachIncStep;
            double pre1 = 3.0 * eachIncStep;
            double pre2 = 3.0 * eachIncStep2;
            double pre4 = 6.0 * eachIncStep2;
            double pre5 = 6.0 * eachIncStep3;
            double tmp1x = x0 - x1 * 2.0 + x2;
            double tmp1y = y0 - y1 * 2.0 + y2;
            double tmp2x = (x1 - x2) * 3.0 - x0 + x3;
            double tmp2y = (y1 - y2) * 3.0 - y0 + y3;
            double _fx = x0;
            double _fy = y0;
            double _dfx = (x1 - x0) * pre1 + tmp1x * pre2 + tmp2x * eachIncStep3;
            double _dfy = (y1 - y0) * pre1 + tmp1y * pre2 + tmp2y * eachIncStep3;
            double _ddfx = tmp1x * pre4 + tmp2x * pre5;
            double _ddfy = tmp1y * pre4 + tmp2y * pre5;
            double _dddfx = tmp2x * pre5;
            double _dddfy = tmp2y * pre5;

            //------------------------------------------------------------------------
            if (!skipFirstPoint)
            {
                output.Append(x0, y0);
            }
            //------------------------------------------------------------------------
            //skip first point?
            for (int i = 0; i < _num_steps; ++i)
            {
                _fx += _dfx;
                _fy += _dfy;
                _dfx += _ddfx;
                _dfy += _ddfy;
                _ddfx += _dddfx;
                _ddfy += _dddfy;

                output.Append(_fx, _fy);
            }
            output.Append(x3, y3);
        }
    }


    public sealed class CurveSubdivisionFlattener
    {
        double _approximation_scale;
        double _distance_tolerance_square;
        double _angle_tolerance;
        double _cusp_limit;

        byte _recursiveLimit;

        public CurveSubdivisionFlattener()
        {
            Reset();
        }
        public void Reset()
        {
            _approximation_scale = 1;
            _angle_tolerance = 1;
            _cusp_limit = 0;
            _recursiveLimit = 10;
        }
        public byte RecursiveLimit
        {
            get => _recursiveLimit;
            set
            {
                if (value > Curves.CURVE_RECURSION_LIMIT)
                {
                    value = Curves.CURVE_RECURSION_LIMIT;
                }
                _recursiveLimit = value;
            }
        }
        //-------------------------------------------------------------curve4_div
        /// <summary>
        /// Flatten Curve4
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="y0"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="x3"></param>
        /// <param name="y3"></param>
        /// <param name="output"></param>
        /// <param name="skipFirstPoint"></param>
        public void Flatten(double x0, double y0,
                  double x1, double y1,
                  double x2, double y2,
                  double x3, double y3,
                  ICurveFlattenerOutput output,
                  bool skipFirstPoint)
        {
            _distance_tolerance_square = 0.5 / _approximation_scale;
            _distance_tolerance_square *= _distance_tolerance_square;

            if (!skipFirstPoint)
            {
                output.Append(x0, y0);
            }
            AddRecursiveBezier(x0, y0, x1, y1, x2, y2, x3, y3, 0, output);
            output.Append(x3, y3);
        }

        public double ApproximationScale
        {
            get => _approximation_scale;
            set => _approximation_scale = value;
        }
        public double AngleTolerance
        {
            get => _angle_tolerance;
            set => _angle_tolerance = value;
        }

        public double CuspLimit
        {
            get => (_cusp_limit == 0.0) ? 0.0 : Math.PI - _cusp_limit;
            set => _cusp_limit = (value == 0.0) ? 0.0 : Math.PI - value;
        }

        void AddRecursiveBezier(double x0, double y0,
                              double x1, double y1,
                              double x2, double y2,
                              double x3, double y3,
                              int level, ICurveFlattenerOutput output)
        {
            //curve4
            //recursive
            if (level > _recursiveLimit)
            {
                return;
            }

            // Calculate all the mid-points of the line segments
            //----------------------
            double x01 = (x0 + x1) / 2;
            double y01 = (y0 + y1) / 2;
            double x12 = (x1 + x2) / 2;
            double y12 = (y1 + y2) / 2;
            double x23 = (x2 + x3) / 2;
            double y23 = (y2 + y3) / 2;
            double x012 = (x01 + x12) / 2;
            double y012 = (y01 + y12) / 2;
            double x123 = (x12 + x23) / 2;
            double y123 = (y12 + y23) / 2;
            double x0123 = (x012 + x123) / 2;
            double y0123 = (y012 + y123) / 2;
            // Try to approximate the full cubic curve by a single straight line
            //------------------
            double dx = x3 - x0;
            double dy = y3 - y0;
            double d2 = Math.Abs(((x1 - x3) * dy - (y1 - y3) * dx));
            double d3 = Math.Abs(((x2 - x3) * dy - (y2 - y3) * dx));
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
                        d2 = AggMath.calc_sq_distance(x0, y0, x1, y1);
                        d3 = AggMath.calc_sq_distance(x3, y3, x2, y2);
                    }
                    else
                    {
                        k = 1 / k;
                        da1 = x1 - x0;
                        da2 = y1 - y0;
                        d2 = k * (da1 * dx + da2 * dy);
                        da1 = x2 - x0;
                        da2 = y2 - y0;
                        d3 = k * (da1 * dx + da2 * dy);
                        if (d2 > 0 && d2 < 1 && d3 > 0 && d3 < 1)
                        {
                            // Simple collinear case, 1---2---3---4
                            // We can leave just two endpoints
                            return;
                        }
                        if (d2 <= 0) d2 = AggMath.calc_sq_distance(x1, y1, x0, y0);
                        else if (d2 >= 1) d2 = AggMath.calc_sq_distance(x1, y1, x3, y3);
                        else d2 = AggMath.calc_sq_distance(x1, y1, x0 + d2 * dx, y0 + d2 * dy);
                        if (d3 <= 0) d3 = AggMath.calc_sq_distance(x2, y2, x0, y0);
                        else if (d3 >= 1) d3 = AggMath.calc_sq_distance(x2, y2, x3, y3);
                        else d3 = AggMath.calc_sq_distance(x2, y2, x0 + d3 * dx, y0 + d3 * dy);
                    }
                    if (d2 > d3)
                    {
                        if (d2 < _distance_tolerance_square)
                        {
                            output.Append(x1, y1);
                            return;
                        }
                    }
                    else
                    {
                        if (d3 < _distance_tolerance_square)
                        {
                            output.Append(x2, y2);
                            return;
                        }
                    }
                    break;
                case 1:
                    // p1,p2,p4 are collinear, p3 is significant
                    //----------------------
                    if (d3 * d3 <= _distance_tolerance_square * (dx * dx + dy * dy))
                    {
                        if (_angle_tolerance < Curves.CURVE_ANGLE_TOLERANCE_EPSILON)
                        {
                            output.Append(x12, y12);
                            return;
                        }

                        // Angle Condition
                        //----------------------
                        da1 = Math.Abs(Math.Atan2(y3 - y2, x3 - x2) - Math.Atan2(y2 - y1, x2 - x1));
                        if (da1 >= Math.PI) da1 = 2 * Math.PI - da1;
                        if (da1 < _angle_tolerance)
                        {
                            output.Append(x1, y1);
                            output.Append(x2, y2);
                            return;
                        }

                        if (_cusp_limit != 0.0)
                        {
                            if (da1 > _cusp_limit)
                            {
                                output.Append(x2, y2);
                                return;
                            }
                        }
                    }
                    break;
                case 2:
                    // p1,p3,p4 are collinear, p2 is significant
                    //----------------------
                    if (d2 * d2 <= _distance_tolerance_square * (dx * dx + dy * dy))
                    {
                        if (_angle_tolerance < Curves.CURVE_ANGLE_TOLERANCE_EPSILON)
                        {
                            output.Append(x12, y12);
                            return;
                        }

                        // Angle Condition
                        //----------------------
                        da1 = Math.Abs(Math.Atan2(y2 - y1, x2 - x1) - Math.Atan2(y1 - y0, x1 - x0));
                        if (da1 >= Math.PI) da1 = 2 * Math.PI - da1;
                        if (da1 < _angle_tolerance)
                        {
                            output.Append(x1, y1);
                            output.Append(x2, y2);
                            return;
                        }

                        if (_cusp_limit != 0.0)
                        {
                            if (da1 > _cusp_limit)
                            {
                                output.Append(x1, y1);
                                return;
                            }
                        }
                    }
                    break;
                case 3:
                    // Regular case
                    //-----------------
                    if ((d2 + d3) * (d2 + d3) <= _distance_tolerance_square * (dx * dx + dy * dy))
                    {
                        // If the curvature doesn't exceed the distance_tolerance value
                        // we tend to finish subdivisions.
                        //----------------------
                        if (_angle_tolerance < Curves.CURVE_ANGLE_TOLERANCE_EPSILON)
                        {
                            output.Append(x12, y12);
                            return;
                        }

                        // Angle & Cusp Condition
                        //----------------------
                        k = Math.Atan2(y2 - y1, x2 - x1);
                        da1 = Math.Abs(k - Math.Atan2(y1 - y0, x1 - x0));
                        da2 = Math.Abs(Math.Atan2(y3 - y2, x3 - x2) - k);
                        if (da1 >= Math.PI) da1 = 2 * Math.PI - da1;
                        if (da2 >= Math.PI) da2 = 2 * Math.PI - da2;
                        if (da1 + da2 < _angle_tolerance)
                        {
                            // Finally we can stop the recursion
                            //----------------------
                            output.Append(x12, y12);
                            return;
                        }

                        if (_cusp_limit != 0.0)
                        {
                            if (da1 > _cusp_limit)
                            {
                                output.Append(x1, y1);
                                return;
                            }

                            if (da2 > _cusp_limit)
                            {
                                output.Append(x2, y2);
                                return;
                            }
                        }
                    }
                    break;
            }

            // Continue subdivision
            //----------------------
            AddRecursiveBezier(x0, y0, x01, y01, x012, y012, x0123, y0123, level + 1, output);
            AddRecursiveBezier(x0123, y0123, x123, y123, x23, y23, x3, y3, level + 1, output);
        }
        //-------------------------------------------------------------------
        //curve3_div
        /// <summary>
        /// Flatten Curve3
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="y0"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="output"></param>
        /// <param name="skipFirstPoint"></param>
        public void Flatten(double x0, double y0,
             double x1, double y1,
             double x2, double y2,
             ICurveFlattenerOutput output,
             bool skipFirstPoint)
        {
            _distance_tolerance_square = 0.5 / _approximation_scale;
            _distance_tolerance_square *= _distance_tolerance_square;

            //
            if (!skipFirstPoint)
            {
                output.Append(x0, y0);
            }
            AddRecursiveBezier(x0, y0, x1, y1, x2, y2, 0, output);
            output.Append(x2, y2);
        }

        void AddRecursiveBezier(double x0, double y0,
                               double x1, double y1,
                               double x2, double y2,
                               int level,
                               ICurveFlattenerOutput output)
        {
            //curve3
            if (level > _recursiveLimit)
            {
                return;
            }

            // Calculate all the mid-points of the line segments
            //----------------------
            double x01 = (x0 + x1) / 2;
            double y01 = (y0 + y1) / 2;
            double x12 = (x1 + x2) / 2;
            double y12 = (y1 + y2) / 2;
            double x012 = (x01 + x12) / 2;
            double y012 = (y01 + y12) / 2;
            double dx = x2 - x0;
            double dy = y2 - y0;

            double d = Math.Abs(((x1 - x2) * dy - (y1 - y2) * dx));
           
            if (d > Curves.CURVE_COLLINEARITY_EPSILON)
            {
                // Regular case
                //-----------------
                if (d * d <= _distance_tolerance_square * (dx * dx + dy * dy))
                {
                    // If the curvature doesn't exceed the distance_tolerance value
                    // we tend to finish subdivisions.
                    //----------------------
                    if (_angle_tolerance < Curves.CURVE_ANGLE_TOLERANCE_EPSILON)
                    {
                        output.Append(x012, y012);
                        return;
                    }

                    // Angle & Cusp Condition
                    //----------------------
                    double da = Math.Abs(Math.Atan2(y2 - y1, x2 - x1) - Math.Atan2(y1 - y0, x1 - x0));
                    if (da >= Math.PI) da = 2 * Math.PI - da;
                    if (da < _angle_tolerance)
                    {
                        // Finally we can stop the recursion
                        //----------------------
                        output.Append(x012, y012);
                        return;
                    }
                }
            }
            else
            {
                // Collinear case
                //------------------
                double da = dx * dx + dy * dy;
                if (da == 0)
                {
                    d = AggMath.calc_sq_distance(x0, y0, x1, y1);
                }
                else
                {
                    d = ((x1 - x0) * dx + (y1 - y0) * dy) / da;
                    if (d > 0 && d < 1)
                    {
                        // Simple collinear case, 1---2---3
                        // We can leave just two endpoints
                        return;
                    }
                    if (d <= 0) d = AggMath.calc_sq_distance(x1, y1, x0, y0);
                    else if (d >= 1) d = AggMath.calc_sq_distance(x1, y1, x2, y2);
                    else d = AggMath.calc_sq_distance(x1, y1, x0 + d * dx, y0 + d * dy);
                }
                if (d < _distance_tolerance_square)
                {
                    output.Append(x1, y1);
                    return;
                }
            }

            // Continue subdivision
            //----------------------
            AddRecursiveBezier(x0, y0, x01, y01, x012, y012, level + 1, output);
            AddRecursiveBezier(x012, y012, x12, y12, x2, y2, level + 1, output);
        }


    }


}