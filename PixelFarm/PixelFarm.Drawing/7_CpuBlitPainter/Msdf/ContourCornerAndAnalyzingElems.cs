//MIT, 2019-present, WinterDev

using System;
namespace ExtMsdfGen
{

    public enum AreaKind : byte
    {
        BorderInside,
        BorderOutside,

        OverlapInside,
        OverlapOutside,

        AreaInsideCoverageX,
        AreaInsideCoverage50,
        AreaInsideCoverage100,

    }
    public struct EdgeStructure
    {
        readonly EdgeSegment _edgeSegment;
        readonly EdgeSegment[] _edgeSegments;
        readonly AreaKind _areaKind;
        readonly bool _isEmpty;

        public EdgeStructure(EdgeSegment edgeSegment, AreaKind areaKind)
        {
            _isEmpty = false;
            _edgeSegment = edgeSegment;
            _areaKind = areaKind;
            //
            _edgeSegments = null;
        }
        public EdgeStructure(EdgeSegment[] edgeSegments, AreaKind areaKind)
        {
            _isEmpty = false;
            _edgeSegment = null;
            _edgeSegments = edgeSegments;
            _areaKind = areaKind;
        }

        public bool HasOverlappedSegments => _edgeSegments != null;
        public EdgeSegment Segment => _edgeSegment;
        public EdgeSegment[] Segments => _edgeSegments;

        public AreaKind AreaKind => _areaKind;
        public bool IsEmpty => _isEmpty;
        public static readonly EdgeStructure Empty = new EdgeStructure();

    }

    public class Vec2Info
    {
        public readonly double x, y;
        public readonly Vec2PointKind Kind;
        public readonly EdgeSegment owner;
        public Vec2Info(EdgeSegment owner, Vec2PointKind kind, Vector2 point)
        {
            this.owner = owner;
            this.x = point.x;
            this.y = point.y;
            Kind = kind;
        }
    }
    public enum Vec2PointKind
    {
        Touch1,//on curve point
        C2, //quadratic curve control point (off-curve)
        C3, //cubic curve control point (off-curve)
        Touch2, //on curve point
    }

    public class ContourCorner
    {

#if DEBUG
        public int dbugLeftIndex;
        public int dbugMiddleIndex;
        public int dbugRightIndex;
#endif

        PixelFarm.Drawing.PointD _pLeft;
        PixelFarm.Drawing.PointD _pCenter;
        PixelFarm.Drawing.PointD _pRight;

        ushort _cornerNo;
        //-----------
        Vec2Info _left; //left 
        Vec2Info _center;
        Vec2Info _right;
        //-----------

        public ContourCorner(int cornerNo, Vec2Info left, Vec2Info center, Vec2Info right)
        {

            if (cornerNo >= ushort.MaxValue) throw new NotSupportedException();

            _cornerNo = (ushort)cornerNo;
            _left = left;
            _center = center;
            _right = right;

            _pLeft = new PixelFarm.Drawing.PointD(left.x, left.y);
            _pCenter = new PixelFarm.Drawing.PointD(center.x, center.y);
            _pRight = new PixelFarm.Drawing.PointD(right.x, right.y);
        }


        /// <summary>
        /// corner number in flatten list
        /// </summary>
        internal ushort CornerNo => _cornerNo;

        public PixelFarm.Drawing.PointD MiddlePoint => _pCenter;
        public PixelFarm.Drawing.PointD LeftPoint => _pLeft;
        public PixelFarm.Drawing.PointD RightPoint => _pRight;

        public EdgeSegment CenterSegment => _center.owner;

        public Vec2PointKind LeftPointKind => _left.Kind;
        public Vec2PointKind MiddlePointKind => _center.Kind;
        public Vec2PointKind RightPointKind => _right.Kind;


        public PixelFarm.Drawing.Color OuterColor => EdgeBmpLut.EncodeToColor(CornerNo, AreaKind.BorderOutside);
        public PixelFarm.Drawing.Color InnerColor => EdgeBmpLut.EncodeToColor(CornerNo, AreaKind.BorderInside);

        public void Offset(double dx, double dy)
        {
            //
            _pLeft = PixelFarm.Drawing.PointD.OffsetPoint(_pLeft, dx, dy);
            _pCenter = PixelFarm.Drawing.PointD.OffsetPoint(_pCenter, dx, dy);
            _pRight = PixelFarm.Drawing.PointD.OffsetPoint(_pRight, dx, dy);
        }

        public bool MiddlePoint_IsTouchPoint => MiddlePointKind == Vec2PointKind.Touch1 || MiddlePointKind == Vec2PointKind.Touch2;
        public bool LeftPoint_IsTouchPoint => LeftPointKind == Vec2PointKind.Touch1 || LeftPointKind == Vec2PointKind.Touch2;
        public bool RightPoint_IsTouchPoint => RightPointKind == Vec2PointKind.Touch1 || RightPointKind == Vec2PointKind.Touch2;


#if DEBUG
        public PixelFarm.Drawing.PointD dbugLeftPoint => _pLeft;
        public PixelFarm.Drawing.PointD dbugRightPoint => _pRight;
        static double dbugCurrentLen(PixelFarm.Drawing.PointD p0, PixelFarm.Drawing.PointD p1)
        {
            double dx = p1.X - p0.X;
            double dy = p1.Y - p0.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
        public override string ToString()
        {
            return dbugLeftIndex + "," + dbugMiddleIndex + "(" + MiddlePoint + ")," + dbugRightIndex;
        }
#endif
    }
}