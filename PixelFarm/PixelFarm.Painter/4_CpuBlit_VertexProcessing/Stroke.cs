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
// conv_stroke
//
//----------------------------------------------------------------------------
using PixelFarm.Drawing;
namespace PixelFarm.CpuBlit.VertexProcessing
{
    public sealed class Stroke
    {
        StrokeGenerator _strokeGen;
        public Stroke(double initWidth)
        {
            _strokeGen = new StrokeGenerator();
            this.Width = initWidth;
        }

        public LineCap LineCap
        {
            get => _strokeGen.LineCap;
            set => _strokeGen.LineCap = value;
        }
        public LineJoin LineJoin
        {
            get => _strokeGen.LineJoin;
            set => _strokeGen.LineJoin = value;
        }
        public InnerJoin InnerJoin
        {
            get => _strokeGen.InnerJoin;
            set => _strokeGen.InnerJoin = value;
        }
        public double MiterLimit
        {
            get => _strokeGen.MiterLimit;
            set => _strokeGen.MiterLimit = value;
        }
        public double InnerMiterLimit
        {
            get => _strokeGen.InnerMiterLimit;
            set => _strokeGen.InnerMiterLimit = value;
        }
        public double Width
        {
            get => _strokeGen.Width;
            set => _strokeGen.Width = value;
        }
        public void SetMiterLimitTheta(double t)
        {
            _strokeGen.SetMiterLimitTheta(t);
        }
        public double ApproximateScale
        {
            get => _strokeGen.ApproximateScale;
            set => _strokeGen.ApproximateScale = value;
        }
        public double Shorten
        {
            get => _strokeGen.Shorten;
            set => _strokeGen.Shorten = value;
        }
        public StrokeSideForClosedShape StrokeSideForClosedShape
        {
            get => _strokeGen.StrokeSideForClosedShape;
            set => _strokeGen.StrokeSideForClosedShape = value;
        }
        public StrokeSideForOpenShape StrokeSideForOpenShape
        {
            get => _strokeGen.StrokeSideForOpenShape;
            set => _strokeGen.StrokeSideForOpenShape = value;
        }
        public VertexStore MakeVxs(VertexStore sourceVxs, VertexStore outputVxs)
        {
            StrokeGenerator strkgen = _strokeGen;
            int j = sourceVxs.Count;
            strkgen.Reset();

            double x = 0, y = 0, startX = 0, startY = 0,
                  latest_x = 0, latest_y = 0;
            for (int i = 0; i < j; ++i)
            {
                VertexCmd cmd = sourceVxs.GetVertex(i, out x, out y);
                switch (cmd)
                {
                    case VertexCmd.NoMore:
                        goto EXIT_LOOP; //***
                    case VertexCmd.Close:
                        if (i < j)
                        {
                            //close command
                            strkgen.Close();
                            strkgen.WriteTo(outputVxs);
                            strkgen.Reset();
                            if (i < j - 1)
                            {
                                VertexCmd nextCmd = sourceVxs.GetCommand(i + 1);
                                if (nextCmd != VertexCmd.MoveTo)
                                {
                                    strkgen.AddVertex(startX, startY, VertexCmd.MoveTo);
                                }
                            }
                        }
                        else
                        {

                        }
                        break;
                    case VertexCmd.CloseAndEndFigure:
                        if (i < j)
                        {
                            //close command
                            strkgen.Close();
                            strkgen.WriteTo(outputVxs);
                            strkgen.Reset();
                        }
                        else
                        {

                        }
                        break;
                    case VertexCmd.LineTo:
                    case VertexCmd.C3://user must flatten the curve before do stroke
                    case VertexCmd.C4://user must flatten the curve before do stroke 
                        strkgen.AddVertex(latest_x = x, latest_y = y, cmd);
                        break;
                    case VertexCmd.MoveTo:
                        {
                            //for stroke,
                            if (strkgen.VertexCount > 0)
                            {
                                strkgen.Close();
                                strkgen.WriteTo(outputVxs);
                                strkgen.Reset();
                            }
                        }

                        strkgen.AddVertex(x, y, cmd);
                        latest_x = startX = x;
                        latest_y = startY = y;
                        break;
                    default: throw new System.NotSupportedException();
                }
            }
        //---
        EXIT_LOOP:
            //
            strkgen.WriteTo(outputVxs);
            strkgen.Reset();

            return outputVxs;
        }

        public VertexStore CreateTrim(VertexStore sourceVxs)
        {
            using (VxsTemp.Borrow(out var v1))
            {
                return MakeVxs(sourceVxs, v1).CreateTrim();
            }
        }
    }

}