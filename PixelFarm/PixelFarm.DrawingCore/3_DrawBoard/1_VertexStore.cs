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



using PixelFarm.CpuBlit;
namespace PixelFarm.Drawing
{
    public sealed class VertexStore
    {

        int _vertices_count;
        int _allocated_vertices_count;
        double[] _coord_xy;
        byte[] _cmds;

        //***
        RenderVx _cachedAreaRenderVx;
        RenderVx _cachedBorderRenerVx;
        //

#if DEBUG
        public readonly bool dbugIsTrim;
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
        public VertexStore(bool isShared)
        {
            AllocIfRequired(2);
            IsShared = isShared;
        }
        public bool IsShared { get; private set; }
        /// <summary>
        /// num of vertex
        /// </summary>
        public int Count => _vertices_count;
        //
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
            //
            _cachedAreaRenderVx = null;
            //
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


        internal void ReplaceVertex(int index, double x, double y)
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


        //--------------------------------------------------
        public static void SetAreaRenderVx(VertexStore vxs, RenderVx renderVx)
        {
#if DEBUG
            if (vxs.IsShared)
            {
                throw new System.NotSupportedException();//don't store renderVx in shared Vxs
            }
#endif
            vxs._cachedAreaRenderVx = renderVx;
        }
        public static RenderVx GetAreaRenderVx(VertexStore vxs)
        {
#if DEBUG
            if (vxs.IsShared)
            {
                throw new System.NotSupportedException();//don't store renderVx in shared Vxs
            }
#endif

            return vxs._cachedAreaRenderVx;
        }
        public static void SetBorderRenderVx(VertexStore vxs, RenderVx renderVx)
        {
#if DEBUG
            if (vxs.IsShared)
            {
                throw new System.NotSupportedException();//don't store renderVx in shared Vxs
            }
#endif
            vxs._cachedBorderRenerVx = renderVx;
        }
        public static RenderVx GetBorderRenderVx(VertexStore vxs)
        {
#if DEBUG
            if (vxs.IsShared)
            {
                throw new System.NotSupportedException();//don't store renderVx in shared Vxs
            }
#endif
            return vxs._cachedBorderRenerVx;
        }
        //--------------------------------------------------
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

        /// <summary>
        /// copy data from 'another' append to this vxs,we DO NOT store 'another' vxs inside this
        /// </summary>
        /// <param name="another"></param>
        public void AppendVertexStore(VertexStore another)
        {

            //append data from another
            if (_allocated_vertices_count < _vertices_count + another._vertices_count)
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
                     _coord_xy,//src_arr
                     0, //src_index
                     new_coord_xy,//dst
                     0, //dst index
                     _vertices_count << 1); //len


                //A.2
                System.Array.Copy(
                    _cmds,//src_arr
                    0,//src_index
                    new_cmds,//dst
                    0, //dst index
                    _vertices_count);//len


                //B.1
                System.Array.Copy(
                   another._coord_xy,//src
                   0, //srcIndex
                   new_coord_xy, //dst
                   _vertices_count << 1,//dst index
                   another._vertices_count << 1);
                //**             

                //B.2 
                System.Array.Copy(
                        another._cmds,//src
                        0,//srcIndex
                        new_cmds, //dst
                        _vertices_count, //dst index
                        another._vertices_count);

