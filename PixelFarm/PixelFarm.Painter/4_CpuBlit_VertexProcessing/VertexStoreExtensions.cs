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
// Arc vertex generator
//
//----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using PixelFarm.Drawing;
using PixelFarm.VectorMath;

namespace PixelFarm.CpuBlit.VertexProcessing
{

    public static class VertexSourceExtensions
    {
        public static VertexStoreSnap MakeVertexSnap(this Ellipse ellipse, VertexStore output)
        {
            return new VertexStoreSnap(MakeVxs(ellipse, output));
        }
        public static VertexStore MakeVxs(this Ellipse ellipse, VertexStore output)
        {
            //TODO: review here
            return VertexStoreBuilder.CreateVxs(GetVertexIter(ellipse), output);
        }
        public static IEnumerable<VertexData> GetVertexIter(this Ellipse ellipse)
        {
            //TODO: review here again
            VertexData vertexData = new VertexData();
            vertexData.command = VertexCmd.MoveTo;
            vertexData.x = ellipse.originX + ellipse.radiusX;
            vertexData.y = ellipse.originY;
            yield return vertexData; // move to cmd
            //
            int numSteps = ellipse.NumSteps;
            double anglePerStep = MathHelper.Tau / numSteps;
            double angle = 0;
            vertexData.command = VertexCmd.LineTo;

            double orgX = ellipse.originX;
            double orgY = ellipse.originY;
            double radX = ellipse.radiusX;
            double radY = ellipse.radiusY;
            if (ellipse.m_cw)
            {
                for (int i = 1; i < numSteps; i++)
                {
                    angle += anglePerStep;
                    vertexData.x = orgX + Math.Cos(MathHelper.Tau - angle) * radX;
                    vertexData.y = orgY + Math.Sin(MathHelper.Tau - angle) * radY;
                    yield return vertexData;
                }
            }
            else
            {
                for (int i = 1; i < numSteps; i++)
                {
                    angle += anglePerStep;
                    vertexData.x = orgX + Math.Cos(angle) * radX;
                    vertexData.y = orgY + Math.Sin(angle) * radY;
                    yield return vertexData;
                }
            }
            vertexData.x = (int)EndVertexOrientation.CCW;
            vertexData.y = 0;
            vertexData.command = VertexCmd.Close;
            yield return vertexData;
            vertexData.command = VertexCmd.NoMore;
            yield return vertexData;
        }


        static int NSteps = 20;
        public static void CreateBezierVxs3(VertexStore vxs,
            double x0, double y0,
            double x1, double y1,
            double x2, double y2)
        {

            //1. subdiv technique
            s_curve3Div.Init(x0, y0, x1, y1, x2, y2);


            ArrayList<Vector2> points = s_curve3Div.GetInternalPoints();
            int n = 0;
            for (int i = points.Length - 1; i >= 0; --i)
            {
                Vector2 p = points[n++];
                vxs.AddLineTo(p.x, p.y);
            }


            //2. old tech --  use incremental
            //var curve = new VectorMath.BezierCurveQuadric(
            //    new Vector2(x0, y0),
            //    new Vector2(x1, y1),
            //    new Vector2(x2, y2));

            //vxs.AddLineTo(x0, y0);
            //float eachstep = (float)1 / NSteps;
            //float stepSum = eachstep;//start
            //for (int i = NSteps - 1; i >= 0; --i)
            //{
            //    var vector2 = curve.CalculatePoint(stepSum);
            //    vxs.AddLineTo(vector2.x, vector2.y);
            //    stepSum += eachstep;
            //}
            //vxs.AddLineTo(x2, y2);

        }

        static Curve4Div s_curve4Div = new Curve4Div();
        static Curve3Div s_curve3Div = new Curve3Div();

