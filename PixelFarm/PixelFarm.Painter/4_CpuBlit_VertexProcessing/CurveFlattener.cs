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
        readonly Curve3 m_curve3 = new Curve3();
        readonly Curve4 m_curve4 = new Curve4();
        public CurveFlattener()
        {
        }
        public double ApproximationScale
        {
            get
            {
                return m_curve4.ApproximationScale;
            }
            set
            {
                m_curve3.ApproximationScale = value;
                m_curve4.ApproximationScale = value;
            }
        }
        public Curves.CurveApproximationMethod ApproximationMethod
        {
            get
            {
                return m_curve4.ApproximationMethod;
            }
            set
            {
                m_curve3.ApproximationMethod = value;
                m_curve4.ApproximationMethod = value;
            }
        }
        public double AngleTolerance
        {
            get
            {
                return m_curve4.AngleTolerance;
            }
            set
            {
                m_curve3.AngleTolerance = value;
                m_curve4.AngleTolerance = value;
            }
        }
        public double CuspLimit
        {
            get
            {
                return m_curve4.CuspLimit;
            }
            set
            {
                m_curve3.CuspLimit = value;
                m_curve4.CuspLimit = value;
            }
        }

        enum CurvePointMode
        {
            NotCurve,
            P2,
            P3
        }

        public void MakeVxs(VertexStoreSnap vsnap, VertexStore vxs)
        {
            m_curve3.Reset();
            m_curve4.Reset();
            var snapIter = vsnap.GetVertexSnapIter();
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


            do
            {
                //this vertex
                cmd = snapIter.GetNextVertex(out x, out y);
#if DEBUG
                if (VertexStore.dbugCheckNANs(x, y))
                {
                    int dbugIter = snapIter.dbugIterIndex;
                }

                //if (VertexStore.dbugCheckIfNAN(x, y))
                //{
                //    
                //}
#endif
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
                                        m_curve3.MakeLines(vxs,
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

                                        m_curve4.MakeLines(vxs,
                                            lastX, lasty,
                                            c4p2.x, c4p2.y,
                                            c4p3.x, c4p3.y,
                                            x, y);
                                    }
                                    break;
                                default:
                                    {
                                        vxs.AddVertex(x, y, cmd);
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
                            vxs.AddVertex(x, y, cmd);
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
                            vxs.AddVertex(x, y, cmd);
                            //move to begin 
                            lastX = lastMoveX;
                            lasty = lastMoveY;
                        }
                        break;

                    default:
                        {
                            //move to, and end command
                            vxs.AddVertex(x, y, cmd);
                            //-----------
                            latestCurveMode = CurvePointMode.NotCurve;
                            lastX = x;
                            lasty = y;
                            //-----------
                        }
                        break;
                }
            } while (cmd != VertexCmd.NoMore);

        }
        public VertexStore MakeVxs(VertexStore srcVxs, VertexStore outputVxs)
        {
            MakeVxs(new VertexStoreSnap(srcVxs), outputVxs);
            return outputVxs;
        }
    }
}