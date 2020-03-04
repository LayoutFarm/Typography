//MIT, 2019-present, WinterDev
//based on  ...
//(MIT, 2016, Viktor Chlumsky, Multi-channel signed distance field generator, from https://github.com/Chlumsky/msdfge)
//-----------------------------------  

using System;
using System.Collections.Generic;

using PixelFarm.CpuBlit;
using PixelFarm.CpuBlit.VertexProcessing;
using PixelFarm.Drawing;

namespace ExtMsdfGen
{



    /// <summary>
    /// msdf texture generator
    /// </summary>
    public class MsdfGen3
    {
        PixelFarm.CpuBlit.Rasterization.PrebuiltGammaTable _prebuiltThresholdGamma_100;
        PixelFarm.CpuBlit.Rasterization.PrebuiltGammaTable _prebuiltThresholdGamma_40;
        PixelFarm.CpuBlit.Rasterization.PrebuiltGammaTable _prebuiltThresholdGamma_50;
        MyCustomPixelBlender _myCustomPixelBlender = new MyCustomPixelBlender();

        public MsdfGen3()
        {
            _prebuiltThresholdGamma_40 = new PixelFarm.CpuBlit.Rasterization.PrebuiltGammaTable(
                new PixelFarm.CpuBlit.FragmentProcessing.GammaThreshold(0.3f));//*** 30% coverage 
            //
            _prebuiltThresholdGamma_50 = new PixelFarm.CpuBlit.Rasterization.PrebuiltGammaTable(
                new PixelFarm.CpuBlit.FragmentProcessing.GammaThreshold(0.5f));//***50% coverage 

            _prebuiltThresholdGamma_100 = new PixelFarm.CpuBlit.Rasterization.PrebuiltGammaTable(
                new PixelFarm.CpuBlit.FragmentProcessing.GammaThreshold(1f));//*** 100% coverage 
        }
        public MsdfGenParams MsdfGenParams { get; set; }
#if DEBUG
        public bool dbugWriteMsdfTexture { get; set; }

#endif 
        void CreateOuterBorder(VertexStore vxs, double x0, double y0, double x1, double y1, double w)
        {
            //-------------
            PixelFarm.VectorMath.Vector2 vector = new PixelFarm.VectorMath.Vector2(x1 - x0, y1 - y0);
            PixelFarm.VectorMath.Vector2 inline1 = vector.NewLength(w);
            x0 = x0 - inline1.x;
            y0 = y0 - inline1.y;
            x1 = x1 + inline1.x;
            y1 = y1 + inline1.y;
            //
            PixelFarm.VectorMath.Vector2 vdiff = vector.RotateInDegree(90).NewLength(w);
            vxs.AddMoveTo(x0, y0);
            vxs.AddLineTo(x0 + vdiff.x, y0 + vdiff.y);
            vxs.AddLineTo(x1 + vdiff.x, y1 + vdiff.y);
            vxs.AddLineTo(x1, y1);
            vxs.AddCloseFigure();
            //-------------

        }
        void CreateInnerBorder(VertexStore vxs, double x0, double y0, double x1, double y1, double w)
        {
            //-------------
            PixelFarm.VectorMath.Vector2 vector = new PixelFarm.VectorMath.Vector2(x1 - x0, y1 - y0);
            //PixelFarm.VectorMath.Vector2 inline1 = vector.NewLength(w);
            //x0 = x0 - inline1.x;
            //y0 = y0 - inline1.y;
            //x1 = x1 + inline1.x;
            //y1 = y1 + inline1.y;
            //
            PixelFarm.VectorMath.Vector2 vdiff = vector.RotateInDegree(270).NewLength(w);
            vxs.AddMoveTo(x0, y0);
            vxs.AddLineTo(x1, y1);
            vxs.AddLineTo(x1 + vdiff.x, y1 + vdiff.y);
            vxs.AddLineTo(x0 + vdiff.x, y0 + vdiff.y);
            vxs.AddCloseFigure();
            //-------------
        }
        void Fill(AggPainter painter, PathWriter writer,
                  CurveFlattener flattener,
                  VertexStore v2, double dx, double dy,
                  ContourCorner c0, ContourCorner c1)
        {

            //counter-clockwise
            if (!c0.MiddlePointKindIsTouchPoint) { return; }
            //-------------------------------------------------------
            if (c0.RightPointKindIsTouchPoint)
            {
                using (VxsTemp.Borrow(out var v9))
                {
                    _myCustomPixelBlender.FillMode = MyCustomPixelBlender.BlenderFillMode.InnerBorder;
                    CreateInnerBorder(v9,
                     c0.middlePoint.X, c0.middlePoint.Y,
                     c1.middlePoint.X, c1.middlePoint.Y, 3);
                    painter.Fill(v9, c0.InnerColor);

                    //-------------
                    v9.Clear(); //reuse
                    _myCustomPixelBlender.FillMode = MyCustomPixelBlender.BlenderFillMode.OuterBorder;
                    CreateOuterBorder(v9,
                        c0.middlePoint.X, c0.middlePoint.Y,
                        c1.middlePoint.X, c1.middlePoint.Y, 3);
                    painter.Fill(v9, c0.OuterColor);
                }
            }
            else
            {
                painter.CurrentBxtBlendOp = null;//**

                //right may be Curve2 or Curve3
                EdgeSegment ownerSeg = c1.CenterSegment;
                switch (ownerSeg.SegmentKind)
                {
                    default: throw new NotSupportedException();
                    case EdgeSegmentKind.CubicSegment:
                        {
                            //approximate 
                            CubicSegment cs = (CubicSegment)ownerSeg;

                            using (VxsTemp.Borrow(out var v3, out var v4, out var v7))
                            using (VectorToolBox.Borrow(out Stroke s))
                            {
                                //double rad0 = Math.Atan2(cs.P0.y - cs.P1.y, cs.P0.x - cs.P1.x);
                                //v3.AddMoveTo(cs.P0.x + dx + Math.Cos(rad0) * 4, cs.P0.y + dy + Math.Sin(rad0) * 4);
                                v3.AddMoveTo(cs.P0.x + dx, cs.P0.y + dy);
                                v3.AddCurve4To(cs.P1.x + dx, cs.P1.y + dy,
                                               cs.P2.x + dx, cs.P2.y + dy,
                                               cs.P3.x + dx, cs.P3.y + dy);

                                //double rad1 = Math.Atan2(cs.P3.y - cs.P2.y, cs.P3.x - cs.P2.x);
                                //v3.AddLineTo((cs.P3.x + dx) + Math.Cos(rad1) * 4, (cs.P3.y + dy) + Math.Sin(rad1) * 4);
                                v3.AddNoMore();// 

                                //
                                flattener.MakeVxs(v3, v4);
                                s.Width = 4;//2 px on each side
                                s.MakeVxs(v4, v7);

                                painter.Fill(v7, c0.OuterColor);


                                writer.Clear();
                                writer.MoveTo(c0.ExtPoint_LeftInner.X, c0.ExtPoint_LeftInner.Y);
                                writer.LineTo(c0.ExtPoint_RightOuter.X, c0.ExtPoint_RightOuter.Y);
                                writer.LineTo(c0.middlePoint.X, c0.middlePoint.Y);
                                writer.CloseFigure();
                                //encode color 
                                ushort overlapCode = _myCustomPixelBlender.RegisterOverlapOuter(c0.CornerNo, c1.CornerNo, AreaKind.OverlapOutside);
                                //TODO: predictable overlap area....
                                Color color = EdgeBmpLut.EncodeToColor(overlapCode, AreaKind.OverlapOutside);
                                painter.Fill(v2, color);
                            }
                        }
                        break;
                    case EdgeSegmentKind.QuadraticSegment:
                        {
                            QuadraticSegment qs = (QuadraticSegment)ownerSeg;
                            using (VxsTemp.Borrow(out var v3, out var v4, out var v7))
                            using (VectorToolBox.Borrow(out Stroke s))
                            {
                                //double rad0 = Math.Atan2(qs.P0.y - qs.P1.y, qs.P0.x - qs.P1.x);
                                //v3.AddMoveTo(qs.P0.x + dx + Math.Cos(rad0) * 4, qs.P0.y + dy + Math.Sin(rad0) * 4);
                                v3.AddMoveTo(qs.P0.x + dx, qs.P0.y + dy);
                                v3.AddCurve3To(qs.P1.x + dx, qs.P1.y + dy,
                                               qs.P2.x + dx, qs.P2.y + dy);

                                //double rad1 = Math.Atan2(qs.P2.y - qs.P1.y, qs.P2.x - qs.P1.x);
                                //v3.AddLineTo((qs.P2.x + dx) + Math.Cos(rad1) * 4, (qs.P2.y + dy) + Math.Sin(rad1) * 4);
                                v3.AddNoMore();//
                                               //
                                flattener.MakeVxs(v3, v4);
                                s.Width = 4;//2 px on each side
                                s.MakeVxs(v4, v7);


                                painter.Fill(v7, c0.OuterColor);

                                //
                                writer.Clear();
                                writer.MoveTo(c0.ExtPoint_LeftInner.X, c0.ExtPoint_LeftInner.Y);
                                writer.LineTo(c0.ExtPoint_RightOuter.X, c0.ExtPoint_RightOuter.Y);
                                writer.LineTo(c0.middlePoint.X, c0.middlePoint.Y);
                                writer.CloseFigure();
                                //painter.Fill(v2, c0.OuterColor);
                                ushort overlapCode = _myCustomPixelBlender.RegisterOverlapOuter(c0.CornerNo, c1.CornerNo, AreaKind.OverlapOutside);
                                //TODO: predictable overlap area....
                                Color color = EdgeBmpLut.EncodeToColor(overlapCode, AreaKind.OverlapOutside);
                                painter.Fill(v2, color);
                            }
                        }
                        break;
                }
            }
        }

