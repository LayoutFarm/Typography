//BSD, 2014-present, WinterDev
//----------------------------------------------------------------------------
// Anti-Grain Geometry - Version 2.4
// Copyright (C) 2002-2005 Maxim Shemanarev (http://www.antigrain.com)
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
using PixelFarm.Drawing;

namespace PixelFarm.CpuBlit.Rasterization.Lines
{
    //-----------------------------------------------------------line_aa_vertex
    // Vertex (x, y) with the distance to the next one. The last vertex has 
    // the distance between the last and the first points
    struct LineAAVertex
    {
        public readonly int x;
        public readonly int y;
        public int len;
        //
        const int SIGDIFF = LineAA.SUBPIXEL_SCALE + (LineAA.SUBPIXEL_SCALE / 2);

        public LineAAVertex(int x, int y)
        {
            this.x = x;
            this.y = y;
            len = 0;
        }
        public bool IsDiff(LineAAVertex val)
        {
            //*** NEED 64 bits long
            long dx = val.x - x;
            long dy = val.y - y;
            if ((dx + dy) == 0)
            {
                return false;
            }
            return (len = AggMath.uround(Math.Sqrt(dx * dx + dy * dy))) > SIGDIFF;
        }
    }

    class LineAAVertexSequence
    {
        ArrayList<LineAAVertex> list = new ArrayList<LineAAVertex>();
        public void AddVertex(LineAAVertex val)
        {
            int count = list.Count;
            if (count > 1)
            {
                var innerArray = list.Array;
                if (!innerArray[count - 2].IsDiff(innerArray[count - 1]))
                {
                    list.RemoveLast();
                }
            }
            list.AddVertex(val);
        }
        public LineAAVertex this[int index]
        {
            get
            {
                return this.list[index];
            }
        }
        public void Clear() { this.list.Clear(); }
        public int Count
        {
            get { return this.list.Count; }
        }
        public void ModifyLast(LineAAVertex val)
        {
            list.RemoveLast();
            AddVertex(val);
        }

        public void Close(bool closed)
        {
            //----------------------
            //iter backward
            int count = list.Count;
            var innerArray = list.Array;
            while (count > 1)
            {
                if (innerArray[count - 2].IsDiff(innerArray[count - 1]))
                {
                    break;
                }
                else
                {
                    LineAAVertex t = list[count - 1];
                    list.RemoveLast();
                    ModifyLast(t);
                    count--;
                }
            }


            if (closed)
            {
                //if close figure
                count = list.Count;
                var first = innerArray[0];
                while (count > 1)
                {
                    if (innerArray[count - 1].IsDiff(first))
                    {
                        break;
                    }
                    count--;
                    list.RemoveLast();
                }
            }
        }
    }

    //=======================================================rasterizer_outline_aa
    public class OutlineAARasterizer
    {
        LineRenderer m_ren;
        LineAAVertexSequence m_src_vertices = new LineAAVertexSequence();
        OutlineJoin m_line_join;
        bool m_round_cap;
        int m_start_x;
        int m_start_y;
        public enum OutlineJoin
        {
            NoJoin,             //-----outline_no_join
            Mitter,          //-----outline_miter_join
            Round,          //-----outline_round_join
            AccurateJoin  //-----outline_accurate_join
        }

        public bool CompareDistStart(int d) { return d > 0; }
        public bool CompareDistEnd(int d) { return d <= 0; }

        struct DrawVarsPart0
        {
            public int idx;
            public int lcurr, lnext;
            public int flags;
        }

        struct DrawVarsPart1
        {
            public int x1, y1, x2, y2;
        }
        struct DrawVarsPart2
        {
            public int xb1, yb1, xb2, yb2;
        }

