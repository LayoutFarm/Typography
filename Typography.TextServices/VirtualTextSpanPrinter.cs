//MIT, 2016-present, WinterDev, Sam Hocevar
using System;
using System.Collections.Generic;

using Typography.OpenFont;
using Typography.OpenFont.Extensions;
using Typography.TextLayout;
using Typography.TextBreak;
using Typography.FontManagement;

namespace Typography.Text
{

    public class VirtualTextSpanPrinter : AbstractTextSpanPrinter
    {

        public delegate bool SelectTypefaceDelegate(int codepoint, AltTypefaceSelectorBase userSelector, out Typeface found);


        readonly GlyphPlanCache _glyphPlanCache = new GlyphPlanCache();
        readonly GlyphLayout _glyphLayout = new GlyphLayout();//for glyph layout

        Typeface _currentTypeface;
        CustomBreaker _textBreaker; //text break        

        public VirtualTextSpanPrinter()
        {            

            this.PositionTechnique = PositionTechnique.OpenFont;
            FontSizeInPoints = 14;// 
        }
        public AltTypefaceSelectorBase AlternativeTypefaceSelector { get; set; }
        public SelectTypefaceDelegate BuiltInAlternativeTypefaceSelector { get; set; }

        public override ScriptLang ScriptLang
        {
            get => base.ScriptLang;
            set
            {
                _glyphLayout.ScriptLang = value;
                base.ScriptLang = value;
            }
        }

        public void EnableGsubGpos(bool value)
        {
            _glyphLayout.EnableGpos = value;
            _glyphLayout.EnableGsub = value;
        }


        protected override void OnFontSizeChanged()
        {
            //update some font metrics property   
            if (_disableBaselineChange)
            {
                //eg. enable multi-typeface mode
                return;
            }

            _glyphLayout.Typeface = _currentTypeface;
            base.OnFontSizeChanged();
        }

        bool _disableBaselineChange;

        public override Typeface Typeface
        {
            get => _currentTypeface;

            set
            {
                if (_currentTypeface == value) return;
                // 
                _currentTypeface = value;
                if (value == null)
                {
                    return;
                }

                OnFontSizeChanged();
            }
        }

        public virtual void UpdateGlyphLayoutSettings()
        {
            if (Typeface == null) return;
            _glyphLayout.Typeface = this.Typeface;
            _glyphLayout.ScriptLang = this.ScriptLang;
            _glyphLayout.PositionTechnique = this.PositionTechnique;
            _glyphLayout.EnableLigature = this.EnableLigature;
        }

        float _orgX;
        float _orgY;

        int _latestAccumulateWidth;
        int _latestCharIndex; //if we use limit width
#if DEBUG
        int dbugExportCount = 0;
#endif

        public override void DrawFromGlyphPlans(GlyphPlanSequence seq, int startAt, int len, float left, float top)
        {
            _latestAccumulateWidth = 0;//reset

            if (_currentTypeface == null) return;

            float baseLine = top;
            switch (TextBaseline)
            {
                case Typography.Text.TextBaseline.Alphabetic:

                    break;
                case Typography.Text.TextBaseline.Top:
                    baseLine += this.FontAscendingPx;
                    break;
                case Typography.Text.TextBaseline.Bottom:
                    baseLine += this.FontDescedingPx;
                    break;
            }

            float fontSizePoint = this.FontSizeInPoints;
            float scale = _currentTypeface.CalculateScaleToPixelFromPointSize(fontSizePoint);

            //4. render each glyph 
            float ox = _orgX;
            float oy = _orgY;

            //--------------------------------------------------- 
            //Test svg font with Twitter Color Emoji Regular 
            int seqLen = seq.Count;
            if (len > seqLen)
            {
                len = seqLen;
            }

            if (_limitWidth > 0)
            {
                var snapToPx = new GlyphPlanSequenceSnapPixelScaleLayout(seq, startAt, len, scale);
                snapToPx.ReadWidthLimitWidth(_limitWidth);

                _latestCharIndex = snapToPx.CurrentIndex;
                _latestAccumulateWidth = snapToPx.AccumWidth;
            }
            else
            {
                var snapToPx = new GlyphPlanSequenceSnapPixelScaleLayout(seq, startAt, len, scale);
                snapToPx.ReadToEnd();
                _latestAccumulateWidth = snapToPx.AccumWidth;
            }
        }

