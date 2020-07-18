//MIT, 2016-present, WinterDev 
using System;
using Typography.OpenFont;
using Typography.OpenFont.Extensions;
using Typography.TextLayout;

namespace Typography.Text
{
    /// <summary>
    /// base class for print a TextSpan
    /// </summary>
    public abstract class AbstractTextSpanPrinter
    {
        public AbstractTextSpanPrinter()
        {
            ScriptLang = new ScriptLang(ScriptTagDefs.Latin.Tag);
        }

        public TextBaseline TextBaseline { get; set; }
        public abstract Typeface Typeface { get; set; }


        public bool FillBackground { get; set; }
        public bool DrawOutline { get; set; }

        public float FontAscendingPx { get; set; }
        public float FontDescedingPx { get; set; }
        public float FontLineGapPx { get; set; }
        public float FontLineSpacingPx { get; set; }
        public float FontClipHeightPx { get; set; }
        public bool SimulateSlant { get; set; }


        float _fontSizeInPoints;
        public float FontSizeInPoints
        {
            get => _fontSizeInPoints;
            set
            {
                if (_fontSizeInPoints != value)
                {
                    _fontSizeInPoints = value;
                    OnFontSizeChanged();
                }
            }
        }
        protected virtual void OnFontSizeChanged()
        {
            Typeface typeface = Typeface;
            if (typeface == null)
            {
                //clear
                FontAscendingPx =
                    FontDescedingPx =
                    FontLineGapPx =
                    FontClipHeightPx =
                    FontLineSpacingPx = 0;
            }
            else
            {
                float pointToPixelScale = typeface.CalculateScaleToPixelFromPointSize(this.FontSizeInPoints);
                this.FontAscendingPx = typeface.Ascender * pointToPixelScale;
                this.FontDescedingPx = typeface.Descender * pointToPixelScale;
                this.FontLineGapPx = typeface.LineGap * pointToPixelScale;
                this.FontClipHeightPx = typeface.CalculateMaxLineClipHeight() * pointToPixelScale;
                this.FontLineSpacingPx = FontAscendingPx - FontDescedingPx + FontLineGapPx;
            }
        }
        public virtual ScriptLang ScriptLang { get; set; }
        public PositionTechnique PositionTechnique { get; set; }
        public bool EnableLigature { get; set; }
        public bool EnableMultiTypefaces { get; set; }
        /// <summary>
        /// draw string at (xpos,ypos), depend on baseline
        /// </summary>
        /// <param name="textBuffer"></param>
        /// <param name="startAt"></param>
        /// <param name="len"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        public abstract void DrawString(char[] textBuffer, int startAt, int len, float left, float top);

        /// <summary>
        /// draw glyph plan list at (xpos,ypos) of baseline
        /// </summary>
        /// <param name="glyphPlanList"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        public abstract void DrawFromGlyphPlans(GlyphPlanSequence glyphPlanList, int startAt, int len, float left, float top);


        //helper methods
        public void DrawString(char[] textBuffer, float left, float top)
        {
            DrawString(textBuffer, 0, textBuffer.Length, left, top);
        }
        public void DrawFromGlyphPlans(GlyphPlanSequence glyphPlanSeq, float left, float top)
        {
            DrawFromGlyphPlans(glyphPlanSeq, 0, glyphPlanSeq.Count, left, top);
        }
    }

}