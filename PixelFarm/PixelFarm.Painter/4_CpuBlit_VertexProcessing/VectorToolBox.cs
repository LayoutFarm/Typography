//BSD, 2014-present, WinterDev

using System;
using System.Collections.Generic;
using PixelFarm.CpuBlit.VertexProcessing;

namespace PixelFarm.CpuBlit.VertexProcessing
{
    using PixelFarm.Drawing;

    //-----------------------------------
    public struct VxsContext1 : IDisposable
    {
        internal readonly VertexStore _vxs;
        internal VxsContext1(out VertexStore outputVxs)
        {
            VxsTemp.GetFreeVxs(out outputVxs);
            _vxs = outputVxs;

        }
        public void Dispose()
        {
            VxsTemp.ReleaseVxs(_vxs);
        }
    }
    public struct VxsContext2 : IDisposable
    {
        internal readonly VertexStore _vxs1;
        internal readonly VertexStore _vxs2;
        internal VxsContext2(out VertexStore outputVxs1, out VertexStore outputVxs2)
        {
            VxsTemp.GetFreeVxs(out _vxs1);
            VxsTemp.GetFreeVxs(out _vxs2);
            outputVxs1 = _vxs1;
            outputVxs2 = _vxs2;
        }
        public void Dispose()
        {
            //release
            VxsTemp.ReleaseVxs(_vxs1);
            VxsTemp.ReleaseVxs(_vxs2);
        }
    }
    public struct VxsContext3 : IDisposable
    {
        internal readonly VertexStore _vxs1;
        internal readonly VertexStore _vxs2;
        internal readonly VertexStore _vxs3;
        internal VxsContext3(out VertexStore outputVxs1, out VertexStore outputVxs2, out VertexStore outputVxs3)
        {
            VxsTemp.GetFreeVxs(out _vxs1);
            VxsTemp.GetFreeVxs(out _vxs2);
            VxsTemp.GetFreeVxs(out _vxs3);
            outputVxs1 = _vxs1;
            outputVxs2 = _vxs2;
            outputVxs3 = _vxs3;

        }
        public void Dispose()
        {
            //release
            VxsTemp.ReleaseVxs(_vxs1);
            VxsTemp.ReleaseVxs(_vxs2);
            VxsTemp.ReleaseVxs(_vxs3);
        }
    }


}
namespace PixelFarm.Drawing
{

    public static class VxsTemp
    {

        public static VxsContext1 Borrow(out VertexStore vxs)
        {
            return new VxsContext1(out vxs);
        }
        public static VxsContext2 Borrow(out VertexStore vxs1, out VertexStore vxs2)
        {
            return new VxsContext2(out vxs1, out vxs2);
        }
        public static VxsContext3 Borrow(out VertexStore vxs1,
            out VertexStore vxs2, out VertexStore vxs3)
        {
            return new VxsContext3(out vxs1, out vxs2, out vxs3);
        }


        //for net20 -- check this
        //TODO: https://stackoverflow.com/questions/18333885/threadstatic-v-s-threadlocalt-is-generic-better-than-attribute

        [System.ThreadStatic]
        static Stack<VertexStore> s_vxsPool = new Stack<VertexStore>();

        internal static void GetFreeVxs(out VertexStore vxs1)
        {
            vxs1 = GetFreeVxs();
        }
        internal static void ReleaseVxs(VertexStore vxs1)
        {
            vxs1.Clear();
            s_vxsPool.Push(vxs1);
        }
        static VertexStore GetFreeVxs()
        {
            if (s_vxsPool == null)
            {
                s_vxsPool = new Stack<VertexStore>();
            }
            if (s_vxsPool.Count > 0)
            {
                return s_vxsPool.Pop();
            }
            else
            {
                return new VertexStore(true);
            }
        }
    }



    public static class VectorToolBox
    {

        public static TempContext<Stroke> Borrow(out Stroke stroke)
        {
            if (!Temp<Stroke>.IsInit())
            {
                Temp<Stroke>.SetNewHandler(() => new Stroke(1),
                    s =>
                    {
                        s.Width = 1;
                        s.LineCap = LineCap.Butt;
                        s.LineJoin = LineJoin.Miter;
                        s.StrokeSideForClosedShape = StrokeSideForClosedShape.Both;
                    }
                    );//reset?
            }
            return Temp<Stroke>.Borrow(out stroke);
        }
        static TempContext<PixelFarm.CpuBlit.PathWriter> Borrow(out PixelFarm.CpuBlit.PathWriter pathWriter)
        {
            if (!Temp<PixelFarm.CpuBlit.PathWriter>.IsInit())
            {
                Temp<PixelFarm.CpuBlit.PathWriter>.SetNewHandler(
                    () => new PixelFarm.CpuBlit.PathWriter(),
                    w => w.UnbindVxs());
            }
            return Temp<PixelFarm.CpuBlit.PathWriter>.Borrow(out pathWriter);
        }
        public static TempContext<PixelFarm.CpuBlit.PathWriter> Borrow(VertexStore vxs, out PixelFarm.CpuBlit.PathWriter pathWriter)
        {
            var tmpPw = Borrow(out pathWriter);
            tmpPw._tool.BindVxs(vxs);
            return tmpPw;
        }
        public static TempContext<Arc> Borrow(out Arc arc)
        {
            if (!Temp<Arc>.IsInit())
            {
                Temp<Arc>.SetNewHandler(() => new Arc());
            }
            return Temp<Arc>.Borrow(out arc);
        }
        public static TempContext<SvgArcSegment> Borrow(out SvgArcSegment arc)
        {
            if (!Temp<SvgArcSegment>.IsInit())
            {
                Temp<SvgArcSegment>.SetNewHandler(() => new SvgArcSegment());
            }
            return Temp<SvgArcSegment>.Borrow(out arc);
        }
        public static TempContext<Ellipse> Borrow(out Ellipse ellipse)
        {
            if (!Temp<Ellipse>.IsInit())
            {
                Temp<Ellipse>.SetNewHandler(() => new Ellipse());
            }
            return Temp<Ellipse>.Borrow(out ellipse);
        }
        public static TempContext<SimpleRect> Borrow(out SimpleRect simpleRect)
        {
            if (!Temp<SimpleRect>.IsInit())
            {
                Temp<SimpleRect>.SetNewHandler(() => new SimpleRect());
            }
            return Temp<SimpleRect>.Borrow(out simpleRect);
        }
        public static TempContext<RoundedRect> Borrow(out RoundedRect roundRect)
        {
            if (!Temp<RoundedRect>.IsInit())
            {
                Temp<RoundedRect>.SetNewHandler(() => new RoundedRect());
            }
            return Temp<RoundedRect>.Borrow(out roundRect);
        }
        public static TempContext<VxsClipper> Borrow(out VxsClipper clipper)
        {
            if (!Temp<VxsClipper>.IsInit())
            {
                Temp<VxsClipper>.SetNewHandler(
                    () => new VxsClipper(),
                    c => c.Reset());
            }
            return Temp<VxsClipper>.Borrow(out clipper);
        }
        public static TempContext<CurveFlattener> Borrow(out CurveFlattener flattener)
        {
            if (!Temp<CurveFlattener>.IsInit())
            {
                Temp<CurveFlattener>.SetNewHandler(
                    () => new CurveFlattener(),
                    f => f.Reset());
            }
            return Temp<CurveFlattener>.Borrow(out flattener);
        }

    }
}