        public bool NeedRightToLeftArr { get; private set; }


        Dictionary<UnicodeRangeInfo, Typeface> _prevResolvedTypefaces = new Dictionary<UnicodeRangeInfo, Typeface>();

        public void PrepapreFormattedStringList(char[] textBuffer, int startAt, int len, FormattedGlyphPlanList fmtGlyphs)
        {
            _prevResolvedTypefaces.Clear();
            _lineSegs.Clear();//clear before reuse
            _textPrinterWordVisitor.SetLineSegmentList(_lineSegs);
            BreakToLineSegments(textBuffer, startAt, len, _textPrinterWordVisitor);//***
            _textPrinterWordVisitor.SetLineSegmentList(null);

            NeedRightToLeftArr = false;

            Typeface defaultTypeface = _currentTypeface;
            Typeface curTypeface = defaultTypeface;

            ResolvedFont resolvedFont = LocalResolveFont(curTypeface, FontSizeInPoints);
            FormattedGlyphPlanSeq latestFmtGlyphPlanSeq = null;
            int prefix_whitespaceCount = 0;

            int count = _lineSegs.Count;
            for (int i = 0; i < count; ++i)
            {
                //
                TextPrinterLineSegment line_seg = _lineSegs.GetLineSegment(i);
                SpanBreakInfo spBreakInfo = line_seg.BreakInfo;

#if DEBUG
                if (spBreakInfo == null)
                {

                }
#endif
                if (spBreakInfo.RightToLeft)
                {
                    NeedRightToLeftArr = true;
                }


                if (line_seg.WordKind == WordKind.Whitespace)
                {
                    if (latestFmtGlyphPlanSeq == null)
                    {
                        prefix_whitespaceCount += line_seg.Length;
                    }
                    else
                    {
                        latestFmtGlyphPlanSeq.PostfixWhitespaceCount += line_seg.Length;
                    }
                    continue; //***
                }


                //each line segment may have different unicode range 
                //and the current typeface may not support that range
                //so we need to ensure that we get a proper typeface,
                //if not => alternative typeface

                ushort sample_glyphIndex = 0;
                char sample_char = textBuffer[line_seg.StartAt];
                int codepoint = sample_char;

                if (line_seg.Length > 1 && line_seg.WordKind == WordKind.SurrogatePair)
                {
                    sample_glyphIndex = curTypeface.GetGlyphIndex(codepoint = char.ConvertToUtf32(sample_char, textBuffer[line_seg.StartAt + 1]));
                }
                else
                {
                    sample_glyphIndex = curTypeface.GetGlyphIndex(codepoint);
                }


                if (sample_glyphIndex == 0)
                {
                    //not found then => find other typeface                    
                    //we need more information about line seg layout
                    bool foundFromPrev = false;
                    if (_prevResolvedTypefaces.Count > 0 && spBreakInfo != null &&
                        spBreakInfo.UnicodeRange != null &&
                        _prevResolvedTypefaces.TryGetValue(spBreakInfo.UnicodeRange, out Typeface prevTypeface))
                    {

                        //found
                        //use this...
                        if (line_seg.WordKind == WordKind.SurrogatePair)
                        {
                            //find unicode range of this surrogate pair
                            if (UnicodeRangeFinder.GetUniCodeRangeFor(codepoint, out UnicodeRangeInfo u1, out SpanBreakInfo brk1) &&
                                u1 == spBreakInfo.UnicodeRange)
                            {
                                resolvedFont = LocalResolveFont(prevTypeface, FontSizeInPoints);
                                curTypeface = prevTypeface;
                                foundFromPrev = true;
                            }
                        }
                        else
                        {
                            resolvedFont = LocalResolveFont(prevTypeface, FontSizeInPoints);
                            curTypeface = prevTypeface;
                            foundFromPrev = true;
                        }
                    }

                    if (!foundFromPrev)
                    {
                        if (AlternativeTypefaceSelector != null)
                        {
                            AlternativeTypefaceSelector.LatestTypeface = curTypeface;
                        }

                        if (BuiltInAlternativeTypefaceSelector != null &&
                            BuiltInAlternativeTypefaceSelector.Invoke(codepoint, AlternativeTypefaceSelector, out Typeface alternative))
                        {
                            resolvedFont = LocalResolveFont(alternative, FontSizeInPoints);
                            curTypeface = alternative;
                            foundFromPrev = true;
                        }
                    }
                }


                //layout glyphs in each context

                var buff = new Typography.Text.TextBufferSpan(textBuffer, line_seg.StartAt, line_seg.Length);

                _glyphLayout.ScriptLang = new ScriptLang(spBreakInfo.ScriptTag, spBreakInfo.LangTag);

                //in some text context (+typeface)=>user can disable gsub, gpos
                //this is an example                  

                if (line_seg.WordKind == WordKind.Tab || line_seg.WordKind == WordKind.Number ||
                    (spBreakInfo.UnicodeRange == Unicode13RangeInfoList.C0_Controls_and_Basic_Latin && line_seg.WordKind != WordKind.SurrogatePair))
                {
                    _glyphLayout.EnableGpos = false;
                    _glyphLayout.EnableGsub = false;
                }
                else
                {
                    _glyphLayout.EnableGpos = true;
                    _glyphLayout.EnableGsub = true;
                }


                GlyphPlanSequence seq = CreateGlyphPlanSeq(buff, curTypeface);
                seq.IsRightToLeft = spBreakInfo.RightToLeft;

                if (spBreakInfo.UnicodeRange != null &&
                    !_prevResolvedTypefaces.ContainsKey(spBreakInfo.UnicodeRange))
                {
                    //add 
                    _prevResolvedTypefaces.Add(spBreakInfo.UnicodeRange, curTypeface);
                }

                //create an object that hold more information about GlyphPlanSequence

                FormattedGlyphPlanSeq formattedGlyphPlanSeq = fmtGlyphs.AppendNew();

                formattedGlyphPlanSeq.PrefixWhitespaceCount = (ushort)prefix_whitespaceCount;//***
                prefix_whitespaceCount = 0;//reset 

                //TODO: other style?... (bold, italic)  

                ResolvedFont foundResolvedFont = LocalResolveFont(curTypeface, FontSizeInPoints);
                formattedGlyphPlanSeq.SetData(seq, foundResolvedFont);

                latestFmtGlyphPlanSeq = formattedGlyphPlanSeq;

                curTypeface = defaultTypeface;//switch back to default

                //restore latest script lang?
            }

        }
        public override void DrawString(char[] textBuffer, int startAt, int len, float x, float y)
        {
#if DEBUG
            if (textBuffer.Length > 2)
            {

            }
#endif 

            UpdateGlyphLayoutSettings();
            _latestAccumulateWidth = 0;
            //unscale layout, with design unit scale
            var buffSpan = new Typography.Text.TextBufferSpan(textBuffer, startAt, len);

            float xpos = x;
            float ypos = y;

            if (!EnableMultiTypefaces)
            {
                GlyphPlanSequence glyphPlanSeq = CreateGlyphPlanSeq(buffSpan, _currentTypeface);
                DrawFromGlyphPlans(glyphPlanSeq, xpos, y);
            }
            else
            {
                Typeface defaultTypeface = _currentTypeface;//save to restore later
                                                            //a single string may be broken into many glyph-plan-seq
                                                            //set segmentlist

                _fmtGlyphPlanList.Clear();

                PrepapreFormattedStringList(textBuffer, startAt, len, _fmtGlyphPlanList);

                _disableBaselineChange = true;
                if (NeedRightToLeftArr)
                {
                    //special arr left-to-right

                    int count = _fmtGlyphPlanList.Count;//re-count
                    for (int i = count - 1; i >= 0; --i)
                    {
                        FormattedGlyphPlanSeq formattedGlyphPlanSeq = _fmtGlyphPlanList[i];

                        ResolvedFont resolvedFont = formattedGlyphPlanSeq.ResolvedFont;
                        Typeface = resolvedFont.Typeface;

                        DrawFromGlyphPlans(formattedGlyphPlanSeq.Seq, xpos + (resolvedFont.WhitespaceWidth * formattedGlyphPlanSeq.PrefixWhitespaceCount), y);

                        xpos += _latestAccumulateWidth + (resolvedFont.WhitespaceWidth * formattedGlyphPlanSeq.PostfixWhitespaceCount);
                    }

                }
                else
                {
                    int count = _fmtGlyphPlanList.Count;//re-count

                    for (int i = 0; i < count; ++i)
                    {
                        FormattedGlyphPlanSeq formattedGlyphPlanSeq = _fmtGlyphPlanList[i];

                        //change typeface                     
                        ResolvedFont resolvedFont = formattedGlyphPlanSeq.ResolvedFont;
                        Typeface = resolvedFont.Typeface;

                        DrawFromGlyphPlans(formattedGlyphPlanSeq.Seq, xpos + (resolvedFont.WhitespaceWidth * formattedGlyphPlanSeq.PrefixWhitespaceCount), y);

                        xpos += _latestAccumulateWidth + (resolvedFont.WhitespaceWidth * formattedGlyphPlanSeq.PostfixWhitespaceCount);

                    }
                }
                _disableBaselineChange = false;

                _fmtGlyphPlanList.Clear();

                //restore prev typeface & settings
                Typeface = defaultTypeface;
            }
        }


