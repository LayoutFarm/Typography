////MIT, 2020, WinterDev

//namespace PixelFarm.Drawing
//{
//    public readonly struct TextBufferSpan
//    {
//        public readonly int start;
//        public readonly int len;

//        readonly char[] _rawString;

//        public TextBufferSpan(char[] rawCharBuffer)
//        {
//            _rawString = rawCharBuffer;
//            this.len = rawCharBuffer.Length;
//            this.start = 0;
//        }
//        public TextBufferSpan(char[] rawCharBuffer, int start, int len)
//        {
//            this.start = start;
//            this.len = len;
//            _rawString = rawCharBuffer;
//        }

//        public override string ToString()
//        {
//            return start + ":" + len;
//        }


//        public char[] GetRawCharBuffer() => _rawString;
//    }

//    public struct TextSpanMeasureResult
//    {
//        public int[] outputXAdvances;
//        public int outputTotalW;
//        public ushort lineHeight;

//        public bool hasSomeExtraOffsetY;
//        public short minOffsetY;
//        public short maxOffsetY;
//    }


//    public interface ITextService
//    {
//        ResolvedFontBase ResolveFont(RequestFont f);
//        float MeasureWhitespace(RequestFont f);
//        float MeasureBlankLineHeight(RequestFont f);
//        //


//        //
//        Size MeasureString(in TextBufferSpan textBufferSpan, ResolvedFontBase font);
//        Size MeasureString(in TextBufferSpan textBufferSpan, RequestFont font);
//        void MeasureString(in TextBufferSpan textBufferSpan, RequestFont font, int maxWidth, out int charFit, out int charFitWidth);

//    }

//}
 
