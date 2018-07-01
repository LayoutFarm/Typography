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

#define UNSAFE_VER 

using PixelFarm.CpuBlit;
namespace PixelFarm.Drawing
{



    public sealed class VertexStore
    {
        int m_num_vertices;
        int m_allocated_vertices;
        double[] m_coord_xy;
        byte[] m_cmds;
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


        /// <summary>
        /// num of vertex
        /// </summary>
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

            return VertexCmd.NoMore;
        }
        public VertexCmd GetLastVertex(out double x, out double y)
        {
            if (m_num_vertices != 0)
            {
                return GetVertex((int)(m_num_vertices - 1), out x, out y);
            }

            x = 0;
            y = 0;
            return VertexCmd.NoMore;
        }

        public VertexCmd GetVertex(int index, out double x, out double y)
        {

            x = m_coord_xy[index << 1];
            y = m_coord_xy[(index << 1) + 1];
            return (VertexCmd)m_cmds[index];
        }
        public void GetVertexXY(int index, out double x, out double y)
        {

            x = m_coord_xy[index << 1];
            y = m_coord_xy[(index << 1) + 1];
        }
        public VertexCmd GetCommand(int index)
        {
            return (VertexCmd)m_cmds[index];
        }

        public void Clear()
        {
            //we clear only command part!
            //clear only latest
            //System.Array.Clear(m_cmds, 0, m_cmds.Length);
            System.Array.Clear(m_cmds, 0, m_num_vertices); //only latest 
            m_num_vertices = 0;
        }
        public void AddVertex(double x, double y, VertexCmd cmd)
        {
#if DEBUG
            if (VertexStore.dbugCheckNANs(x, y))
            {

            }
#endif
            if (m_num_vertices >= m_allocated_vertices)
            {
                AllocIfRequired(m_num_vertices);
            }
            m_coord_xy[m_num_vertices << 1] = x;
            m_coord_xy[(m_num_vertices << 1) + 1] = y;
            m_cmds[m_num_vertices] = (byte)cmd;
            m_num_vertices++;
        }
        //--------------------------------------------------


