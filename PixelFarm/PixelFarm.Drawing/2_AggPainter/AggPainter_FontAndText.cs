//MIT, 2016-present, WinterDev

using System;
using PixelFarm.Drawing;

namespace PixelFarm.CpuBlit
{
    public interface IAggTextPrinter : ITextPrinter
    {
        /// <summary>
        /// render from RenderVxFormattedString object to specific pos
        /// </summary>
        /// <param name="renderVx"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        void DrawString(AggRenderVxFormattedString renderVx, double left, double top);
        void PrepareStringForRenderVx(AggRenderVxFormattedString renderVx, char[] text, int startAt, int len);
    }

    partial class AggPainter
    {
        //font
        RequestFont _currentFont;
        IAggTextPrinter _textPrinter;

        public IAggTextPrinter TextPrinter
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
                if (_orientation == RenderSurfaceOriginKind.LeftBottom)
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
        public override void DrawString(RenderVxFormattedString renderVx, double left, double top)
        {
            //draw string from render vx 

            _textPrinter?.DrawString((AggRenderVxFormattedString)renderVx, left, top);
        }
        public override RenderVxFormattedString CreateRenderVx(string textspan)
        {

            var renderVxFmtStr = new AggRenderVxFormattedString();
            if (_textPrinter != null)
            {
                char[] buffer = textspan.ToCharArray();
                _textPrinter.PrepareStringForRenderVx(renderVxFmtStr, buffer, 0, buffer.Length);

            }
            return renderVxFmtStr;
        }
        public override RenderVxFormattedString CreateRenderVx(char[] textspanBuff, int startAt, int len)
        {
            var renderVxFmtStr = new AggRenderVxFormattedString();
            if (_textPrinter != null)
            {
                _textPrinter.PrepareStringForRenderVx(renderVxFmtStr, textspanBuff, startAt, len);
            }
            return renderVxFmtStr;
        }
    }
}