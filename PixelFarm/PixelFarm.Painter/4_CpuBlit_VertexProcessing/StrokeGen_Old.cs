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
using PixelFarm.VectorMath;
using PixelFarm.Drawing;
namespace PixelFarm.CpuBlit.VertexProcessing
{



    class StrokeGenerator
    {

        /// <summary>
        /// stroke generator's status
        /// </summary>
        public enum Status
        {
            Init,
            Ready,
            Cap1,
            Cap2,
            Outline1,
            CloseFirst,
            Outline2,
            OutVertices,
            EndPoly1,
            EndPoly2,
            Stop
        }


        StrokeMath m_stroker;
        MultiPartsVertexList multipartVertexDistanceList = new MultiPartsVertexList();
        VertexStore m_out_vertices;
        double m_shorten;
        bool m_closed;
        Status m_status;
        Status m_prev_status;
        int m_src_vertex;
        int m_out_vertex;
        public StrokeGenerator()
        {
            m_stroker = new StrokeMath();
            m_out_vertices = new VertexStore();
            m_status = Status.Init;
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
            multipartVertexDistanceList.Clear();
            m_closed = false;
            m_status = Status.Init;
        }
        public void Close()
        {
            multipartVertexDistanceList.Close();
        }
        public void AddVertex(double x, double y, VertexCmd cmd)
        {
            //TODO: review 
            m_status = Status.Init;
            switch (cmd)
            {
                case VertexCmd.MoveTo:
                    multipartVertexDistanceList.AddMoveTo(x, y);
                    break;
                case VertexCmd.Close:
                case VertexCmd.CloseAndEndFigure:
                    //  m_closed = true;
                    multipartVertexDistanceList.Close();
                    break;
                default:
                    multipartVertexDistanceList.AddVertex(new Vertex2d(x, y));
                    break;
            }
        }

        public void WriteTo(VertexStore outputVxs)
        {

            this.Rewind();
            int currentRangeIndex = 0;
            double x = 0, y = 0;
            //int n = 0;
            for (; ; )
            {
                VertexCmd cmd = GetNextVertex(out x, out y);
                if (cmd == VertexCmd.NoMore)
                {
                    if (currentRangeIndex + 1 < multipartVertexDistanceList.RangeCount)
                    {
                        //move to next range
                        multipartVertexDistanceList.SetRangeIndex(currentRangeIndex + 1);
                        currentRangeIndex++;

                        m_status = Status.Ready;
                        m_src_vertex = 0;
                        m_out_vertex = 0;
                        continue;
                    }
                    else
                    {
                        break;//exit from loop
                    }
                }
                outputVxs.AddVertex(x, y, cmd);


                //Console.WriteLine(n + " " + x + "," + y);
                //n++;
                //if (n == 419)
                //{ 
                //}
            }
        }
        void Rewind()
        {
            if (m_status == Status.Init)
            {
                multipartVertexDistanceList.Rewind();
                if (multipartVertexDistanceList.CurrentRangeLen < 3)
                {
                    //force
                    m_closed = false;
                }
                //_curCurtexDistanceList.Close(m_closed);
                //VertexHelper.ShortenPath(_curCurtexDistanceList, m_shorten, m_closed);
                //if (_curCurtexDistanceList.Count < 3) { m_closed = false; }
            }
            m_status = Status.Ready;
            m_src_vertex = 0;
            m_out_vertex = 0;
            //if (_vertextDistanceListQueue.Count > 0)
            //{
            //    _vertextDistanceListQueue.Enqueue(_curCurtexDistanceList);
            //    //switch to first one
            //    _curCurtexDistanceList = _vertextDistanceListQueue.Dequeue();
            //}
            multipartVertexDistanceList.Rewind();
        }

