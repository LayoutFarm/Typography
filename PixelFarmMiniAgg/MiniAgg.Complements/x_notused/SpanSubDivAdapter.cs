////MIT, 2014-2016, WinterDev 
////----------------------------------------------------------------------------
//// Anti-Grain Geometry - Version 2.4
//// Copyright (C) 2002-2005 Maxim Shemanarev (http://www.antigrain.com)
////
//// C# Port port by: Lars Brubaker
////                  larsbrubaker@gmail.com
//// Copyright (C) 2007
////
//// Permission to copy, use, modify, sell and distribute this software 
//// is granted provided this copyright notice appears in all copies. 
//// This software is provided "as is" without express or implied
//// warranty, and with no claim as to its suitability for any purpose.
////
////----------------------------------------------------------------------------
//// Contact: mcseem@antigrain.com
////          mcseemagg@yahoo.com
////          http://www.antigrain.com
////----------------------------------------------------------------------------
//using System;

//namespace PixelFarm.Agg
//{

//    //=================================================span_subdiv_adaptor
//    public class SpanSubDivAdapter : ISpanInterpolator
//    {
//        int m_subdiv_shift;
//        int m_subdiv_size;
//        int m_subdiv_mask;
//        ISpanInterpolator m_interpolator;
//        int m_src_x;
//        double m_src_y;
//        int m_pos;
//        int m_len;

//        const int SUBPIX_SHIFT = 8;
//        const int SUBPIX_SCALE = 1 << SUBPIX_SHIFT;


//        //----------------------------------------------------------------
//        public SpanSubDivAdapter(ISpanInterpolator interpolator)
//            : this(interpolator, 4)
//        {
//        }

//        public SpanSubDivAdapter(ISpanInterpolator interpolator, int subdiv_shift)
//        {
//            m_subdiv_shift = subdiv_shift;
//            m_subdiv_size = 1 << m_subdiv_shift;
//            m_subdiv_mask = m_subdiv_size - 1;
//            m_interpolator = interpolator;
//        }

//        public SpanSubDivAdapter(ISpanInterpolator interpolator,
//                             double x, double y, int len,
//                             int subdiv_shift)
//            : this(interpolator, subdiv_shift)
//        {
//            Begin(x, y, len);
//        }

//        public void Resync(double xe, double ye, int len)
//        {
//            throw new System.NotImplementedException();
//        }


//        public ISpanInterpolator Interpolator
//        {
//            get { return this.m_interpolator; }
//            set { this.m_interpolator = value; }
//        }
//        //----------------------------------------------------------------
//        public Transform.ICoordTransformer Transformer
//        {
//            get { return this.m_interpolator.Transformer; }
//            set { this.m_interpolator.Transformer = value; }
//        }


//        //----------------------------------------------------------------

//        public int SubDivShift
//        {
//            get { return this.m_subdiv_shift; }
//            set
//            {
//                m_subdiv_shift = value;
//                m_subdiv_size = 1 << m_subdiv_shift;
//                m_subdiv_mask = m_subdiv_size - 1;
//            }
//        }

//        //----------------------------------------------------------------
//        public void Begin(double x, double y, int len)
//        {
//            m_pos = 1;
//            m_src_x = AggBasics.iround(x * SUBPIX_SCALE) + SUBPIX_SCALE;
//            m_src_y = y;
//            m_len = len;
//            if (len > m_subdiv_size) len = (int)m_subdiv_size;
//            m_interpolator.Begin(x, y, len);
//        }

//        //----------------------------------------------------------------
//        public void Next()
//        {
//            m_interpolator.Next();
//            if (m_pos >= m_subdiv_size)
//            {
//                int len = m_len;
//                if (len > m_subdiv_size) len = (int)m_subdiv_size;
//                m_interpolator.Resync((double)m_src_x / (double)SUBPIX_SCALE + len,
//                                              m_src_y,
//                                              len);
//                m_pos = 0;
//            }
//            m_src_x += SUBPIX_SCALE;
//            ++m_pos;
//            --m_len;
//        }

//        //----------------------------------------------------------------
//        public void GetCoord(out int x, out int y)
//        {
//            m_interpolator.GetCoord(out x, out y);
//        }

//        //----------------------------------------------------------------
//        public void GetLocalScale(out int x, out int y)
//        {
//            m_interpolator.GetLocalScale(out x, out y);
//        }
//    }
//}