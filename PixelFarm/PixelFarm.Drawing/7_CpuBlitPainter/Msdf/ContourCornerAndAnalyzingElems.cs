//MIT, 2019-present, WinterDev

using System;
using System.Collections.Generic;
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

        public bool IsOverlapList => _edgeSegments != null;
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



        public ContourCorner NextCorner { get; private set; }
        public ContourCorner PrevCorner { get; private set; }

        internal static void ConnectToEachOther(ContourCorner a, ContourCorner b)
        {
            a.NextCorner = b;
            b.PrevCorner = a;
        }

        public PixelFarm.Drawing.PointD ExtPoint_LeftOuterDest => NextCorner.ExtPoint_RightOuter;
        public PixelFarm.Drawing.PointD ExtPoint_LeftInnerDest => NextCorner.ExtPoint_RightInner;

        public PixelFarm.Drawing.PointD ExtPoint_RightOuterDest => PrevCorner.ExtPoint_LeftOuter;
        public PixelFarm.Drawing.PointD ExtPoint_RightInnerDest => PrevCorner.ExtPoint_LeftInner;



        public PixelFarm.Drawing.PointD LeftPoint => _pLeft;
        public PixelFarm.Drawing.PointD middlePoint => _pCenter;
        public PixelFarm.Drawing.PointD RightPoint => _pRight;

        public EdgeSegment LeftSegment => _left.owner;
        public EdgeSegment CenterSegment => _center.owner;
        public EdgeSegment RightSegment => _right.owner;

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

        public bool MiddlePointKindIsTouchPoint => MiddlePointKind == Vec2PointKind.Touch1 || MiddlePointKind == Vec2PointKind.Touch2;
        public bool LeftPointKindIsTouchPoint => LeftPointKind == Vec2PointKind.Touch1 || LeftPointKind == Vec2PointKind.Touch2;
        public bool RightPointKindIsTouchPoint => RightPointKind == Vec2PointKind.Touch1 || RightPointKind == Vec2PointKind.Touch2;
        static double CurrentLen(PixelFarm.Drawing.PointD p0, PixelFarm.Drawing.PointD p1)
        {
            double dx = p1.X - p0.X;
            double dy = p1.Y - p0.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
        //-----------
        /// <summary>
        /// extended point of left->middle line
        /// </summary>
        public PixelFarm.Drawing.PointD ExtPoint_LeftOuter => CreateExtendedOuterEdges(LeftPoint, middlePoint);
        public PixelFarm.Drawing.PointD ExtPoint_LeftInner => CreateExtendedInnerEdges(LeftPoint, middlePoint);
        /// <summary>
        /// extended point of right->middle line
        /// </summary>
        public PixelFarm.Drawing.PointD ExtPoint_RightOuter => CreateExtendedOuterEdges(RightPoint, middlePoint);
        public PixelFarm.Drawing.PointD ExtPoint_RightOuter2 => CreateExtendedOuterEdges(RightPoint, middlePoint, 2);
        public PixelFarm.Drawing.PointD ExtPoint_RightInner => CreateExtendedInnerEdges(RightPoint, middlePoint);


        PixelFarm.Drawing.PointD CreateExtendedOuterEdges(PixelFarm.Drawing.PointD p0, PixelFarm.Drawing.PointD p1, double dlen = 3)
        {

            //create perpendicular line 

            PixelFarm.VectorMath.Vector2 v2 = new PixelFarm.VectorMath.Vector2(p1.X - p0.X, p1.Y - p0.Y);
            PixelFarm.VectorMath.Vector2 r1 = v2.RotateInDegree(90).NewLength(3);
            return new PixelFarm.Drawing.PointD(p1.X + r1.x, p1.Y + r1.Y);
            //if (LeftPointKind == Vec2PointKind.Touch1 || LeftPointKind == Vec2PointKind.Touch2)
            //{
            //    double rad = Math.Atan2(p1.Y - p0.Y, p1.X - p0.X);
            //    double currentLen = CurrentLen(p0, p1);
            //    double newLen = currentLen + dlen;

            //    //double new_dx = Math.Cos(rad) * newLen;
            //    //double new_dy = Math.Sin(rad) * newLen;
            //    return new PixelFarm.Drawing.PointD(p0.X + (Math.Cos(rad) * newLen), p0.Y + (Math.Sin(rad) * newLen));
            //}
            //else
            //{

            //    //create perpendicular line 

            //    PixelFarm.VectorMath.Vector2 v2 = new PixelFarm.VectorMath.Vector2(p1.X - p0.X, p1.Y - p0.Y);
            //    PixelFarm.VectorMath.Vector2 r1 = v2.RotateInDegree(90).NewLength(3);
            //    return new PixelFarm.Drawing.PointD(p1.X + r1.x, p1.Y + r1.Y);
            //}
        }

        PixelFarm.Drawing.PointD CreateExtendedInnerEdges(PixelFarm.Drawing.PointD p0, PixelFarm.Drawing.PointD p1)
        {
            PixelFarm.VectorMath.Vector2 v2 = new PixelFarm.VectorMath.Vector2(p1.X - p0.X, p1.Y - p0.Y);
            PixelFarm.VectorMath.Vector2 r1 = v2.RotateInDegree(270).NewLength(3);
            return new PixelFarm.Drawing.PointD(p1.X + r1.x, p1.Y + r1.Y);
            //if (LeftPointKind == Vec2PointKind.Touch1 || LeftPointKind == Vec2PointKind.Touch2)
            //{
            //    double rad = Math.Atan2(p1.Y - p0.Y, p1.X - p0.X);
            //    double currentLen = CurrentLen(p0, p1);
            //    if (currentLen - 3 < 0)
            //    {
            //        return p0;//***
            //    }
            //    double newLen = currentLen - 3;
            //    //double new_dx = Math.Cos(rad) * newLen;
            //    //double new_dy = Math.Sin(rad) * newLen;
            //    return new PixelFarm.Drawing.PointD(p0.X + (Math.Cos(rad) * newLen), p0.Y + (Math.Sin(rad) * newLen));
            //}
            //else
            //{
            //    PixelFarm.VectorMath.Vector2 v2 = new PixelFarm.VectorMath.Vector2(p1.X - p0.X, p1.Y - p0.Y);
            //    PixelFarm.VectorMath.Vector2 r1 = v2.RotateInDegree(270).NewLength(3);
            //    return new PixelFarm.Drawing.PointD(p1.X + r1.x, p1.Y + r1.Y);
            //}

        }
#if DEBUG
        public override string ToString()
        {
            return dbugLeftIndex + "," + dbugMiddleIndex + "(" + middlePoint + ")," + dbugRightIndex;
        }
#endif
    }
}