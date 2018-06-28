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
// class ellipse
//
//----------------------------------------------------------------------------

using System; 
namespace PixelFarm.CpuBlit.VertexProcessing
{
    public class Ellipse
    {
        public double originX;
        public double originY;
        public double radiusX;
        public double radiusY;
        double m_scale = 1;
        int numSteps;
        public bool m_cw;
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
            m_scale = 1;
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

        public int NumSteps { get { return this.numSteps; } }

        void CalculateNumSteps()
        {
            double ra = (Math.Abs(radiusX) + Math.Abs(radiusY)) / 2;
            double da = Math.Acos(ra / (ra + 0.125 / m_scale)) * 2;
            numSteps = (int)Math.Round(2 * Math.PI / da);
        }


    }
}