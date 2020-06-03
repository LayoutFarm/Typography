//MIT, 2020, WinterDev

namespace PixelFarm.Drawing
{

    public interface ITextService
    {

        float MeasureWhitespace(RequestFont f);
        float MeasureBlankLineHeight(RequestFont f);
        //
        bool SupportsWordBreak { get; }

        //ILineSegmentList BreakToLineSegments(in TextBufferSpan textBufferSpan);
        //
        Size MeasureString(in TextBufferSpan textBufferSpan, RequestFont font);

        void MeasureString(in TextBufferSpan textBufferSpan, RequestFont font, int maxWidth, out int charFit, out int charFitWidth);

        void CalculateUserCharGlyphAdvancePos(in TextBufferSpan textBufferSpan,
                RequestFont font,
                ref TextSpanMeasureResult result);
        //void CalculateUserCharGlyphAdvancePos(in TextBufferSpan textBufferSpan, ILineSegmentList lineSegs,
        //        RequestFont font,
        //        ref TextSpanMeasureResult result);
    }
 

    public struct TextBufferSpan
    {
        public readonly int start;
        public readonly int len;

        char[] _rawString;


        public TextBufferSpan(char[] rawCharBuffer)
        {
            _rawString = rawCharBuffer;
            this.len = rawCharBuffer.Length;
            this.start = 0;
        }
        public TextBufferSpan(char[] rawCharBuffer, int start, int len)
        {
            this.start = start;
            this.len = len;
            _rawString = rawCharBuffer;
        }

        public char GetChar(int localOffset) => _rawString[start + localOffset];
        public char[] GetRawCharBuffer() => _rawString;

#if DEBUG
        public override string ToString()
        {
            return start + ":" + len;
        }
#endif


    }

    public struct TextSpanMeasureResult
    {
        public int[] outputXAdvances;
        public int outputTotalW;
        public ushort lineHeight;

        public bool hasSomeExtraOffsetY;
        public short minOffsetY;
        public short maxOffsetY;
    }


}