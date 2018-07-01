//MIT, 2016-present, WinterDev
using System;
using System.Collections.Generic;
using Typography.OpenFont;

namespace Typography.TextLayout
{
    /// <summary>
    /// scaled glyph plan to specfic font size.
    /// offsetX,offsetY,advanceX are adjusted to fit with specific font size    
    /// </summary>
    public struct PxScaledGlyphPlan
    {
        public readonly ushort input_cp_offset;
        public readonly ushort glyphIndex;
        public PxScaledGlyphPlan(ushort input_cp_offset, ushort glyphIndex, float advanceW, float offsetX, float offsetY)
        {
            this.input_cp_offset = input_cp_offset;
            this.glyphIndex = glyphIndex;
            this.OffsetX = offsetX;
            this.OffsetY = offsetY;
            this.AdvanceX = advanceW;
        }
        public float AdvanceX { get; private set; }
        /// <summary>
        /// x offset from current position
        /// </summary>
        public float OffsetX { get; private set; }
        /// <summary>
        /// y offset from current position
        /// </summary>
        public float OffsetY { get; private set; }

        public bool AdvanceMoveForward { get { return this.AdvanceX > 0; } }

#if DEBUG
        public override string ToString()
        {
            return " adv:" + AdvanceX;
        }
#endif
    }


    /// <summary>
    /// scaled glyph plan 
    /// </summary>
    public struct GlyphPlanSequencePixelScaleLayout
    {

        GlyphPlanSequence _seq;
        float _pxscale;
        float _accW;
        int _index;
        int _end;

        float _exactX;
        float _exactY;

        ushort _currentGlyphIndex;
        public GlyphPlanSequencePixelScaleLayout(GlyphPlanSequence glyphPlans, float pxscale)
        {
            _seq = glyphPlans;
            _pxscale = pxscale;
            _accW = 0;
            _index = glyphPlans.startAt;
            _end = glyphPlans.startAt + glyphPlans.len;
            _exactX = _exactY = 0;
            _currentGlyphIndex = 0;
        }
        public int CurrentIndex { get { return _index; } }
        public PxScaledGlyphPlan GlyphPlan
        {
            get
            {
                UnscaledGlyphPlan unscale = _seq[_index];
                float scaled_advW = unscale.AdvanceX * _pxscale;
                return new PxScaledGlyphPlan(
                    unscale.input_cp_offset,
                    unscale.glyphIndex,
                    scaled_advW,
                    unscale.OffsetX * _pxscale,
                    unscale.OffsetY * _pxscale);
            }
        }

        public float AccumWidth { get { return _accW; } }
        public float ExactX { get { return _exactX; } }
        public float ExactY { get { return _exactY; } }
        public ushort CurrentGlyphIndex { get { return _currentGlyphIndex; } }
        public bool Read()
        {
            if (_index >= _end)
            {
                return false;
            }

            //read current 
            UnscaledGlyphPlan unscale = _seq[_index];

            float scaled_advW = unscale.AdvanceX * _pxscale;
            _exactX = _accW + (unscale.AdvanceX + unscale.OffsetX) * _pxscale;
            _exactY = unscale.OffsetY * _pxscale;
            _accW += scaled_advW;
            _currentGlyphIndex = unscale.glyphIndex;
            _index++;
            return true;
        }
    }
    /// <summary>
    /// scaled glyph plan + snap-to-grid 
    /// </summary>
    public struct GlyphPlanSequenceSnapPixelScaleLayout
    {

        GlyphPlanSequence _seq;
        float _pxscale;
        int _accW;
        int _index;
        int _end;

        int _exactX;
        int _exactY;

        ushort _currentGlyphIndex;
        public GlyphPlanSequenceSnapPixelScaleLayout(GlyphPlanSequence glyphPlans, float pxscale)
        {
            _seq = glyphPlans;
            _pxscale = pxscale;
            _accW = 0;
            _index = glyphPlans.startAt;
            _end = glyphPlans.startAt + glyphPlans.len;
            _exactX = _exactY = 0;
            _currentGlyphIndex = 0;
        }
        public GlyphPlanSequenceSnapPixelScaleLayout(GlyphPlanSequence glyphPlans, int start, int len, float pxscale)
        {
            _seq = glyphPlans;
            _pxscale = pxscale;
            _accW = 0;
            _index = start;
            _end = start + len;
            _exactX = _exactY = 0;
            _currentGlyphIndex = 0;
        }
        public ushort CurrentGlyphIndex { get { return _currentGlyphIndex; } }
        public int CurrentIndex { get { return _index; } }

