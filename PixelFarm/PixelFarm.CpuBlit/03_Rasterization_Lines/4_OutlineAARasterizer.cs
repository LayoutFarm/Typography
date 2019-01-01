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

//Maxim's note on C++ version:
//see https://pdfium.googlesource.com/pdfium/+/master/third_party/agg23/agg_rasterizer_scanline_aa.cpp#35
// ...
// The author gratefully acknowleges the support of David Turner,
// Robert Wilhelm, and Werner Lemberg - the authors of the FreeType
// libray - in producing this work. See http://www.freetype.org for details.
//
// Initially the rendering algorithm was designed by David Turner and the
// other authors of the FreeType library - see the above notice. I nearly
// created a similar renderer, but still I was far from David's work.
// I completely redesigned the original code and adapted it for Anti-Grain
// ideas. Two functions - render_line and render_hline are the core of
// the algorithm - they calculate the exact coverage of each pixel cell
// of the polygon. I left these functions almost as is, because there's
// no way to improve the perfection - hats off to David and his group!
//
// All other code is very different from the original.
// 
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
        ArrayList<LineAAVertex> _list = new ArrayList<LineAAVertex>();
        public void AddVertex(LineAAVertex val)
        {
            int count = _list.Count;
            if (count > 1)
            {
                LineAAVertex[] innerArray = _list.UnsafeInternalArray;
                if (!innerArray[count - 2].IsDiff(innerArray[count - 1]))
                {
                    _list.RemoveLast();
                }
            }
            _list.Append(val);
        }
        //
        public LineAAVertex this[int index] => _list[index];
        public int Count => _list.Count;
        //
        public void Clear() { _list.Clear(); }
        //
        public void ModifyLast(LineAAVertex val)
        {
            _list.RemoveLast();
            AddVertex(val);
        }

        public void Close(bool closed)
        {
            //----------------------
            //iter backward
            int count = _list.Count;
            var innerArray = _list.UnsafeInternalArray;
            while (count > 1)
            {
                if (innerArray[count - 2].IsDiff(innerArray[count - 1]))
                {
                    break;
                }
                else
                {
                    LineAAVertex t = _list[count - 1];
                    _list.RemoveLast();
                    ModifyLast(t);
                    count--;
                }
            }


            if (closed)
            {
                //if close figure
                count = _list.Count;
                var first = innerArray[0];
                while (count > 1)
                {
                    if (innerArray[count - 1].IsDiff(first))
                    {
                        break;
                    }
                    count--;
                    _list.RemoveLast();
                }
            }
        }
    }

    //=======================================================rasterizer_outline_aa
    public class OutlineAARasterizer
    {
        LineRenderer _ren;
        LineAAVertexSequence _src_vertices = new LineAAVertexSequence();
        OutlineJoin _line_join;
        bool _round_cap;
        int _start_x;
        int _start_y;
        public enum OutlineJoin
        {
            NoJoin,             //-----outline_no_join
            Mitter,          //-----outline_miter_join
            Round,          //-----outline_round_join
            AccurateJoin  //-----outline_accurate_join
        }

        public bool CompareDistStart(int d) => d > 0;
        public bool CompareDistEnd(int d) => d <= 0;

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

#if DEBUG
        static int dbuglatest_i = 0;
#endif
        void Draw(ref DrawVarsPart0 dv,
            ref DrawVarsPart1 dv1,
            ref DrawVarsPart2 dv2,
            ref LineParameters curr,
            ref LineParameters next,
            int start,
            int end)
        {

            try
            {
                for (int i = start; i < end; i++)
                {

#if DEBUG
                    dbuglatest_i = i;
                    if (i == 6)
                    {

                    }
#endif
                    if (_line_join == OutlineJoin.Round)
                    {
                        dv2.xb1 = curr.x1 + (curr.y2 - curr.y1);
                        dv2.yb1 = curr.y1 - (curr.x2 - curr.x1);
                        dv2.xb2 = curr.x2 + (curr.y2 - curr.y1);
                        dv2.yb2 = curr.y2 - (curr.x2 - curr.x1);
                    }

                    switch (dv.flags)
                    {
                        case 0: _ren.Line3(curr, dv2.xb1, dv2.yb1, dv2.xb2, dv2.yb2); break;
                        case 1: _ren.Line2(curr, dv2.xb2, dv2.yb2); break;
                        case 2: _ren.Line1(curr, dv2.xb1, dv2.yb1); break;
                        case 3: _ren.Line0(curr); break;
                    }

                    if (_line_join == OutlineJoin.Round && (dv.flags & 2) == 0)
                    {
                        _ren.Pie(curr.x2, curr.y2,
                                   curr.x2 + (curr.y2 - curr.y1),
                                   curr.y2 - (curr.x2 - curr.x1),
                                   curr.x2 + (next.y2 - next.y1),
                                   curr.y2 - (next.x2 - next.x1));
                    }

                    dv1.x1 = dv1.x2;
                    dv1.y1 = dv1.y2;
                    dv.lcurr = dv.lnext;
                    dv.lnext = _src_vertices[dv.idx].len;
                    ++dv.idx;
                    if (dv.idx >= _src_vertices.Count) dv.idx = 0;
                    dv1.x2 = _src_vertices[dv.idx].x;
                    dv1.y2 = _src_vertices[dv.idx].y;
                    curr = next;
                    next = new LineParameters(dv1.x1, dv1.y1, dv1.x2, dv1.y2, dv.lnext);
                    dv2.xb1 = dv2.xb2;
                    dv2.yb1 = dv2.yb2;
                    switch (_line_join)
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
            catch (Exception ex)
            {

            }

        }

        public OutlineAARasterizer(LineRenderer ren)
        {
            _ren = ren;
            _line_join = (OutlineRenderer.AccurateJoinOnly ?
                            OutlineJoin.AccurateJoin :
                            OutlineJoin.Round);
            _round_cap = (false);
            _start_x = (0);
            _start_y = (0);
        }

        public void Attach(LineRenderer ren) { _ren = ren; }


        public OutlineJoin LineJoin
        {
            get { return _line_join; }
            set
            {
                _line_join = OutlineRenderer.AccurateJoinOnly ?
                OutlineJoin.AccurateJoin : value;
            }
        }

        public bool RoundCap
        {
            get => _round_cap;
            set => _round_cap = value;
        }
        public void MoveTo(int x, int y)
        {
            _src_vertices.ModifyLast(new LineAAVertex(_start_x = x, _start_y = y));
        }

        public void LineTo(int x, int y)
        {
            _src_vertices.AddVertex(new LineAAVertex(x, y));
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
            _src_vertices.Close(close_polygon);
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
                if (_src_vertices.Count >= 3)
                {
                    dv.idx = 2;
                    v = _src_vertices[_src_vertices.Count - 1];
                    x1 = v.x;
                    y1 = v.y;
                    lprev = v.len;
                    v = _src_vertices[0];
                    x2 = v.x;
                    y2 = v.y;
                    dv.lcurr = v.len;
                    LineParameters prev = new LineParameters(x1, y1, x2, y2, lprev);
                    v = _src_vertices[1];
                    dv1.x1 = v.x;
                    dv1.y1 = v.y;
                    dv.lnext = v.len;
                    curr = new LineParameters(x2, y2, dv1.x1, dv1.y1, dv.lcurr);
                    v = _src_vertices[dv.idx];
                    dv1.x2 = v.x;
                    dv1.y2 = v.y;
                    next = new LineParameters(dv1.x1, dv1.y1, dv1.x2, dv1.y2, dv.lnext);
                    dv2.xb1 = 0;
                    dv2.yb1 = 0;
                    dv2.xb2 = 0;
                    dv2.yb2 = 0;
                    switch (_line_join)
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

                    if (_line_join != OutlineJoin.Round)
                    {
                        if ((dv.flags & 1) == 0)
                        {
                            LineAA.Bisectrix(prev, curr, out dv2.xb1, out dv2.yb1);
                        }

                        if ((dv.flags & 2) == 0)
                        {
                            LineAA.Bisectrix(curr, next, out dv2.xb2, out dv2.yb2);
                        }
                    }

                    Draw(ref dv, ref dv1, ref dv2, ref curr, ref next, 0, _src_vertices.Count);
                }
            }
            else
            {
                switch (_src_vertices.Count)
                {
                    case 0:
                    case 1:
                        break;
                    case 2:
                        {
                            v = _src_vertices[0];
                            x1 = v.x;
                            y1 = v.y;
                            lprev = v.len;
                            v = _src_vertices[1];
                            x2 = v.x;
                            y2 = v.y;
                            LineParameters lp = new LineParameters(x1, y1, x2, y2, lprev);
                            if (_round_cap)
                            {
                                _ren.SemiDot(CompareDistStart, x1, y1, x1 + (y2 - y1), y1 - (x2 - x1));
                            }
                            _ren.Line3(lp,
                                         x1 + (y2 - y1),
                                         y1 - (x2 - x1),
                                         x2 + (y2 - y1),
                                         y2 - (x2 - x1));
                            if (_round_cap)
                            {
                                _ren.SemiDot(CompareDistEnd, x2, y2, x2 + (y2 - y1), y2 - (x2 - x1));
                            }
                        }
                        break;
                    case 3:
                        {
                            int x3, y3;
                            int lnext;
                            v = _src_vertices[0];
                            x1 = v.x;
                            y1 = v.y;
                            lprev = v.len;
                            v = _src_vertices[1];
                            x2 = v.x;
                            y2 = v.y;
                            lnext = v.len;
                            v = _src_vertices[2];
                            x3 = v.x;
                            y3 = v.y;

                            LineParameters lp1 = new LineParameters(x1, y1, x2, y2, lprev);
                            LineParameters lp2 = new LineParameters(x2, y2, x3, y3, lnext);
                            if (_round_cap)
                            {
                                _ren.SemiDot(CompareDistStart, x1, y1, x1 + (y2 - y1), y1 - (x2 - x1));
                            }

                            if (_line_join == OutlineJoin.Round)
                            {
                                _ren.Line3(lp1, x1 + (y2 - y1), y1 - (x2 - x1),
                                                  x2 + (y2 - y1), y2 - (x2 - x1));
                                _ren.Pie(x2, y2, x2 + (y2 - y1), y2 - (x2 - x1),
                                                   x2 + (y3 - y2), y2 - (x3 - x2));
                                _ren.Line3(lp2, x2 + (y3 - y2), y2 - (x3 - x2),
                                                  x3 + (y3 - y2), y3 - (x3 - x2));
                            }
                            else
                            {
                                LineAA.Bisectrix(lp1, lp2, out dv2.xb1, out dv2.yb1);
                                _ren.Line3(lp1, x1 + (y2 - y1), y1 - (x2 - x1),
                                                  dv2.xb1, dv2.yb1);
                                _ren.Line3(lp2, dv2.xb1, dv2.yb1,
                                                  x3 + (y3 - y2), y3 - (x3 - x2));
                            }
                            if (_round_cap)
                            {
                                _ren.SemiDot(CompareDistEnd, x3, y3, x3 + (y3 - y2), y3 - (x3 - x2));
                            }
                        }
                        break;
                    default:
                        {
                            dv.idx = 3;
                            v = _src_vertices[0];
                            x1 = v.x;
                            y1 = v.y;
                            lprev = v.len;
                            v = _src_vertices[1];
                            x2 = v.x;
                            y2 = v.y;
                            dv.lcurr = v.len;
                            var prev = new LineParameters(x1, y1, x2, y2, lprev);
                            v = _src_vertices[2];
                            dv1.x1 = v.x;
                            dv1.y1 = v.y;
                            dv.lnext = v.len;
                            curr = new LineParameters(x2, y2, dv1.x1, dv1.y1, dv.lcurr);
                            v = _src_vertices[dv.idx];
                            dv1.x2 = v.x;
                            dv1.y2 = v.y;
                            next = new LineParameters(dv1.x1, dv1.y1, dv1.x2, dv1.y2, dv.lnext);
                            dv2.xb1 = 0;
                            dv2.yb1 = 0;
                            dv2.xb2 = 0;
                            dv2.yb2 = 0;
                            switch (_line_join)
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

                            if (_round_cap)
                            {
                                _ren.SemiDot(CompareDistStart, x1, y1, x1 + (y2 - y1), y1 - (x2 - x1));
                            }
                            if ((dv.flags & 1) == 0)
                            {
                                if (_line_join == OutlineJoin.Round)
                                {
                                    _ren.Line3(prev, x1 + (y2 - y1), y1 - (x2 - x1),
                                                       x2 + (y2 - y1), y2 - (x2 - x1));
                                    _ren.Pie(prev.x2, prev.y2,
                                               x2 + (y2 - y1), y2 - (x2 - x1),
                                                curr.x1 + (curr.y2 - curr.y1),
                                               curr.y1 - (curr.x2 - curr.x1));
                                }
                                else
                                {
                                    LineAA.Bisectrix(prev, curr, out dv2.xb1, out dv2.yb1);
                                    _ren.Line3(prev, x1 + (y2 - y1), y1 - (x2 - x1),
                                                       dv2.xb1, dv2.yb1);
                                }
                            }
                            else
                            {
                                _ren.Line1(prev,
                                             x1 + (y2 - y1),
                                             y1 - (x2 - x1));
                            }

                            if ((dv.flags & 2) == 0 && _line_join != OutlineJoin.Round)
                            {
                                LineAA.Bisectrix(curr, next, out dv2.xb2, out dv2.yb2);
                            }

                            Draw(ref dv, ref dv1, ref dv2, ref curr, ref next, 1, _src_vertices.Count - 2);
                            if ((dv.flags & 1) == 0)
                            {
                                if (_line_join == OutlineJoin.Round)
                                {
                                    _ren.Line3(curr,
                                                 curr.x1 + (curr.y2 - curr.y1),
                                                 curr.y1 - (curr.x2 - curr.x1),
                                                 curr.x2 + (curr.y2 - curr.y1),
                                                 curr.y2 - (curr.x2 - curr.x1));
                                }
                                else
                                {
                                    _ren.Line3(curr, dv2.xb1, dv2.yb1,
                                                 curr.x2 + (curr.y2 - curr.y1),
                                                 curr.y2 - (curr.x2 - curr.x1));
                                }
                            }
                            else
                            {
                                _ren.Line2(curr,
                                             curr.x2 + (curr.y2 - curr.y1),
                                             curr.y2 - (curr.x2 - curr.x1));
                            }
                            if (_round_cap)
                            {
                                _ren.SemiDot(CompareDistEnd, curr.x2, curr.y2,
                                               curr.x2 + (curr.y2 - curr.y1),
                                               curr.y2 - (curr.x2 - curr.x1));
                            }
                        }
                        break;
                }
            }

            _src_vertices.Clear();
        }

        public void AddVertex(double x, double y, VertexCmd cmd)
        {
            switch (cmd)
            {
                case VertexCmd.NoMore:
                    //do nothing
                    return;
                case VertexCmd.MoveTo:
                    Render(false);
                    MoveTo(x, y);
                    break;

                case VertexCmd.Close:
                case VertexCmd.CloseAndEndFigure:
                    Render(true);
                    MoveTo(_start_x, _start_y);
                    break;
                default:
                    LineTo(x, y);
                    break;
            }
        }

#if DEBUG
        static int dbugAddPathCount = 0;
#endif
        void AddPath(VertexStore vxs)
        {
            double x;
            double y;
            VertexCmd cmd;
            int index = 0;
            while ((cmd = vxs.GetVertex(index++, out x, out y)) != VertexCmd.NoMore)
            {
                AddVertex(x, y, cmd);
            }
            Render(false);
        }
        public void RenderVertexSnap(VertexStore s, Drawing.Color c)
        {
            _ren.Color = c;
            AddPath(s);
        }
    }
}