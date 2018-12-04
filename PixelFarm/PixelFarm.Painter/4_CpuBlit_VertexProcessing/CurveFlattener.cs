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
// classes conv_curve
//
//----------------------------------------------------------------------------
using PixelFarm.Drawing;
namespace PixelFarm.CpuBlit.VertexProcessing
{
    //---------------------------------------------------------------conv_curve
    // Curve converter class. Any path storage can have Bezier curves defined 
    // by their control points. There're two types of curves supported: curve3 
    // and curve4. Curve3 is a conic Bezier curve with 2 endpoints and 1 control
    // point. Curve4 has 2 control points (4 points in total) and can be used
    // to interpolate more complicated curves. Curve4, unlike curve3 can be used 
    // to approximate arcs, both circular and elliptical. Curves are approximated 
    // with straight lines and one of the approaches is just to store the whole 
    // sequence of vertices that approximate our curve. It takes additional 
    // memory, and at the same time the consecutive vertices can be calculated 
    // on demand. 
    //
    // Initially, path storages are not suppose to keep all the vertices of the
    // curves (although, nothing prevents us from doing so). Instead, path_storage
    // keeps only vertices, needed to calculate a curve on demand. Those vertices
    // are marked with special commands. So, if the path_storage contains curves 
    // (which are not real curves yet), and we render this storage directly, 
    // all we will see is only 2 or 3 straight line segments (for curve3 and 
    // curve4 respectively). If we need to see real curves drawn we need to 
    // include this class into the conversion pipeline. 
    //
    // Class conv_curve recognizes commands path_cmd_curve3 and path_cmd_curve4 
    // and converts these vertices into a move_to/line_to sequence. 
    //-----------------------------------------------------------------------

    public class CurveFlattener
    {
        //tools , curve producer
        readonly Curve3 _curve3 = new Curve3();
        readonly Curve4 _curve4 = new Curve4();
        public CurveFlattener()
        {
        }
        public double ApproximationScale
        {
            //default 1
            get
            {
                return _curve4.ApproximationScale;
            }
            set
            {
                _curve3.ApproximationScale = value;
                _curve4.ApproximationScale = value;
            }
        }
        public Curves.CurveApproximationMethod ApproximationMethod
        {
            //default div
            get
            {
                return _curve4.ApproximationMethod;
            }
            set
            {
                _curve3.ApproximationMethod = value;
                _curve4.ApproximationMethod = value;
            }
        }
        public double AngleTolerance
        {
            //default 0
            get
            {
                return _curve4.AngleTolerance;
            }
            set
            {
                _curve3.AngleTolerance = value;
                _curve4.AngleTolerance = value;
            }
        }
        public double CuspLimit
        {
            //default 0
            get
            {
                return _curve4.CuspLimit;
            }
            set
            {
                _curve3.CuspLimit = value;
                _curve4.CuspLimit = value;
            }
        }

        enum CurvePointMode
        {
            NotCurve,
            P2,
            P3
        }



        public void Reset()
        {
            ApproximationScale = 1;
            ApproximationMethod = Curves.CurveApproximationMethod.Div;
            AngleTolerance = 0;
            CuspLimit = 0;
        }

        public VertexStore MakeVxs(VertexStore vxs, VertexStore output)
        {
            return MakeVxs(vxs, null, output);
        }


        public VertexStore MakeVxs(VertexStore vxs, ICoordTransformer tx, VertexStore output)
        {
            _curve3.Reset();
            _curve4.Reset();

            CurvePointMode latestCurveMode = CurvePointMode.NotCurve;
            double x, y;
            VertexCmd cmd;
            VectorMath.Vector2 c3p2 = new VectorMath.Vector2();
            VectorMath.Vector2 c4p2 = new VectorMath.Vector2();
            VectorMath.Vector2 c4p3 = new VectorMath.Vector2();
            double lastX = 0;
            double lasty = 0;
            double lastMoveX = 0;
            double lastMoveY = 0;


            int index = 0;
            bool hasTx = tx != null;

            while ((cmd = vxs.GetVertex(index++, out x, out y)) != VertexCmd.NoMore)
            {
#if DEBUG
                if (VertexStore.dbugCheckNANs(x, y))
                {

                }
#endif

                //-----------------
                if (hasTx)
                {
                    tx.Transform(ref x, ref y);
                }

                //-----------------
                switch (cmd)
                {

                    case VertexCmd.P2c:
                        {
                            switch (latestCurveMode)
                            {
                                case CurvePointMode.P2:
                                    {
                                    }
                                    break;
                                case CurvePointMode.P3:
                                    {
                                    }
                                    break;
                                case CurvePointMode.NotCurve:
                                    {
                                        c3p2.x = x;
                                        c3p2.y = y;
                                    }
                                    break;
                                default:
                                    {
                                    }
                                    break;
                            }
                            latestCurveMode = CurvePointMode.P2;
                        }
                        break;
                    case VertexCmd.P3c:
                        {
                            //this is p3c
                            switch (latestCurveMode)
                            {
                                case CurvePointMode.P2:
                                    {
                                        c3p2.x = x;
                                        c3p2.y = y;
                                    }
                                    break;
                                case CurvePointMode.P3:
                                    {
                                        c4p3.x = x;
                                        c4p3.y = y;
                                    }
                                    break;
                                case CurvePointMode.NotCurve:
                                    {
                                        c4p2.x = x;
                                        c4p2.y = y;
                                    }
                                    break;
                            }
                            latestCurveMode = CurvePointMode.P3;
                        }
                        break;
                    case VertexCmd.LineTo:
                        {
                            switch (latestCurveMode)
                            {
                                case CurvePointMode.P2:
                                    {
                                        _curve3.MakeLines(output,
                                            lastX,
                                            lasty,
                                            c3p2.X,
                                            c3p2.Y,
                                            x,
                                            y);
                                    }
                                    break;
                                case CurvePointMode.P3:
                                    {

                                        _curve4.MakeLines(output,
                                            lastX, lasty,
                                            c4p2.x, c4p2.y,
                                            c4p3.x, c4p3.y,
                                            x, y);
                                    }
                                    break;
                                default:
                                    {
                                        output.AddVertex(x, y, cmd);
                                    }
                                    break;
                            }
                            //-----------
                            latestCurveMode = CurvePointMode.NotCurve;
                            lastX = x;
                            lasty = y;
                            //-----------
                        }
                        break;
                    case VertexCmd.MoveTo:
                        {
                            //move to, and end command
                            output.AddVertex(x, y, cmd);
                            //-----------
                            latestCurveMode = CurvePointMode.NotCurve;
                            lastMoveX = lastX = x;
                            lastMoveY = lasty = y;
                            //-----------
                        }
                        break;

                    case VertexCmd.Close:
                    case VertexCmd.CloseAndEndFigure:
                        {
                            latestCurveMode = CurvePointMode.NotCurve;
                            output.AddVertex(lastMoveX, lastMoveY, cmd);
                            //move to begin 
                            lastX = lastMoveX;
                            lasty = lastMoveY;
                        }
                        break;

                    default:
                        {
                            //move to, and end command
                            output.AddVertex(x, y, cmd);
                            //-----------
                            latestCurveMode = CurvePointMode.NotCurve;
                            lastX = x;
                            lasty = y;
                            //-----------
                        }
                        break;
                }
            }

            return output;
        }

    }
}