        void FillInnerArea(AggPainter painter, PathWriter writer,
                 CurveFlattener flattener,
                 VertexStore v2, double dx, double dy,
                 ContourCorner c0, ContourCorner c1, Color color)
        {
            //counter-clockwise
            if (!c0.MiddlePointKindIsTouchPoint) { return; }
            //-------------------------------------------------------
            if (c0.RightPointKindIsTouchPoint)
            {
                using (VxsTemp.Borrow(out var v9))
                {
                    _myCustomPixelBlender.FillMode = MyCustomPixelBlender.BlenderFillMode.InnerAreaX;

                    CreateInnerBorder(v9,
                     c0.middlePoint.X, c0.middlePoint.Y,
                     c1.middlePoint.X, c1.middlePoint.Y, 6);
                    painter.Fill(v9, color);

                    //-------------
                    v9.Clear(); //reuse 
                }
            }
            else
            {
                painter.CurrentBxtBlendOp = null;//**
                _myCustomPixelBlender.FillMode = MyCustomPixelBlender.BlenderFillMode.InnerAreaX;
                //right may be Curve2 or Curve3
                EdgeSegment ownerSeg = c1.CenterSegment;
                switch (ownerSeg.SegmentKind)
                {
                    default: throw new NotSupportedException();
                    case EdgeSegmentKind.CubicSegment:
                        {
                            //approximate 
                            CubicSegment cs = (CubicSegment)ownerSeg;

                            using (VxsTemp.Borrow(out var v3, out var v4, out var v7))
                            using (VectorToolBox.Borrow(out Stroke s))
                            {
                                //double rad0 = Math.Atan2(cs.P0.y - cs.P1.y, cs.P0.x - cs.P1.x);
                                //v3.AddMoveTo(cs.P0.x + dx + Math.Cos(rad0) * 4, cs.P0.y + dy + Math.Sin(rad0) * 4);
                                v3.AddMoveTo(cs.P0.x + dx, cs.P0.y + dy);
                                v3.AddCurve4To(cs.P1.x + dx, cs.P1.y + dy,
                                               cs.P2.x + dx, cs.P2.y + dy,
                                               cs.P3.x + dx, cs.P3.y + dy);

                                //double rad1 = Math.Atan2(cs.P3.y - cs.P2.y, cs.P3.x - cs.P2.x);
                                //v3.AddLineTo((cs.P3.x + dx) + Math.Cos(rad1) * 4, (cs.P3.y + dy) + Math.Sin(rad1) * 4);
                                v3.AddNoMore();// 

                                //
                                flattener.MakeVxs(v3, v4);
                                s.Width = 4;//2 px on each side
                                s.MakeVxs(v4, v7);

                                painter.Fill(v7, color);


                                writer.Clear();
                                writer.MoveTo(c0.ExtPoint_LeftInner.X, c0.ExtPoint_LeftInner.Y);
                                writer.LineTo(c0.ExtPoint_RightOuter.X, c0.ExtPoint_RightOuter.Y);
                                writer.LineTo(c0.middlePoint.X, c0.middlePoint.Y);
                                writer.CloseFigure();
                                //encode color 
                                ushort overlapCode = _myCustomPixelBlender.RegisterOverlapOuter(c0.CornerNo, c1.CornerNo, AreaKind.OverlapOutside);
                                //TODO: predictable overlap area....

                                painter.Fill(v2, color);
                            }
                        }
                        break;
                    case EdgeSegmentKind.QuadraticSegment:
                        {
                            QuadraticSegment qs = (QuadraticSegment)ownerSeg;
                            using (VxsTemp.Borrow(out var v3, out var v4, out var v7))
                            using (VectorToolBox.Borrow(out Stroke s))
                            {
                                //double rad0 = Math.Atan2(qs.P0.y - qs.P1.y, qs.P0.x - qs.P1.x);
                                //v3.AddMoveTo(qs.P0.x + dx + Math.Cos(rad0) * 4, qs.P0.y + dy + Math.Sin(rad0) * 4);
                                v3.AddMoveTo(qs.P0.x + dx, qs.P0.y + dy);
                                v3.AddCurve3To(qs.P1.x + dx, qs.P1.y + dy,
                                               qs.P2.x + dx, qs.P2.y + dy);

                                //double rad1 = Math.Atan2(qs.P2.y - qs.P1.y, qs.P2.x - qs.P1.x);
                                //v3.AddLineTo((qs.P2.x + dx) + Math.Cos(rad1) * 4, (qs.P2.y + dy) + Math.Sin(rad1) * 4);
                                v3.AddNoMore();//
                                               //
                                flattener.MakeVxs(v3, v4);
                                s.Width = 4;//2 px on each side
                                s.MakeVxs(v4, v7);


                                painter.Fill(v7, color);

                                //
                                writer.Clear();
                                writer.MoveTo(c0.ExtPoint_LeftInner.X, c0.ExtPoint_LeftInner.Y);
                                writer.LineTo(c0.ExtPoint_RightOuter.X, c0.ExtPoint_RightOuter.Y);
                                writer.LineTo(c0.middlePoint.X, c0.middlePoint.Y);
                                writer.CloseFigure();
                                //painter.Fill(v2, c0.OuterColor);
                                ushort overlapCode = _myCustomPixelBlender.RegisterOverlapOuter(c0.CornerNo, c1.CornerNo, AreaKind.OverlapOutside);
                                //TODO: predictable overlap area.... 
                                painter.Fill(v2, color);
                            }
                        }
                        break;
                }
            }
        }


