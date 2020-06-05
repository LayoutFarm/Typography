//Apache2, 2014-present, WinterDev
using System;
using System.Collections.Generic;

using Typography.OpenFont;
using Typography.OpenFont.Extensions;

using Typography.TextLayout;
using Typography.TextServices;
using Typography.FontManagement;
using Typography.TextBreak;

namespace PixelFarm.Drawing
{

    public interface ITextService
    {

        float MeasureWhitespace(RequestFont f);
        float MeasureBlankLineHeight(RequestFont f);
        //
        bool SupportsWordBreak { get; }
        //
        Size MeasureString(in TextBufferSpan textBufferSpan, RequestFont font);

        void MeasureString(in TextBufferSpan textBufferSpan, RequestFont font, int maxWidth, out int charFit, out int charFitWidth);

        void CalculateUserCharGlyphAdvancePos(in TextBufferSpan textBufferSpan,
                RequestFont font,
                ref TextSpanMeasureResult result);



        ILineSegmentList BreakToLineSegments(in TextBufferSpan textBufferSpan);
        void CalculateUserCharGlyphAdvancePos(in TextBufferSpan textBufferSpan, ILineSegmentList lineSegs,
               RequestFont font,
              ref TextSpanMeasureResult result);
    }

    public class OpenFontTextService : ITextService
    {
        /// <summary>
        /// instance of Typography lib's text service
        /// </summary>
        TextServices _txtServices;
        Dictionary<int, Typeface> _resolvedTypefaceCache = new Dictionary<int, Typeface>(); //similar to TypefaceStore
        readonly int _system_id;
        //
        public static ScriptLang DefaultScriptLang { get; set; }

        public OpenFontTextService()
        {
            _system_id = PixelFarm.Drawing.Internal.RequestFontCacheAccess.GetNewCacheSystemId();

            //set up typography text service
            _txtServices = new TextServices();
            //default, user can set this later
            _txtServices.InstalledFontCollection = InstalledTypefaceCollection.GetSharedTypefaceCollection(collection =>
            {
                collection.SetFontNameDuplicatedHandler((f0, f1) => FontNameDuplicatedDecision.Skip);

            });


            //create typography service
            //you can implement this service on your own
            //just see the code inside the service 
            //script lang has a potentail effect on how the layout engine instance work.
            //
            //so try to set default script lang to the layout engine instance
            //from current system default value...
            //user can set this to other choices...
            //eg. directly specific the script lang  


            //set script-lang 
            ScriptLang scLang = DefaultScriptLang;
            //---------------
            //if not default then try guess
            //
            if (scLang == null &&
                !TryGetScriptLangFromCurrentThreadCultureInfo(out scLang))
            {
                //TODO: handle error here

                throw new NotSupportedException();
            }


            _txtServices.SetDefaultScriptLang(scLang);
            _txtServices.CurrentScriptLang = scLang;
        }

        public void LoadSystemFonts() => _txtServices.InstalledFontCollection.LoadSystemFonts();

        public void LoadFontsFromFolder(string folder) => _txtServices.InstalledFontCollection.LoadFontsFromFolder(folder);

        public void UpdateUnicodeRanges() => _txtServices.InstalledFontCollection.UpdateUnicodeRanges();

        static bool TryGetScriptLangFromCurrentThreadCultureInfo(out Typography.OpenFont.ScriptLang scLang)
        {
            var currentCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
            scLang = null;
            if (Typography.TextBreak.IcuData.TryGetFullLanguageNameFromLangCode(
                 currentCulture.TwoLetterISOLanguageName,
                 currentCulture.ThreeLetterISOLanguageName,
                 out string langFullName))
            {
                Typography.OpenFont.ScriptLangInfo scLang1 = Typography.OpenFont.ScriptLangs.GetRegisteredScriptLangFromLanguageName(langFullName);
                if (scLang1 == null)
                {
                    //not found -> use default latin
                    //use default lang
#if DEBUG
                    System.Diagnostics.Debug.WriteLine(langFullName + " :use latin");
#endif
                    scLang = ScriptLangs.Latin.GetScriptLang();
                    return true;
                }
                else
                {
                    scLang = scLang1.GetScriptLang();
                    return true;
                }
            }
            return false;
        }


