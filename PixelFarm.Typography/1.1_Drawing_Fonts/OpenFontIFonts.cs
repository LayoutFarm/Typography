//Apache2, 2014-2017, WinterDev
using System;
using System.Collections.Generic;
using PixelFarm.Drawing;
using Typography.OpenFont;
using Typography.TextLayout;
using Typography.TextServices;

namespace LayoutFarm
{
    public class OpenFontIFonts : IFonts
    {


        TypefaceStore typefaceStore;
        GlyphLayout glyphLayout;
        GlyphPlanList userGlyphPlanList;
        List<UserCharToGlyphIndexMap> userCharToGlyphMapList;

        public OpenFontIFonts()
        {

            typefaceStore = new TypefaceStore();
            typefaceStore.FontCollection = InstalledFontCollection.GetSharedFontCollection(null);
            glyphLayout = new GlyphLayout(); //create glyph layout with default value
            userGlyphPlanList = new GlyphPlanList();
            userCharToGlyphMapList = new List<UserCharToGlyphIndexMap>();
        }
        public void CalculateGlyphAdvancePos(char[] str, int startAt, int len, RequestFont font, int[] glyphXAdvances)
        {


            ////from font
            ////resolve for typeface
            //userGlyphPlanList.Clear();
            //userCharToGlyphMapList.Clear();
            //// 
            //Typeface typeface = typefaceStore.GetTypeface(font.Name, InstalledFontStyle.Normal);
            //glyphLayout.Typeface = typeface;
            //glyphLayout.GenerateGlyphPlans(str, startAt, len, userGlyphPlanList, userCharToGlyphMapList);
            ////
            ////
            //float scale = typeface.CalculateToPixelScaleFromPointSize(font.SizeInPoints);
            //int j = glyphXAdvances.Length;
            //double actualX = 0;
            //for (int i = 0; i < j; ++i)
            //{
            //    GlyphPlan p = userGlyphPlanList[i];
            //    double actualAdvX = p.advX * scale;
            //    double newX = actualX + actualAdvX;

            //    glyphXAdvances[i] = (int)Math.Round(newX - actualX);
            //    actualX = newX;
            //}

        }


        public Size MeasureString(char[] str, int startAt, int len, RequestFont font)
        {
            int w, h;
            MeasureString(str, startAt, len, out w, out h);
            return new Size(w, h);

        }
        public Size MeasureString(char[] str, int startAt, int len, RequestFont font,
            float maxWidth, out int charFit, out int charFitWidth)
        {
            throw new NotImplementedException();
        }

        public float MeasureWhitespace(RequestFont f)
        {
            throw new NotImplementedException();
        }


        GlyphPlanList _reusableGlyphPlanList = new GlyphPlanList();
        List<MeasuredStringBox> _reusableMeasureBoxList = new List<MeasuredStringBox>();
        public void MeasureString(char[] str, int startAt, int len, out int w, out int h)
        {
            throw new NotSupportedException();

            ////measure string 
            //if (str.Length < 1)
            //{
            //    w = h = 0;
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
            //float accumH = 0;

            //foreach (BreakSpan breakSpan in BreakToLineSegments(str, startAt, len))
            //{


            //    //measure string at specific px scale 
            //    _glyphLayout.Layout(str, breakSpan.startAt, breakSpan.len);
            //    //
            //    _reusableGlyphPlanList.Clear();
            //    GlyphLayoutExtensions.GenerateGlyphPlan(
            //        _glyphLayout.ResultUnscaledGlyphPositions,
            //        pxscale,
            //        true,
            //        _reusableGlyphPlanList);
            //    //measure string size
            //    var result = new MeasuredStringBox(
            //        _reusableGlyphPlanList.AccumAdvanceX * pxscale,
            //        _currentTypeface.Ascender * pxscale,
            //        _currentTypeface.Descender * pxscale,
            //        _currentTypeface.LineGap * pxscale,
            //         Typography.OpenFont.Extensions.TypefaceExtensions.CalculateRecommendLineSpacing(_currentTypeface) * pxscale);
            //    //
            //    ConcatMeasureBox(ref accumW, ref accumH, ref result);

            //}

            //w = (int)System.Math.Round(accumW);
            //h = (int)System.Math.Round(accumH);
        }
        static void ConcatMeasureBox(ref float accumW, ref float accumH, ref MeasuredStringBox measureBox)
        {
            accumW += measureBox.width;
            float h = measureBox.CalculateLineHeight();
            if (h > accumH)
            {
                accumH = h;
            }
        }

    }
}
