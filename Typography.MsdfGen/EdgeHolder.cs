//MIT, 2016, Viktor Chlumsky, Multi-channel signed distance field generator, from https://github.com/Chlumsky/msdfgen
//MIT, 2017, WinterDev (C# port)

namespace Msdfgen
{

    public class EdgeHolder
    {
        public EdgeSegment edgeSegment;
        public EdgeHolder(EdgeSegment segment)
        {
            this.edgeSegment = segment;
        }
        public EdgeColor color
        {
            get { return edgeSegment.color; }
            set
            {
                edgeSegment.color = value;
            }
        }

        public bool HasComponent(EdgeColor c)
        {
            return (color & c) != 0;
        }
        public Vector2 Direction(double param)
        {
            if (edgeSegment != null)
            {
                return edgeSegment.direction(param);
            }
            else
            {
                return default(Vector2);
            }
        }
        public Vector2 point(double param)
        {
            return edgeSegment.point(param);
        }
    }

}

//#include "stdafx.h"
//#include "EdgeHolder.h"

//namespace msdfgen
//{

//EdgeHolder::EdgeHolder() : edgeSegment(NULL) { }

//    EdgeHolder::EdgeHolder(EdgeSegment* segment) : edgeSegment(segment) { }

//    EdgeHolder::EdgeHolder(Point2 p0, Point2 p1, EdgeColor edgeColor) : edgeSegment(new LinearSegment(p0, p1, edgeColor)) { }

//EdgeHolder::EdgeHolder(Point2 p0, Point2 p1, Point2 p2, EdgeColor edgeColor) : edgeSegment(new QuadraticSegment(p0, p1, p2, edgeColor)) { }

//EdgeHolder::EdgeHolder(Point2 p0, Point2 p1, Point2 p2, Point2 p3, EdgeColor edgeColor) : edgeSegment(new CubicSegment(p0, p1, p2, p3, edgeColor)) { }

//EdgeHolder::EdgeHolder(const EdgeHolder &orig) : edgeSegment(orig.edgeSegment? orig.edgeSegment->clone() : NULL) { }

//#ifdef MSDFGEN_USE_CPP11
//EdgeHolder::EdgeHolder(EdgeHolder &&orig) : edgeSegment(orig.edgeSegment)
//{
//    orig.edgeSegment = NULL;
//}
//#endif

//EdgeHolder::~EdgeHolder()
//{
//    delete edgeSegment;
//}

//EdgeHolder & EdgeHolder::operator=(const EdgeHolder &orig)
//{
//    delete edgeSegment;
//    edgeSegment = orig.edgeSegment ? orig.edgeSegment->clone() : NULL;
//    return *this;
//}

//#ifdef MSDFGEN_USE_CPP11
//EdgeHolder & EdgeHolder::operator=(EdgeHolder &&orig)
//{
//    delete edgeSegment;
//    edgeSegment = orig.edgeSegment;
//    orig.edgeSegment = NULL;
//    return *this;
//}
//#endif

//EdgeSegment & EdgeHolder::operator *()
//{
//    return *edgeSegment;
//}

//const EdgeSegment & EdgeHolder::operator *() const {
//    return * edgeSegment;
//}

//EdgeSegment* EdgeHolder::operator->()
//{
//    return edgeSegment;
//}

//const EdgeSegment* EdgeHolder::operator->() const {
//    return edgeSegment;
//}

//EdgeHolder::operator EdgeSegment *()
//{
//    return edgeSegment;
//}

//EdgeHolder::operator const EdgeSegment*() const {
//    return edgeSegment;
//}

//}
