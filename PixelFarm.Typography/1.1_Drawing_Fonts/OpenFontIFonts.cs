//Apache2, 2014-2017, WinterDev
using System;
using System.Collections.Generic;
using PixelFarm.Drawing;
using PixelFarm.Drawing.Fonts;
using Typography.OpenFont;
using Typography.TextLayout;
namespace LayoutFarm
{
    public class OpenFontIFonts : IFonts
    {

        IFontLoader _fontloader;
        TypefaceStore typefaceStore;
        GlyphLayout glyphLayout;
        List<GlyphPlan> userGlyphPlanList;
        List<UserCharToGlyphIndexMap> userCharToGlyphMapList;

        public OpenFontIFonts(IFontLoader fontloader)
        {
            this._fontloader = fontloader;
            typefaceStore = new TypefaceStore();
            typefaceStore.FontCollection = InstalledFontCollection.GetSharedFontCollection(null);
            glyphLayout = new GlyphLayout(); //create glyph layout with default value
            userGlyphPlanList = new List<GlyphPlan>();
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
            //resolve type face
            Typeface typeface = typefaceStore.GetTypeface(font.Name, InstalledFontStyle.Normal);
            glyphLayout.Typeface = typeface;
            MeasuredStringBox result;
            float scale = typeface.CalculateToPixelScaleFromPointSize(font.SizeInPoints);

            //measure string at specific px scale
            glyphLayout.MeasureString(str, startAt, len, out result, scale);

            return new Size((int)result.width, (int)result.CalculateLineHeight());

        }
        public Size MeasureString(char[] str, int startAt, int len, RequestFont font, float maxWidth, out int charFit, out int charFitWidth)
        {
            throw new NotImplementedException();
        }

        public float MeasureWhitespace(RequestFont f)
        {
            throw new NotImplementedException();
        }
    }
}
