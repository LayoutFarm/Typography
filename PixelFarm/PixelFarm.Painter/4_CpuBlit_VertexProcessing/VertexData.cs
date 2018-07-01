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


using PixelFarm.VectorMath;
namespace PixelFarm.CpuBlit
{
    public struct VertexData
    {
        public VertexCmd command;
        public double x;
        public double y;
        public VertexData(VertexCmd command)
        {
            this.command = command;
            x = y = 0;
        }
        public VertexData(VertexCmd command, Vector2 position)
        {
            this.command = command;
            this.x = position.x;
            this.y = position.y;
        }
        public VertexData(VertexCmd command, double x, double y)
        {
            this.command = command;
            this.x = x;
            this.y = y;
        }

        public Vector2 position
        {
            get { return new Vector2(this.x, this.y); }
            set { this.x = value.x; this.y = value.y; }
        }


#if DEBUG
        public override string ToString()
        {
            return command + " " + this.x + "," + this.y;
        }
#endif
    }
}
