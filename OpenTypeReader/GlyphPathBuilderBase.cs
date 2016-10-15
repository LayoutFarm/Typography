//-----------------------------------------------------
//Apache2, 2014-2016,   WinterDev
//some logics from FreeType Lib (FTL, BSD-3 clause)
//-----------------------------------------------------

using System;
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

        void RenderGlyph(ushort[] contours, short[] xs, short[] ys, bool[] onCurves)
        {

            //outline version
            //-----------------------------
            int npoints = xs.Length;
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

                    short vpoint_x = xs[cpoint_index];
                    short vpoint_y = ys[cpoint_index];
                    //int vtag = (int)flags[cpoint_index] & 0x1;
                    //bool has_dropout = (((vtag >> 2) & 0x1) != 0);
                    //int dropoutMode = vtag >> 3;
                    if (onCurves[cpoint_index])
                    {
                        //on curve
                        if (justFromCurveMode)
                        {
                            switch (controlPointCount)
                            {
                                case 1:
                                    {
                                        OnCurve3(secondControlPoint.x / FT_RESIZE, secondControlPoint.y / FT_RESIZE,
                                            vpoint_x / FT_RESIZE, vpoint_y / FT_RESIZE);
                                    }
                                    break;
                                case 2:
                                    {
                                        OnCurve4(secondControlPoint.x / FT_RESIZE, secondControlPoint.y / FT_RESIZE,
                                             thirdControlPoint.x / FT_RESIZE, thirdControlPoint.y / FT_RESIZE,
                                             vpoint_x / FT_RESIZE, vpoint_y / FT_RESIZE);
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
                                OnMoveTo(lastMoveX = (vpoint_x / FT_RESIZE), lastMoveY = (vpoint_y / FT_RESIZE));
                            }
                            else
                            {
                                OnLineTo(vpoint_x / FT_RESIZE, vpoint_y / FT_RESIZE);
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
                                    secondControlPoint = new FtPointD(vpoint_x, vpoint_y);
                                }
                                break;
                            case 1:
                                {

                                    //we already have prev second control point
                                    //so auto calculate line to 
                                    //between 2 point
                                    FtPointD mid = GetMidPoint(secondControlPoint, vpoint_x, vpoint_y);
                                    //----------
                                    //generate curve3
                                    OnCurve3(secondControlPoint.x / FT_RESIZE, secondControlPoint.y / FT_RESIZE,
                                        mid.x / FT_RESIZE, mid.y / FT_RESIZE);
                                    //------------------------
                                    controlPointCount--;
                                    //------------------------
                                    //printf("[%d] bzc2nd,  x: %d,y:%d \n", mm, vpoint.x, vpoint.y);
                                    secondControlPoint = new FtPointD(vpoint_x, vpoint_y);

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

        static FtPointD GetMidPoint(FtPointD v1, int v2x, int v2y)
        {
            return new FtPointD(
                ((double)v1.x + (double)v2x) / 2d,
                ((double)v1.y + (double)v2y) / 2d);
        }
        static FtPointD GetMidPoint(FtPointD v1, FtPointD v2)
        {
            return new FtPointD(
                ((double)v1.x + (double)v2.x) / 2d,
                ((double)v1.y + (double)v2.y) / 2d);
        }

        void RenderGlyph(Glyph glyph)
        {
            RenderGlyph(glyph.EndPoints, glyph.Xs, glyph.Ys, glyph.OnCurves);
        }

        public void Build(char c, int size, int resolution)
        {
            float scale = (float)(size * resolution) / (pointsPerInch * _typeface.UnitsPerEm);
            RenderGlyph(_typeface.Lookup(c));
        }
        public void BuildFromGlyphIndex(ushort glyphIndex, int size, int resolution)
        {
            float scale = (float)(size * resolution) / (pointsPerInch * _typeface.UnitsPerEm);
            RenderGlyph(_typeface.GetGlyphByIndex(glyphIndex));
        }
    }
}