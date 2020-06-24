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
    public class OpenFontTextService : ITextService
    {
        /// <summary>
        /// instance of Typography lib's text service
        /// </summary>
        readonly TextServices _txtServices;
        readonly Dictionary<int, Typeface> _resolvedTypefaceCache = new Dictionary<int, Typeface>(); //similar to TypefaceStore

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
            Typeface typeface = ResolveTypeface(font);
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
            Typeface typeface = PixelFarm.Drawing.Internal.RequestFontCacheAccess.GetActualFont<Typeface>(font);
            if (typeface != null) return typeface;
            //
            //cache level-2 (stored in this openfont service)
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

            PixelFarm.Drawing.Internal.RequestFontCacheAccess.SetActualFont(font, typeface);
            PixelFarm.Drawing.Internal.RequestFontCacheAccess.SetGeneralFontMetricInfo(font,
                pxSize,
                ascentInPx,
                descentInPx,
                lineGapInPx,
                recommedLineSpacingInPx);

            float advW = typeface.GetAdvanceWidthFromGlyphIndex(typeface.GetGlyphIndex(' ')) * pxscale;
            PixelFarm.Drawing.Internal.RequestFontCacheAccess.SetWhitespaceWidth(font, (int)advW); //rounding?
            return typeface;
        }


        public float MeasureWhitespace(RequestFont f)
        {

            if (!PixelFarm.Drawing.Internal.RequestFontCacheAccess.GetWhitespaceWidth(f, out int cacheWidth))
            {
                //not have a cache data, new measure here
                Typeface typeface = ResolveTypeface(f);
                cacheWidth = (int)(typeface.GetAdvanceWidthFromGlyphIndex(typeface.GetGlyphIndex(' ')) * typeface.CalculateScaleToPixelFromPointSize(f.SizeInPoints));
                PixelFarm.Drawing.Internal.RequestFontCacheAccess.SetWhitespaceWidth(f, cacheWidth);
            }
            return cacheWidth;
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
