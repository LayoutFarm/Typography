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
// Arc vertex generator
//
//----------------------------------------------------------------------------

using System;
namespace PixelFarm.CpuBlit.VertexProcessing
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
        double flattenDeltaAngle;
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
        //
        public double StartX { get { return this.startX; } }
        public double StartY { get { return this.startY; } }
        //
        public double EndX { get { return this.endX; } }
        public double EndY { get { return this.endY; } }
        //
        public double OriginX { get { return this.originX; } }
        public double OriginY { get { return this.originY; } }
        //
        public double StartAngle { get { return this.startAngle; } }
        public double EndAngle { get { return this.endAngle; } }
        //
        public double RadiusX { get { return this.radiusX; } }
        public double RadiusY { get { return this.radiusY; } }
        //
        public int CalculateNSteps { get { return this.calculateNSteps; } }
        public double FlattenDeltaAngle { get { return this.flattenDeltaAngle; } }
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

        void Normalize(double angle1, double angle2, ArcDirection direction)
        {
            double ra = (Math.Abs(radiusX) + Math.Abs(radiusY)) / 2;
            flattenDeltaAngle = Math.Acos(ra / (ra + 0.125 / m_Scale)) * 2;
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
                flattenDeltaAngle = -flattenDeltaAngle;
            }
            m_Direction = direction;
            startAngle = angle1;
            endAngle = angle2;
            m_IsInitialized = true;
            calculateNSteps = (int)Math.Floor(((endAngle - startAngle) / flattenDeltaAngle));
        }
    }
}
