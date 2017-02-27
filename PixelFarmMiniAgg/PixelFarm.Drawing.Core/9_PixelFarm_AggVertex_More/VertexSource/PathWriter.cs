//BSD, 2014-2017, WinterDev
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
namespace PixelFarm.Agg.VertexSource
{
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
        double lastMoveX;
        double lastMoveY;
        double lastX;
        double lastY;
        /// <summary>
        /// curve3 point2 (1st control point)
        /// </summary>
        Vector2 c3p2;
        /// <summary>
        /// curve4 point3
        /// </summary>
        Vector2 c4p3;
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
            lastMoveX = lastMoveY = lastX = lastY = 0;
            c3p2 = new Vector2();
            c4p3 = new Vector2();
            latestSVGPathCmd = SvgPathCommand.MoveTo;
            figureCount = 0;
        }
        public void ClearAndStartNewVxs(VertexStore newVxsOutput)
        {

            myvxs = newVxsOutput;
            Clear();
        }
        //-------------------------------------------------------------------
        public double LastMoveX { get { return this.lastMoveX; } }
        public double LastMoveY { get { return this.lastMoveY; } }
        //-------------------------------------------------------------------

        public int StartFigure()
        {
            if (figureCount > 0)
            {
               
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
        public void MoveTo(double x, double y)
        {
            this.latestSVGPathCmd = SvgPathCommand.MoveTo;
            myvxs.AddMoveTo(
                this.lastMoveX = this.lastX = x,
                this.lastMoveY = this.lastY = y);
        }
        public void MoveToRel(double x, double y)
        {
            this.latestSVGPathCmd = SvgPathCommand.MoveTo;
            myvxs.AddMoveTo(
                this.lastMoveX = (this.lastX += x),
                this.lastMoveY = (this.lastY += y));
        }
        public void LineTo(double x, double y)
        {
            this.latestSVGPathCmd = SvgPathCommand.LineTo;
            myvxs.AddLineTo(this.lastX = x, this.lastY = y);
        }
        public void LineToRel(double x, double y)
        {
            this.latestSVGPathCmd = SvgPathCommand.LineTo;
            myvxs.AddLineTo(
                this.lastX += x,
                this.lastY += y);
        }
        public void HorizontalLineTo(double x)
        {
            this.latestSVGPathCmd = SvgPathCommand.HorizontalLineTo;
            myvxs.AddLineTo(this.lastX = x, lastY);
        }
        public void HorizontalLineToRel(double x)
        {
            this.latestSVGPathCmd = SvgPathCommand.HorizontalLineTo;
            myvxs.AddLineTo(this.lastX += x, lastY);
        }
        public void VerticalLineTo(double y)
        {
            this.latestSVGPathCmd = SvgPathCommand.VerticalLineTo;
            myvxs.AddLineTo(lastX, this.lastY = y);
        }
        public void VerticalLineToRel(double y)
        {
            this.latestSVGPathCmd = SvgPathCommand.VerticalLineTo;
            myvxs.AddLineTo(lastX, this.lastY += y);
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
        ///  Draws a quadratic Bezier curve from the current point to (x,y) using (xControl,yControl) as the control point.
        /// </summary>
        /// <param name="p2x"></param>
        /// <param name="p2y"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void Curve3(double p2x, double p2y, double x, double y)
        {
            this.latestSVGPathCmd = SvgPathCommand.QuadraticBezierCurve;
            this.c3p2.x = p2x;
            this.c3p2.y = p2y;
            myvxs.AddP2c(p2x, p2y);
            myvxs.AddLineTo(this.lastX = x, this.lastY = y);
        }
        /// <summary>
        /// Draws a quadratic Bezier curve from the current point to (x,y) using (xControl,yControl) as the control point.
        /// </summary>
        /// <param name="xControl"></param>
        /// <param name="yControl"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void Curve3Rel(double p2x, double p2y, double x, double y)
        {
            this.latestSVGPathCmd = SvgPathCommand.QuadraticBezierCurve;
            this.c3p2.x = this.lastX + p2x;
            this.c3p2.y = this.lastY + p2y;
            myvxs.AddP2c(this.lastX + p2x, this.lastY + p2y);
            myvxs.AddLineTo(this.lastX += x, this.lastY += y);
        }

        /// <summary> 
        /// <para>Draws a quadratic Bezier curve from the current point to (x,y).</para>
        /// <para>The control point is assumed to be the reflection of the control point on the previous command relative to the current point.</para>
        /// <para>(If there is no previous command or if the previous command was not a curve, assume the control point is coincident with the current point.)</para>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SmoothCurve3(double x, double y)
        {
            switch (this.latestSVGPathCmd)
            {
                case SvgPathCommand.QuadraticBezierCurve:
                case SvgPathCommand.TSmoothQuadraticBezierCurveTo:
                    {
                        //curve3
                        Vector2 newC3 = CreateMirrorPoint(this.c3p2, new Vector2(this.lastX, this.lastY));
                        Curve3(newC3.X, newC3.Y, x, y);
                    }
                    break;
                case SvgPathCommand.CurveTo:
                case SvgPathCommand.SmoothCurveTo:
                    {
                        //curve4
                        Vector2 newC3 = CreateMirrorPoint(this.c4p3, new Vector2(this.lastX, this.lastY));
                        Curve3(newC3.X, newC3.Y, x, y);
                    }
                    break;
                default:
                    {
                        Curve3(this.lastX, this.lastY, x, y);
                    }
                    break;
            }
        }

        /// <summary>
        /// <para>Draws a quadratic Bezier curve from the current point to (x,y).</para>
        /// <para>The control point is assumed to be the reflection of the control point on the previous command relative to the current point.</para>
        /// <para>(If there is no previous command or if the previous command was not a curve, assume the control point is coincident with the current point.)</para>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SmoothCurve3Rel(double x, double y)
        {
            this.SmoothCurve3(this.lastX + x, this.lastY + y);
        }
        //-----------------------------------------------------------------------
        public void Curve4(double p2x, double p2y,
                           double p3x, double p3y,
                           double x, double y)
        {
            this.latestSVGPathCmd = SvgPathCommand.CurveTo;
            myvxs.AddP3c(p2x, p2y);
            myvxs.AddP3c(p3x, p3y);
            myvxs.AddLineTo(this.lastX = x, this.lastY = y);
        }

        public void Curve4Rel(double p2x, double p2y,
                              double p3x, double p3y,
                              double x, double y)
        {
            this.latestSVGPathCmd = SvgPathCommand.CurveTo;
            myvxs.AddP3c(this.lastX + p2x, this.lastY + p2y);
            myvxs.AddP3c(this.lastX + p3x, this.lastY + p3y);
            myvxs.AddLineTo(this.lastX += x, this.lastY += y);
        }

        //--------------------------------------------------------------------
        public void SmoothCurve4(double p3x, double p3y,
                       double x, double y)
        {
            switch (this.latestSVGPathCmd)
            {
                case SvgPathCommand.QuadraticBezierCurve:
                case SvgPathCommand.TSmoothQuadraticBezierCurveTo:
                    {
                        //create c4p1 from c3p1
                        Vector2 c4p2 = CreateMirrorPoint(this.c3p2, new Vector2(this.lastX, this.lastY));
                        Curve4(c4p2.X, c4p2.Y, p3x, p3y, x, y);
                    }
                    break;
                case SvgPathCommand.CurveTo:
                case SvgPathCommand.SmoothCurveTo:
                    {
                        //curve4
                        Vector2 c4p2 = CreateMirrorPoint(this.c4p3, new Vector2(this.lastX, this.lastY));
                        Curve4(c4p2.X, c4p2.Y, p3x, p3y, x, y);
                    }
                    break;
                default:
                    {
                        Curve4(this.lastX, this.lastY, p3x, p3y, x, y);
                    }
                    break;
            }
        }

        public void SmoothCurve4Rel(double p3x, double p3y,
                                    double x, double y)
        {
            SmoothCurve4(this.lastX + p3x, this.lastY + p3y, this.lastX + x, this.lastY + y);
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