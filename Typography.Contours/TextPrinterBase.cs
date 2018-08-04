//MIT, 2016-present, WinterDev
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
        Typography.Contours.HintTechnique _hintTech;
        public TextPrinterBase()
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
                  IUnscaledGlyphPlanList unscaledGlyphPlan)
        {
            GlyphLayout glyphLayout = this.GlyphLayoutMan;
            glyphLayout.Layout(textBuffer, startAt, len);
            glyphLayout.GenerateUnscaledGlyphPlans(unscaledGlyphPlan); 
        }
        

        public bool FillBackground { get; set; }
        public bool DrawOutline { get; set; }
        public float FontAscendingPx { get; set; }
        public float FontDescedingPx { get; set; }
        public float FontLineGapPx { get; set; }
        public float FontLineSpacingPx { get; set; }
        public bool SimulateSlant { get; set; }

        public Typography.Contours.HintTechnique HintTechnique
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
        /// <param name="x"></param>
        /// <param name="y"></param>
        public abstract void DrawString(char[] textBuffer, int startAt, int len, float x, float y);
        /// <summary>
        /// draw glyph plan list at (xpos,ypos) of baseline
        /// </summary>
        /// <param name="glyphPlanList"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public abstract void DrawFromGlyphPlans(GlyphPlanSequence glyphPlanList, int startAt, int len, float x, float y);
       
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
        public void DrawFromGlyphPlans(GlyphPlanSequence glyphPlanSeq, float x, float y)
        {
            DrawFromGlyphPlans(glyphPlanSeq, 0, glyphPlanSeq.Count, x, y);
        }
        

    }

}