        public bool TryGetAlternativeTypefaceFromChar(char c, out Typeface found)
        {
            //find a typeface that supported input char c
            if (_txtServices.InstalledFontCollection.TryGetAlternativeTypefaceFromChar(c, out List<InstalledTypeface> installedTypefaceList))
            {
                InstalledTypeface selected = installedTypefaceList[0];
                found = _txtServices.GetTypeface(selected.FontName, TypefaceStyle.Regular);
                return true;
            }
            found = null;
            return false;
        }
        //
        public ScriptLang CurrentScriptLang
        {
            get => _txtServices.CurrentScriptLang;
            set => _txtServices.CurrentScriptLang = value;
        }
        //
        public void CalculateUserCharGlyphAdvancePos(in TextBufferSpan textBufferSpan, RequestFont font, ref TextSpanMeasureResult measureResult)
        {
            CalculateUserCharGlyphAdvancePos(textBufferSpan,
                this.BreakToLineSegments(textBufferSpan),
                font,
                ref measureResult);
        }
        //
        ReusableTextBuffer _reusableTextBuffer = new ReusableTextBuffer();
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
            Typeface typeface = ResolveTypeface(font);
            _txtServices.SetCurrentFont(typeface, font.SizeInPoints);
            MyLineSegmentList mylineSegs = (MyLineSegmentList)lineSegs;
            float scale = typeface.CalculateScaleToPixelFromPointSize(font.SizeInPoints);

            int j = mylineSegs.Count;
            int pos = 0; //start at 0

            _reusableTextBuffer.SetRawCharBuffer(textBufferSpan.GetRawCharBuffer());

            short minOffsetY = 0;
            short maxOffsetY = 0;
            int outputTotalW = 0;
            bool hasSomeExtraOffsetY = false;

