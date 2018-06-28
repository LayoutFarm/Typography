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

using PixelFarm.VectorMath;
using PixelFarm.Drawing;
using PixelFarm.CpuBlit.VertexProcessing;
namespace PixelFarm.CpuBlit
{


    //https://en.wikipedia.org/wiki/B%C3%A9zier_curve
    //--------------------
    //Line, has 2 points..
    //  (x0,y0) begin point
    //  (x1,y1) end point
    //--------------------
    //Curve3 (Quadratic Bézier curves), has 3 points
    //  (x0,y0)  begin point
    //  (x1,y1)  1st control point 
    //  (x2,y2)  end point
    //--------------------
    //Curve4 (Cubic  Bézier curves), has 4 points
    //  (x0,y0)  begin point
    //  (x1,y1)  1st control point 
    //  (x2,y2)  2nd control point
    //  (x3,y3)  end point    
    //-------------------- 
    //please note that TrueType font
    //compose of Quadractic Bezier Curve ***
    //--------------------- 


    //---------------------------------------------------------------path_base
    // A container to store vertices with their flags. 
    // A path consists of a number of contours separated with "move_to" 
    // commands. The path storage can keep and maintain more than one
    // path. 
    // To navigate to the beginning of a particular path, use rewind(path_id);
    // Where path_id is what start_new_path() returns. So, when you call
    // start_new_path() you need to store its return value somewhere else
    // to navigate to the path afterwards.
    //
    // See also: vertex_source concept
    //------------------------------------------------------------------------ 

    public enum SvgPathCommand : byte
    {
        MoveTo,
        LineTo,
        HorizontalLineTo,
        VerticalLineTo,
        CurveTo,
        SmoothCurveTo,
        QuadraticBezierCurve,
        TSmoothQuadraticBezierCurveTo,
        Arc,
        ZClosePath
    }


    /// <summary>
    /// forward path writer
    /// </summary>
    public sealed class PathWriter
    {

        double latest_moveTo_X;
        double latest_moveTo_Y;

        /// <summary>
        /// latest X
        /// </summary>
        double latest_x;
        /// <summary>
        /// latest Y
        /// </summary>
        double latest_y;

        /// <summary>
        /// 1st curve control point
        /// </summary>
        Vector2 c1;
        /// <summary>
        /// 2nd curve control point 
        /// </summary>
        Vector2 c2;
        //
        SvgPathCommand latestSVGPathCmd;
        int figureCount = 0;
        VertexStore myvxs;
        public PathWriter()
        {
            myvxs = new VertexStore();
        }
        public PathWriter(VertexStore externalVxs)
        {
            myvxs = externalVxs;
        }
        public int Count
        {
            get { return myvxs.Count; }
        }
        public void Clear()
        {
            myvxs.Clear();
            latest_moveTo_X = latest_moveTo_Y = latest_x = latest_y = 0;
            c1 = new Vector2();
            c2 = new Vector2();
            latestSVGPathCmd = SvgPathCommand.MoveTo;
            figureCount = 0;
        }
         
        public void ResetWithExternalVxs(VertexStore newVxsOutput)
        {   
            myvxs = newVxsOutput;
            Clear();
        }
        //-------------------------------------------------------------------
        public double LastMoveX { get { return this.latest_moveTo_X; } }
        public double LastMoveY { get { return this.latest_moveTo_Y; } }
        //-------------------------------------------------------------------

