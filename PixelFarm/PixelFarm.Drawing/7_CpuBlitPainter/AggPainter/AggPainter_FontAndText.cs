//MIT, 2016-present, WinterDev

using System;
using PixelFarm.Drawing;

namespace PixelFarm.CpuBlit
{

    partial class AggPainter
    {
        //font
        RequestFont _currentFont;
        ITextPrinter _textPrinter;

        public ITextPrinter TextPrinter
        {
            get => _textPrinter;
            set
            {
                _textPrinter = value;
                if (_textPrinter != null)
                {
                    _textPrinter.ChangeFont(_currentFont);
                }
            }
        }

        public override RequestFont CurrentFont
        {
            get => _currentFont;
            set
            {
                _currentFont = value;
                //this request font must resolve to actual font
                //within canvas *** 
                //TODO: review drawing string  with agg here 
                if (_textPrinter != null && value != null)
                {
                    _textPrinter.ChangeFont(value);
                }
            }
        }

        public override void DrawString(
           string text,
           double x,
           double y)
        {
            //TODO: review drawing string  with agg here   
            if (_textPrinter != null)
            {
                if (_orientation == RenderSurfaceOrientation.LeftBottom)
                {
                    _textPrinter.DrawString(text, x, y);
                }
                else
                {
                    //from current point size 
                    //we need line height of current font size
                    //then we will start on 'base line'

                    _textPrinter.DrawString(text, x, this.Height - y);
                }

            }
        }
        public override void DrawString(RenderVxFormattedString renderVx, double x, double y)
        {
            //draw string from render vx
            if (_textPrinter != null)
            {
                _textPrinter.DrawString(renderVx, x, y);
            }
        }
        public override RenderVxFormattedString CreateRenderVx(string textspan)
        {

            var renderVxFmtStr = new AggRenderVxFormattedString(textspan);
            if (_textPrinter != null)
            {
                char[] buffer = textspan.ToCharArray();
                _textPrinter.PrepareStringForRenderVx(renderVxFmtStr, buffer, 0, buffer.Length);

            }
            return renderVxFmtStr;
        }



    }
}