        void FillX1(AggPainter painter, PathWriter writer,
                CurveFlattener flattener,
                VertexStore v2, double dx, double dy,
                ContourCorner c0, ContourCorner c1)
        {

            //counter-clockwise
            if (!c0.MiddlePointKindIsTouchPoint) { return; }
            //-------------------------------------------------------
            if (c0.RightPointKindIsTouchPoint)
            {

                //outer
                writer.MoveTo(c0.middlePoint.X, c0.middlePoint.Y);
                writer.LineTo(c0.ExtPoint_LeftOuter.X, c0.ExtPoint_LeftOuter.Y);
                writer.LineTo(c0.ExtPoint_LeftOuterDest.X, c0.ExtPoint_LeftOuterDest.Y);
                writer.LineTo(c1.middlePoint.X, c1.middlePoint.Y);
                writer.LineTo(c0.middlePoint.X, c0.middlePoint.Y);
                writer.CloseFigure();
                // 
                painter.Fill(v2, c0.OuterColor);
                //inner
                writer.Clear();
                writer.MoveTo(c0.ExtPoint_LeftInner.X, c0.ExtPoint_LeftInner.Y);
                writer.LineTo(c0.middlePoint.X, c0.middlePoint.Y);
                writer.LineTo(c1.middlePoint.X, c1.middlePoint.Y);
                writer.LineTo(c1.ExtPoint_RightInner.X, c1.ExtPoint_RightInner.Y);
                writer.LineTo(c0.ExtPoint_LeftInner.X, c0.ExtPoint_LeftInner.Y);
                writer.CloseFigure();
                ////
                painter.Fill(v2, c0.InnerColor);

                //gap
                writer.Clear();
                //large corner that cover gap
                writer.MoveTo(c0.ExtPoint_LeftInner.X, c0.ExtPoint_LeftInner.Y);
                writer.LineTo(c0.ExtPoint_RightOuter.X, c0.ExtPoint_RightOuter.Y);
                writer.LineTo(c0.ExtPoint_LeftOuter.X, c0.ExtPoint_LeftOuter.Y);
                writer.LineTo(c0.ExtPoint_RightInner.X, c0.ExtPoint_RightInner.Y);
                writer.LineTo(c0.middlePoint.X, c0.middlePoint.Y);
                writer.CloseFigure();

                ushort overlapCode = _myCustomPixelBlender.RegisterOverlapOuter(c0.CornerNo, c1.CornerNo, AreaKind.OverlapOutside);
                //TODO: predictable overlap area....
                Color color = EdgeBmpLut.EncodeToColor(overlapCode, AreaKind.OverlapOutside);
                painter.Fill(v2, color);
                //painter.Fill(v2, PixelFarm.Drawing.Color.Red);
                //painter.Fill(v2, c0.OuterColor);
            }
            else
            {
                painter.CurrentBxtBlendOp = null;//**

                //right may be Curve2 or Curve3
                EdgeSegment ownerSeg = c1.CenterSegment;
                switch (ownerSeg.SegmentKind)
                {
                    default: throw new NotSupportedException();
                    case EdgeSegmentKind.CubicSegment:
                        {
                            //approximate 
                            CubicSegment cs = (CubicSegment)ownerSeg;

                            using (VxsTemp.Borrow(out var v3, out var v4, out var v7))
                            using (VectorToolBox.Borrow(out Stroke s))
                            {
                                //double rad0 = Math.Atan2(cs.P0.y - cs.P1.y, cs.P0.x - cs.P1.x);
                                //v3.AddMoveTo(cs.P0.x + dx + Math.Cos(rad0) * 4, cs.P0.y + dy + Math.Sin(rad0) * 4);
                                v3.AddMoveTo(cs.P0.x + dx, cs.P0.y + dy);
                                v3.AddCurve4To(cs.P1.x + dx, cs.P1.y + dy,
                                               cs.P2.x + dx, cs.P2.y + dy,
                                               cs.P3.x + dx, cs.P3.y + dy);

                                //double rad1 = Math.Atan2(cs.P3.y - cs.P2.y, cs.P3.x - cs.P2.x);
                                //v3.AddLineTo((cs.P3.x + dx) + Math.Cos(rad1) * 4, (cs.P3.y + dy) + Math.Sin(rad1) * 4);
                                v3.AddNoMore();// 

                                //
                                flattener.MakeVxs(v3, v4);
                                s.Width = 4;//2 px on each side
                                s.MakeVxs(v4, v7);

                                painter.Fill(v7, c0.OuterColor);


                                writer.Clear();
                                writer.MoveTo(c0.ExtPoint_LeftInner.X, c0.ExtPoint_LeftInner.Y);
                                writer.LineTo(c0.ExtPoint_RightOuter.X, c0.ExtPoint_RightOuter.Y);
                                writer.LineTo(c0.middlePoint.X, c0.middlePoint.Y);
                                writer.CloseFigure();
                                //encode color 
                                ushort overlapCode = _myCustomPixelBlender.RegisterOverlapOuter(c0.CornerNo, c1.CornerNo, AreaKind.OverlapOutside);
                                //TODO: predictable overlap area....
                                Color color = EdgeBmpLut.EncodeToColor(overlapCode, AreaKind.OverlapOutside);
                                painter.Fill(v2, color);
                            }
                        }
                        break;
                    case EdgeSegmentKind.QuadraticSegment:
                        {
                            QuadraticSegment qs = (QuadraticSegment)ownerSeg;
                            using (VxsTemp.Borrow(out var v3, out var v4, out var v7))
                            using (VectorToolBox.Borrow(out Stroke s))
                            {
                                //double rad0 = Math.Atan2(qs.P0.y - qs.P1.y, qs.P0.x - qs.P1.x);
                                //v3.AddMoveTo(qs.P0.x + dx + Math.Cos(rad0) * 4, qs.P0.y + dy + Math.Sin(rad0) * 4);
                                v3.AddMoveTo(qs.P0.x + dx, qs.P0.y + dy);
                                v3.AddCurve3To(qs.P1.x + dx, qs.P1.y + dy,
                                               qs.P2.x + dx, qs.P2.y + dy);

                                //double rad1 = Math.Atan2(qs.P2.y - qs.P1.y, qs.P2.x - qs.P1.x);
                                //v3.AddLineTo((qs.P2.x + dx) + Math.Cos(rad1) * 4, (qs.P2.y + dy) + Math.Sin(rad1) * 4);
                                v3.AddNoMore();//
                                               //
                                flattener.MakeVxs(v3, v4);
                                s.Width = 4;//2 px on each side
                                s.MakeVxs(v4, v7);


                                painter.Fill(v7, c0.OuterColor);

                                //
                                writer.Clear();
                                writer.MoveTo(c0.ExtPoint_LeftInner.X, c0.ExtPoint_LeftInner.Y);
                                writer.LineTo(c0.ExtPoint_RightOuter.X, c0.ExtPoint_RightOuter.Y);
                                writer.LineTo(c0.middlePoint.X, c0.middlePoint.Y);
                                writer.CloseFigure();
                                //painter.Fill(v2, c0.OuterColor);
                                ushort overlapCode = _myCustomPixelBlender.RegisterOverlapOuter(c0.CornerNo, c1.CornerNo, AreaKind.OverlapOutside);
                                //TODO: predictable overlap area....
                                Color color = EdgeBmpLut.EncodeToColor(overlapCode, AreaKind.OverlapOutside);
                                painter.Fill(v2, color);
                            }
                        }
                        break;
                }
            }
        }
        public SpriteTextureMapData<MemBitmap> GenerateMsdfTexture(VertexStore v1)
        {

            //split contour inside v1
            //List<VxsContour> contourList = new List<VxsContour>();
            //SplitContours(v1, contourList);
            //generate shape for each contour *** 
            //create shape and edge-bmp-lut from a given v1
            Shape shape = CreateShape(v1, out EdgeBmpLut edgeBmpLut);

            if (MsdfGenParams == null)
            {
                MsdfGenParams = new MsdfGenParams();//use default
            }

            //---preview v1 bounds-----------
            MsdfGlyphGen.PreviewSizeAndLocation(
               shape,
               MsdfGenParams,
               out int imgW, out int imgH,
               out Vector2 translateVec);

            //------------------------------------
            List<ContourCorner> corners = edgeBmpLut.Corners;
            TranslateCorners(corners, translateVec.x, translateVec.y);


            using (MemBitmap bmpLut = new MemBitmap(imgW, imgH)) //intermediate data for 
            using (VxsTemp.Borrow(out var v2, out var v5, out var v6))
            using (VxsTemp.Borrow(out var v7))
            using (VectorToolBox.Borrow(out CurveFlattener flattener))
            using (VectorToolBox.Borrow(v2, out PathWriter writer))
            using (AggPainterPool.Borrow(bmpLut, out AggPainter painter))
            {
                _myCustomPixelBlender.ClearOverlapList();
                painter.RenderSurface.SetCustomPixelBlender(_myCustomPixelBlender);


                painter.Clear(PixelFarm.Drawing.Color.Black);

                v1.TranslateToNewVxs(translateVec.x, translateVec.y, v5);
                flattener.MakeVxs(v5, v7);

                //---------
                //standard coverage 50 
                painter.RenderSurface.SetGamma(_prebuiltThresholdGamma_50);
                _myCustomPixelBlender.FillMode = MyCustomPixelBlender.BlenderFillMode.Force;
                painter.Fill(v7, EdgeBmpLut.EncodeToColor(0, AreaKind.AreaInsideCoverage50));
                //---------

                int cornerCount = corners.Count;
                List<int> cornerOfNextContours = edgeBmpLut.CornerOfNextContours;
                int startAt = 0;
                int n = 1;
                int m = 1;
                for (int cc = 0; cc < cornerOfNextContours.Count; ++cc)
                {
                    int nextStartAt = cornerOfNextContours[cc];
                    //fill white solid bg for each contour

                    using (VxsTemp.Borrow(out var vxs1))
                    {
                        int a = 0;
                        for (; m <= nextStartAt - 1; ++m)
                        {
                            ContourCorner c = corners[m];
                            EdgeSegment seg = c.CenterSegment;
                            switch (seg.SegmentKind)
                            {
                                default: throw new NotSupportedException();
                                case EdgeSegmentKind.CubicSegment:
                                    {
                                        CubicSegment cubicSeg = (CubicSegment)seg;
                                        if (a == 0)
                                        {
                                            vxs1.AddMoveTo(cubicSeg.P0.x, cubicSeg.P0.y);
                                        }
                                        //
                                        vxs1.AddCurve4To(cubicSeg.P1.x, cubicSeg.P1.y,
                                            cubicSeg.P2.x, cubicSeg.P2.y,
                                            cubicSeg.P3.x, cubicSeg.P3.y);
                                    }
                                    break;
                                case EdgeSegmentKind.LineSegment:
                                    {
                                        LinearSegment lineSeg = (LinearSegment)seg;
                                        if (a == 0)
                                        {
                                            vxs1.AddMoveTo(lineSeg.P0.x, lineSeg.P0.y);
                                        }
                                        vxs1.AddLineTo(lineSeg.P1.x, lineSeg.P1.y);
                                    }
                                    break;
                                case EdgeSegmentKind.QuadraticSegment:
                                    {
                                        QuadraticSegment quadraticSeg = (QuadraticSegment)seg;
                                        if (a == 0)
                                        {
                                            vxs1.AddMoveTo(quadraticSeg.P0.x, quadraticSeg.P0.y);
                                        }
                                        vxs1.AddCurve3To(quadraticSeg.P1.x, quadraticSeg.P1.y,
                                            quadraticSeg.P2.x, quadraticSeg.P2.y);
                                    }
                                    break;
                            }
                            a++;
                        }

                        v6.Clear();

                        vxs1.TranslateToNewVxs(translateVec.x, translateVec.y, v5);
                        flattener.MakeVxs(v5, v6);

                        Color insideCoverage50 = EdgeBmpLut.EncodeToColor((ushort)(cc), AreaKind.AreaInsideCoverage100);
                        _myCustomPixelBlender.FillMode = MyCustomPixelBlender.BlenderFillMode.Force; //***
                        _myCustomPixelBlender.SetCurrentInsideAreaCoverage(insideCoverage50);
                        painter.RenderSurface.SetGamma(_prebuiltThresholdGamma_100);
                        painter.Fill(v6, insideCoverage50);

                        v5.Clear();
                        v6.Clear();
                    }

                    //Color insideCoverageX = EdgeBmpLut.EncodeToColor((ushort)(cc), AreaKind.AreaInsideCoverageX);
                    //painter.RenderSurface.SetGamma(_prebuiltThresholdGamma_50); //*** with 40% coverage , this creates overlapped area 
                    //for (; n <= nextStartAt - 1; ++n)
                    //{
                    //    FillInnerArea(painter, writer, flattener, v2, translateVec.x, translateVec.y, corners[n - 1], corners[n], insideCoverageX);
                    //    writer.Clear();//**
                    //}
                    //{
                    //    //the last one 
                    //    FillInnerArea(painter, writer, flattener, v2, translateVec.x, translateVec.y, corners[nextStartAt - 1], corners[startAt], insideCoverageX);
                    //    writer.Clear();//**
                    //}

                    //-----------
                    //AA-borders
                    painter.RenderSurface.SetGamma(_prebuiltThresholdGamma_40); //*** with 40% coverage , this creates overlapped area 
                    for (; n <= nextStartAt - 1; ++n)
                    {
                        Fill(painter, writer, flattener, v2, translateVec.x, translateVec.y, corners[n - 1], corners[n]);
                        writer.Clear();//**
                    }
                    {
                        //the last one 
                        Fill(painter, writer, flattener, v2, translateVec.x, translateVec.y, corners[nextStartAt - 1], corners[startAt]);
                        writer.Clear();//**
                    }

                    startAt = nextStartAt;
                    n++;
                    m++;
                }

                ////----------------
                //painter.RenderSurface.SetGamma(_prebuiltThresholdGamma_100);
                //_myCustomPixelBlender.FillMode = MyCustomPixelBlender.BlenderFillMode.FinalFill;
                //painter.Fill(v7, EdgeBmpLut.EncodeToColor(0, AreaKind.AreaInsideCoverage100));
                ////----------------


                painter.RenderSurface.SetCustomPixelBlender(null);
                painter.RenderSurface.SetGamma(null);

                //
                List<CornerList> overlappedList = MakeUniqueList(_myCustomPixelBlender._overlapList);
                edgeBmpLut.SetOverlappedList(overlappedList);

#if DEBUG

                if (dbugWriteMsdfTexture)
                {
                    //save for debug 
                    //we save to msdf_shape_lut2.png
                    //and check it from external program
                    //but we generate msdf bitmap from msdf_shape_lut.png 
                    bmpLut.SaveImage("msdf_shape_lut2.png");//intern
                    var bmp5 = MemBitmap.LoadBitmap("msdf_shape_lut2.png");
                    int[] lutBuffer5 = bmp5.CopyImgBuffer(bmpLut.Width, bmpLut.Height);
                    if (bmpLut.Width == 338 && bmpLut.Height == 477)
                    {
                        dbugBreak = true;
                    }
                    edgeBmpLut.SetBmpBuffer(bmpLut.Width, bmpLut.Height, lutBuffer5);
                    //generate actual sprite
                    SpriteTextureMapData<MemBitmap> spriteTextureMapData = MsdfGlyphGen.CreateMsdfImage(shape, MsdfGenParams, edgeBmpLut);
                    //save msdf bitmap to file              
                    spriteTextureMapData.Source.SaveImage("msdf_shape.png");
                    return spriteTextureMapData;
                }

#endif
                int[] lutBuffer = bmpLut.CopyImgBuffer(bmpLut.Width, bmpLut.Height);
                edgeBmpLut.SetBmpBuffer(bmpLut.Width, bmpLut.Height, lutBuffer);
                return MsdfGlyphGen.CreateMsdfImage(shape, MsdfGenParams, edgeBmpLut);
            }
        }
        //        public SpriteTextureMapData<MemBitmap> GenerateMsdfTexture(VertexStore v1)
        //        {

