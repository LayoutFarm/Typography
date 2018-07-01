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
// Rounded rectangle vertex generator
//
//----------------------------------------------------------------------------
using PixelFarm.Drawing;
namespace PixelFarm.CpuBlit.VertexProcessing
{

    public class SimpleRect
    {
        RectD bounds;
        //TODO: review here again
        PathWriter _reusablePathWriter = new PathWriter();

        public SimpleRect()
        {
        }
        public SimpleRect(double left, double bottom, double right, double top)
        {
            bounds = new RectD(left, bottom, right, top);
            if (left > right)
            {
                bounds.Left = right;
                bounds.Right = left;
            }

            if (bottom > top)
            {
                bounds.Bottom = top;
                bounds.Top = bottom;
            }
        }
        public void SetRect(double left, double bottom, double right, double top)
        {
            bounds = new RectD(left, bottom, right, top);
            if (left > right) { bounds.Left = right; bounds.Right = left; }
            if (bottom > top) { bounds.Bottom = top; bounds.Top = bottom; }
        }
        public void Offset(double dx, double dy)
        {
            bounds.Offset(dx, dy);
        }
        public double Height
        {
            get { return bounds.Height; }
        }
        public double Width
        {
            get { return bounds.Width; }
        }
        public VertexStore MakeVxs(VertexStore output)
        {

            _reusablePathWriter.ResetWithExternalVxs(output);
            _reusablePathWriter.MoveTo(bounds.Left, bounds.Bottom);
            _reusablePathWriter.LineTo(bounds.Right, bounds.Bottom);
            _reusablePathWriter.LineTo(bounds.Right, bounds.Top);
            _reusablePathWriter.LineTo(bounds.Left, bounds.Top);
            _reusablePathWriter.CloseFigure();
            return output;
        }
        public VertexStoreSnap MakeVertexSnap(VertexStore output)
        {
            return new VertexStoreSnap(this.MakeVxs(output));
        }
    }
}

