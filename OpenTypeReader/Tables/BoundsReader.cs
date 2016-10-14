//Apache2, 2014-2016, Samuel Carlsson

using System.IO;
namespace NRasterizer.Tables
{
    static class BoundsReader
    {
        public static Bounds ReadFrom(BinaryReader input)
        {
            var xMin = input.ReadInt16();
            var yMin = input.ReadInt16();
            var xMax = input.ReadInt16();
            var yMax = input.ReadInt16();
            return new Bounds(xMin, yMin, xMax, yMax);
        }
    }
}
