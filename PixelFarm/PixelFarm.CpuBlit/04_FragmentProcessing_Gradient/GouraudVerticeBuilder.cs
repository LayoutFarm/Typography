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
using PixelFarm.Drawing;
namespace PixelFarm.CpuBlit.FragmentProcessing
{
    //Gouraud shading
    //============================================================span_gouraud
    public sealed class GouraudVerticeBuilder
    {
        CoordAndColor _coord_0;
        CoordAndColor _coord_1;
        CoordAndColor _coord_2;
        double[] _x = new double[8];
        double[] _y = new double[8];
        VertexCmd[] _cmd = new VertexCmd[8];

        public struct CoordAndColor
        {
            public double x;
            public double y;
            public Drawing.Color color;
        }

        public GouraudVerticeBuilder()
        {
            _cmd[0] = VertexCmd.NoMore;
            DilationValue = 0.175f;//init value
        }
        public void SetColor(Drawing.Color c1, Drawing.Color c2, Drawing.Color c3)
        {
            _coord_0.color = c1;
            _coord_1.color = c2;
            _coord_2.color = c3;
        }

        public float DilationValue { get; set; }
        //--------------------------------------------------------------------
        // Sets the triangle and dilates it if needed.
        // The trick here is to calculate beveled joins in the vertices of the 
        // triangle and render it as a 6-vertex polygon. 
        // It's necessary to achieve numerical stability. 
        // However, the coordinates to interpolate colors are calculated
        // as miter joins (calc_intersection).
        public void SetTriangle(double x1, double y1,
                      double x2, double y2,
                      double x3, double y3)
        {
            _coord_0.x = _x[0] = x1;
            _coord_0.y = _y[0] = y1;
            _coord_1.x = _x[1] = x2;
            _coord_1.y = _y[1] = y2;
            _coord_2.x = _x[2] = x3;
            _coord_2.y = _y[2] = y3;
            _cmd[0] = VertexCmd.MoveTo;
            _cmd[1] = VertexCmd.LineTo;
            _cmd[2] = VertexCmd.LineTo;
            _cmd[3] = VertexCmd.NoMore;
            if (DilationValue != 0.0)
            {
                AggMath.DilateTriangle(_coord_0.x, _coord_0.y,
                                _coord_1.x, _coord_1.y,
                                _coord_2.x, _coord_2.y,
                                _x, _y, DilationValue);
                AggMath.CalcIntersect(_x[4], _y[4], _x[5], _y[5],
                                  _x[0], _y[0], _x[1], _y[1],
                                  out _coord_0.x, out _coord_0.y);
                AggMath.CalcIntersect(_x[0], _y[0], _x[1], _y[1],
                                  _x[2], _y[2], _x[3], _y[3],
                                  out _coord_1.x, out _coord_1.y);
                AggMath.CalcIntersect(_x[2], _y[2], _x[3], _y[3],
                                  _x[4], _y[4], _x[5], _y[5],
                                  out _coord_2.x, out _coord_2.y);
                _cmd[3] = VertexCmd.LineTo;
                _cmd[4] = VertexCmd.LineTo;
                _cmd[5] = VertexCmd.LineTo;
                _cmd[6] = VertexCmd.NoMore;
            }
        }
        public VertexStore MakeVxs(VertexStore outputVxs)
        {
            for (int i = 0; i < 8; ++i)
            {
                VertexCmd cmd;
                outputVxs.AddVertex(_x[i], _y[i], cmd = _cmd[i]);
                if (cmd == VertexCmd.NoMore)
                {
                    break;
                }
            }
            return outputVxs;
        }

        // Vertex Source Interface to feed the coordinates to the rasterizer 
        public void GetArrangedVertices(out CoordAndColor c0, out CoordAndColor c1, out CoordAndColor c2)
        {
            c0 = _coord_0;
            c1 = _coord_1;
            c2 = _coord_2;
            if (_coord_0.y > _coord_2.y)
            {
                c0 = _coord_2;
                c2 = _coord_0;
            }

            CoordAndColor tmp;
            if (c0.y > c1.y)
            {
                tmp = c1;
                c1 = c0;
                c0 = tmp;
            }

            if (c1.y > c2.y)
            {
                tmp = c2;
                c2 = c1;
                c1 = tmp;
            }
        }
    }
}
