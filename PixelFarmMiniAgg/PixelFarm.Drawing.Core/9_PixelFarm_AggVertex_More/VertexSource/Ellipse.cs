//BSD, 2014-2016, WinterDev
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
// class ellipse
//
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using PixelFarm.VectorMath;
namespace PixelFarm.Agg.VertexSource
{
    public class Ellipse
    {
        public double originX;
        public double originY;
        public double radiusX;
        public double radiusY;
        double m_scale = 1;
        int numSteps;
        bool m_cw;
        public Ellipse()
        {
            Set(0, 0, 1, 1, 4, false);
        }
        public Ellipse(double originX, double originY, double radiusX, double radiusY, int num_steps = 0, bool cw = false)
        {
            Set(originX, originY, radiusX, radiusY, num_steps, cw);
        }
        public void Reset(double originX, double originY, double radiusX, double radiusY, int num_steps = 0)
        {
            Set(originX, originY, radiusX, radiusY, num_steps, false);
        }
        public void Set(double ox, double oy,
                 double rx, double ry,
                 int num_steps = 0, bool cw = false)
        {
            originX = ox;
            originY = oy;
            radiusX = rx;
            radiusY = ry;
            m_cw = cw;
            numSteps = num_steps;
            if (numSteps == 0)
            {
                CalculateNumSteps();
            }
        }
        public double ApproximateScale
        {
            get { return this.m_scale; }
            set
            {
                this.m_scale = value;
                CalculateNumSteps();
            }
        }

        IEnumerable<VertexData> GetVertexIter()
        {
            VertexData vertexData = new VertexData();
            vertexData.command = VertexCmd.MoveTo;
            vertexData.x = originX + radiusX;
            vertexData.y = originY;
            yield return vertexData;
            double anglePerStep = MathHelper.Tau / (double)numSteps;
            double angle = 0;
            vertexData.command = VertexCmd.LineTo;
            if (m_cw)
            {
                for (int i = 1; i < numSteps; i++)
                {
                    angle += anglePerStep;
                    vertexData.x = originX + Math.Cos(MathHelper.Tau - angle) * radiusX;
                    vertexData.y = originY + Math.Sin(MathHelper.Tau - angle) * radiusY;
                    yield return vertexData;
                }
            }
            else
            {
                for (int i = 1; i < numSteps; i++)
                {
                    angle += anglePerStep;
                    vertexData.x = originX + Math.Cos(angle) * radiusX;
                    vertexData.y = originY + Math.Sin(angle) * radiusY;
                    yield return vertexData;
                }
            }
            vertexData.x = (int)EndVertexOrientation.CCW;
            vertexData.y = 0;
            vertexData.command = VertexCmd.CloseAndEndFigure;
            yield return vertexData;
            vertexData.command = VertexCmd.Stop;
            yield return vertexData;
        }

        void CalculateNumSteps()
        {
            double ra = (Math.Abs(radiusX) + Math.Abs(radiusY)) / 2;
            double da = Math.Acos(ra / (ra + 0.125 / m_scale)) * 2;
            numSteps = (int)Math.Round(2 * Math.PI / da);
        }

        //-------------------------------------------------------
        public VertexStoreSnap MakeVertexSnap(VertexStore vxs)
        {
            return new VertexStoreSnap(MakeVxs(vxs));
        }
        public VertexStore MakeVxs(VertexStore vxs)
        {
            //TODO: review here
            return VertexStoreBuilder.CreateVxs(this.GetVertexIter(), vxs);
        }
        //-------------------------------------------------------
    }
}