        void Draw(ref DrawVarsPart0 dv,
            ref DrawVarsPart1 dv1,
            ref DrawVarsPart2 dv2,
            ref LineParameters curr,
            ref LineParameters next,
            int start,
            int end)
        {
            int i;
            for (i = start; i < end; i++)
            {
                if (m_line_join == OutlineJoin.Round)
                {
                    dv2.xb1 = curr.x1 + (curr.y2 - curr.y1);
                    dv2.yb1 = curr.y1 - (curr.x2 - curr.x1);
                    dv2.xb2 = curr.x2 + (curr.y2 - curr.y1);
                    dv2.yb2 = curr.y2 - (curr.x2 - curr.x1);
                }

                switch (dv.flags)
                {
                    case 0: m_ren.Line3(curr, dv2.xb1, dv2.yb1, dv2.xb2, dv2.yb2); break;
                    case 1: m_ren.Line2(curr, dv2.xb2, dv2.yb2); break;
                    case 2: m_ren.Line1(curr, dv2.xb1, dv2.yb1); break;
                    case 3: m_ren.Line0(curr); break;
                }

                if (m_line_join == OutlineJoin.Round && (dv.flags & 2) == 0)
                {
                    m_ren.Pie(curr.x2, curr.y2,
                               curr.x2 + (curr.y2 - curr.y1),
                               curr.y2 - (curr.x2 - curr.x1),
                               curr.x2 + (next.y2 - next.y1),
                               curr.y2 - (next.x2 - next.x1));
                }

                dv1.x1 = dv1.x2;
                dv1.y1 = dv1.y2;
                dv.lcurr = dv.lnext;
                dv.lnext = m_src_vertices[dv.idx].len;
                ++dv.idx;
                if (dv.idx >= m_src_vertices.Count) dv.idx = 0;
                dv1.x2 = m_src_vertices[dv.idx].x;
                dv1.y2 = m_src_vertices[dv.idx].y;
                curr = next;
                next = new LineParameters(dv1.x1, dv1.y1, dv1.x2, dv1.y2, dv.lnext);
                dv2.xb1 = dv2.xb2;
                dv2.yb1 = dv2.yb2;
                switch (m_line_join)
                {
                    case OutlineJoin.NoJoin:
                        dv.flags = 3;
                        break;
                    case OutlineJoin.Mitter:
                        dv.flags >>= 1;
                        dv.flags |= (curr.DiagonalQuadrant ==
                            next.DiagonalQuadrant ? 1 : 0);
                        if ((dv.flags & 2) == 0)
                        {
                            LineAA.Bisectrix(curr, next, out dv2.xb2, out dv2.yb2);
                        }
                        break;
                    case OutlineJoin.Round:
                        dv.flags >>= 1;
                        dv.flags |= (((curr.DiagonalQuadrant ==
                            next.DiagonalQuadrant) ? 1 : 0) << 1);
                        break;
                    case OutlineJoin.AccurateJoin:
                        dv.flags = 0;
                        LineAA.Bisectrix(curr, next, out dv2.xb2, out dv2.yb2);
                        break;
                }
            }
        }

        public OutlineAARasterizer(LineRenderer ren)
        {
            m_ren = ren;
            m_line_join = (OutlineRenderer.AccurateJoinOnly ?
                            OutlineJoin.AccurateJoin :
                            OutlineJoin.Round);
            m_round_cap = (false);
            m_start_x = (0);
            m_start_y = (0);
        }

        public void Attach(LineRenderer ren) { m_ren = ren; }


        public OutlineJoin LineJoin
        {
            get { return this.m_line_join; }
            set
            {
                m_line_join = OutlineRenderer.AccurateJoinOnly ?
                OutlineJoin.AccurateJoin : value;
            }
        }

        public bool RoundCap
        {
            get { return this.m_round_cap; }
            set { this.m_round_cap = value; }
        }
        public void MoveTo(int x, int y)
        {
            m_src_vertices.ModifyLast(new LineAAVertex(m_start_x = x, m_start_y = y));
        }

        public void LineTo(int x, int y)
        {
            m_src_vertices.AddVertex(new LineAAVertex(x, y));
        }

        public void MoveTo(double x, double y)
        {
            MoveTo(LineCoordSat.Convert(x), LineCoordSat.Convert(y));
        }