        readonly protected TextPrinterWordVisitor _textPrinterWordVisitor = new TextPrinterWordVisitor();
        readonly protected TextPrinterLineSegmentList<TextPrinterLineSegment> _lineSegs = new TextPrinterLineSegmentList<TextPrinterLineSegment>();

        readonly protected Dictionary<int, ResolvedFont> _localResolvedFonts = new Dictionary<int, ResolvedFont>();
        protected ResolvedFont LocalResolveFont(Typeface typeface, float sizeInPoint)
        {
            //find local resolved font cache

            //check if we have a cache key or not
            int typefaceKey = TypefaceExtensions.GetCustomTypefaceKey(typeface);
            if (typefaceKey == 0)
            {
                throw new System.NotSupportedException();
                ////calculate and cache
                //TypefaceExtensions.SetCustomTypefaceKey(typeface,
                //    typefaceKey = RequestFont.CalculateTypefaceKey(typeface.Name));
            }

            int key = InternalFontKey.CalculateGetHasCode(typefaceKey, sizeInPoint, 0);
            if (!_localResolvedFonts.TryGetValue(key, out ResolvedFont found))
            {
                return _localResolvedFonts[key] = new ResolvedFont(typeface, sizeInPoint, key);
            }
            return found;
        }


        public void BreakToLineSegments(char[] str, int startAt, int len, WordVisitor visitor)
        {
            //user must setup the CustomBreakerBuilder before use      
            if (len < 1)
            {
                return;
            }

#if DEBUG
            if (len > 2)
            {

            }
#endif
            if (_textBreaker == null)
            {
                //setup 
                _textBreaker = Typography.TextBreak.CustomBreakerBuilder.NewCustomBreaker();
            }

            _textBreaker.UseUnicodeRangeBreaker = true;
            _textBreaker.CurrentVisitor = visitor;
            _textBreaker.BreakWords(str, startAt, len);
        }


