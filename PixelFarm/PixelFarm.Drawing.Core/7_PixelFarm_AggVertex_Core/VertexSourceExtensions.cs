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
// Arc vertex generator
//
//----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using PixelFarm.Agg.VertexSource;
using PixelFarm.VectorMath;

namespace PixelFarm.Agg
{

    public static class VertexSourceExtensions
    {
        public static VertexStoreSnap MakeVertexSnap(this Ellipse ellipse, VertexStore vxs)
        {
            return new VertexStoreSnap(MakeVxs(ellipse, vxs));
        }
        public static VertexStore MakeVxs(this Ellipse ellipse, VertexStore vxs)
        {
            //TODO: review here
            return VertexStoreBuilder.CreateVxs(GetVertexIter(ellipse), vxs);
        }
        public static IEnumerable<VertexData> GetVertexIter(this Ellipse ellipse)
        {
            //TODO: review here again
            VertexData vertexData = new VertexData();
            vertexData.command = VertexCmd.MoveTo;
            vertexData.x = ellipse.originX + ellipse.radiusX;
            vertexData.y = ellipse.originY;
            yield return vertexData;
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
                    vertexData.x = orgX + Math.Cos(MathHelper.Tau - angle) * orgY;
                    vertexData.y = orgY + Math.Sin(MathHelper.Tau - angle) * radY;
                    yield return vertexData;
                }
            }
            else
            {
                for (int i = 1; i < numSteps; i++)
                {
                    angle += anglePerStep;
                    vertexData.x = orgX + Math.Cos(angle) * orgY;
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
        public static void CreateBezierVxs3(VertexStore vxs, Vector2 start, Vector2 end,
           Vector2 control1)
        {
            var curve = new VectorMath.BezierCurveQuadric(
                start, end,
                control1);
            vxs.AddLineTo(start.x, start.y);
            float eachstep = (float)1 / NSteps;
            float stepSum = eachstep;//start
            for (int i = 1; i < NSteps; ++i)
            {
                var vector2 = curve.CalculatePoint(stepSum);
                vxs.AddLineTo(vector2.x, vector2.y);
                stepSum += eachstep;
            }
            vxs.AddLineTo(end.x, end.y);
            //------------------------------------------------------
            //convert c3 to c4
            //Vector2 c4p2, c4p3;
            //Curve3GetControlPoints(start, control1, end, out c4p2, out c4p3);
            //CreateBezierVxs4(vxs, start, end, c4p2, c4p3); 
        }
        public static void CreateBezierVxs4(VertexStore vxs, Vector2 start, Vector2 end,
           Vector2 control1, Vector2 control2)
        {
            var curve = new VectorMath.BezierCurveCubic(
                start, end,
                control1, control2);
            vxs.AddLineTo(start.x, start.y);
            float eachstep = (float)1 / NSteps;
            float stepSum = eachstep;//start
            for (int i = 1; i < NSteps; ++i)
            {
                var vector2 = curve.CalculatePoint(stepSum);
                vxs.AddLineTo(vector2.x, vector2.y);
                stepSum += eachstep;
            }
            vxs.AddLineTo(end.x, end.y);
        }
        public static void MakeLines(this Curve4 curve, VertexStore vxs, double x1, double y1,
            double p2x, double p2y,
            double p3x, double p3y,
            double x2, double y2)
        {
            CreateBezierVxs4(vxs,
             new PixelFarm.VectorMath.Vector2(x1, y1),
             new PixelFarm.VectorMath.Vector2(x2, y2),
             new PixelFarm.VectorMath.Vector2(p2x, p2y),
             new PixelFarm.VectorMath.Vector2(p3x, p3y));
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
               new PixelFarm.VectorMath.Vector2(x1, y1),
               new PixelFarm.VectorMath.Vector2(x2, y2),
               new PixelFarm.VectorMath.Vector2(cx, cy));
            return;
            //if (this.m_approximation_method == Curves.CurveApproximationMethod.Inc)
            //{
            //    //m_curve_inc.Init(x1, y1, cx, cy, x2, y2);
            //    //bool isFirst = true;
            //    //foreach (VertexData currentVertextData in m_curve_inc.GetVertexIter())
            //    //{
            //    //    if (isFirst)
            //    //    {
            //    //        isFirst = false;
            //    //        continue;
            //    //    }

            //    //    if (ShapePath.IsEmpty(currentVertextData.command))
            //    //    {
            //    //        break;
            //    //    }

            //    //    VertexData vertexData = new VertexData(
            //    //       NxCmdAndFlags.LineTo,
            //    //       currentVertextData.position);

            //    //    vxs.AddVertex(vertexData);
            //    //}
            //}
            //else
            //{
            //    m_curve_div.Init(x1, y1, cx, cy, x2, y2);
            //    m_curve_div.MakeLines(vxs);
            //}

            ////---------------------------------------------------------------------
            //IEnumerator<VertexData> curveIterator = this.GetVertexIter().GetEnumerator();
            //curveIterator.MoveNext(); // First call returns path_cmd_move_to
            //do
            //{
            //    curveIterator.MoveNext();
            //    VertexData currentVertextData = curveIterator.Current;
            //    if (ShapePath.IsEmpty(currentVertextData.command))
            //    {
            //        break;
            //    }

            //    VertexData vertexData = new VertexData(
            //       NxCmdAndFlags.LineTo,
            //       currentVertextData.position);

            //    vxs.AddVertex(vertexData);

            //} while (!ShapePath.IsEmpty(curveIterator.Current.command));
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