        //            //split contour inside v1
        //            //List<VxsContour> contourList = new List<VxsContour>();
        //            //SplitContours(v1, contourList);
        //            //generate shape for each contour *** 
        //            //create shape and edge-bmp-lut from a given v1
        //            Shape shape = CreateShape(v1, out EdgeBmpLut edgeBmpLut);

        //            if (MsdfGenParams == null)
        //            {
        //                MsdfGenParams = new MsdfGenParams();//use default
        //            }

        //            //---preview v1 bounds-----------
        //            MsdfGlyphGen.PreviewSizeAndLocation(
        //               shape,
        //               MsdfGenParams,
        //               out int imgW, out int imgH,
        //               out Vector2 translateVec);

        //            //------------------------------------
        //            List<ContourCorner> corners = edgeBmpLut.Corners;
        //            TranslateCorners(corners, translateVec.x, translateVec.y);


        //            using (MemBitmap bmpLut = new MemBitmap(imgW, imgH)) //intermediate data for 
        //            using (VxsTemp.Borrow(out var v2, out var v5, out var v6))
        //            using (VxsTemp.Borrow(out var v7))
        //            using (VectorToolBox.Borrow(out CurveFlattener flattener))
        //            using (VectorToolBox.Borrow(v2, out PathWriter writer))
        //            using (AggPainterPool.Borrow(bmpLut, out AggPainter painter))
        //            {
        //                _myCustomPixelBlender.ClearOverlapList();
        //                painter.RenderSurface.SetCustomPixelBlender(_myCustomPixelBlender);