        GlyphPlanSequence CreateGlyphPlanSeq(in Typography.Text.TextBufferSpan textBufferSpan, Typeface typeface)
        {

            Typeface t = Typeface;

            Typeface = typeface;

            GlyphPlanSequence seq = CreateGlyphPlanSeq(textBufferSpan);
            //restore typeface and font size

            Typeface = t;

            return seq;
        }
        public GlyphPlanSequence CreateGlyphPlanSeq(
          in Typography.Text.TextBufferSpan buffer,
          IUnscaledGlyphPlanList output)
        {
            //UNSCALED VERSION and NO CACHE here
            //use current typeface + scriptlang

            //create a new one if we don't has a cache
            //1. layout 

            _glyphLayout.Layout(
                buffer.GetRawCharBuffer(),
                buffer.start,
                buffer.len);

            int pre_count = output.Count;
            //create glyph-plan ( UnScaled version) and add it to planList                

            _glyphLayout.GenerateUnscaledGlyphPlans(output);

            int post_count = output.Count;
            return new GlyphPlanSequence(output, pre_count, post_count - pre_count);//** 
        }

        public GlyphPlanSequence CreateGlyphPlanSeq(in Typography.Text.TextBufferSpan textBufferSpan)
        {
            //use CACHE,
            _glyphPlanCache.SetCurrentFont(_currentTypeface, FontSizeInPoints, this.ScriptLang);
            Typography.Text.TextBufferSpan buffSpan1 = new Typography.Text.TextBufferSpan(textBufferSpan.GetRawCharBuffer(), textBufferSpan.start, textBufferSpan.len);
            return _glyphPlanCache.GetUnscaledGlyphPlanSequence(buffSpan1, _glyphLayout);
        }
        public GlyphPlanSequence CreateGlyphPlanSeq(in Typography.Text.TextBufferSpan textBufferSpan, ResolvedFont font)
        {

            Typeface t = Typeface;
            float sizeInPoints = FontSizeInPoints;

            GlyphPlanSequence seq = CreateGlyphPlanSeq(textBufferSpan);

            Typeface = t; //restore
            FontSizeInPoints = sizeInPoints;
            return seq;
        }

