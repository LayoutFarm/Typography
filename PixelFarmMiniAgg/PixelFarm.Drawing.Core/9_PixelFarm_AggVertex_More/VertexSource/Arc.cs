//BSD, 2014-2017, WinterDev
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
// Arc vertex generator
//
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
namespace PixelFarm.Agg.VertexSource
{
    //=====================================================================arc
    //
    // See Implementation agg_arc.cpp 
    //
    public class Arc
    {
        double originX;
        double originY;
        double radiusX;
        double radiusY;
        double startAngle;
        double endAngle;
        double m_Scale;
        ArcDirection m_Direction;
        double flatenDeltaAngle;
        bool m_IsInitialized;
        //------------        
        double startX;
        double startY;
        double endX;
        double endY;
        int calculateNSteps;
        public enum ArcDirection
        {
            ClockWise,
            CounterClockWise,
        }

        public Arc()
        {
            m_Scale = 1.0;
            m_IsInitialized = false;
        }


        public Arc(double ox, double oy,
             double rx, double ry,
             double angle1, double angle2,
             ArcDirection direction)
        {
            this.originX = ox;
            this.originY = oy;
            this.radiusX = rx;
            this.radiusY = ry;
            this.m_Scale = 1.0;
            Normalize(angle1, angle2, direction);
        }

        public void Init(double ox, double oy,
                  double rx, double ry,
                  double angle1, double angle2)
        {
            Init(ox, oy, rx, ry, angle1, angle2, ArcDirection.CounterClockWise);
        }


        public void Init(double ox, double oy,
                   double rx, double ry,
                   double angle1, double angle2,
                   ArcDirection direction)
        {
            this.originX = ox;
            this.originY = oy;
            this.radiusX = rx;
            this.radiusY = ry;
            Normalize(angle1, angle2, direction);
        }

        public void SetStartEndLimit(double startX, double startY, double endX, double endY)
        {
            this.startX = startX;
            this.startY = startY;
            this.endX = endX;
            this.endY = endY;
        }
        public bool UseStartEndLimit
        {
            get;
            set;
        }
        public double ApproximateScale
        {
            get { return this.m_Scale; }
            set
            {
                m_Scale = value;
                if (m_IsInitialized)
                {
                    Normalize(startAngle, endAngle, m_Direction);
                }
            }
        }

        public IEnumerable<VertexData> GetVertexIter()
        {
            // go to the start
            if (UseStartEndLimit)
            {
                //---------------------------------------------------------
                VertexData vertexData = new VertexData();
                vertexData.command = VertexCmd.MoveTo;
                vertexData.x = startX;
                vertexData.y = startY;
                yield return vertexData;
                //---------------------------------------------------------
                double angle = startAngle;
                vertexData.command = VertexCmd.LineTo;
                //calculate nsteps
                int n = 0;
                while (n < calculateNSteps - 1)
                {
                    angle += flatenDeltaAngle;
                    vertexData.x = originX + Math.Cos(angle) * radiusX;
                    vertexData.y = originY + Math.Sin(angle) * radiusY;
                    yield return vertexData;
                    n++;
                }

                //while ((angle < endAngle - flatenDeltaAngle / 4) == (((int)ArcDirection.CounterClockWise) == 1))
                //{
                //    angle += flatenDeltaAngle;
                //    vertexData.x = originX + Math.Cos(angle) * radiusX;
                //    vertexData.y = originY + Math.Sin(angle) * radiusY;

                //    yield return vertexData;
                //}
                //---------------------------------------------------------
                vertexData.x = endX;
                vertexData.y = endY;
                yield return vertexData;
                vertexData.command = VertexCmd.NoMore;
                yield return vertexData;
            }
            else
            {
                VertexData vertexData = new VertexData();
                vertexData.command = VertexCmd.MoveTo;
                vertexData.x = originX + Math.Cos(startAngle) * radiusX;
                vertexData.y = originY + Math.Sin(startAngle) * radiusY;
                yield return vertexData;
                //---------------------------------------------------------
                double angle = startAngle;
                vertexData.command = VertexCmd.LineTo;
                while ((angle < endAngle - flatenDeltaAngle / 4) == (((int)ArcDirection.CounterClockWise) == 1))
                {
                    angle += flatenDeltaAngle;
                    vertexData.x = originX + Math.Cos(angle) * radiusX;
                    vertexData.y = originY + Math.Sin(angle) * radiusY;
                    yield return vertexData;
                }
                //---------------------------------------------------------
                vertexData.x = originX + Math.Cos(endAngle) * radiusX;
                vertexData.y = originY + Math.Sin(endAngle) * radiusY;
                yield return vertexData;
                vertexData.command = VertexCmd.NoMore;
                yield return vertexData;
            }
        }

        void Normalize(double angle1, double angle2, ArcDirection direction)
        {
            double ra = (Math.Abs(radiusX) + Math.Abs(radiusY)) / 2;
            flatenDeltaAngle = Math.Acos(ra / (ra + 0.125 / m_Scale)) * 2;
            if (direction == ArcDirection.CounterClockWise)
            {
                while (angle2 < angle1)
                {
                    angle2 += Math.PI * 2.0;
                }
            }
            else
            {
                while (angle1 < angle2)
                {
                    angle1 += Math.PI * 2.0;
                }
                flatenDeltaAngle = -flatenDeltaAngle;
            }
            m_Direction = direction;
            startAngle = angle1;
            endAngle = angle2;
            m_IsInitialized = true;
            calculateNSteps = (int)Math.Floor(((endAngle - startAngle) / flatenDeltaAngle));
        }
    }
}
