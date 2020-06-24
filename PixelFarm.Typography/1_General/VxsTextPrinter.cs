//MIT, 2016-present, WinterDev, Sam Hocevar
using System;
using System.Collections.Generic;
using PixelFarm.CpuBlit.BitmapAtlas;

using Typography.Contours;
using Typography.OpenFont;
using Typography.OpenFont.Tables;
using Typography.TextLayout;
using Typography.TextBreak;
using Typography.FontManagement;
using PixelFarm.CpuBlit;
using PixelFarm.CpuBlit.VertexProcessing;

namespace PixelFarm.Drawing
{
    public class MyAlternativeTypefaceSelector : AlternativeTypefaceSelector
    {
        readonly Dictionary<string, PreferTypefaceList> _dics = new Dictionary<string, PreferTypefaceList>();
        PreferTypefaceList _emojiPreferList = new PreferTypefaceList();

#if DEBUG
        public MyAlternativeTypefaceSelector() { }
#endif
        public void SetPreferTypefaces(string scriptTag, PreferTypefaceList typefaceNames)
        {
            _dics[scriptTag] = typefaceNames;
        }
        public void SetPreferTypefaces(ScriptTagDef scriptTag, PreferTypefaceList typefaceNames)
        {
            _dics[scriptTag.StringTag] = typefaceNames;
        }

        public void SetPerferEmoji(PreferTypefaceList typefaceNames)
        {
            _emojiPreferList = typefaceNames;
        }

        public PreferTypefaceList GetPreferTypefaces(string scriptTag) => _dics.TryGetValue(scriptTag, out PreferTypefaceList foundList) ? foundList : null;

        public override InstalledTypeface Select(List<InstalledTypeface> choices, ScriptLangInfo scriptLangInfo, int hintCodePoint, AddtionalHint additionalHint)
        {
            List<PreferTypeface> list = null;
            if (additionalHint.UnicodeHint == UnicodeHint.Emoji)
            {
                list = _emojiPreferList._list;
            }
            else if (_dics.TryGetValue(scriptLangInfo.shortname, out PreferTypefaceList foundList))
            {
                list = foundList._list;
            }

            if (list != null)
            {
                int j = list.Count;
                for (int i = 0; i < j; ++i)
                {
                    //select that first one
                    PreferTypeface p = list[i];

                    if (p.InstalledTypeface == null && !p.ResolvedInstalledTypeface)
                    {
                        //find
                        int choice_count = choices.Count;

                        for (int m = 0; m < choice_count; ++m)
                        {
                            InstalledTypeface instTypeface = choices[m];
                            if (p.RequestTypefaceName == instTypeface.FontName)
                            {
                                //TODO: review here again
                                p.InstalledTypeface = instTypeface;

                                break;
                            }
                        }
                        p.ResolvedInstalledTypeface = true;
                    }
                    //-------
                    if (p.InstalledTypeface != null)
                    {
                        return p.InstalledTypeface;
                    }
                }
            }

            return base.Select(choices, scriptLangInfo, hintCodePoint, additionalHint);
        }

        public class PreferTypeface
        {
            public PreferTypeface(string reqTypefaceName) => RequestTypefaceName = reqTypefaceName;
            public string RequestTypefaceName { get; }
            public InstalledTypeface InstalledTypeface { get; internal set; }
            internal bool ResolvedInstalledTypeface { get; set; }
        }


        public class PreferTypefaceList
        {
            internal List<PreferTypeface> _list = new List<PreferTypeface>();
            public void AddTypefaceName(string typefaceName)
            {
                _list.Add(new PreferTypeface(typefaceName));
            }
        }
    }

    public class SvgBmpBuilderReq
    {
        //input
        public System.Text.StringBuilder SvgContent;
        public float ExpectedWidth;
        public Color DefaultBgColor = Color.White;
        //output 
        public MemBitmap Output;
        public int BitmapXOffset;
        public int BitmapYOffset;

    }

    public delegate void SvgBmpBuilderFunc(SvgBmpBuilderReq req);

    public interface ILineSegmentList
    {
        int Count { get; }
        ILineSegment this[int index] { get; }
    }
    public interface ILineSegment
    {
        int StartAt { get; }
        ushort Length { get; }
    }


    //---------
    public struct TextPrinterLineSegment : ILineSegment
    {
        public TextPrinterLineSegment(int startAt, int len, WordKind wordKind, SpanBreakInfo breakInfo)
        {
            StartAt = startAt;
            Length = (ushort)len; //***
            WordKind = wordKind;
            BreakInfo = breakInfo;
        }
        public int StartAt { get; }
        public ushort Length { get; }
        public WordKind WordKind { get; }
        public SpanBreakInfo BreakInfo { get; }
#if DEBUG
        public override string ToString()
        {
            return StartAt + ":" + Length + (BreakInfo.RightToLeft ? "(rtl)" : "");
        }
#endif
    }

