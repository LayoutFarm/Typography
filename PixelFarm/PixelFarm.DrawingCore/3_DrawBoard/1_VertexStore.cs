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
        public readonly bool _isTrimed;
        int _vertices_count;
        int _allocated_vertices_count;
        double[] _coord_xy;
        byte[] _cmds;
#if DEBUG
        static int dbugTotal = 0;
        public readonly int dbugId = dbugGetNewId();
        public int dbugNote;

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
            get { return _vertices_count; }
        }
        public VertexCmd GetLastCommand()
        {
            if (_vertices_count != 0)
            {
                return GetCommand(_vertices_count - 1);
            }

            return VertexCmd.NoMore;
        }
        public VertexCmd GetLastVertex(out double x, out double y)
        {
            if (_vertices_count != 0)
            {
                return GetVertex((int)(_vertices_count - 1), out x, out y);
            }

            x = 0;
            y = 0;
            return VertexCmd.NoMore;
        }

        public VertexCmd GetVertex(int index, out double x, out double y)
        {
            x = _coord_xy[index << 1];
            y = _coord_xy[(index << 1) + 1];
            return (VertexCmd)_cmds[index];
        }


        public void GetVertexXY(int index, out double x, out double y)
        {

            x = _coord_xy[index << 1];
            y = _coord_xy[(index << 1) + 1];
        }
        public VertexCmd GetCommand(int index)
        {
            return (VertexCmd)_cmds[index];
        }

        public void Clear()
        {
            //we clear only command part!
            //clear only latest
            //System.Array.Clear(m_cmds, 0, m_cmds.Length);
            System.Array.Clear(_cmds, 0, _vertices_count); //only latest 
            _vertices_count = 0;
        }
        public void ConfirmNoMore()
        {
            AddVertex(0, 0, VertexCmd.NoMore);
            _vertices_count--;//not count
        }
        public void AddVertex(double x, double y, VertexCmd cmd)
        {
#if DEBUG
            if (VertexStore.dbugCheckNANs(x, y))
            {

            }
#endif
            if (_vertices_count >= _allocated_vertices_count)
            {
                AllocIfRequired(_vertices_count);
            }
            _coord_xy[_vertices_count << 1] = x;
            _coord_xy[(_vertices_count << 1) + 1] = y;
            _cmds[_vertices_count] = (byte)cmd;
            _vertices_count++;
        }
        //--------------------------------------------------


        public void EndGroup()
        {
            if (_vertices_count > 0)
            {
                _cmds[_vertices_count - 1] = (byte)VertexCmd.CloseAndEndFigure;
            }

        }
        public void ReplaceVertex(int index, double x, double y)
        {
#if DEBUG
            _dbugIsChanged = true;
#endif
            _coord_xy[index << 1] = x;
            _coord_xy[(index << 1) + 1] = y;
        }
        internal void ReplaceCommand(int index, VertexCmd cmd)
        {
            _cmds[index] = (byte)cmd;
        }
        internal void SwapVertices(int v1, int v2)
        {
            double x_tmp = _coord_xy[v1 << 1];
            double y_tmp = _coord_xy[(v1 << 1) + 1];
            _coord_xy[v1 << 1] = _coord_xy[v2 << 1];//x
            _coord_xy[(v1 << 1) + 1] = _coord_xy[(v2 << 1) + 1];//y
            _coord_xy[v2 << 1] = x_tmp;
            _coord_xy[(v2 << 1) + 1] = y_tmp;
            byte cmd = _cmds[v1];
            _cmds[v1] = _cmds[v2];
            _cmds[v2] = cmd;
        }


#if DEBUG
        public override string ToString()
        {
            return _vertices_count.ToString();
        }

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
            if (indexToAdd < _allocated_vertices_count)
            {
                return;
            }

#if DEBUG
            int nrounds = 0;
