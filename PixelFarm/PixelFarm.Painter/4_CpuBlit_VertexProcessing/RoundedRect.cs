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
        RectD _bounds;
        Vector2 _leftBottomRadius;
        Vector2 _rightBottomRadius;
        Vector2 _rightTopRadius;
        Vector2 _leftTopRadius;
        Arc _currentProcessingArc = new Arc();
        public RoundedRect()
        {
        }
        public RoundedRect(double left, double bottom, double right, double top, double radius)
        {
            _bounds = new RectD(left, bottom, right, top);
            _leftBottomRadius.x = radius;
            _leftBottomRadius.y = radius;
            _rightBottomRadius.x = radius;
            _rightBottomRadius.y = radius;
            _rightTopRadius.x = radius;
            _rightTopRadius.y = radius;
            _leftTopRadius.x = radius;
            _leftTopRadius.y = radius;
            if (left > right)
            {
                _bounds.Left = right;
                _bounds.Right = left;
            }

            if (bottom > top)
            {
                _bounds.Bottom = top;
                _bounds.Top = bottom;
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
            _bounds = new RectD(left, bottom, right, top);
            if (left > right) { _bounds.Left = right; _bounds.Right = left; }
            if (bottom > top) { _bounds.Bottom = top; _bounds.Top = bottom; }
        }

        public void SetRadius(double r)
        {
            _leftBottomRadius.x = _leftBottomRadius.y = _rightBottomRadius.x = _rightBottomRadius.y = _rightTopRadius.x = _rightTopRadius.y = _leftTopRadius.x = _leftTopRadius.y = r;
        }

        public void SetRadius(double rx, double ry)
        {
            _leftBottomRadius.x = _rightBottomRadius.x = _rightTopRadius.x = _leftTopRadius.x = rx;
            _leftBottomRadius.y = _rightBottomRadius.y = _rightTopRadius.y = _leftTopRadius.y = ry;
        }

        public void SetRadius(double leftBottomRadius, double rightBottomRadius, double rightTopRadius, double leftTopRadius)
        {
            _leftBottomRadius = new Vector2(leftBottomRadius, leftBottomRadius);
            _rightBottomRadius = new Vector2(rightBottomRadius, rightBottomRadius);
            _rightTopRadius = new Vector2(rightTopRadius, rightTopRadius);
            _leftTopRadius = new Vector2(leftTopRadius, leftTopRadius);
        }

        public void SetRadius(double rx1, double ry1, double rx2, double ry2,
                              double rx3, double ry3, double rx4, double ry4)
        {
            _leftBottomRadius.x = rx1; _leftBottomRadius.y = ry1; _rightBottomRadius.x = rx2; _rightBottomRadius.y = ry2;
            _rightTopRadius.x = rx3; _rightTopRadius.y = ry3; _leftTopRadius.x = rx4; _leftTopRadius.y = ry4;
        }

        public void NormalizeRadius()
        {
            double dx = Math.Abs(_bounds.Top - _bounds.Bottom);
            double dy = Math.Abs(_bounds.Right - _bounds.Left);
            double k = 1.0;
            double t;
            t = dx / (_leftBottomRadius.x + _rightBottomRadius.x); if (t < k) k = t;
            t = dx / (_rightTopRadius.x + _leftTopRadius.x); if (t < k) k = t;
            t = dy / (_leftBottomRadius.y + _rightBottomRadius.y); if (t < k) k = t;
            t = dy / (_rightTopRadius.y + _leftTopRadius.y); if (t < k) k = t;
            if (k < 1.0)
            {
                _leftBottomRadius.x *= k; _leftBottomRadius.y *= k; _rightBottomRadius.x *= k; _rightBottomRadius.y *= k;
                _rightTopRadius.x *= k; _rightTopRadius.y *= k; _leftTopRadius.x *= k; _leftTopRadius.y *= k;
            }
        }
        public double ApproximationScale
        {
            get => _currentProcessingArc.ApproximateScale;
            set => _currentProcessingArc.ApproximateScale = value;
        }
        IEnumerable<VertexData> GetVertexIter()
        {
            _currentProcessingArc.UseStartEndLimit = true;
            _currentProcessingArc.Init(_bounds.Left + _leftBottomRadius.x, _bounds.Bottom + _leftBottomRadius.y, _leftBottomRadius.x, _leftBottomRadius.y, Math.PI, Math.PI + Math.PI * 0.5);
            _currentProcessingArc.SetStartEndLimit(_bounds.Left, _bounds.Bottom + _leftBottomRadius.y,
                _bounds.Left + _leftBottomRadius.x, _bounds.Bottom);
            foreach (VertexData vertexData in _currentProcessingArc.GetVertexIter())
            {
                if (VertexHelper.IsEmpty(vertexData.command))
                {
                    break;
                }
                yield return vertexData;
            }


            _currentProcessingArc.Init(_bounds.Right - _rightBottomRadius.x, _bounds.Bottom + _rightBottomRadius.y, _rightBottomRadius.x, _rightBottomRadius.y, Math.PI + Math.PI * 0.5, 0.0);
            _currentProcessingArc.SetStartEndLimit(_bounds.Right - _rightBottomRadius.x,
                _bounds.Bottom, _bounds.Right, _bounds.Bottom + _rightBottomRadius.y);
            foreach (VertexData vertexData in _currentProcessingArc.GetVertexIter())
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


            _currentProcessingArc.Init(_bounds.Right - _rightTopRadius.x, _bounds.Top - _rightTopRadius.y, _rightTopRadius.x, _rightTopRadius.y, 0.0, Math.PI * 0.5);
            _currentProcessingArc.SetStartEndLimit(_bounds.Right, _bounds.Top - _rightTopRadius.y,
                _bounds.Right - _rightTopRadius.x, _bounds.Top);
            foreach (VertexData vertexData in _currentProcessingArc.GetVertexIter())
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


            _currentProcessingArc.Init(_bounds.Left + _leftTopRadius.x, _bounds.Top - _leftTopRadius.y, _leftTopRadius.x, _leftTopRadius.y, Math.PI * 0.5, Math.PI);
            _currentProcessingArc.SetStartEndLimit(_bounds.Left - _leftTopRadius.x, _bounds.Top,
                  _bounds.Left, _bounds.Top - _leftTopRadius.y);
            foreach (VertexData vertexData in _currentProcessingArc.GetVertexIter())
            {
                switch (vertexData.command)
                {
                    case VertexCmd.MoveTo:
                        continue;
                    case VertexCmd.NoMore:
                        goto EXIT_LOOP;
                    default:
                        yield return vertexData;
                        break;
                }
            }

            EXIT_LOOP:

            yield return new VertexData(VertexCmd.Close, (int)EndVertexOrientation.CCW, 0);
            yield return new VertexData(VertexCmd.NoMore);
        }

        public VertexStore MakeVxs(VertexStore vxs)
        {
            return VertexStoreBuilder.CreateVxs(this.GetVertexIter(), vxs);
        }

    }
}