        public void LineTo(double x, double y)
        {
            LineTo(LineCoordSat.Convert(x), LineCoordSat.Convert(y));
        }

        public void Render(bool close_polygon)
        {
            m_src_vertices.Close(close_polygon);
            DrawVarsPart0 dv = new DrawVarsPart0();
            DrawVarsPart1 dv1 = new DrawVarsPart1();
            DrawVarsPart2 dv2 = new DrawVarsPart2();
            LineAAVertex v;
            int x1;
            int y1;
            int x2;
            int y2;
            int lprev;
            LineParameters curr;
            LineParameters next;
            if (close_polygon)
            {
                if (m_src_vertices.Count >= 3)
                {
                    dv.idx = 2;
                    v = m_src_vertices[m_src_vertices.Count - 1];
                    x1 = v.x;
                    y1 = v.y;
                    lprev = v.len;
                    v = m_src_vertices[0];
                    x2 = v.x;
                    y2 = v.y;
                    dv.lcurr = v.len;
                    LineParameters prev = new LineParameters(x1, y1, x2, y2, lprev);
                    v = m_src_vertices[1];
                    dv1.x1 = v.x;
                    dv1.y1 = v.y;
                    dv.lnext = v.len;
                    curr = new LineParameters(x2, y2, dv1.x1, dv1.y1, dv.lcurr);
                    v = m_src_vertices[dv.idx];
                    dv1.x2 = v.x;
                    dv1.y2 = v.y;
                    next = new LineParameters(dv1.x1, dv1.y1, dv1.x2, dv1.y2, dv.lnext);
                    dv2.xb1 = 0;
                    dv2.yb1 = 0;
                    dv2.xb2 = 0;
                    dv2.yb2 = 0;
                    switch (m_line_join)
                    {
                        case OutlineJoin.NoJoin:
                            dv.flags = 3;
                            break;
                        case OutlineJoin.Mitter:
                        case OutlineJoin.Round:
                            dv.flags =
                                (prev.DiagonalQuadrant == curr.DiagonalQuadrant ? 1 : 0) |
                                    ((curr.DiagonalQuadrant == next.DiagonalQuadrant ? 1 : 0) << 1);
                            break;
                        case OutlineJoin.AccurateJoin:
                            dv.flags = 0;
                            break;
                    }

                    if ((dv.flags & 1) == 0 && m_line_join != OutlineJoin.Round)
                    {
                        LineAA.Bisectrix(prev, curr, out dv2.xb1, out dv2.yb1);
                    }

                    if ((dv.flags & 2) == 0 && m_line_join != OutlineJoin.Round)
                    {
                        LineAA.Bisectrix(curr, next, out dv2.xb2, out dv2.yb2);
                    }
                    Draw(ref dv, ref dv1, ref dv2, ref curr, ref next, 0, m_src_vertices.Count);
                }
            }
            else
            {
                switch (m_src_vertices.Count)
                {
                    case 0:
                    case 1:
                        break;
                    case 2:
                        {
                            v = m_src_vertices[0];
                            x1 = v.x;
                            y1 = v.y;
                            lprev = v.len;
                            v = m_src_vertices[1];
                            x2 = v.x;
                            y2 = v.y;
                            LineParameters lp = new LineParameters(x1, y1, x2, y2, lprev);
                            if (m_round_cap)
                            {
                                m_ren.SemiDot(CompareDistStart, x1, y1, x1 + (y2 - y1), y1 - (x2 - x1));
                            }
                            m_ren.Line3(lp,
                                         x1 + (y2 - y1),
                                         y1 - (x2 - x1),
                                         x2 + (y2 - y1),
                                         y2 - (x2 - x1));
                            if (m_round_cap)
                            {
                                m_ren.SemiDot(CompareDistEnd, x2, y2, x2 + (y2 - y1), y2 - (x2 - x1));
                            }
                        }
                        break;
                    case 3:
                        {
                            int x3, y3;
                            int lnext;
                            v = m_src_vertices[0];
                            x1 = v.x;
                            y1 = v.y;
                            lprev = v.len;
                            v = m_src_vertices[1];
                            x2 = v.x;
                            y2 = v.y;
                            lnext = v.len;
                            v = m_src_vertices[2];
                            x3 = v.x;
                            y3 = v.y;

                            LineParameters lp1 = new LineParameters(x1, y1, x2, y2, lprev);
                            LineParameters lp2 = new LineParameters(x2, y2, x3, y3, lnext);
                            if (m_round_cap)
                            {
                                m_ren.SemiDot(CompareDistStart, x1, y1, x1 + (y2 - y1), y1 - (x2 - x1));
                            }

                            if (m_line_join == OutlineJoin.Round)
                            {
                                m_ren.Line3(lp1, x1 + (y2 - y1), y1 - (x2 - x1),
                                                  x2 + (y2 - y1), y2 - (x2 - x1));
                                m_ren.Pie(x2, y2, x2 + (y2 - y1), y2 - (x2 - x1),
                                                   x2 + (y3 - y2), y2 - (x3 - x2));
                                m_ren.Line3(lp2, x2 + (y3 - y2), y2 - (x3 - x2),
                                                  x3 + (y3 - y2), y3 - (x3 - x2));
                            }
                            else
                            {
                                LineAA.Bisectrix(lp1, lp2, out dv2.xb1, out dv2.yb1);
                                m_ren.Line3(lp1, x1 + (y2 - y1), y1 - (x2 - x1),
                                                  dv2.xb1, dv2.yb1);
                                m_ren.Line3(lp2, dv2.xb1, dv2.yb1,
                                                  x3 + (y3 - y2), y3 - (x3 - x2));
                            }
                            if (m_round_cap)
                            {
                                m_ren.SemiDot(CompareDistEnd, x3, y3, x3 + (y3 - y2), y3 - (x3 - x2));
                            }
                        }
                        break;
                    default:
                        {
                            dv.idx = 3;
                            v = m_src_vertices[0];
                            x1 = v.x;
                            y1 = v.y;
                            lprev = v.len;
                            v = m_src_vertices[1];
                            x2 = v.x;
                            y2 = v.y;
                            dv.lcurr = v.len;
                            LineParameters prev = new LineParameters(x1, y1, x2, y2, lprev);
                            v = m_src_vertices[2];
                            dv1.x1 = v.x;
                            dv1.y1 = v.y;
                            dv.lnext = v.len;
                            curr = new LineParameters(x2, y2, dv1.x1, dv1.y1, dv.lcurr);
                            v = m_src_vertices[dv.idx];
                            dv1.x2 = v.x;
                            dv1.y2 = v.y;
                            next = new LineParameters(dv1.x1, dv1.y1, dv1.x2, dv1.y2, dv.lnext);
                            dv2.xb1 = 0;
                            dv2.yb1 = 0;
                            dv2.xb2 = 0;
                            dv2.yb2 = 0;
                            switch (m_line_join)
                            {
                                case OutlineJoin.NoJoin:
                                    dv.flags = 3;
                                    break;
                                case OutlineJoin.Mitter:
                                case OutlineJoin.Round:
                                    dv.flags =
                                        (prev.DiagonalQuadrant == curr.DiagonalQuadrant ? 1 : 0) |
                                            ((curr.DiagonalQuadrant == next.DiagonalQuadrant ? 1 : 0) << 1);
                                    break;
                                case OutlineJoin.AccurateJoin:
                                    dv.flags = 0;
                                    break;
                            }

                            if (m_round_cap)
                            {
                                m_ren.SemiDot(CompareDistStart, x1, y1, x1 + (y2 - y1), y1 - (x2 - x1));
                            }
                            if ((dv.flags & 1) == 0)
                            {
                                if (m_line_join == OutlineJoin.Round)
                                {
                                    m_ren.Line3(prev, x1 + (y2 - y1), y1 - (x2 - x1),
                                                       x2 + (y2 - y1), y2 - (x2 - x1));
                                    m_ren.Pie(prev.x2, prev.y2,
                                               x2 + (y2 - y1), y2 - (x2 - x1),
                                                curr.x1 + (curr.y2 - curr.y1),
                                               curr.y1 - (curr.x2 - curr.x1));
                                }
                                else
                                {
                                    LineAA.Bisectrix(prev, curr, out dv2.xb1, out dv2.yb1);
                                    m_ren.Line3(prev, x1 + (y2 - y1), y1 - (x2 - x1),
                                                       dv2.xb1, dv2.yb1);
                                }
                            }
                            else
                            {
                                m_ren.Line1(prev,
                                             x1 + (y2 - y1),
                                             y1 - (x2 - x1));
                            }
                            if ((dv.flags & 2) == 0 && m_line_join != OutlineJoin.Round)
                            {
                                LineAA.Bisectrix(curr, next, out dv2.xb2, out dv2.yb2);
                            }

                            Draw(ref dv, ref dv1, ref dv2, ref curr, ref next, 1, m_src_vertices.Count - 2);
                            if ((dv.flags & 1) == 0)
                            {
                                if (m_line_join == OutlineJoin.Round)
                                {
                                    m_ren.Line3(curr,
                                                 curr.x1 + (curr.y2 - curr.y1),
                                                 curr.y1 - (curr.x2 - curr.x1),
                                                 curr.x2 + (curr.y2 - curr.y1),
                                                 curr.y2 - (curr.x2 - curr.x1));
                                }
                                else
                                {
                                    m_ren.Line3(curr, dv2.xb1, dv2.yb1,
                                                 curr.x2 + (curr.y2 - curr.y1),
                                                 curr.y2 - (curr.x2 - curr.x1));
                                }
                            }
                            else
                            {
                                m_ren.Line2(curr,
                                             curr.x2 + (curr.y2 - curr.y1),
                                             curr.y2 - (curr.x2 - curr.x1));
                            }
                            if (m_round_cap)
                            {
                                m_ren.SemiDot(CompareDistEnd, curr.x2, curr.y2,
                                               curr.x2 + (curr.y2 - curr.y1),
                                               curr.y2 - (curr.x2 - curr.x1));
                            }
                        }
                        break;
                }
            }

            m_src_vertices.Clear();
        }