        //                painter.Clear(PixelFarm.Drawing.Color.Black);

        //                v1.TranslateToNewVxs(translateVec.x, translateVec.y, v5);
        //                flattener.MakeVxs(v5, v7);

        //                //---------
        //                //standard coverage 50 
        //                painter.RenderSurface.SetGamma(_prebuiltThresholdGamma_50);
        //                _myCustomPixelBlender.FillMode = MyCustomPixelBlender.BlenderFillMode.Force;
        //                painter.Fill(v7, EdgeBmpLut.EncodeToColor(0, AreaKind.AreaInsideCoverage50));
        //                //---------

        //                int cornerCount = corners.Count;
        //                List<int> cornerOfNextContours = edgeBmpLut.CornerOfNextContours;
        //                int startAt = 0;
        //                int n = 1;
        //                int m = 1;
        //                for (int cc = 0; cc < cornerOfNextContours.Count; ++cc)
        //                {
        //                    int nextStartAt = cornerOfNextContours[cc];
        //                    //fill white solid bg for each contour

        //                    using (VxsTemp.Borrow(out var vxs1))
        //                    {
        //                        int a = 0;
        //                        for (; m <= nextStartAt - 1; ++m)
        //                        {
        //                            ContourCorner c = corners[m];
        //                            EdgeSegment seg = c.CenterSegment;
        //                            switch (seg.SegmentKind)
        //                            {
        //                                default: throw new NotSupportedException();
        //                                case EdgeSegmentKind.CubicSegment:
        //                                    {
        //                                        CubicSegment cubicSeg = (CubicSegment)seg;
        //                                        if (a == 0)
        //                                        {
        //                                            vxs1.AddMoveTo(cubicSeg.P0.x, cubicSeg.P0.y);
        //                                        }
        //                                        //
        //                                        vxs1.AddCurve4To(cubicSeg.P1.x, cubicSeg.P1.y,
        //                                            cubicSeg.P2.x, cubicSeg.P2.y,
        //                                            cubicSeg.P3.x, cubicSeg.P3.y);
        //                                    }
        //                                    break;
        //                                case EdgeSegmentKind.LineSegment:
        //                                    {
        //                                        LinearSegment lineSeg = (LinearSegment)seg;
        //                                        if (a == 0)
        //                                        {
        //                                            vxs1.AddMoveTo(lineSeg.P0.x, lineSeg.P0.y);
        //                                        }
        //                                        vxs1.AddLineTo(lineSeg.P1.x, lineSeg.P1.y);
        //                                    }
        //                                    break;
        //                                case EdgeSegmentKind.QuadraticSegment:
        //                                    {
        //                                        QuadraticSegment quadraticSeg = (QuadraticSegment)seg;
        //                                        if (a == 0)
        //                                        {
        //                                            vxs1.AddMoveTo(quadraticSeg.P0.x, quadraticSeg.P0.y);
        //                                        }
        //                                        vxs1.AddCurve3To(quadraticSeg.P1.x, quadraticSeg.P1.y,
        //                                            quadraticSeg.P2.x, quadraticSeg.P2.y);
        //                                    }
        //                                    break;
        //                            }
        //                            a++;
        //                        }