                _coord_xy = new_coord_xy;
                _cmds = new_cmds;
                _vertices_count += another._vertices_count;
            }
            else
            {
                System.Array.Copy(
                  another._coord_xy,//src
                  0,//src index
                  _coord_xy,//dst
                  _vertices_count << 1,//*2 //
                  another._vertices_count << 1);

                

                //B.2 
                System.Array.Copy(
                        another._cmds,//src
                        0,//src index
                       _cmds,//dst
                       _vertices_count, //dst index
                      another._vertices_count);

                _vertices_count += another._vertices_count;
            }
        }
        private VertexStore(VertexStore src, bool trim)
        {
            //for copy from src to this instance


            _vertices_count = src._vertices_count;

            if (trim)
            {
#if DEBUG
                dbugIsTrim = true;
#endif
                int coord_len = _vertices_count; //+1 for no more cmd
                int cmds_len = _vertices_count; //+1 for no more cmd

                _coord_xy = new double[(coord_len + 1) << 1];//*2
                _cmds = new byte[(cmds_len + 1)];

                System.Array.Copy(
                     src._coord_xy,
                     0,
                     _coord_xy,
                     0,
                     coord_len << 1); //*2

                System.Array.Copy(
                     src._cmds,
                     0,
                     _cmds,
                     0,
                     cmds_len);

                _allocated_vertices_count = _cmds.Length;
            }
            else
            {
                int coord_len = src._coord_xy.Length;
                int cmds_len = src._cmds.Length;

                _coord_xy = new double[(coord_len + 1) << 1];
                _cmds = new byte[(cmds_len + 1)]; //TODO: review here again***

                System.Array.Copy(
                     src._coord_xy,
                     0,
                     _coord_xy,
                     0,
                     coord_len);

                System.Array.Copy(
                     src._cmds,
                     0,
                     _cmds,
                     0,
                     cmds_len);
                _allocated_vertices_count = _cmds.Length;
            }

        }
        private VertexStore(VertexStore src, PixelFarm.CpuBlit.VertexProcessing.ICoordTransformer tx)
        {
            //for copy from src to this instance

            _allocated_vertices_count = src._allocated_vertices_count;
            _vertices_count = src._vertices_count;
            //
            //
#if DEBUG
            dbugIsTrim = true;
#endif
            //
            int coord_len = _vertices_count; //+1 for no more cmd
            int cmds_len = _vertices_count; //+1 for no more cmd

            _coord_xy = new double[(coord_len + 1) << 1];//*2
            _cmds = new byte[(cmds_len + 1)];


            System.Array.Copy(
                 src._coord_xy,
                 0,
                 _coord_xy,
                 0,
                 coord_len << 1); //*2

            System.Array.Copy(
                 src._cmds,
                 0,
                 _cmds,
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



    public enum ClipingTechnique
    {
        None,
        ClipMask,
        ClipSimpleRect
    }

    public static class SimpleRectClipEvaluator
    {
        enum RectSide
        {
            None,
            Vertical,
            Horizontal
        }

        static RectSide FindRectSide(float x0, float y0, float x1, float y1)
        {
            if (x0 == x1 && y0 != y1)
            {
                return RectSide.Vertical;
            }
            else if (y0 == y1 && x0 != x1)
            {
                return RectSide.Horizontal;
            }
            return RectSide.None;
        }

        /// <summary>
        /// check if this is a simple rect
        /// </summary>
        /// <param name="vxs"></param>
        /// <returns></returns>
        public static bool EvaluateRectClip(VertexStore vxs, out RectangleF clipRect)
        {
            float x0 = 0, y0 = 0;
            float x1 = 0, y1 = 0;
            float x2 = 0, y2 = 0;
            float x3 = 0, y3 = 0;
            float x4 = 0, y4 = 0;
            clipRect = new RectangleF();

            int sideCount = 0;

            int j = vxs.Count;
            for (int i = 0; i < j; ++i)
            {
                VertexCmd cmd = vxs.GetVertex(i, out double x, out double y);
                switch (cmd)
                {
                    default: return false;
                    case VertexCmd.NoMore:
                        if (i > 6) return false;
                        break;
                    case VertexCmd.Close:
                        if (i > 5)
                        {
                            return false;
                        }
                        break;
                    case VertexCmd.LineTo:
                        {
                            switch (i)
                            {
                                case 1:
                                    x1 = (float)x;
                                    y1 = (float)y;
                                    sideCount++;
                                    break;
                                case 2:
                                    x2 = (float)x;
                                    y2 = (float)y;
                                    sideCount++;
                                    break;
                                case 3:
                                    x3 = (float)x;
                                    y3 = (float)y;
                                    sideCount++;
                                    break;
                                case 4:
                                    x4 = (float)x;
                                    y4 = (float)y;
                                    sideCount++;
                                    break;
                            }
                        }
                        break;
                    case VertexCmd.MoveTo:
                        {
                            if (i == 0)
                            {
                                x0 = (float)x;
                                y0 = (float)y;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        break;
                }
            }

            if (sideCount == 4)
            {
                RectSide s0 = FindRectSide(x0, y0, x1, y1);
                if (s0 == RectSide.None) return false;
                //
                RectSide s1 = FindRectSide(x1, y1, x2, y2);
                if (s1 == RectSide.None || s0 == s1) return false;
                //
                RectSide s2 = FindRectSide(x2, y2, x3, y3);
                if (s2 == RectSide.None || s1 == s2) return false;
                //
                RectSide s3 = FindRectSide(x3, y3, x4, y4);
                if (s3 == RectSide.None || s2 == s3) return false;
                //
                if (x4 == x0 && y4 == y0)
                {

                    if (s0 == RectSide.Horizontal)
                    {
                        clipRect = new RectangleF(x0, y0, x1 - x0, y3 - y0);
                    }
                    else
                    {
                        clipRect = new RectangleF(x0, y0, x3 - x0, y3 - y0);
                    }

                    return true;

                }
            }
            return false;

        }
    }
}