        public PxScaledGlyphPlan GlyphPlan
        {
            get
            {
                UnscaledGlyphPlan unscale = _seq[_index];
                float scaled_advW = (short)Math.Round(unscale.AdvanceX * _pxscale);
                return new PxScaledGlyphPlan(
                    unscale.input_cp_offset,
                    unscale.glyphIndex,
                    scaled_advW,
                    unscale.OffsetX * _pxscale,
                    unscale.OffsetY * _pxscale);

            }
        }

        public bool Read()
        {
            if (_index >= _end)
            {

                return false;
            }

            //read current 
            UnscaledGlyphPlan unscale = _seq[_index];

            short scaled_advW = (short)Math.Round(unscale.AdvanceX * _pxscale);
            short scaled_offsetX = (short)Math.Round(unscale.OffsetX * _pxscale);
            short scaled_offsetY = (short)Math.Round(unscale.OffsetY * _pxscale);

            _exactX = _accW + scaled_offsetX;
            _exactY = scaled_offsetY;
            _accW += scaled_advW;

            _currentGlyphIndex = unscale.glyphIndex;
            _index++;
            return true;
        }
        public int AccumWidth { get { return _accW; } }
        public int ExactX { get { return _exactX; } }
        public int ExactY { get { return _exactY; } }


    }
    public static class PixelScaleLayoutExtensions
    {

#if DEBUG
        public static float dbugSnapToFitInteger(float value)
        {
            int floor_value = (int)value;
            return (value - floor_value >= (1f / 2f)) ? floor_value + 1 : floor_value;
        }
        public static float dbugSnapHalf(float value)
        {
            int floor_value = (int)value;
            //round to int 0, 0.5,1.0
            return (value - floor_value >= (2f / 3f)) ? floor_value + 1 : //else->
                   (value - floor_value >= (1f / 3f)) ? floor_value + 0.5f : floor_value;
        }
        static int dbugSnapUpper(float value)
        {
            int floor_value = (int)value;
            return floor_value + 1;
        }
#endif



        static void GenerateScaledGlyphPlans(this GlyphLayout glyphLayout,
         float pxscale,
         bool snapToGrid,
         out float accumW)
        {
            //user can implement this with some 'PixelScaleEngine'  
            IGlyphPositions glyphPositions = glyphLayout.ResultUnscaledGlyphPositions;
            accumW = 0; //acummulate Width

            if (snapToGrid)
            {
                int finalGlyphCount = glyphPositions.Count;
                for (int i = 0; i < finalGlyphCount; ++i)
                {
                    short offsetX, offsetY, advW; //all from pen-pos
                    ushort glyphIndex = glyphPositions.GetGlyph(i,
                        out ushort input_offset,
                        out offsetX,
                        out offsetY,
                        out advW);

                    float scaled_advW = (short)Math.Round(advW * pxscale);
                    accumW += scaled_advW;

                }
            }
            else
            {
                //not snap to grid
                //scaled but not snap to grid
                int finalGlyphCount = glyphPositions.Count;
                for (int i = 0; i < finalGlyphCount; ++i)
                {
                    short offsetX, offsetY, advW; //all from pen-pos
                    ushort glyphIndex = glyphPositions.GetGlyph(i,
                        out ushort input_offset,
                        out offsetX,
                        out offsetY,
                        out advW);
                    accumW += advW * pxscale;
                }
            }
        }

        public static MeasuredStringBox LayoutAndMeasureString(
            this GlyphLayout glyphLayout,
            char[] textBuffer,
            int startAt,
            int len,
            float fontSizeInPoints,
            bool snapToGrid = true)
        {
            //1. unscale layout, in design unit
            glyphLayout.Layout(textBuffer, startAt, len);


            //2. scale  to specific font size           

            Typeface typeface = glyphLayout.Typeface;
            float pxscale = typeface.CalculateScaleToPixelFromPointSize(fontSizeInPoints);

            //....
            GenerateScaledGlyphPlans(
                glyphLayout,
                pxscale,
                snapToGrid,
                out float scaled_accumX);

            return new MeasuredStringBox(
                  scaled_accumX,
                  typeface.Ascender * pxscale,
                  typeface.Descender * pxscale,
                  typeface.LineGap * pxscale,
                  Typography.OpenFont.Extensions.TypefaceExtensions.CalculateRecommendLineSpacing(typeface) * pxscale);
        }
    }

}
