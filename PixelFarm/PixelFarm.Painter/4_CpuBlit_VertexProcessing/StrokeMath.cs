//BSD, 2014-present, WinterDev

//----------------------------------------------------------------------------
// Anti-Grain Geometry - Version 2.4
// Copyright (C) 2002-2005 Maxim Shemanarev (http://www.antigrain.com)
//
// C# Port port by: Lars Brubaker
//                  larsbrubaker@gmail.com
// Copyright (C) 2007
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
//
// Stroke math
//
//----------------------------------------------------------------------------
using System;
using PixelFarm.Drawing; 

namespace PixelFarm.CpuBlit.VertexProcessing
{
    struct Vertex2d
    {
        public readonly double x;
        public readonly double y;
        public Vertex2d(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
        public double CalLen(Vertex2d another)
        {
            return AggMath.calc_distance(x, y, another.x, another.y);
        }
        public bool IsEqual(Vertex2d val)
        {
            return AggMath.calc_distance(x, y, val.x, val.y) <= AggMath.VERTEX_DISTANCE_EPSILON;
            //if ((dist = AggMath.calc_distance(x, y, val.x, val.y)) > AggMath.VERTEX_DISTANCE_EPSILON)
            //{
            //    //diff enough=> this is NOT equal with val
            //    return false;
            //}
            //else
            //{
            //    //not diff enough => this is equal (with val)
            //    dist = 1.0 / AggMath.VERTEX_DISTANCE_EPSILON;
            //    return true;
            //}
        }
#if DEBUG
        public override string ToString()
        {
            return "(" + x + "," + y + ")";
        }
#endif
    }


    class StrokeMath
    {

        double m_width;
        double m_width_abs;
        double m_width_eps;
        int m_width_sign;
        double m_miter_limit;
        double m_inner_miter_limit;
        double m_approx_scale;
        LineCap m_line_cap;
        LineJoin m_line_join;
        InnerJoin m_inner_join;
        public StrokeMath()
        {
            m_width = 0.5;
            m_width_abs = 0.5;
            m_width_eps = 0.5 / 1024.0;
            m_width_sign = 1;
            m_miter_limit = 4.0;
            m_inner_miter_limit = 1.01;
            m_approx_scale = 1.0;
            m_line_cap = LineCap.Butt;
            m_line_join = LineJoin.Miter;
            m_inner_join = InnerJoin.Miter;
        }

        public LineCap LineCap
        {
            get { return this.m_line_cap; }
            set { this.m_line_cap = value; }
        }
        public LineJoin LineJoin
        {
            get { return this.m_line_join; }
            set { this.m_line_join = value; }
        }
        public InnerJoin InnerJoin
        {
            get { return this.m_inner_join; }
            set { this.m_inner_join = value; }
        }

        public double Width
        {
            get { return this.m_width * 2.0; }
            set
            {
                m_width = value / 2.0;
                if (m_width < 0)
                {
                    m_width_abs = -m_width;
                    m_width_sign = -1;
                }
                else
                {
                    m_width_abs = m_width;
                    m_width_sign = 1;
                }
                m_width_eps = m_width / 1024.0;
            }
        }



        public double MiterLimit
        {
            get { return this.m_miter_limit; }
            set { this.m_miter_limit = value; }
        }
        public void SetMiterLimitTheta(double t)
        {
            m_miter_limit = 1.0 / Math.Sin(t * 0.5);
        }



        public double InnerMiterLimit
        {
            get { return this.m_inner_miter_limit; }
            set { this.m_inner_miter_limit = value; }
        }


        public double ApproximateScale
        {
            get { return this.m_approx_scale; }
            set { this.m_approx_scale = value; }
        }