        //                        v6.Clear();

        //                        vxs1.TranslateToNewVxs(translateVec.x, translateVec.y, v5);
        //                        flattener.MakeVxs(v5, v6);

        //                        Color insideCoverage100 = EdgeBmpLut.EncodeToColor((ushort)cc, AreaKind.AreaInsideCoverage100);
        //                        _myCustomPixelBlender.FillMode = MyCustomPixelBlender.BlenderFillMode.InnerArea;
        //                        _myCustomPixelBlender.SetCurrentInsideAreaCoverage100(insideCoverage100);
        //                        painter.RenderSurface.SetGamma(_prebuiltThresholdGamma_50);
        //                        painter.Fill(v6, insideCoverage100);

        //                        v5.Clear();
        //                        v6.Clear();
        //                    }

        //                    //borders 
        //                    painter.RenderSurface.SetGamma(_prebuiltThresholdGamma_40); //*** with 40% coverage , this creates overlapped area 

        //                    for (; n <= nextStartAt - 1; ++n)
        //                    {
        //                        Fill(painter, writer, flattener, v2, translateVec.x, translateVec.y, corners[n - 1], corners[n]);
        //                        writer.Clear();//**
        //                    }
        //                    {
        //                        //the last one 
        //                        Fill(painter, writer, flattener, v2, translateVec.x, translateVec.y, corners[nextStartAt - 1], corners[startAt]);
        //                        writer.Clear();//**
        //                    }

