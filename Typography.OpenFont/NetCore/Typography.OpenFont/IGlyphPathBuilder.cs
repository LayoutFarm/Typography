//Apache2, 2016-2017, WinterDev
//MIT, 2015, Michael Popoloski 
//FTL, 3-clauses BSD, FreeType project
//-----------------------------------------------------

using System;
using System.Numerics;

namespace Typography.OpenFont
{
    //https://en.wikipedia.org/wiki/B%C3%A9zier_curve
    //--------------------
    //Line, has 2 points..
    //  (x0,y0) begin point
    //  (x1,y1) end point
    //--------------------
    //Curve3 (Quadratic Bézier curves), has 3 points
    //  (x0,y0)  begin point
    //  (x1,y1)  1st control point 
    //  (x2,y2)  end point
    //--------------------
    //Curve4 (Cubic  Bézier curves), has 4 points
    //  (x0,y0)  begin point
    //  (x1,y1)  1st control point 
    //  (x2,y2)  2nd control point
    //  (x3,y3)  end point    
    //-------------------- 
    //please note that TrueType font
    //compose of Quadractic Bezier Curve ***
    //--------------------- 
    public interface IGlyphPathBuilder
    {
        /// <summary>
        /// begin read a glyph
        /// </summary>
        /// <param name="countourCount"></param>
        void BeginRead(int countourCount);
        /// <summary>
        /// end read a glyph
        /// </summary>
        void EndRead();
        /// <summary>
        /// set CURRENT pen position to (x0,y0) And set the position as lastest MOVETO position
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="y0"></param>
        void MoveTo(float x0, float y0);
        /// <summary>
        /// add line,begin from CURRENT pen position to (x1,y1) then set (x1,y1) as CURRENT pen position
        /// </summary>
        /// <param name="x1">end point x</param>
        /// <param name="y1">end point y</param>
        void LineTo(float x1, float y1);
        /// <summary>
        /// add Quadratic Bézier curve,begin from CURRENT pen pos, to (x2,y2), then set (x2,y2) as CURRENT pen pos
        /// </summary>
        /// <param name="x1">x of 1st control point</param>
        /// <param name="y1">y of 1st control point</param>
        /// <param name="x2">end point x</param>
        /// <param name="y2">end point y</param>
        void Curve3(float x1, float y1, float x2, float y2);
        /// <summary>
        /// add Quadratic Bézier curve,begin from CURRENT pen pos, to (x3,y3), then set (x3,y3) as CURRENT pen pos
        /// </summary>
        /// <param name="x1">x of 1st control point</param>
        /// <param name="y1">y of 1st control point</param>
        /// <param name="x2">x of 2nd control point</param>
        /// <param name="y2">y of 2dn control point</param>
        /// <param name="x3">end point x</param>
        /// <param name="y3">end point y</param>
        void Curve4(float x1, float y1, float x2, float y2, float x3, float y3);

        /// <summary>
        /// close current contour, create line from CURRENT pen position to lastest MOVETO position
        /// </summary>
        void CloseContour();
    }