        public void CreateCap(VertexStore output, Vertex2d v0, Vertex2d v1, double len)
        {
            output.Clear();
            double dx1 = (v1.y - v0.y) / len;
            double dy1 = (v1.x - v0.x) / len;
            double dx2 = 0;
            double dy2 = 0;
            dx1 *= m_width;
            dy1 *= m_width;
            if (m_line_cap != LineCap.Round)
            {
                if (m_line_cap == LineCap.Square)
                {
                    dx2 = dy1 * m_width_sign;
                    dy2 = dx1 * m_width_sign;
                }
                AddVertex(output, v0.x - dx1 - dx2, v0.y + dy1 - dy2);
                AddVertex(output, v0.x + dx1 - dx2, v0.y - dy1 - dy2);
            }
            else
            {
                //round cap
                double da = Math.Acos(m_width_abs / (m_width_abs + 0.125 / m_approx_scale)) * 2;
                double a1;
                int i;
                int n = (int)(Math.PI / da);
                da = Math.PI / (n + 1);
                AddVertex(output, v0.x - dx1, v0.y + dy1);
                if (m_width_sign > 0)
                {
                    a1 = Math.Atan2(dy1, -dx1);
                    a1 += da;
                    for (i = 0; i < n; i++)
                    {
                        AddVertex(output, v0.x + Math.Cos(a1) * m_width,
                                       v0.y + Math.Sin(a1) * m_width);
                        a1 += da;
                    }
                }
                else
                {
                    a1 = Math.Atan2(-dy1, dx1);
                    a1 -= da;
                    for (i = 0; i < n; i++)
                    {
                        AddVertex(output, v0.x + Math.Cos(a1) * m_width,
                                       v0.y + Math.Sin(a1) * m_width);
                        a1 -= da;
                    }
                }
                AddVertex(output, v0.x + dx1, v0.y - dy1);
            }
        }