        //                    startAt = nextStartAt;
        //                    n++;
        //                    m++;
        //                }

        //                //----------------
        //                painter.RenderSurface.SetGamma(_prebuiltThresholdGamma_100);
        //                _myCustomPixelBlender.FillMode = MyCustomPixelBlender.BlenderFillMode.FinalFill;
        //                painter.Fill(v7, EdgeBmpLut.EncodeToColor(0, AreaKind.AreaInsideCoverage100));
        //                //----------------


        //                painter.RenderSurface.SetCustomPixelBlender(null);
        //                painter.RenderSurface.SetGamma(null);

        //                //
        //                List<CornerList> overlappedList = MakeUniqueList(_myCustomPixelBlender._overlapList);
        //                edgeBmpLut.SetOverlappedList(overlappedList);

        //#if DEBUG

        //                if (dbugWriteMsdfTexture)
        //                {
        //                    //save for debug 
        //                    //we save to msdf_shape_lut2.png
        //                    //and check it from external program
        //                    //but we generate msdf bitmap from msdf_shape_lut.png 
        //                    bmpLut.SaveImage("msdf_shape_lut2.png");//intern
        //                    var bmp5 = MemBitmap.LoadBitmap("msdf_shape_lut2.png");
        //                    int[] lutBuffer5 = bmp5.CopyImgBuffer(bmpLut.Width, bmpLut.Height);
        //                    if (bmpLut.Width == 338 && bmpLut.Height == 477)
        //                    {
        //                        dbugBreak = true;
        //                    }
        //                    edgeBmpLut.SetBmpBuffer(bmpLut.Width, bmpLut.Height, lutBuffer5);
        //                    //generate actual sprite
        //                    SpriteTextureMapData<MemBitmap> spriteTextureMapData = MsdfGlyphGen.CreateMsdfImage(shape, MsdfGenParams, edgeBmpLut);
        //                    //save msdf bitmap to file              
        //                    spriteTextureMapData.Source.SaveImage(msdf_shape.png");
        //                    return spriteTextureMapData;
        //                }

        //#endif

        //                int[] lutBuffer = bmpLut.CopyImgBuffer(bmpLut.Width, bmpLut.Height);
        //                edgeBmpLut.SetBmpBuffer(bmpLut.Width, bmpLut.Height, lutBuffer);
        //                return MsdfGlyphGen.CreateMsdfImage(shape, MsdfGenParams, edgeBmpLut);
        //            }
        //        }
#if DEBUG
        public static bool dbugBreak;
#endif
        Dictionary<int, bool> _uniqueCorners = new Dictionary<int, bool>();

        List<CornerList> MakeUniqueList(List<CornerList> primaryOverlappedList)
        {

            List<CornerList> list = new List<CornerList>();
            //copy data to bmpLut
            int j = primaryOverlappedList.Count;


            for (int k = 0; k < j; ++k)
            {
                if (k >= 388)
                {

                }
                _uniqueCorners.Clear();
                CornerList overlapped = primaryOverlappedList[k];
                //each group -> make unique 
                CornerList newlist = new CornerList();
                int m = overlapped.Count;
                for (int n = 0; n < m; ++n)
                {
                    ushort corner = overlapped[n];
                    if (!_uniqueCorners.ContainsKey(corner))
                    {
                        _uniqueCorners.Add(corner, true);
                        newlist.Append(corner);
                    }
                }
                _uniqueCorners.Clear();
                // 
                list.Add(newlist);
            }
            return list;

        }
        static void TranslateCorners(List<ContourCorner> corners, double dx, double dy)
        {
            //test 2 if each edge has unique color
            int j = corners.Count;
            for (int i = 0; i < j; ++i)
            {
                corners[i].Offset(dx, dy);
            }
        }
        static void FlattenPoints(EdgeSegment segment, List<Vec2Info> points)
        {
            switch (segment.SegmentKind)
            {
                default: throw new NotSupportedException();
                case EdgeSegmentKind.LineSegment:
                    {
                        LinearSegment seg = (LinearSegment)segment;
                        points.Add(new Vec2Info(segment, Vec2PointKind.Touch1, seg.P0));
                    }
                    break;
                case EdgeSegmentKind.QuadraticSegment:
                    {
                        QuadraticSegment seg = (QuadraticSegment)segment;
                        points.Add(new Vec2Info(segment, Vec2PointKind.Touch1, seg.P0));
                        points.Add(new Vec2Info(segment, Vec2PointKind.C2, seg.P1));
                    }
                    break;
                case EdgeSegmentKind.CubicSegment:
                    {
                        CubicSegment seg = (CubicSegment)segment;
                        points.Add(new Vec2Info(segment, Vec2PointKind.Touch1, seg.P0));
                        points.Add(new Vec2Info(segment, Vec2PointKind.C3, seg.P1));
                        points.Add(new Vec2Info(segment, Vec2PointKind.C3, seg.P2));
                    }
                    break;
            }

        }
        static void CreateCorners(List<Vec2Info> points, List<ContourCorner> corners)
        {

            int j = points.Count;
            int beginAt = corners.Count;
            if (beginAt >= ushort.MaxValue)
            {
                throw new NotSupportedException();
            }

            for (int i = 1; i < j - 1; ++i)
            {
                ContourCorner corner = new ContourCorner(corners.Count, points[i - 1], points[i], points[i + 1]);
                corners.Add(corner);

#if DEBUG
                corner.dbugLeftIndex = beginAt + i - 1;
                corner.dbugMiddleIndex = beginAt + i;
                corner.dbugRightIndex = beginAt + i + 1;
#endif

            }

            {

                ContourCorner corner = new ContourCorner(corners.Count, points[j - 2], points[j - 1], points[0]);
                corners.Add(corner);
#if DEBUG
                corner.dbugLeftIndex = beginAt + j - 2;
                corner.dbugMiddleIndex = beginAt + j - 1;
                corner.dbugRightIndex = beginAt + 0;
#endif

            }

            {

                ContourCorner corner = new ContourCorner(corners.Count, points[j - 1], points[0], points[1]);
                corners.Add(corner);
#if DEBUG
                corner.dbugLeftIndex = beginAt + j - 1;
                corner.dbugMiddleIndex = beginAt + 0;
                corner.dbugRightIndex = beginAt + 1;
#endif

            }

        }
        static void CreateCorners(Contour contour, List<ContourCorner> output)
        {
            //create corner-arm relation for a given contour
            List<EdgeSegment> edges = contour.edges;
            int j = edges.Count;
            List<Vec2Info> flattenPoints = new List<Vec2Info>();
            for (int i = 0; i < j; ++i)
            {
                FlattenPoints(edges[i], flattenPoints);
            }
            CreateCorners(flattenPoints, output);
        }

