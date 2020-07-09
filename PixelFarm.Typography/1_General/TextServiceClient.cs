//MIT, 2020-present, WinterDev
using System;
using System.Collections.Generic;

using Typography.OpenFont;
using Typography.OpenFont.Extensions;

using Typography.FontManagement;
using Typography.TextLayout;
using Typography.TextBreak;

using PixelFarm.Drawing;

namespace Typography.TextServices
{
    public class TextServiceClient : ITextService
    {
        readonly TextServices _txtServices = new TextServices();
        readonly OpenFontTextService _openFontTextService;
        internal TextServiceClient(OpenFontTextService openFontTextService)
        {
            _openFontTextService = openFontTextService;
        }
        //
        public ScriptLang CurrentScriptLang
        {
            get => _txtServices.CurrentScriptLang;
            set => _txtServices.CurrentScriptLang = value;
        }
        public bool EnableGsub
        {
            get => _txtServices.EnableGsub;
            set => _txtServices.EnableGsub = value;
        }
        public bool EnableGpos
        {
            get => _txtServices.EnableGpos;
            set => _txtServices.EnableGpos = value;
        }

        readonly TextPrinterWordVisitor _wordVisitor = new TextPrinterWordVisitor();
        readonly TextPrinterLineSegmentList<TextPrinterLineSegment> _lineSegmentList = new TextPrinterLineSegmentList<TextPrinterLineSegment>();

        public void CalculateUserCharGlyphAdvancePos(in TextBufferSpan textBufferSpan, RequestFont font, ref TextSpanMeasureResult measureResult)
        {

            _lineSegmentList.Clear();
            _wordVisitor.SetLineSegmentList(_lineSegmentList);

            char[] str = textBufferSpan.GetRawCharBuffer(); //TODO: review here again!
            _txtServices.BreakToLineSegments(str, textBufferSpan.start, textBufferSpan.len, _wordVisitor);

            _wordVisitor.SetLineSegmentList(null); //clear

            CalculateUserCharGlyphAdvancePos(textBufferSpan,
                _lineSegmentList,
                font,
                ref measureResult);
        }
        //
        readonly ReusableTextBuffer _reusableTextBuffer = new ReusableTextBuffer();
        //
        public void CalculateUserCharGlyphAdvancePos(in TextBufferSpan textBufferSpan,
            ILineSegmentList lineSegs,
            RequestFont font,
            ref TextSpanMeasureResult measureResult)
        {

            //layout  
            //from font
            //resolve for typeface
            //  
            Typeface typeface = _openFontTextService.ResolveFont(font).Typeface;
            _txtServices.SetCurrentFont(typeface, font.SizeInPoints);

            float scale = typeface.CalculateScaleToPixelFromPointSize(font.SizeInPoints);

            int j = lineSegs.Count;
            int pos = 0; //start at 0

            _reusableTextBuffer.SetRawCharBuffer(textBufferSpan.GetRawCharBuffer());

            short minOffsetY = 0;
            short maxOffsetY = 0;
            int outputTotalW = 0;
            bool hasSomeExtraOffsetY = false;

            for (int i = 0; i < j; ++i)
            {
                //get each segment
                ILineSegment lineSeg = lineSegs[i];

                //each line seg may has different script lang

                //_txtServices.CurrentScriptLang = lineSeg.scriptLang;
                //
                //CACHING ...., reduce number of GSUB/GPOS
                //
                //we cache used line segment for a while
                //we ask for caching context for a specific typeface and font size   
#if DEBUG
                if (lineSeg.Length > _reusableTextBuffer.Len)
                {

                }
#endif
                GlyphPlanSequence seq = _txtServices.GetUnscaledGlyphPlanSequence(_reusableTextBuffer,
                 lineSeg.StartAt,
                 lineSeg.Length);

                int seqLen = seq.Count;

                for (int s = 0; s < seqLen; ++s)
                {
                    UnscaledGlyphPlan glyphPlan = seq[s];
                    if (glyphPlan.OffsetY != 0)
                    {
                        hasSomeExtraOffsetY = true;
                        if (minOffsetY > glyphPlan.OffsetY)
                        {
                            minOffsetY = glyphPlan.OffsetY;
                        }
                        if (maxOffsetY < glyphPlan.OffsetY)
                        {
                            maxOffsetY = glyphPlan.OffsetY;
                        }

                    }

                    outputTotalW +=
                          measureResult.outputXAdvances[pos + glyphPlan.input_cp_offset] += (int)Math.Round(glyphPlan.AdvanceX * scale);
                }
                pos += lineSeg.Length;
            }


            measureResult.outputTotalW = outputTotalW;
            measureResult.lineHeight = (ushort)Math.Round(typeface.CalculateMaxLineClipHeight() * scale);

            if (hasSomeExtraOffsetY)
            {
                measureResult.minOffsetY = (short)Math.Round(minOffsetY * scale);
                measureResult.maxOffsetY = (short)Math.Round(maxOffsetY * scale);
                if (measureResult.maxOffsetY != 0 || measureResult.minOffsetY != 0)
                {
                    measureResult.hasSomeExtraOffsetY = true;
                }
            }
            _reusableTextBuffer.SetRawCharBuffer(null);
        }

