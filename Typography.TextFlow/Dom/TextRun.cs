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
        TextBuffer _srcText;
        int _startAt;
        int _len;

        WordSpanKind _kind;
        GlyphPlanSequence glyphPlanSeq; //1 text run 1 glyph plan sequence

        float _runWidth;
        public TextRun(TextBuffer srcTextBuffer, int startAt, int len, WordSpanKind kind)
        {
            this._srcText = srcTextBuffer;
            this._startAt = startAt;
            this._len = len;
            this._kind = kind;

        }
        internal void SetGlyphPlanSeq(GlyphPlanSequence seq)
        {
            this.glyphPlanSeq = seq;
            this._runWidth = seq.CalculateWidth();
        }
        public GlyphPlanSequence GetGlyphPlanSeq()
        {
            return this.glyphPlanSeq;
        }

        public float Width
        {
            get { return _runWidth; }
        }

        public TextRunFontStyle FontStyle { get; set; }
        internal bool IsMeasured { get; set; }
        internal TextBuffer TextBuffer
        {
            get { return _srcText; }
        }
        internal int StartAt { get { return _startAt; } }
        internal int Len { get { return _len; } }
#if DEBUG
        public override string ToString()
        {
            switch (_kind)
            {
                case WordSpanKind.Text:
                    return _srcText.CopyString(this._startAt, _len);
                default:
                case WordSpanKind.WhiteSpace:
                case WordSpanKind.Tab:
                case WordSpanKind.NewLine:
                    return _kind + " ^^ len" + _len;

            }
        }
#endif

    }

   

}