    public class TextPrinterLineSegmentList<T> : ILineSegmentList
        where T : ILineSegment
    {
        List<T> _segments = new List<T>();
        public TextPrinterLineSegmentList()
        {
        }
        public void AddLineSegment(T lineSegment)
        {
            _segments.Add(lineSegment);
        }
        public void Clear()
        {
            _segments.Clear();
        }

        public T GetLineSegment(int index) => _segments[index];
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
        public void Dispose()
        {
        }
    }


    public class TextPrinterWordVisitor : WordVisitor
    {
        TextPrinterLineSegmentList<TextPrinterLineSegment> _lineSegs;
        public void SetLineSegmentList(TextPrinterLineSegmentList<TextPrinterLineSegment> lineSegs)
        {
            _lineSegs = lineSegs;
        }
        protected override void OnBreak()
        {
            _lineSegs.AddLineSegment(new TextPrinterLineSegment(
                this.LatestSpanStartAt,
                this.LatestSpanLen,
                this.LatestWordKind,
                this.SpanBreakInfo));
        }
    }



    public class VxsTextPrinter : TextPrinterBase, ITextPrinter
    {
        OpenFontTextService _textServices;

        /// <summary>
        /// target canvas
        /// </summary>
        Painter _painter;

        Typeface _currentTypeface;
        GlyphMeshStore _glyphMeshStore;
        float _currentFontSizePxScale;

        GlyphBitmapStore _glyphBitmapStore;

        public VxsTextPrinter(Painter painter, OpenFontTextService textService)
        {

            //
            _painter = painter;
            _glyphMeshStore = new GlyphMeshStore();
            _glyphMeshStore.FlipGlyphUpward = true;
            this.PositionTechnique = PositionTechnique.OpenFont;
            //
            _textServices = textService;
            ChangeFont(new RequestFont("Source Sans Pro", 10));

            _glyphBitmapStore = new GlyphBitmapStore();

        }
        public AlternativeTypefaceSelector AlternativeTypefaceSelector { get; set; }


        SvgBmpBuilderFunc _svgBmpBuilderFunc;
        public void SetSvgBmpBuilderFunc(SvgBmpBuilderFunc svgBmpBuilderFunc)
        {
            _svgBmpBuilderFunc = svgBmpBuilderFunc;
        }

        public AntialiasTechnique AntialiasTechnique { get; set; }

        public void ChangeFont(RequestFont font)
        {
            //1.  resolve actual font file             
            this.Typeface = _textServices.ResolveTypeface(font); //resolve for 'actual' font 
            this.FontSizeInPoints = font.SizeInPoints;
        }
        public void ChangeFillColor(Color fontColor)
        {
            //change font color

#if DEBUG
            Console.Write("please impl change font color");
#endif
        }
        public void ChangeStrokeColor(Color strokeColor)
        {

        }

        protected override void OnFontSizeChanged()
        {
            //update some font metrics property   
            Typeface currentTypeface = _currentTypeface;
            if (currentTypeface != null)
            {
                float pointToPixelScale = currentTypeface.CalculateScaleToPixelFromPointSize(this.FontSizeInPoints);
                this.FontAscendingPx = currentTypeface.Ascender * pointToPixelScale;
                this.FontDescedingPx = currentTypeface.Descender * pointToPixelScale;
                this.FontLineGapPx = currentTypeface.LineGap * pointToPixelScale;
                this.FontLineSpacingPx = FontAscendingPx - FontDescedingPx + FontLineGapPx;
            }
        }
        public override GlyphLayout GlyphLayoutMan
        {
            get
            {
                //TODO: review here
                throw new NotSupportedException();
            }
        }


        Typography.OpenFont.Tables.COLR _colrTable;
        Typography.OpenFont.Tables.CPAL _cpalTable;
        bool _hasColorInfo;
        public override Typeface Typeface
        {
            get => _currentTypeface;

            set
            {

                if (_currentTypeface == value) return;
                // 
                _currentTypeface = value;
                if (value == null) { return; }
                //
                _colrTable = _currentTypeface.COLRTable;
                _cpalTable = _currentTypeface.CPALTable;
                _hasColorInfo = _colrTable != null && _cpalTable != null;

                OnFontSizeChanged();
            }
        }

