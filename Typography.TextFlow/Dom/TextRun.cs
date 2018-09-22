//MIT, 2014-present, WinterDev
using System.Collections.Generic;

namespace Typography.TextLayout
{

    public class TextRunFontStyle
    {
        //font spec
        //resolve later        
        public string Name { get; set; }
        public float SizeInPoints { get; set; }
    }
    public enum WordSpanKind : ushort
    {
        Unknown,
        Custom,
        WhiteSpace,
        NewLine,
        Text,
        Tab,
    }

    public class TextRun : IRun
    {
        WordSpanKind _kind;
        GlyphPlanSequence glyphPlanSeq; //1 text run 1 glyph plan sequence

        public TextRun(System.ReadOnlyMemory<char> srcTextBuffer, WordSpanKind kind)
        {
            this.TextBuffer = srcTextBuffer;
            this._kind = kind;

        }
        internal void SetGlyphPlanSeq(GlyphPlanSequence seq)
        {
            this.glyphPlanSeq = seq;
            this.Width = seq.CalculateWidth();
        }
        public GlyphPlanSequence GetGlyphPlanSeq()
        {
            return this.glyphPlanSeq;
        }

        public float Width { get; private set; }
        public TextRunFontStyle FontStyle { get; set; }
        internal bool IsMeasured { get; set; }
        internal System.ReadOnlyMemory<char> TextBuffer { get; private set; }
#if DEBUG
        public override string ToString()
        {
            switch (_kind)
            {
                case WordSpanKind.Text:
                    return TextBuffer.ToString();
                default:
                case WordSpanKind.WhiteSpace:
                case WordSpanKind.Tab:
                case WordSpanKind.NewLine:
                    return _kind + " ^^ len" + TextBuffer.Length;

            }
        }
#endif

    }

   

}