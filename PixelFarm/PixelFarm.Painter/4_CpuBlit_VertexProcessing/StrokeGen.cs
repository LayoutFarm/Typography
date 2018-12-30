//BSD, 2014-present, WinterDev_
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

        StrokeMath _stroker;
        Vertex2dList _vtx2dList = new Vertex2dList();
        VertexStore _tmpVxs;
        double _shorten;
        bool _closed;
        public StrokeGenerator()
        {
            _stroker = new StrokeMath();
            _tmpVxs = new VertexStore();
            _closed = false;
        }

        public LineCap LineCap
        {
            get => _stroker.LineCap;
            set => _stroker.LineCap = value;
        }
        public LineJoin LineJoin
        {
            get => _stroker.LineJoin;
            set => _stroker.LineJoin = value;
        }
        public InnerJoin InnerJoin
        {
            get => _stroker.InnerJoin;
            set => _stroker.InnerJoin = value;
        }

        public double Width
        {
            get => _stroker.Width;
            set => _stroker.Width = value;
        }
        public void SetMiterLimitTheta(double t) => _stroker.SetMiterLimitTheta(t);

        public double InnerMiterLimit
        {
            get => _stroker.InnerMiterLimit;
            set => _stroker.InnerMiterLimit = value;
        }
        public double MiterLimit
        {
            get => _stroker.InnerMiterLimit;
            set => _stroker.InnerMiterLimit = value;
        }
        public double ApproximateScale
        {
            get => _stroker.ApproximateScale;
            set => _stroker.ApproximateScale = value;
        }
        public bool AutoDetectOrientation
        {
            get => throw new NotSupportedException();
            set { throw new NotSupportedException(); }
        }
        public double Shorten
        {
            get => _shorten;
            set => _shorten = value;
        }
        // Vertex Generator Interface
        public void Reset()
        {
            _vtx2dList.Clear();
            _closed = false;
        }
        public void Close()
        {
            _closed = true;
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
                _closed = false;
            }

            //ready
            if (_vtx2dList.Count < 2 + (_closed ? 1 : 0))
            {
                return;
            }

            //[A]
            //we start at cap1 
            //check if close polygon or not
            //if lines( not close) => then start with some kind of line cap
            //if closed polygon => start with outline

            int m_src_vertex = 0;
            double latest_moveX = 0;
            double latest_moveY = 0;

            int _latestFigBeginAt = output.Count;

            if (!_closed)
            {
                //[B] cap1

                _vtx2dList.GetFirst2(out Vertex2d v0, out Vertex2d v1);
                _stroker.CreateCap(
                    _tmpVxs,
                    v0,
                    v1);

                _tmpVxs.GetVertex(0, out latest_moveX, out latest_moveY);
                AppendVertices(output, _tmpVxs);
            }
            else
            {

                //[C]
                _vtx2dList.GetFirst2(out Vertex2d v0, out Vertex2d v1);
                _vtx2dList.GetLast2(out Vertex2d v_beforeLast, out Vertex2d v_last);

                if (v_last.x == v0.x && v_last.y == v0.y)
                {
                    v_last = v_beforeLast;
                }

                // v_last-> v0-> v1
                _stroker.CreateJoin(_tmpVxs,
                    v_last,
                    v0,
                    v1);
                _tmpVxs.GetVertex(0, out latest_moveX, out latest_moveY);
                output.AddMoveTo(latest_moveX, latest_moveY);
                //others 
                AppendVertices(output, _tmpVxs, 1);

            }
            //----------------
            m_src_vertex = 1;
            //----------------

            //[D] draw lines until end cap ***

            while (m_src_vertex < _vtx2dList.Count - 1)
            {

                _vtx2dList.GetTripleVertices(m_src_vertex,
                    out Vertex2d prev,
                    out Vertex2d cur,
                    out Vertex2d next);
                //check if we should join or not ?


                _stroker.CreateJoin(_tmpVxs,
                   prev,
                   cur,
                   next);

                ++m_src_vertex;


                AppendVertices(output, _tmpVxs);
            }

            //[E] draw end line
            {
                if (!_closed)
                {

                    _vtx2dList.GetLast2(out Vertex2d beforeLast, out Vertex2d last);
                    _stroker.CreateCap(_tmpVxs,
                        last, //**please note different direction (compare with above)
                        beforeLast);

                    AppendVertices(output, _tmpVxs);
                }
                else
                {

                    output.GetVertex(_latestFigBeginAt, out latest_moveX, out latest_moveY);
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

                    _stroker.CreateJoin(_tmpVxs,
                        v1,
                        v0,
                        v_last);


                    _tmpVxs.GetVertex(0, out latest_moveX, out latest_moveY);
                    output.AddMoveTo(latest_moveX, latest_moveY);
                    //others 
                    AppendVertices(output, _tmpVxs, 1);

                    _latestFigBeginAt = output.Count;
                }
            }


            //----------------------------------
            //[F] and turn back and run to begin***


            --m_src_vertex;
            while (m_src_vertex > 0)
            {


                _vtx2dList.GetTripleVertices(m_src_vertex,
                    out Vertex2d prev,
                    out Vertex2d cur,
                    out Vertex2d next);

                _stroker.CreateJoin(_tmpVxs,
                  next, //**please note different direction (compare with above)
                  cur,
                  prev);

                --m_src_vertex;

                AppendVertices(output, _tmpVxs);
            }


            if (!_closed)
            {
                output.GetVertex(_latestFigBeginAt, out latest_moveX, out latest_moveY);
                output.AddLineTo(latest_moveX, latest_moveY);
            }
        }



        class Vertex2dList
        {
            Vertex2d _latestVertex = new Vertex2d();
            ArrayList<Vertex2d> _list = new ArrayList<Vertex2d>();
            double _latestMoveToX;
            double _latestMoveToY;
            bool _empty = true;
            public Vertex2dList()
            {

            }

            public int Count => _list.Count;

            public void AddVertex(Vertex2d val)
            {

                if (_empty)
                {
                    _list.Append(_latestVertex = val);
                    _empty = false;
                }
                else
                {
                    //Ensure that the new one is not duplicate with the last one
                    if (!_latestVertex.IsEqual(val))
                    {
                        _list.Append(_latestVertex = val);
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
                _empty = true;
                _list.Clear();
                _latestVertex = new Vertex2d();
                _latestMoveToX = _latestMoveToY = 0;
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