        public int StartFigure()
        {
            if (figureCount > 0)
            {
                //TODO: review here***
                //NoMore cmd ?
                myvxs.AddVertex(0, 0, VertexCmd.NoMore);
            }
            figureCount++;
            return myvxs.Count;
        }
        public void Stop()
        {
            //TODO: review stop command again
            myvxs.AddVertex(0, 0, VertexCmd.NoMore);
        }
        //--------------------------------------------------------------------
        public void MoveTo(double x0, double y0)
        {
            this.latestSVGPathCmd = SvgPathCommand.MoveTo;
            myvxs.AddMoveTo(
                this.latest_moveTo_X = this.latest_x = x0,
                this.latest_moveTo_Y = this.latest_y = y0);
        }
        public void MoveToRel(double dx0, double dy0)
        {
            //*** move to, relative to last(x,y) ***
            this.latestSVGPathCmd = SvgPathCommand.MoveTo;
            myvxs.AddMoveTo(
                this.latest_moveTo_X = (this.latest_x += dx0),
                this.latest_moveTo_Y = (this.latest_y += dy0));
        }
        public void LineTo(double x1, double y1)
        {
            this.latestSVGPathCmd = SvgPathCommand.LineTo;
            myvxs.AddLineTo(this.latest_x = x1, this.latest_y = y1);
        }
        public void LineToRel(double dx1, double dy1)
        {
            //*** line to to, relative to last(x,y) ***
            this.latestSVGPathCmd = SvgPathCommand.LineTo;
            myvxs.AddLineTo(
                this.latest_x += dx1,
                this.latest_y += dy1);
        }
        public void HorizontalLineTo(double x1)
        {
            this.latestSVGPathCmd = SvgPathCommand.HorizontalLineTo;
            myvxs.AddLineTo(this.latest_x = x1, latest_y);
        }
        public void HorizontalLineToRel(double dx1)
        {
            //relative ***
            this.latestSVGPathCmd = SvgPathCommand.HorizontalLineTo;
            myvxs.AddLineTo(this.latest_x += dx1, latest_y);
        }
        public void VerticalLineTo(double y1)
        {
            this.latestSVGPathCmd = SvgPathCommand.VerticalLineTo;
            myvxs.AddLineTo(latest_x, this.latest_y = y1);
        }
        public void VerticalLineToRel(double dy1)
        {
            //relative ***
            this.latestSVGPathCmd = SvgPathCommand.VerticalLineTo;
            myvxs.AddLineTo(latest_x, this.latest_y += dy1);
        }

        /// <summary>
        ///  Draws a quadratic Bezier curve from the current point to (x,y) using (xControl,yControl) as the control point.
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        public void Curve3(double x1, double y1, double x2, double y2)
        {
            this.latestSVGPathCmd = SvgPathCommand.QuadraticBezierCurve;
            this.c1.x = x1;
            this.c1.y = y1;
            myvxs.AddP2c(x1, y1);
            myvxs.AddLineTo(this.latest_x = x2, this.latest_y = y2);
        }
        /// <summary>
        /// Draws a quadratic Bezier curve from the current point to (x,y) using (xControl,yControl) as the control point.
        /// </summary>
        /// <param name="dx1"></param>
        /// <param name="dy1"></param>
        /// <param name="dx2"></param>
        /// <param name="dy2"></param>
        public void Curve3Rel(double dx1, double dy1, double dx2, double dy2)
        {
            this.latestSVGPathCmd = SvgPathCommand.QuadraticBezierCurve;
            this.c1.x = this.latest_x + dx1;
            this.c1.y = this.latest_y + dy1;
            myvxs.AddP2c(this.latest_x + dx1, this.latest_y + dy1);
            myvxs.AddLineTo(this.latest_x += dx2, this.latest_y += dy2);
        }

        /// <summary> 
        /// <para>Draws a quadratic Bezier curve from the current point to (x,y).</para>
        /// <para>The control point is assumed to be the reflection of the control point on the previous command relative to the current point.</para>
        /// <para>(If there is no previous command or if the previous command was not a curve, assume the control point is coincident with the current point.)</para>
        /// </summary>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        public void SmoothCurve3(double x2, double y2)
        {
            switch (this.latestSVGPathCmd)
            {
                case SvgPathCommand.QuadraticBezierCurve:
                case SvgPathCommand.TSmoothQuadraticBezierCurveTo:
                    {
                        //curve3,
                        //create new c1 from current c1
                        Vector2 new_c1 = CreateMirrorPoint(this.c1, new Vector2(this.latest_x, this.latest_y));
                        Curve3(new_c1.X, new_c1.Y, x2, y2);
                    }
                    break;
                case SvgPathCommand.CurveTo:
                case SvgPathCommand.SmoothCurveTo:
                    {
                        //curve4,
                        //create new c1 from current c2
                        Vector2 new_c1 = CreateMirrorPoint(this.c2, new Vector2(this.latest_x, this.latest_y));
                        Curve3(new_c1.X, new_c1.Y, x2, y2);
                    }
                    break;
                default:
                    {
                        Curve3(this.latest_x, this.latest_y, x2, y2);
                    }
                    break;
            }
        }

