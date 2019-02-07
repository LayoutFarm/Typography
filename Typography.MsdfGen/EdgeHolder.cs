//MIT, 2016, Viktor Chlumsky, Multi-channel signed distance field generator, from https://github.com/Chlumsky/msdfgen
//MIT, 2017-present, WinterDev (C# port)

namespace Msdfgen
{
    //#include "EdgeHolder.h"

    public class EdgeHolder
    {
        public EdgeSegment edgeSegment;
        public EdgeHolder(EdgeSegment segment) => this.edgeSegment = segment;
        public EdgeColor color
        {
            get => edgeSegment.color;
            set => edgeSegment.color = value;
        }

        public bool HasComponent(EdgeColor c) => (color & c) != 0;
        public Vector2 Direction(double param) => (edgeSegment != null) ? edgeSegment.direction(param) : default(Vector2);
        public Vector2 point(double param) => edgeSegment.point(param);
#if DEBUG
        public override string ToString()=> edgeSegment.ToString();
#endif
    }
}