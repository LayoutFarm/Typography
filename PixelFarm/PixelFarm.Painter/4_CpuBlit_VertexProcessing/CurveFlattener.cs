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

    public enum CurveApproximationMethod
    {
        Unknow,
        Div,
        Inc,
    }

    public class CurveFlattener
    {
        CurveApproximationMethod _selectedApproximationMethod = CurveApproximationMethod.Div;

        //tools , curve producer 
        readonly CurveSubdivisionFlattener _div_curveFlattener = new CurveSubdivisionFlattener();
        readonly CurveIncFlattener _inc_curveFlattener = new CurveIncFlattener();
        readonly CurveFlattenerOutput _curveFlattenerOutput = new CurveFlattenerOutput();

        class CurveFlattenerOutput : ICurveFlattenerOutput
        {
            VertexStore _vxs;
            public CurveFlattenerOutput()
            {
            }
            public void Append(double x, double y)
            {
                _vxs.AddLineTo(x, y);
            }
            public void SetVxs(VertexStore vxs)
            {
                _vxs = vxs;
            }
        }

        double _approximateScale = 1;//default
        public CurveFlattener()
        {
        }
        public double ApproximationScale
        {
            //default 1
            get => _approximateScale;
            set
            {
                _inc_curveFlattener.ApproximationScale =
                    _div_curveFlattener.ApproximationScale =
                    _approximateScale = value;
            }
        }

        public CurveApproximationMethod ApproximationMethod
        {
            get => _selectedApproximationMethod;
            set => _selectedApproximationMethod = value;
        }
        /// <summary>
        ///  curve incremental flattener, use specific step count
        /// </summary>
        public bool IncUseFixedStep
        {
            get => _inc_curveFlattener.UseFixedStepCount;
            set => _inc_curveFlattener.UseFixedStepCount = value;
        }
        /// <summary>
        /// curve incremental flattener, incremental step count
        /// </summary>
        public int IncStepCount
        {
            get => _inc_curveFlattener.FixedStepCount;
            set => _inc_curveFlattener.FixedStepCount = value;
        }
        /// <summary>
        /// curve subdivision flattener , angle tolerance
        /// </summary>
        public double AngleTolerance
        {
            //default 0
            get => _div_curveFlattener.AngleTolerance;
            set => _div_curveFlattener.AngleTolerance = value;
        }
        /// <summary>
        /// curve subdivision flattener, recursive limit
        /// </summary>
        public byte RecursiveLimit
        {
            get => _div_curveFlattener.RecursiveLimit;
            set => _div_curveFlattener.RecursiveLimit = value;
        }
        /// <summary>
        /// curve subdivision flattener, cusp limit
        /// </summary>
        public double CuspLimit
        {
            //default 0
            get => _div_curveFlattener.CuspLimit;
            set => _div_curveFlattener.CuspLimit = value;
        }

        public void Reset()
        {
            ApproximationScale = 1;
            ApproximationMethod = CurveApproximationMethod.Div;
            AngleTolerance = 0;
            CuspLimit = 0;


            _inc_curveFlattener.Reset();
            _div_curveFlattener.Reset();
        }

        public VertexStore MakeVxs(VertexStore vxs, VertexStore output)
        {
            return MakeVxs(vxs, null, output);
        }
        public VertexStore MakeVxs(VertexStore vxs, ICoordTransformer tx, VertexStore output)
        {
            double x, y;
            VertexCmd cmd;
            double lastX = 0;
            double lastY = 0;
            double lastMoveX = 0;
            double lastMoveY = 0;

            int index = 0;
            bool hasTx = tx != null;

            _curveFlattenerOutput.SetVxs(output);

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
                    case VertexCmd.C3:
                        {
                            //for curve3, it contains (x0,y0), (x1,y1), (x2,y2)
                            //this is (x1,y1) so next point must be (x2,y2)

                            //read next
                            cmd = vxs.GetVertex(index++, out double x2, out double y2);
                            if (cmd != VertexCmd.LineTo) { throw new System.NotSupportedException(); }

                            if (_selectedApproximationMethod == CurveApproximationMethod.Inc)
                            {
                                _inc_curveFlattener.Flatten(
                                   lastX, lastY,
                                   x, y,
                                   lastX = x2, lastY = y2,
                                   _curveFlattenerOutput, true);

                            }
                            else
                            {
                                _div_curveFlattener.Flatten(
                                    lastX, lastY,
                                    x, y,
                                   lastX = x2, lastY = y2,
                                    _curveFlattenerOutput,
                                    true
                                    );
                            }
                        }
                        break;
                    case VertexCmd.C4:
                        {
                            //for curve4, it contains (x0,y0), (x1,y1), (x2,y2), (x3,y3)                            
                            //this is (x1,y1) so next point must be (x2,y2) and (x3,y3)

                            cmd = vxs.GetVertex(index++, out double x2, out double y2);
                            if (cmd != VertexCmd.C4)
                            {
                                throw new System.NotSupportedException();
                            }
                            cmd = vxs.GetVertex(index++, out double x3, out double y3);
                            if (cmd != VertexCmd.LineTo)
                            {
                                throw new System.NotSupportedException();
                            }

                            if (_selectedApproximationMethod == CurveApproximationMethod.Inc)
                            {
                                _inc_curveFlattener.Flatten(
                                    lastX, lastY,
                                    x, y,
                                    x2, y2,
                                   lastX = x3, lastY = y3,
                                    _curveFlattenerOutput, true
                                    );
                            }
                            else
                            {
                                _div_curveFlattener.Flatten(
                                     lastX, lastY,
                                     x, y,
                                     x2, y2,
                                     lastX = x3, lastY = y3,
                                     _curveFlattenerOutput, true
                                    );
                            }
                        }
                        break;
                    case VertexCmd.LineTo:
                        output.AddLineTo(lastX = x, lastY = y);

                        break;
                    case VertexCmd.MoveTo:
                        //move to, and end command
                        output.AddVertex(lastMoveX = lastX = x, lastMoveY = lastY = y, cmd);
                        break;
                    case VertexCmd.Close:
                        //we need only command
                        //move to begin 
                        output.AddVertex(lastX = lastMoveX, lastY = lastMoveY, cmd);
                        break;
                    default:
                        //move to, and end command
                        output.AddVertex(lastX = x, lastY = y, cmd);
                        break;
                }
            }
            return output;
        }
    }
}