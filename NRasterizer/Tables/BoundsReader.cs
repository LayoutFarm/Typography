using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRasterizer.Tables
{
    public static class BoundsReader
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
