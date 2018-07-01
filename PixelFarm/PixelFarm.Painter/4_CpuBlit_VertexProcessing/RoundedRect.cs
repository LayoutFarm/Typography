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
// Rounded rectangle vertex generator
//
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using PixelFarm.VectorMath;
using PixelFarm.Drawing;
namespace PixelFarm.CpuBlit.VertexProcessing
{
    //------------------------------------------------------------rounded_rect
    //
    // See Implemantation agg_rounded_rect.cpp
    //
    public class RoundedRect
    {
        RectD bounds;
        Vector2 leftBottomRadius;
        Vector2 rightBottomRadius;
        Vector2 rightTopRadius;
        Vector2 leftTopRadius;
        Arc currentProcessingArc = new Arc();
        public RoundedRect()
        {
        }
        public RoundedRect(double left, double bottom, double right, double top, double radius)
        {
            bounds = new RectD(left, bottom, right, top);
            leftBottomRadius.x = radius;
            leftBottomRadius.y = radius;
            rightBottomRadius.x = radius;
            rightBottomRadius.y = radius;
            rightTopRadius.x = radius;
            rightTopRadius.y = radius;
            leftTopRadius.x = radius;
            leftTopRadius.y = radius;
            if (left > right)
            {
                bounds.Left = right;
                bounds.Right = left;
            }

            if (bottom > top)
            {
                bounds.Bottom = top;
                bounds.Top = bottom;
            }
        }

        public RoundedRect(RectD bounds, double r)
            : this(bounds.Left, bounds.Bottom, bounds.Right, bounds.Top, r)
        {
        }

        public RoundedRect(RectInt bounds, double r)
            : this(bounds.Left, bounds.Bottom, bounds.Right, bounds.Top, r)
        {
        }

        public void SetRect(double left, double bottom, double right, double top)
        {
            bounds = new RectD(left, bottom, right, top);
            if (left > right) { bounds.Left = right; bounds.Right = left; }
            if (bottom > top) { bounds.Bottom = top; bounds.Top = bottom; }
        }

        public void SetRadius(double r)
        {
            leftBottomRadius.x = leftBottomRadius.y = rightBottomRadius.x = rightBottomRadius.y = rightTopRadius.x = rightTopRadius.y = leftTopRadius.x = leftTopRadius.y = r;
        }

        public void SetRadius(double rx, double ry)
        {
            leftBottomRadius.x = rightBottomRadius.x = rightTopRadius.x = leftTopRadius.x = rx;
            leftBottomRadius.y = rightBottomRadius.y = rightTopRadius.y = leftTopRadius.y = ry;
        }

        public void SetRadius(double leftBottomRadius, double rightBottomRadius, double rightTopRadius, double leftTopRadius)
        {
            this.leftBottomRadius = new Vector2(leftBottomRadius, leftBottomRadius);
            this.rightBottomRadius = new Vector2(rightBottomRadius, rightBottomRadius);
            this.rightTopRadius = new Vector2(rightTopRadius, rightTopRadius);
            this.leftTopRadius = new Vector2(leftTopRadius, leftTopRadius);
        }

        public void SetRadius(double rx1, double ry1, double rx2, double ry2,
                              double rx3, double ry3, double rx4, double ry4)
        {
            leftBottomRadius.x = rx1; leftBottomRadius.y = ry1; rightBottomRadius.x = rx2; rightBottomRadius.y = ry2;
            rightTopRadius.x = rx3; rightTopRadius.y = ry3; leftTopRadius.x = rx4; leftTopRadius.y = ry4;
        }