        public void PrepareStringForRenderVx(RenderVxFormattedString renderVx, char[] text, int startAt, int len)
        {
            UpdateGlyphLayoutSettings();
        }
        public void PrepareStringForRenderVx(RenderVxFormattedString renderVx)
        {
            UpdateGlyphLayoutSettings();
        }

        public void UpdateGlyphLayoutSettings()
        {
            if (Typeface == null) return;
            //
            _glyphMeshStore.SetHintTechnique(this.HintTechnique);
            _currentFontSizePxScale = Typeface.CalculateScaleToPixelFromPointSize(FontSizeInPoints);
            _textServices.CurrentScriptLang = this.ScriptLang;
        }

        public void MeasureString(char[] buffer, int startAt, int len, out int w, out int h)
        {
            UpdateGlyphLayoutSettings();
            _glyphMeshStore.SetFont(_currentTypeface, this.FontSizeInPoints);
            _glyphMeshStore.SimulateOblique = this.SimulateSlant;
            var textBuffSpan = new TextBufferSpan(buffer, startAt, len);
            Size s = _textServices.MeasureString(textBuffSpan, _painter.CurrentFont);
            w = s.Width;
            h = s.Height;
        }
        public void DrawString(RenderVxFormattedString renderVx, double left, double top)
        {
            //TODO: review here
            float ox = _painter.OriginX;
            float oy = _painter.OriginY;

            //1. update some props.. 
            //2. update current type face
            UpdateGlyphLayoutSettings();
            _glyphMeshStore.SetFont(_currentTypeface, this.FontSizeInPoints);
            _glyphMeshStore.SimulateOblique = this.SimulateSlant;

            //3. layout glyphs with selected layout technique
            //TODO: review this again, we should use pixel? 
            float fontSizePoint = this.FontSizeInPoints;
            float scale = _currentTypeface.CalculateScaleToPixelFromPointSize(fontSizePoint);
            Internal.RenderVxGlyphPlan[] glyphPlans = ((PixelFarm.CpuBlit.AggRenderVxFormattedString)renderVx).GlyphList;
            int j = glyphPlans.Length;
            //---------------------------------------------------
            //consider use cached glyph, to increase performance 

            float g_x = 0;
            float g_y = 0;
            float baseY = (int)top; //TODO, review here again

            for (int i = 0; i < j; ++i)
            {
                Internal.RenderVxGlyphPlan glyphPlan = glyphPlans[i];
                //-----------------------------------
                //TODO: review here ***
                //PERFORMANCE revisit here 
                //if we have create a vxs we can cache it for later use?
                //-----------------------------------  
                VertexStore vxs = _glyphMeshStore.GetGlyphMesh(glyphPlan.glyphIndex);
                g_x = (float)(glyphPlan.x * scale + left);
                g_y = (float)glyphPlan.y * scale;

                _painter.SetOrigin(g_x, g_y);
                _painter.Fill(vxs);
            }
            //restore prev origin
            _painter.SetOrigin(ox, oy);
        }

        public int LatestAccumulateWidth { get; private set; }

#if DEBUG
        int dbugExportCount = 0;
#endif

