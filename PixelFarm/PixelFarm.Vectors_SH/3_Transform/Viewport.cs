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
// Viewport transformer - simple orthogonal conversions from world coordinates
//                        to screen (device) ones.
//
//----------------------------------------------------------------------------

using System;
namespace PixelFarm.CpuBlit.VertexProcessing
{
    //----------------------------------------------------------trans_viewport

    public sealed class Viewport
    {
        double _world_x1;
        double _world_y1;
        double _world_x2;
        double _world_y2;
        double _device_x1;
        double _device_y1;
        double _device_x2;
        double _device_y2;
        AspectRatio _aspect;
        bool _is_valid;
        double _align_x;
        double _align_y;
        double _wx1;
        double _wy1;
        double _wx2;
        double _wy2;
        double _dx1;
        double _dy1;
        double _kx;
        double _ky;
        public enum AspectRatio
        {
            //aspect_ratio_e 
            Stretch,
            Meet,
            Slice
        }
        //-------------------------------------------------------------------
        public Viewport()
        {
            _world_x1 = (0.0);
            _world_y1 = (0.0);
            _world_x2 = (1.0);
            _world_y2 = (1.0);
            _device_x1 = (0.0);
            _device_y1 = (0.0);
            _device_x2 = (1.0);
            _device_y2 = (1.0);
            _aspect = AspectRatio.Stretch;
            _is_valid = (true);
            _align_x = (0.5);
            _align_y = (0.5);
            _wx1 = (0.0);
            _wy1 = (0.0);
            _wx2 = (1.0);
            _wy2 = (1.0);
            _dx1 = (0.0);
            _dy1 = (0.0);
            _kx = (1.0);
            _ky = (1.0);
        }

        //-------------------------------------------------------------------
        public void preserve_aspect_ratio(double alignx,
                                   double aligny,
                                   AspectRatio aspect)
        {
            _align_x = alignx;
            _align_y = aligny;
            _aspect = aspect;
            update();
        }

        //-------------------------------------------------------------------
        public void device_viewport(double x1, double y1, double x2, double y2)
        {
            _device_x1 = x1;
            _device_y1 = y1;
            _device_x2 = x2;
            _device_y2 = y2;
            update();
        }

        //-------------------------------------------------------------------
        public void world_viewport(double x1, double y1, double x2, double y2)
        {
            _world_x1 = x1;
            _world_y1 = y1;
            _world_x2 = x2;
            _world_y2 = y2;
            update();
        }

        //-------------------------------------------------------------------
        public void device_viewport(out double x1, out double y1, out double x2, out double y2)
        {
            x1 = _device_x1;
            y1 = _device_y1;
            x2 = _device_x2;
            y2 = _device_y2;
        }

        //-------------------------------------------------------------------
        public void world_viewport(out double x1, out double y1, out double x2, out double y2)
        {
            x1 = _world_x1;
            y1 = _world_y1;
            x2 = _world_x2;
            y2 = _world_y2;
        }

        //-------------------------------------------------------------------
        public void world_viewport_actual(out double x1, out double y1,
                                   out double x2, out double y2)
        {
            x1 = _wx1;
            y1 = _wy1;
            x2 = _wx2;
            y2 = _wy2;
        }

        //-------------------------------------------------------------------
        public bool is_valid() => _is_valid;
        public double align_x() => _align_x;
        public double align_y() => _align_y;
        public AspectRatio aspect_ratio() => _aspect;

        //-------------------------------------------------------------------
        public void transform(ref double x, ref double y)
        {
            x = (x - _wx1) * _kx + _dx1;
            y = (y - _wy1) * _ky + _dy1;
        }

        //-------------------------------------------------------------------
        public void transform_scale_only(ref double x, ref double y)
        {
            x *= _kx;
            y *= _ky;
        }

        //-------------------------------------------------------------------
        public void inverse_transform(ref double x, ref double y)
        {
            x = (x - _dx1) / _kx + _wx1;
            y = (y - _dy1) / _ky + _wy1;
        }

        //-------------------------------------------------------------------
        public void inverse_transform_scale_only(ref double x, ref double y)
        {
            x /= _kx;
            y /= _ky;
        }

        //-------------------------------------------------------------------
        public double device_dx() => _dx1 - _wx1 * _kx;
        public double device_dy() => _dy1 - _wy1 * _ky;

        //-------------------------------------------------------------------
        public double scale_x() => _kx;

        //-------------------------------------------------------------------
        public double scale_y() => _ky;

        //-------------------------------------------------------------------
        public double scale() => (_kx + _ky) * 0.5;

        //-------------------------------------------------------------------
        public Affine to_affine()
        {
            Affine mtx = Affine.NewTranslation(-_wx1, -_wy1);
            mtx *= Affine.NewScaling(_kx, _ky);
            mtx *= Affine.NewTranslation(_dx1, _dy1);
            return mtx;
        }

        //-------------------------------------------------------------------
        public Affine to_affine_scale_only() => Affine.NewScaling(_kx, _ky);


        void update()
        {
            double epsilon = 1e-30;
            if (Math.Abs(_world_x1 - _world_x2) < epsilon ||
               Math.Abs(_world_y1 - _world_y2) < epsilon ||
               Math.Abs(_device_x1 - _device_x2) < epsilon ||
               Math.Abs(_device_y1 - _device_y2) < epsilon)
            {
                _wx1 = _world_x1;
                _wy1 = _world_y1;
                _wx2 = _world_x1 + 1.0;
                _wy2 = _world_y2 + 1.0;
                _dx1 = _device_x1;
                _dy1 = _device_y1;
                _kx = 1.0;
                _ky = 1.0;
                _is_valid = false;
                return;
            }

            double world_x1 = _world_x1;
            double world_y1 = _world_y1;
            double world_x2 = _world_x2;
            double world_y2 = _world_y2;
            double device_x1 = _device_x1;
            double device_y1 = _device_y1;
            double device_x2 = _device_x2;
            double device_y2 = _device_y2;
            if (_aspect != AspectRatio.Stretch)
            {
                double d;
                _kx = (device_x2 - device_x1) / (world_x2 - world_x1);
                _ky = (device_y2 - device_y1) / (world_y2 - world_y1);
                if ((_aspect == AspectRatio.Meet) == (_kx < _ky))
                {
                    d = (world_y2 - world_y1) * _ky / _kx;
                    world_y1 += (world_y2 - world_y1 - d) * _align_y;
                    world_y2 = world_y1 + d;
                }
                else
                {
                    d = (world_x2 - world_x1) * _kx / _ky;
                    world_x1 += (world_x2 - world_x1 - d) * _align_x;
                    world_x2 = world_x1 + d;
                }
            }
            _wx1 = world_x1;
            _wy1 = world_y1;
            _wx2 = world_x2;
            _wy2 = world_y2;
            _dx1 = device_x1;
            _dy1 = device_y1;
            _kx = (device_x2 - device_x1) / (world_x2 - world_x1);
            _ky = (device_y2 - device_y1) / (world_y2 - world_y1);
            _is_valid = true;
        }
    };
}
