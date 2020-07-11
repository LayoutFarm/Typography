//MIT, 2020, WinterDev
using System;
using System.Collections.Generic;
using System.Drawing;

using System.IO;
using System.Windows.Forms;

using PixelFarm.CpuBlit;
using Typography.OpenFont;
using Typography.OpenFont.Extensions;
using Typography.TextLayout;
using Typography.Contours;
using Typography.Text;
using PixelFarm.Drawing;

namespace SampleWinForms
{
    public partial class FormTestRequestFont : Form
    {
        Graphics _g;
        AggPainter _painter;
        MemBitmap _destImg;
        Bitmap _winBmp;

        PixelFarm.Drawing.VxsTextPrinter _devVxsTextPrinter = null;

        bool _readyToRender;
        Typography.Text.OpenFontTextService _textService;
        PixelFarm.Drawing.Color _grayColor = new PixelFarm.Drawing.Color(0xFF, 0x80, 0x80, 0x80);
        PixelFarm.Drawing.RequestFont _defaultReqFont;

        public FormTestRequestFont()
        {
            InitializeComponent();
        }

        private void FormTestFontRequest_Load(object sender, EventArgs e)
        {
            if (_g == null)
            {
                InitGraphics();
            }
        }

        Typography.Text.MyAlternativeTypefaceSelector _myAlternativeTypefaceSelector;
        void InitGraphics()
        {
            //INIT ONCE
            if (_g != null) return;
            //

            _destImg = new MemBitmap(800, 600);
            _painter = AggPainter.Create(_destImg);
            _winBmp = new Bitmap(_destImg.Width, _destImg.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            _g = this.CreateGraphics();

            _defaultReqFont = new PixelFarm.Drawing.RequestFont("Source Sans Pro", 10);
            _painter.CurrentFont = _defaultReqFont;


            _textService = new Typography.Text.OpenFontTextService();
            _textService.LoadFontsFromFolder("../../../TestFonts");
            _textService.UpdateUnicodeRanges();

            _devVxsTextPrinter = new PixelFarm.Drawing.VxsTextPrinter(_painter, _textService);
            _devVxsTextPrinter.SetSvgBmpBuilderFunc(PaintLab.SvgBuilderHelper.ParseAndRenderSvg);

            _devVxsTextPrinter.ScriptLang = new ScriptLang(ScriptTagDefs.Latin.Tag);
            _devVxsTextPrinter.PositionTechnique = Typography.TextLayout.PositionTechnique.OpenFont;

            //Alternative Typeface selector..
            _myAlternativeTypefaceSelector = new Typography.Text.MyAlternativeTypefaceSelector();
            {
                //------------
                //TODO: review this again
                //load from config?
                //------------

                //arabic
                //1. create prefer typeface list for arabic script
                var preferTypefaces = new Typography.FontManagement.PreferredTypefaceList();
                preferTypefaces.AddTypefaceName("Noto Sans Arabic UI");

                //2. set unicode ranges and prefered typeface list. 
                _myAlternativeTypefaceSelector.SetPreferredTypefaces(
                     new[]{Typography.TextBreak.Unicode13RangeInfoList.Arabic,
                               Typography.TextBreak.Unicode13RangeInfoList.Arabic_Supplement,
                               Typography.TextBreak.Unicode13RangeInfoList.Arabic_Extended_A},
                    preferTypefaces);
            }
            {
                //latin

                var preferTypefaces = new Typography.FontManagement.PreferredTypefaceList();
                preferTypefaces.AddTypefaceName("Sarabun");

                _myAlternativeTypefaceSelector.SetPreferredTypefaces(
                     new[]{Typography.TextBreak.Unicode13RangeInfoList.C0_Controls_and_Basic_Latin,
                           Typography.TextBreak.Unicode13RangeInfoList.C1_Controls_and_Latin_1_Supplement,
                           Typography.TextBreak.Unicode13RangeInfoList.Latin_Extended_A,
                           Typography.TextBreak.Unicode13RangeInfoList.Latin_Extended_B,
                     },
                    preferTypefaces);
            }

            _devVxsTextPrinter.AlternativeTypefaceSelector = _myAlternativeTypefaceSelector;
        }


        void DrawStringToMemBitmap(RequestFont reqFont, string textOutput, float x_pos, float y_pos, int repeatLines = 1)
        {

            ResolvedFont resolvedFont = _textService.ResolveFont(reqFont);
            if (resolvedFont == null)
            {
                //we dont' have 
                resolvedFont = _textService.ResolveFont(_defaultReqFont);
                if (resolvedFont == null) { throw new NotSupportedException(); }

                //use alternative typeface, but use reqFont's Size
                resolvedFont = new ResolvedFont(resolvedFont.Typeface, reqFont.SizeInPoints);
            }

            //check if reqFont has alternative or not

            _myAlternativeTypefaceSelector.SetCurrentReqFont(reqFont, _textService);

            PixelFarm.Drawing.VxsTextPrinter _selectedTextPrinter = _devVxsTextPrinter;
            _painter.UseLcdEffectSubPixelRendering = true;
            _painter.FillColor = PixelFarm.Drawing.Color.Black;

            _selectedTextPrinter = _devVxsTextPrinter;

            _selectedTextPrinter.FontSizeInPoints = resolvedFont.SizeInPoints;
            _selectedTextPrinter.Typeface = resolvedFont.Typeface;
            _selectedTextPrinter.ScriptLang = new ScriptLang(ScriptTagDefs.Latin.Tag);
            _selectedTextPrinter.PositionTechnique = PositionTechnique.OpenFont;

            _selectedTextPrinter.HintTechnique = HintTechnique.None;
            _selectedTextPrinter.EnableLigature = true;
            _selectedTextPrinter.SimulateSlant = false;

            _selectedTextPrinter.EnableMultiTypefaces = true; //*** for auto typeface selection*** 

            //_selectedTextPrinter.TextBaseline = PixelFarm.Drawing.TextBaseline.Alphabetic;
            //_selectedTextPrinter.TextBaseline = PixelFarm.Drawing.TextBaseline.Bottom;
            _selectedTextPrinter.TextBaseline = PixelFarm.Drawing.TextBaseline.Top;

            //test print 3 lines
            //#if DEBUG
            //            DynamicOutline.dbugTestNewGridFitting = _contourAnalysisOpts.EnableGridFit;
            //            DynamicOutline.dbugActualPosToConsole = _contourAnalysisOpts.WriteFitOutputToConsole;
            //            DynamicOutline.dbugUseHorizontalFitValue = _contourAnalysisOpts.UseHorizontalFitAlignment;
            //#endif


            char[] printTextBuffer = textOutput.ToCharArray();


            float lineSpacingPx = _selectedTextPrinter.FontLineSpacingPx;

            const int REF_LINE_LEN = 300;

            for (int i = 0; i < repeatLines; ++i)
            {
                _selectedTextPrinter.DrawString(printTextBuffer, x_pos, y_pos);
#if DEBUG
                //show debug info...
                var prevColor = _painter.FillColor;
                var prevStrokColor = _painter.StrokeColor;
                _painter.FillColor = PixelFarm.Drawing.Color.Red;
                _painter.FillRect(x_pos, y_pos, 5, 5); // start point

                //see   //https://developer.mozilla.org/en-US/docs/Web/API/CanvasRenderingContext2D/textBaseline
                switch (_selectedTextPrinter.TextBaseline)
                {
                    default:
                        {
                            System.Diagnostics.Debug.WriteLine("UNIMPLEMENTED" + _selectedTextPrinter.TextBaseline.ToString());
                            goto case PixelFarm.Drawing.TextBaseline.Alphabetic;//
                        }
                    case PixelFarm.Drawing.TextBaseline.Alphabetic:
                        {
                            //alphabetic baseline
                            _painter.StrokeColor = _grayColor;
                            _painter.DrawLine(x_pos,           /**/ y_pos,
                                              x_pos + REF_LINE_LEN, y_pos);

                            _painter.StrokeColor = PixelFarm.Drawing.Color.Blue;
                            _painter.DrawLine(x_pos,           /**/ y_pos - _selectedTextPrinter.FontDescedingPx,
                                              x_pos + REF_LINE_LEN, y_pos - _selectedTextPrinter.FontDescedingPx);//bottom most

                        }
                        break;
                    case PixelFarm.Drawing.TextBaseline.Top:
                        {
                            //alphabetic baseline
                            _painter.StrokeColor = _grayColor;
                            _painter.DrawLine(x_pos,           /**/ y_pos + _selectedTextPrinter.FontAscendingPx,
                                              x_pos + REF_LINE_LEN, y_pos + _selectedTextPrinter.FontAscendingPx);
                            //em bottom
                            _painter.StrokeColor = PixelFarm.Drawing.Color.Blue;
                            _painter.DrawLine(x_pos,           /**/ y_pos + (_selectedTextPrinter.FontAscendingPx - _selectedTextPrinter.FontDescedingPx),
                                              x_pos + REF_LINE_LEN, y_pos + (_selectedTextPrinter.FontAscendingPx - _selectedTextPrinter.FontDescedingPx));//bottom most


                        }
                        break;
                    case PixelFarm.Drawing.TextBaseline.Bottom:
                        {
                            //alphabetic baseline
                            _painter.StrokeColor = _grayColor;
                            _painter.DrawLine(x_pos,           /**/ y_pos + _selectedTextPrinter.FontDescedingPx,
                                              x_pos + REF_LINE_LEN, y_pos + _selectedTextPrinter.FontDescedingPx);
                            //em bottom
                            _painter.StrokeColor = PixelFarm.Drawing.Color.Blue;
                            _painter.DrawLine(x_pos,           /**/ y_pos,
                                              x_pos + REF_LINE_LEN, y_pos);//bottom most 
                        }
                        break;
                }


                _painter.FillColor = prevColor;
                _painter.StrokeColor = prevColor;
#endif
                y_pos += (_selectedTextPrinter.FontAscendingPx - _selectedTextPrinter.FontDescedingPx);

            }

            //reset
            _myAlternativeTypefaceSelector.SetCurrentReqFont(null, null);

        }

        void CopyMemBitmapToScreen()
        {

            //copy from Agg's memory buffer to gdi 
            PixelFarm.CpuBlit.BitmapHelper.CopyToGdiPlusBitmapSameSizeNotFlip(_destImg, _winBmp);
            _g.Clear(System.Drawing.Color.White);
            _g.DrawImage(_winBmp, new System.Drawing.Point(0, 0));
        }

        private void button1_Click(object sender, EventArgs e)
        {

            //clear previous draw
            _painter.Clear(PixelFarm.Drawing.Color.White);

            string textOutput = "Hello!";
            string textOutput2 = "شمس حب ";

            textOutput += textOutput2;
            {
                //RequestFont reqFont1 = new RequestFont("Source Sans Pro", 20);
                RequestFont reqFont1 = new RequestFont("Droid Sans", 20);
                DrawStringToMemBitmap(reqFont1, textOutput, 0, 50);
            }

            //{
            //    RequestFont reqFont1 = new RequestFont("Source Sans Pro", 18);

            //    "Source Sans Pro" does not have arabic , CJK, and Emoji glyphs
            //     so this will use AlternativeTypefaceSelector to select a proper typeface for
            //     a specific unicode range 
            //     eg. for Emoticon=> use TwitterColorEmoji (global settings)
            //             CJK => use "Noto CJK" (global settings) 
            //             Arabic => use "Noto Sans Arabic UI" 



            //    //***TODO: please improve 1st loading time for CJK font***

            //    textOutput = "😁こんにちは, 你好, 여보세요 abc 0123 ";

            //    DrawStringToMemBitmap(reqFont1, textOutput, 100, 50);
            //}


            //{
            //    RequestFont reqFont1 = new RequestFont("Source Sans Pro", 30);

            //    //"Source Sans Pro" does not have arabic glyphs
            //    //so this will switch to "Noto Sans Arabic UI" as described in AlternativeTypefaceSelector above.

            //    textOutput = "شمس حب ";

            //    //DrawStringToMemBitmap(reqFont1, textOutput, 150, 100);
            //    DrawStringToMemBitmap(reqFont1, textOutput, 0, 0);
            //}

            {
                //use Roboto 
                RequestFont reqFont1 = new RequestFont("Roboto", 20);

                textOutput = "hello Roboto";

                DrawStringToMemBitmap(reqFont1, textOutput, 0, 150);
            }

            CopyMemBitmapToScreen();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //test request missing typeface

            //clear previous draw
            _painter.Clear(PixelFarm.Drawing.Color.White);

            string textOutput = "Hello! 1";

            {
                //example1 
                //we have Roboto
                RequestFont reqFont1 = new RequestFont("Roboto", 20);
                DrawStringToMemBitmap(reqFont1, textOutput, 0, 30);
            }

            {
                //example2 
                //we don't have Roboto-X, 
                //the printer should switch back to use _defaultReqFont 
                textOutput = "Hello! 2";
                RequestFont reqFont1 = new RequestFont("Roboto-X", 20);
                DrawStringToMemBitmap(reqFont1, textOutput, 0, 100);
            }

            {
                //example3 
                //we don't have Roboto-X, 
                //but we add alternative to RequestFont
                //that if the system does not found Roboto-X
                //then should use alternative Asana Math
                textOutput = "Hello! 3";
                RequestFont reqFont1 = new RequestFont("Roboto-X", 20, PixelFarm.Drawing.FontStyle.Regular,
                    new[]
                    {
                       new RequestFont.Choice("Asana Math",20)
                    });

                DrawStringToMemBitmap(reqFont1, textOutput, 0, 150);
            }
            {
                //example4 
                //we use explicit typeface name for our req font
                textOutput = "Hello! 4";
                RequestFont reqFont1 = RequestFont.FromFile("Test/latinmodern-math.otf", 30);
                DrawStringToMemBitmap(reqFont1, textOutput, 0, 200);
            }

            CopyMemBitmapToScreen();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //clear previous draw
            _painter.Clear(PixelFarm.Drawing.Color.White);

            string textOutput = "Hello! 1";

            {
                //example1 
                //we have Roboto

                textOutput = "Hello! 1😁";
                RequestFont reqFont1 = new RequestFont("Roboto", 20, PixelFarm.Drawing.FontStyle.Regular,
                   new[]
                   {
                       new RequestFont.Choice("Asana Math",20)
                   });
                DrawStringToMemBitmap(reqFont1, textOutput, 0, 50);
            }

            {
                //example2  

                //for Emoji=> our System default=> TwitterColorEmoji
                //and in this case we want to specific that we want to use FireFoxColor Emoji instead 
                textOutput = "Hello! 2😁";
                RequestFont reqFont1 = new RequestFont("Roboto", 20, PixelFarm.Drawing.FontStyle.Regular,
                   new[]
                   {
                       new RequestFont.Choice("Asana Math",20),
                       new RequestFont.Choice("Firefox Emoji",20),
                   });
                DrawStringToMemBitmap(reqFont1, textOutput, 0, 100);
            }

            {
                //example3 
                //use Droid Sans Fallback for CJK 

                textOutput = "你好 Hello! 3 😁";
                RequestFont reqFont1 = new RequestFont("Droid Sans Fallback", 20, PixelFarm.Drawing.FontStyle.Regular,
                   new[]
                   {
                       new RequestFont.Choice("Asana Math",20)
                   });
                DrawStringToMemBitmap(reqFont1, textOutput, 0, 150);
            }

            CopyMemBitmapToScreen();
        }
    }
}