        public void CreateJoin(VertexStore output,
                               Vertex2d v0,
                               Vertex2d v1,
                               Vertex2d v2,
                               double len1,
                               double len2)
        {
            double dx1 = m_width * (v1.y - v0.y) / len1;
            double dy1 = m_width * (v1.x - v0.x) / len1;
            double dx2 = m_width * (v2.y - v1.y) / len2;
            double dy2 = m_width * (v2.x - v1.x) / len2;
            output.Clear();
            double cp = AggMath.Cross(v0.x, v0.y, v1.x, v1.y, v2.x, v2.y);
            if (cp != 0 && (cp > 0) == (m_width > 0))
            {
                // Inner join
                //---------------
                double limit = ((len1 < len2) ? len1 : len2) / m_width_abs;
                if (limit < m_inner_miter_limit)
                {
                    limit = m_inner_miter_limit;
                }

                switch (m_inner_join)
                {
                    default: // inner_bevel
                        AddVertex(output, v1.x + dx1, v1.y - dy1);
                        AddVertex(output, v1.x + dx2, v1.y - dy2);
                        break;
                    case InnerJoin.Miter:
                        CreateMiter(output,
                                   v0, v1, v2, dx1, dy1, dx2, dy2,
                                   LineJoin.MiterRevert,
                                   limit, 0);
                        break;
                    case InnerJoin.Jag:
                    case InnerJoin.Round:
                        cp = (dx1 - dx2) * (dx1 - dx2) + (dy1 - dy2) * (dy1 - dy2);
                        if (cp < len1 * len1 && cp < len2 * len2)
                        {
                            CreateMiter(output,
                                       v0, v1, v2, dx1, dy1, dx2, dy2,
                                       LineJoin.MiterRevert,
                                       limit, 0);
                        }
                        else
                        {
                            if (m_inner_join == InnerJoin.Jag)
                            {
                                AddVertex(output, v1.x + dx1, v1.y - dy1);
                                AddVertex(output, v1.x, v1.y);
                                AddVertex(output, v1.x + dx2, v1.y - dy2);
                            }
                            else
                            {
                                AddVertex(output, v1.x + dx1, v1.y - dy1);
                                AddVertex(output, v1.x, v1.y);
                                CreateArc(output, v1.x, v1.y, dx2, -dy2, dx1, -dy1);
                                AddVertex(output, v1.x, v1.y);
                                AddVertex(output, v1.x + dx2, v1.y - dy2);
                            }
                        }
                        break;
                }
            }
            else
            {
                // Outer join
                //---------------

                // Calculate the distance between v1 and 
                // the central point of the bevel line segment
                //---------------
                double dx = (dx1 + dx2) / 2;
                double dy = (dy1 + dy2) / 2;
                double dbevel = Math.Sqrt(dx * dx + dy * dy);
                if (m_line_join == LineJoin.Round || m_line_join == LineJoin.Bevel)
                {
                    // This is an optimization that reduces the number of points 
                    // in cases of almost collinear segments. If there's no
                    // visible difference between bevel and miter joins we'd rather
                    // use miter join because it adds only one point instead of two. 
                    //
                    // Here we calculate the middle point between the bevel points 
                    // and then, the distance between v1 and this middle point. 
                    // At outer joins this distance always less than stroke width, 
                    // because it's actually the height of an isosceles triangle of
                    // v1 and its two bevel points. If the difference between this
                    // width and this value is small (no visible bevel) we can 
                    // add just one point. 
                    //
                    // The constant in the expression makes the result approximately 
                    // the same as in round joins and caps. You can safely comment 
                    // out this entire "if".
                    //-------------------
                    if (m_approx_scale * (m_width_abs - dbevel) < m_width_eps)
                    {
                        if (AggMath.CalcIntersect(v0.x + dx1, v0.y - dy1,
                                             v1.x + dx1, v1.y - dy1,
                                             v1.x + dx2, v1.y - dy2,
                                             v2.x + dx2, v2.y - dy2,
                                             out dx, out dy))
                        {
                            AddVertex(output, dx, dy);
                        }
                        else
                        {
                            AddVertex(output, v1.x + dx1, v1.y - dy1);
                        }
                        return;
                    }
                }

                switch (m_line_join)
                {
                    case LineJoin.Miter:
                    case LineJoin.MiterRevert:
                    case LineJoin.MiterRound:
                        CreateMiter(output,
                                   v0, v1, v2, dx1, dy1, dx2, dy2,
                                   m_line_join,
                                   m_miter_limit,
                                   dbevel);
                        break;
                    case LineJoin.Round:
                        CreateArc(output, v1.x, v1.y, dx1, -dy1, dx2, -dy2);
                        break;
                    default: // Bevel join 
                        AddVertex(output, v1.x + dx1, v1.y - dy1);
                        AddVertex(output, v1.x + dx2, v1.y - dy2);
                        break;
                }
            }
        }
        static void AddVertex(VertexStore output, double x, double y)
        {
            output.AddVertex(x, y, VertexCmd.LineTo);
        }

        void CreateArc(VertexStore output,
                      double x, double y,
                      double dx1, double dy1,
                      double dx2, double dy2)
        {
            double a1 = Math.Atan2(dy1 * m_width_sign, dx1 * m_width_sign);
            double a2 = Math.Atan2(dy2 * m_width_sign, dx2 * m_width_sign);
            double da = a1 - a2;
            int i, n;
            da = Math.Acos(m_width_abs / (m_width_abs + 0.125 / m_approx_scale)) * 2;
            AddVertex(output, x + dx1, y + dy1);
            if (m_width_sign > 0)
            {
                if (a1 > a2) a2 += 2 * Math.PI;
                n = (int)((a2 - a1) / da);
                da = (a2 - a1) / (n + 1);
                a1 += da;
                for (i = 0; i < n; i++)
                {
                    AddVertex(output, x + Math.Cos(a1) * m_width, y + Math.Sin(a1) * m_width);
                    a1 += da;
                }
            }
            else
            {
                if (a1 < a2) a2 -= 2 * Math.PI;
                n = (int)((a1 - a2) / da);
                da = (a1 - a2) / (n + 1);
                a1 -= da;
                for (i = 0; i < n; i++)
                {
                    AddVertex(output, x + Math.Cos(a1) * m_width, y + Math.Sin(a1) * m_width);
                    a1 -= da;
                }
            }
            AddVertex(output, x + dx2, y + dy2);
        }