        //-------------------------------------------------------------------
        static Vector2 CreateMirrorPoint(Vector2 mirrorPoint, Vector2 fixedPoint)
        {
            return new Vector2(
                fixedPoint.X - (mirrorPoint.X - fixedPoint.X),
                fixedPoint.Y - (mirrorPoint.Y - fixedPoint.Y));
        }
        //--------------------------------------------------------------------
        /// <summary>
        /// <para>Draws a quadratic Bezier curve from the current point to (x,y).</para>
        /// <para>The control point is assumed to be the reflection of the control point on the previous command relative to the current point.</para>
        /// <para>(If there is no previous command or if the previous command was not a curve, assume the control point is coincident with the current point.)</para>
        /// </summary>
        /// <param name="dx1"></param>
        /// <param name="dy1"></param>
        public void SmoothCurve3Rel(double dx1, double dy1)
        {
            this.SmoothCurve3(this.latest_x + dx1, this.latest_y + dy1);
        }
        //-----------------------------------------------------------------------
        public void Curve4(double x1, double y1,
                           double x2, double y2,
                           double x3, double y3)
        {
            this.latestSVGPathCmd = SvgPathCommand.CurveTo;
            myvxs.AddP3c(x1, y1);
            myvxs.AddP3c(x2, y2);
            this.c2 = new Vector2(x2, y2);
            myvxs.AddLineTo(this.latest_x = x3, this.latest_y = y3);
        }

        public void Curve4Rel(double dx1, double dy1,
                              double dx2, double dy2,
                              double dx3, double dy3)
        {
            this.latestSVGPathCmd = SvgPathCommand.CurveTo;
            myvxs.AddP3c(this.latest_x + dx1, this.latest_y + dy1);
            myvxs.AddP3c(this.latest_x + dx2, this.latest_y + dy2);
            this.c2 = new Vector2(this.latest_x + dx2, this.latest_y + dy2);

            myvxs.AddLineTo(this.latest_x += dx3, this.latest_y += dy3);
        }

        //--------------------------------------------------------------------
        public void SmoothCurve4(
                       double x2, double y2,
                       double x3, double y3)
        {
            switch (this.latestSVGPathCmd)
            {

                case SvgPathCommand.QuadraticBezierCurve:
                case SvgPathCommand.TSmoothQuadraticBezierCurveTo:
                    {
                        //create new c1 from current c1
                        Vector2 new_c1 = CreateMirrorPoint(this.c1, new Vector2(this.latest_x, this.latest_y));
                        Curve4(new_c1.X, new_c1.Y, x2, y2, x3, y3);
                    }
                    break;
                case SvgPathCommand.CurveTo:
                case SvgPathCommand.SmoothCurveTo:
                    {
                        //create new c1 from current c2
                        Vector2 new_c1 = CreateMirrorPoint(this.c2, new Vector2(this.latest_x, this.latest_y));
                        Curve4(new_c1.X, new_c1.Y, x2, y2, x3, y3);
                    }
                    break;
                default:
                    {
                        Curve4(this.latest_x, this.latest_y, x2, y2, x3, y3);
                    }
                    break;
            }
        }

        public void SmoothCurve4Rel(double dx2, double dy2,
                                    double dx3, double dy3)
        {
            //relative version
            SmoothCurve4(this.latest_x + dx2, this.latest_y + dy2, this.latest_x + dx3, this.latest_y + dy3);
        }

        //=======================================================================
        //TODO: implement arc to ***
        /*
        public void arc_to(double rx, double ry,
                               double angle,
                               bool large_arc_flag,
                               bool sweep_flag,
                               double x, double y)
        {
        if(m_vertices.total_vertices() && is_vertex(m_vertices.last_command()))
        {
            double epsilon = 1e-30;
            double x0 = 0.0;
            double y0 = 0.0;
            m_vertices.last_vertex(&x0, &y0);

            rx = fabs(rx);
            ry = fabs(ry);

            // Ensure radii are valid
            //-------------------------
            if(rx < epsilon || ry < epsilon) 
            {
                line_to(x, y);
                return;
            }

            if(calc_distance(x0, y0, x, y) < epsilon)
            {
                // If the endpoints (x, y) and (x0, y0) are identical, then this
                // is equivalent to omitting the elliptical arc segment entirely.
                return;
            }
            bezier_arc_svg a(x0, y0, rx, ry, angle, large_arc_flag, sweep_flag, x, y);
            if(a.radii_ok())
            {
                join_path(a);
            }
            else
            {
                line_to(x, y);
            }
        }
        else
        {
            move_to(x, y);
        }
    } 
    public void arc_rel(double rx, double ry,
                                double angle,
                                bool large_arc_flag,
                                bool sweep_flag,
                                double dx, double dy)
    {
        rel_to_abs(&dx, &dy);
        arc_to(rx, ry, angle, large_arc_flag, sweep_flag, dx, dy);
    }
     */
        //=======================================================================


