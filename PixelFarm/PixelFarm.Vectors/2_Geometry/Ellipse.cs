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
        double _scale = 1;
        int _numSteps;
        public bool _cw;
        public Ellipse()
        {
            Set(0, 0, 1, 1, 4, false);
        }
        public Ellipse(double originX, double originY, double radiusX, double radiusY, int num_steps = 0, bool cw = false)
        {
            Set(originX, originY, radiusX, radiusY, num_steps, cw);
        }

        public void Set(double ox, double oy,
                 double rx, double ry,
                 int num_steps = 0, bool cw = false)
        {
            originX = ox;
            originY = oy;
            radiusX = rx;
            radiusY = ry;
            _cw = cw;
            _scale = 1;
            _numSteps = num_steps;
            if (_numSteps == 0)
            {
                CalculateNumSteps();
            }
        }
        public void SetFromLTWH(double left, double top, double width, double height, int num_steps = 0, bool cw = false)
        {
            double x = (left + width / 2);
            double y = (top + height / 2);
            double rx = Math.Abs(width / 2);
            double ry = Math.Abs(height / 2);
            Set(x, y, rx, ry, num_steps, cw);
        }
        public double ApproximateScale
        {
            get => _scale;
            set
            {
                _scale = value;
                CalculateNumSteps();
            }
        }

        public int NumSteps => _numSteps;

        void CalculateNumSteps()
        {
            double ra = (Math.Abs(radiusX) + Math.Abs(radiusY)) / 2;
            double da = Math.Acos(ra / (ra + 0.125 / _scale)) * 2;
            _numSteps = (int)Math.Round(2 * Math.PI / da);
        }


    }
}