        VertexCmd GetNextVertex(out double x, out double y)
        {
            x = 0; y = 0;
            VertexCmd cmd = VertexCmd.LineTo;
            do
            {
                switch (m_status)
                {
                    case Status.Init:
                        this.Rewind();
                        goto case Status.Ready;
                    case Status.Ready:

                        if (multipartVertexDistanceList.CurrentRangeLen < 2 + (m_closed ? 1 : 0))
                        {
                            cmd = VertexCmd.NoMore;
                            break;
                        }
                        m_status = m_closed ? Status.Outline1 : Status.Cap1;
                        cmd = VertexCmd.MoveTo;
                        m_src_vertex = 0;
                        m_out_vertex = 0;
                        break;
                    case Status.CloseFirst:
                        m_status = Status.Outline2;
                        cmd = VertexCmd.MoveTo;
                        goto case Status.Outline2;
                    case Status.Cap1:
                        {
                            Vertex2d v0, v1;

                            multipartVertexDistanceList.GetFirst2(out v0, out v1);
                            m_stroker.CreateCap(
                                m_out_vertices,
                                v0,
                                v1,
                                v0.CalLen(v1));

                            m_src_vertex = 1;
                            m_prev_status = Status.Outline1;
                            m_status = Status.OutVertices;
                            m_out_vertex = 0;
                        }
                        break;
                    case Status.Cap2:
                        {
                            Vertex2d beforeLast, last;
                            multipartVertexDistanceList.GetLast2(out beforeLast, out last);
                            m_stroker.CreateCap(m_out_vertices,
                                last,
                                beforeLast,
                                beforeLast.CalLen(last));
                            m_prev_status = Status.Outline2;
                            m_status = Status.OutVertices;
                            m_out_vertex = 0;
                        }
                        break;
                    case Status.Outline1:
                        {
                            if (m_closed)
                            {
                                if (m_src_vertex >= multipartVertexDistanceList.CurrentRangeLen)
                                {
                                    m_prev_status = Status.CloseFirst;
                                    m_status = Status.EndPoly1;
                                    break;
                                }
                            }
                            else
                            {
                                if (m_src_vertex >= multipartVertexDistanceList.CurrentRangeLen - 1)
                                {
                                    m_status = Status.Cap2;
                                    break;
                                }
                            }

                            Vertex2d prev, cur, next;
                            multipartVertexDistanceList.GetTripleVertices(m_src_vertex,
                                out prev,
                                out cur,
                                out next);
                            //check if we should join or not ?

                            //don't join it
                            m_stroker.CreateJoin(m_out_vertices,
                           prev,
                           cur,
                           next,
                           prev.CalLen(cur),
                           cur.CalLen(next));

                            ++m_src_vertex;
                            m_prev_status = m_status;
                            m_status = Status.OutVertices;
                            m_out_vertex = 0;

                        }
                        break;

                    case Status.Outline2:
                        {
                            if (m_src_vertex <= (!m_closed ? 1 : 0))
                            {
                                m_status = Status.EndPoly2;
                                m_prev_status = Status.Stop;
                                break;
                            }

                            --m_src_vertex;

                            Vertex2d prev, cur, next;
                            multipartVertexDistanceList.GetTripleVertices(m_src_vertex,
                                out prev,
                                out cur,
                                out next);

                            m_stroker.CreateJoin(m_out_vertices,
                              next,
                              cur,
                              prev,
                              cur.CalLen(next),
                              prev.CalLen(cur));
                            m_prev_status = m_status;
                            m_status = Status.OutVertices;
                            m_out_vertex = 0;

                        }
                        break;
                    case Status.OutVertices:
                        if (m_out_vertex >= m_out_vertices.Count)
                        {
                            m_status = m_prev_status;
                        }
                        else
                        {
                            m_out_vertices.GetVertex(m_out_vertex++, out x, out y);
                            return cmd;
                        }
                        break;
                    case Status.EndPoly1:
                        m_status = m_prev_status;
                        x = (int)EndVertexOrientation.CCW;
                        y = 0;
                        return VertexCmd.Close;
                    case Status.EndPoly2:
                        m_status = m_prev_status;
                        x = (int)EndVertexOrientation.CW;
                        y = 0;
                        return VertexCmd.Close;
                    case Status.Stop:
                        cmd = VertexCmd.NoMore;
                        break;
                }

            } while (!VertexHelper.IsEmpty(cmd));
            return cmd;
        }
    }

