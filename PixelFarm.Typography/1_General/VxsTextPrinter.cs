//MIT, 2016-present, WinterDev, Sam Hocevar
using System;
using System.Collections.Generic;
using PixelFarm.CpuBlit.BitmapAtlas;

using Typography.Contours;
using Typography.OpenFont;
using Typography.OpenFont.Extensions;
using Typography.TextLayout;
using Typography.TextBreak;
using Typography.FontManagement;

namespace PixelFarm.Drawing
{
    public class MyAlternativeTypefaceSelector : AlternativeTypefaceSelector
    {
        Dictionary<string, PreferTypefaceList> _dics = new Dictionary<string, PreferTypefaceList>();
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
        public PreferTypefaceList GetPreferTypefaces(string scriptTag) => _dics.TryGetValue(scriptTag, out PreferTypefaceList foundList) ? foundList : null;

        public override InstalledTypeface Select(List<InstalledTypeface> choices, ScriptLangInfo scriptLangInfo, char hintChar)
        {
            if (_dics.TryGetValue(scriptLangInfo.shortname, out PreferTypefaceList foundList))
            {
                //select only resolved font
                List<PreferTypeface> list = foundList._list;
                int j = list.Count;
                for (int i = 0; i < j; ++i)
                {
                    PreferTypeface p = list[i];
                    //-------
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
            return base.Select(choices, scriptLangInfo, hintChar);
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
        BitmapCacheForSvgGlyph _glyphSvgStore;

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
            _glyphSvgStore = new BitmapCacheForSvgGlyph();

        }
        public AlternativeTypefaceSelector AlternativeTypefaceSelector { get; set; }

        public void SetSvgBmpBuilderFunc(SvgBmpBuilderFunc svgBmpBuilderFunc)
        {
            _glyphSvgStore.SetSvgBmpBuilderFunc(svgBmpBuilderFunc);
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

        public override Typeface Typeface
        {
            get => _currentTypeface;

            set
            {

                if (_currentTypeface == value) return;
                // 
                _currentTypeface = value;
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
            Internal.RenderVxGlyphPlan[] glyphPlans = renderVx.GlyphList;
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

        public override void DrawFromGlyphPlans(GlyphPlanSequence seq, int startAt, int len, float left, float top)
        {
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


            Typography.OpenFont.Tables.COLR colrTable = _currentTypeface.COLRTable;
            Typography.OpenFont.Tables.CPAL cpalTable = _currentTypeface.CPALTable;
            bool hasColorGlyphs = (colrTable != null) && (cpalTable != null);

            //--------------------------------------------------- 
            _glyphMeshStore.SetHintTechnique(this.HintTechnique);
            _glyphMeshStore.SetFont(_currentTypeface, fontSizePoint);
            _glyphMeshStore.SimulateOblique = this.SimulateSlant;
            //---------------------------------------------------


            if (_currentTypeface.HasSvgTable())
            {
                _glyphSvgStore.SetCurrentTypeface(_currentTypeface);
                int seqLen = seq.Count;
                if (len > seqLen)
                {
                    len = seqLen;
                }

                var snapToPx = new GlyphPlanSequenceSnapPixelScaleLayout(seq, startAt, len, scale);
                while (snapToPx.Read())
                {
                    _painter.SetOrigin((float)Math.Round(left + snapToPx.ExactX) + 0.33f, (float)Math.Floor(baseLine + snapToPx.ExactY));

                    GlyphBitmap glyphBmp = _glyphSvgStore.GetGlyphBitmap(snapToPx.CurrentGlyphIndex);
                    //how to draw the image
                    //1. 
                    if (glyphBmp != null)
                    {
                        _painter.DrawImage(glyphBmp.Bitmap);
                    }
                }
            }
            else if (_currentTypeface.IsBitmapFont)
            {
                //check if we have exported all the glyph bitmap 
                //to some 'ready' form?
                //if not then create it
                _glyphBitmapStore.SetCurrentTypeface(_currentTypeface);

                int seqLen = seq.Count;

                if (len > seqLen)
                {
                    len = seqLen;
                }

                var snapToPx = new GlyphPlanSequenceSnapPixelScaleLayout(seq, startAt, len, scale);
                while (snapToPx.Read())
                {
                    _painter.SetOrigin((float)Math.Round(left + snapToPx.ExactX) + 0.33f, (float)Math.Floor(baseLine + snapToPx.ExactY));
                    GlyphBitmap glyphBmp = _glyphBitmapStore.GetGlyphBitmap(snapToPx.CurrentGlyphIndex);
                    //how to draw the image
                    if (glyphBmp != null)
                    {
                        _painter.DrawImage(glyphBmp.Bitmap);
                    }
                }
            }
            else
            {
                if (!hasColorGlyphs)
                {
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

                    //restore
                    _painter.RenderQuality = savedRederQuality;
                    _painter.UseLcdEffectSubPixelRendering = savedUseLcdMode;

                }
                else
                {
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
                    while (snapToPx.Read())
                    {

                        _painter.SetOrigin((float)Math.Round(left + snapToPx.ExactX), (float)Math.Floor(baseLine + snapToPx.ExactY));

                        if (colrTable.LayerIndices.TryGetValue(snapToPx.CurrentGlyphIndex, out ushort colorLayerStart))
                        {
                            //TODO: optimize this                        
                            //we found color info for this glyph 
                            ushort colorLayerCount = colrTable.LayerCounts[snapToPx.CurrentGlyphIndex];
                            byte r, g, b, a;
                            for (int c = colorLayerStart; c < colorLayerStart + colorLayerCount; ++c)
                            {
                                ushort gIndex = colrTable.GlyphLayers[c];

                                int palette = 0; // FIXME: assume palette 0 for now 
                                cpalTable.GetColor(
                                    cpalTable.Palettes[palette] + colrTable.GlyphPalettes[c], //index
                                    out r, out g, out b, out a);
                                //-----------  
                                _painter.FillColor = new Color(r, g, b);//? a component
                                _painter.Fill(_glyphMeshStore.GetGlyphMesh(gIndex));
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
                        }
                    }

                    _painter.FillColor = originalFillColor; //restore color
                }
            }
            //restore prev origin
            _painter.SetOrigin(ox, oy);
        }

        public void DrawString(char[] text, int startAt, int len, double x, double y)
        {
            DrawString(text, startAt, len, (float)x, (float)y);
        }

        public override void DrawString(char[] textBuffer, int startAt, int len, float x, float y)
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
                using (ILineSegmentList segments = _textServices.BreakToLineSegments(buffSpan))
                {
                    int count = segments.Count;

                    _tmpGlyphPlanSeqs.Clear();
                    _tmpTypefaces.Clear();

                    bool needRightToLeftArr = false;

                    Typeface defaultTypeface = _currentTypeface;
                    Typeface curTypeface = defaultTypeface;

                    for (int i = 0; i < count; ++i)
                    {
                        //
                        ILineSegment line_seg = segments[i];
                        SpanBreakInfo spBreakInfo = (SpanBreakInfo)line_seg.SpanBreakInfo;
                        TextBufferSpan buff = new TextBufferSpan(textBuffer, line_seg.StartAt, line_seg.Length);
                        if (spBreakInfo.RightToLeft)
                        {
                            needRightToLeftArr = true;
                        }

                        //each line segment may have different unicode range 
                        //and the current typeface may not support that range
                        //so we need to ensure that we get a proper typeface,
                        //if not => alternative typeface

                        ushort glyphIndex = 0;
                        char sample_char = textBuffer[line_seg.StartAt];
                        if (line_seg.Length > 1)
                        {
                            //high serogate pair or not
                            int codepoint = sample_char;
                            if (sample_char >= 0xd800 && sample_char <= 0xdbff)
                            {
                                char nextCh = textBuffer[line_seg.StartAt + 1];
                                if (nextCh >= 0xdc00 && nextCh <= 0xdfff)
                                {
                                    codepoint = char.ConvertToUtf32(sample_char, nextCh);
                                }
                            }

                            glyphIndex = curTypeface.GetGlyphIndex(codepoint);

                        }
                        else
                        {
                            glyphIndex = curTypeface.GetGlyphIndex(sample_char);
                        }


                        if (glyphIndex == 0)
                        {
                            //not found then => find other typeface                    
                            //we need more information about line seg layout

                            if (AlternativeTypefaceSelector != null)
                            {
                                AlternativeTypefaceSelector.LatestTypeface = curTypeface;
                            }

                            if (_textServices.TryGetAlternativeTypefaceFromChar(sample_char, AlternativeTypefaceSelector, out Typeface alternative))
                            {
                                curTypeface = alternative;
                                _tmpTypefaces.Add(alternative);
                            }
                            else
                            {
                                _tmpTypefaces.Add(curTypeface);
#if DEBUG
                                if (sample_char >= 0 && sample_char < 255)
                                {


                                }
#endif
                            }
                        }
                        else
                        {
                            _tmpTypefaces.Add(curTypeface);
                        }

                        _textServices.CurrentScriptLang = new ScriptLang(spBreakInfo.ScriptTag, spBreakInfo.LangTag);

                        GlyphPlanSequence glyphPlanSeq = _textServices.CreateGlyphPlanSeq(buff, curTypeface, FontSizeInPoints);
                        glyphPlanSeq.IsRightToLeft = spBreakInfo.RightToLeft;

                        _tmpGlyphPlanSeqs.Add(glyphPlanSeq);


                        curTypeface = defaultTypeface;//switch back to default
                    }

                    if (needRightToLeftArr)
                    {
                        //special arr left-to-right
                        for (int i = count - 1; i >= 0; --i)
                        {
                            GlyphPlanSequence glyphPlanSeq = _tmpGlyphPlanSeqs[i];

                            Typeface = _tmpTypefaces[i];

                            DrawFromGlyphPlans(glyphPlanSeq, xpos, y);
                            xpos += (glyphPlanSeq.CalculateWidth() * _currentFontSizePxScale);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < count; ++i)
                        {
                            GlyphPlanSequence glyphPlanSeq = _tmpGlyphPlanSeqs[i];

                            //change typeface                            
                            Typeface = _tmpTypefaces[i];
                            //update pxscale size                             
                            _currentFontSizePxScale = Typeface.CalculateScaleToPixelFromPointSize(FontSizeInPoints);

                            DrawFromGlyphPlans(glyphPlanSeq, xpos, y);
                            xpos += (glyphPlanSeq.CalculateWidth() * _currentFontSizePxScale);
                        }
                    }
                    Typeface = defaultTypeface;
                    _tmpGlyphPlanSeqs.Clear();
                    _tmpTypefaces.Clear();
                }
            }

        }

        List<GlyphPlanSequence> _tmpGlyphPlanSeqs = new List<GlyphPlanSequence>();
        List<Typeface> _tmpTypefaces = new List<Typeface>();
    }
}
