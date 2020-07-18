//MIT, 2016-present, WinterDev
using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
// 

using Typography.FontCollections;
using Typography.OpenFont;
using Typography.OpenFont.Extensions;
using Typography.OpenFont.Contours;

using Typography.Text;
using Typography.TextLayout;

using PixelFarm.Drawing;


namespace SampleWinForms
{
    public partial class Form1 : Form
    {
        Graphics _g;
        //for this sample code,
        //create text printer env for developer.


        DevGdiTextSpanPrinter _currentTextPrinter;
        TextServiceClient _txtServiceClient;

        public Form1()
        {
            InitializeComponent();

            //choose Thai script for 'complex script' testing.
            //you can change this to test other script.


            _txtServiceClient = OurOpenFontSystem.CreateTextServiceClient();
            _currentTextPrinter = new DevGdiTextSpanPrinter(_txtServiceClient);
            _currentTextPrinter.ScriptLang = new ScriptLang(ScriptTagDefs.Latin.Tag);


            //----------
            button1.Click += (s, e) => UpdateRenderOutput();
            //simple load test fonts from local test dir
            //and send it into test list
            chkFillBackground.Checked = true;
            chkBorder.CheckedChanged += (s, e) => UpdateRenderOutput();
            chkFillBackground.CheckedChanged += (s, e) => UpdateRenderOutput();
            //----------
            cmbPositionTech.Items.Add(PositionTechnique.OpenFont);
            cmbPositionTech.Items.Add(PositionTechnique.Kerning);
            cmbPositionTech.Items.Add(PositionTechnique.None);
            cmbPositionTech.SelectedIndex = 0;
            cmbPositionTech.SelectedIndexChanged += (s, e) => UpdateRenderOutput();
            //----------
            lstHintList.Items.Add(HintTechnique.None);
            lstHintList.Items.Add(HintTechnique.TrueTypeInstruction);
            lstHintList.Items.Add(HintTechnique.TrueTypeInstruction_VerticalOnly);
            //lstHintList.Items.Add(HintTechnique.CustomAutoFit);
            lstHintList.SelectedIndex = 0;
            lstHintList.SelectedIndexChanged += (s, e) => UpdateRenderOutput();
            //---------- 
            txtInputChar.TextChanged += (s, e) => UpdateRenderOutput();
            // 
            //---------- 
            //show result
            InstalledTypeface selectedFF = null;
            int selected_index = 0;
            int ffcount = 0;
            bool found = false;

            foreach (InstalledTypeface ff in OurOpenFontSystem.GetInstalledTypefaceIter())
            {
                if (!found && ff.FontName == "Source Sans Pro")
                {
                    selectedFF = ff;
                    selected_index = ffcount;
                    found = true;
                }
                lstFontList.Items.Add(ff);
                ffcount++;
            }
            //set default font for current text printer
            //
          
            //set default font for current text printer
            _currentTextPrinter.Typeface = OurOpenFontSystem.ResolveTypeface(selectedFF);


            //Alternative Typeface Selector
            {

                AlternativeTypefaceSelector alternativeTypefaceSelector = new AlternativeTypefaceSelector();
                PreferredTypefaceList preferredTypefaces = new PreferredTypefaceList();
                preferredTypefaces.AddTypefaceName("Segoe UI Emoji");
                alternativeTypefaceSelector.SetPerferredEmoji(preferredTypefaces);

                //set alternative typeface selector to printer
                _currentTextPrinter.AlternativeTypefaceSelector = alternativeTypefaceSelector;
            }

            //---------- 
#if DEBUG
            //test get font from typeface store 
            //InstalledTypeface instFont = OurOpenFontSystem.GetFontCollection().GetFontByPostScriptName("SourceSansPro-Regular");


#endif

            if (selected_index < 0) { selected_index = 0; }
            lstFontList.SelectedIndex = selected_index;
            lstFontList.SelectedIndexChanged += (s, e) =>
            {
                if (lstFontList.SelectedItem is InstalledTypeface ff)
                {
                    _currentTextPrinter.Typeface = OurOpenFontSystem.ResolveTypeface(ff);
                    //sample text box 
                    UpdateRenderOutput();
                }
            };
            //----------
            lstFontSizes.Items.AddRange(
                new object[]{
                    8, 9,
                    10,11,
                    12,
                    14,
                    16,
                    18,20,22,24,26,28,36,48,72,240,300,360
                });
            lstFontSizes.SelectedIndexChanged += (s, e) =>
            {
                //new font size
                _currentTextPrinter.FontSizeInPoints = (int)lstFontSizes.SelectedItem;
                UpdateRenderOutput();
            };
            lstFontSizes.SelectedIndex = 0;
            this.Text = "Gdi+ Sample";
            //------ 


        }
        void UpdateRenderOutput()
        {
            //render glyph with gdi path
            if (_g == null)
            {
                _g = this.CreateGraphics();
            }
            if (string.IsNullOrEmpty(this.txtInputChar.Text))
            {
                return;
            }
            //-----------------------  
            //set some props ...
            _g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            _g.Clear(Color.White);
            //credit:
            //http://stackoverflow.com/questions/1485745/flip-coordinates-when-drawing-to-control
            _g.ScaleTransform(1.0F, -1.0F);// Flip the Y-Axis 
            _g.TranslateTransform(0.0F, -(float)500);// Translate the drawing area accordingly   


            _currentTextPrinter.FillBackground = this.chkFillBackground.Checked;
            _currentTextPrinter.DrawOutline = this.chkBorder.Checked;
            _currentTextPrinter.EnableMultiTypefaces = this.chkEnableMultiTypefaces.Checked;

            //-----------------------  
            _currentTextPrinter.HintTechnique = (HintTechnique)lstHintList.SelectedItem;
            _currentTextPrinter.PositionTechnique = (PositionTechnique)cmbPositionTech.SelectedItem;
            _currentTextPrinter.TargetGraphics = _g;
            //render at specific pos
            int lineSpacingPx = (int)System.Math.Ceiling(_currentTextPrinter.FontLineSpacingPx);
            float x_pos = 0, y_pos = y_pos = lineSpacingPx * 2; //start 1st line
            char[] textBuffer = txtInputChar.Text.ToCharArray();

            //test draw multiple lines

            for (int i = 0; i < 3; ++i)
            {
                _currentTextPrinter.DrawString(
                 textBuffer,
                 0,
                 textBuffer.Length,
                 x_pos,
                 y_pos
                );
                //draw top to bottom 
                y_pos -= lineSpacingPx;
            }
            //
            //-----------------------  
            //transform back
            _g.ScaleTransform(1.0F, -1.0F);// Flip the Y-Axis 
            _g.TranslateTransform(0.0F, -(float)500);// Translate the drawing area accordingly            

            //-----------------------   
        }

