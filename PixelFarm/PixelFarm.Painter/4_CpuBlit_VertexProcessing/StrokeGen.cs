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

using System;
using System.Collections.Generic;
using PixelFarm.Drawing;
namespace PixelFarm.CpuBlit.VertexProcessing
{



    class StrokeGenerator
    {

        StrokeMath m_stroker;
        Vertex2dList _vtx2dList = new Vertex2dList();
        VertexStore m_out_vertices;
        double m_shorten;
        bool m_closed;


        public StrokeGenerator()
        {
            m_stroker = new StrokeMath();
            m_out_vertices = new VertexStore();

            m_closed = false;
        }

        public LineCap LineCap
        {
            get { return this.m_stroker.LineCap; }
            set { this.m_stroker.LineCap = value; }
        }
        public LineJoin LineJoin
        {
            get { return this.m_stroker.LineJoin; }
            set { this.m_stroker.LineJoin = value; }
        }
        public InnerJoin InnerJoin
        {
            get { return this.m_stroker.InnerJoin; }
            set { this.m_stroker.InnerJoin = value; }
        }

        public double Width
        {
            get { return m_stroker.Width; }
            set { this.m_stroker.Width = value; }
        }
        public void SetMiterLimitTheta(double t) { m_stroker.SetMiterLimitTheta(t); }


        public double InnerMiterLimit
        {
            get { return this.m_stroker.InnerMiterLimit; }
            set { this.m_stroker.InnerMiterLimit = value; }
        }
        public double MiterLimit
        {
            get { return this.m_stroker.InnerMiterLimit; }
            set { this.m_stroker.InnerMiterLimit = value; }
        }
        public double ApproximateScale
        {
            get { return this.m_stroker.ApproximateScale; }
            set { this.m_stroker.ApproximateScale = value; }
        }
        public bool AutoDetectOrientation
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }
        public double Shorten
        {
            get { return this.m_shorten; }
            set { this.m_shorten = value; }
        }
        // Vertex Generator Interface
        public void Reset()
        {
            _vtx2dList.Clear();
            m_closed = false;
        }
        public void Close()
        {
            m_closed = true;
            _vtx2dList.Close();
        }
        public void AddVertex(double x, double y, VertexCmd cmd)
        {
            //TODO: review 

            switch (cmd)
            {
                case VertexCmd.MoveTo:
                    _vtx2dList.AddMoveTo(x, y);
                    break;
                case VertexCmd.Close:
                case VertexCmd.CloseAndEndFigure:
                    //  m_closed = true;
                    _vtx2dList.Close();
                    break;
                default:
                    _vtx2dList.AddVertex(new Vertex2d(x, y));
                    break;
            }
        }
        public void WriteTo(VertexStore outputVxs)
        {
            GenStroke(outputVxs);
        }