        readonly FormattedGlyphPlanList _fmtGlyphPlanList = new FormattedGlyphPlanList();
        int _limitWidth;

        public void MeasureString(in Typography.Text.TextBufferSpan textBufferSpan, MeasureStringArgs args)
        {
            //TODO: review here again

            //draw string and measure result
            _latestAccumulateWidth = 0;

            _limitWidth = args.LimitWidth;

            DrawString(textBufferSpan.GetRawCharBuffer(), textBufferSpan.start, textBufferSpan.len, 0, 0);

            args.CharFitWidth = _latestAccumulateWidth;
            args.CharFit = _latestCharIndex;

            _limitWidth = -1;//reset

            float pxscale = _currentTypeface.CalculateScaleToPixelFromPointSize(FontSizeInPoints);
            args.Width = _latestAccumulateWidth;
            args.Height = (int)Math.Round(_currentTypeface.CalculateMaxLineClipHeight() * pxscale);
        }


        static class InternalFontKey
        {
            //NOTE: need a copy from 'Painter_Layer2'

            //only typeface name
            static readonly Dictionary<string, int> s_registerFontNames = new Dictionary<string, int>();

            static InternalFontKey()
            {
                RegisterFontName(""); //blank font name
            }
            public static int RegisterFontName(string fontName)
            {
                fontName = fontName.ToUpper();//***
                if (!s_registerFontNames.TryGetValue(fontName, out int found))
                {
                    int nameCrc32 = TinyCRC32Calculator.CalculateCrc32(fontName);
                    s_registerFontNames.Add(fontName, nameCrc32);
                    return nameCrc32;
                }
                return found;
            }
            public static int CalculateGetHasCode(int typefaceKey, float fontSize, int fontstyle)
            {
                //modified from https://stackoverflow.com/questions/1646807/quick-and-simple-hash-code-combinations
                unchecked
                {
                    int hash = 17;
                    hash = hash * 31 + typefaceKey.GetHashCode();
                    hash = hash * 31 + fontSize.GetHashCode();
                    hash = hash * 31 + fontstyle.GetHashCode();
                    return hash;
                }
            }
        }


