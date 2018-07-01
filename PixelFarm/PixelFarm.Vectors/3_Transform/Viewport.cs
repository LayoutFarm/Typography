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
        double m_world_x1;
        double m_world_y1;
        double m_world_x2;
        double m_world_y2;
        double m_device_x1;
        double m_device_y1;
        double m_device_x2;
        double m_device_y2;
        AspectRatio m_aspect;
        bool m_is_valid;
        double m_align_x;
        double m_align_y;
        double m_wx1;
        double m_wy1;
        double m_wx2;
        double m_wy2;
        double m_dx1;
        double m_dy1;
        double m_kx;
        double m_ky;
        public enum AspectRatio
        {
            //aspect_ratio_e 
            Stretch,
            Meet,
            Slice
        };
        //-------------------------------------------------------------------
        public Viewport()
        {
            m_world_x1 = (0.0);
            m_world_y1 = (0.0);
            m_world_x2 = (1.0);
            m_world_y2 = (1.0);
            m_device_x1 = (0.0);
            m_device_y1 = (0.0);
            m_device_x2 = (1.0);
            m_device_y2 = (1.0);
            m_aspect = AspectRatio.Stretch;
            m_is_valid = (true);
            m_align_x = (0.5);
            m_align_y = (0.5);
            m_wx1 = (0.0);
            m_wy1 = (0.0);
            m_wx2 = (1.0);
            m_wy2 = (1.0);
            m_dx1 = (0.0);
            m_dy1 = (0.0);
            m_kx = (1.0);
            m_ky = (1.0);
        }

        //-------------------------------------------------------------------
        public void preserve_aspect_ratio(double alignx,
                                   double aligny,
                                   AspectRatio aspect)
        {
            m_align_x = alignx;
            m_align_y = aligny;
            m_aspect = aspect;
            update();
        }

        //-------------------------------------------------------------------
        public void device_viewport(double x1, double y1, double x2, double y2)
        {
            m_device_x1 = x1;
            m_device_y1 = y1;
            m_device_x2 = x2;
            m_device_y2 = y2;
            update();
        }

        //-------------------------------------------------------------------
        public void world_viewport(double x1, double y1, double x2, double y2)
        {
            m_world_x1 = x1;
            m_world_y1 = y1;
            m_world_x2 = x2;
            m_world_y2 = y2;
            update();
        }

        //-------------------------------------------------------------------
        public void device_viewport(out double x1, out double y1, out double x2, out double y2)
        {
            x1 = m_device_x1;
            y1 = m_device_y1;
            x2 = m_device_x2;
            y2 = m_device_y2;
        }

        //-------------------------------------------------------------------
        public void world_viewport(out double x1, out double y1, out double x2, out double y2)
        {
            x1 = m_world_x1;
            y1 = m_world_y1;
            x2 = m_world_x2;
            y2 = m_world_y2;
        }

        //-------------------------------------------------------------------
        public void world_viewport_actual(out double x1, out double y1,
                                   out double x2, out double y2)
        {
            x1 = m_wx1;
            y1 = m_wy1;
            x2 = m_wx2;
            y2 = m_wy2;
        }

        //-------------------------------------------------------------------
        public bool is_valid() { return m_is_valid; }
        public double align_x() { return m_align_x; }
        public double align_y() { return m_align_y; }
        public AspectRatio aspect_ratio() { return m_aspect; }

        //-------------------------------------------------------------------
        public void transform(ref double x, ref double y)
        {
            x = (x - m_wx1) * m_kx + m_dx1;
            y = (y - m_wy1) * m_ky + m_dy1;
        }

        //-------------------------------------------------------------------
        public void transform_scale_only(ref double x, ref double y)
        {
            x *= m_kx;
            y *= m_ky;
        }

        //-------------------------------------------------------------------
        public void inverse_transform(ref double x, ref double y)
        {
            x = (x - m_dx1) / m_kx + m_wx1;
            y = (y - m_dy1) / m_ky + m_wy1;
        }

        //-------------------------------------------------------------------
        public void inverse_transform_scale_only(ref double x, ref double y)
        {
            x /= m_kx;
            y /= m_ky;
        }

        //-------------------------------------------------------------------
        public double device_dx() { return m_dx1 - m_wx1 * m_kx; }
        public double device_dy() { return m_dy1 - m_wy1 * m_ky; }

        //-------------------------------------------------------------------
        public double scale_x()
        {
            return m_kx;
        }

        //-------------------------------------------------------------------
        public double scale_y()
        {
            return m_ky;
        }

        //-------------------------------------------------------------------
        public double scale()
        {
            return (m_kx + m_ky) * 0.5;
        }

        //-------------------------------------------------------------------
        public Affine to_affine()
        {
            Affine mtx = Affine.NewTranslation(-m_wx1, -m_wy1);
            mtx *= Affine.NewScaling(m_kx, m_ky);
            mtx *= Affine.NewTranslation(m_dx1, m_dy1);
            return mtx;
        }

        //-------------------------------------------------------------------
        public Affine to_affine_scale_only()
        {
            return Affine.NewScaling(m_kx, m_ky);
        }

        private void update()
        {
            double epsilon = 1e-30;
            if (Math.Abs(m_world_x1 - m_world_x2) < epsilon ||
               Math.Abs(m_world_y1 - m_world_y2) < epsilon ||
               Math.Abs(m_device_x1 - m_device_x2) < epsilon ||
               Math.Abs(m_device_y1 - m_device_y2) < epsilon)
            {
                m_wx1 = m_world_x1;
                m_wy1 = m_world_y1;
                m_wx2 = m_world_x1 + 1.0;
                m_wy2 = m_world_y2 + 1.0;
                m_dx1 = m_device_x1;
                m_dy1 = m_device_y1;
                m_kx = 1.0;
                m_ky = 1.0;
                m_is_valid = false;
                return;
            }

            double world_x1 = m_world_x1;
            double world_y1 = m_world_y1;
            double world_x2 = m_world_x2;
            double world_y2 = m_world_y2;
            double device_x1 = m_device_x1;
            double device_y1 = m_device_y1;
            double device_x2 = m_device_x2;
            double device_y2 = m_device_y2;
            if (m_aspect != AspectRatio.Stretch)
            {
                double d;
                m_kx = (device_x2 - device_x1) / (world_x2 - world_x1);
                m_ky = (device_y2 - device_y1) / (world_y2 - world_y1);
                if ((m_aspect == AspectRatio.Meet) == (m_kx < m_ky))
                {
                    d = (world_y2 - world_y1) * m_ky / m_kx;
                    world_y1 += (world_y2 - world_y1 - d) * m_align_y;
                    world_y2 = world_y1 + d;
                }
                else
                {
                    d = (world_x2 - world_x1) * m_kx / m_ky;
                    world_x1 += (world_x2 - world_x1 - d) * m_align_x;
                    world_x2 = world_x1 + d;
                }
            }
            m_wx1 = world_x1;
            m_wy1 = world_y1;
            m_wx2 = world_x2;
            m_wy2 = world_y2;
            m_dx1 = device_x1;
            m_dy1 = device_y1;
            m_kx = (device_x2 - device_x1) / (world_x2 - world_x1);
            m_ky = (device_y2 - device_y1) / (world_y2 - world_y1);
            m_is_valid = true;
        }
    };
}
