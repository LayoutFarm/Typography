//BSD, 2014-present, WinterDev
//----------------------------------------------------------------------------
// Anti-Grain Geometry - Version 2.4
// Copyright (C) 2002-2005 Maxim Shemanarev (http://www.antigrain.com)
//
// C# Port port by: Lars Brubaker
//                  larsbrubaker@gmail.com
// Copyright (C) 2007
//
// Permission to copy, use, modify, sell and distribute this software 
// is granted provided this copyright notice appears in all copies. 
// This software is provided "as is" without express or implied
// warranty, and with no claim as to its suitability for any purpose.
//
//----------------------------------------------------------------------------
// Contact: mcseem@antigrain.com
//          mcseemagg@yahoo.com
//          http://www.antigrain.com
//----------------------------------------------------------------------------



using PixelFarm.CpuBlit;
using PixelFarm.CpuBlit.VertexProcessing;

namespace PixelFarm.Drawing
{
    public static class VertexStoreExtensions2
    {
        /// <summary>
        /// copy + translate vertext data from src to outputVxs
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        /// <param name="outputVxs"></param>
        /// <returns></returns>
        public static VertexStore TranslateToNewVxs(this VertexStore src, double dx, double dy, VertexStore outputVxs)
        {
            int count = src.Count;
            VertexCmd cmd;
            double x, y;
            for (int i = 0; i < count; ++i)
            {
                cmd = src.GetVertex(i, out x, out y);
                x += dx;
                y += dy;
                outputVxs.AddVertex(x, y, cmd);
            }
            return outputVxs;
        }
        public static VertexStore ScaleToNewVxs(this VertexStore src, double s, VertexStore outputVxs)
        {
            //TODO: review here
            Affine aff = Affine.NewScaling(s, s);
            return aff.TransformToVxs(src, outputVxs);
        }
        public static VertexStore ScaleToNewVxs(this VertexStore src, double sx, double sy, VertexStore outputVxs)
        {
            //TODO: review here
            Affine aff = Affine.NewScaling(sx, sy);
            return aff.TransformToVxs(src, outputVxs);
        }

        public static VertexStore RotateToNewVxs(this VertexStore src, double deg, VertexStore outputVxs)
        {
            //TODO: review here
            Affine aff = Affine.NewRotationDeg(deg);
            return aff.TransformToVxs(src, outputVxs);
        }
    }
}