        void AppendVertices(VertexStore dest, VertexStore src, int src_index = 0)
        {
            int j = src.Count;
            for (int i = src_index; i < j; ++i)
            {
                VertexCmd cmd = src.GetVertex(i, out double x, out double y);
                if (cmd != VertexCmd.NoMore)
                {
                    dest.AddVertex(x, y, cmd);
                }
            }
        }
        void GenStroke(VertexStore output)
        {
            //agg_vcgen_stroke.cpp 

            if (_vtx2dList.Count < 3)
            {
                //force
                m_closed = false;
            }

            //ready
            if (_vtx2dList.Count < 2 + (m_closed ? 1 : 0))
            {
                return;
            }

            //
            //we start at cap1 
            //check if close polygon or not
            //if lines( not close) => then start with some kind of line cap
            //if closed polygon => start with outline
            int m_src_vertex = 0;

            double latest_moveX = 0;
            double latest_moveY = 0;

            if (!m_closed)
            {
                //cap1

                _vtx2dList.GetFirst2(out Vertex2d v0, out Vertex2d v1);
                m_stroker.CreateCap(
                    m_out_vertices,
                    v0,
                    v1,
                    v0.CalLen(v1));

                m_out_vertices.GetVertex(0, out latest_moveX, out latest_moveY);
                AppendVertices(output, m_out_vertices);
            }
            else
            {


                _vtx2dList.GetFirst2(out Vertex2d v0, out Vertex2d v1);
                _vtx2dList.GetLast2(out Vertex2d v_beforeLast, out Vertex2d v_last);

                if (v_last.x == v0.x && v_last.y == v0.y)
                {
                    v_last = v_beforeLast;
                }

                // v_last-> v0-> v1
                m_stroker.CreateJoin(m_out_vertices,
                    v_last,
                    v0,
                    v1,
                    v_last.CalLen(v0),
                    v0.CalLen(v1));
                m_out_vertices.GetVertex(0, out latest_moveX, out latest_moveY);
                output.AddMoveTo(latest_moveX, latest_moveY);
                //others 
                AppendVertices(output, m_out_vertices, 1);

            }
            //----------------
            m_src_vertex = 1;
            //----------------

            //line until end cap 
            while (m_src_vertex < _vtx2dList.Count - 1)
            {

                _vtx2dList.GetTripleVertices(m_src_vertex,
                    out Vertex2d prev,
                    out Vertex2d cur,
                    out Vertex2d next);
                //check if we should join or not ?

                //don't join it
                m_stroker.CreateJoin(m_out_vertices,
                   prev,
                   cur,
                   next,
                   prev.CalLen(cur),
                   cur.CalLen(next));

                ++m_src_vertex;


                AppendVertices(output, m_out_vertices);
            }

            //draw end line
            {
                if (!m_closed)
                {

                    _vtx2dList.GetLast2(out Vertex2d beforeLast, out Vertex2d last);
                    m_stroker.CreateCap(m_out_vertices,
                        last, //**please note different direction (compare with above)
                        beforeLast,
                        beforeLast.CalLen(last));

                    AppendVertices(output, m_out_vertices);
                }
                else
                {

                    output.GetVertex(0, out latest_moveX, out latest_moveY);
                    output.AddLineTo(latest_moveX, latest_moveY);
                    output.AddCloseFigure();
                    //begin inner
                    //move to inner 

                    // v_last <- v0 <- v1

                    _vtx2dList.GetFirst2(out Vertex2d v0, out Vertex2d v1);
                    _vtx2dList.GetLast2(out Vertex2d v_beforeLast, out Vertex2d v_last);

                    if (v_last.x == v0.x && v_last.y == v0.y)
                    {
                        v_last = v_beforeLast;
                    }

                    //**please note different direction (compare with above)

                    m_stroker.CreateJoin(m_out_vertices,
                        v1,
                        v0,
                        v_last,
                        v1.CalLen(v0),
                        v0.CalLen(v_last));


                    m_out_vertices.GetVertex(0, out latest_moveX, out latest_moveY);
                    output.AddMoveTo(latest_moveX, latest_moveY);
                    //others 
                    AppendVertices(output, m_out_vertices, 1);

                }
            }
            //and turn back to begin
            --m_src_vertex;
            while (m_src_vertex > 0)
            {


                _vtx2dList.GetTripleVertices(m_src_vertex,
                    out Vertex2d prev,
                    out Vertex2d cur,
                    out Vertex2d next);

                m_stroker.CreateJoin(m_out_vertices,
                  next, //**please note different direction (compare with above)
                  cur,
                  prev,
                  cur.CalLen(next),
                  prev.CalLen(cur));

                --m_src_vertex;

                AppendVertices(output, m_out_vertices);
            }

            {
                if (!m_closed)
                {
                    output.GetVertex(0, out latest_moveX, out latest_moveY);

                }
                output.AddLineTo(latest_moveX, latest_moveY);
                output.AddCloseFigure();

            }
        }



        class Vertex2dList
        {
            Vertex2d _latestVertex = new Vertex2d();
            List<Vertex2d> _list = new List<Vertex2d>();

            double _latestMoveToX;
            double _latestMoveToY;

            public Vertex2dList()
            {
            }
            public int Count
            {
                get { return _list.Count; }
            }
            public void AddVertex(Vertex2d val)
            {
                int count = _list.Count;
                if (count == 0)
                {
                    _list.Add(_latestVertex = val);
                }
                else
                {
                    //Ensure that the new one is not duplicate with the last one
                    if (!_latestVertex.IsEqual(val))
                    {

                        _list.Add(_latestVertex = val);
                    }
                }

            }
            public void Close()
            {
                //close current range
                AddVertex(new Vertex2d(_latestMoveToX, _latestMoveToY));

            }
            public void AddMoveTo(double x, double y)
            {

                AddVertex(new Vertex2d(x, y));
                _latestMoveToX = x;
                _latestMoveToY = y;
            }
            public void Clear()
            {
                //_ranges.Clear();
                _list.Clear();
                _latestVertex = new Vertex2d();
            }

            public void GetTripleVertices(int idx, out Vertex2d prev, out Vertex2d cur, out Vertex2d next)
            {
                //we want 3 vertices
                if (idx > 0 && idx + 2 <= _list.Count)
                {
                    prev = _list[idx - 1];
                    cur = _list[idx];
                    next = _list[idx + 1];
                }
                else
                {
                    prev = cur = next = new Vertex2d();
                }
            }
            public void GetFirst2(out Vertex2d first, out Vertex2d second)
            {
                first = _list[0];
                second = _list[1];
            }
            public void GetLast2(out Vertex2d beforeLast, out Vertex2d last)
            {
                beforeLast = _list[_list.Count - 2];
                last = _list[_list.Count - 1];
            }

        }
    }
}