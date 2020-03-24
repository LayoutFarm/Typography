//MIT, 2014-present, WinterDev
using System.Collections.Generic;

namespace Typography.TextLayout
{

    public class TextRunFontStyle
    {
        public TextRunFontStyle(string name, float sizeInPoints)
        {
            Name = name;
            SizeInPoints = sizeInPoints;
        }

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
        GlyphPlanSequence _glyphPlanSeq; //1 text run 1 glyph plan sequence

        float _runWidth;
        public TextRun(TextBuffer srcTextBuffer, int startAt, int len, WordSpanKind kind, TextRunFontStyle fontStyle)
        {
            _srcText = srcTextBuffer;
            _startAt = startAt;
            _len = len;
            _kind = kind;
            FontStyle = fontStyle;

        }
        internal void SetGlyphPlanSeq(GlyphPlanSequence seq)
        {
            _glyphPlanSeq = seq;
            _runWidth = seq.CalculateWidth();
        }
        //
        public GlyphPlanSequence GetGlyphPlanSeq() => _glyphPlanSeq;
        //
        public float Width => _runWidth;
        public TextRunFontStyle FontStyle { get; set; }
        internal bool IsMeasured { get; set; }
        internal TextBuffer TextBuffer => _srcText;
        //
        internal int StartAt => _startAt;
        internal int Len => _len;
#if DEBUG
        public override string ToString()
        {
            switch (_kind)
            {
                case WordSpanKind.Text:
                    return _srcText.CopyString(_startAt, _len);
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