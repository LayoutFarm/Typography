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


namespace PixelFarm.CpuBlit.FragmentProcessing
{
    //================================================span_interpolator_linear
    struct SpanInterpolatorLinear : FragmentProcessing.ISpanInterpolator
    {
        LineInterpolatorDDA2S _li_x; //use 'struct' version
        LineInterpolatorDDA2S _li_y;
        const int SUB_PIXEL_SHIFT = 8;
        const int SUB_PIXEL_SCALE = 1 << SUB_PIXEL_SHIFT;

        public VertexProcessing.ICoordTransformer Transformer { get; set; }

        public void Begin(double x, double y, int len)
        {
            double tx = x;
            double ty = y;

            Transformer.Transform(ref tx, ref ty);
            int x1 = AggMath.iround(tx * SUB_PIXEL_SCALE);
            int y1 = AggMath.iround(ty * SUB_PIXEL_SCALE);
            //
            tx = x + len; //*** 
            ty = y;//**
            Transformer.Transform(ref tx, ref ty);
            int x2 = AggMath.iround(tx * SUB_PIXEL_SCALE);
            int y2 = AggMath.iround(ty * SUB_PIXEL_SCALE);
            //

            _li_x = new LineInterpolatorDDA2S(x1, x2, len);
            _li_y = new LineInterpolatorDDA2S(y1, y2, len);

        }
        public void Next()
        {
            _li_x.Next();
            _li_y.Next();
        }

        //----------------------------------------------------------------
        public void GetCoord(out int x, out int y)
        {
            x = _li_x.Y;
            y = _li_y.Y;
        }
    }
}