        public VertexStore Vxs
        {
            get { return this.myvxs; }
        }
        public VertexStoreSnap MakeVertexSnap()
        {
            return new VertexStoreSnap(this.myvxs);
        }

        VertexCmd GetLastVertex(out double x, out double y)
        {
            return myvxs.GetLastVertex(out x, out y);
        }

        public void CloseFigureCCW()
        {
            if (VertexHelper.IsVertextCommand(myvxs.GetLastCommand()))
            {
                myvxs.AddVertex((int)EndVertexOrientation.CCW, 0, VertexCmd.Close);
            }
        }
        public void CloseFigure()
        {
            if (VertexHelper.IsVertextCommand(myvxs.GetLastCommand()))
            {
                myvxs.AddVertex(0, 0, VertexCmd.Close);
            }
        }
        public void EndGroup()
        {
            if (VertexHelper.IsCloseOrEnd(myvxs.GetLastCommand()))
            {
                myvxs.EndGroup();
            }
        }
        //// Concatenate path. The path is added as is.
        public void ConcatPath(VertexStoreSnap s)
        {
            double x, y;
            VertexCmd cmd_flags;
            VertexSnapIter snapIter = s.GetVertexSnapIter();
            while ((cmd_flags = snapIter.GetNextVertex(out x, out y)) != VertexCmd.NoMore)
            {
                myvxs.AddVertex(x, y, cmd_flags);
            }
        }

        //--------------------------------------------------------------------
        // Join path. The path is joined with the existing one, that is, 
        // it behaves as if the pen of a plotter was always down (drawing)
        //template<class VertexSource>  
        public void JoinPath(VertexStoreSnap s)
        {
            double x, y;
            VertexSnapIter snapIter = s.GetVertexSnapIter();
            VertexCmd cmd = snapIter.GetNextVertex(out x, out y);
            if (cmd == VertexCmd.NoMore)
            {
                return;
            }

            if (VertexHelper.IsVertextCommand(cmd))
            {
                double x0, y0;
                VertexCmd flags0 = GetLastVertex(out x0, out y0);
                if (VertexHelper.IsVertextCommand(flags0))
                {
                    if (AggMath.calc_distance(x, y, x0, y0) > AggMath.VERTEX_DISTANCE_EPSILON)
                    {
                        if (VertexHelper.IsMoveTo(cmd))
                        {
                            cmd = VertexCmd.LineTo;
                        }
                        myvxs.AddVertex(x, y, cmd);
                    }
                }
                else
                {
                    if (VertexHelper.IsEmpty(flags0))
                    {
                        cmd = VertexCmd.MoveTo;
                    }
                    else if (VertexHelper.IsMoveTo(cmd))
                    {
                        cmd = VertexCmd.LineTo;
                    }

                    myvxs.AddVertex(x, y, cmd);
                }
            }

            while ((cmd = snapIter.GetNextVertex(out x, out y)) != VertexCmd.NoMore)
            {
                myvxs.AddVertex(x, y, VertexHelper.IsMoveTo(cmd) ? VertexCmd.LineTo : cmd);
            }
        }


        public static void UnsafeDirectSetData(
            PathWriter pathStore,
            int m_allocated_vertices,
            int m_num_vertices,
            double[] m_coord_xy,
            byte[] m_CommandAndFlags)
        {
            VertexStore.UnsafeDirectSetData(
                pathStore.Vxs,
                m_allocated_vertices,
                m_num_vertices,
                m_coord_xy,
                m_CommandAndFlags);
        }
        public static void UnsafeDirectGetData(
            PathWriter pathStore,
            out int m_allocated_vertices,
            out int m_num_vertices,
            out double[] m_coord_xy,
            out byte[] m_CommandAndFlags)
        {
            VertexStore.UnsafeDirectGetData(
                pathStore.Vxs,
                out m_allocated_vertices,
                out m_num_vertices,
                out m_coord_xy,
                out m_CommandAndFlags);
        }
    }
}