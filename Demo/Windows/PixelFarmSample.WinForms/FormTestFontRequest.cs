//MIT, 2020, WinterDev
using System;
using System.Collections.Generic;
using System.Drawing;

using System.IO;
using System.Windows.Forms;

using PixelFarm.CpuBlit;
using PixelFarm.CpuBlit.BitmapAtlas;
using PixelFarm.Contours;

using Typography.OpenFont;
using Typography.TextLayout;
using Typography.Contours;
using PixelFarm.Drawing;

namespace SampleWinForms
{
    public partial class FormTestFontRequest : Form
    {
        Graphics _g;
        AggPainter _painter;
        MemBitmap _destImg;
        Bitmap _winBmp;

        PixelFarm.Drawing.VxsTextPrinter _devVxsTextPrinter = null;

        bool _readyToRender;
        PixelFarm.Drawing.OpenFontTextService _textService;
        PixelFarm.Drawing.Color _grayColor = new PixelFarm.Drawing.Color(0xFF, 0x80, 0x80, 0x80);

        public FormTestFontRequest()
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
        void InitGraphics()
        {
            //INIT ONCE
            if (_g != null) return;
            //

            _destImg = new MemBitmap(800, 600);
            _painter = AggPainter.Create(_destImg);
            _winBmp = new Bitmap(_destImg.Width, _destImg.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            _g = this.CreateGraphics();

            _painter.CurrentFont = new PixelFarm.Drawing.RequestFont("Source Sans Pro", 10);


            _textService = new PixelFarm.Drawing.OpenFontTextService();
            _textService.LoadFontsFromFolder("../../../TestFonts");
            _textService.UpdateUnicodeRanges();

            _devVxsTextPrinter = new PixelFarm.Drawing.VxsTextPrinter(_painter, _textService);
            _devVxsTextPrinter.SetSvgBmpBuilderFunc(PaintLab.SvgBuilderHelper.ParseAndRenderSvg);

            _devVxsTextPrinter.ScriptLang = new ScriptLang("latn");
            _devVxsTextPrinter.PositionTechnique = Typography.TextLayout.PositionTechnique.OpenFont;

            //Alternative Typeface selector..
            var myAlternativeTypefaceSelector = new PixelFarm.Drawing.MyAlternativeTypefaceSelector();
            {
                //arabic

                //1. create prefer typeface list for arabic script
                var preferTypefaces = new PixelFarm.Drawing.MyAlternativeTypefaceSelector.PreferTypefaceList();
                preferTypefaces.AddTypefaceName("Noto Sans Arabic UI");

                //2. set unicode ranges and prefered typeface list. 
                myAlternativeTypefaceSelector.SetPreferTypefaces(
                     new[]{Typography.TextBreak.Unicode13RangeInfoList.Arabic,
                               Typography.TextBreak.Unicode13RangeInfoList.Arabic_Supplement,
                               Typography.TextBreak.Unicode13RangeInfoList.Arabic_Extended_A},
                    preferTypefaces);
            }
            {
                //latin

                var preferTypefaces = new PixelFarm.Drawing.MyAlternativeTypefaceSelector.PreferTypefaceList();
                preferTypefaces.AddTypefaceName("Sarabun");

                myAlternativeTypefaceSelector.SetPreferTypefaces(
                     new[]{Typography.TextBreak.Unicode13RangeInfoList.C0_Controls_and_Basic_Latin,
                               Typography.TextBreak.Unicode13RangeInfoList.C1_Controls_and_Latin_1_Supplement,
                               Typography.TextBreak.Unicode13RangeInfoList.Latin_Extended_A,
                               Typography.TextBreak.Unicode13RangeInfoList.Latin_Extended_B,
                     },
                    preferTypefaces);
            }

            _devVxsTextPrinter.AlternativeTypefaceSelector = myAlternativeTypefaceSelector;
        }

        void DrawStringToMemBitmap(RequestFont reqFont, string textOutput, float x_pos, float y_pos, int repeatLines = 1)
        {

            ResolvedFont resFont = _textService.ResolveFont(reqFont);


            PixelFarm.Drawing.VxsTextPrinter _selectedTextPrinter = _devVxsTextPrinter;

            _painter.UseLcdEffectSubPixelRendering = true;
            _painter.FillColor = PixelFarm.Drawing.Color.Black;

            _selectedTextPrinter = _devVxsTextPrinter;
            _selectedTextPrinter.Typeface = resFont.Typeface;
            _selectedTextPrinter.FontSizeInPoints = resFont.SizeInPoints;
            _selectedTextPrinter.ScriptLang = new ScriptLang("latn");
            _selectedTextPrinter.PositionTechnique = PositionTechnique.OpenFont;

            _selectedTextPrinter.HintTechnique = HintTechnique.None;
            _selectedTextPrinter.EnableLigature = true;
            _selectedTextPrinter.SimulateSlant = false;

            _selectedTextPrinter.EnableMultiTypefaces = true; //*** for auto typeface selection***


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
            {
                RequestFont reqFont1 = new RequestFont("Source Sans Pro", 20);
                DrawStringToMemBitmap(reqFont1, textOutput, 0, 0);
            }
            {
                RequestFont reqFont1 = new RequestFont("Source Sans Pro", 18);

                //
                //***TODO: please improve 1st loading time for CJK font***
                //
                textOutput = "😁こんにちは, 你好, 여보세요 abc 0123 ";

                DrawStringToMemBitmap(reqFont1, textOutput, 100, 50);
            }
            {
                RequestFont reqFont1 = new RequestFont("Source Sans Pro", 30);

                //TODO: optimize 1st loading time for CJK font
                textOutput = "شمس حب ";

                DrawStringToMemBitmap(reqFont1, textOutput, 150, 100);
            }
            CopyMemBitmapToScreen();
        }
    }
}