        GlyphBitmap GetGlyphBitmapFromSvg(ushort glyphIndex)
        {
            if (_glyphBitmapStore.CurrrentBitmapCache.TryGetBitmap(glyphIndex, out GlyphBitmap glyphBmp))
            {
                return glyphBmp;
            }


            Glyph glyph = _currentTypeface.GetGlyph(glyphIndex);


            //TODO: use string builder from pool?
            var stbuilder = new System.Text.StringBuilder();
            _currentTypeface.ReadSvgContent(glyph, stbuilder);

            float bmpScale = _currentTypeface.CalculateScaleToPixelFromPointSize(FontSizeInPoints);
            float target_advW = _currentTypeface.GetAdvanceWidthFromGlyphIndex(glyphIndex) * bmpScale;

            var req = new SvgBmpBuilderReq
            {
                SvgContent = stbuilder,
                ExpectedWidth = target_advW
            };

            _svgBmpBuilderFunc.Invoke(req);

            MemBitmap memBmp = req.Output;

            if (memBmp == null)
            {
                //TODO: use blank img?
                return null;
            }

            TypefaceGlyphBitmapCache currentCache = _glyphBitmapStore.CurrrentBitmapCache;
            //find bitmap scale             

            //TODO...
            short offset_x = 0;
            short offset_y = 0;

            currentCache.RegisterBitmap(glyphIndex,
                glyphBmp = new GlyphBitmap
                {
                    Bitmap = memBmp,
                    Width = memBmp.Width,
                    Height = memBmp.Height,
                    ImageStartX = -offset_x,//offset back
                    ImageStartY = -offset_y //offset back
                });

            return glyphBmp;
        }
        GlyphBitmap GetGlyphBitmapFromColorOutlineGlyph(ushort glyphIndex, ushort colorLayerStart)
        {
            if (_glyphBitmapStore.CurrrentBitmapCache.TryGetBitmap(glyphIndex, out GlyphBitmap glyphBmp))
            {
                return glyphBmp;
            }


            //not found=> create a newone 
            Q1RectD totalBounds = Q1RectD.ZeroIntersection();
            {
                //calculate bounds of this glyph
                ushort colorLayerCount = _colrTable.LayerCounts[glyphIndex];
                for (int c = colorLayerStart; c < colorLayerStart + colorLayerCount; ++c)
                {
                    BoundingRect.GetBoundingRect(_glyphMeshStore.GetGlyphMesh(_colrTable.GlyphLayers[c]), ref totalBounds);
                }
            }
            //dbugExportCount++;     
            var memBmp = new MemBitmap((int)Math.Round(totalBounds.Width), (int)Math.Round(totalBounds.Height));//???
            int offset_x = 0;
            int offset_y = 0;
            using (Tools.BorrowAggPainter(memBmp, out AggPainter painter))
            {
                painter.Clear(Color.Transparent);
                painter.SetOrigin(0, 0);

                offset_x = -(int)(totalBounds.Left);
                offset_y = -(int)(totalBounds.Bottom);

                ushort colorLayerCount = _colrTable.LayerCounts[glyphIndex];
                int palette = 0; // FIXME: assume palette 0 for now 
                for (int c = colorLayerStart; c < colorLayerStart + colorLayerCount; ++c)
                {

                    _cpalTable.GetColor(
                        _cpalTable.Palettes[palette] + _colrTable.GlyphPalettes[c], //index
                         out byte r, out byte g, out byte b, out byte a);

                    ushort gIndex = _colrTable.GlyphLayers[c];
                    VertexStore vxs = _glyphMeshStore.GetGlyphMesh(gIndex);
                    using (Tools.BorrowVxs(out var v1))
                    {
                        vxs.TranslateToNewVxs(offset_x, offset_y, v1);
                        painter.FillColor = new Color(r, g, b);//? a component
                        painter.Fill(v1);
                    }
                }
                //find ex
#if DEBUG
                //memBmp.SaveImage("a0x" + (dbugExportCount) + ".png");
#endif
            }

            TypefaceGlyphBitmapCache currentCache = _glyphBitmapStore.CurrrentBitmapCache;
            //find bitmap scale             

            currentCache.RegisterBitmap(glyphIndex,
                glyphBmp = new GlyphBitmap
                {
                    Bitmap = memBmp,
                    Width = memBmp.Width,
                    Height = memBmp.Height,
                    ImageStartX = -offset_x,//offset back
                    ImageStartY = -offset_y //offset back
                });

            return glyphBmp;
        }
        GlyphBitmap GetGlyphBitmapFromBitmapFont(ushort glyphIndex)
        {

            TypefaceGlyphBitmapCache currentCache = _glyphBitmapStore.CurrrentBitmapCache;
            float actualCacheSize = currentCache.ActualCacheSize;

            //actual size of glyph that we store may not equal the current req size
            //since we do not store all font size in the cache

#if DEBUG
            if (actualCacheSize != FontSizeInPoints)
            {
                System.Diagnostics.Debugger.Break();
            }
#endif

            if (currentCache.TryGetBitmap(glyphIndex, out GlyphBitmap glyphBmp))
            {
                return glyphBmp;
            }

            //not found=> create a new one
            if (_currentTypeface.IsBitmapFont)
            {
                //try load
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    //load actual bitmap font
                    Glyph glyph = _currentTypeface.GetGlyph(glyphIndex);
                    _currentTypeface.ReadBitmapContent(glyph, ms);

                    using (MemBitmap memBitmap = MemBitmap.LoadBitmap(ms))
                    {
                        //bitmap that are load may be larger than we need
                        //so we need to scale it to specfic size

                        float bmpScale = _currentTypeface.CalculateScaleToPixelFromPointSize(FontSizeInPoints);
                        float target_advW = _currentTypeface.GetAdvanceWidthFromGlyphIndex(glyphIndex) * bmpScale;
                        float scaleForBmp = target_advW / memBitmap.Width;

                        MemBitmap scaledMemBmp = memBitmap.ScaleImage(scaleForBmp, scaleForBmp);

                        var glyphBitmap = new GlyphBitmap
                        {
                            Width = scaledMemBmp.Width,
                            Height = scaledMemBmp.Height,
                            Bitmap = scaledMemBmp //**
                        };

                        _glyphBitmapStore.CurrrentBitmapCache.RegisterBitmap(glyphIndex, glyphBitmap);
                        return glyphBitmap;
                    }
                }
            }
            return null;
        }
        public override void DrawFromGlyphPlans(GlyphPlanSequence seq, int startAt, int len, float left, float top)
        {
            LatestAccumulateWidth = 0;//reset

            if (_currentTypeface == null) return;

            float baseLine = top;
            switch (TextBaseline)
            {
                case TextBaseline.Alphabetic:

                    break;
                case TextBaseline.Top:
                    baseLine += this.FontAscendingPx;
                    break;
                case TextBaseline.Bottom:
                    baseLine += this.FontDescedingPx;
                    break;
            }

            float fontSizePoint = this.FontSizeInPoints;
            float scale = _currentTypeface.CalculateScaleToPixelFromPointSize(fontSizePoint);

            //4. render each glyph 
            float ox = _painter.OriginX;
            float oy = _painter.OriginY;

            //--------------------------------------------------- 
            _glyphMeshStore.SetHintTechnique(this.HintTechnique);
            _glyphMeshStore.SetFont(_currentTypeface, fontSizePoint);
            _glyphMeshStore.SimulateOblique = this.SimulateSlant;
            //---------------------------------------------------


            if (_currentTypeface.HasSvgTable())
            {
                //Test svg font with Twitter Color Emoji Regular

                _glyphBitmapStore.SetCurrentTypeface(_currentTypeface, fontSizePoint);

                int seqLen = seq.Count;
                if (len > seqLen)
                {
                    len = seqLen;
                }

                var snapToPx = new GlyphPlanSequenceSnapPixelScaleLayout(seq, startAt, len, scale);
                while (snapToPx.Read())
                {
                    _painter.SetOrigin((float)Math.Round(left + snapToPx.ExactX) + 0.33f, (float)Math.Floor(baseLine + snapToPx.ExactY));

                    //***
                    GlyphBitmap glyphBmp = GetGlyphBitmapFromSvg(snapToPx.CurrentGlyphIndex);
                    //how to draw the image
                    //1. 
                    if (glyphBmp != null)
                    {
                        _painter.DrawImage(glyphBmp.Bitmap);
                    }
                }
                LatestAccumulateWidth = snapToPx.AccumWidth;
            }
            else if (_currentTypeface.IsBitmapFont)
            {
                //Test IsBitmapFont font with Noto Color Emoji

                //check if we have exported all the glyph bitmap 
                //to some 'ready' form?
                //if not then create it

                //TODO: review this again
                _glyphBitmapStore.SetCurrentTypeface(_currentTypeface, fontSizePoint);

                int seqLen = seq.Count;

                if (len > seqLen)
                {
                    len = seqLen;
                }

                var snapToPx = new GlyphPlanSequenceSnapPixelScaleLayout(seq, startAt, len, scale);
                while (snapToPx.Read())
                {
                    _painter.SetOrigin((float)Math.Round(left + snapToPx.ExactX) + 0.33f, (float)Math.Floor(baseLine + snapToPx.ExactY));

                    GlyphBitmap glyphBmp = GetGlyphBitmapFromBitmapFont(snapToPx.CurrentGlyphIndex);
                    //how to draw the image
                    if (glyphBmp != null)
                    {
                        _painter.DrawImage(glyphBmp.Bitmap);
                    }
                }
                LatestAccumulateWidth = snapToPx.AccumWidth;
            }
            else
            {
                if (!_hasColorInfo)
                {
                    //NO color information, 

                    bool savedUseLcdMode = _painter.UseLcdEffectSubPixelRendering; //save,restore later
                    RenderQuality savedRederQuality = _painter.RenderQuality;
                    _painter.RenderQuality = RenderQuality.HighQuality;
                    _painter.UseLcdEffectSubPixelRendering = true;

                    int seqLen = seq.Count;

                    if (len > seqLen)
                    {
                        len = seqLen;
                    }

                    var snapToPx = new GlyphPlanSequenceSnapPixelScaleLayout(seq, startAt, len, scale);
                    while (snapToPx.Read())
                    {
                        _painter.SetOrigin((float)Math.Round(left + snapToPx.ExactX) + 0.33f, (float)Math.Floor(baseLine + snapToPx.ExactY));
                        _painter.Fill(_glyphMeshStore.GetGlyphMesh(snapToPx.CurrentGlyphIndex));
                    }

                    LatestAccumulateWidth = snapToPx.AccumWidth;
                    //restore
                    _painter.RenderQuality = savedRederQuality;
                    _painter.UseLcdEffectSubPixelRendering = savedUseLcdMode;

                }
                else
                {
                    //Test Color Outline Font with Firefox Emoji
                    //-------------    
                    //this glyph has color information
                    //-------------
                    Color originalFillColor = _painter.FillColor;
                    int seqLen = seq.Count;

                    if (len > seqLen)
                    {
                        len = seqLen;
                    }

                    var snapToPx = new GlyphPlanSequenceSnapPixelScaleLayout(seq, startAt, len, scale);
                    float maxAccumWidth = 0;

                    if (EnableColorGlyphBitmapCache)
                    {
                        _glyphBitmapStore.SetCurrentTypeface(_currentTypeface, fontSizePoint);
                    }


                    while (snapToPx.Read())
                    {
                        float start_pos_x = (float)Math.Round(left + snapToPx.ExactX);

                        _painter.SetOrigin(start_pos_x, (float)Math.Floor(baseLine + snapToPx.ExactY));

                        if (_colrTable.LayerIndices.TryGetValue(snapToPx.CurrentGlyphIndex, out ushort colorLayerStart))
                        {
                            //check if we have a bitmap cache for this glyph or not
                            //
                            if (EnableColorGlyphBitmapCache)
                            {
                                GlyphBitmap glyphBmp = GetGlyphBitmapFromColorOutlineGlyph(snapToPx.CurrentGlyphIndex, colorLayerStart);

                                if (glyphBmp != null)
                                {
                                    _painter.DrawImage(glyphBmp.Bitmap, glyphBmp.ImageStartX, glyphBmp.ImageStartY);

                                    if (start_pos_x + glyphBmp.ImageStartX + glyphBmp.Width > maxAccumWidth)
                                    {
                                        maxAccumWidth = start_pos_x + glyphBmp.ImageStartX + glyphBmp.Width;
                                    }
                                }

                                LatestAccumulateWidth = (int)maxAccumWidth;
                            }
                            else
                            {

                                //TODO: optimize this                        
                                //we found color info for this glyph 
                                ushort colorLayerCount = _colrTable.LayerCounts[snapToPx.CurrentGlyphIndex];

                                int palette = 0; // FIXME: assume palette 0 for now 
                                for (int c = colorLayerStart; c < colorLayerStart + colorLayerCount; ++c)
                                {
                                    _cpalTable.GetColor(
                                        _cpalTable.Palettes[palette] + _colrTable.GlyphPalettes[c], //index
                                        out byte r, out byte g, out byte b, out byte a);

                                    _painter.FillColor = new Color(r, g, b);//? a component
                                    ushort gIndex = _colrTable.GlyphLayers[c];
                                    _painter.Fill(_glyphMeshStore.GetGlyphMesh(gIndex));
                                }

                                LatestAccumulateWidth = snapToPx.AccumWidth;
                            }
                        }
                        else
                        {
                            //-----------------------------------
                            //TODO: review here ***
                            //PERFORMANCE revisit here 
                            //if we have create a vxs we can cache it for later use?
                            //----------------------------------- 
                            _painter.Fill(_glyphMeshStore.GetGlyphMesh(snapToPx.CurrentGlyphIndex));
                            LatestAccumulateWidth = snapToPx.AccumWidth;
                        }
                    }

                    _painter.FillColor = originalFillColor; //restore color
                }
            }
            //restore prev origin
            _painter.SetOrigin(ox, oy);
        }