        public void NormalizeRadius()
        {
            double dx = Math.Abs(bounds.Top - bounds.Bottom);
            double dy = Math.Abs(bounds.Right - bounds.Left);
            double k = 1.0;
            double t;
            t = dx / (leftBottomRadius.x + rightBottomRadius.x); if (t < k) k = t;
            t = dx / (rightTopRadius.x + leftTopRadius.x); if (t < k) k = t;
            t = dy / (leftBottomRadius.y + rightBottomRadius.y); if (t < k) k = t;
            t = dy / (rightTopRadius.y + leftTopRadius.y); if (t < k) k = t;
            if (k < 1.0)
            {
                leftBottomRadius.x *= k; leftBottomRadius.y *= k; rightBottomRadius.x *= k; rightBottomRadius.y *= k;
                rightTopRadius.x *= k; rightTopRadius.y *= k; leftTopRadius.x *= k; leftTopRadius.y *= k;
            }
        }
        public double ApproximationScale
        {
            get { return currentProcessingArc.ApproximateScale; }
            set { currentProcessingArc.ApproximateScale = value; }
        }
        IEnumerable<VertexData> GetVertexIter()
        {
            currentProcessingArc.UseStartEndLimit = true;
            currentProcessingArc.Init(bounds.Left + leftBottomRadius.x, bounds.Bottom + leftBottomRadius.y, leftBottomRadius.x, leftBottomRadius.y, Math.PI, Math.PI + Math.PI * 0.5);
            currentProcessingArc.SetStartEndLimit(bounds.Left, bounds.Bottom + leftBottomRadius.y,
                bounds.Left + leftBottomRadius.x, bounds.Bottom);
            foreach (VertexData vertexData in currentProcessingArc.GetVertexIter())
            {
                if (VertexHelper.IsEmpty(vertexData.command))
                {
                    break;
                }
                yield return vertexData;
            }


            currentProcessingArc.Init(bounds.Right - rightBottomRadius.x, bounds.Bottom + rightBottomRadius.y, rightBottomRadius.x, rightBottomRadius.y, Math.PI + Math.PI * 0.5, 0.0);
            currentProcessingArc.SetStartEndLimit(bounds.Right - rightBottomRadius.x,
                bounds.Bottom, bounds.Right, bounds.Bottom + rightBottomRadius.y);
            foreach (VertexData vertexData in currentProcessingArc.GetVertexIter())
            {
                if (VertexHelper.IsMoveTo(vertexData.command))
                {
                    // skip the initial moveto
                    continue;
                }
                if (VertexHelper.IsEmpty(vertexData.command))
                {
                    break;
                }
                yield return vertexData;
            }


            currentProcessingArc.Init(bounds.Right - rightTopRadius.x, bounds.Top - rightTopRadius.y, rightTopRadius.x, rightTopRadius.y, 0.0, Math.PI * 0.5);
            currentProcessingArc.SetStartEndLimit(bounds.Right, bounds.Top - rightTopRadius.y,
                bounds.Right - rightTopRadius.x, bounds.Top);
            foreach (VertexData vertexData in currentProcessingArc.GetVertexIter())
            {
                if (VertexHelper.IsMoveTo(vertexData.command))
                {
                    // skip the initial moveto
                    continue;
                }
                if (VertexHelper.IsEmpty(vertexData.command))
                {
                    break;
                }
                yield return vertexData;
            }


            currentProcessingArc.Init(bounds.Left + leftTopRadius.x, bounds.Top - leftTopRadius.y, leftTopRadius.x, leftTopRadius.y, Math.PI * 0.5, Math.PI);
            currentProcessingArc.SetStartEndLimit(bounds.Left - leftTopRadius.x, bounds.Top,
                  bounds.Left, bounds.Top - leftTopRadius.y);
            foreach (VertexData vertexData in currentProcessingArc.GetVertexIter())
            {
                switch (vertexData.command)
                {
                    case VertexCmd.MoveTo:
                        continue;
                    case VertexCmd.NoMore:
                        break;
                    default:
                        yield return vertexData;
                        break;
                }
            }

            yield return new VertexData(VertexCmd.Close, (int)EndVertexOrientation.CCW, 0);
            yield return new VertexData(VertexCmd.NoMore);
        }

        public VertexStore MakeVxs(VertexStore vxs)
        {
            return VertexStoreBuilder.CreateVxs(this.GetVertexIter(), vxs);
        }
        public VertexStoreSnap MakeVertexSnap(VertexStore vxs)
        {
            return new VertexStoreSnap(this.MakeVxs(vxs));
        }
    }
}