    class MultiPartsVertexList
    {
        //TODO make this struct
        class Range
        {
            public int beginAt;
            public int len;
            public Range(int beginAt)
            {
                this.beginAt = beginAt;
                this.len = 0;
            }
            public int Count
            {
                get { return len; }
            }
            public void SetLen(int len)
            {
                this.len = len;
            }
            public void SetEndAt(int endAt)
            {
                this.len = endAt - beginAt;
            }
        }


        Vertex2d _latestVertex = new Vertex2d();

        List<Vertex2d> _vertextDistanceList = new List<Vertex2d>();
        List<Range> _ranges = new List<Range>(); //prev ranges

        Range _latestRange; //current range (before each close)
        int _rangeIndex = 0;//point to reading index in to _ranges List

        double _latestMoveToX;
        double _latestMoveToY;

        public MultiPartsVertexList()
        {

        }
        public void AddVertex(Vertex2d val)
        {
            int count = _latestRange.Count;
            if (count == 0)
            {
                _vertextDistanceList.Add(_latestVertex = val);
                _latestRange.SetLen(count + 1);
            }
            else
            {
                //Ensure that the new one is not duplicate with the last one
                if (!_latestVertex.IsEqual(val))
                {
                    _latestRange.SetLen(count + 1);
                    _vertextDistanceList.Add(_latestVertex = val);
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
            //TODO: review here
            //1. stop current range
            if (_ranges.Count > 0)
            {
                //update end of latest range
                _ranges[_ranges.Count - 1].SetEndAt(_vertextDistanceList.Count);
            }

            //start new range with x and y
            _ranges.Add(_latestRange = new Range(_vertextDistanceList.Count));
            AddVertex(new Vertex2d(x, y));
            _latestMoveToX = x;
            _latestMoveToY = y;
        }

        public int RangeIndex { get { return this._rangeIndex; } }
        public void SetRangeIndex(int index)
        {
            this._rangeIndex = index;
            _latestRange = _ranges[index];
        }
        public int RangeCount
        {
            get { return _ranges.Count; }
        }
        public int CurrentRangeLen
        {
            get
            {
                return (_latestRange == null) ? 0 : _latestRange.len;
            }
        }


        public void Clear()
        {
            _ranges.Clear();
            _vertextDistanceList.Clear();
            _latestVertex = new Vertex2d();
            _rangeIndex = 0;
            _latestRange = null;
        }
        public void Rewind()
        {
            _rangeIndex = 0;
            if (_ranges.Count > 0)
            {
                _latestRange = _ranges[_rangeIndex];
            }
        }

        //public void ReplaceLast(Vertex2d val)
        //{
        //    _vertextDistanceList.RemoveAt(_vertextDistanceList.Count - 1);
        //    AddVertex(val);
        //}
        public void GetTripleVertices(int idx, out Vertex2d prev, out Vertex2d cur, out Vertex2d next)
        {
            //we want 3 vertices
            if (idx > 0 && idx + 2 <= _latestRange.Count)
            {
                prev = _vertextDistanceList[_latestRange.beginAt + idx - 1];
                cur = _vertextDistanceList[_latestRange.beginAt + idx];
                next = _vertextDistanceList[_latestRange.beginAt + idx + 1];

            }
            else
            {
                prev = cur = next = new Vertex2d();
            }
        }
        public void GetFirst2(out Vertex2d first, out Vertex2d second)
        {
            first = _vertextDistanceList[_latestRange.beginAt];
            second = _vertextDistanceList[_latestRange.beginAt + 1];

        }
        public void GetLast2(out Vertex2d beforeLast, out Vertex2d last)
        {
            beforeLast = _vertextDistanceList[_latestRange.beginAt + _latestRange.len - 2];
            last = _vertextDistanceList[_latestRange.beginAt + _latestRange.len - 1];

        }
    }

}