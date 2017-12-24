//MIT, 2016-2017, WinterDev
using System.Collections.Generic;
using Typography.TextLayout; 

namespace Typography.Contours
{
    /// <summary>
    /// base TextPrinter class for developer only, 
    /// </summary>
    public abstract class DevTextPrinterBase
    {
        HintTechnique _hintTech;
        public DevTextPrinterBase()
        {
            FontSizeInPoints = 14;//
            ScriptLang = Typography.OpenFont.ScriptLangs.Latin;//default?
        }

        public abstract Typography.TextLayout.GlyphLayout GlyphLayoutMan { get; }
        public abstract Typography.OpenFont.Typeface Typeface { get; set; }
        public virtual void GenerateGlyphPlan(
                 char[] textBuffer,
                 int startAt,
                 int len,
                 GlyphPlanList outputGlyphPlanList,
                 List<UserCharToGlyphIndexMap> charToGlyphMapList)
        {

            this.GlyphLayoutMan.Layout(textBuffer, startAt, len);
            GlyphLayoutExtensions.GenerateGlyphPlan(this.GlyphLayoutMan.ResultUnscaledGlyphPositions,
                this.Typeface.CalculateScaleToPixelFromPointSize(this.FontSizeInPoints),
                false, outputGlyphPlanList);
        }


        public bool FillBackground { get; set; }
        public bool DrawOutline { get; set; }
        public float FontAscendingPx { get; set; }
        public float FontDescedingPx { get; set; }
        public float FontLineGapPx { get; set; }
        public float FontLineSpacingPx { get; set; }

        public HintTechnique HintTechnique
        {
            get { return _hintTech; }
            set
            {
                this._hintTech = value;
            }
        }

        float _fontSizeInPoints;
        public float FontSizeInPoints
        {
            get { return _fontSizeInPoints; }
            set
            {
                if (_fontSizeInPoints != value)
                {
                    _fontSizeInPoints = value;
                    OnFontSizeChanged();
                }
            }
        }

        protected virtual void OnFontSizeChanged() { }
        public Typography.OpenFont.ScriptLang ScriptLang { get; set; }
        public Typography.TextLayout.PositionTechnique PositionTechnique { get; set; }
        public bool EnableLigature { get; set; }
        /// <summary>
        /// draw string at (xpos,ypos) of baseline 
        /// </summary>
        /// <param name="textBuffer"></param>
        /// <param name="startAt"></param>
        /// <param name="len"></param>
        /// <param name="xpos"></param>
        /// <param name="ypos"></param>
        public abstract void DrawString(char[] textBuffer, int startAt, int len, float xpos, float ypos);
        /// <summary>
        /// draw glyph plan list at (xpos,ypos) of baseline
        /// </summary>
        /// <param name="glyphPlanList"></param>
        /// <param name="xpos"></param>
        /// <param name="ypos"></param>
        public abstract void DrawFromGlyphPlans(GlyphPlanList glyphPlanList, int startAt, int len, float xpos, float ypos);

        /// <summary>
        /// draw caret at xpos,ypos (sample only)
        /// </summary>
        /// <param name="xpos"></param>
        /// <param name="ypos"></param>
        public abstract void DrawCaret(float xpos, float ypos);


        //----------------------------------------------------
        //helper methods
        public void DrawString(char[] textBuffer, float xpos, float ypos)
        {
            DrawString(textBuffer, 0, textBuffer.Length, xpos, ypos);
        }
        public void DrawFromGlyphPlans(GlyphPlanList glyphPlanList, float xpos, float ypos)
        {
            DrawFromGlyphPlans(glyphPlanList, 0, glyphPlanList.Count, xpos, ypos);
        }

    }

}