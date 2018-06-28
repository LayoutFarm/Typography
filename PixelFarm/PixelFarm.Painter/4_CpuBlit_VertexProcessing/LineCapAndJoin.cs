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
// Stroke math
//
//----------------------------------------------------------------------------

namespace PixelFarm.CpuBlit.VertexProcessing
{
    public enum LineCap
    {
        Butt,
        Square,
        Round
    }

    public enum LineJoin
    {
        Miter,
        MiterRevert,
        Round,
        Bevel,
        MiterRound

        //TODO: implement svg arg join
    }

    public enum InnerJoin
    {
        Bevel,
        Miter,
        Jag,
        Round
    }
}