//MIT, 2017-present, WinterDev
using System;
using System.Collections.Generic;
using System.IO;
using Typography.OpenFont;
using Typography.TextLayout; 


namespace Typography.TextServices
{
    /// <summary>
    /// expandable list of glyph plan
    /// </summary>
    class UnscaledGlyphPlanList : IUnscaledGlyphPlanList
    {
        List<UnscaledGlyphPlan> _glyphPlans = new List<UnscaledGlyphPlan>();
        float _accumAdvanceX;

        public void Clear()
        {
            _glyphPlans.Clear();
            _accumAdvanceX = 0;
        }
        public void Append(UnscaledGlyphPlan glyphPlan)
        {
            _glyphPlans.Add(glyphPlan);
            _accumAdvanceX += glyphPlan.AdvanceX;
        }
        public float AccumAdvanceX { get { return _accumAdvanceX; } }

        public UnscaledGlyphPlan this[int index]
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

#if DEBUG
        public UnscaledGlyphPlanList()
        {

        }
#endif
    }



    public static class SampleMeasureStringUtil
    {
        //-----------------
        //measure string utils

        static PxScaledGlyphPlanList _reusableScaledGlyphPlanList = new PxScaledGlyphPlanList();
        static List<MeasuredStringBox> _reusableMeasureBoxList = new List<MeasuredStringBox>();

        static UnscaledGlyphPlanList _reusableGlyphPlanList = new UnscaledGlyphPlanList();

        public static MeasuredStringBox MeasureString(
             Typography.TextLayout.GlyphLayout glyphLayout,
             float fontSizeInPts,
             char[] str, int startAt, int len, out int w, out int h)
        {
            //measure string 
            //check if we use cache feature or not

            Typography.OpenFont.Typeface typeface = glyphLayout.Typeface;

            if (str.Length < 1)
            {
                w = h = 0;
            }
            _reusableMeasureBoxList.Clear(); //reset 

            float pxscale = typeface.CalculateScaleToPixelFromPointSize(fontSizeInPts);
            //NOET:at this moment, simple operation
            //may not be simple...  
            //-------------------
            //input string may contain more than 1 script lang
            //user can parse it by other parser
            //but in this code, we use our Typography' parser
            //-------------------
            //user must setup the CustomBreakerBuilder before use         

            int cur_startAt = startAt;
            float accumW = 0;
            float accumH = 0;



            glyphLayout.Layout(str, 0, str.Length);
            //
            _reusableGlyphPlanList.Clear();
            GlyphLayoutExtensions.GenerateScaledGlyphPlans(
                glyphLayout.ResultUnscaledGlyphPositions,
                pxscale,
                true,
                _reusableScaledGlyphPlanList);
            //measure string size
            var result = new MeasuredStringBox(
                _reusableGlyphPlanList.AccumAdvanceX,
                typeface.Ascender * pxscale,
                typeface.Descender * pxscale,
                typeface.LineGap * pxscale,
                 Typography.OpenFont.Extensions.TypefaceExtensions.CalculateRecommendLineSpacing(typeface) * pxscale);


            w = (int)System.Math.Round(accumW);
            h = (int)System.Math.Round(accumH);

            return result;
        }
    }


}