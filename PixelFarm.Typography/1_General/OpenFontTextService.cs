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
    public class ResolvedFont : ResolvedFontBase
    {
        public ResolvedFont(Typeface typeface, float sizeInPoints, FontStyle fontStyle)
             : base(typeface?.Name, sizeInPoints, fontStyle)
        {
            Typeface = typeface;
            CalculateOtherValues();
        }
        public ResolvedFont(Typeface typeface, float sizeInPoints, FontStyle fontStyle, int fontKey)
            : base(typeface?.Name, sizeInPoints, fontStyle, fontKey)
        {
            Typeface = typeface;
            CalculateOtherValues();

        }
        void CalculateOtherValues()
        {
            Typeface typeface = this.Typeface;
            if (typeface != null)
            {
                ScaleToPixel = typeface.CalculateScaleToPixelFromPointSize(SizeInPoints);//pxscale
                WhitespaceWidthF = typeface.GetWhitespaceWidth() * ScaleToPixel;
                WhitespaceWidth = (int)Math.Round(WhitespaceWidthF);

                LineSpacingInPixels = (int)(Math.Round(typeface.CalculateRecommendLineSpacing() * ScaleToPixel));
                AscentInPixels = typeface.Ascender * ScaleToPixel;
                DescentInPixels = typeface.Descender * ScaleToPixel;
                LineGapInPx = typeface.LineGap * ScaleToPixel;
            }
        }
        public Typeface Typeface { get; }

        class EmptyResolvedFont : ResolvedFont
        {
            public EmptyResolvedFont() : base(null, 0, FontStyle.Regular, 0) { }
        }

        internal static readonly ResolvedFont s_empty = new EmptyResolvedFont();
    }
    public class OpenFontTextService : ITextService
    {
        /// <summary>
        /// instance of Typography lib's text service
        /// </summary>
        readonly TextServices _txtServices;
        readonly Dictionary<int, ResolvedFont> _resolvedTypefaceCache = new Dictionary<int, ResolvedFont>(); //similar to TypefaceStore

        //
        public static ScriptLang DefaultScriptLang { get; set; }

        public OpenFontTextService()
        {

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
            if (scLang.scriptTag == 0 &&
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


        static readonly ScriptLang s_latin = new ScriptLang(ScriptTagDefs.Latin.Tag);
        static bool TryGetScriptLangFromCurrentThreadCultureInfo(out Typography.OpenFont.ScriptLang scLang)
        {
            var currentCulture = System.Threading.Thread.CurrentThread.CurrentCulture;

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
                    scLang = s_latin;
                    return true;
                }
                else
                {
                    scLang = new ScriptLang(scLang1.shortname);// scLang1.GetScriptLang();
                    return true;
                }
            }
            else
            {
                scLang = default;
            }
            return false;
        }


        public PixelFarm.Drawing.SvgBmpBuilderFunc SvgBmpBuilder { get; set; }

        /// <summary>
        /// get alternative typeface from a given unicode codepoint
        /// </summary>
        /// <param name="codepoint"></param>
        /// <param name="selector"></param>
        /// <param name="found"></param>
        /// <returns></returns>
        public bool TryGetAlternativeTypefaceFromCodepoint(int codepoint, AlternativeTypefaceSelector selector, out Typeface found)
        {
            if (_txtServices.InstalledFontCollection.TryGetAlternativeTypefaceFromCodepoint(codepoint, selector, out InstalledTypeface foundInstalledTypeface))
            {
                found = _txtServices.GetTypeface(foundInstalledTypeface.FontName, TypefaceStyle.Regular);
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
            Typeface typeface = ResolveFont(font).Typeface;
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

        public float CalculateScaleToPixelsFromPoint(RequestFont font) => (ResolveFont(font) is ResolvedFontBase resolvedFont) ? resolvedFont.ScaleToPixel : 0;

        ResolvedFontBase ITextService.ResolveFont(RequestFont f) => this.ResolveFont(f);

        public ResolvedFont ResolveFont(RequestFont font)
        {
            //cache level-1 (attached inside the request font)
            ResolvedFont resolvedFont = PixelFarm.Drawing.Internal.RequestFontCacheAccess.GetResolvedFont1<ResolvedFont>(font);
            if (resolvedFont != null) return resolvedFont;

            //cache level-2 (stored in this openfont service)
            if (_resolvedTypefaceCache.TryGetValue(font.FontKey, out resolvedFont))
            {
                if (resolvedFont.Typeface == null)
                {
                    //this is 'not found' resovled font
                    //so don't return it
                    return null;
                }
                //----
                //cache to level-1
                PixelFarm.Drawing.Internal.RequestFontCacheAccess.SetResolvedFont1(font, resolvedFont);
                return resolvedFont;
            }
            //-----
            //when not found
            //find it
            RequestFont.OtherChoice otherChoice = null;
            Typeface typeface;
            if ((typeface = _txtServices.GetTypeface(font.Name, PixelFarm.Drawing.FontStyleExtensions.ConvToInstalledFontStyle(font.Style))) == null)
            {
                //not found!
                //select other font?
                int otherChoiceCount;
                if ((otherChoiceCount = font.OtherChoicesCount) > 0)
                {
                    for (int i = 0; i < otherChoiceCount; ++i)
                    {
                        otherChoice = font.GetOtherChoice(i);
                        if ((typeface = _txtServices.GetTypeface(otherChoice.Name, PixelFarm.Drawing.FontStyleExtensions.ConvToInstalledFontStyle(otherChoice.Style))) != null)
                        {
                            break;
                        }
                    }
                }
                //still not found
                if (typeface == null)
                {
                    //register this font key => not found
                    _resolvedTypefaceCache.Add(font.FontKey, ResolvedFont.s_empty);
                    return null;
                }
            }

            //-----------
            //Here, we found some typeface for the request,
            //it may come from primary or other choice


            if (otherChoice != null)
            {
                //this come from other choice
                resolvedFont = new ResolvedFont(typeface, otherChoice.SizeInPoints, otherChoice.Style, otherChoice.FontKey);
            }
            else
            {
                resolvedFont = new ResolvedFont(typeface, font.SizeInPoints, font.Style, font.FontKey);
            }
            //cache to level2
            _resolvedTypefaceCache.Add(resolvedFont.FontKey, resolvedFont);
            //cache to level 1
            PixelFarm.Drawing.Internal.RequestFontCacheAccess.SetResolvedFont1(font, resolvedFont);

            return resolvedFont;

        }
        //public Typeface ResolveTypeface(RequestFont font)
        //{
        //    //from user's request font
        //    //resolve to actual Typeface
        //    //get data from... 
        //    //cache level-1 (attached inside the request font)
        //    Typeface typeface = PixelFarm.Drawing.Internal.RequestFontCacheAccess.GetActualFont<Typeface>(font);
        //    if (typeface != null) return typeface;
        //    //
        //    //cache level-2 (stored in this openfont service)
        //    if (!_resolvedTypefaceCache.TryGetValue(font.FontKey, out typeface))
        //    {
        //        //not found ask the typeface store to load that font
        //        //....
        //        typeface = _txtServices.GetTypeface(font.Name, PixelFarm.Drawing.FontStyleExtensions.ConvToInstalledFontStyle(font.Style));
        //        if (typeface == null)
        //        {
        //            throw new NotSupportedException(font.Name);
        //        }
        //        //
        //        //cache here (level-1)
        //        _resolvedTypefaceCache.Add(font.FontKey, typeface);
        //    }
        //    //and cache into level-0

        //    float pxSize = Typeface.ConvPointsToPixels(font.SizeInPoints);
        //    float pxscale = typeface.CalculateScaleToPixelFromPointSize(font.SizeInPoints);

        //    float recommedLineSpacingInPx = typeface.CalculateRecommendLineSpacing() * pxscale;
        //    float descentInPx = typeface.Descender * pxscale;
        //    float ascentInPx = typeface.Ascender * pxscale;
        //    float lineGapInPx = typeface.LineGap * pxscale;

        //    PixelFarm.Drawing.Internal.RequestFontCacheAccess.SetActualFont(font, typeface);
        //    PixelFarm.Drawing.Internal.RequestFontCacheAccess.SetGeneralFontMetricInfo(font,
        //        pxSize,
        //        ascentInPx,
        //        descentInPx,
        //        lineGapInPx,
        //        recommedLineSpacingInPx);

        //    float advW = typeface.GetWhitespaceWidth() * pxscale;
        //    PixelFarm.Drawing.Internal.RequestFontCacheAccess.SetWhitespaceWidth(font, (int)advW); //rounding?
        //    return typeface;
        //}


        public float MeasureWhitespace(RequestFont f)
        {
            ResolvedFontBase resolvedFont = this.ResolveFont(f);
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
            return CreateGlyphPlanSeq(textBufferSpan, ResolveFont(font).Typeface, font.SizeInPoints);
        }
        public GlyphPlanSequence CreateGlyphPlanSeq(in TextBufferSpan textBufferSpan, ResolvedFont font)
        {
            return CreateGlyphPlanSeq(textBufferSpan, font.Typeface, font.SizeInPoints);
        }
        public Size MeasureString(in TextBufferSpan textBufferSpan, RequestFont font)
        {
            //TODO: review here
            Typeface typeface = ResolveFont(font).Typeface;
            _txtServices.SetCurrentFont(typeface, font.SizeInPoints);
            _txtServices.MeasureString(textBufferSpan.GetRawCharBuffer(), textBufferSpan.start, textBufferSpan.len, out int w, out int h);
            return new Size(w, h);
        }
        public Size MeasureString(in TextBufferSpan textBufferSpan, ResolvedFontBase font)
        {
            //TODO: review here
            Typeface typeface = ((ResolvedFont)font).Typeface;
            _txtServices.SetCurrentFont(typeface, font.SizeInPoints);
            _txtServices.MeasureString(textBufferSpan.GetRawCharBuffer(), textBufferSpan.start, textBufferSpan.len, out int w, out int h);
            return new Size(w, h);
        }
        public void MeasureString(in TextBufferSpan textBufferSpan, RequestFont font, int limitWidth, out int charFit, out int charFitWidth)
        {
            Typeface typeface = ResolveFont(font).Typeface;
            _txtServices.SetCurrentFont(typeface, font.SizeInPoints);
            _txtServices.MeasureString(textBufferSpan.GetRawCharBuffer(), textBufferSpan.start, textBufferSpan.len, limitWidth, out charFit, out charFitWidth);

        }
        float ITextService.MeasureBlankLineHeight(RequestFont font)
        {
            ResolvedFont resolvedFont = ResolveFont(font);
            Typeface typeface = resolvedFont.Typeface;

            return (int)(Math.Round(typeface.CalculateMaxLineClipHeight() * resolvedFont.ScaleToPixel));
        }
        //
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
