//BSD, 2014-present, WinterDev

using System.Collections.Generic;
using PixelFarm.CpuBlit.VertexProcessing;
namespace PixelFarm.Drawing
{
    public static class VectorToolBox
    {
        [System.ThreadStatic]
        static Stack<VertexStore> s_vxsPool = new Stack<VertexStore>();

        public static void GetFreeVxs(out VertexStore vxs1)
        {
            vxs1 = GetFreeVxs();
        }
        public static void GetFreeVxs(out VertexStore vxs1, out VertexStore vxs2)
        {
            vxs1 = GetFreeVxs();
            vxs2 = GetFreeVxs();
        }
        public static void GetFreeVxs(out VertexStore vxs1, out VertexStore vxs2, out VertexStore vxs3)
        {
            vxs1 = GetFreeVxs();
            vxs2 = GetFreeVxs();
            vxs3 = GetFreeVxs();
        }
        public static void ReleaseVxs(ref VertexStore vxs1, ref VertexStore vxs2)
        {
            ReleaseVxs(ref vxs1);
            ReleaseVxs(ref vxs2);
        }

        public static void ReleaseVxs(ref VertexStore vxs1, ref VertexStore vxs2, ref VertexStore vxs3)
        {
            ReleaseVxs(ref vxs1);
            ReleaseVxs(ref vxs2);
            ReleaseVxs(ref vxs3);
        }
        public static void ReleaseVxs(ref VertexStore vxs1)
        {
            vxs1.Clear();
            s_vxsPool.Push(vxs1);
            vxs1 = null;
        }
        static VertexStore GetFreeVxs()
        {
            if (s_vxsPool.Count > 0)
            {
                return s_vxsPool.Pop();
            }
            else
            {
                return new VertexStore();
            }
        }

        //-----------------------------------
        [System.ThreadStatic]
        static Stack<Stroke> s_strokePool = new Stack<Stroke>();
        public static void GetFreeStroke(out Stroke stroke, int w)
        {
            if (s_strokePool.Count > 0)
            {
                stroke = s_strokePool.Pop();
                stroke.Width = w;
            }
            else
            {
                stroke = new Stroke(w);
            }
        }
        public static void ReleaseStroke(ref Stroke stroke)
        {
            s_strokePool.Push(stroke);
            stroke = null;
        }
        //-----------------------------------


        [System.ThreadStatic]
        static Stack<PixelFarm.CpuBlit.PathWriter> s_pathWriters = new Stack<PixelFarm.CpuBlit.PathWriter>();
        public static void GetFreePathWriter(out PixelFarm.CpuBlit.PathWriter p)
        {
            if (s_pathWriters.Count > 0)
            {
                p = s_pathWriters.Pop();
            }
            else
            {
                p = new CpuBlit.PathWriter();
            }
        }
        public static void ReleasePathWriter(ref PixelFarm.CpuBlit.PathWriter p)
        {
            p.Clear();
            s_pathWriters.Push(p);
            p = null;
        }
        //-----------------------------------
    }
}