        UnscaledGlyphPlanList _reusableUnscaledGlyphPlanList = new UnscaledGlyphPlanList();

        FormattedGlyphPlanList _ftmGlyphPlansList = new FormattedGlyphPlanList();
        VirtualTextSpanPrinter _virtualTextPrinter;

        void ShowMeasureBox2()
        {



        }
        void RenderAndShowMeasureBox()
        {
            bool flipY = chkFlipY.Checked;


            //set some Gdi+ props... 
            _g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            _g.Clear(Color.White);

            Typography.OpenFont.Typeface typeface = _currentTextPrinter.Typeface;
            Typography.OpenFont.Extensions.TypefaceExtensions.UpdateAllCffGlyphBounds(typeface);


            float pxscale = typeface.CalculateScaleToPixelFromPointSize(_currentTextPrinter.FontSizeInPoints);
            int lineSpacing = (int)System.Math.Ceiling((double)typeface.CalculateLineSpacing(LineSpacingChoice.TypoMetric) * pxscale);


            if (flipY)
            {
                //credit:
                //http://stackoverflow.com/questions/1485745/flip-coordinates-when-drawing-to-control
                _g.ScaleTransform(1.0F, -1.0F);// Flip the Y-Axis 
                _g.TranslateTransform(0.0F, -500);// Translate the drawing area accordingly   
            }


            //--------------------------------
            //textspan measurement sample
            //--------------------------------  
            _currentTextPrinter.HintTechnique = (HintTechnique)lstHintList.SelectedItem;
            _currentTextPrinter.PositionTechnique = (PositionTechnique)cmbPositionTech.SelectedItem;
            _currentTextPrinter.UpdateGlyphLayoutSettings();

            //render at specific pos
            float x_pos = 0, y_pos = lineSpacing * 2;

            char[] textBuffer = txtInputChar.Text.ToCharArray();

            //Example 1: this is a basic draw sample
            _currentTextPrinter.FillColor = Color.Black;
            _currentTextPrinter.TargetGraphics = _g;
            //_currentTextPrinter.DrawString(
            //    textBuffer,
            //     0,
            //     textBuffer.Length,
            //     x_pos,
            //     y_pos
            //    );
            //
            //--------------------------------------------------
            //Example 2: print glyph plan to 'user' list-> then draw it (or hold it/ not draw)                         
            //you can create you own class to hold userGlyphPlans.***
            //2.1
            if (chkEnableMultiTypefaces.Checked)
            {
                _ftmGlyphPlansList.Clear();
                _currentTextPrinter.GenerateGlyphPlans(textBuffer, 0, textBuffer.Length, _ftmGlyphPlansList);
                //2.2
                //and we can print the formatted glyph plan later.
                y_pos -= lineSpacing;//next line
                _currentTextPrinter.FillColor = Color.Red;

                _currentTextPrinter.DrawFromFormattedGlyphPlans(
                    _ftmGlyphPlansList,
                      x_pos,
                      y_pos
                 );


                //--------------------------------------------------
                //Example 3: MeasureString   
                //3.1 use data in formatted_glyph_plan_list to measure string => OK, OR 
                //3.2 use another 'light-weight' text printer to measure string (without drawing actual string)

                //3.1                  
                _currentTextPrinter.MeasureGlyphPlanList(_ftmGlyphPlansList, out int measureWidth);
                _g.DrawRectangle(Pens.Red, x_pos, y_pos, measureWidth, 30);


                float baseline = y_pos;

                //3.2 draw each glyph bounds
                {
                    int m = _ftmGlyphPlansList.Count;

                    for (int i = 0; i < m; ++i)
                    {
                        FormattedGlyphPlanSeq seq = _ftmGlyphPlansList[i];
                        ResolvedFont resolvedFont = seq.ResolvedFont;
                        Typeface localTypeface = resolvedFont.Typeface;

                        pxscale = resolvedFont.GetScaleToPixelFromPointInSize();
                        GlyphPlanSequence glyph_seq = seq.Seq;
                        var snapToPxScale = new GlyphPlanSequenceSnapPixelScaleLayout(seq.Seq, 0, glyph_seq.Count, pxscale);


                        x_pos += (resolvedFont.WhitespaceWidth * seq.PrefixWhitespaceCount);

                        int nn = 0;///
                        while (snapToPxScale.Read())
                        {
                            //float cx = (float)Math.Round(snapToPxScale.ExactX + x_pos);
                            //float cy = (float)Math.Floor(snapToPxScale.ExactY + baseline);

                            UnscaledGlyphPlan glyphPlan = glyph_seq[nn];
                            Glyph glyph = localTypeface.GetGlyph(glyphPlan.glyphIndex);

                            Bounds b = glyph.Bounds;
                            //
                            float xmin = b.XMin * pxscale;
                            float ymin = b.YMin * pxscale;
                            //
                            float xmax = b.XMax * pxscale;
                            float ymax = b.YMax * pxscale;
                            //
                            float glyph_x = x_pos + (glyphPlan.OffsetX * pxscale);
                            _g.DrawRectangle(Pens.Red, glyph_x + xmin, y_pos + ymin, xmax - xmin, ymax - ymin);

                            x_pos += glyphPlan.AdvanceX * pxscale;

                            nn++;
                        }

                        x_pos += (resolvedFont.WhitespaceWidth * seq.PostfixWhitespaceCount);

                    }
                }

                _ftmGlyphPlansList.Clear();
            }
            else
            {
                _reusableUnscaledGlyphPlanList.Clear();
                _currentTextPrinter.GenerateGlyphPlans(textBuffer, 0, textBuffer.Length, _reusableUnscaledGlyphPlanList);
                //2.2
                //and we can print the formatted glyph plan later.
                y_pos -= lineSpacing;//next line
                _currentTextPrinter.FillColor = Color.Red;


                //
                GlyphPlanSequence seq = new GlyphPlanSequence(_reusableUnscaledGlyphPlanList);

                _currentTextPrinter.DrawFromGlyphPlans(
                      seq,
                      x_pos,
                      y_pos
                 );

                //--------------------------------------------------
                //Example 3: MeasureString    

                float descending_px = _currentTextPrinter.FontDescedingPx;
                float ascending_px = _currentTextPrinter.FontAscendingPx;
                float lineSpacing_px = _currentTextPrinter.FontLineSpacingPx;
                float clippedHeight_px = _currentTextPrinter.FontClipHeightPx;

                _currentTextPrinter.MeasureGlyphPlanSeq(seq, out int textspanW);

                UnscaledGlyphPlanList glyphPlans = new UnscaledGlyphPlanList();
                _currentTextPrinter.GenerateGlyphPlans(textBuffer, 0, textBuffer.Length, glyphPlans);


                int j = glyphPlans.Count;
                float backup_xpos = x_pos;
                for (int i = 0; i < j; ++i)
                {
                    UnscaledGlyphPlan glyphPlan = glyphPlans[i];
                    Glyph glyph = typeface.GetGlyph(glyphPlan.glyphIndex);
                    //
                    Bounds b = glyph.Bounds;
                    //
                    float xmin = b.XMin * pxscale;
                    float ymin = b.YMin * pxscale;
                    //
                    float xmax = b.XMax * pxscale;
                    float ymax = b.YMax * pxscale;
                    //
                    float glyph_x = x_pos + (glyphPlan.OffsetX * pxscale);
                    _g.DrawRectangle(Pens.Red, glyph_x + xmin, y_pos + ymin, xmax - xmin, ymax - ymin);
                    x_pos += glyphPlan.AdvanceX * pxscale;
                }

                x_pos = backup_xpos;

                _g.FillRectangle(Brushes.Red, new RectangleF(0, 0, 5, 5));//reference point(0,0)
                _g.FillRectangle(Brushes.Green, new RectangleF(x_pos, y_pos, 3, 3));


                float x_pos2 = x_pos + textspanW + 10;


                _g.DrawRectangle(Pens.Black, x_pos, y_pos + descending_px, textspanW, clippedHeight_px);
                _g.DrawRectangle(Pens.Red, x_pos, y_pos + descending_px, textspanW, lineSpacing_px);

                _g.DrawLine(Pens.Blue, x_pos, y_pos, x_pos2, y_pos); //baseline
                _g.DrawLine(Pens.Green, x_pos, y_pos + descending_px, x_pos2, y_pos + descending_px);//descending
                _g.DrawLine(Pens.Magenta, x_pos, y_pos + ascending_px, x_pos2, y_pos + ascending_px);//ascending


                ////------------
                ////draw another line (for reference)
                y_pos -= lineSpacing;//next line


                _currentTextPrinter.FillColor = Color.Black;

                _currentTextPrinter.DrawFromGlyphPlans(
                      new GlyphPlanSequence(_reusableUnscaledGlyphPlanList),
                      x_pos,
                      y_pos
                 );
            }





            //transform back
            if (flipY)
            {
                _g.ScaleTransform(1.0F, -1.0F);// Flip the Y-Axis 
                _g.TranslateTransform(0.0F, -500);// Translate the drawing area accordingly   
            }

            //---------

            //txtMsgInfo.Text = "choice:" + choice.ToString() + "=" + lineSpacing.ToString();
        }
        private void cmdMeasureTextSpan_Click(object sender, System.EventArgs e)
        {
            RenderAndShowMeasureBox();
        }

        private void checkBox1_CheckedChanged(object sender, System.EventArgs e)
        {
            RenderAndShowMeasureBox();
        }

        private void button2_Click(object sender, System.EventArgs e)
        {
            ShowMeasureBox2();
        }
    }
}
