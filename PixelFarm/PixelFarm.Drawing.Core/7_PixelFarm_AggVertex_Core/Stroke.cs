//BSD, 2014-2017, WinterDev
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
// conv_stroke
//
//----------------------------------------------------------------------------

namespace PixelFarm.Agg
{
    public sealed class Stroke
    {
        StrokeGenerator _strokeGen;
        public Stroke(double inWidth)
        {
            this._strokeGen = new StrokeGenerator();
            this.Width = inWidth;
        }

        public LineCap LineCap
        {
            get { return _strokeGen.LineCap; }
            set { _strokeGen.LineCap = value; }
        }
        public LineJoin LineJoin
        {
            get { return _strokeGen.LineJoin; }
            set { _strokeGen.LineJoin = value; }
        }
        public InnerJoin InnerJoin
        {
            get { return _strokeGen.InnerJoin; }
            set { _strokeGen.InnerJoin = value; }
        }
        public double MiterLimit
        {
            get { return _strokeGen.MiterLimit; }
            set { _strokeGen.MiterLimit = value; }
        }
        public double InnerMiterLimit
        {
            get { return _strokeGen.InnerMiterLimit; }
            set { _strokeGen.InnerMiterLimit = value; }
        }
        public double Width
        {
            get { return _strokeGen.Width; }
            set { _strokeGen.Width = value; }
        }

        public void SetMiterLimitTheta(double t)
        {
            _strokeGen.SetMiterLimitTheta(t);
        }
        public double ApproximateScale
        {
            get { return _strokeGen.ApproximateScale; }
            set { _strokeGen.ApproximateScale = value; }
        }
        public double Shorten
        {
            get { return _strokeGen.Shorten; }
            set { _strokeGen.Shorten = value; }
        }
        public VertexStore MakeVxs(VertexStore sourceVxs, VertexStore vxs)
        {
            StrokeGenerator strkgen = _strokeGen;
            int j = sourceVxs.Count;
            strkgen.Reset();
            //
            //
            VertexCmd cmd;
            double x = 0, y = 0, startX = 0, startY = 0;

            for (int i = 0; i < j; ++i)
            {
                cmd = sourceVxs.GetVertex(i, out x, out y);
                switch (cmd)
                {
                    case VertexCmd.NoMore:
                        break;

                    case VertexCmd.Close:
                    case VertexCmd.CloseAndEndFigure:

                        strkgen.AddVertex(x, y, cmd);
                        if (i < j - 2)
                        {
                            strkgen.AddVertex(startX, startY, VertexCmd.LineTo);
                            strkgen.WriteTo(vxs);
                            strkgen.Reset();
                        }
                        //end this polygon 

                        break;
                    case VertexCmd.LineTo:
                    case VertexCmd.P2c://user must flatten the curve before do stroke
                    case VertexCmd.P3c://user must flatten the curve before do stroke

                        strkgen.AddVertex(x, y, cmd);

                        break;
                    case VertexCmd.MoveTo:

                        strkgen.AddVertex(x, y, cmd);
                        startX = x;
                        startY = y;

                        break;
                    default: throw new System.NotSupportedException();
                }
            }
            strkgen.WriteTo(vxs);
            strkgen.Reset();

            return vxs;
        }
    }

}