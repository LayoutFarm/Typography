//MIT, 2016-present, WinterDev

using System;
using PixelFarm.Drawing;

namespace PixelFarm.CpuBlit
{
    public interface IAggTextPrinter
    {
        /// <summary>
        /// render from RenderVxFormattedString object to specific pos
        /// </summary>
        /// <param name="renderVx"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        void DrawString(AggRenderVxFormattedString renderVx, double left, double top);
        void PrepareStringForRenderVx(AggRenderVxFormattedString renderVx, char[] text, int startAt, int len);
        void PrepareStringForRenderVx(AggRenderVxFormattedString renderVx, IFormattedGlyphPlanList fmtGlyphPlans);
        int CurrentLineSpaceHeight { get; }
        void ChangeFont(RequestFont font);
        void ChangeFillColor(Color fillColor);
        void ChangeStrokeColor(Color strokColor);
        TextBaseline TextBaseline { get; set; }
        void DrawString(char[] text, int startAt, int len, double left, double top);
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
                if (_currentFont != null)
                {
                    _textPrinter?.ChangeFont(_currentFont);
                }
            }
        }


        public override RequestFont CurrentFont
        {
            get => _currentFont;
            set
            {
                _currentFont = value;
                //                
                if (_textPrinter != null && value != null)
                {
                    _textPrinter.ChangeFont(value);
                }
            }
        }

        public int CurrentLineSpaceHeight => (_textPrinter != null) ? _textPrinter.CurrentLineSpaceHeight : 0;

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
                    char[] buffer = text.ToCharArray();
                    _textPrinter.DrawString(buffer, 0, buffer.Length, x, y);
                }
                else
                {
                    //from current point size 
                    //we need line height of current font size
                    //then we will start on 'base line'
                    char[] buffer = text.ToCharArray();
                    _textPrinter.DrawString(buffer, 0, buffer.Length, x, this.Height - y);
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
        public override RenderVxFormattedString CreateRenderVx(IFormattedGlyphPlanList formattedGlyphPlans)
        {
            var renderVxFmtStr = new AggRenderVxFormattedString();
            if (_textPrinter != null)
            {
                _textPrinter.PrepareStringForRenderVx(renderVxFmtStr, formattedGlyphPlans);
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