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
        ZClosePath
    }


    /// <summary>
    /// forward path writer
    /// </summary>
    public sealed class PathWriter
    {

        double _latest_moveTo_X;
        double _latest_moveTo_Y;

        /// <summary>
        /// latest X
        /// </summary>
        double _latest_x;
        /// <summary>
        /// latest Y
        /// </summary>
        double _latest_y;

        /// <summary>
        /// 1st curve control point
        /// </summary>
        Vector2 _c1;
        /// <summary>
        /// 2nd curve control point 
        /// </summary>
        Vector2 _c2;
        //
        SvgPathCommand _latestSVGPathCmd;
        int _figureCount = 0;
        VertexStore _myvxs;


        public PathWriter() { }

        public int Count => _myvxs.Count;
        public void BindVxs(VertexStore vxs)
        {
            _myvxs = vxs;
        }
        public void UnbindVxs()
        {
            _myvxs = null;
            _latest_moveTo_X = _latest_moveTo_Y = _latest_x = _latest_y = 0;
            _c1 = new Vector2();
            _c2 = new Vector2();
            _latestSVGPathCmd = SvgPathCommand.MoveTo;
            _figureCount = 0;
        }
        public void Clear()
        {
            if (_myvxs != null)
            {
                _myvxs.Clear();
            }

            _latest_moveTo_X = _latest_moveTo_Y = _latest_x = _latest_y = 0;
            _c1 = new Vector2();
            _c2 = new Vector2();
            _latestSVGPathCmd = SvgPathCommand.MoveTo;
            _figureCount = 0;
        }

        //-------------------------------------------------------------------
        public double LatestMoveToX => _latest_moveTo_X;
        public double LatestMoveToY => _latest_moveTo_Y;
        public double CurrentX => _latest_x;
        public double CurrentY => _latest_y;
        //-------------------------------------------------------------------

        public int StartFigure()
        {
            if (_figureCount > 0)
            {
                //TODO: review here***
                //NoMore cmd ?
                //***
                _myvxs.AddVertex(0, 0, VertexCmd.NoMore);
            }
            _figureCount++;
            return _myvxs.Count;
        }
        public void Stop()
        {
            //TODO: review stop command again
            _myvxs.AddVertex(0, 0, VertexCmd.NoMore);
        }
        //--------------------------------------------------------------------
        public void MoveTo(double x0, double y0)
        {
            _latestSVGPathCmd = SvgPathCommand.MoveTo;
            _myvxs.AddMoveTo(
                _latest_moveTo_X = _latest_x = x0,
                _latest_moveTo_Y = _latest_y = y0);
        }
        public void MoveToRel(double dx0, double dy0)
        {
            //*** move to, relative to last(x,y) ***
            _latestSVGPathCmd = SvgPathCommand.MoveTo;
            _myvxs.AddMoveTo(
                _latest_moveTo_X = (_latest_x += dx0),
                _latest_moveTo_Y = (_latest_y += dy0));
        }
        public void LineTo(double x1, double y1)
        {
            _latestSVGPathCmd = SvgPathCommand.LineTo;
            _myvxs.AddLineTo(_latest_x = x1, _latest_y = y1);
        }
        public void LineToRel(double dx1, double dy1)
        {
            //*** line to to, relative to last(x,y) ***
            _latestSVGPathCmd = SvgPathCommand.LineTo;
            _myvxs.AddLineTo(
               _latest_x += dx1,
               _latest_y += dy1);
        }
        public void HorizontalLineTo(double x1)
        {
            _latestSVGPathCmd = SvgPathCommand.HorizontalLineTo;
            _myvxs.AddLineTo(_latest_x = x1, _latest_y);
        }
        public void HorizontalLineToRel(double dx1)
        {
            //relative ***
            _latestSVGPathCmd = SvgPathCommand.HorizontalLineTo;
            _myvxs.AddLineTo(_latest_x += dx1, _latest_y);
        }
        public void VerticalLineTo(double y1)
        {
            _latestSVGPathCmd = SvgPathCommand.VerticalLineTo;
            _myvxs.AddLineTo(_latest_x, _latest_y = y1);
        }
        public void VerticalLineToRel(double dy1)
        {
            //relative ***
            _latestSVGPathCmd = SvgPathCommand.VerticalLineTo;
            _myvxs.AddLineTo(_latest_x, _latest_y += dy1);
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
            _latestSVGPathCmd = SvgPathCommand.QuadraticBezierCurve;
            _myvxs.AddC3To(
                _c1.x = x1, _c1.y = y1,
                _latest_x = x2, _latest_y = y2);

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
            _latestSVGPathCmd = SvgPathCommand.QuadraticBezierCurve; 
            _myvxs.AddC3To(
              _c1.x = _latest_x + dx1, _c1.y = _latest_y + dy1,
              _latest_x += dx2, _latest_y += dy2); 
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
            switch (_latestSVGPathCmd)
            {
                case SvgPathCommand.QuadraticBezierCurve:
                case SvgPathCommand.TSmoothQuadraticBezierCurveTo:
                    {
                        //curve3,
                        //create new c1 from current c1
                        Vector2 new_c1 = CreateMirrorPoint(_c1, new Vector2(_latest_x, _latest_y));
                        Curve3(new_c1.X, new_c1.Y, x2, y2);
                    }
                    break;
                case SvgPathCommand.CurveTo:
                case SvgPathCommand.SmoothCurveTo:
                    {
                        //curve4,
                        //create new c1 from current c2
                        Vector2 new_c1 = CreateMirrorPoint(_c2, new Vector2(_latest_x, _latest_y));
                        Curve3(new_c1.X, new_c1.Y, x2, y2);
                    }
                    break;
                default:
                    {
                        Curve3(_latest_x, _latest_y, x2, y2);
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
            this.SmoothCurve3(_latest_x + dx1, _latest_y + dy1);
        }
        //-----------------------------------------------------------------------
        public void Curve4(double x1, double y1,
                           double x2, double y2,
                           double x3, double y3)
        {
            _latestSVGPathCmd = SvgPathCommand.CurveTo;
            _c2 = new Vector2(x2, y2);

            _myvxs.AddC4To(
                x1, y1,
                x2, y2,
                _latest_x = x3, _latest_y = y3
                );

        }

        public void Curve4Rel(double dx1, double dy1,
                              double dx2, double dy2,
                              double dx3, double dy3)
        {
            _latestSVGPathCmd = SvgPathCommand.CurveTo;
            _c2 = new Vector2(_latest_x + dx2, _latest_y + dy2);

            _myvxs.AddC4To(
               _latest_x + dx1, _latest_y + dy1,
               _latest_x + dx2, _latest_y + dy2,
               _latest_x += dx3, _latest_y += dy3
               );
        }

        //--------------------------------------------------------------------
        public void SmoothCurve4(
                       double x2, double y2,
                       double x3, double y3)
        {
            switch (_latestSVGPathCmd)
            {

                case SvgPathCommand.QuadraticBezierCurve:
                case SvgPathCommand.TSmoothQuadraticBezierCurveTo:
                    {
                        //create new c1 from current c1
                        Vector2 new_c1 = CreateMirrorPoint(_c1, new Vector2(_latest_x, _latest_y));
                        Curve4(new_c1.X, new_c1.Y, x2, y2, x3, y3);
                    }
                    break;
                case SvgPathCommand.CurveTo:
                case SvgPathCommand.SmoothCurveTo:
                    {
                        //create new c1 from current c2
                        Vector2 new_c1 = CreateMirrorPoint(_c2, new Vector2(_latest_x, _latest_y));
                        Curve4(new_c1.X, new_c1.Y, x2, y2, x3, y3);
                    }
                    break;
                default:
                    {
                        Curve4(_latest_x, _latest_y, x2, y2, x3, y3);
                    }
                    break;
            }
        }
        public void SmoothCurve4Rel(double dx2, double dy2,
                                    double dx3, double dy3)
        {
            //relative version
            SmoothCurve4(_latest_x + dx2, _latest_y + dy2, _latest_x + dx3, _latest_y + dy3);
        }
        VertexCmd GetLastVertex(out double x, out double y)
        {
            return _myvxs.GetLastVertex(out x, out y);
        }
        public void CloseFigureCCW()
        {
            if (VertexHelper.IsVertextCommand(_myvxs.GetLastCommand()))
            {
                _myvxs.AddVertex((int)EndVertexOrientation.CCW, 0, VertexCmd.Close);
            }
        }
        public void CloseFigure()
        {
            if (VertexHelper.IsVertextCommand(_myvxs.GetLastCommand()))
            {
                _latest_x = _latest_moveTo_X;
                _latest_y = _latest_moveTo_Y;

                _myvxs.AddVertex(0, 0, VertexCmd.Close);
            }
        }
        //// Concatenate path. The path is added as is.
        public void ConcatPath(VertexStore s)
        {
            double x, y;
            VertexCmd cmd;
            int index = 0;
            while ((cmd = s.GetVertex(index++, out x, out y)) != VertexCmd.NoMore)
            {
                _myvxs.AddVertex(x, y, cmd);
            }
            _myvxs.ConfirmNoMore(); //TODO: review here, we need to confirm end of myvxs
        }

        //--------------------------------------------------------------------
        // Join path. The path is joined with the existing one, that is, 
        // it behaves as if the pen of a plotter was always down (drawing)
        //template<class VertexSource>  
        public void JoinPath(VertexStore vxs)
        {
            double x, y;
            int index = 0;
            VertexCmd cmd = vxs.GetVertex(index++, out x, out y);
            if (cmd == VertexCmd.NoMore)
            {
                return;
            }
            //---------------------
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
                        _myvxs.AddVertex(x, y, cmd);
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

                    _myvxs.AddVertex(x, y, cmd);
                }
            }

            while ((cmd = vxs.GetVertex(index++, out x, out y)) != VertexCmd.NoMore)
            {
                _myvxs.AddVertex(x, y, VertexHelper.IsMoveTo(cmd) ? VertexCmd.LineTo : cmd);
            }
        }
    }


    public static class PathWriterExtensions
    {

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


        /// <summary>
        /// approximate arc with curve4, cubic curve
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <param name="xAxisRotation"></param>
        /// <param name="largeArcFlag"></param>
        /// <param name="sweepFlags"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="isRelative"></param>
        public static void SvgArcToCurve4(this PathWriter _writer, float r1, float r2, float xAxisRotation, int largeArcFlag, int sweepFlags, float x, float y, bool isRelative)
        {

            using (VectorToolBox.Borrow(out SvgArcSegment svgArc))
            {
                if (isRelative)
                {
                    svgArc.Set(
                        (float)_writer.CurrentX, (float)_writer.CurrentY,
                        r1, r2,
                        xAxisRotation,
                        (SvgArcSize)largeArcFlag,
                        (SvgArcSweep)sweepFlags,
                        (float)(_writer.CurrentX + x), (float)(_writer.CurrentY + y));
                    //
                    svgArc.AddToPath(_writer);
                }
                else
                {
                    //approximate with bezier curve
                    svgArc.Set(
                       (float)_writer.CurrentX, (float)_writer.CurrentY,
                        r1, r2,
                        xAxisRotation,
                       (SvgArcSize)largeArcFlag,
                       (SvgArcSweep)sweepFlags,
                       x, y);
                    //
                    svgArc.AddToPath(_writer);
                }
            }
        }


        public static bool CatmullRomSegmentToCurve4(this PathWriter _writer,
            double x0, double y0, //p0 //explicit x0 y0
            double x1, double y1, //p1
            double x2, double y2, //p2
            double x3, double y3) //p3
        {
            //just experiment only,
            //not correct now

            //https://en.wikipedia.org/wiki/Centripetal_Catmull%E2%80%93Rom_spline
            //https://stackoverflow.com/questions/30748316/catmull-rom-interpolation-on-svg-paths


            double t0 = 0.0f,
                   t1 = CatmullRomGetT(t0, x0, y0, x1, y1),
                   t2 = CatmullRomGetT(t1, x1, y1, x2, y2),
                   t3 = CatmullRomGetT(t2, x2, y2, x3, y3);

            if ((t0 == t1) || (t1 == t2) || (t2 == t3))
            {
                //invalid 
                _writer.LineTo(x1, y1);
                _writer.LineTo(x2, y2);
                return false;
            }


            double c1 = (t2 - t1) / (t2 - t0),
                   c2 = (t1 - t0) / (t2 - t0),
                   d1 = (t3 - t2) / (t3 - t1),
                   d2 = (t2 - t1) / (t3 - t1);



            double m1x = (t2 - t1) * (c1 * (x1 - x0) / (t1 - t0) + c2 * (x2 - x1) / (t2 - t1));
            double m2x = (t2 - t1) * (d1 * (x2 - x1) / (t2 - t1) + d2 * (x3 - x2) / (t3 - t2));

            double m1y = (t2 - t1) * (c1 * (y1 - y0) / (t1 - t0) + c2 * (y2 - y1) / (t2 - t1));
            double m2y = (t2 - t1) * (d1 * (y2 - y1) / (t2 - t1) + d2 * (y3 - y2) / (t3 - t2));


            //Q0 = P1
            //Q1 = P1 + M1 / 3
            //Q2 = P2 - M2 / 3
            //Q3 = P2

            _writer.LineTo(x1, y1);

            _writer.Curve4(x1 + m1x / 3, y1 + m1y / 3, x2 - m2x / 3, y2 - m2y / 3, x2, y2);

            return true;
        }


        //catmull_rom_alpha  from 0 to 1 for knot parameterization,
        //alpha=0.5, =>centripetal Catmull-Rom spline
        //alpha=0.0, =>standard uniform Catmull-Rom spline
        //alpha=1.0, =>chordal Catmull-Rom spline

        static float catmull_rom_alpha = 0.5f;
        static double CatmullRomGetT(double t, double ax, double ay, double bx, double by)
        {

            double a = ((bx - ax) * (bx - ax)) + ((by - ay) * (by - ay));// System.Math.Pow((bx - ax), 2.0f) + System.Math.Pow((by - ay), 2.0f);
            double b = System.Math.Pow(a, 0.5f);
            double c = System.Math.Pow(b, catmull_rom_alpha);

            return (c + t);
        }
        public static void CatmullRomToCurve4Rel(this PathWriter _writer, double dx0, double dy0, double dx1, double dy1, double dx2, double dy2, double dx3, double dy3)
        {
            //https://en.wikipedia.org/wiki/Centripetal_Catmull%E2%80%93Rom_spline
            //relative point
            double curX = _writer.CurrentX;
            double curY = _writer.CurrentY;
            CatmullRomSegmentToCurve4(_writer, curX + dx0, curY + dy0, curX + dx1, curY + dy1, curX + dx2, curY + dy2, curX + dx3, curY + dy3);
        }
    }
}