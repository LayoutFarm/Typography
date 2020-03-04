//MIT, 2016-present, WinterDev
//-----------------------------------  
using System;
using System.Collections.Generic;
using PixelFarm.Contours;
namespace Typography.Rendering
{

    /// <summary>
    /// parameter for msdf generation
    /// </summary>
    public class MsdfGenParams
    {
        public float scaleX = 1;
        public float scaleY = 1;
        public float shapeScale = 1;
        public int minImgWidth = 5;
        public int minImgHeight = 5;

        public double angleThreshold = 3; //default
        public double pxRange = 4; //default
        public double edgeThreshold = 1.00000001;//default,(from original code)


        public MsdfGenParams()
        {

        }
        public void SetScale(float scaleX, float scaleY)
        {
            this.scaleX = scaleX;
            this.scaleY = scaleY;
        }


    }
    public static class MsdfGlyphGen
    {
        public static Msdfgen.Shape CreateMsdfShape(ContourBuilder glyphToContour, float pxScale)
        {
            List<Contour> cnts = glyphToContour.GetContours();
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
        //---------------------------------------------------------------------

        public static GlyphImage CreateMsdfImage(ContourBuilder glyphToContour, MsdfGenParams genParams)
        {
            // create msdf shape , then convert to actual image
            return CreateMsdfImage(CreateMsdfShape(glyphToContour, genParams.shapeScale), genParams);
        }

        const double MAX = 1e240;
        public static GlyphImage CreateMsdfImage(Msdfgen.Shape shape, MsdfGenParams genParams)
        {
            double left = MAX;
            double bottom = MAX;
            double right = -MAX;
            double top = -MAX;

            shape.findBounds(ref left, ref bottom, ref right, ref top);
            int w = (int)Math.Ceiling((right - left));
            int h = (int)Math.Ceiling((top - bottom));

            if (w < genParams.minImgWidth)
            {
                w = genParams.minImgWidth;
            }
            if (h < genParams.minImgHeight)
            {
                h = genParams.minImgHeight;
            }


            //temp, for debug with glyph 'I', tahoma font
            //double edgeThreshold = 1.00000001;//default, if edgeThreshold < 0 then  set  edgeThreshold=1 
            //Msdfgen.Vector2 scale = new Msdfgen.Vector2(0.98714652956298199, 0.98714652956298199);
            //double pxRange = 4;
            //translate = new Msdfgen.Vector2(12.552083333333332, 4.0520833333333330);
            //double range = pxRange / Math.Min(scale.x, scale.y);


            int borderW = (int)((float)w / 5f);

            //org
            //var translate = new Msdfgen.Vector2(left < 0 ? -left + borderW : borderW, bottom < 0 ? -bottom + borderW : borderW);
            //test
            var translate = new Msdfgen.Vector2(-left + borderW, -bottom + borderW);

            w += borderW * 2; //borders,left- right
            h += borderW * 2; //borders, top- bottom

            double edgeThreshold = genParams.edgeThreshold;
            if (edgeThreshold < 0)
            {
                edgeThreshold = 1.00000001; //use default if  edgeThreshold <0
            }

            var scale = new Msdfgen.Vector2(genParams.scaleX, genParams.scaleY); //scale               
            double range = genParams.pxRange / Math.Min(scale.x, scale.y);
            //---------
            Msdfgen.FloatRGBBmp frgbBmp = new Msdfgen.FloatRGBBmp(w, h);
            Msdfgen.EdgeColoring.edgeColoringSimple(shape, genParams.angleThreshold);
            Msdfgen.MsdfGenerator.generateMSDF(frgbBmp,
                shape,
                range,
                scale,
                translate,//translate to positive quadrant
                edgeThreshold);
            //-----------------------------------
            int[] buffer = Msdfgen.MsdfGenerator.ConvertToIntBmp(frgbBmp);

            GlyphImage img = new GlyphImage(w, h);
            img.TextureOffsetX = (short)translate.x; //TODO: review here, rounding err
            img.TextureOffsetY = (short)translate.y; //TODO: review here, rounding err
            img.SetImageBuffer(buffer, false);
            return img;
        }

    }
}