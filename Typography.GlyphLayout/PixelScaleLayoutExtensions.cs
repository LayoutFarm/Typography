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
    /// expandable list of glyph plan
    /// </summary>
    public class PxScaledGlyphPlanList : IPixelScaledGlyphPlanList
    {
        List<PxScaledGlyphPlan> _glyphPlans = new List<PxScaledGlyphPlan>();
        public void Append(PxScaledGlyphPlan glyphPlan)
        {
            _glyphPlans.Add(glyphPlan);
        }
        public PxScaledGlyphPlan this[int index]
        {
            get
            {
                return _glyphPlans[index];
            }
        }
        public int Count
        {
            get
            {
                return _glyphPlans.Count;
            }
        }

        public void Clear()
        {
            _glyphPlans.Clear();
        }
#if DEBUG
        public PxScaledGlyphPlanList()
        {

        }
#endif
    }

    public interface IPixelScaledGlyphPlanList
    {
        void Append(PxScaledGlyphPlan glyphPlan);
        PxScaledGlyphPlan this[int index] { get; }
        int Count { get; }
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


        

        /// <summary>
        /// fetch layout result, generate specific scaled version from unscaled glyph plan
        /// </summary>
        /// <param name="glyphLayout"></param>
        /// <param name="pxscale"></param>
        /// <param name="snapToGrid"></param>
        /// <param name="accumW"></param>
        /// <param name="outputGlyphPlanList"></param>
        public static void GenerateScaledGlyphPlans(this GlyphLayout glyphLayout,
            float pxscale,
            bool snapToGrid,
            IPixelScaledGlyphPlanList outputGlyphPlanList)
        {
            GenerateScaledGlyphPlans(glyphLayout, pxscale, snapToGrid, out float accumW, outputGlyphPlanList);
        }

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
        /// <summary>
        /// fetch layout result, generate specific scaled version from unscaled glyph plan
        /// </summary>
        /// <param name="glyphPositions"></param>
        /// <param name="pxscale"></param>
        /// <param name="outputGlyphPlanList"></param>
        public static void GenerateScaledGlyphPlans(this GlyphLayout glyphLayout,
            float pxscale,
            bool snapToGrid,
            out float accumW, //accumulate Width
            IPixelScaledGlyphPlanList outputGlyphPlanList)
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

                    outputGlyphPlanList.Append(new PxScaledGlyphPlan(
                        input_offset,
                        glyphIndex,
                        scaled_advW,
                        (short)Math.Round(offsetX * pxscale),
                        (short)Math.Round(offsetY * pxscale)
                        ));

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

                    outputGlyphPlanList.Append(new PxScaledGlyphPlan(
                        input_offset,
                        glyphIndex,
                        advW * pxscale,
                        offsetX * pxscale,
                        offsetY * pxscale
                        ));
                }
            }
        }
         
        /// <summary>
        /// fetch layout result, generate specific scaled version from unscaled glyph plan
        /// </summary>
        /// <param name="unscaledGlyphPlanList"></param>
        /// <param name="startAt"></param>
        /// <param name="len"></param>
        /// <param name="pxscale"></param>
        /// <param name="snapToGrid"></param>
        /// <param name="outputGlyphPlanList"></param>
        public static void GenerateScaledGlyphPlans(
            this IUnscaledGlyphPlanList unscaledGlyphPlanList,
            int startAt,
            int len,
            float pxscale,
            bool snapToGrid,
            IPixelScaledGlyphPlanList outputGlyphPlanList)
        {
            //user can implement this with some 'PixelScaleEngine'  

            int end = startAt + len;
            if (snapToGrid)
            {

                for (int i = startAt; i < end; ++i)
                {
                    UnscaledGlyphPlan gp = unscaledGlyphPlanList[i];
                    outputGlyphPlanList.Append(new PxScaledGlyphPlan(
                        gp.input_cp_offset,
                        gp.glyphIndex,
                        (short)Math.Round(gp.AdvanceX * pxscale),
                        (short)Math.Round(gp.OffsetX * pxscale),
                        (short)Math.Round(gp.OffsetY * pxscale)
                        ));
                }
            }
            else
            {
                //not snap to grid
                //scaled but not snap to grid

                for (int i = startAt; i < end; ++i)
                {

                    UnscaledGlyphPlan gp = unscaledGlyphPlanList[i];

                    outputGlyphPlanList.Append(new PxScaledGlyphPlan(
                        gp.input_cp_offset,
                        gp.glyphIndex,
                        gp.AdvanceX * pxscale,
                        gp.OffsetX * pxscale,
                        gp.OffsetY * pxscale
                        ));
                }
            }
        }


        //string measurement result depends on multiple variables eg. font-size, snap-to-grid.
        // 
        public static MeasuredStringBox LayoutAndMeasureString(
            this GlyphLayout glyphLayout,
            char[] textBuffer,
            int startAt,
            int len,
            float fontSizeInPoints,
            bool snapToGrid, //default 
            IPixelScaledGlyphPlanList outputGlyphPlans)
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
                out float scaled_accumX,
                outputGlyphPlans);

            return new MeasuredStringBox(
                  scaled_accumX,
                  typeface.Ascender * pxscale,
                  typeface.Descender * pxscale,
                  typeface.LineGap * pxscale,
                  Typography.OpenFont.Extensions.TypefaceExtensions.CalculateRecommendLineSpacing(typeface) * pxscale);
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