        void CreateMiter(VertexStore output,
                        Vertex2d v0,
                        Vertex2d v1,
                        Vertex2d v2,
                        double dx1, double dy1,
                        double dx2, double dy2,
                        LineJoin lj,
                        double mlimit,
                        double dbevel)
        {
            double xi = v1.x;
            double yi = v1.y;
            double di = 1;
            double lim = m_width_abs * mlimit;
            bool miter_limit_exceeded = true; // Assume the worst
            bool intersection_failed = true; // Assume the worst
            if (AggMath.CalcIntersect(v0.x + dx1, v0.y - dy1,
                                 v1.x + dx1, v1.y - dy1,
                                 v1.x + dx2, v1.y - dy2,
                                 v2.x + dx2, v2.y - dy2,
                                 out xi, out yi))
            {
                // Calculation of the intersection succeeded
                //---------------------
                di = AggMath.calc_distance(v1.x, v1.y, xi, yi);
                if (di <= lim)
                {
                    // Inside the miter limit
                    //---------------------
                    AddVertex(output, xi, yi);
                    miter_limit_exceeded = false;
                }
                intersection_failed = false;
            }
            else
            {
                // Calculation of the intersection failed, most probably
                // the three points lie one straight line. 
                // First check if v0 and v2 lie on the opposite sides of vector: 
                // (v1.x, v1.y) -> (v1.x+dx1, v1.y-dy1), that is, the perpendicular
                // to the line determined by vertices v0 and v1.
                // This condition determines whether the next line segments continues
                // the previous one or goes back.
                //----------------
                double x2 = v1.x + dx1;
                double y2 = v1.y - dy1;
                if ((AggMath.Cross(v0.x, v0.y, v1.x, v1.y, x2, y2) < 0.0) ==
                   (AggMath.Cross(v1.x, v1.y, v2.x, v2.y, x2, y2) < 0.0))
                {
                    // This case means that the next segment continues 
                    // the previous one (straight line)
                    //-----------------
                    AddVertex(output, v1.x + dx1, v1.y - dy1);
                    miter_limit_exceeded = false;
                }
            }

            if (miter_limit_exceeded)
            {
                // Miter limit exceeded
                //------------------------
                switch (lj)
                {
                    case LineJoin.MiterRevert:
                        // For the compatibility with SVG, PDF, etc, 
                        // we use a simple bevel join instead of
                        // "smart" bevel
                        //-------------------
                        AddVertex(output, v1.x + dx1, v1.y - dy1);
                        AddVertex(output, v1.x + dx2, v1.y - dy2);
                        break;
                    case LineJoin.MiterRound:
                        CreateArc(output, v1.x, v1.y, dx1, -dy1, dx2, -dy2);
                        break;
                    default:
                        // If no miter-revert, calculate new dx1, dy1, dx2, dy2
                        //----------------
                        if (intersection_failed)
                        {
                            mlimit *= m_width_sign;
                            AddVertex(output, v1.x + dx1 + dy1 * mlimit,
                                           v1.y - dy1 + dx1 * mlimit);
                            AddVertex(output, v1.x + dx2 - dy2 * mlimit,
                                           v1.y - dy2 - dx2 * mlimit);
                        }
                        else
                        {
                            double x1 = v1.x + dx1;
                            double y1 = v1.y - dy1;
                            double x2 = v1.x + dx2;
                            double y2 = v1.y - dy2;
                            di = (lim - dbevel) / (di - dbevel);
                            AddVertex(output, x1 + (xi - x1) * di,
                                           y1 + (yi - y1) * di);
                            AddVertex(output, x2 + (xi - x2) * di,
                                           y2 + (yi - y2) * di);
                        }
                        break;
                }
            }
        }
    }
}