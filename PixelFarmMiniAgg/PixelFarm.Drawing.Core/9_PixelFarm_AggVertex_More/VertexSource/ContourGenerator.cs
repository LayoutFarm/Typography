//BSD, 2014-2017, WinterDev

namespace PixelFarm.Agg.VertexSource
{
    class ContourGenerator
    {
        StrokeMath m_stroker;
        double m_width;
        VertexDistanceList vertexDistanceList;
        VertexStore m_out_vertices;
        StrokeMath.Status m_status;
        int m_src_vertex;
        int m_out_vertex;
        bool m_closed;
        EndVertexOrientation m_orientation;
        bool m_auto_detect;
        double m_shorten;
        public ContourGenerator()
        {
            m_stroker = new StrokeMath();
            m_width = 1;
            vertexDistanceList = new VertexDistanceList();
            m_out_vertices = new VertexStore();
            m_status = StrokeMath.Status.Init;
            m_src_vertex = 0;
            m_closed = false;
            m_orientation = 0;
            m_auto_detect = false;
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
        public double MiterLimit
        {
            get { return this.m_stroker.MiterLimit; }
            set { this.m_stroker.MiterLimit = value; }
        }
        public double InnerMiterLimit
        {
            get { return this.m_stroker.InnerMiterLimit; }
            set { this.m_stroker.InnerMiterLimit = value; }
        }


        public void SetMiterLimitTheta(double t) { m_stroker.SetMiterLimitTheta(t); }



        public double Width { get { return m_stroker.Width; } set { this.m_stroker.Width = value; } }

        public double ApproximateScale
        {
            get { return this.m_stroker.ApproximateScale; }
            set { this.m_stroker.ApproximateScale = value; }
        }
        public double Shorten
        {
            get { return this.m_shorten; }
            set { this.m_shorten = value; }
        }
        public bool AutoDetectOrientation
        {
            get { return m_auto_detect; }
            set { this.m_auto_detect = value; }
        }
        // Generator interface
        public void RemoveAll()
        {
            vertexDistanceList.Clear();
            m_closed = false;
            m_status = StrokeMath.Status.Init;
        }
        public void AddVertex(double x, double y, VertexCmd cmd)
        {
            m_status = StrokeMath.Status.Init;
            switch (cmd)
            {
                case VertexCmd.MoveTo:
                    vertexDistanceList.ReplaceLast(new VertexDistance(x, y));
                    break;
                case VertexCmd.Close:
                case VertexCmd.CloseAndEndFigure:
                    {
                        //end and close
                        m_closed = true;
                        if (m_orientation == EndVertexOrientation.Unknown)
                        {
                            switch ((int)x)
                            {
                                case 1:
                                case 2:
                                    {
                                        m_orientation = (EndVertexOrientation)x;
                                    }
                                    break;
                            }
                        }
                    }
                    break;

                default:

                    vertexDistanceList.AddVertex(new VertexDistance(x, y));
                    break;
            }
        }

        // Vertex Source Interface
        void RewindZero()
        {
            if (m_status == StrokeMath.Status.Init)
            {
                vertexDistanceList.Close(true);
                if (m_auto_detect)
                {
                    if (m_orientation == EndVertexOrientation.Unknown)
                    {
                        m_orientation = (AggMath.CalculatePolygonArea(vertexDistanceList) > 0.0) ?
                                        EndVertexOrientation.CCW :
                                        EndVertexOrientation.CW;
                    }
                }
                switch (m_orientation)
                {
                    case EndVertexOrientation.CCW:
                        {
                            m_stroker.Width = m_width;
                        }
                        break;
                    case EndVertexOrientation.CW:
                        {
                            m_stroker.Width = -m_width;
                        }
                        break;
                }
            }
            m_status = StrokeMath.Status.Ready;
            m_src_vertex = 0;
        }

        public VertexCmd GetNextVertex(ref double x, ref double y)
        {
            VertexCmd cmd = VertexCmd.LineTo;
            while (!VertexHelper.IsEmpty(cmd))
            {
                switch (m_status)
                {
                    case StrokeMath.Status.Init:
                        this.RewindZero();
                        goto case StrokeMath.Status.Ready;
                    case StrokeMath.Status.Ready:

                        if (vertexDistanceList.Count < 2 + (m_closed ? 1 : 0))
                        {
                            cmd = VertexCmd.NoMore;
                            break;
                        }
                        m_status = StrokeMath.Status.Outline1;
                        cmd = VertexCmd.MoveTo;
                        m_src_vertex = 0;
                        m_out_vertex = 0;
                        goto case StrokeMath.Status.Outline1;
                    case StrokeMath.Status.Outline1:
                        if (m_src_vertex >= vertexDistanceList.Count)
                        {
                            m_status = StrokeMath.Status.EndPoly1;
                            break;
                        }
                        m_stroker.CreateJoin(m_out_vertices,
                                            vertexDistanceList.prev(m_src_vertex),
                                            vertexDistanceList.curr(m_src_vertex),
                                            vertexDistanceList.next(m_src_vertex),
                                            vertexDistanceList.prev(m_src_vertex).dist,
                                            vertexDistanceList.curr(m_src_vertex).dist);
                        ++m_src_vertex;
                        m_status = StrokeMath.Status.OutVertices;
                        m_out_vertex = 0;
                        goto case StrokeMath.Status.OutVertices;
                    case StrokeMath.Status.OutVertices:
                        if (m_out_vertex >= m_out_vertices.Count)
                        {
                            m_status = StrokeMath.Status.Outline1;
                        }
                        else
                        {
                            m_out_vertices.GetVertex(m_out_vertex++, out x, out y);
                            return cmd;
                        }
                        break;
                    case StrokeMath.Status.EndPoly1:

                        if (!m_closed) return VertexCmd.NoMore;
                        m_status = StrokeMath.Status.Stop;
                        x = (int)EndVertexOrientation.CCW;
                        return VertexCmd.Close;
                    case StrokeMath.Status.Stop:
                        return VertexCmd.NoMore;
                }
            }
            return cmd;
        }
    }
}
