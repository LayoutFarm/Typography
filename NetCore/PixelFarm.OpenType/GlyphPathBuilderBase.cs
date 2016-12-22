//-----------------------------------------------------
//Apache2, 2014-2016,   WinterDev
//some logics from FreeType Lib (FTL, BSD-3 clause)
//-----------------------------------------------------

using System;
namespace NOpenType
{
    public abstract class GlyphPathBuilderBase
    {
        readonly Typeface _typeface;
        public GlyphPathBuilderBase(Typeface typeface)
        {
            _typeface = typeface;
        }
        struct FtPoint
        {
            readonly short _x;
            readonly short _y;
            public FtPoint(short x, short y)
            {
                _x = x;
                _y = y;
            }
            public short X { get { return _x; } }
            public short Y { get { return _y; } }

            public override string ToString()
            {
                return "(" + _x + "," + _y + ")";
            }
        }
        struct FtPointF
        {
            readonly float _x;
            readonly float _y;
            public FtPointF(float x, float y)
            {
                _x = x;
                _y = y;
            }
            public float X { get { return _x; } }
            public float Y { get { return _y; } }

            public override string ToString()
            {
                return "(" + _x + "," + _y + ")";
            }
        }
        protected abstract void OnBeginRead(int countourCount);
        protected abstract void OnEndRead();
        protected abstract void OnCloseFigure();
        protected abstract void OnCurve3(float p2x, float p2y, float x, float y);
        protected abstract void OnCurve4(float p2x, float p2y, float p3x, float p3y, float x, float y);
        protected abstract void OnMoveTo(float x, float y);
        protected abstract void OnLineTo(float x, float y);

        void RenderGlyph(ushort[] contours, GlyphPointF[] glyphPoints)
        {

            //outline version
            //-----------------------------
            int npoints = glyphPoints.Length;
            int startContour = 0;
            int cpoint_index = 0;
            int todoContourCount = contours.Length;
            //----------------------------------- 
            OnBeginRead(todoContourCount);
            //-----------------------------------
            float lastMoveX = 0;
            float lastMoveY = 0;


            int controlPointCount = 0;
            while (todoContourCount > 0)
            {
                int nextContour = contours[startContour] + 1;
                bool isFirstPoint = true;
                FtPointF secondControlPoint = new FtPointF();
                FtPointF thirdControlPoint = new FtPointF();

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
                                        OnCurve3(secondControlPoint.X, secondControlPoint.Y,
                                            vpoint_x, vpoint_y);
                                    }
                                    break;
                                case 2:
                                    {
                                        OnCurve4(secondControlPoint.X, secondControlPoint.Y,
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
                                OnMoveTo(lastMoveX = (vpoint_x), lastMoveY = (vpoint_y));
                            }
                            else
                            {
                                OnLineTo(vpoint_x, vpoint_y);
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
                                    secondControlPoint = new FtPointF(vpoint_x, vpoint_y);
                                }
                                break;
                            case 1:
                                {

                                    //we already have prev second control point
                                    //so auto calculate line to 
                                    //between 2 point
                                    FtPointF mid = GetMidPointF(secondControlPoint, vpoint_x, vpoint_y);
                                    //----------
                                    //generate curve3
                                    OnCurve3(secondControlPoint.X, secondControlPoint.Y,
                                        mid.X, mid.Y);
                                    //------------------------
                                    controlPointCount--;
                                    //------------------------
                                    //printf("[%d] bzc2nd,  x: %d,y:%d \n", mm, vpoint.x, vpoint.y);
                                    secondControlPoint = new FtPointF(vpoint_x, vpoint_y);

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
                                OnCurve3(secondControlPoint.X, secondControlPoint.Y,
                                    lastMoveX, lastMoveY);
                            }
                            break;
                        case 2:
                            {
                                OnCurve4(secondControlPoint.X, secondControlPoint.Y,
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
                OnCloseFigure();
                //--------                   
                startContour++;
                todoContourCount--;
            }
            OnEndRead();

        }

        static FtPoint GetMidPoint(FtPoint v1, short v2x, short v2y)
        {
            return new FtPoint(
                (short)((v1.X + v2x) >> 1),
                (short)((v1.Y + v2y) >> 1));
        }
        static FtPointF GetMidPointF(FtPointF v1, float v2x, float v2y)
        {
            return new FtPointF(
                ((v1.X + v2x) / 2),
                 ((v1.Y + v2y) / 2));
        }
        void RenderGlyph(Glyph glyph)
        {
            RenderGlyph(glyph.EndPoints, glyph.GlyphPoints);
        }

        public void Build(char c, float sizeInPoints)
        {
            BuildFromGlyphIndex((ushort)_typeface.LookupIndex(c), sizeInPoints);
        }
        public void BuildFromGlyphIndex(ushort glyphIndex, float sizeInPoints)
        {
            this.SizeInPoints = sizeInPoints;

            RenderGlyph(_typeface.GetGlyphByIndex(glyphIndex));
        }
        public float SizeInPoints
        {
            get;
            private set;
        }
        protected Typeface TypeFace
        {
            get { return _typeface; }
        }
        protected ushort TypeFaceUnitPerEm
        {
            get { return _typeface.UnitsPerEm; }
        }

    }
}