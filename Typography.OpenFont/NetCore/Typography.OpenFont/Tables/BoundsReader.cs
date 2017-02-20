//Apache2, 2014-2016, Samuel Carlsson

using System.IO;
namespace Typography.OpenFont
{
    static class BoundsReader
    {
        public static Bounds ReadFrom(BinaryReader input)
        { 
            //xmin, ymin,xmax,ymax
            return new Bounds(input.ReadInt16(), input.ReadInt16(), input.ReadInt16(), input.ReadInt16());
        }
    }
}
