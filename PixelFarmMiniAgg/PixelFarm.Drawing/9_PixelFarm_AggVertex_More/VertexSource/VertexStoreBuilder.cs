//BSD, 2014-2016, WinterDev
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

using System.Collections.Generic;
namespace PixelFarm.Agg
{
    public static class VertexStoreBuilder
    {
        public static VertexStore CreateVxs(IEnumerable<VertexData> iter)
        {
            VertexStore vxs = new VertexStore(2);
            foreach (VertexData v in iter)
            {
                vxs.AddVertex(v.x, v.y, v.command);
            }
            return vxs;
        }
    }
}