        public float CalculateScaleToPixelsFromPoint(RequestFont font) => (_openFontTextService.ResolveFont(font) is ResolvedFont resolvedFont) ? resolvedFont.GetScaleToPixelFromPointInSize() : 0;

        public float MeasureWhitespace(RequestFont f)
        {
            ResolvedFont resolvedFont = _openFontTextService.ResolveFont(f);
            if (resolvedFont != null)
            {
                return resolvedFont.WhitespaceWidthF;
            }
            return 0;
        }
        public GlyphPlanSequence CreateGlyphPlanSeq(in TextBufferSpan textBufferSpan, Typeface typeface, float sizeInPts)
        {
            _txtServices.SetCurrentFont(typeface, sizeInPts);

            _reusableTextBuffer.SetRawCharBuffer(textBufferSpan.GetRawCharBuffer());

            return _txtServices.GetUnscaledGlyphPlanSequence(_reusableTextBuffer, textBufferSpan.start, textBufferSpan.len);
        }

        public GlyphPlanSequence CreateGlyphPlanSeq(in TextBufferSpan textBufferSpan, RequestFont font)
        {
            return CreateGlyphPlanSeq(textBufferSpan, _openFontTextService.ResolveFont(font).Typeface, font.SizeInPoints);
        }
        public GlyphPlanSequence CreateGlyphPlanSeq(in TextBufferSpan textBufferSpan, ResolvedFont font)
        {
            return CreateGlyphPlanSeq(textBufferSpan, font.Typeface, font.SizeInPoints);
        }
        public Size MeasureString(in TextBufferSpan textBufferSpan, RequestFont font)
        {
            //TODO: review here
            Typeface typeface = _openFontTextService.ResolveFont(font).Typeface;
            _txtServices.SetCurrentFont(typeface, font.SizeInPoints);
            _txtServices.MeasureString(textBufferSpan.GetRawCharBuffer(), textBufferSpan.start, textBufferSpan.len, out int w, out int h);
            return new Size(w, h);
        }
        public Size MeasureString(in TextBufferSpan textBufferSpan, ResolvedFont font)
        {
            //TODO: review here
            Typeface typeface = ((ResolvedFont)font).Typeface;
            _txtServices.SetCurrentFont(typeface, font.SizeInPoints);
            _txtServices.MeasureString(textBufferSpan.GetRawCharBuffer(), textBufferSpan.start, textBufferSpan.len, out int w, out int h);
            return new Size(w, h);
        }
        public void MeasureString(in TextBufferSpan textBufferSpan, RequestFont font, int limitWidth, out int charFit, out int charFitWidth)
        {
            Typeface typeface = _openFontTextService.ResolveFont(font).Typeface;
            _txtServices.SetCurrentFont(typeface, font.SizeInPoints);
            _txtServices.MeasureString(textBufferSpan.GetRawCharBuffer(), textBufferSpan.start, textBufferSpan.len, limitWidth, out charFit, out charFitWidth);
        }
        float MeasureBlankLineHeight(RequestFont font)
        {
            ResolvedFont resolvedFont = _openFontTextService.ResolveFont(font);
            return resolvedFont.LineSpacingInPixels;
        }
        float ITextService.MeasureBlankLineHeight(RequestFont font)
        {
            ResolvedFont resolvedFont = ResolveFont(font);
            return resolvedFont.LineSpacingInPixels;
        }
        public bool SupportsWordBreak => true;

        public void BreakToLineSegments(in TextBufferSpan textBufferSpan, WordVisitor wordVisitor)
        {
            //a text buffer span is separated into multiple line segment list  
            _txtServices.BreakToLineSegments(
                textBufferSpan.GetRawCharBuffer(),
                textBufferSpan.start,
                textBufferSpan.len,
                wordVisitor);
        }
        public ResolvedFont ResolveFont(RequestFont reqFont) => _openFontTextService.ResolveFont(reqFont);
        public bool TryGetAlternativeTypefaceFromCodepoint(int codepoint, AlternativeTypefaceSelector selector, out Typeface found) => _openFontTextService.TryGetAlternativeTypefaceFromCodepoint(codepoint, selector, out found);
    }
}