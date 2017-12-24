//MIT, 2014-2017, WinterDev

using System.Collections.Generic;
using Typography.TextLayout;
using Typography.Contours;

namespace SampleWinForms.UI
{

    class VisualLine
    {

        SmallLine _line;
        DevTextPrinterBase _printer;

        float toPxScale = 1;
        public VisualLine()
        {

        }
        public void BindLine(SmallLine line)
        {
            this._line = line;
        }
        public void BindPrinter(DevTextPrinterBase printer)
        {
            _printer = printer;
        }

        public float X { get; set; }
        public float Y { get; set; }
        public void SetCharIndexFromPos(float x, float y)
        {
            _line.SetCharIndexFromPos(x, y);
        }

        public void Draw()
        {

            GlyphPlanList glyphPlans = _line._glyphPlans;
            List<UserCharToGlyphIndexMap> userCharToGlyphIndexMap = _line._userCharToGlyphMap;
            if (_line.ContentChanged)
            {
                //re-calculate 
                char[] textBuffer = _line._charBuffer.ToArray();
                glyphPlans.Clear();

                userCharToGlyphIndexMap.Clear();

                //read glyph plan and userCharToGlyphIndexMap                 
                
                _printer.GenerateGlyphPlan(textBuffer, 0, textBuffer.Length, glyphPlans, userCharToGlyphIndexMap);


                toPxScale = _printer.Typeface.CalculateScaleToPixelFromPointSize(_printer.FontSizeInPoints);
                _line.ContentChanged = false;
            }

            if (glyphPlans.Count > 0)
            {

                _printer.DrawFromGlyphPlans(glyphPlans, X, Y);
                //draw caret 
                //not blink in this version
                int caret_index = _line.CaretCharIndex;
                //find caret pos based on glyph plan
                //TODO: check when do gsub (glyph number may not match with user char number)                 

                if (caret_index == 0)
                {
                    _printer.DrawCaret(X, this.Y);
                }
                else
                {
                    UserCharToGlyphIndexMap map = userCharToGlyphIndexMap[caret_index - 1];
                    GlyphPlan p = glyphPlans[map.glyphIndexListOffset_plus1 + map.len - 2];
                    _printer.DrawCaret(X + p.ExactRight, this.Y);
                }
            }
            else
            {

                _printer.DrawCaret(X, this.Y);
            }
        }
    }


}