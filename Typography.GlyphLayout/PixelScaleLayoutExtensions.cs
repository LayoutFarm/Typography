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
        //
        public int CurrentIndex => _index;
        //
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

        public float AccumWidth => _accW;
        public float ExactX => _exactX;
        public float ExactY => _exactY;
        public ushort CurrentGlyphIndex => _currentGlyphIndex;
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
        //
        public ushort CurrentGlyphIndex => _currentGlyphIndex;
        public int CurrentIndex => _index;
        //
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
        public int AccumWidth => _accW;
        public int ExactX => _exactX;
        public int ExactY => _exactY;
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

        static float MeasureGlyphPlans(this GlyphLayout glyphLayout,
             float pxscale,
             bool snapToGrid)
        {
            //user can implement this with some 'PixelScaleEngine'  
            IGlyphPositions glyphPositions = glyphLayout.ResultUnscaledGlyphPositions;
            float accumW = 0; //acummulate Width

            if (snapToGrid)
            {
                int finalGlyphCount = glyphPositions.Count;
                for (int i = 0; i < finalGlyphCount; ++i)
                {
                    //all from pen-pos 
                    ushort glyphIndex = glyphPositions.GetGlyph(i,
                        out ushort input_offset,
                        out short offsetX,
                        out short offsetY,
                        out short advW);
                    accumW += (short)Math.Round(advW * pxscale);
                }

            }
            else
            {
                //not snap to grid
                //scaled but not snap to grid
                int finalGlyphCount = glyphPositions.Count;
                for (int i = 0; i < finalGlyphCount; ++i)
                {
                    //all from pen-pos 
                    ushort glyphIndex = glyphPositions.GetGlyph(i,
                        out ushort input_offset,
                        out short offsetX,
                        out short offsetY,
                        out short advW);
                    accumW += advW * pxscale;
                }
            }
            return accumW;
        }
        static float MeasureGlyphPlanWithLimitWidth(this GlyphLayout glyphLayout,
             float pxscale,
             float limitWidth,
             bool snapToGrid,
             out int stopAtGlyphIndex)
        {
            //user can implement this with some 'PixelScaleEngine'  
            IGlyphPositions glyphPositions = glyphLayout.ResultUnscaledGlyphPositions;
            float accumW = 0; //acummulate Width
            stopAtGlyphIndex = 0;

            if (snapToGrid)
            {
                int finalGlyphCount = glyphPositions.Count;
                for (int i = 0; i < finalGlyphCount; ++i)
                {

                     //all from pen-pos
                    ushort glyphIndex = glyphPositions.GetGlyph(i,
                        out ushort input_offset,
                        out short offsetX,
                        out short offsetY,
                        out short advW);

                    stopAtGlyphIndex = i; //***
                    //
                    short w = (short)Math.Round(advW * pxscale);
                    if (accumW + w > limitWidth)
                    {
                        //stop           
                        break;
                    }
                    else
                    {
                        accumW += w;
                    }
                }
            }
            else
            {
                //not snap to grid
                //scaled but not snap to grid
                int finalGlyphCount = glyphPositions.Count;
                for (int i = 0; i < finalGlyphCount; ++i)
                {
                    //all from pen-pos
                    ushort glyphIndex = glyphPositions.GetGlyph(i,
                        out ushort input_offset,
                        out short offsetX,
                        out short offsetY,
                        out short advW);


                    stopAtGlyphIndex = i; //***

                    float w = advW * pxscale;
                    if (accumW + w > limitWidth)
                    {
                        //stop           
                        break;
                    }
                    else
                    {
                        accumW += w;
                    }
                }
            }
            return accumW;


            ////measure string 
            //if (str.Length < 1)
            //{
            //    charFitWidth = 0;
            //}

            //_reusableMeasureBoxList.Clear(); //reset 


            //float pxscale = _currentTypeface.CalculateScaleToPixelFromPointSize(_fontSizeInPts);
            ////NOET:at this moment, simple operation
            ////may not be simple...  
            ////-------------------
            ////input string may contain more than 1 script lang
            ////user can parse it by other parser
            ////but in this code, we use our Typography' parser
            ////-------------------
            ////user must setup the CustomBreakerBuilder before use         

            //int cur_startAt = startAt;
            //float accumW = 0;

            //float acc_x = 0;//accum_x
            //float acc_y = 0;//accum_y
            //float g_x = 0;
            //float g_y = 0;
            //float x = 0;
            //float y = 0;
            //foreach (Typography.TextLayout.BreakSpan breakSpan in BreakToLineSegments(str, startAt, len))
            //{

            //    //measure string at specific px scale 
            //    _glyphLayout.Layout(str, breakSpan.startAt, breakSpan.len);
            //    //

            //    _reusableGlyphPlanList.Clear();
            //    _glyphLayout.GenerateUnscaledGlyphPlans(_reusableGlyphPlanList);
            //    //measure ...


            //    //measure each glyph
            //    //limit at specific width
            //    int glyphCount = _reusableGlyphPlanList.Count;



            //    for (int i = 0; i < glyphCount; ++i)
            //    {
            //        UnscaledGlyphPlan glyphPlan = _reusableGlyphPlanList[i];

            //        float ngx = acc_x + (float)Math.Round(glyphPlan.OffsetX * pxscale);
            //        float ngy = acc_y + (float)Math.Round(glyphPlan.OffsetY * pxscale);
            //        //NOTE:
            //        // -glyphData.TextureXOffset => restore to original pos
            //        // -glyphData.TextureYOffset => restore to original pos 
            //        //--------------------------
            //        g_x = (float)(x + (ngx)); //ideal x
            //        g_y = (float)(y + (ngy));
            //        float g_w = (float)Math.Round(glyphPlan.AdvanceX * pxscale);
            //        acc_x += g_w;
            //        //g_x = (float)Math.Round(g_x);
            //        g_y = (float)Math.Floor(g_y);

            //        float right = g_x + g_w;

            //        if (right >= accumW)
            //        {
            //            //stop here at this glyph
            //            charFit = i - 1;
            //            //TODO: review this
            //            charFitWidth = (int)System.Math.Round(accumW);
            //            return;
            //        }
            //        else
            //        {
            //            accumW = right;
            //        }
            //    }
            //}

            //charFit = 0;
            //charFitWidth = 0;
        }


        //static void ConcatMeasureBox(ref float accumW, ref float accumH, ref MeasuredStringBox measureBox)
        //{
        //    accumW += measureBox.width;
        //    float h = measureBox.CalculateLineHeight();
        //    if (h > accumH)
        //    {
        //        accumH = h;
        //    }
        //}



        public static MeasuredStringBox LayoutAndMeasureString(
            this GlyphLayout glyphLayout,
            char[] textBuffer,
            int startAt,
            int len,
            float fontSizeInPoints,
            float limitW = -1,//-1 unlimit scaled width (px)
            bool snapToGrid = true)
        {
            //1. unscale layout, in design unit
            glyphLayout.Layout(textBuffer, startAt, len);

            //2. scale  to specific font size           

            Typeface typeface = glyphLayout.Typeface;
            float pxscale = typeface.CalculateScaleToPixelFromPointSize(fontSizeInPoints);

            //....
            float scaled_accumX = 0;
            if (limitW < 0)
            {
                //no limit
                scaled_accumX = MeasureGlyphPlans(
                    glyphLayout,
                    pxscale,
                    snapToGrid);

                return new MeasuredStringBox(
                     scaled_accumX,
                     typeface.Ascender,
                     typeface.Descender,
                     typeface.LineGap,
                     typeface.ClipedAscender,
                     typeface.ClipedDescender,
                     pxscale);

            }
            else if (limitW > 0)
            {
                scaled_accumX = MeasureGlyphPlanWithLimitWidth(
                    glyphLayout,
                    pxscale,
                    limitW,
                    snapToGrid,
                    out int stopAtChar);

                var mstrbox = new MeasuredStringBox(
                 scaled_accumX,
                 typeface.Ascender,
                 typeface.Descender,
                 typeface.LineGap,
                 typeface.ClipedAscender,
                 typeface.ClipedDescender,
                 pxscale);

                mstrbox.StopAt = (ushort)stopAtChar;
                return mstrbox;
            }
            else
            {
                return new MeasuredStringBox(
                    0,
                    typeface.Ascender,
                    typeface.Descender,
                    typeface.LineGap,
                    typeface.ClipedAscender,
                    typeface.ClipedDescender,
                    pxscale);
            }

        }
    }

}
