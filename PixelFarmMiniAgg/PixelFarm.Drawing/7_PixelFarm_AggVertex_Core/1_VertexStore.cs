//BSD, 2014-2016, WinterDev
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


namespace PixelFarm.Agg
{
    public sealed class VertexStore
    {
        int m_num_vertices;
        int m_allocated_vertices;
        double[] m_coord_xy;
        VertexCmd[] m_cmds;
#if DEBUG
        static int dbugTotal = 0;
        public readonly int dbugId = dbugGetNewId();
        static int dbugGetNewId()
        {
            return dbugTotal++;
        }
#endif
        public VertexStore()
        {
            AllocIfRequired(2);
        }
        public VertexStore(int initsize)
        {
            AllocIfRequired(initsize);
        }


        internal bool HasMoreThanOnePart { get; set; }

        public int Count
        {
            get { return m_num_vertices; }
        }
        public VertexCmd GetLastCommand()
        {
            if (m_num_vertices != 0)
            {
                return GetCommand(m_num_vertices - 1);
            }

            return VertexCmd.Stop;
        }
        public VertexCmd GetLastVertex(out double x, out double y)
        {
            if (m_num_vertices != 0)
            {
                return GetVertex((int)(m_num_vertices - 1), out x, out y);
            }

            x = 0;
            y = 0;
            return VertexCmd.Stop;
        }

        public VertexCmd GetVertex(int index, out double x, out double y)
        {
            int i = index << 1;
            x = m_coord_xy[i];
            y = m_coord_xy[i + 1];
            return m_cmds[index];
        }
        public void GetVertexXY(int index, out double x, out double y)
        {
            int i = index << 1;
            x = m_coord_xy[i];
            y = m_coord_xy[i + 1];
        }
        public VertexCmd GetCommand(int index)
        {
            return m_cmds[index];
        }
        //--------------------------------------------------
        //mutable properties
        public void Clear()
        {
            m_num_vertices = 0;
        }
        public void AddVertex(double x, double y, VertexCmd cmd)
        {
            if (m_num_vertices >= m_allocated_vertices)
            {
                AllocIfRequired(m_num_vertices);
            }
            m_coord_xy[m_num_vertices << 1] = x;
            m_coord_xy[(m_num_vertices << 1) + 1] = y;
            m_cmds[m_num_vertices] = cmd;
            m_num_vertices++;
        }
        //--------------------------------------------------
        /// <summary>
        /// add 2nd curve point (for C3,C4 curve)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void AddP2c(double x, double y)
        {
            AddVertex(x, y, VertexCmd.P2c);
        }
        /// <summary>
        /// add 3rd curve point (for C4 curve)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void AddP3c(double x, double y)
        {
            AddVertex(x, y, VertexCmd.P3c);
        }
        public void AddMoveTo(double x, double y)
        {
            AddVertex(x, y, VertexCmd.MoveTo);
        }
        public void AddLineTo(double x, double y)
        {
            AddVertex(x, y, VertexCmd.LineTo);
        }
        public void AddCloseFigure()
        {
            AddVertex(0, 0, VertexCmd.CloseAndEndFigure);
        }
        public void AddStop()
        {
            AddVertex(0, 0, VertexCmd.Stop);
        }
        internal void ReplaceVertex(int index, double x, double y)
        {
            m_coord_xy[index << 1] = x;
            m_coord_xy[(index << 1) + 1] = y;
        }
        internal void ReplaceCommand(int index, VertexCmd CommandAndFlags)
        {
            m_cmds[index] = CommandAndFlags;
        }

        internal void SwapVertices(int v1, int v2)
        {
            double x_tmp = m_coord_xy[v1 << 1];
            double y_tmp = m_coord_xy[(v1 << 1) + 1];
            m_coord_xy[v1 << 1] = m_coord_xy[v2 << 1];//x
            m_coord_xy[(v1 << 1) + 1] = m_coord_xy[(v2 << 1) + 1];//y
            m_coord_xy[v2 << 1] = x_tmp;
            m_coord_xy[(v2 << 1) + 1] = y_tmp;
            VertexCmd cmd = m_cmds[v1];
            m_cmds[v1] = m_cmds[v2];
            m_cmds[v2] = cmd;
        }
        void AllocIfRequired(int indexToAdd)
        {
            if (indexToAdd < m_allocated_vertices)
            {
                return;
            }

            while (indexToAdd >= m_allocated_vertices)
            {
                int newSize = m_allocated_vertices + 256;
                double[] new_xy = new double[newSize << 1];
                VertexCmd[] newCmd = new VertexCmd[newSize];
                if (m_coord_xy != null)
                {
                    //copy old buffer to new buffer 
                    int actualLen = m_num_vertices << 1;
                    for (int i = actualLen - 1; i >= 0;)
                    {
                        new_xy[i] = m_coord_xy[i];
                        i--;
                        new_xy[i] = m_coord_xy[i];
                        i--;
                    }
                    for (int i = m_num_vertices - 1; i >= 0; --i)
                    {
                        newCmd[i] = m_cmds[i];
                    }
                }
                m_coord_xy = new_xy;
                m_cmds = newCmd;
                m_allocated_vertices = newSize;
            }
        }
        //----------------------------------------------------------

        public void AddSubVertices(VertexStore anotherVxs)
        {
            int j = anotherVxs.Count;
            this.HasMoreThanOnePart = true;
            for (int i = 0; i < j; ++i)
            {
                double x, y;
                VertexCmd cmd = anotherVxs.GetVertex(i, out x, out y);
                this.AddVertex(x, y, cmd);
                if (cmd == VertexCmd.Stop)
                {
                    break;
                }
            }
        }
        //internal use only!
        public static void UnsafeDirectSetData(
            VertexStore vstore,
            int m_allocated_vertices,
            int m_num_vertices,
            double[] m_coord_xy,
            VertexCmd[] m_CommandAndFlags)
        {
            vstore.m_num_vertices = m_num_vertices;
            vstore.m_allocated_vertices = m_allocated_vertices;
            vstore.m_coord_xy = m_coord_xy;
            vstore.m_cmds = m_CommandAndFlags;
        }
        public static void UnsafeDirectGetData(
            VertexStore vstore,
            out int m_allocated_vertices,
            out int m_num_vertices,
            out double[] m_coord_xy,
            out VertexCmd[] m_CommandAndFlags)
        {
            m_num_vertices = vstore.m_num_vertices;
            m_allocated_vertices = vstore.m_allocated_vertices;
            m_coord_xy = vstore.m_coord_xy;
            m_CommandAndFlags = vstore.m_cmds;
        }

        //----------------------------------------------------------
    }
}