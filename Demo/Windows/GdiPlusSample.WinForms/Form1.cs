//MIT, 2016-present, WinterDev
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
// 
using Typography.TextLayout;
using Typography.Contours;
using Typography.TextServices;
using Typography.OpenFont;
using Typography.OpenFont.Extensions;
namespace SampleWinForms
{
    public partial class Form1 : Form
    {
        Graphics g;
        //for this sample code,
        //create text printer env for developer.
        DevGdiTextPrinter _currentTextPrinter = new DevGdiTextPrinter();
        InstalledFontCollection installedFontCollection;
        TypefaceStore _typefaceStore;
        public Form1()
        {
            InitializeComponent();

            //choose Thai script for 'complex script' testing.
            //you can change this to test other script.
            _currentTextPrinter.ScriptLang = Typography.OpenFont.ScriptLangs.Thai;
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
            lstHintList.Items.Add(TrueTypeHintTechnique.None);
            lstHintList.Items.Add(TrueTypeHintTechnique.Instructions);
            lstHintList.Items.Add(TrueTypeHintTechnique.Instructions_VerticalOnly);
            //lstHintList.Items.Add(TrueTypeHintTechnique.CustomAutoFit);
            lstHintList.SelectedIndex = 0;
            lstHintList.SelectedIndexChanged += (s, e) => UpdateRenderOutput();
            //---------- 
            txtInputChar.TextChanged += (s, e) => UpdateRenderOutput();
            //

            //1. create font collection             
            installedFontCollection = new InstalledFontCollection();
            //2. set some essential handler
            installedFontCollection.SetFontNameDuplicatedHandler((f1, f2) => FontNameDuplicatedDecision.Skip);

            installedFontCollection.LoadFontsFromFolder("../../../TestFonts_Err");
            installedFontCollection.LoadFontsFromFolder("../../../TestFonts");
            //installedFontCollection.LoadSystemFonts();

            //---------- 
            //show result
            InstalledFont selectedFF = null;
            int selected_index = 0;
            int ffcount = 0;
            bool found = false;
            foreach (InstalledFont ff in installedFontCollection.GetInstalledFontIter())
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
            _typefaceStore = new TypefaceStore();
            _typefaceStore.FontCollection = installedFontCollection;
            //set default font for current text printer
            _currentTextPrinter.Typeface = _typefaceStore.GetTypeface(selectedFF);
            //---------- 


            if (selected_index < 0) { selected_index = 0; }
            lstFontList.SelectedIndex = selected_index;
            lstFontList.SelectedIndexChanged += (s, e) =>
            {
                InstalledFont ff = lstFontList.SelectedItem as InstalledFont;
                if (ff != null)
                {
                    _currentTextPrinter.Typeface = _typefaceStore.GetTypeface(ff);
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
            if (g == null)
            {
                g = this.CreateGraphics();
            }
            if (string.IsNullOrEmpty(this.txtInputChar.Text))
            {
                return;
            }
            //-----------------------  
            //set some props ...
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.Clear(Color.White);
            //credit:
            //http://stackoverflow.com/questions/1485745/flip-coordinates-when-drawing-to-control
            g.ScaleTransform(1.0F, -1.0F);// Flip the Y-Axis 
            g.TranslateTransform(0.0F, -(float)500);// Translate the drawing area accordingly   


            _currentTextPrinter.FillBackground = this.chkFillBackground.Checked;
            _currentTextPrinter.DrawOutline = this.chkBorder.Checked;

            //-----------------------  
            _currentTextPrinter.TrueTypeHintTechnique = (TrueTypeHintTechnique)lstHintList.SelectedItem;
            _currentTextPrinter.PositionTechnique = (PositionTechnique)cmbPositionTech.SelectedItem;
            _currentTextPrinter.TargetGraphics = g;
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
            g.ScaleTransform(1.0F, -1.0F);// Flip the Y-Axis 
            g.TranslateTransform(0.0F, -(float)500);// Translate the drawing area accordingly            

            //-----------------------   
        }

        UnscaledGlyphPlanList _reusableUnscaledGlyphPlanList = new UnscaledGlyphPlanList();


        void RenderAndShowMeasureBox()
        {
            bool flipY = chkFlipY.Checked;

           

            //set some Gdi+ props... 
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.Clear(Color.White);

            Typography.OpenFont.Typeface typeface = _currentTextPrinter.Typeface;
            Typography.OpenFont.TypefaceExtension2.UpdateAllCffGlyphBounds(typeface);
            

            float pxscale = typeface.CalculateScaleToPixelFromPointSize(_currentTextPrinter.FontSizeInPoints); 
            int lineSpacing = (int)System.Math.Ceiling((double)typeface.CalculateLineSpacing(LineSpacingChoice.TypoMetric) * pxscale);
            

            if (flipY)
            {
                //credit:
                //http://stackoverflow.com/questions/1485745/flip-coordinates-when-drawing-to-control
                g.ScaleTransform(1.0F, -1.0F);// Flip the Y-Axis 
                g.TranslateTransform(0.0F, -500);// Translate the drawing area accordingly   
            }


            //--------------------------------
            //textspan measurement sample
            //--------------------------------  
            _currentTextPrinter.TrueTypeHintTechnique = (TrueTypeHintTechnique)lstHintList.SelectedItem;
            _currentTextPrinter.PositionTechnique = (PositionTechnique)cmbPositionTech.SelectedItem;
            _currentTextPrinter.UpdateGlyphLayoutSettings();

            //render at specific pos
            float x_pos = 0, y_pos = lineSpacing * 2; 

            char[] textBuffer = txtInputChar.Text.ToCharArray();

            //Example 1: this is a basic draw sample
            _currentTextPrinter.FillColor = Color.Black;
            _currentTextPrinter.TargetGraphics = g;
            _currentTextPrinter.DrawString(
                textBuffer,
                 0,
                 textBuffer.Length,
                 x_pos,
                 y_pos
                );
            //
            //--------------------------------------------------
            //Example 2: print glyph plan to 'user' list-> then draw it (or hold it/ not draw)                         
            //you can create you own class to hold userGlyphPlans.***
            //2.1
            _reusableUnscaledGlyphPlanList.Clear();
            _currentTextPrinter.GenerateGlyphPlan(textBuffer, 0, textBuffer.Length, _reusableUnscaledGlyphPlanList);
            //2.2
            //and we can print the formatted glyph plan later.
            y_pos -= lineSpacing;//next line
            _currentTextPrinter.FillColor = Color.Red;

            _currentTextPrinter.DrawFromGlyphPlans(
                  new GlyphPlanSequence(_reusableUnscaledGlyphPlanList),
                  x_pos,
                  y_pos
             );

            //Example 3: MeasureString    
            UnscaledGlyphPlanList glyphPlans = new UnscaledGlyphPlanList();
            _currentTextPrinter.GlyphLayoutMan.GenerateUnscaledGlyphPlans(glyphPlans);
            MeasuredStringBox strBox = _currentTextPrinter.GlyphLayoutMan.LayoutAndMeasureString(
              textBuffer, 0, textBuffer.Length,
              _currentTextPrinter.FontSizeInPoints);

            int j = glyphPlans.Count;
            float backup_xpos = x_pos;
            for (int i = 0; i < j; ++i)
            {
                UnscaledGlyphPlan glyphPlan = glyphPlans[i];
                Typography.OpenFont.Glyph glyph = typeface.GetGlyphByIndex(glyphPlan.glyphIndex);
                //
                Typography.OpenFont.Bounds b = glyph.Bounds;
                //
                float xmin = b.XMin * pxscale;
                float ymin = b.YMin * pxscale;
                //
                float xmax = b.XMax * pxscale;
                float ymax = b.YMax * pxscale;
                //
                float glyph_x = x_pos + glyphPlan.OffsetX;
                g.DrawRectangle(Pens.Red, glyph_x + xmin, y_pos + ymin, xmax - xmin, ymax - ymin);
                x_pos += glyphPlan.AdvanceX * pxscale;
            }

            x_pos = backup_xpos;

            g.FillRectangle(Brushes.Red, new RectangleF(0, 0, 5, 5));//reference point(0,0)
            g.FillRectangle(Brushes.Green, new RectangleF(x_pos, y_pos, 3, 3));


            float x_pos2 = x_pos + strBox.width + 10;


            g.DrawRectangle(Pens.Black, x_pos, y_pos + strBox.DescendingInPx, strBox.width, strBox.ClipHeightInPx);
            g.DrawRectangle(Pens.Red, x_pos, y_pos + strBox.DescendingInPx, strBox.width, strBox.LineSpaceInPx);

            g.DrawLine(Pens.Blue, x_pos, y_pos, x_pos2, y_pos); //baseline
            g.DrawLine(Pens.Green, x_pos, y_pos + strBox.DescendingInPx, x_pos2, y_pos + strBox.DescendingInPx);//descending
            g.DrawLine(Pens.Magenta, x_pos, y_pos + strBox.AscendingInPx, x_pos2, y_pos + strBox.AscendingInPx);//ascending


            ////------------
            ////draw another line (for reference)
            y_pos -= lineSpacing;//next line


            _currentTextPrinter.FillColor = Color.Black;

            _currentTextPrinter.DrawFromGlyphPlans(
                  new GlyphPlanSequence(_reusableUnscaledGlyphPlanList),
                  x_pos,
                  y_pos
             );
            //transform back
            if (flipY)
            {
                g.ScaleTransform(1.0F, -1.0F);// Flip the Y-Axis 
                g.TranslateTransform(0.0F, -500);// Translate the drawing area accordingly   
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


    }
}
