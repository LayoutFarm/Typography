//Apache2, 2014-2016,   WinterDev
using System;
using System.Collections.Generic;

namespace NRasterizer
{
    public abstract class GlyphPathBuilderBase
    {
        readonly Typeface _typeface;
        const int pointsPerInch = 72;

        public GlyphPathBuilderBase(Typeface typeface)
        {
            _typeface = typeface;
        }
        const double FT_RESIZE = 64; //essential to be floating point

        protected abstract void OnBeginRead(int countourCount);
        protected abstract void OnEndRead();
        protected abstract void OnCloseFigure();
        protected abstract void OnCurve3(double p2x, double p2y, double x, double y);
        protected abstract void OnCurve4(double p2x, double p2y, double p3x, double p3y, double x, double y);
        protected abstract void OnMoveTo(double x, double y);
        protected abstract void OnLineTo(double x, double y);


        void RenderGlyph(ushort[] contours, FtPoint[] ftpoints, Flag[] flags)
        {

            //outline version
            //-----------------------------
            int npoints = ftpoints.Length;
            int startContour = 0;
            int cpoint_index = 0;
            int todoContourCount = contours.Length;
            //-----------------------------------
            OnBeginRead(todoContourCount);
            //-----------------------------------
            double lastMoveX = 0;
            double lastMoveY = 0;

            int controlPointCount = 0;
            while (todoContourCount > 0)
            {
                int nextContour = contours[startContour] + 1;
                bool isFirstPoint = true;
                FtPointD secondControlPoint = new FtPointD();
                FtPointD thirdControlPoint = new FtPointD();
                bool justFromCurveMode = false;

                for (; cpoint_index < nextContour; ++cpoint_index)
                {
                    FtPoint vpoint = ftpoints[cpoint_index];
                    int vtag = (int)flags[cpoint_index] & 0x1;
                    //bool has_dropout = (((vtag >> 2) & 0x1) != 0);
                    //int dropoutMode = vtag >> 3;
                    if ((vtag & 0x1) != 0)
                    {
                        //on curve
                        if (justFromCurveMode)
                        {
                            switch (controlPointCount)
                            {
                                case 1:
                                    {
                                        OnCurve3(secondControlPoint.x / FT_RESIZE, secondControlPoint.y / FT_RESIZE,
                                            vpoint.X / FT_RESIZE, vpoint.Y / FT_RESIZE);
                                    }
                                    break;
                                case 2:
                                    {
                                        OnCurve4(secondControlPoint.x / FT_RESIZE, secondControlPoint.y / FT_RESIZE,
                                           thirdControlPoint.x / FT_RESIZE, thirdControlPoint.y / FT_RESIZE,
                                           vpoint.X / FT_RESIZE, vpoint.Y / FT_RESIZE);
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
                                OnMoveTo(lastMoveX = (vpoint.X / FT_RESIZE), lastMoveY = (vpoint.Y / FT_RESIZE));
                            }
                            else
                            {
                                OnLineTo(vpoint.X / FT_RESIZE, vpoint.Y / FT_RESIZE);
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
                                {   //bit 1 set=> off curve, this is a control point
                                    //if this is a 2nd order or 3rd order control point
                                    if (((vtag >> 1) & 0x1) != 0)
                                    {
                                        //printf("[%d] bzc3rd,  x: %d,y:%d \n", mm, vpoint.x, vpoint.y);
                                        thirdControlPoint = new FtPointD(vpoint);
                                    }
                                    else
                                    {
                                        //printf("[%d] bzc2nd,  x: %d,y:%d \n", mm, vpoint.x, vpoint.y);
                                        secondControlPoint = new FtPointD(vpoint);
                                    }
                                }
                                break;
                            case 1:
                                {
                                    if (((vtag >> 1) & 0x1) != 0)
                                    {
                                        //printf("[%d] bzc3rd,  x: %d,y:%d \n", mm, vpoint.x, vpoint.y);
                                        thirdControlPoint = new FtPointD(vpoint.X, vpoint.Y);
                                    }
                                    else
                                    {
                                        //we already have prev second control point
                                        //so auto calculate line to 
                                        //between 2 point
                                        FtPointD mid = GetMidPoint(secondControlPoint, vpoint);
                                        //----------
                                        //generate curve3
                                        OnCurve3(secondControlPoint.x / FT_RESIZE, secondControlPoint.y / FT_RESIZE,
                                            mid.x / FT_RESIZE, mid.y / FT_RESIZE);
                                        //------------------------
                                        controlPointCount--;
                                        //------------------------
                                        //printf("[%d] bzc2nd,  x: %d,y:%d \n", mm, vpoint.x, vpoint.y);
                                        secondControlPoint = new FtPointD(vpoint);
                                    }
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
                                OnCurve3(secondControlPoint.x / FT_RESIZE, secondControlPoint.y / FT_RESIZE,
                                  lastMoveX, lastMoveY);
                            }
                            break;
                        case 2:
                            {
                                OnCurve4(secondControlPoint.x / FT_RESIZE, secondControlPoint.y / FT_RESIZE,
                                   thirdControlPoint.x / FT_RESIZE, thirdControlPoint.y / FT_RESIZE,
                                  lastMoveX, lastMoveY);
                            }
                            break;
                        default:
                            { throw new NotSupportedException(); }
                    }
                    justFromCurveMode = false;
                    controlPointCount = 0;
                }
                OnCloseFigure();
                //--------                   
                startContour++;
                todoContourCount--;
            }
            OnEndRead();
        }
        static FtPointD GetMidPoint(FtPoint v1, FtPoint v2)
        {
            return new FtPointD(
                ((double)v1.X + (double)v2.X) / 2d,
                ((double)v1.Y + (double)v2.Y) / 2d);
        }
        static FtPointD GetMidPoint(FtPointD v1, FtPointD v2)
        {
            return new FtPointD(
                ((double)v1.x + (double)v2.x) / 2d,
                ((double)v1.y + (double)v2.y) / 2d);
        }
        static FtPointD GetMidPoint(FtPointD v1, FtPoint v2)
        {
            return new FtPointD(
                (v1.x + (double)v2.X) / 2d,
                (v1.y + (double)v2.Y) / 2d);
        }

        void RenderGlyph(Glyph glyph)
        {
            ushort[] endPoints;
            Flag[] flags;
            FtPoint[] ftpoints = glyph.GetPoints(out endPoints, out flags);
            RenderGlyph(endPoints, ftpoints, flags);
        }

        public void Build(char c, int size, int resolution)
        {
            float scale = (float)(size * resolution) / (pointsPerInch * _typeface.UnitsPerEm);
            RenderGlyph(_typeface.Lookup(c));
        }
    }

}