        bool EnableColorGlyphBitmapCache { get; set; } = true;

        void ITextPrinter.DrawString(char[] textBuffer, int startAt, int len, double x, double y)
        {
            InnerDrawString(textBuffer, startAt, len, (float)x, (float)y);
        }
        public override void DrawString(char[] textBuffer, int startAt, int len, float x, float y)
        {
            InnerDrawString(textBuffer, startAt, len, x, y);
        }


        TextPrinterWordVisitor _textPrinterWordVisitor = new TextPrinterWordVisitor();
        TextPrinterLineSegmentList<TextPrinterLineSegment> _textPrinterLineSegmentList = new TextPrinterLineSegmentList<TextPrinterLineSegment>();

        void InnerDrawString(char[] textBuffer, int startAt, int len, float x, float y)
        {

#if DEBUG
            if (textBuffer.Length > 3)
            {

            }
#endif 

            UpdateGlyphLayoutSettings();

            //unscale layout, with design unit scale
            var buffSpan = new TextBufferSpan(textBuffer, startAt, len);

            float xpos = x;
            float ypos = y;

            if (!EnableMultiTypefaces)
            {
                GlyphPlanSequence glyphPlanSeq = _textServices.CreateGlyphPlanSeq(buffSpan, _currentTypeface, FontSizeInPoints);
                DrawFromGlyphPlans(glyphPlanSeq, xpos, y);
            }
            else
            {

                //a single string may be broken into many glyph-plan-seq
                //set segmentlist

                _textPrinterLineSegmentList.Clear();//clear before reuse
                _textPrinterWordVisitor.SetLineSegmentList(_textPrinterLineSegmentList);
                _textServices.BreakToLineSegments(buffSpan, _textPrinterWordVisitor);//***
                _textPrinterWordVisitor.SetLineSegmentList(null);


                int count = _textPrinterLineSegmentList.Count;

                ClearTempFormattedGlyphPlanSeq();

                bool needRightToLeftArr = false;

                Typeface defaultTypeface = _currentTypeface;
                Typeface curTypeface = defaultTypeface;

                for (int i = 0; i < count; ++i)
                {
                    //
                    TextPrinterLineSegment line_seg = _textPrinterLineSegmentList.GetLineSegment(i);

                    SpanBreakInfo spBreakInfo = line_seg.BreakInfo;
                    if (spBreakInfo.RightToLeft)
                    {
                        needRightToLeftArr = true;
                    }

                    TextBufferSpan buff = new TextBufferSpan(textBuffer, line_seg.StartAt, line_seg.Length);

                    //each line segment may have different unicode range 
                    //and the current typeface may not support that range
                    //so we need to ensure that we get a proper typeface,
                    //if not => alternative typeface

                    ushort glyphIndex = 0;
                    char sample_char = textBuffer[line_seg.StartAt];
                    int codepoint = sample_char;

                    bool contains_surrogate_pair = false;

                    if (line_seg.Length > 1 && line_seg.WordKind == WordKind.SurrogatePair)
                    {
                        contains_surrogate_pair = true;//***
                        //bind 2 char to utf32=> codepoint
                        //and get glyphIndex from current typeface
                        glyphIndex = curTypeface.GetGlyphIndex(char.ConvertToUtf32(sample_char, textBuffer[line_seg.StartAt + 1]));
                    }
                    else
                    {
                        glyphIndex = curTypeface.GetGlyphIndex(codepoint);
                    }


                    if (glyphIndex == 0)
                    {
                        //not found then => find other typeface                    
                        //we need more information about line seg layout

                        if (AlternativeTypefaceSelector != null)
                        {
                            AlternativeTypefaceSelector.LatestTypeface = curTypeface;
                        }

                        if (_textServices.TryGetAlternativeTypefaceFromCodepoint(codepoint, AlternativeTypefaceSelector, out Typeface alternative))
                        {
                            curTypeface = alternative;
                        }
                        else
                        {
#if DEBUG
                            if (sample_char >= 0 && sample_char < 255)
                            {


                            }
#endif
                        }
                    }


                    _textServices.CurrentScriptLang = new ScriptLang(spBreakInfo.ScriptTag, spBreakInfo.LangTag);
                    GlyphPlanSequence seq = _textServices.CreateGlyphPlanSeq(buff, curTypeface, FontSizeInPoints);
                    seq.IsRightToLeft = spBreakInfo.RightToLeft;

                    FormattedGlyphPlanSeq formattedGlyphPlanSeq = _pool.GetFreeFmtGlyphPlanSeqs();
                    formattedGlyphPlanSeq.seq = seq;
                    formattedGlyphPlanSeq.Typeface = curTypeface;
                    formattedGlyphPlanSeq.ContainsSurrogatePair = contains_surrogate_pair;

                    _tmpGlyphPlanSeqs.Add(formattedGlyphPlanSeq);

                    curTypeface = defaultTypeface;//switch back to default

                    //restore latest script lang?
                }

                if (needRightToLeftArr)
                {
                    //special arr left-to-right
                    for (int i = count - 1; i >= 0; --i)
                    {
                        FormattedGlyphPlanSeq formattedGlyphPlanSeq = _tmpGlyphPlanSeqs[i];

                        Typeface = formattedGlyphPlanSeq.Typeface;

                        DrawFromGlyphPlans(formattedGlyphPlanSeq.seq, xpos, y);


                        //xpos += (glyphPlanSeq.CalculateWidth() * _currentFontSizePxScale);
                        xpos += LatestAccumulateWidth;
                    }
                }
                else
                {
                    for (int i = 0; i < count; ++i)
                    {
                        FormattedGlyphPlanSeq formattedGlyphPlanSeq = _tmpGlyphPlanSeqs[i];

                        //change typeface                            
                        Typeface = formattedGlyphPlanSeq.Typeface;
                        //update pxscale size                             
                        _currentFontSizePxScale = Typeface.CalculateScaleToPixelFromPointSize(FontSizeInPoints);

                        DrawFromGlyphPlans(formattedGlyphPlanSeq.seq, xpos, y);
                        xpos += LatestAccumulateWidth;

                    }
                }

                Typeface = defaultTypeface;
                ClearTempFormattedGlyphPlanSeq();

            }
        }