        static void CreateBezierVxs4(VertexStore vxs,
        double x0, double y0,
        double x1, double y1,
        double x2, double y2,
        double x3, double y3)
        {

            //1. subdiv technique

            s_curve4Div.Init(x0, y0, x1, y1, x2, y2, x3, y3);
            ArrayList<Vector2> points = s_curve4Div.GetInternalPoints();

            int n = 0;
            for (int i = points.Length - 1; i >= 0; --i)
            {
                Vector2 p = points[n++];
                vxs.AddLineTo(p.x, p.y);
            }


            //----------------------------------------
            //2. old tech --  use incremental
            //var curve = new VectorMath.BezierCurveCubic(
            //    start, end,
            //    control1, control2);
            //vxs.AddLineTo(start.x, start.y);
            //float eachstep = (float)1 / NSteps;
            //float stepSum = eachstep;//start
            //for (int i = NSteps - 1; i >= 0; --i)
            //{
            //    var vector2 = curve.CalculatePoint(stepSum);
            //    vxs.AddLineTo(vector2.x, vector2.y);
            //    stepSum += eachstep;
            //}
            //vxs.AddLineTo(end.x, end.y); 


        }
        /// <summary>
        /// create lines from curve
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="vxs"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="p2x"></param>
        /// <param name="p2y"></param>
        /// <param name="p3x"></param>
        /// <param name="p3y"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        public static void MakeLines(this Curve4 curve, VertexStore vxs, double x1, double y1,
            double p2x, double p2y,
            double p3x, double p3y,
            double x2, double y2)
        {
            CreateBezierVxs4(vxs,
                x1, y1,
                p2x, p2y,
                p3x, p3y,
                x2, y2);
        }
        /// <summary>
        /// create lines from curve
        /// </summary>
        /// <param name="vxs"></param>
        public static void MakeLines(this Curve3 curve, VertexStore vxs, double x1, double y1,
               double cx, double cy,
               double x2, double y2)
        {
            CreateBezierVxs3(vxs,
                x1, y1,
                cx, cy,
                x2, y2);
        }
        public static void MakeLines(this Curve3Div curve, VertexStore vxs)
        {
            ArrayList<Vector2> m_points = curve.GetInternalPoints();
            int j = m_points.Count;
            if (j > 0)
            {
                //others
                for (int i = 1; i < j; i++)
                {
                    var p = m_points[i];
                    vxs.AddLineTo(p.x, p.y);
                }
            }
        }
        public static IEnumerable<VertexData> GetVertexIter(this Curve3Div curve)
        {
            throw new NotImplementedException();
        }
        public static IEnumerable<VertexData> GetVertexIter(this Curve4Div curve)
        {
            ArrayList<Vector2> m_points = curve.GetInternalPoints();
            VertexData vertexData = new VertexData();
            vertexData.command = VertexCmd.MoveTo;
            vertexData.position = m_points[0];
            yield return vertexData;
            vertexData.command = VertexCmd.LineTo;
            for (int i = 1; i < m_points.Count; i++)
            {
                vertexData.position = m_points[i];
                yield return vertexData;
            }

            vertexData.command = VertexCmd.NoMore;
            vertexData.position = new Vector2();
            yield return vertexData;
        }
        public static IEnumerable<VertexData> GetVertexIter(this Arc arc)
        {
            // go to the start
            if (arc.UseStartEndLimit)
            {
                //---------------------------------------------------------
                VertexData vertexData = new VertexData();
                vertexData.command = VertexCmd.MoveTo;
                vertexData.x = arc.StartX;
                vertexData.y = arc.StartY;
                yield return vertexData;
                //---------------------------------------------------------
                double angle = arc.StartAngle;
                vertexData.command = VertexCmd.LineTo;
                //calculate nsteps
                int calculateNSteps = arc.CalculateNSteps;

                int n = 0;
                double radX = arc.RadiusX;
                double radY = arc.RadiusY;
                double flatternDeltaAngle = arc.FlattenDeltaAngle;
                double orgX = arc.OriginX;
                double orgY = arc.OriginY;

                while (n < calculateNSteps - 1)
                {
                    angle += flatternDeltaAngle;
                    vertexData.x = orgX + Math.Cos(angle) * radX;
                    vertexData.y = orgY + Math.Sin(angle) * radY;
                    yield return vertexData;
                    n++;
                }

                //while ((angle < endAngle - flatenDeltaAngle / 4) == (((int)ArcDirection.CounterClockWise) == 1))
                //{
                //    angle += flatenDeltaAngle;
                //    vertexData.x = originX + Math.Cos(angle) * radiusX;
                //    vertexData.y = originY + Math.Sin(angle) * radiusY;

                //    yield return vertexData;
                //}
                //---------------------------------------------------------
                vertexData.x = arc.EndX;
                vertexData.y = arc.EndY;
                yield return vertexData;
                vertexData.command = VertexCmd.NoMore;
                yield return vertexData;
            }
            else
            {
                double originX = arc.OriginX;
                double originY = arc.OriginY;
                double startAngle = arc.StartAngle;
                double radX = arc.RadiusX;
                double radY = arc.RadiusY;
                VertexData vertexData = new VertexData();
                vertexData.command = VertexCmd.MoveTo;
                vertexData.x = originX + Math.Cos(startAngle) * radX;
                vertexData.y = originY + Math.Sin(startAngle) * radY;
                yield return vertexData;
                //---------------------------------------------------------
                double angle = startAngle;
                double endAngle = arc.EndY;
                double flatternDeltaAngle = arc.FlattenDeltaAngle;
                vertexData.command = VertexCmd.LineTo;
                while ((angle < endAngle - flatternDeltaAngle / 4) == (((int)Arc.ArcDirection.CounterClockWise) == 1))
                {
                    angle += flatternDeltaAngle;
                    vertexData.x = originX + Math.Cos(angle) * radX;
                    vertexData.y = originY + Math.Sin(angle) * radY;
                    yield return vertexData;
                }
                //---------------------------------------------------------
                vertexData.x = originX + Math.Cos(endAngle) * radX;
                vertexData.y = originY + Math.Sin(endAngle) * radY;
                yield return vertexData;
                vertexData.command = VertexCmd.NoMore;
                yield return vertexData;
            }
        }
    }

}