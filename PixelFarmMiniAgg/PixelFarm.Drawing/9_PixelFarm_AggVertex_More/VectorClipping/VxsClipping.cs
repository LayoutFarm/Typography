//MIT 2016, WinterDev

using System.Collections.Generic;
using ClipperLib;
namespace PixelFarm.Agg.VertexSource
{
    public enum VxsClipperType : byte
    {
        InterSect = ClipType.ctIntersection,
        Union = ClipType.ctUnion,
        Difference = ClipType.ctDifference,
        Xor = ClipType.ctXor,
    }

    public static class VxsClipper
    {
        public static List<VertexStore> CombinePaths(VertexStoreSnap a, VertexStoreSnap b, VxsClipperType vxsClipType, bool separateIntoSmallSubPaths)
        {
            ClipType clipType = (ClipType)vxsClipType;
            List<List<IntPoint>> aPolys = CreatePolygons(a);
            List<List<IntPoint>> bPolys = CreatePolygons(b);
            Clipper clipper = new Clipper();
            clipper.AddPaths(aPolys, PolyType.ptSubject, true);
            clipper.AddPaths(bPolys, PolyType.ptClip, true);
            List<List<IntPoint>> intersectedPolys = new List<List<IntPoint>>();
            clipper.Execute(clipType, intersectedPolys);
            List<VertexStore> resultList = new List<VertexStore>();
            PathWriter output = new PathWriter();
            if (separateIntoSmallSubPaths)
            {
                foreach (List<IntPoint> polygon in intersectedPolys)
                {
                    int j = polygon.Count;
                    if (j > 0)
                    {
                        //first one
                        IntPoint point = polygon[0];
                        output.MoveTo(point.X / 1000.0, point.Y / 1000.0);
                        //next others ...
                        if (j > 1)
                        {
                            for (int i = 1; i < j; ++i)
                            {
                                point = polygon[i];
                                output.LineTo(point.X / 1000.0, point.Y / 1000.0);
                            }
                        }

                        output.CloseFigure();
                        resultList.Add(output.Vxs);
                        //---
                        //clear 
                        output.ClearAndStartNewVxs();
                    }
                }
            }
            else
            {
                foreach (List<IntPoint> polygon in intersectedPolys)
                {
                    int j = polygon.Count;
                    if (j > 0)
                    {
                        //first one
                        IntPoint point = polygon[0];
                        output.MoveTo(point.X / 1000.0, point.Y / 1000.0);
                        //next others ...
                        if (j > 1)
                        {
                            for (int i = 1; i < j; ++i)
                            {
                                point = polygon[i];
                                output.LineTo(point.X / 1000.0, point.Y / 1000.0);
                            }
                        }
                        output.CloseFigure();
                    }

                    //bool first = true;
                    //foreach (IntPoint point in polygon)
                    //{
                    //    if (first)
                    //    {
                    //        output.AddVertex(point.X / 1000.0, point.Y / 1000.0, ShapePath.FlagsAndCommand.CommandMoveTo);
                    //        first = false;
                    //    }
                    //    else
                    //    {
                    //        output.AddVertex(point.X / 1000.0, point.Y / 1000.0, ShapePath.FlagsAndCommand.CommandLineTo);
                    //    }
                    //} 
                }

                output.Stop();
                resultList.Add(output.Vxs);
            }

            return resultList;
        }
        static List<List<IntPoint>> CreatePolygons(VertexStoreSnap a)
        {
            List<List<IntPoint>> allPolys = new List<List<IntPoint>>();
            List<IntPoint> currentPoly = null;
            VertexData last = new VertexData();
            VertexData first = new VertexData();
            bool addedFirst = false;
            var snapIter = a.GetVertexSnapIter();
            double x, y;
            VertexCmd cmd = snapIter.GetNextVertex(out x, out y);
            do
            {
                if (cmd == VertexCmd.LineTo)
                {
                    if (!addedFirst)
                    {
                        currentPoly.Add(new IntPoint((long)(last.x * 1000), (long)(last.y * 1000)));
                        addedFirst = true;
                        first = last;
                    }
                    currentPoly.Add(new IntPoint((long)(x * 1000), (long)(y * 1000)));
                    last = new VertexData(cmd, x, y);
                }
                else
                {
                    addedFirst = false;
                    currentPoly = new List<IntPoint>();
                    allPolys.Add(currentPoly);
                    if (cmd == VertexCmd.MoveTo)
                    {
                        last = new VertexData(cmd, x, y);
                    }
                    else
                    {
                        last = first;
                    }
                }
                cmd = snapIter.GetNextVertex(out x, out y);
            } while (cmd != VertexCmd.Stop);
            return allPolys;
        }
    }
}
