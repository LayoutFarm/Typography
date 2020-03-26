//MIT, 2014-present, WinterDev
using System.Collections.Generic;


using Typography.OpenFont;
using Typography.TextBreak;

namespace Typography.TextLayout
{

    /// <summary>
    /// collect and managed editable text line
    /// </summary>
    public class EditableTextBlockLayoutEngine
    {

        TextBlockLexer _textBlockLexer;
        List<EditableTextLine> _lines = new List<EditableTextLine>();
        GlyphLayout _glyphLayout;
        UnscaledGlyphPlanList _outputGlyphPlan = new UnscaledGlyphPlanList();

        public EditableTextBlockLayoutEngine(Typeface defaultTypeface, float fontSizeInPts = 10)
        {
            _textBlockLexer = new TextBlockLexer();
            _glyphLayout = new GlyphLayout(defaultTypeface);
            FontSizeInPts = fontSizeInPts;
            DefaultTypeface = defaultTypeface;
        }
        public Typeface DefaultTypeface { get; set; }
        /// <summary>
        /// font size in points
        /// </summary>
        public float FontSizeInPts { get; set; }
        public float ContentWidth { get; set; }
        public float ContentHeight { get; set; }


        public void LoadText(string text)
        {
            //1. all line is clear
            //
            //2. new text is parsed into multiple line (line break)
            //   
            //3. eachline parse its own word in its context

            //output is a line group of run

            //TODO: extend with custom lexer***  
            //test only ...
            TextRunFontStyle fontStyle = new TextRunFontStyle("tahoma", FontSizeInPts);//10 pts;

            TextBuffer buffer = new TextBuffer(text.ToCharArray());
            _textBlockLexer.Lex(buffer);

            //create a set of line
            List<LexWordSpan> spanLists = _textBlockLexer.ResultSpans;
            _lines.Clear();
            int j = spanLists.Count;

            EditableTextLine line = new EditableTextLine();
            _lines.Add(line);

            //create text run from parsed word span
            for (int i = 0; i < j; ++i)
            {
                LexWordSpan sp = spanLists[i];
                if (sp.kind == WordSpanKind.NewLine)
                {
                    line.ExplicitEnd = true;
                    line = new EditableTextLine();
                    line.LineNumber = _lines.Count;
                    _lines.Add(line);
                }
                else
                {
                    //create a 'Run' for this span
                    TextRun textRun = new TextRun(buffer, sp.start, sp.len, sp.kind, fontStyle);
                    line.AppendLast(textRun);
                }
            }
            //at this point each text run in the line is not layout, 
            //no span size calculation
            //------------

            //we can calculate the text run size when
            //we known more about font of each style 
        }




        public void DoLayout()
        {

            //----------------
            //TODO: use typography text service
            //it should be faster since it has glyph-plan cache
            //----------------

            //user can use other native methods 
            //to do the layout ***

            //the following code is how-to-layout with
            //our Typography lib
            //

            //then at this step 
            //we calculate span size 
            //resolve each font style  
            _glyphLayout.EnableComposition = true;
            _glyphLayout.EnableLigature = true;
            int lineCount = _lines.Count;


            Typeface selectedTypeface = this.DefaultTypeface;
            float pxscale = selectedTypeface.CalculateScaleToPixelFromPointSize(this.FontSizeInPts);
            for (int i = 0; i < lineCount; ++i)
            {
                EditableTextLine line = _lines[i];
                List<IRun> runList = line.UnsageGetTextRunList();
                int runCount = runList.Count;

                for (int r = 0; r < runCount; ++r)
                {
                    var tt = runList[r] as TextRun;
                    if (tt == null) continue;
                    //this is text run
                    if (tt.IsMeasured) continue;
                    //

                    TextRunFontStyle fontStyle = tt.FontStyle;
                    //resolve to actual font face
                    TextBuffer buffer = tt.TextBuffer;
                    char[] rawBuffer = buffer.UnsafeGetInternalBuffer();


                    //TODO: review here again

                    int preCount = _outputGlyphPlan.Count;

                    _glyphLayout.Typeface = selectedTypeface;
                    _glyphLayout.Layout(rawBuffer, tt.StartAt, tt.Len);

                    _glyphLayout.GenerateUnscaledGlyphPlans(_outputGlyphPlan);


                    //use pixel-scale-layout-engine to scale to specific font size
                    //or scale it manually 
                    int postCount = _outputGlyphPlan.Count;
                    //
                    tt.SetGlyphPlanSeq(new GlyphPlanSequence(_outputGlyphPlan, preCount, postCount - preCount));
                    tt.IsMeasured = true;
                    //
                }
            }
        }
        //
        public TextBlockLexer CurrentLexer
        {
            get => _textBlockLexer;
            set => _textBlockLexer = value;
        }

        public List<EditableTextLine> UnsafeGetEditableTextLine() => _lines;
    }


}