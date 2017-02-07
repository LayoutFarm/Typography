//Apache2, 2016-2017, WinterDev
//MIT, 2015, Michael Popoloski 
//FTL, 3-clauses BSD, FreeType project
//-----------------------------------------------------

using System;
namespace NOpenType
{
    public abstract class GlyphPathBuilderBase
    {
        readonly Typeface _typeface;
        SharpFont.Interpreter _interpreter;
        bool _useInterpreter;
        bool _passInterpreterModule;
        public GlyphPathBuilderBase(Typeface typeface)
        {
            _typeface = typeface;
            this.UseTrueTypeInterpreter = true;//default?
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

        /// <summary>
        /// use Maxim's Agg Vertical Hinting
        /// </summary>
        public bool UseVerticalHinting { get; set; }

        public bool UseTrueTypeInterpreter
        {
            get { return _useInterpreter; }
            set
            {

                _useInterpreter = value;
                if (value && _interpreter == null)
                {
                    //we can init it later
                    NOpenType.Typeface currentTypeFace = this.TypeFace;
                    Tables.MaxProfile maximumProfile = currentTypeFace.MaxProfile;
                    _interpreter = new SharpFont.Interpreter(
                        maximumProfile.MaxStackElements,
                        maximumProfile.MaxStorage,
                        maximumProfile.MaxFunctionDefs,
                        maximumProfile.MaxInstructionDefs,
                        maximumProfile.MaxTwilightPoints);
                    // the fpgm table optionally contains a program to run at initialization time 
                    if (currentTypeFace.FpgmProgramBuffer != null)
                    {
                        _interpreter.InitializeFunctionDefs(currentTypeFace.FpgmProgramBuffer);
                    }
                }
            }
        }
        protected bool PassHintInterpreterModule
        {
            get { return this._passInterpreterModule; }
        }
        protected abstract void OnBeginRead(int countourCount);
        protected abstract void OnEndRead();
        protected abstract void OnCloseFigure();
        protected abstract void OnCurve3(float p2x, float p2y, float x, float y);
        protected abstract void OnCurve4(float p2x, float p2y, float p3x, float p3y, float x, float y);
        protected abstract void OnMoveTo(float x, float y);
        protected abstract void OnLineTo(float x, float y);


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
        static void ApplyScaleOnlyOnXAxis(GlyphPointF[] glyphPoints, float xscale)
        {

            for (int i = glyphPoints.Length - 1; i >= 0; --i)
            {
                glyphPoints[i].ApplyScaleOnlyOnXAxis(xscale);
            }

        }
        void RenderGlyph(ushort glyphIndex, Glyph glyph)
        {
            //-------------------------------------------
            GlyphPointF[] glyphPoints = glyph.GlyphPoints;
            ushort[] contourEndPoints = glyph.EndPoints;
            //-------------------------------------------
            _passInterpreterModule = false;
            int npoints = glyphPoints.Length;

            Typeface currentTypeFace = this.TypeFace;
            if (UseTrueTypeInterpreter &&
                currentTypeFace.PrepProgramBuffer != null &&
                glyph.GlyphInstructions != null)
            {

                //the true type hint logics come from Michael Popoloski 's SharpFont project.
#if DEBUG
                GlyphPointF[] backupGlyphPoints = glyphPoints;
#endif
                //1. use a clone version           
                int orgLen = glyphPoints.Length;
                GlyphPointF[] newGlyphPoints = Utils.CloneArray(glyphPoints, 4); //extend org with 4 elems
                //2. scale
                float scaleFactor = currentTypeFace.CalculateScale(SizeInPoints);
                for (int i = orgLen - 1; i >= 0; --i)
                {
                    newGlyphPoints[i].ApplyScale(scaleFactor);
                }

                // add phantom points; these are used to define the extents of the glyph,
                // and can be modified by hinting instructions

                int horizontalAdv = currentTypeFace.GetHAdvanceWidthFromGlyphIndex(glyphIndex);
                int hFrontSideBearing = currentTypeFace.GetHFrontSideBearingFromGlyphIndex(glyphIndex);
                int verticalAdv = 0;
                int vFrontSideBearing = 0;

                //-------------------------
                //TODO: review here again
                var pp1 = new GlyphPointF((glyph.MinX - hFrontSideBearing), 0, true);
                var pp2 = new GlyphPointF(pp1.X + horizontalAdv, 0, true);
                var pp3 = new GlyphPointF(0, glyph.MaxY + vFrontSideBearing, true);
                var pp4 = new GlyphPointF(0, pp3.Y - verticalAdv, true);
                //-------------------------
                newGlyphPoints[orgLen] = (pp1 * scaleFactor);
                newGlyphPoints[orgLen + 1] = (pp2 * scaleFactor);
                newGlyphPoints[orgLen + 2] = (pp3 * scaleFactor);
                newGlyphPoints[orgLen + 3] = (pp4 * scaleFactor);
                //----------------------------------------------
                //test : agg's vertical hint
                //apply large scale on horizontal axis only 
                //translate and then scale back

                float agg_x_scale = 1000;
                if (UseVerticalHinting)
                {
                    ApplyScaleOnlyOnXAxis(newGlyphPoints, agg_x_scale);
                }

                //3. 
                float sizeInPixels = Typeface.ConvPointsToPixels(SizeInPoints);
                _interpreter.SetControlValueTable(currentTypeFace.ControlValues,
                    scaleFactor,
                    sizeInPixels,
                    currentTypeFace.PrepProgramBuffer);
                //then hint
                _interpreter.HintGlyph(newGlyphPoints, contourEndPoints, glyph.GlyphInstructions);

                //scale back
                if (UseVerticalHinting)
                {
                    ApplyScaleOnlyOnXAxis(newGlyphPoints, 1f / agg_x_scale);
                }

                glyphPoints = newGlyphPoints;
                _passInterpreterModule = true;
            }


            int startContour = 0;
            int cpoint_index = 0;
            int todoContourCount = contourEndPoints.Length;
            //----------------------------------- 
            OnBeginRead(todoContourCount);
            //-----------------------------------
            float lastMoveX = 0;
            float lastMoveY = 0;
            int controlPointCount = 0;
            while (todoContourCount > 0)
            {
                int nextContour = contourEndPoints[startContour] + 1;
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

        public void Build(char c, float sizeInPoints)
        {
            BuildFromGlyphIndex((ushort)_typeface.LookupIndex(c), sizeInPoints);
        }
        public void BuildFromGlyphIndex(ushort glyphIndex, float sizeInPoints)
        {
            this.SizeInPoints = sizeInPoints;

            RenderGlyph(glyphIndex, _typeface.GetGlyphByIndex(glyphIndex));
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