        static Shape CreateShape(VertexStore vxs, out EdgeBmpLut bmpLut)
        {
            List<EdgeSegment> flattenEdges = new List<EdgeSegment>();
            Shape shape = new Shape(); //start with blank shape

            int i = 0;
            double x, y;
            VertexCmd cmd;
            Contour cnt = null;
            double latestMoveToX = 0;
            double latestMoveToY = 0;
            double latestX = 0;
            double latestY = 0;

            List<ContourCorner> corners = new List<ContourCorner>();
            List<int> edgeOfNextContours = new List<int>();
            List<int> cornerOfNextContours = new List<int>();

            while ((cmd = vxs.GetVertex(i, out x, out y)) != VertexCmd.NoMore)
            {
                switch (cmd)
                {
                    case VertexCmd.Close:
                        {
                            //close current cnt

                            if ((latestMoveToX != latestX) ||
                                (latestMoveToY != latestY))
                            {
                                //add line to close the shape
                                if (cnt != null)
                                {
                                    flattenEdges.Add(cnt.AddLine(latestX, latestY, latestMoveToX, latestMoveToY));
                                }
                            }
                            if (cnt != null)
                            {
                                //***                                
                                CreateCorners(cnt, corners);
                                edgeOfNextContours.Add(flattenEdges.Count);
                                cornerOfNextContours.Add(corners.Count);
                                shape.contours.Add(cnt);
                                //***
                                cnt = null;
                            }
                        }
                        break;
                    case VertexCmd.C3:
                        {

                            //C3 curve (Quadratic)                            
                            if (cnt == null)
                            {
                                cnt = new Contour();
                                cnt.winding();
                            }
                            VertexCmd cmd1 = vxs.GetVertex(i + 1, out double x1, out double y1);
                            i++;
                            if (cmd1 != VertexCmd.LineTo)
                            {
                                throw new NotSupportedException();
                            }

                            //in this version, 
                            //we convert Quadratic to Cubic (https://stackoverflow.com/questions/9485788/convert-quadratic-curve-to-cubic-curve)

                            //Control1X = StartX + ((2f/3) * (ControlX - StartX))
                            //Control2X = EndX + ((2f/3) * (ControlX - EndX))


                            //flattenEdges.Add(cnt.AddCubicSegment(
                            //    latestX, latestY,
                            //    ((2f / 3) * (x - latestX)) + latestX, ((2f / 3) * (y - latestY)) + latestY,
                            //    ((2f / 3) * (x - x1)) + x1, ((2f / 3) * (y - y1)) + y1,
                            //    x1, y1));

                            flattenEdges.Add(cnt.AddQuadraticSegment(latestX, latestY, x, y, x1, y1));

                            latestX = x1;
                            latestY = y1;

                        }
                        break;
                    case VertexCmd.C4:
                        {
                            //C4 curve (Cubic)
                            if (cnt == null)
                            {
                                cnt = new Contour();
                            }

                            VertexCmd cmd1 = vxs.GetVertex(i + 1, out double x2, out double y2);
                            VertexCmd cmd2 = vxs.GetVertex(i + 2, out double x3, out double y3);
                            i += 2;

                            if (cmd1 != VertexCmd.C4 || cmd2 != VertexCmd.LineTo)
                            {
                                throw new NotSupportedException();
                            }

                            flattenEdges.Add(cnt.AddCubicSegment(latestX, latestY, x, y, x2, y2, x3, y3));

                            latestX = x3;
                            latestY = y3;

                        }
                        break;
                    case VertexCmd.LineTo:
                        {
                            if (cnt == null)
                            {
                                cnt = new Contour();
                            }
                            LinearSegment lineseg = cnt.AddLine(latestX, latestY, x, y);
                            flattenEdges.Add(lineseg);

                            latestX = x;
                            latestY = y;
                        }
                        break;
                    case VertexCmd.MoveTo:
                        {
                            latestX = latestMoveToX = x;
                            latestY = latestMoveToY = y;
                            if (cnt != null)
                            {
                                shape.contours.Add(cnt);
                                cnt = null;
                            }
                        }
                        break;
                }
                i++;
            }

            if (cnt != null)
            {
                shape.contours.Add(cnt);
                CreateCorners(cnt, corners);
                edgeOfNextContours.Add(flattenEdges.Count);
                cornerOfNextContours.Add(corners.Count);
                cnt = null;
            }

            GroupingOverlapContours(shape);



            //from a given shape we create a corner-arm for each corner  
            bmpLut = new EdgeBmpLut(corners, flattenEdges, edgeOfNextContours, cornerOfNextContours);

            return shape;
        }
        static void GroupingOverlapContours(Shape shape)
        {

            //if (shape.contours.Count > 1)
            //{
            //    //group contour into intersect group
            //    List<Contour> contours = shape.contours;
            //    int n = contours.Count;

            //    RectD[] boundsList = new RectD[n];
            //    for (int i = 0; i < n; ++i)
            //    {
            //        Contour c = contours[i];
            //        boundsList[i] = c.GetRectBounds();
            //    }

            //    //collapse all connected rgn

            //    List<ConnectedContours> connectedCnts = new List<ConnectedContours>();

            //    for (int i = 1; i < n; ++i)
            //    {
            //        Contour c0 = contours[i - 1];
            //        Contour c1 = contours[i];
            //        RectD b0 = c0.GetRectBounds();
            //        RectD b1 = c1.GetRectBounds();
            //        if (b0.IntersectWithRectangle(b1))
            //        {
            //            //if yes then we create a map
            //            ConnectedContours connContours = new ConnectedContours();
            //            connContours._members.Add(c0);
            //            connContours._members.Add(c1);
            //            connectedCnts.Add(connContours);
            //            i++;
            //        }
            //    }

            //}
        }

    }
}