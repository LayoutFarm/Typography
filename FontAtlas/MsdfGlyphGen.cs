//MIT, 2016-2017, WinterDev
//-----------------------------------  
using System;
using System.Collections.Generic;


namespace Typography.Rendering
{
    public static class MsdfGlyphGen
    {
        public static Msdfgen.Shape CreateMsdfShape(GlyphTranslatorToContour glyphToContour, float pxScale = 1)
        {
            List<GlyphContour> cnts = glyphToContour.GetContours();
            List<GlyphContour> newFitContours = new List<GlyphContour>();
            int j = cnts.Count;
            for (int i = 0; i < j; ++i)
            {
                newFitContours.Add(
                    CreateFitContour(
                        cnts[i], pxScale, false, true));
            }
            return CreateMsdfShape(newFitContours);
        }


        static Msdfgen.Shape CreateMsdfShape(List<GlyphContour> contours)
        {
            var shape = new Msdfgen.Shape();
            int j = contours.Count;
            for (int i = 0; i < j; ++i)
            {
                var cnt = new Msdfgen.Contour();
                shape.contours.Add(cnt);

                GlyphContour contour = contours[i];
                List<GlyphPart> parts = contour.parts;
                int m = parts.Count;
                for (int n = 0; n < m; ++n)
                {
                    GlyphPart p = parts[n];
                    switch (p.Kind)
                    {
                        default: throw new NotSupportedException();
                        case GlyphPartKind.Curve3:
                            {
                                GlyphCurve3 curve3 = (GlyphCurve3)p;
                                cnt.AddQuadraticSegment(
                                    curve3.x0, curve3.y0,
                                    curve3.p2x, curve3.p2y,
                                    curve3.x, curve3.y
                                   );
                            }
                            break;
                        case GlyphPartKind.Curve4:
                            {
                                GlyphCurve4 curve4 = (GlyphCurve4)p;
                                cnt.AddCubicSegment(
                                    curve4.x0, curve4.y0,
                                    curve4.p2x, curve4.p2y,
                                    curve4.p3x, curve4.p3y,
                                    curve4.x, curve4.y);
                            }
                            break;
                        case GlyphPartKind.Line:
                            {
                                GlyphLine line = (GlyphLine)p;
                                cnt.AddLine(
                                    line.x0, line.y0,
                                    line.x1, line.y1);
                            }
                            break;
                    }
                }
            }
            return shape;
        }
        static GlyphContour CreateFitContour(GlyphContour contour, float pixelScale, bool x_axis, bool y_axis)
        {
            GlyphContour newc = new GlyphContour();
            List<GlyphPart> parts = contour.parts;
            int m = parts.Count;
            for (int n = 0; n < m; ++n)
            {
                GlyphPart p = parts[n];
                switch (p.Kind)
                {
                    default: throw new NotSupportedException();
                    case GlyphPartKind.Curve3:
                        {
                            GlyphCurve3 curve3 = (GlyphCurve3)p;
                            newc.AddPart(new GlyphCurve3(
                                curve3.x0 * pixelScale, curve3.y0 * pixelScale,
                                curve3.p2x * pixelScale, curve3.p2y * pixelScale,
                                curve3.x * pixelScale, curve3.y * pixelScale));

                        }
                        break;
                    case GlyphPartKind.Curve4:
                        {
                            GlyphCurve4 curve4 = (GlyphCurve4)p;
                            newc.AddPart(new GlyphCurve4(
                                  curve4.x0 * pixelScale, curve4.y0 * pixelScale,
                                  curve4.p2x * pixelScale, curve4.p2y * pixelScale,
                                  curve4.p3x * pixelScale, curve4.p3y * pixelScale,
                                  curve4.x * pixelScale, curve4.y * pixelScale
                                ));
                        }
                        break;
                    case GlyphPartKind.Line:
                        {
                            GlyphLine line = (GlyphLine)p;
                            newc.AddPart(new GlyphLine(
                                line.x0 * pixelScale, line.y0 * pixelScale,
                                line.x1 * pixelScale, line.y1 * pixelScale
                                ));
                        }
                        break;
                }
            }
            return newc;
        }
        //---------------------------------------------------------------------

        public static GlyphImage CreateMsdfImage(
             GlyphTranslatorToContour glyphToContour)
        {
            // create msdf shape , then convert to actual image
            return CreateMsdfImage(CreateMsdfShape(glyphToContour, 1));
        }
        public static GlyphImage CreateMsdfImage(Msdfgen.Shape shape)
        {
            double left, bottom, right, top;
            shape.findBounds(out left, out bottom, out right, out top);
            int w = (int)Math.Ceiling((right - left));
            int h = (int)Math.Ceiling((top - bottom));
            if (w < 5)
            {
                w = 5;
            }
            if (h < 5)
            {
                h = 5;
            }


            int borderW = (int)((float)w / 5f);
            var translate = new Msdfgen.Vector2(left < 0 ? -left + borderW : borderW, bottom < 0 ? -bottom + borderW : borderW);
            w += borderW * 2; //borders,left- right
            h += borderW * 2; //borders, top- bottom



            Msdfgen.FloatRGBBmp frgbBmp = new Msdfgen.FloatRGBBmp(w, h);
            Msdfgen.EdgeColoring.edgeColoringSimple(shape, 3);


            Msdfgen.MsdfGenerator.generateMSDF(frgbBmp,
                shape,
                4,
                new Msdfgen.Vector2(1, 1), //scale                 
                translate,//translate to positive quadrant
                -1);
            //-----------------------------------
            int[] buffer = Msdfgen.MsdfGenerator.ConvertToIntBmp(frgbBmp);

            GlyphImage img = new GlyphImage(w, h);
            img.TextureOffsetX = translate.x;
            img.TextureOffsetY = translate.y;
            img.SetImageBuffer(buffer, false);
            return img;
        }

    }
}