        public void EndGroup()
        {
            if (m_num_vertices > 0)
            {
                m_cmds[m_num_vertices - 1] = (byte)VertexCmd.CloseAndEndFigure;
            }

        }
        public void ReplaceVertex(int index, double x, double y)
        {
#if DEBUG
            _dbugIsChanged = true;
#endif
            m_coord_xy[index << 1] = x;
            m_coord_xy[(index << 1) + 1] = y;
        }
        internal void ReplaceCommand(int index, VertexCmd CommandAndFlags)
        {
            m_cmds[index] = (byte)CommandAndFlags;
        }
        internal void SwapVertices(int v1, int v2)
        {
            double x_tmp = m_coord_xy[v1 << 1];
            double y_tmp = m_coord_xy[(v1 << 1) + 1];
            m_coord_xy[v1 << 1] = m_coord_xy[v2 << 1];//x
            m_coord_xy[(v1 << 1) + 1] = m_coord_xy[(v2 << 1) + 1];//y
            m_coord_xy[v2 << 1] = x_tmp;
            m_coord_xy[(v2 << 1) + 1] = y_tmp;
            byte cmd = m_cmds[v1];
            m_cmds[v1] = m_cmds[v2];
            m_cmds[v2] = cmd;
        }



#if DEBUG
        public bool _dbugIsChanged;
        public static bool dbugCheckNANs(double x, double y)
        {
            if (double.IsNaN(x))
            {
                return true;
            }
            else if (double.IsNaN(y))
            {
                return true;
            }
            return false;
        }
#endif
        void AllocIfRequired(int indexToAdd)
        {
            if (indexToAdd < m_allocated_vertices)
            {
                return;
            }

#if DEBUG
            int nrounds = 0;
#endif
            while (indexToAdd >= m_allocated_vertices)
            {
#if DEBUG

                if (nrounds > 0)
                {
                }
                nrounds++;
#endif

                //newsize is LARGER than original  ****
                int newSize = ((indexToAdd + 257) / 256) * 256; //calculate new size in single round
                //int newSize = m_allocated_vertices + 256; //original
                //-------------------------------------- 
                double[] new_xy = new double[newSize << 1];
                byte[] newCmd = new byte[newSize];

                if (m_coord_xy != null)
                {
                    //copy old buffer to new buffer 
                    int actualLen = m_num_vertices << 1;
                    //-----------------------------
                    //TODO: review faster copy
                    //----------------------------- 
                    unsafe
                    {
                        //unsafed version?
                        fixed (double* srcH = &m_coord_xy[0])
                        {
                            System.Runtime.InteropServices.Marshal.Copy(
                                (System.IntPtr)srcH,
                                new_xy, //dest
                                0,
                                actualLen);
                        }
                        fixed (byte* srcH = &m_cmds[0])
                        {
                            System.Runtime.InteropServices.Marshal.Copy(
                                (System.IntPtr)srcH,
                                newCmd, //dest
                                0,
                                m_num_vertices);
                        }
                    }
                }
                m_coord_xy = new_xy;
                m_cmds = newCmd;
                m_allocated_vertices = newSize;
            }
        }
        //internal use only!
        public static void UnsafeDirectSetData(
            VertexStore vstore,
            int m_allocated_vertices,
            int m_num_vertices,
            double[] m_coord_xy,
            byte[] m_CommandAndFlags)
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
            out byte[] m_CommandAndFlags)
        {
            m_num_vertices = vstore.m_num_vertices;
            m_allocated_vertices = vstore.m_allocated_vertices;
            m_coord_xy = vstore.m_coord_xy;
            m_CommandAndFlags = vstore.m_cmds;
        }

        private VertexStore(VertexStore src, bool trim)
        {
            //for copy from src to this instance

            this.m_allocated_vertices = src.m_allocated_vertices;
            this.m_num_vertices = src.m_num_vertices;

            if (trim)
            {
                int coord_len = m_num_vertices + 1; //+1 for no more cmd
                int cmds_len = m_num_vertices + 1; //+1 for no more cmd

                this.m_coord_xy = new double[coord_len << 1];//*2
                this.m_cmds = new byte[cmds_len];

                System.Array.Copy(
                     src.m_coord_xy,
                     0,
                     this.m_coord_xy,
                     0,
                     coord_len << 1); //*2

                System.Array.Copy(
                     src.m_cmds,
                     0,
                     this.m_cmds,
                     0,
                     cmds_len);
            }
            else
            {
                int coord_len = src.m_coord_xy.Length;
                int cmds_len = src.m_cmds.Length;

                this.m_coord_xy = new double[coord_len];
                this.m_cmds = new byte[cmds_len];

                System.Array.Copy(
                     src.m_coord_xy,
                     0,
                     this.m_coord_xy,
                     0,
                     coord_len);

                System.Array.Copy(
                     src.m_cmds,
                     0,
                     this.m_cmds,
                     0,
                     cmds_len);
            }

        }

        /// <summary>
        /// copy from src to the new one
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static VertexStore CreateCopy(VertexStore src)
        {
            return new VertexStore(src, false);
        }

        /// <summary>
        /// trim to new vertex store
        /// </summary>
        /// <returns></returns>
        public VertexStore CreateTrim()
        {
            return new VertexStore(this, true);
        }
    }


    public static class VertexStoreExtensions
    {
        /// <summary>
        /// add 2nd curve point (for C3,C4 curve)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void AddP2c(this VertexStore vxs, double x, double y)
        {


            vxs.AddVertex(x, y, VertexCmd.P2c);
        }
        /// <summary>
        /// add 3rd curve point (for C4 curve)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void AddP3c(this VertexStore vxs, double x, double y)
        {
            vxs.AddVertex(x, y, VertexCmd.P3c);
        }
        public static void AddMoveTo(this VertexStore vxs, double x0, double y0)
        {
            vxs.AddVertex(x0, y0, VertexCmd.MoveTo);
        }
        public static void AddLineTo(this VertexStore vxs, double x1, double y1)
        {
            vxs.AddVertex(x1, y1, VertexCmd.LineTo);
        }
        public static void AddCurve4To(this VertexStore vxs,
            double x1, double y1,
            double x2, double y2,
            double x3, double y3)
        {
            vxs.AddVertex(x1, y1, VertexCmd.P3c);
            vxs.AddVertex(x2, y2, VertexCmd.P3c);
            vxs.AddVertex(x3, y3, VertexCmd.LineTo);

        }
        public static void AddCloseFigure(this VertexStore vxs)
        {
            vxs.AddVertex(0, 0, VertexCmd.Close);
        }

        /// <summary>
        /// copy + translate vertext data from src to outputVxs
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        /// <param name="outputVxs"></param>
        /// <returns></returns>
        public static VertexStore TranslateToNewVxs(this VertexStore src, double dx, double dy, VertexStore outputVxs)
        {
            int count = src.Count;
            VertexCmd cmd;
            double x, y;
            for (int i = 0; i < count; ++i)
            {
                cmd = src.GetVertex(i, out x, out y);
                x += dx;
                y += dy;
                outputVxs.AddVertex(x, y, cmd);
            }
            return outputVxs;
        }
    }

    public static class VertexHelper
    {
        public static bool IsVertextCommand(VertexCmd c)
        {
            // return c >= VertexCmd.MoveTo;
            return c > VertexCmd.NoMore;
        }
        public static bool IsEmpty(VertexCmd c)
        {
            return c == VertexCmd.NoMore;
        }
        public static bool IsMoveTo(VertexCmd c)
        {
            return c == VertexCmd.MoveTo;
        }
        public static bool IsCloseOrEnd(VertexCmd c)
        {
            //check only 2 lower bit
            //TODO: review here
            return ((int)c & 0x3) >= (int)VertexCmd.Close;
        }

        public static bool IsNextPoly(VertexCmd c)
        {
            //?
            return c <= VertexCmd.MoveTo;
        }

        //internal static void ShortenPath(VertexDistanceList vertexDistanceList, double s, bool closed)
        //{
        //    if (s > 0.0 && vertexDistanceList.Count > 1)
        //    {
        //        double d;
        //        int n = (int)(vertexDistanceList.Count - 2);
        //        while (n != 0)
        //        {
        //            d = vertexDistanceList[n].dist;
        //            if (d > s) break;
        //            vertexDistanceList.RemoveLast();
        //            s -= d;
        //            --n;
        //        }
        //        if (vertexDistanceList.Count < 2)
        //        {
        //            vertexDistanceList.Clear();
        //        }
        //        else
        //        {
        //            n = (int)vertexDistanceList.Count - 1;
        //            VertexDistance prev = vertexDistanceList[n - 1];
        //            VertexDistance last = vertexDistanceList[n];
        //            d = (prev.dist - s) / prev.dist;
        //            double x = prev.x + (last.x - prev.x) * d;
        //            double y = prev.y + (last.y - prev.y) * d;
        //            last = new VertexDistance(x, y);
        //            if (prev.IsEqual(last))
        //            {
        //                vertexDistanceList.RemoveLast();
        //            }
        //            vertexDistanceList.Close(closed);
        //        }
        //    }
        //}
        public static void ArrangeOrientationsAll(VertexStore myvxs, bool closewise)
        {
            int start = 0;
            while (start < myvxs.Count)
            {
                start = ArrangeOrientations(myvxs, start, closewise);
            }
        }
        //---------------------------------------------------------------- 
        // Arrange the orientation of a polygon, all polygons in a path, 
        // or in all paths. After calling arrange_orientations() or 
        // arrange_orientations_all_paths(), all the polygons will have 
        // the same orientation, i.e. path_flags_cw or path_flags_ccw
        //--------------------------------------------------------------------
        static int ArrangePolygonOrientation(VertexStore myvxs, int start, bool clockwise)
        {
            //if (orientation == ShapePath.FlagsAndCommand.FlagNone) return start;

            // Skip all non-vertices at the beginning
            //ShapePath.FlagsAndCommand orientFlags = clockwise ? ShapePath.FlagsAndCommand.FlagCW : ShapePath.FlagsAndCommand.FlagCCW;

            int vcount = myvxs.Count;
            while (start < vcount &&
                  !VertexHelper.IsVertextCommand(myvxs.GetCommand(start)))
            {
                ++start;
            }

            // Skip all insignificant move_to
            while (start + 1 < vcount &&
                  VertexHelper.IsMoveTo(myvxs.GetCommand(start)) &&
                  VertexHelper.IsMoveTo(myvxs.GetCommand(start + 1)))
            {
                ++start;
            }

            // Find the last vertex
            int end = start + 1;
            while (end < vcount && !VertexHelper.IsNextPoly(myvxs.GetCommand(end)))
            {
                ++end;
            }
            if (end - start > 2)
            {
                bool isCW;
                if ((isCW = IsCW(myvxs, start, end)) != clockwise)
                {
                    // Invert polygon, set orientation flag, and skip all end_poly
                    InvertPolygon(myvxs, start, end);
                    VertexCmd flags;
                    int myvxs_count = myvxs.Count;
                    var orientFlags = isCW ? (int)EndVertexOrientation.CW : (int)EndVertexOrientation.CCW;
                    while (end < myvxs_count &&
                          VertexHelper.IsCloseOrEnd(flags = myvxs.GetCommand(end)))
                    {
                        //TODO: review hhere
                        myvxs.ReplaceVertex(end++, orientFlags, 0);
                        //myvxs.ReplaceCommand(end++, flags | orientFlags);// Path.set_orientation(cmd, orientation));
                    }
                }
            }
            return end;
        }

        static int ArrangeOrientations(VertexStore myvxs, int start, bool closewise)
        {
            while (start < myvxs.Count)
            {
                start = ArrangePolygonOrientation(myvxs, start, closewise);
                if (VertexHelper.IsEmpty(myvxs.GetCommand(start)))
                {
                    ++start;
                    break;
                }
            }

            return start;
        }
        static bool IsCW(VertexStore myvxs, int start, int end)
        {
            // Calculate signed area (double area to be exact)
            //---------------------
            int np = end - start;
            double area = 0.0;
            int i;
            for (i = 0; i < np; i++)
            {
                double x1, y1, x2, y2;
                myvxs.GetVertexXY(start + i, out x1, out y1);
                myvxs.GetVertexXY(start + (i + 1) % np, out x2, out y2);
                area += x1 * y2 - y1 * x2;
            }
            return (area < 0.0);
            //return (area < 0.0) ? ShapePath.FlagsAndCommand.FlagCW : ShapePath.FlagsAndCommand.FlagCCW;
        }
        //--------------------------------------------------------------------
        public static void InvertPolygon(VertexStore myvxs, int start)
        {
            // Skip all non-vertices at the beginning
            int vcount = myvxs.Count;
            while (start < vcount &&
                  !VertexHelper.IsVertextCommand(myvxs.GetCommand(start)))
            { ++start; }

            // Skip all insignificant move_to
            while (start + 1 < vcount &&
                  VertexHelper.IsMoveTo(myvxs.GetCommand(start)) &&
                  VertexHelper.IsMoveTo(myvxs.GetCommand(start + 1)))
            { ++start; }

            // Find the last vertex
            int end = start + 1;
            while (end < vcount && !VertexHelper.IsNextPoly(myvxs.GetCommand(end))) { ++end; }

            InvertPolygon(myvxs, start, end);
        }



        static void InvertPolygon(VertexStore myvxs, int start, int end)
        {
            int i;
            VertexCmd tmp_PathAndFlags = myvxs.GetCommand(start);
            --end; // Make "end" inclusive 
            // Shift all commands to one position
            for (i = start; i < end; i++)
            {
                myvxs.ReplaceCommand(i, myvxs.GetCommand(i + 1));
            }
            // Assign starting command to the ending command
            myvxs.ReplaceCommand(end, tmp_PathAndFlags);

            // Reverse the polygon
            while (end > start)
            {
                myvxs.SwapVertices(start++, end--);
            }
        }
        public static void FlipX(VertexStore vxs, double x1, double x2)
        {
            int i;
            double x, y;
            int count = vxs.Count;
            for (i = 0; i < count; ++i)
            {
                VertexCmd flags = vxs.GetVertex(i, out x, out y);
                if (VertexHelper.IsVertextCommand(flags))
                {
                    vxs.ReplaceVertex(i, x2 - x + x1, y);
                }
            }
        }

        public static void FlipY(VertexStore vxs, double y1, double y2)
        {
            int i;
            double x, y;
            int count = vxs.Count;
            for (i = 0; i < count; ++i)
            {
                VertexCmd flags = vxs.GetVertex(i, out x, out y);
                if (VertexHelper.IsVertextCommand(flags))
                {
                    vxs.ReplaceVertex(i, x, y2 - y + y1);
                }
            }
        }
    }

}
