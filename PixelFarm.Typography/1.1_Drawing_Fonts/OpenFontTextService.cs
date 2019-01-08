//Apache2, 2014-present, WinterDev
using System;
using System.Collections.Generic;
using PixelFarm.Drawing;
using PixelFarm.Drawing.Fonts;

using Typography.OpenFont;
using Typography.OpenFont.Extensions;

using Typography.TextLayout;
using Typography.TextServices;
using Typography.FontManagement;

namespace LayoutFarm
{



    public class OpenFontTextService : ITextService
    {
        /// <summary>
        /// instance of Typography lib's text service
        /// </summary>
        TextServices _txtServices;
        Dictionary<int, Typeface> _resolvedTypefaceCache = new Dictionary<int, Typeface>();
        readonly int _system_id;
        //
        public static Typography.OpenFont.ScriptLang DefaultScriptLang { get; set; }

        public OpenFontTextService(Typography.OpenFont.ScriptLang scLang = null)
        {


            _system_id = PixelFarm.Drawing.Internal.RequestFontCacheAccess.GetNewCacheSystemId();

            //set up typography text service
            _txtServices = new TextServices();
            //default, user can set this later

            _txtServices.InstalledFontCollection = InstalledTypefaceCollection.GetSharedTypefaceCollection(collection =>
            {
                collection.SetFontNameDuplicatedHandler((f0, f1) => FontNameDuplicatedDecision.Skip);
                collection.LoadSystemFonts(); //load system fonts
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
            if (scLang == null)
            {
                //use default
                scLang = DefaultScriptLang;
            }
            // if not default then try guess
            if (scLang == null &&
                !TryGetScriptLangFromCurrentThreadCultureInfo(out scLang))
            {
                //TODO: handle error here
            }

            _txtServices.SetDefaultScriptLang(scLang);
            _txtServices.CurrentScriptLang = scLang;

            // ... or specific the scriptlang manully, eg. ...
            //_shapingServices.SetDefaultScriptLang(scLang);
            //_shapingServices.SetCurrentScriptLang(scLang);
            //--------------- 
        }


        public void LoadFontsFromFolder(string folder)
        {
            _txtServices.InstalledFontCollection.LoadFontsFromFolder(folder);
        }
        static bool TryGetScriptLangFromCurrentThreadCultureInfo(out Typography.OpenFont.ScriptLang scLang)
        {
            var currentCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
            scLang = null;
            if (Typography.TextBreak.IcuData.TryGetFullLanguageNameFromLangCode(
                 currentCulture.TwoLetterISOLanguageName,
                 currentCulture.ThreeLetterISOLanguageName,
                 out string langFullName))
            {
                scLang = Typography.OpenFont.ScriptLangs.GetRegisteredScriptLangFromLanguageName(langFullName);
                if (scLang == null)
                {
                    //not found -> use default latin
                    //use default lang
#if DEBUG
                    System.Diagnostics.Debug.WriteLine(langFullName + " :use latin");
#endif
                    scLang = ScriptLangs.Latin;
                    return true;
                }
            }
            return false;
        }

        //
        public ScriptLang CurrentScriptLang => _txtServices.CurrentScriptLang;
        //
        public void CalculateUserCharGlyphAdvancePos(ref TextBufferSpan textBufferSpan, RequestFont font, int[] outputGlyphAdvances, out int outputTotalW, out int outputLineHeight)
        {
            CalculateUserCharGlyphAdvancePos(ref textBufferSpan, this.BreakToLineSegments(ref textBufferSpan), font, outputGlyphAdvances, out outputTotalW, out outputLineHeight);

        }
        //
        ReusableTextBuffer _reusableTextBuffer = new ReusableTextBuffer();
        //
        public void CalculateUserCharGlyphAdvancePos(ref TextBufferSpan textBufferSpan,
            ILineSegmentList lineSegs,
            RequestFont font,
            int[] outputUserInputCharAdvance, out int outputTotalW, out int lineHeight)
        {

            //layout  
            //from font
            //resolve for typeface
            //  
            Typeface typeface = ResolveTypeface(font);
            _txtServices.SetCurrentFont(typeface, font.SizeInPoints);


            MyLineSegmentList mylineSegs = (MyLineSegmentList)lineSegs;
            float scale = typeface.CalculateScaleToPixelFromPointSize(font.SizeInPoints);
            outputTotalW = 0;
            int j = mylineSegs.Count;
            int pos = 0; //start at 0

            _reusableTextBuffer.SetRawCharBuffer(textBufferSpan.GetRawCharBuffer());

            for (int i = 0; i < j; ++i)
            {

                //get each segment
                MyLineSegment lineSeg = (MyLineSegment)mylineSegs.GetSegment(i);
                //each line seg may has different script lang
                _txtServices.CurrentScriptLang = lineSeg.scriptLang;
                //
                //CACHING ...., reduce number of GSUB/GPOS
                //
                //we cache used line segment for a while
                //we ask for caching context for a specific typeface and font size   
                GlyphPlanSequence seq = _txtServices.GetUnscaledGlyphPlanSequence(_reusableTextBuffer,
                     lineSeg.StartAt,
                     lineSeg.Length);

                int seqLen = seq.Count;

                for (int s = 0; s < seqLen; ++s)
                {
                    UnscaledGlyphPlan glyphPlan = seq[s];

                    double actualAdvX = glyphPlan.AdvanceX;

                    outputTotalW +=
                        outputUserInputCharAdvance[pos + glyphPlan.input_cp_offset] +=
                        (int)Math.Round(actualAdvX * scale);
                }
                pos += lineSeg.Length;
            }

            //

            lineHeight = (int)Math.Round(typeface.CalculateRecommendLineSpacing() * scale);

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
                typeface = _txtServices.GetTypeface(font.Name, font.Style.ConvToInstalledFontStyle());
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

            TextBufferSpan w = new TextBufferSpan(new char[] { ' ' });
            Size whiteSpaceW = MeasureString(ref w, font);
            PixelFarm.Drawing.Internal.RequestFontCacheAccess.SetWhitespaceWidth(font, _system_id, whiteSpaceW.Width);
            return typeface;
        }
        public float MeasureWhitespace(RequestFont f)
        {
            ResolveTypeface(f);
            return PixelFarm.Drawing.Internal.RequestFontCacheAccess.GetWhitespaceWidth(f, _system_id);
        }

        public GlyphPlanSequence CreateGlyphPlanSeq(ref TextBufferSpan textBufferSpan, RequestFont font)
        {

            Typeface typeface = ResolveTypeface(font);
            _txtServices.SetCurrentFont(typeface, font.SizeInPoints);

            _reusableTextBuffer.SetRawCharBuffer(textBufferSpan.GetRawCharBuffer());

            return _txtServices.GetUnscaledGlyphPlanSequence(_reusableTextBuffer, textBufferSpan.start, textBufferSpan.len);
        }
        public Size MeasureString(ref TextBufferSpan textBufferSpan, RequestFont font)
        {
            Typeface typeface = ResolveTypeface(font);
            _txtServices.SetCurrentFont(typeface, font.SizeInPoints);
            _txtServices.MeasureString(textBufferSpan.GetRawCharBuffer(), textBufferSpan.start, textBufferSpan.len, out int w, out int h);
            return new Size(w, h);
        }
        public void MeasureString(ref TextBufferSpan textBufferSpan, RequestFont font, int limitWidth, out int charFit, out int charFitWidth)
        {
            Typeface typeface = ResolveTypeface(font);
            _txtServices.SetCurrentFont(typeface, font.SizeInPoints);

            charFit = 0;
            _txtServices.MeasureString(textBufferSpan.GetRawCharBuffer(), textBufferSpan.start, textBufferSpan.len, limitWidth, out charFit, out charFitWidth);

        }
        float ITextService.MeasureBlankLineHeight(RequestFont font)
        {
            LineSpacingChoice sel_linespcingChoice;
            Typeface typeface = ResolveTypeface(font);
            return (int)(Math.Round(typeface.CalculateRecommendLineSpacing(out sel_linespcingChoice) *
                                    typeface.CalculateScaleToPixelFromPointSize(font.SizeInPoints)));
        }
        //
        public bool SupportsWordBreak => true;
        //
        struct MyLineSegment : ILineSegment
        {
            ILineSegmentList _owner;
            readonly int _startAt;
            readonly int _len;
            internal ScriptLang scriptLang;
            public MyLineSegment(ILineSegmentList owner, int startAt, int len)
            {
                _owner = owner;
                _startAt = startAt;
                _len = len;
                this.scriptLang = null;
            }
            public int Length => _len;
            public int StartAt => _startAt;
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



        public ILineSegmentList BreakToLineSegments(ref TextBufferSpan textBufferSpan)
        {
            //a text buffer span is separated into multiple line segment list 
            char[] str = textBufferSpan.GetRawCharBuffer();
            int cur_startAt = textBufferSpan.start;


            MyLineSegmentList lineSegments = MyLineSegmentList.GetFreeLineSegmentList();
            foreach (BreakSpan breakSpan in _txtServices.BreakToLineSegments(str, textBufferSpan.start, textBufferSpan.len))
            {
                MyLineSegment lineSeg = new MyLineSegment(lineSegments, breakSpan.startAt, breakSpan.len);
                lineSeg.scriptLang = breakSpan.scLang;
                lineSegments.AddLineSegment(lineSeg);
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
