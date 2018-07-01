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
//
// Affine transformation classes.
//
//----------------------------------------------------------------------------
//#ifndef AGG_TRANS_AFFINE_INCLUDED
//#define AGG_TRANS_AFFINE_INCLUDED

//#include <math.h>
//#include "agg_basics.h"


using PixelFarm.Drawing;
namespace PixelFarm.CpuBlit.VertexProcessing
{

    public static class VertexStoreTransformExtensions
    {
        public static void TransformToVertexSnap(this Affine affine, VertexStore src, VertexStore output)
        {
            affine.TransformToVxs(src, output);
        }
        /// <summary>
        /// we do NOT store vxs, return original outputVxs
        /// </summary>
        /// <param name="src"></param>
        /// <param name="outputVxs"></param>
        public static VertexStore TransformToVxs(this Affine aff, VertexStore src, VertexStore outputVxs)
        {
            int count = src.Count;
            VertexCmd cmd;
            double x, y;
            for (int i = 0; i < count; ++i)
            {
                cmd = src.GetVertex(i, out x, out y);
                aff.Transform(ref x, ref y);
                outputVxs.AddVertex(x, y, cmd);
            }

            //outputVxs.HasMoreThanOnePart = src.HasMoreThanOnePart;
            return outputVxs;
        }
        /// <summary>
        /// we do NOT store vxs, return original outputVxs
        /// </summary>
        /// <param name="src"></param>
        /// <param name="outputVxs"></param>
        /// <returns></returns>
        public static VertexStore TransformToVxs(this Affine aff, VertexStoreSnap src, VertexStore outputVxs)
        {
            var snapIter = src.GetVertexSnapIter();
            VertexCmd cmd;
            double x, y;
            while ((cmd = snapIter.GetNextVertex(out x, out y)) != VertexCmd.NoMore)
            {
                aff.Transform(ref x, ref y);
                outputVxs.AddVertex(x, y, cmd);
            }
            return outputVxs;
        }
        public static VertexStore TransformToVxs(this Bilinear bilinearTx, VertexStore src, VertexStore vxs)
        {
            int count = src.Count;
            VertexCmd cmd;
            double x, y;
            for (int i = 0; i < count; ++i)
            {
                cmd = src.GetVertex(i, out x, out y);
                bilinearTx.Transform(ref x, ref y);
                vxs.AddVertex(x, y, cmd);
            }
            return vxs;
        }
        public static VertexStore TransformToVxs(this Perspective perspecitveTx, VertexStoreSnap snap, VertexStore vxs)
        {
            var vsnapIter = snap.GetVertexSnapIter();
            double x, y;
            VertexCmd cmd;
            do
            {
                cmd = vsnapIter.GetNextVertex(out x, out y);
                perspecitveTx.Transform(ref x, ref y);
                vxs.AddVertex(x, y, cmd);
            } while (!VertexHelper.IsEmpty(cmd));
            return vxs;
        }
        public static VertexStore TransformToVxs(this Perspective perspecitveTx, VertexStore src, VertexStore vxs)
        {
            VertexCmd cmd;
            double x, y;
            int count = src.Count;
            for (int i = 0; i < count; ++i)
            {
                cmd = src.GetVertex(i, out x, out y);
                perspecitveTx.Transform(ref x, ref y);
                vxs.AddVertex(x, y, cmd);
            }
            return vxs;
        }
    }




}