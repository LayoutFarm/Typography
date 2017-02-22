//Apache2, 2016-2017, WinterDev
//MIT, 2015, Michael Popoloski 
//FTL, 3-clauses BSD, FreeType project
//-----------------------------------------------------

using System;
using System.Numerics;

namespace Typography.OpenFont
{
    public interface IGlyphPathBuilder
    {   
        void BeginRead(int countourCount);
        void EndRead();
        void CloseFigure();
        void Curve3(float p2x, float p2y, float x, float y);
        void Curve4(float p2x, float p2y, float p3x, float p3y, float x, float y);
        void MoveTo(float x, float y);
        void LineTo(float x, float y); 
    }

    public static class IGlyphPathBuilderExtensions
    {
        public static void Read(this IGlyphPathBuilder r, GlyphPointF[] glyphPoints, ushort[] contourEndPoints)
        {
            //2. start build path
            int startContour = 0;
            int cpoint_index = 0;
            int todoContourCount = contourEndPoints.Length;
            //----------------------------------- 
            r.BeginRead(todoContourCount);
            //-----------------------------------
            float lastMoveX = 0;
            float lastMoveY = 0;
            int controlPointCount = 0;
            while (todoContourCount > 0)
            {
                int nextContour = contourEndPoints[startContour] + 1;
                bool isFirstPoint = true;
                Vector2 secondControlPoint = new Vector2();
                Vector2 thirdControlPoint = new Vector2();

                bool justFromCurveMode = false;
                for (; cpoint_index < nextContour; ++cpoint_index)
                {

                    GlyphPointF vpoint = glyphPoints[cpoint_index];
                    float vpoint_x = vpoint.P.X;
                    float vpoint_y = vpoint.P.Y;
                    //int vtag = (int)flags[cpoint_index] & 0x1;
                    //bool has_dropout = (((vtag >> 2) & 0x1) != 0);
                    //int dropoutMode = vtag >> 3;
                    if (vpoint.onCurve)
                    {
                        //on curve
                        if (justFromCurveMode)
                        {
                            switch (controlPointCount)
                            {
                                case 1:
                                    {
                                        r.Curve3(secondControlPoint.X, secondControlPoint.Y,
                                            vpoint_x, vpoint_y);
                                    }
                                    break;
                                case 2:
                                    {
                                        r.Curve4(secondControlPoint.X, secondControlPoint.Y,
                                             thirdControlPoint.X, thirdControlPoint.Y,
                                             vpoint_x, vpoint_y);
                                    }
                                    break;
                                default:
                                    {
                                        throw new NotSupportedException();
                                    }
                            }
                            controlPointCount = 0;
                            justFromCurveMode = false;
                        }
                        else
                        {
                            if (isFirstPoint)
                            {
                                isFirstPoint = false;
                                r.MoveTo(lastMoveX = (vpoint_x), lastMoveY = (vpoint_y));
                            }
                            else
                            {
                                r.LineTo(vpoint_x, vpoint_y);
                            }

                            //if (has_dropout)
                            //{
                            //    //printf("[%d] on,dropoutMode=%d: %d,y:%d \n", mm, dropoutMode, vpoint.x, vpoint.y);
                            //}
                            //else
                            //{
                            //    //printf("[%d] on,x: %d,y:%d \n", mm, vpoint.x, vpoint.y);
                            //}
                        }
                    }
                    else
                    {
                        switch (controlPointCount)
                        {
                            case 0:
                                {
                                    secondControlPoint = new Vector2(vpoint_x, vpoint_y);
                                }
                                break;
                            case 1:
                                {

                                    //we already have prev second control point
                                    //so auto calculate line to 
                                    //between 2 point
                                    Vector2 mid = GetMid(secondControlPoint, vpoint_x, vpoint_y);
                                    //----------
                                    //generate curve3
                                    r.Curve3(secondControlPoint.X, secondControlPoint.Y,
                                        mid.X, mid.Y);
                                    //------------------------
                                    controlPointCount--;
                                    //------------------------
                                    //printf("[%d] bzc2nd,  x: %d,y:%d \n", mm, vpoint.x, vpoint.y);
                                    secondControlPoint = new Vector2(vpoint_x, vpoint_y);

                                }
                                break;
                            default:
                                {
                                    throw new NotSupportedException();
                                }
                                break;
                        }

                        controlPointCount++;
                        justFromCurveMode = true;
                    }
                }
                //--------
                //close figure
                //if in curve mode
                if (justFromCurveMode)
                {
                    switch (controlPointCount)
                    {
                        case 0: break;
                        case 1:
                            {
                                r.Curve3(secondControlPoint.X, secondControlPoint.Y,
                                    lastMoveX, lastMoveY);
                            }
                            break;
                        case 2:
                            {
                                r.Curve4(secondControlPoint.X, secondControlPoint.Y,
                                    thirdControlPoint.X, thirdControlPoint.Y,
                                    lastMoveX, lastMoveY);
                            }
                            break;
                        default:
                            { throw new NotSupportedException(); }
                    }
                    justFromCurveMode = false;
                    controlPointCount = 0;
                }
                r.CloseFigure();
                //--------                   
                startContour++;
                todoContourCount--;
            }
            r.EndRead();
        }
        //----------------------------------------------------------------------------
        static Vector2 GetMid(Vector2 v1, float v2x, float v2y)
        {
            return new Vector2(
                ((v1.X + v2x) / 2f),
                 ((v1.Y + v2y) / 2f));
        }
    }

    //static int s_POINTS_PER_INCH = 72; //default value, 
    //static int s_PIXELS_PER_INCH = 96; //default value
    //public static float ConvEmSizeInPointsToPixels(float emsizeInPoint)
    //{
    //    return (int)(((float)emsizeInPoint / (float)s_POINTS_PER_INCH) * (float)s_PIXELS_PER_INCH);
    //}

    ////from http://www.w3schools.com/tags/ref_pxtoemconversion.asp
    ////set default
    //// 16px = 1 em
    ////-------------------
    ////1. conv font design unit to em
    //// em = designUnit / unit_per_Em       
    ////2. conv font design unit to pixels 
    //// float scale = (float)(size * resolution) / (pointsPerInch * _typeface.UnitsPerEm);

    ////-------------------
    ////https://www.microsoft.com/typography/otspec/TTCH01.htm
    ////Converting FUnits to pixels
    ////Values in the em square are converted to values in the pixel coordinate system by multiplying them by a scale. This scale is:
    ////pointSize * resolution / ( 72 points per inch * units_per_em )
    ////where pointSize is the size at which the glyph is to be displayed, and resolution is the resolution of the output device.
    ////The 72 in the denominator reflects the number of points per inch.
    ////For example, assume that a glyph feature is 550 FUnits in length on a 72 dpi screen at 18 point. 
    ////There are 2048 units per em. The following calculation reveals that the feature is 4.83 pixels long.
    ////550 * 18 * 72 / ( 72 * 2048 ) = 4.83
    ////-------------------
    //public static float ConvFUnitToPixels(ushort reqFUnit, float fontSizeInPoint, ushort unitPerEm)
    //{
    //    //reqFUnit * scale             
    //    return reqFUnit * GetFUnitToPixelsScale(fontSizeInPoint, unitPerEm);
    //}
    //public static float GetFUnitToPixelsScale(float fontSizeInPoint, ushort unitPerEm)
    //{
    //    //reqFUnit * scale             
    //    return ((fontSizeInPoint * s_PIXELS_PER_INCH) / (s_POINTS_PER_INCH * unitPerEm));
    //}



}