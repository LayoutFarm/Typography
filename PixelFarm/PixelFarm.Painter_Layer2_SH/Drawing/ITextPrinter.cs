////MIT, 2014-present, WinterDev 

//namespace PixelFarm.Drawing
//{


//    /// <summary>
//    /// for printing a string to target canvas
//    /// </summary>
//    public interface ITextPrinter
//    {
        
//    }


//    public static class ITextPrinterExtensions
//    {
//        public static void DrawString(this ITextPrinter textPrinter, string text, double left, double top)
//        {
//#if DEBUG
//            if (text == null)
//            {
//                return;
//            }
//#endif
//            //TODO: review here!!!
//            //Do Not alloc new char[]
//            //plan: use Span<T>  or some ptr to string           

//            char[] textBuffer = text.ToCharArray();
//            textPrinter.DrawString(textBuffer, 0, textBuffer.Length, left, top);
//        }
//    }

//}