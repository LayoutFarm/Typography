//MIT, 2014-present, WinterDev 

namespace PixelFarm.Drawing
{
   

    /// <summary>
    /// for printing a string to target canvas
    /// </summary>
    public interface ITextPrinter
    {
        //
        TextBaseline TextBaseline { get; set; }
        void DrawString(char[] text, int startAt, int len, double left, double top);
        /// <summary>
        /// render from RenderVxFormattedString object to specific pos
        /// </summary>
        /// <param name="renderVx"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        void DrawString(RenderVxFormattedString renderVx, double left, double top);
        //-------------
        void PrepareStringForRenderVx(RenderVxFormattedString renderVx, char[] text, int startAt, int len);
        void ChangeFont(RequestFont font);
        //-------------
        void ChangeFillColor(Color fillColor);
        void ChangeStrokeColor(Color strokColor);
       
        //-------------
        void MeasureString(char[] buffer, int startAt, int len, out int w, out int h);
    }


    public static class ITextPrinterExtensions
    {
        public static void DrawString(this ITextPrinter textPrinter, string text, double left, double top)
        {
#if DEBUG
            if (text == null)
            {
                return;
            }
#endif
            //TODO: review here!!!
            //Do Not alloc new char[]
            //plan: use Span<T>  or some ptr to string           

            char[] textBuffer = text.ToCharArray();
            textPrinter.DrawString(textBuffer, 0, textBuffer.Length, left, top);
        }
    }

}