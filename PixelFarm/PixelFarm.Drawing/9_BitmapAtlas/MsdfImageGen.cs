//MIT, 2016-present, WinterDev
//-----------------------------------  
using System;
using System.Collections.Generic;
using PixelFarm.Contours;
namespace PixelFarm.CpuBlit.BitmapAtlas
{

    /// <summary>
    /// msdf image generator
    /// </summary>
    public static class MsdfImageGen
    {
        public static Msdfgen.Shape CreateMsdfShape(ContourBuilder contourBuilder, float pxScale)
        {
            List<Contour> cnts = contourBuilder.GetContours();
            List<Contour> newFitContours = new List<Contour>();
            int j = cnts.Count;
            for (int i = 0; i < j; ++i)
            {
                newFitContours.Add(
                    CreateFitContour(
                        cnts[i], pxScale, false, true));
            }
            return CreateMsdfShape(newFitContours);
        }


        static Msdfgen.Shape CreateMsdfShape(List<Contour> contours)
        {
            var shape = new Msdfgen.Shape();
            int j = contours.Count;
            for (int i = 0; i < j; ++i)
            {
                var cnt = new Msdfgen.Contour();
                shape.contours.Add(cnt);

                Contour contour = contours[i];
                List<ContourPart> parts = contour.parts;
                int m = parts.Count;
                for (int n = 0; n < m; ++n)
                {
                    ContourPart p = parts[n];
                    switch (p.Kind)
                    {
                        default: throw new NotSupportedException();
                        case PartKind.Curve3:
                            {
                                Curve3 curve3 = (Curve3)p;
                                cnt.AddQuadraticSegment(
                                    curve3.FirstPoint.X, curve3.FirstPoint.Y,
                                    curve3.x1, curve3.y1,
                                    curve3.x2, curve3.y2
                                   );
                            }
                            break;
                        case PartKind.Curve4:
                            {
                                Curve4 curve4 = (Curve4)p;
                                cnt.AddCubicSegment(
                                    curve4.FirstPoint.X, curve4.FirstPoint.Y,
                                    curve4.x1, curve4.y1,
                                    curve4.x2, curve4.y2,
                                    curve4.x3, curve4.y3);
                            }
                            break;
                        case PartKind.Line:
                            {
                                Line line = (Line)p;
                                cnt.AddLine(
                                    line.FirstPoint.X, line.FirstPoint.Y,
                                    line.x1, line.y1);
                            }
                            break;
                    }
                }
            }
            return shape;
        }
        static Contour CreateFitContour(Contour contour, float pixelScale, bool x_axis, bool y_axis)
        {
            Contour newc = new Contour();
            List<ContourPart> parts = contour.parts;
            int m = parts.Count;
            for (int n = 0; n < m; ++n)
            {
                ContourPart p = parts[n];
                switch (p.Kind)
                {
                    default: throw new NotSupportedException();
                    case PartKind.Curve3:
                        {
                            Curve3 curve3 = (Curve3)p;
                            newc.AddPart(new Curve3(
                                    curve3.FirstPoint.X * pixelScale, curve3.FirstPoint.Y * pixelScale,
                                    curve3.x1 * pixelScale, curve3.y1 * pixelScale,
                                    curve3.x2 * pixelScale, curve3.y2 * pixelScale));

                        }
                        break;
                    case PartKind.Curve4:
                        {
                            Curve4 curve4 = (Curve4)p;
                            newc.AddPart(new Curve4(
                                  curve4.FirstPoint.X * pixelScale, curve4.FirstPoint.Y * pixelScale,
                                  curve4.x1 * pixelScale, curve4.y1 * pixelScale,
                                  curve4.x2 * pixelScale, curve4.y2 * pixelScale,
                                  curve4.x3 * pixelScale, curve4.y3 * pixelScale
                                ));
                        }
                        break;
                    case PartKind.Line:
                        {
                            Line line = (Line)p;
                            newc.AddPart(new Line(
                                line.FirstPoint.X * pixelScale, line.FirstPoint.Y * pixelScale,
                                line.x1 * pixelScale, line.y1 * pixelScale
                                ));
                        }
                        break;
                }
            }
            return newc;
        }


        public static BitmapAtlasItem CreateMsdfImageV1(ContourBuilder contourBuilder, Msdfgen.MsdfGenParams genParams)
        {
            // create msdf shape , then convert to actual image
            Msdfgen.Shape shape = CreateMsdfShape(contourBuilder, genParams.shapeScale);
            //int w, int h, Vector2 translate
            Msdfgen.MsdfGen3.PreviewSizeAndLocation(shape, genParams, out int imgW, out int imgH, out Msdfgen.Vector2 translate);

            return Msdfgen.MsdfGen3.CreateMsdfImage(shape, genParams, imgW, imgH, translate, null);//output is msdf v1, since we set lut=null
        }
        public static BitmapAtlasItem CreateMsdfImageV1(Msdfgen.Shape shape, Msdfgen.MsdfGenParams genParams)
        {
            //output is msdf v1,
            //int w, int h, Vector2 translate
            Msdfgen.MsdfGen3.PreviewSizeAndLocation(shape, genParams, out int imgW, out int imgH, out Msdfgen.Vector2 translate);
            //output is msdf v1
            return Msdfgen.MsdfGen3.CreateMsdfImage(shape, genParams, imgW, imgH, translate, null);//output is msdf v1, since we set lut=null
        }
    }
}