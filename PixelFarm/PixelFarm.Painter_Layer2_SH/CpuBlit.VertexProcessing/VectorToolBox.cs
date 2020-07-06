//BSD, 2014-present, WinterDev

using System;
using System.Collections.Generic;
using PixelFarm.Drawing;

namespace PixelFarm.CpuBlit.VertexProcessing
{

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


    static class VxsTemp
    {

        public static VxsContext1 Borrow(out VertexStore vxs) => new VxsContext1(out vxs);

        public static VxsContext2 Borrow(out VertexStore vxs1, out VertexStore vxs2) => new VxsContext2(out vxs1, out vxs2);

        public static VxsContext3 Borrow(out VertexStore vxs1, out VertexStore vxs2, out VertexStore vxs3) => new VxsContext3(out vxs1, out vxs2, out vxs3);

        ////for net20 -- check this
        ////TODO: https://stackoverflow.com/questions/18333885/threadstatic-v-s-threadlocalt-is-generic-better-than-attribute

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
                VertexStore vxs = new VertexStore();
                VertexStore.SetSharedState(vxs, true);
                return vxs;
            }
        }
    }



    static class VectorToolBox
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

        
    }



    public class PolygonSimplifier
    {
        public PolygonSimplifier()
        {

        }
        public void Reset()
        {
            EnableHighQuality = false;
            Tolerance = 0.5f;//default
        }
        public bool EnableHighQuality { get; set; }
        public float Tolerance { get; set; }
        public void Simplify(List<VectorMath.Vector2> inputPoints, List<VectorMath.Vector2> simplifiedOutput)
        {

            PixelFarm.CpuBlit.VertexProcessing.SimplificationHelpers.Simplify(
                 inputPoints,
                 (p1, p2) => p1 == p2,
                     p => p.x,
                     p => p.y,
                     simplifiedOutput,
                     Tolerance,
                     EnableHighQuality);
        }
    }

    public interface IVector2dProvider
    {
        int CoordCount { get; }
        void GetCoord(int index, out double x, out double y);
        void GetCoord(int index, out float x, out float y);
    }

    public class Vec2dSource : IVector2dProvider
    {
        List<PixelFarm.VectorMath.Vector2> _vecSource;
        public Vec2dSource()
        {
        }
        public void SetVectorSource(List<PixelFarm.VectorMath.Vector2> source)
        {
            _vecSource = source;
        }
        int IVector2dProvider.CoordCount => _vecSource.Count;
        void IVector2dProvider.GetCoord(int index, out double x, out double y)
        {
            PixelFarm.VectorMath.Vector2 vec = _vecSource[index];
            x = vec.x;
            y = vec.y;
        }
        void IVector2dProvider.GetCoord(int index, out float x, out float y)
        {
            PixelFarm.VectorMath.Vector2 vec = _vecSource[index];
            x = (float)vec.x;
            y = (float)vec.y;
        }
    }


    public static class PathWriterExtensions
    {
        public static void WritePolylines(this PathWriter pw, IVector2dProvider points)
        {
            int count = points.CoordCount;
            if (count > 1)
            {
                points.GetCoord(0, out double x, out double y);
                pw.MoveTo(x, y);
                for (int i = 1; i < count; ++i)
                {
                    points.GetCoord(i, out x, out y);
                    pw.LineTo(x, y);
                }
            }
        }
        public static void WritePolylines(this PathWriter pw, float[] points)
        {
            int j = points.Length;
            if (j >= 4)
            {
                pw.MoveTo(points[0], points[1]);
                for (int i = 2; i < j;)
                {
                    pw.LineTo(points[i], points[i + 1]);
                    i += 2;
                }
            }
        }
        public static void WritePolylines(this PathWriter pw, double[] points)
        {
            int j = points.Length;
            if (j >= 4)
            {
                pw.MoveTo(points[0], points[1]);
                for (int i = 2; i < j;)
                {
                    pw.LineTo(points[i], points[i + 1]);
                    i += 2;
                }
            }
        }

        //-----------------------------------------------------------------------------------------
        public static void WritePolygon(this PathWriter pw, IVector2dProvider points)
        {
            int count = points.CoordCount;
            if (count > 1)
            {
                points.GetCoord(0, out double x, out double y);
                pw.MoveTo(x, y);
                for (int i = 1; i < count; ++i)
                {
                    pw.LineTo(x, y);
                }
                pw.CloseFigure();
            }

        }
        public static void WritePolygon(this PathWriter pw, float[] points)
        {
            int j = points.Length;
            if (j >= 4)
            {
                pw.MoveTo(points[0], points[1]);
                for (int i = 2; i < j;)
                {
                    pw.LineTo(points[i], points[i + 1]);
                    i += 2;
                }
                pw.CloseFigure();
            }
        }
        public static void WritePolygon(this PathWriter pw, double[] points)
        {
            int j = points.Length;
            if (j >= 4)
            {
                pw.MoveTo(points[0], points[1]);
                for (int i = 2; i < j;)
                {
                    pw.LineTo(points[i], points[i + 1]);
                    i += 2;
                }
                pw.CloseFigure();
            }
        }
        //-----------------------------------------------------------------------------------------

        public static void WriteSmoothCurve3(this PathWriter pw, IVector2dProvider points, bool closedShape)
        {
            int coordCount = points.CoordCount;
            switch (coordCount)
            {
                case 0:
                case 1: return;
                case 2:
                    {
                        points.GetCoord(0, out double x, out double y);
                        pw.MoveTo(x, y);
                        points.GetCoord(1, out x, out y);
                        pw.LineTo(x, y);
                    }
                    break;
                case 3:
                    {
                        points.GetCoord(0, out double x0, out double y0);
                        points.GetCoord(1, out double x1, out double y1);
                        points.GetCoord(2, out double x2, out double y2);
                        pw.MoveTo(x0, y0);
                        pw.Curve3(x1, y1, x2, y2);
                        if (closedShape)
                        {
                            pw.SmoothCurve3(x0, y0);
                        }
                    }
                    break;
                default:
                    {
                        points.GetCoord(0, out double x0, out double y0);
                        points.GetCoord(1, out double x1, out double y1);
                        points.GetCoord(2, out double x2, out double y2);
                        pw.MoveTo(x0, y0);
                        pw.Curve3(x1, y1, x2, y2);
                        for (int i = 3; i < coordCount; ++i)
                        {
                            points.GetCoord(i, out double x, out double y);
                            pw.SmoothCurve3(x, y);
                        }
                        if (closedShape)
                        {
                            pw.SmoothCurve3(x0, y0);
                        }
                    }
                    break;
            }
        }
        public static void WriteSmoothCurve4(this PathWriter pw, IVector2dProvider points, bool closedShape)
        {
            int coordCount = points.CoordCount;
            switch (coordCount)
            {
                case 0:
                case 1: return;
                case 2:
                    {
                        points.GetCoord(0, out double x, out double y);
                        pw.MoveTo(x, y);
                        points.GetCoord(1, out x, out y);
                        pw.LineTo(x, y);
                    }
                    break;
                case 3:
                    {
                        points.GetCoord(0, out double x0, out double y0);
                        points.GetCoord(1, out double x1, out double y1);
                        points.GetCoord(2, out double x2, out double y2);
                        pw.MoveTo(x0, y0);
                        pw.Curve3(x1, y1, x2, y2);//***, we have 3 points, so we use Curve3
                        if (closedShape)
                        {
                            pw.SmoothCurve3(x0, y0);
                        }
                    }
                    break;
                default:
                    {
                        points.GetCoord(0, out double x0, out double y0);
                        points.GetCoord(1, out double x1, out double y1);
                        points.GetCoord(2, out double x2, out double y2);
                        points.GetCoord(3, out double x3, out double y3);
                        pw.MoveTo(x0, y0);
                        pw.Curve4(x1, y1, x2, y2, x3, y3);

                        for (int i = 4; i < coordCount - 1;)
                        {
                            points.GetCoord(i, out x2, out y2);
                            points.GetCoord(i + 1, out x3, out y3);
                            pw.SmoothCurve4(x2, y2, x3, y3);
                            i += 2;
                        }
                        if (closedShape)
                        {
                            pw.SmoothCurve4(x3, y3, x0, y0);
                        }
                    }
                    break;
            }
        }
    }




    public class Spiral
    {
        double _x;
        double _y;
        double _r1;
        double _r2;
        double _step;
        double _start_angle;
        double _angle;
        double _curr_r;
        double _da;
        double _dr;
        bool _start;
        public Spiral()
        {

        }
        public void SetParameters(double x, double y, double r1, double r2, double step, double start_angle = 0)
        {
            _x = x;
            _y = y;
            _r1 = r1;
            _r2 = r2;
            _step = step;
            _start_angle = start_angle;
            _angle = start_angle;
            _da = AggMath.deg2rad(4.0);
            _dr = _step / 90.0;
        }
        IEnumerable<VertexData> GetVertexIter()
        {
            //--------------
            //rewind
            _angle = _start_angle;
            _curr_r = _r1;
            _start = true;
            //--------------

            VertexCmd cmd;
            double x, y;
            for (; ; )
            {
                cmd = GetNextVertex(out x, out y);
                switch (cmd)
                {
                    case VertexCmd.NoMore:
                        {
                            yield return new VertexData(cmd, x, y);
                            yield break;
                        }
                    default:
                        {
                            yield return new VertexData(cmd, x, y);
                        }
                        break;
                }
            }
        }
        public VertexStore MakeVxs(VertexStore vxs)
        {

            foreach (VertexData v in this.GetVertexIter())
            {
                vxs.AddVertex(v.x, v.y, v.command);
            }
            return vxs;
        }

        public VertexCmd GetNextVertex(out double x, out double y)
        {
            x = 0;
            y = 0;
            if (_curr_r > _r2)
            {
                return VertexCmd.NoMore;
            }

            x = _x + Math.Cos(_angle) * _curr_r;
            y = _y + Math.Sin(_angle) * _curr_r;
            _curr_r += _dr;
            _angle += _da;
            if (_start)
            {
                _start = false;
                return VertexCmd.MoveTo;
            }
            return VertexCmd.LineTo;
        }
    }
}