        public void AddVertex(double x, double y, VertexCmd cmd)
        {
            switch (cmd)
            {
                case VertexCmd.NoMore:
                    {
                    }
                    break;
                case VertexCmd.MoveTo:
                    Render(false);
                    MoveTo(x, y);
                    break;

                case VertexCmd.Close:
                case VertexCmd.CloseAndEndFigure:
                    Render(true);
                    MoveTo(m_start_x, m_start_y);
                    break;
                default:
                    LineTo(x, y);
                    break;
            }
        }

        void AddPath(VertexStoreSnap s)
        {
            double x;
            double y;
            VertexCmd cmd;
            var snapIter = s.GetVertexSnapIter();
            while ((cmd = snapIter.GetNextVertex(out x, out y)) != VertexCmd.NoMore)
            {
                AddVertex(x, y, cmd);
            }



            Render(false);
        }
        public void RenderVertexSnap(VertexStoreSnap s, Drawing.Color c)
        {
            m_ren.Color = c;
            AddPath(s);
        }
        //public void RenderAllPaths(IVertexSource vs,
        //                      ColorRGBA[] colors,
        //                      int[] path_id,
        //                      int num_paths)
        //{
        //    for (int i = 0; i < num_paths; i++)
        //    {
        //        m_ren.color(colors[i]);
        //        AddPath(vs, path_id[i]);
        //    }
        //}

        /* // for debugging only
        public void render_path_index(IVertexSource vs,
                              RGBA_Bytes[] colors,
                              int[] path_id,
                              int pathIndex)
        {
            m_ren.color(colors[pathIndex]);
            add_path(vs, path_id[pathIndex]);
        }
         */
    };
}