        readonly struct TinyCRC32Calculator
        {

            //NOTE: need a copy from 'Painter_Layer2'

            // Copyright (c) 2006-2009 Dino Chiesa and Microsoft Corporation.
            // All rights reserved.
            //
            // This code module is part of DotNetZip, a zipfile class library.
            //
            // ------------------------------------------------------------------
            //
            // This code is licensed under the Microsoft Public License.
            // See the file License.txt for the license details.
            // More info on: http://dotnetzip.codeplex.com
            //
            // ------------------------------------------------------------------
            //
            // last saved (in emacs):
            // Time-stamp: <2010-January-16 13:16:27>
            //
            // ------------------------------------------------------------------
            //
            // Implements the CRC algorithm, which is used in zip files.  The zip format calls for
            // the zipfile to contain a CRC for the unencrypted byte stream of each file.
            //
            // It is based on example source code published at
            //    http://www.vbaccelerator.com/home/net/code/libraries/CRC32/Crc32_zip_CRC32_CRC32_cs.asp
            //
            // This implementation adds a tweak of that code for use within zip creation.  While
            // computing the CRC we also compress the byte stream, in the same read loop. This
            // avoids the need to read through the uncompressed stream twice - once to compute CRC
            // and another time to compress.
            //
            // ------------------------------------------------------------------

            /// <summary>
            /// Update the value for the running CRC32 using the given block of bytes.
            /// This is useful when using the CRC32() class in a Stream.
            /// </summary>
            /// <param name="block">block of bytes to slurp</param>
            /// <param name="offset">starting point in the block</param>
            /// <param name="count">how many bytes within the block to slurp</param>
            static int SlurpBlock(byte[] block, int offset, int count)
            {
                if (block == null)
                {
                    throw new NotSupportedException("The data buffer must not be null.");
                }

                // UInt32 tmpRunningCRC32Result = _RunningCrc32Result;

                uint _runningCrc32Result = 0xFFFFFFFF;
                for (int i = 0; i < count; i++)
                {
#if DEBUG
                    int x = offset + i;
#endif
                    //_runningCrc32Result = ((_runningCrc32Result) >> 8) ^ s_crc32Table[(block[x]) ^ ((_runningCrc32Result) & 0x000000FF)];
                    _runningCrc32Result = ((_runningCrc32Result) >> 8) ^ s_crc32Table[(block[offset + i]) ^ ((_runningCrc32Result) & 0x000000FF)];
                    //tmpRunningCRC32Result = ((tmpRunningCRC32Result) >> 8) ^ crc32Table[(block[offset + i]) ^ ((tmpRunningCRC32Result) & 0x000000FF)];
                }
                return unchecked((Int32)(~_runningCrc32Result));
            }


            // pre-initialize the crc table for speed of lookup.
            static TinyCRC32Calculator()
            {
                unchecked
                {
                    // PKZip specifies CRC32 with a polynomial of 0xEDB88320;
                    // This is also the CRC-32 polynomial used bby Ethernet, FDDI,
                    // bzip2, gzip, and others.
                    // Often the polynomial is shown reversed as 0x04C11DB7.
                    // For more details, see http://en.wikipedia.org/wiki/Cyclic_redundancy_check
                    UInt32 dwPolynomial = 0xEDB88320;


                    s_crc32Table = new UInt32[256];
                    UInt32 dwCrc;
                    for (uint i = 0; i < 256; i++)
                    {
                        dwCrc = i;
                        for (uint j = 8; j > 0; j--)
                        {
                            if ((dwCrc & 1) == 1)
                            {
                                dwCrc = (dwCrc >> 1) ^ dwPolynomial;
                            }
                            else
                            {
                                dwCrc >>= 1;
                            }
                        }
                        s_crc32Table[i] = dwCrc;
                    }
                }
            }


#if DEBUG
            //Int64 dbugTotalBytesRead;
#endif

            static readonly UInt32[] s_crc32Table;
            const int BUFFER_SIZE = 2048;

            [System.ThreadStatic]
            static byte[] s_buffer;

            public static int CalculateCrc32(string inputData)
            {
                if (s_buffer == null)
                {
                    s_buffer = new byte[BUFFER_SIZE];
                }

                if (inputData.Length > 512)
                {
                    byte[] utf8 = System.Text.Encoding.UTF8.GetBytes(inputData);
                    return SlurpBlock(utf8, 0, utf8.Length);
                }
                else
                {
                    int write = System.Text.Encoding.UTF8.GetBytes(inputData, 0, inputData.Length, s_buffer, 0);
                    if (write >= BUFFER_SIZE)
                    {
                        throw new System.NotSupportedException("crc32:");
                    }
                    return SlurpBlock(s_buffer, 0, write);
                }
            }

            public static int CalculateCrc32(byte[] buffer) => SlurpBlock(buffer, 0, buffer.Length);
        }
    }



}