    public static class IGlyphPathBuilderExtensions
    {
        public static void Read(this IGlyphPathBuilder b, GlyphPointF[] glyphPoints, ushort[] contourEndPoints)
        {

            int startContour = 0;
            int cpoint_index = 0;//current point index

            int todoContourCount = contourEndPoints.Length;
            //----------------------------------- 
            //1. start read data from a glyph
            b.BeginRead(todoContourCount);
            //-----------------------------------
            float lastMoveX = 0;
            float lastMoveY = 0;

            int curveControlPointCount = 0; // 1 curve control point => Quadratic, 2 curve control points => Cubic


            while (todoContourCount > 0)
            {
                //foreach contour...

                //next contour will begin at...
                int nextCntBeginAtIndex = contourEndPoints[startContour] + 1;

                //reset  ...
                Vector2 c1 = new Vector2();
                Vector2 c2 = new Vector2();
                bool curveMode = false;
                bool isFirstPoint = true;  //first point of this contour


                ///for each point in this contour
                for (; cpoint_index < nextCntBeginAtIndex; ++cpoint_index)
                {

                    GlyphPointF p = glyphPoints[cpoint_index];
                    float p_x = p.X;
                    float p_y = p.Y;

                    //int vtag = (int)flags[cpoint_index] & 0x1;
                    //bool has_dropout = (((vtag >> 2) & 0x1) != 0);
                    //int dropoutMode = vtag >> 3;

                    if (p.onCurve)
                    {
                        //point p is an on-curve point (on outline). (not curve control point)
                        //possible ways..
                        //1. if we are in curve mode, then p is end point
                        //   we must decide which curve to create (Curve3 or Curve4)
                        //   easy, ... 
                        //      if  curveControlPointCount == 1 , then create Curve3
                        //      else curveControlPointCount ==2 , then create Curve4
                        //2. if we are NOT in curve mode, 
                        //      if p is first point then set this to x0,y0
                        //      else then p is end point of a line.


                        if (curveMode)
                        {
                            switch (curveControlPointCount)
                            {
                                case 1:
                                    {
                                        b.Curve3(
                                            c1.X, c1.Y,
                                            p_x, p_y);
                                    }
                                    break;
                                case 2:
                                    {
                                        //for TrueType font 
                                        //we should not be here?

                                        b.Curve4(
                                             c1.X, c1.Y,
                                             c2.X, c2.Y,
                                             p_x, p_y);
                                    }
                                    break;
                                default:
                                    {
                                        throw new NotSupportedException();
                                    }
                            }

                            //reset curve control point count
                            curveControlPointCount = 0;
                            //exist from curve mode
                            curveMode = false;
                        }
                        else
                        {
                            //as describe above,...

                            if (isFirstPoint)
                            {
                                isFirstPoint = false;
                                b.MoveTo(lastMoveX = (p_x), lastMoveY = (p_y));
                            }
                            else
                            {
                                b.LineTo(p_x, p_y);
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
                        //this point is curve control point***
                        //so set curve mode = true
                        curveMode = true;
                        //check number if existing curve control 

                        switch (curveControlPointCount)
                        {

                            case 0:
                                c1 = new Vector2(p_x, p_y);
                                //this point may be part 1st control point of a curve,
                                //store it and wait for next point before make decision *** 
                                break;
                            case 1:
                                //we already have previous 1st control point (c1)
                                //------------------------------------- 
                                //please note that TrueType font
                                //compose of Quadractic Bezier Curve (Curve3) *** 
                                //------------------------------------- 
                                //in this case, this is NOT Cubic,
                                //this is 2 CONNECTED Quadractic Bezier Curves***
                                //
                                //we must create 'end point' of the first curve
                                //and set it as 'begin point of the second curve.
                                //
                                //this is done by ...
                                //1. calculate mid point between c1 and the latest point (p_x,p_y)
                                Vector2 mid = GetMid(c1, p_x, p_y);
                                //----------
                                //2. generate curve3 ***
                                b.Curve3(
                                    c1.X, c1.Y,
                                    mid.X, mid.Y);
                                //------------------------
                                //3. so curve control point number is reduce by 1***
                                curveControlPointCount--;
                                //------------------------
                                //4. and set (p_x,p_y) as 1st control point for the new curve
                                c1 = new Vector2(p_x, p_y);
                                //
                                //printf("[%d] bzc2nd,  x: %d,y:%d \n", mm, vpoint.x, vpoint.y); 
                                break;
                            default:
                                throw new NotSupportedException();
                        }
                        //count
                        curveControlPointCount++;
                    }
                }
                //--------
                //when finish,                 
                //ensure that the contour is closed.
                if (curveMode)
                {
                    switch (curveControlPointCount)
                    {
                        case 0: break;
                        case 1:
                            {
                                b.Curve3(c1.X, c1.Y,
                                    lastMoveX, lastMoveY);
                            }
                            break;
                        case 2:
                            {
                                //for TrueType font 
                                //we should not be here? 
                                b.Curve4(c1.X, c1.Y,
                                    c2.X, c2.Y,
                                    lastMoveX, lastMoveY);
                            }
                            break;
                        default:
                            { throw new NotSupportedException(); }
                    }
                    //reset
                    curveMode = false;
                    curveControlPointCount = 0;
                }
                //--------      
                b.CloseContour(); //***                            
                startContour++;
                todoContourCount--;
                //--------      
            }
            //finish
            b.EndRead();
        }

        static Vector2 GetMid(Vector2 v0, float x1, float y1)
        {
            //mid point between v0 and (x1,y1)
            return new Vector2(
                ((v0.X + x1) / 2f),
                ((v0.Y + y1) / 2f));
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