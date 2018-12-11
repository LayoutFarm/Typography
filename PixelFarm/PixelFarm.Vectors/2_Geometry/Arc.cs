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
        double _originX;
        double _originY;
        double _radiusX;
        double _radiusY;
        double _startAngle;
        double _endAngle;
        double _scale;
        ArcDirection _Direction;
        double _flattenDeltaAngle;
        bool _IsInitialized;
        //------------        
        double _startX;
        double _startY;
        double _endX;
        double _endY;
        int _calculateNSteps;
        public enum ArcDirection
        {
            ClockWise,
            CounterClockWise,
        }

        public Arc()
        {
            _scale = 1.0;
            _IsInitialized = false;
        }


        public Arc(double ox, double oy,
             double rx, double ry,
             double angle1, double angle2,
             ArcDirection direction)
        {
            _originX = ox;
            _originY = oy;
            _radiusX = rx;
            _radiusY = ry;
            _scale = 1.0;
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
            _originX = ox;
            _originY = oy;
            _radiusX = rx;
            _radiusY = ry;
            Normalize(angle1, angle2, direction);
        }

        public void SetStartEndLimit(double startX, double startY, double endX, double endY)
        {
            _startX = startX;
            _startY = startY;
            _endX = endX;
            _endY = endY;
        }
        //
        public double StartX => _startX;
        public double StartY => _startY;
        //
        public double EndX => _endX;
        public double EndY => _endY;
        //
        public double OriginX => _originX;
        public double OriginY => _originY;
        //
        public double StartAngle => _startAngle;
        public double EndAngle => _endAngle;
        //
        public double RadiusX => _radiusX;
        public double RadiusY => _radiusY;
        //
        public int CalculateNSteps => _calculateNSteps;
        public double FlattenDeltaAngle => _flattenDeltaAngle;
        public bool UseStartEndLimit { get; set; }

        public double ApproximateScale
        {
            get => _scale;
            set
            {
                _scale = value;
                if (_IsInitialized)
                {
                    Normalize(_startAngle, _endAngle, _Direction);
                }
            }
        }

        void Normalize(double angle1, double angle2, ArcDirection direction)
        {
            double ra = (Math.Abs(_radiusX) + Math.Abs(_radiusY)) / 2;
            _flattenDeltaAngle = Math.Acos(ra / (ra + 0.125 / _scale)) * 2;
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
                _flattenDeltaAngle = -_flattenDeltaAngle;
            }
            _Direction = direction;
            _startAngle = angle1;
            _endAngle = angle2;
            _IsInitialized = true;
            _calculateNSteps = (int)Math.Floor(((_endAngle - _startAngle) / _flattenDeltaAngle));
        }
    }
}