            for (int i = 0; i < j; ++i)
            {
                //get each segment
                MyLineSegment lineSeg = (MyLineSegment)mylineSegs.GetSegment(i);

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

        public float CalculateScaleToPixelsFromPoint(RequestFont font)
        {
            return ResolveTypeface(font).CalculateScaleToPixelFromPointSize(font.SizeInPoints);
        }
        public Typeface ResolveTypeface(RequestFont font)
        {
            //from user's request font
            //resolve to actual Typeface
            //get data from... 
            //cache level-1 (attached inside the request font)
            Typeface typeface = PixelFarm.Drawing.Internal.RequestFontCacheAccess.GetActualFont<Typeface>(font, _system_id);
            if (typeface != null) return typeface;
            //
            //cache level-2 (stored in this Ifonts)
            if (!_resolvedTypefaceCache.TryGetValue(font.FontKey, out typeface))
            {
                //not found ask the typeface store to load that font
                //....
                typeface = _txtServices.GetTypeface(font.Name, PixelFarm.Drawing.FontStyleExtensions.ConvToInstalledFontStyle(font.Style));
                if (typeface == null)
                {
                    throw new NotSupportedException(font.Name);
                }
                //
                //cache here (level-1)
                _resolvedTypefaceCache.Add(font.FontKey, typeface);
            }
            //and cache into level-0

            float pxSize = Typeface.ConvPointsToPixels(font.SizeInPoints);
            float pxscale = typeface.CalculateScaleToPixelFromPointSize(font.SizeInPoints);

            float recommedLineSpacingInPx = typeface.CalculateRecommendLineSpacing() * pxscale;
            float descentInPx = typeface.Descender * pxscale;
            float ascentInPx = typeface.Ascender * pxscale;
            float lineGapInPx = typeface.LineGap * pxscale;

            PixelFarm.Drawing.Internal.RequestFontCacheAccess.SetActualFont(font, _system_id, typeface);
            PixelFarm.Drawing.Internal.RequestFontCacheAccess.SetGeneralFontMetricInfo(font,
                pxSize,
                ascentInPx,
                descentInPx,
                lineGapInPx,
                recommedLineSpacingInPx);

            var span = new TextBufferSpan(new char[] { ' ' });
            Size whiteSpaceW = MeasureString(span, font);
            PixelFarm.Drawing.Internal.RequestFontCacheAccess.SetWhitespaceWidth(font, _system_id, whiteSpaceW.Width);
            return typeface;
        }
        public float MeasureWhitespace(RequestFont f)
        {
            ResolveTypeface(f);
            return PixelFarm.Drawing.Internal.RequestFontCacheAccess.GetWhitespaceWidth(f, _system_id);
        }



        public GlyphPlanSequence CreateGlyphPlanSeq(in TextBufferSpan textBufferSpan, Typeface typeface, float sizeInPts)
        {
            _txtServices.SetCurrentFont(typeface, sizeInPts);

            _reusableTextBuffer.SetRawCharBuffer(textBufferSpan.GetRawCharBuffer());

            return _txtServices.GetUnscaledGlyphPlanSequence(_reusableTextBuffer, textBufferSpan.start, textBufferSpan.len);
        }
        public GlyphPlanSequence CreateGlyphPlanSeq(in TextBufferSpan textBufferSpan, RequestFont font)
        {
            return CreateGlyphPlanSeq(textBufferSpan, ResolveTypeface(font), font.SizeInPoints);
        }
        public Size MeasureString(in TextBufferSpan textBufferSpan, RequestFont font)
        {
            Typeface typeface = ResolveTypeface(font);
            _txtServices.SetCurrentFont(typeface, font.SizeInPoints);
            _txtServices.MeasureString(textBufferSpan.GetRawCharBuffer(), textBufferSpan.start, textBufferSpan.len, out int w, out int h);
            return new Size(w, h);
        }
        public void MeasureString(in TextBufferSpan textBufferSpan, RequestFont font, int limitWidth, out int charFit, out int charFitWidth)
        {
            Typeface typeface = ResolveTypeface(font);
            _txtServices.SetCurrentFont(typeface, font.SizeInPoints);

            charFit = 0;
            _txtServices.MeasureString(textBufferSpan.GetRawCharBuffer(), textBufferSpan.start, textBufferSpan.len, limitWidth, out charFit, out charFitWidth);

        }
        float ITextService.MeasureBlankLineHeight(RequestFont font)
        {
            Typeface typeface = ResolveTypeface(font);

            return (int)(Math.Round(typeface.CalculateMaxLineClipHeight() *
                                    typeface.CalculateScaleToPixelFromPointSize(font.SizeInPoints)));
        }
        //
        public bool SupportsWordBreak => true;
        //
        struct MyLineSegment : ILineSegment
        {
            readonly int _startAt;
            readonly int _len;
            public readonly SpanLayoutInfo spanLayoutInfo;
            public MyLineSegment(int startAt, int len, SpanLayoutInfo spanLayoutInfo)
            {
                _startAt = startAt;
                _len = len;

#if DEBUG
                if (spanLayoutInfo == null)
                {

                }
#endif
                this.spanLayoutInfo = spanLayoutInfo;


            }
            public int Length => _len;
            public int StartAt => _startAt;
            public SpanLayoutInfo SpanLayoutInfo => spanLayoutInfo;

#if DEBUG
            public override string ToString()
            {
                return _startAt + ":" + _len + (spanLayoutInfo.RightToLeft ? "(rtl)" : "");
            }
#endif
        }

        class MyLineSegmentList : ILineSegmentList
        {
            List<ILineSegment> _segments = new List<ILineSegment>();
            private MyLineSegmentList()
            {
            }
            public void AddLineSegment(ILineSegment lineSegment)
            {
                _segments.Add(lineSegment);
            }
            public void Clear()
            {
                _segments.Clear();
            }
            //
            public ILineSegment this[int index] => _segments[index];
            //
            public int Count => _segments.Count;
            //
            public ILineSegment GetSegment(int index) => _segments[index];
            //
#if DEBUG
            public int dbugStartAt;
            public int dbugLen;
#endif

            void IDisposable.Dispose()
            {
                if (s_lineSegmentPool.Count > 100)
                {
                    _segments.Clear();
                    _segments = null;
                }
                else
                {
                    _segments.Clear();
                    s_lineSegmentPool.Push(this);
                }
            }

            [ThreadStatic]
            static Stack<MyLineSegmentList> s_lineSegmentPool;
            public static MyLineSegmentList GetFreeLineSegmentList()
            {
                if (s_lineSegmentPool == null) s_lineSegmentPool = new Stack<MyLineSegmentList>();
                if (s_lineSegmentPool.Count == 0)
                {
                    return new MyLineSegmentList();
                }
                else
                {
                    return s_lineSegmentPool.Pop();
                }

            }
        }

        public ILineSegmentList BreakToLineSegments(in TextBufferSpan textBufferSpan)
        {
            //a text buffer span is separated into multiple line segment list 
            char[] str = textBufferSpan.GetRawCharBuffer();
#if DEBUG
            int cur_startAt = textBufferSpan.start;
#endif

            MyLineSegmentList lineSegments = MyLineSegmentList.GetFreeLineSegmentList();
            foreach (BreakSpan breakSpan in _txtServices.BreakToLineSegments(str, textBufferSpan.start, textBufferSpan.len))
            {
                SpanLayoutInfo spLayoutInfo = breakSpan.spanLayoutInfo;

                if (!(spLayoutInfo.ResolvedScriptLang is Typography.OpenFont.ScriptLang scLang))
                {
                    if (!Typography.OpenFont.ScriptLangs.TryGetScriptLang((char)spLayoutInfo.SampleCodePoint, out var scLang1))
                    {

                    }
                    else
                    {
                        spLayoutInfo.ResolvedScriptLang = scLang1.GetScriptLang();
                    }
                }

                lineSegments.AddLineSegment(new MyLineSegment(breakSpan.startAt, breakSpan.len, spLayoutInfo));
            }
            return lineSegments;
        }
        //-----------------------------------
        static OpenFontTextService()
        {
            CurrentEnv.CurrentOSName = (IsOnMac()) ?
                         CurrentOSName.Mac :
                         CurrentOSName.Windows;
        }

        static bool s_evaluatedOS;
        static bool s_onMac;
        static bool IsOnMac()
        {

            if (s_evaluatedOS) return s_onMac;
            // 
            s_evaluatedOS = true;
#if NETCORE
                return _s_onMac=  System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                  System.Runtime.InteropServices.OSPlatform.OSX);                    
#else

            return s_onMac = (System.Environment.OSVersion.Platform == System.PlatformID.MacOSX);
#endif
        }


    }


}
