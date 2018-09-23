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
    public abstract class GouraudSpanGen
    {
        CoordAndColor m_coord_0;
        CoordAndColor m_coord_1;
        CoordAndColor m_coord_2;
        double[] m_x = new double[8];
        double[] m_y = new double[8];
        VertexCmd[] m_cmd = new VertexCmd[8];
        public struct CoordAndColor
        {
            public double x;
            public double y;
            public Drawing.Color color;
        }

        public GouraudSpanGen()
        {
            m_cmd[0] = VertexCmd.NoMore;
        }

        public GouraudSpanGen(Drawing.Color c1,
                     Drawing.Color c2,
                     Drawing.Color c3,
                     double x1, double y1,
                     double x2, double y2,
                     double x3, double y3,
                     double d)
        {
            SetColor(c1, c2, c3);
            SetTriangle(x1, y1, x2, y2, x3, y3, d);
        }

        public void SetColor(Drawing.Color c1, Drawing.Color c2, Drawing.Color c3)
        {
            m_coord_0.color = c1;
            m_coord_1.color = c2;
            m_coord_2.color = c3;
        }

        //--------------------------------------------------------------------
        // Sets the triangle and dilates it if needed.
        // The trick here is to calculate beveled joins in the vertices of the 
        // triangle and render it as a 6-vertex polygon. 
        // It's necessary to achieve numerical stability. 
        // However, the coordinates to interpolate colors are calculated
        // as miter joins (calc_intersection).
        public void SetTriangle(double x1, double y1,
                      double x2, double y2,
                      double x3, double y3,
                      double d)
        {
            m_coord_0.x = m_x[0] = x1;
            m_coord_0.y = m_y[0] = y1;
            m_coord_1.x = m_x[1] = x2;
            m_coord_1.y = m_y[1] = y2;
            m_coord_2.x = m_x[2] = x3;
            m_coord_2.y = m_y[2] = y3;
            m_cmd[0] = VertexCmd.MoveTo;
            m_cmd[1] = VertexCmd.LineTo;
            m_cmd[2] = VertexCmd.LineTo;
            m_cmd[3] = VertexCmd.NoMore;
            if (d != 0.0)
            {
                AggMath.DilateTriangle(m_coord_0.x, m_coord_0.y,
                                m_coord_1.x, m_coord_1.y,
                                m_coord_2.x, m_coord_2.y,
                                m_x, m_y, d);
                AggMath.CalcIntersect(m_x[4], m_y[4], m_x[5], m_y[5],
                                  m_x[0], m_y[0], m_x[1], m_y[1],
                                  out m_coord_0.x, out m_coord_0.y);
                AggMath.CalcIntersect(m_x[0], m_y[0], m_x[1], m_y[1],
                                  m_x[2], m_y[2], m_x[3], m_y[3],
                                  out m_coord_1.x, out m_coord_1.y);
                AggMath.CalcIntersect(m_x[2], m_y[2], m_x[3], m_y[3],
                                  m_x[4], m_y[4], m_x[5], m_y[5],
                                  out m_coord_2.x, out m_coord_2.y);
                m_cmd[3] = VertexCmd.LineTo;
                m_cmd[4] = VertexCmd.LineTo;
                m_cmd[5] = VertexCmd.LineTo;
                m_cmd[6] = VertexCmd.NoMore;
            }
        }
        public VertexStore MakeVxs(VertexStore outputVxs)
        {
            for (int i = 0; i < 8; ++i)
            {
                VertexCmd cmd;
                outputVxs.AddVertex(m_x[i], m_y[i], cmd = m_cmd[i]);
                if (cmd == VertexCmd.NoMore)
                {
                    break;
                }
            }
            return outputVxs;
        }

        // Vertex Source Interface to feed the coordinates to the rasterizer 
        protected void LoadArrangedVertices(out CoordAndColor c0, out CoordAndColor c1, out CoordAndColor c2)
        {
            c0 = m_coord_0;
            c1 = m_coord_1;
            c2 = m_coord_2;
            if (m_coord_0.y > m_coord_2.y)
            {
                c0 = m_coord_2;
                c2 = m_coord_0;
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