        void ClearTempFormattedGlyphPlanSeq()
        {
            for (int i = _tmpGlyphPlanSeqs.Count - 1; i >= 0; --i)
            {
                _pool.ReleaseFmtGlyphPlanSeqs(_tmpGlyphPlanSeqs[i]);
            }
            _tmpGlyphPlanSeqs.Clear();
        }

        FormattedGlyphPlanSeqPool _pool = new FormattedGlyphPlanSeqPool();
        List<FormattedGlyphPlanSeq> _tmpGlyphPlanSeqs = new List<FormattedGlyphPlanSeq>();
    }

    public class FormattedGlyphPlanSeqPool
    {
        Queue<FormattedGlyphPlanSeq> _pool = new Queue<FormattedGlyphPlanSeq>();

        public FormattedGlyphPlanSeq GetFreeFmtGlyphPlanSeqs() => (_pool.Count > 0) ? _pool.Dequeue() : new FormattedGlyphPlanSeq();
        public void ReleaseFmtGlyphPlanSeqs(FormattedGlyphPlanSeq seq)
        {
            seq.Reset();
            _pool.Enqueue(seq);
        }
    }
    public class FormattedGlyphPlanSeq
    {
        static readonly GlyphPlanSequence s_EmptyGlypgPlanSeq = new GlyphPlanSequence();

        public GlyphPlanSequence seq;

        public Typeface Typeface;
        public bool ContainsSurrogatePair;
        public bool IsEmpty() => Typeface == null;
        public void Reset()
        {

            seq = s_EmptyGlypgPlanSeq;
            Typeface = null;
        }
    }

    public interface IMultiLayerGlyphTranslator : IGlyphTranslator
    {
        void HasColorInfo(int nsubLayer);//if 0 => no color info
        void BeginSubGlyph(ushort glyphIndex);
        void EndSubGlyph(ushort glyphIndex);
        void SetFillColor(byte r, byte g, byte b, byte a);
    }



    static class MultiLayeGlyphTx
    {
        //experiment

        /// <summary>
        /// build a multi-layer glyph (eg. Emoji)
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="glyphIndex"></param>
        /// <param name="sizeInPoints"></param>
        /// <param name="tx"></param>
        public static void BuildFromGlyphIndex(this GlyphOutlineBuilderBase builder, ushort glyphIndex, float sizeInPoints, IMultiLayerGlyphTranslator tx)
        {
            //1. current typeface support multilayer or not
            if (builder.HasColorInfo)
            {
                Typeface typeface = builder.Typeface;
                COLR colrTable = typeface.COLRTable;
                CPAL cpalTable = typeface.CPALTable;

                if (colrTable.LayerIndices.TryGetValue(glyphIndex, out ushort colorLayerStart))
                {
                    //has color information on this glyphIndex

                    ushort colorLayerCount = colrTable.LayerCounts[glyphIndex];
                    tx.HasColorInfo(colorLayerCount);


                    for (int c = colorLayerStart; c < colorLayerStart + colorLayerCount; ++c)
                    {
                        ushort gIndex = colrTable.GlyphLayers[c];
                        tx.BeginSubGlyph(gIndex);//BEGIN SUB GLYPH

                        int palette = 0; // FIXME: assume palette 0 for now 
                        cpalTable.GetColor(
                            cpalTable.Palettes[palette] + colrTable.GlyphPalettes[c], //index
                            out byte r, out byte g, out byte b, out byte a);

                        tx.SetFillColor(r, g, b, a); //SET COLOR

                        builder.BuildFromGlyphIndex(glyphIndex, sizeInPoints);

                        builder.ReadShapes(tx);

                        tx.EndSubGlyph(gIndex);//END SUB GLYPH
                    }

                }
                else
                {
                    //build as normal glyph
                    builder.BuildFromGlyphIndex(glyphIndex, sizeInPoints);

                    tx.HasColorInfo(0);
                    builder.ReadShapes(tx);
                }
            }
            else
            {
                //build as normal glyph
                builder.BuildFromGlyphIndex(glyphIndex, sizeInPoints);

                tx.HasColorInfo(0);
                builder.ReadShapes(tx);
            }

        }
    }
}