#endif
            while (indexToAdd >= _allocated_vertices_count)
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

                if (_coord_xy != null)
                {
                    //copy old buffer to new buffer 
                    int actualLen = _vertices_count << 1;
                    //-----------------------------
                    //TODO: review faster copy
                    //----------------------------- 
                    unsafe
                    {
#if COSMOS
                        System.Array.Copy(m_coord_xy, new_xy, actualLen);
                        System.Array.Copy(m_cmds, newCmd, m_num_vertices);
#else
                        //unsafed version?
                        fixed (double* srcH = &_coord_xy[0])
                        {
                            System.Runtime.InteropServices.Marshal.Copy(
                                (System.IntPtr)srcH,
                                new_xy, //dest
                                0,
                                actualLen);
                        }
                        fixed (byte* srcH = &_cmds[0])
                        {
                            System.Runtime.InteropServices.Marshal.Copy(
                                (System.IntPtr)srcH,
                                newCmd, //dest
                                0,
                                _vertices_count);
                        }
#endif

                    }
                }
                _coord_xy = new_xy;
                _cmds = newCmd;
                _allocated_vertices_count = newSize;
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
            vstore._vertices_count = m_num_vertices;
            vstore._allocated_vertices_count = m_allocated_vertices;
            vstore._coord_xy = m_coord_xy;
            vstore._cmds = m_CommandAndFlags;
        }
        public static void UnsafeDirectGetData(
            VertexStore vstore,
            out int m_allocated_vertices,
            out int m_num_vertices,
            out double[] m_coord_xy,
            out byte[] m_CommandAndFlags)
        {
            m_num_vertices = vstore._vertices_count;
            m_allocated_vertices = vstore._allocated_vertices_count;
            m_coord_xy = vstore._coord_xy;
            m_CommandAndFlags = vstore._cmds;
        }
        public void AppendVertexStore(VertexStore another)
        {

            //append data from another

            if (this._allocated_vertices_count < _vertices_count + another._vertices_count)
            {
                //alloc a new one
                int new_alloc = _vertices_count + another._vertices_count;

                _allocated_vertices_count = new_alloc;
                _vertices_count = new_alloc;//new 

                var new_coord_xy = new double[(new_alloc + 1) << 1];//*2
                var new_cmds = new byte[(new_alloc + 1)];
                //copy org

                //A.1
                System.Array.Copy(
                     this._coord_xy,
                     0,
                     new_coord_xy,
                     0,
                     _vertices_count << 1);


                //A.2
                System.Array.Copy(
                    _cmds,
                    0,
                    new_cmds,
                    0,
                    _vertices_count);

                //B.1
                System.Array.Copy(
                   another._coord_xy,
                   _vertices_count << 1,
                   new_coord_xy,
                   0,
                   another._vertices_count << 1);

                //B.2 
                System.Array.Copy(
                        another._cmds,
                       _vertices_count,
                        new_cmds,
                        0,
                        another._vertices_count);

                this._coord_xy = new_coord_xy;
                this._cmds = new_cmds;
            }
            else
            {
                System.Array.Copy(
                  another._coord_xy,
                  _vertices_count << 1,
                  _coord_xy,
                  0,
                  another._vertices_count << 1);

                //B.2 
                System.Array.Copy(
                        another._cmds,
                       _vertices_count,
                       _cmds,
                        0,
                      another._vertices_count);
            }
        }
        private VertexStore(VertexStore src, bool trim)
        {
            //for copy from src to this instance

            this._allocated_vertices_count = src._allocated_vertices_count;
            this._vertices_count = src._vertices_count;

            if (trim)
            {

                _isTrimed = true;
                int coord_len = _vertices_count; //+1 for no more cmd
                int cmds_len = _vertices_count; //+1 for no more cmd

                this._coord_xy = new double[(coord_len + 1) << 1];//*2
                this._cmds = new byte[(cmds_len + 1)];

                System.Array.Copy(
                     src._coord_xy,
                     0,
                     this._coord_xy,
                     0,
                     coord_len << 1); //*2

                System.Array.Copy(
                     src._cmds,
                     0,
                     this._cmds,
                     0,
                     cmds_len);
            }
            else
            {
                int coord_len = src._coord_xy.Length;
                int cmds_len = src._cmds.Length;

                this._coord_xy = new double[(coord_len + 1) << 1];
                this._cmds = new byte[(cmds_len + 1) << 1];

                System.Array.Copy(
                     src._coord_xy,
                     0,
                     this._coord_xy,
                     0,
                     coord_len);

                System.Array.Copy(
                     src._cmds,
                     0,
                     this._cmds,
                     0,
                     cmds_len);
            }

        }
        private VertexStore(VertexStore src, PixelFarm.CpuBlit.VertexProcessing.ICoordTransformer tx)
        {
            //for copy from src to this instance

            this._allocated_vertices_count = src._allocated_vertices_count;
            this._vertices_count = src._vertices_count;

            _isTrimed = true;
            int coord_len = _vertices_count; //+1 for no more cmd
            int cmds_len = _vertices_count; //+1 for no more cmd

            this._coord_xy = new double[(coord_len + 1) << 1];//*2
            this._cmds = new byte[(cmds_len + 1)];


            System.Array.Copy(
                 src._coord_xy,
                 0,
                 this._coord_xy,
                 0,
                 coord_len << 1); //*2

            System.Array.Copy(
                 src._cmds,
                 0,
                 this._cmds,
                 0,
                 cmds_len);

            //-------------------------
            int coord_count = coord_len;
            int a = 0;
            for (int n = 0; n < coord_count; ++n)
            {
                tx.Transform(ref _coord_xy[a++], ref _coord_xy[a++]);
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
        public VertexStore CreateTrim(PixelFarm.CpuBlit.VertexProcessing.ICoordTransformer tx)
        {
            return new VertexStore(this, tx);
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
        public static void AddCloseFigure(this VertexStore vxs, double x, double y)
        {
            vxs.AddVertex(x, y, VertexCmd.Close);
        }
        public static void AddNoMore(this VertexStore vxs)
        {
            vxs.AddVertex(0, 0, VertexCmd.NoMore);
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
