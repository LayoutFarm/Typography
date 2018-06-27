//MIT, 2016-2017, WinterDev
using System.Collections.Generic;
using Typography.OpenFont;
using Typography.TextLayout;


namespace Typography.Contours
{
    /// <summary>
    /// base TextPrinter class
    /// </summary>
    public abstract class TextPrinterBase
    {
        public TextPrinterBase()
        {
            FontSizeInPoints = 14;//
            ScriptLang = ScriptLangs.Latin;//default?
        }

        public abstract GlyphLayout GlyphLayout { get; }
        public virtual Typeface Typeface { get { return GlyphLayout.Typeface; } set { GlyphLayout.Typeface = value; } }
        public ScriptLang ScriptLang { get { return GlyphLayout.ScriptLang; } set { GlyphLayout.ScriptLang = value; } }
        public PositionTechnique PositionTechnique { get { return GlyphLayout.PositionTechnique; } set { GlyphLayout.PositionTechnique = value; } }
        public bool EnableLigature { get { return GlyphLayout.EnableLigature; } set { GlyphLayout.EnableLigature = value; } }
        public virtual void GenerateGlyphPlan(
                 char[] textBuffer,
                 int startAt,
                 int len,
                 GlyphPlanList outputGlyphPlanList,
                 List<UserCodePointToGlyphIndex> charToGlyphMapList)
        {

            this.GlyphLayout.Layout(textBuffer, startAt, len);
            GlyphLayoutExtensions.GenerateGlyphPlan(this.GlyphLayout.ResultUnscaledGlyphPositions,
                this.Typeface.CalculateScaleToPixelFromPointSize(this.FontSizeInPoints),
                false, outputGlyphPlanList);
        }


        public bool FillBackground { get; set; }
        public bool DrawOutline { get; set; }
        public float FontAscendingPx { get; set; }
        public float FontDescedingPx { get; set; }
        public float FontLineGapPx { get; set; }
        public float FontLineSpacingPx { get; set; }

        public HintTechnique HintTechnique { get; set; }

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

        /// <summary>
        /// draw string at (xpos,ypos) of baseline 
        /// </summary>
        /// <param name="textBuffer"></param>
        /// <param name="startAt"></param>
        /// <param name="len"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public abstract void DrawString(char[] textBuffer, int startAt, int len, float x, float y);
        /// <summary>
        /// draw glyph plan list at (xpos,ypos) of baseline
        /// </summary>
        /// <param name="glyphPlanList"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public abstract void DrawFromGlyphPlans(GlyphPlanList glyphPlanList, int startAt, int len, float x, float y);

        /// <summary>
        /// draw caret at xpos,ypos (sample only)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public abstract void DrawCaret(float x, float y);


        //----------------------------------------------------
        //helper methods
        public void DrawString(char[] textBuffer, float x, float y)
        {
            DrawString(textBuffer, 0, textBuffer.Length, x, y);
        }
        public void DrawFromGlyphPlans(GlyphPlanList glyphPlanList, float x, float y)
        {
            DrawFromGlyphPlans(glyphPlanList, 0, glyphPlanList.Count, x, y);
        }

    }

}